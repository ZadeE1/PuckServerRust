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
// Enable with env PUCK_PROFILE=1 (optional PUCK_PROFILE_PATH, default ./profiler.jsonl). Off by default = zero overhead.
public static class PerformanceProfiler
{
	private static readonly Logger Logger = new Logger("PerformanceProfiler");
	private static readonly Harmony harmony = new Harmony("Puck.PerformanceProfiler");
	private static readonly ConcurrentDictionary<MethodBase, Stats> stats = new ConcurrentDictionary<MethodBase, Stats>();
	private static readonly ThreadLocal<Stack<Frame>> stacks = new ThreadLocal<Stack<Frame>>();
	private static Thread writer;
	private static volatile bool running;
	private static string outputPath = "profiler.jsonl";
	private static bool allocSupported;

	private struct Frame
	{
		public MethodBase method;
		public long startTicks;
		public long startAlloc;
	}

	private sealed class Stats
	{
		public long calls;
		public long ticks;
		public long allocBytes;
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
		try
		{
			GC.GetAllocatedBytesForCurrentThread();
			allocSupported = true;
		}
		catch
		{
			allocSupported = false;
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
		Logger.Info("Profiling " + patched + " methods -> " + outputPath + " (alloc tracking: " + (allocSupported ? "on" : "off") + ")");
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
			if (t == typeof(PerformanceProfiler))
			{
				return true;
			}
		}
		return false;
	}

	private static int PatchType(Type type, HarmonyMethod prefix, HarmonyMethod postfix)
	{
		if (IsProfilerCode(type))
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
		Stack<Frame> stack = StackForThread();
		if (stack.Count > 512)
		{
			stack.Clear();
		}
		Frame frame;
		frame.method = __originalMethod;
		frame.startTicks = Stopwatch.GetTimestamp();
		frame.startAlloc = allocSupported ? GC.GetAllocatedBytesForCurrentThread() : 0L;
		stack.Push(frame);
	}

	private static void Postfix(MethodBase __originalMethod)
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
		if (allocSupported)
		{
			Interlocked.Add(ref s.allocBytes, GC.GetAllocatedBytesForCurrentThread() - frame.startAlloc);
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
			long alloc = Interlocked.Exchange(ref kv.Value.allocBytes, 0);
			double ms = ticks / perMs;
			MethodBase m = kv.Key;
			functions.Add(new Dictionary<string, object>
			{
				{ "n", (m.DeclaringType != null ? m.DeclaringType.FullName + "." : "") + m.Name },
				{ "calls", calls },
				{ "ms", ms },
				{ "avgUs", ms * 1000.0 / calls },
				{ "allocBytes", alloc }
			});
		}
		functions.Sort(CompareByMsDesc);
		File.AppendAllText(outputPath, JsonSerializer.Serialize(new Dictionary<string, object>
		{
			{ "t", DateTime.UtcNow.ToString("o") },
			{ "gcHeapBytes", GC.GetTotalMemory(false) },
			{ "functions", functions }
		}) + "\n");
	}

	private static int CompareByMsDesc(Dictionary<string, object> a, Dictionary<string, object> b)
	{
		return ((double)b["ms"]).CompareTo((double)a["ms"]);
	}
}
