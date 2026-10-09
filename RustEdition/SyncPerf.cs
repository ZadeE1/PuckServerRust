using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;

// ponytail: hand-rolled timing for the 3 sync types Harmony must never touch
// (Snapshot, SynchronizedObjectData, TickHeader — patching them corrupts
// change-mask computation; see AGENTS.md). Static Enter/Exit pairs with fixed
// slots: no allocations, no Harmony, no behavior change. Same JSONL schema as
// PerformanceProfiler so the same analysis scripts apply.
// Enable with env PUCK_SYNC_PROFILE=1 (optional PUCK_SYNC_PROFILE_PATH,
// default ./profiler-sync.jsonl). Off by default = two static-bool reads.
public static class SyncPerf
{
	private static readonly Logger Logger = new Logger("SyncPerf");

	public const int SnapshotCapture = 0;
	public const int DataCtor = 1;
	public const int DataGetChangeMask = 2;
	public const int DataWithComponentMask = 3;
	public const int DataMerge = 4;
	public const int DataWithAsleep = 5;
	public const int DataWithTickRateDivisor = 6;
	public const int TickHeaderCtor = 7;
	public const int TickHeaderGetChangeMask = 8;
	public const int TickHeaderWithComponentMask = 9;
	private const int SlotCount = 10;

	private static readonly string[] names = new string[SlotCount]
	{
		"SynchronizedObjectSnapshot.Capture",
		"SynchronizedObjectData.ctor",
		"SynchronizedObjectData.GetChangeMask",
		"SynchronizedObjectData.WithComponentMask",
		"SynchronizedObjectData.Merge",
		"SynchronizedObjectData.WithAsleep",
		"SynchronizedObjectData.WithTickRateDivisor",
		"SynchronizedObjectTickHeader.ctor",
		"SynchronizedObjectTickHeader.GetChangeMask",
		"SynchronizedObjectTickHeader.WithComponentMask"
	};

	private static readonly long[] calls = new long[SlotCount];
	private static readonly long[] ticks = new long[SlotCount];
	private static string outputPath = "profiler-sync.jsonl";
	private static Thread writer;
	private static volatile bool running;

	public static bool Enabled;

	public static void MaybeStart()
	{
		if (Environment.GetEnvironmentVariable("PUCK_SYNC_PROFILE") != "1")
		{
			return;
		}
		string path = Environment.GetEnvironmentVariable("PUCK_SYNC_PROFILE_PATH");
		if (!string.IsNullOrEmpty(path))
		{
			outputPath = path;
		}
		if (running)
		{
			return;
		}
		Enabled = true;
		running = true;
		writer = new Thread(WriterLoop);
		writer.IsBackground = true;
		writer.Name = "SyncPerf";
		writer.Start();
		Logger.Info("SyncPerf recording " + SlotCount + " methods -> " + outputPath);
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
		Enabled = false;
		Logger.Info("SyncPerf stopped");
	}

	public static long Enter()
	{
		return Enabled ? Stopwatch.GetTimestamp() : 0L;
	}

	public static void Exit(int slot, long startTicks)
	{
		if (!Enabled)
		{
			return;
		}
		Interlocked.Increment(ref calls[slot]);
		Interlocked.Add(ref ticks[slot], Stopwatch.GetTimestamp() - startTicks);
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
		for (int i = 0; i < SlotCount; i++)
		{
			long callCount = Interlocked.Exchange(ref calls[i], 0);
			if (callCount == 0)
			{
				continue;
			}
			long tickCount = Interlocked.Exchange(ref ticks[i], 0);
			double ms = tickCount / perMs;
			functions.Add(new Dictionary<string, object>
			{
				{ "n", names[i] },
				{ "calls", callCount },
				{ "ms", ms },
				{ "avgUs", ms * 1000.0 / callCount },
				{ "allocBytes", 0L }
			});
		}
		functions.Sort(CompareByMsDesc);
		try
		{
			File.AppendAllText(outputPath, JsonSerializer.Serialize(new Dictionary<string, object>
			{
				{ "t", DateTime.UtcNow.ToString("o") },
				{ "gcHeapBytes", GC.GetTotalMemory(false) },
				{ "functions", functions }
			}) + "\n");
		}
		catch
		{
		}
	}

	private static int CompareByMsDesc(Dictionary<string, object> a, Dictionary<string, object> b)
	{
		return ((double)b["ms"]).CompareTo((double)a["ms"]);
	}
}
