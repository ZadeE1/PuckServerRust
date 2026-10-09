using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using HarmonyLib;

// ponytail: exact per-call timing via one shared Harmony prefix/postfix; MethodBase dict keys avoid per-call string allocs.
// The profiler never patches itself (declaring-chain guard) and uses no lambdas on the hot path, so it cannot recurse.
// Per-call alloc tracking is deliberately absent: Unity's Mono stubs GC.GetAllocatedBytesForCurrentThread (always 0)
// and calling it per-invocation stalls the main thread under load. Heap is still sampled per second via gcHeapBytes.
// Any hot-path failure disables recording (dead flag) so the profiler can never break gameplay.
// Enable with env PUCK_PROFILE=1 (optional PUCK_PROFILE_PATH, default ./profiler.jsonl). Off by default = zero overhead.
public static class PerformanceProfiler
{
	private static readonly Logger Logger = new Logger("PerformanceProfiler");
	private static readonly Harmony harmony = new Harmony("Puck.PerformanceProfiler");
	private static readonly ConcurrentDictionary<MethodBase, Stats> stats = new ConcurrentDictionary<MethodBase, Stats>();
	private static readonly ThreadLocal<Stack<Frame>> stacks = new ThreadLocal<Stack<Frame>>();
	private static Thread writer;
	private static volatile bool running;
	private static volatile bool dead;
	private static int failures;
	private static int heartbeatCounter;
	private static int skippedTypes;
	private static string outputPath = "profiler.jsonl";
	private static string[] skipPrefixes = new string[0];

	private struct Frame
	{
		public MethodBase method;
		public long startTicks;
	}

	private sealed class Stats
	{
		public long calls;
		public long ticks;
	}

	public static void MaybeStart()
	{
		if (Environment.GetEnvironmentVariable("PUCK_PROFILE") != "1")
		{
			return;
		}
		Start(Environment.GetEnvironmentVariable("PUCK_PROFILE_PATH"));
	}

	public static void Start(string path)
	{
		if (running)
		{
			return;
		}
		if (!string.IsNullOrEmpty(path))
		{
			outputPath = path;
		}
		string skip = Environment.GetEnvironmentVariable("PUCK_PROFILE_SKIP");
		if (!string.IsNullOrEmpty(skip))
		{
			skipPrefixes = skip.Split(';');
		}
		MethodInfo prefix = typeof(PerformanceProfiler).GetMethod("Prefix", BindingFlags.Static | BindingFlags.NonPublic);
		MethodInfo postfix = typeof(PerformanceProfiler).GetMethod("Postfix", BindingFlags.Static | BindingFlags.NonPublic);
		HarmonyMethod prefixMethod = new HarmonyMethod(prefix);
		HarmonyMethod postfixMethod = new HarmonyMethod(postfix);
		int patched = 0;
		foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
		{
			patched += PatchType(type, prefixMethod, postfixMethod);
		}
		running = true;
		writer = new Thread(WriterLoop);
		writer.IsBackground = true;
		writer.Name = "PerformanceProfiler";
		writer.Start();
		Logger.Info("Profiling " + patched + " methods (" + skippedTypes + " types skipped) -> " + outputPath);
	}

	public static void Stop()
	{
		if (!running)
		{
			return;
		}
		running = false;
		if (writer != null && Thread.CurrentThread != writer)
		{
			writer.Join();
		}
		WriteSnapshot(Stopwatch.Frequency / 1000.0);
		harmony.UnpatchSelf();
		Logger.Info("Profiler stopped");
	}

	private static bool IsProfilerCode(Type type)
	{
		for (Type t = type; t != null; t = t.DeclaringType)
		{
			if (t == typeof(PerformanceProfiler) || t == typeof(SyncPerf))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsSkipped(Type type)
	{
		if (skipPrefixes.Length == 0)
		{
			return false;
		}
		string name = type.FullName;
		foreach (string prefix in skipPrefixes)
		{
			if (prefix == "*" || (name != null && name.StartsWith(prefix)))
			{
				skippedTypes++;
				return true;
			}
		}
		return false;
	}

	private static int PatchType(Type type, HarmonyMethod prefix, HarmonyMethod postfix)
	{
		if (IsProfilerCode(type) || IsSkipped(type))
		{
			return 0;
		}
		int count = 0;
		foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
		{
			if (method.IsAbstract || method.IsGenericMethodDefinition || (method.IsConstructor && method.IsStatic) || method.GetMethodBody() == null)
			{
				continue;
			}
			try
			{
				harmony.Patch(method, prefix, postfix);
				count++;
			}
			catch
			{
			}
		}
		foreach (Type nested in type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic))
		{
			count += PatchType(nested, prefix, postfix);
		}
		return count;
	}

	private static void DisableOnFailure()
	{
		if (Interlocked.Increment(ref failures) > 1000 && !dead)
		{
			dead = true;
			Logger.Error("PROFILER AUTO-DISABLED after " + failures + " hot-path failures; recording stopped, game unaffected");
		}
	}

	private static Stack<Frame> StackForThread()
	{
		Stack<Frame> stack = stacks.Value;
		if (stack == null)
		{
			stack = new Stack<Frame>();
			stacks.Value = stack;
		}
		return stack;
	}

	private static void Prefix(MethodBase __originalMethod)
	{
		if (dead)
		{
			return;
		}
		try
		{
			Stack<Frame> stack = StackForThread();
			if (stack.Count > 512)
			{
				stack.Clear();
			}
			Frame frame;
			frame.method = __originalMethod;
			frame.startTicks = Stopwatch.GetTimestamp();
			stack.Push(frame);
		}
		catch
		{
			DisableOnFailure();
		}
	}

	private static void Postfix(MethodBase __originalMethod)
	{
		if (dead)
		{
			return;
		}
		try
		{
			Stack<Frame> stack = StackForThread();
			if (stack.Count == 0)
			{
				return;
			}
			Frame frame = stack.Pop();
			long elapsed = Stopwatch.GetTimestamp() - frame.startTicks;
			Stats s;
			if (!stats.TryGetValue(frame.method, out s))
			{
				Stats fresh = new Stats();
				if (stats.TryAdd(frame.method, fresh))
				{
					s = fresh;
				}
				else
				{
					stats.TryGetValue(frame.method, out s);
				}
			}
			Interlocked.Increment(ref s.calls);
			Interlocked.Add(ref s.ticks, elapsed);
		}
		catch
		{
			DisableOnFailure();
		}
	}

	private static void WriterLoop()
	{
		double perMs = Stopwatch.Frequency / 1000.0;
		while (running)
		{
			Thread.Sleep(1000);
			try
			{
				WriteSnapshot(perMs);
				if (Interlocked.Increment(ref heartbeatCounter) % 30 == 0)
				{
					Logger.Info("profiler heartbeat: " + stats.Count + " methods tracked, failures=" + failures + ", dead=" + dead);
				}
			}
			catch
			{
			}
		}
	}

	private static void WriteSnapshot(double perMs)
	{
		List<Dictionary<string, object>> functions = new List<Dictionary<string, object>>();
		foreach (KeyValuePair<MethodBase, Stats> kv in stats)
		{
			long calls = Interlocked.Exchange(ref kv.Value.calls, 0);
			if (calls == 0)
			{
				continue;
			}
			long ticks = Interlocked.Exchange(ref kv.Value.ticks, 0);
			double ms = ticks / perMs;
			MethodBase m = kv.Key;
			functions.Add(new Dictionary<string, object>
			{
				{ "n", (m.DeclaringType != null ? m.DeclaringType.FullName + "." : "") + m.Name },
				{ "calls", calls },
				{ "ms", ms },
				{ "avgUs", ms * 1000.0 / calls },
				{ "allocBytes", 0L }
			});
		}
		functions.Sort(CompareByMsDesc);
		File.AppendAllText(outputPath, JsonSerializer.Serialize(new Dictionary<string, object>
		{
			{ "t", DateTime.UtcNow.ToString("o") },
			{ "gcHeapBytes", GC.GetTotalMemory(false) },
			{ "failures", failures },
			{ "dead", dead },
			{ "functions", functions }
		}) + "\n");
	}

	private static int CompareByMsDesc(Dictionary<string, object> a, Dictionary<string, object> b)
	{
		return ((double)b["ms"]).CompareTo((double)a["ms"]);
	}
}
