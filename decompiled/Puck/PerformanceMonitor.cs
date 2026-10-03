using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using HarmonyLib;
using UnityEngine.Profiling;

public static class PerformanceMonitor
{
	private sealed class MethodStats
	{
		public readonly string Name;

		public long Count;

		public long TotalTicks;

		public long MinTicks;

		public long MaxTicks;

		public long FlushCount;

		public long FlushTotalTicks;

		public MethodStats(string name)
		{
			Name = name;
		}
	}

	private sealed class ReferenceComparer : IEqualityComparer<MethodBase>
	{
		public bool Equals(MethodBase x, MethodBase y)
		{
			return ReferenceEquals(x, y);
		}

		public int GetHashCode(MethodBase obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}
	}

	private const string LogDir = "performance_log";

	private const string MsLogPath = "performance_log/ms_log.txt";

	private const string RamLogPath = "performance_log/ram_log.txt";

	private const string DisableMarkerPath = "performance_monitor_disabled.txt";

	private const double FlushIntervalSeconds = 10.0;

	private static readonly Logger Logger = new Logger("PerformanceMonitor");

	private static readonly Harmony Harmony = new Harmony("Puck.PerformanceMonitor");

	private static readonly ConcurrentDictionary<MethodBase, MethodStats> Stats = new ConcurrentDictionary<MethodBase, MethodStats>(new ReferenceComparer());

	private static readonly Stopwatch FlushClock = Stopwatch.StartNew();

	private static readonly double TicksToMs = 1000.0 / Stopwatch.Frequency;

	private static volatile bool enabled;

	public static void Initialize()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		int num = 0;
		int num2 = 0;
		try
		{
			if (File.Exists(DisableMarkerPath))
			{
				Logger.Info("PerformanceMonitor disabled via " + DisableMarkerPath);
				return;
			}
			Directory.CreateDirectory(LogDir);
			File.AppendAllText(MsLogPath, $"[{Now()}] === performance log started ===\n");
			foreach (MethodBase method in CollectMethods())
			{
				try
				{
					Harmony.Patch(method, new HarmonyMethod(typeof(PerformanceMonitor), "Prefix"), new HarmonyMethod(typeof(PerformanceMonitor), "Postfix"));
					Stats[method] = new MethodStats(FormatName(method));
					num++;
				}
				catch (Exception)
				{
					num2++;
				}
				if ((num + num2) % 500 == 0)
				{
					File.AppendAllText(MsLogPath, $"[{Now()}] patching progress: {num} instrumented, {num2} failed ({stopwatch.ElapsedMilliseconds}ms)\n");
				}
			}
		}
		catch (Exception ex)
		{
			try
			{
				File.AppendAllText(MsLogPath, $"[{Now()}] patching aborted: {ex}\n");
			}
			catch (Exception)
			{
			}
		}
		FlushClock.Restart();
		enabled = true;
		stopwatch.Stop();
		try
		{
			File.AppendAllText(MsLogPath, $"[{Now()}] === instrumentation done | {num} functions in {stopwatch.ElapsedMilliseconds}ms | {num2} failed ===\n");
		}
		catch (Exception)
		{
		}
		try
		{
			Logger.Info($"PerformanceMonitor instrumented {num} functions in {stopwatch.ElapsedMilliseconds}ms ({num2} failed)");
		}
		catch (Exception)
		{
		}
	}

	public static void Dispose()
	{
		if (!enabled)
		{
			return;
		}
		enabled = false;
		Flush();
	}

	public static void Tick()
	{
		if (enabled && FlushClock.Elapsed.TotalSeconds >= FlushIntervalSeconds)
		{
			Flush();
		}
	}

	public static void Prefix(out long __state)
	{
		__state = Stopwatch.GetTimestamp();
	}

	public static void Postfix(long __state, MethodBase __originalMethod)
	{
		if (!enabled || __state == 0)
		{
			return;
		}
		try
		{
			long num = Stopwatch.GetTimestamp() - __state;
			if (num <= 0)
			{
				return;
			}
			if (Stats.TryGetValue(__originalMethod, out MethodStats value))
			{
				Record(value, num);
			}
		}
		catch (Exception)
		{
		}
	}

	private static void Record(MethodStats stats, long elapsedTicks)
	{
		Interlocked.Increment(ref stats.Count);
		Interlocked.Add(ref stats.TotalTicks, elapsedTicks);
		long num = Interlocked.Read(ref stats.MaxTicks);
		while (elapsedTicks > num && Interlocked.CompareExchange(ref stats.MaxTicks, elapsedTicks, num) != num)
		{
			num = Interlocked.Read(ref stats.MaxTicks);
		}
		long num2 = Interlocked.Read(ref stats.MinTicks);
		while ((num2 == 0 || elapsedTicks < num2) && Interlocked.CompareExchange(ref stats.MinTicks, elapsedTicks, num2) != num2)
		{
			num2 = Interlocked.Read(ref stats.MinTicks);
		}
	}

	private static List<MethodBase> CollectMethods()
	{
		List<MethodBase> list = new List<MethodBase>();
		Type[] types;
		try
		{
			types = typeof(PerformanceMonitor).Assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			types = ex.Types;
		}
		foreach (Type type in types)
		{
			if (type == null || type.IsValueType || IsOwnType(type) || typeof(Delegate).IsAssignableFrom(type))
			{
				continue;
			}
			const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
			MethodInfo[] array;
			try
			{
				array = type.GetMethods(bindingFlags | BindingFlags.DeclaredOnly);
			}
			catch (Exception)
			{
				continue;
			}
			foreach (MethodInfo methodInfo in array)
			{
				if (methodInfo.IsAbstract || methodInfo.ContainsGenericParameters)
				{
					continue;
				}
				MethodImplAttributes methodImplementationFlags = methodInfo.MethodImplementationFlags;
				if ((methodImplementationFlags & MethodImplAttributes.CodeTypeMask) != MethodImplAttributes.IL || (methodInfo.Attributes & (MethodAttributes)8192) != 0)
				{
					continue;
				}
				list.Add(methodInfo);
			}
			ConstructorInfo[] array2;
			try
			{
				array2 = type.GetConstructors(bindingFlags);
			}
			catch (Exception)
			{
				continue;
			}
			foreach (ConstructorInfo constructorInfo in array2)
			{
				if (!constructorInfo.IsStatic && (constructorInfo.MethodImplementationFlags & MethodImplAttributes.CodeTypeMask) == MethodImplAttributes.IL)
				{
					list.Add(constructorInfo);
				}
			}
		}
		return list;
	}

	private static bool IsOwnType(Type type)
	{
		for (Type type2 = type; type2 != null; type2 = type2.DeclaringType)
		{
			if (type2 == typeof(PerformanceMonitor))
			{
				return true;
			}
		}
		return false;
	}

	private static string FormatName(MethodBase method)
	{
		string text = ((method.DeclaringType != null) ? method.DeclaringType.FullName : "?");
		if (method.IsConstructor)
		{
			return text + "..ctor";
		}
		return text + "." + method.Name;
	}

	private static void Flush()
	{
		double totalSeconds = FlushClock.Elapsed.TotalSeconds;
		FlushClock.Restart();
		try
		{
			Directory.CreateDirectory(LogDir);
			List<(string Name, long Calls, double TotalMs, double AvgMs, double MinMs, double MaxMs)> list = new List<(string, long, double, double, double, double)>();
			foreach (KeyValuePair<MethodBase, MethodStats> item in Stats)
			{
				MethodStats value = item.Value;
				long num = Interlocked.Read(ref value.Count);
				long num2 = Interlocked.Read(ref value.TotalTicks);
				long num3 = num - value.FlushCount;
				long num4 = num2 - value.FlushTotalTicks;
				value.FlushCount = num;
				value.FlushTotalTicks = num2;
				if (num3 <= 0)
				{
					continue;
				}
				double ticksToMs = TicksToMs;
				list.Add((value.Name, num3, num4 * ticksToMs, num4 * ticksToMs / num3, Interlocked.Read(ref value.MinTicks) * ticksToMs, Interlocked.Read(ref value.MaxTicks) * ticksToMs));
			}
			list.Sort((a, b) => b.TotalMs.CompareTo(a.TotalMs));
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append('[').Append(Now()).Append("] === ms_log | interval ").Append(totalSeconds.ToString("F1", CultureInfo.InvariantCulture)).Append("s | active functions: ").Append(list.Count).Append(" ===\n");
			stringBuilder.Append("function\tcalls\ttotal_ms\tavg_ms\tmin_ms\tmax_ms\n");
			foreach (var item2 in list)
			{
				stringBuilder.Append(item2.Name).Append('\t').Append(item2.Calls)
					.Append('\t')
					.Append(item2.TotalMs.ToString("F4", CultureInfo.InvariantCulture))
					.Append('\t')
					.Append(item2.AvgMs.ToString("F5", CultureInfo.InvariantCulture))
					.Append('\t')
					.Append(item2.MinMs.ToString("F5", CultureInfo.InvariantCulture))
					.Append('\t')
					.Append(item2.MaxMs.ToString("F5", CultureInfo.InvariantCulture))
					.Append('\n');
			}
			stringBuilder.Append('\n');
			File.AppendAllText(MsLogPath, stringBuilder.ToString());
			AppendRamLog(totalSeconds);
		}
		catch (Exception ex)
		{
			Logger.Error($"PerformanceMonitor flush failed: {ex}");
		}
	}

	private static void AppendRamLog(double intervalSeconds)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('[').Append(Now()).Append("] working_set=").Append(Mb(GetProcessMemoryBytes(workingSet: true)))
			.Append("MB private=")
			.Append(Mb(GetProcessMemoryBytes(workingSet: false)))
			.Append("MB unity_allocated=")
			.Append(Mb(Profiler.GetTotalAllocatedMemoryLong()))
			.Append("MB unity_reserved=")
			.Append(Mb(Profiler.GetTotalReservedMemoryLong()))
			.Append("MB mono_used=")
			.Append(Mb(Profiler.GetMonoUsedSizeLong()))
			.Append("MB mono_heap=")
			.Append(Mb(Profiler.GetMonoHeapSizeLong()))
			.Append("MB gc_heap=")
			.Append(Mb(GC.GetTotalMemory(false)))
			.Append("MB gen0=")
			.Append(GC.CollectionCount(0))
			.Append(" gen1=")
			.Append(GC.CollectionCount(1))
			.Append(" gen2=")
			.Append(GC.CollectionCount(2))
			.Append(" interval=")
			.Append(intervalSeconds.ToString("F1", CultureInfo.InvariantCulture))
			.Append("s\n");
		File.AppendAllText(RamLogPath, stringBuilder.ToString());
	}

	private static long GetProcessMemoryBytes(bool workingSet)
	{
		try
		{
			if (GetProcessMemoryInfo(GetCurrentProcess(), out ProcessMemoryCounters processMemoryCounters, Marshal.SizeOf(typeof(ProcessMemoryCounters))))
			{
				return workingSet ? processMemoryCounters.WorkingSetSize.ToInt64() : processMemoryCounters.PagefileUsage.ToInt64();
			}
		}
		catch (Exception)
		{
		}
		return workingSet ? Environment.WorkingSet : 0L;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct ProcessMemoryCounters
	{
		public int cb;

		public int PageFaultCount;

		public IntPtr PeakWorkingSetSize;

		public IntPtr WorkingSetSize;

		public IntPtr QuotaPeakPagedPoolUsage;

		public IntPtr QuotaPagedPoolUsage;

		public IntPtr QuotaPeakNonPagedPoolUsage;

		public IntPtr QuotaNonPagedPoolUsage;

		public IntPtr PagefileUsage;

		public IntPtr PeakPagefileUsage;
	}

	[DllImport("psapi.dll", SetLastError = true)]
	private static extern bool GetProcessMemoryInfo(IntPtr process, out ProcessMemoryCounters counters, int size);

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetCurrentProcess();

	private static string Mb(long bytes)
	{
		return (bytes / 1048576.0).ToString("F1", CultureInfo.InvariantCulture);
	}

	private static string Now()
	{
		return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
	}
}
