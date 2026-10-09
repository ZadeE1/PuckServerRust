using System.Threading;
using UnityEngine;
using UnityEngine.Profiling;

// ponytail: 30s memory breakdown log so native/mono/gfx/texture spend can be
// targeted instead of guessed. Opt-in via PUCK_MEMLOG=1. Off = zero overhead.
public static class MemoryReporter
{
	private static readonly Logger Logger = new Logger("MemoryReporter");

	private static Thread reporter;

	private static volatile bool running;

	private static bool audited;

	public static void MaybeStart()
	{
		if (System.Environment.GetEnvironmentVariable("PUCK_MEMLOG") != "1")
		{
			return;
		}
		Start();
	}

	public static void Start()
	{
		if (running)
		{
			return;
		}
		running = true;
		reporter = new Thread(Loop);
		reporter.IsBackground = true;
		reporter.Name = "MemoryReporter";
		reporter.Start();
	}

	public static void Stop()
	{
		running = false;
	}
	private static void Loop()
	{
		bool warmedUp = false;
		while (running)
		{
			try
			{
				Logger.Info(string.Format("nativeAlloc={0:F1}MB nativeReserved={1:F1}MB monoHeap={2:F1}MB monoUsed={3:F1}MB gfxDriver={4:F1}MB textures={5:F1}MB",
					Profiler.GetTotalAllocatedMemoryLong() / 1048576.0,
					Profiler.GetTotalReservedMemoryLong() / 1048576.0,
					Profiler.GetMonoHeapSizeLong() / 1048576.0,
					Profiler.GetMonoUsedSizeLong() / 1048576.0,
					Profiler.GetAllocatedMemoryForGraphicsDriver() / 1024.0,
					Texture.currentTextureMemory / 1048576.0));
				if (!audited && warmedUp)
				{
					audited = true;
					AuditTopAssets();
				}
				warmedUp = true;
			}
			catch
			{
			}
			Thread.Sleep(15000);
		}
	}

	private static void AuditTopAssets()
	{
		try
		{
			LogTop("Texture2D", Resources.FindObjectsOfTypeAll(typeof(Texture2D)));
			LogTop("RenderTexture", Resources.FindObjectsOfTypeAll(typeof(RenderTexture)));
			LogTop("AudioClip", Resources.FindObjectsOfTypeAll(typeof(AudioClip)));
			LogTop("Mesh", Resources.FindObjectsOfTypeAll(typeof(Mesh)));
		}
		catch
		{
		}
	}

	private static void LogTop(string kind, Object[] objects)
	{
		long total = 0;
		string e1 = "";
		string e2 = "";
		string e3 = "";
		long s1 = -1;
		long s2 = -1;
		long s3 = -1;
		foreach (Object o in objects)
		{
			if (o == null)
			{
				continue;
			}
			long size = Profiler.GetRuntimeMemorySizeLong(o);
			total += size;
			string entry = (size / 1048576.0).ToString("F1") + "MB " + o.GetType().Name + " '" + o.name + "'";
			if (size > s1)
			{
				e3 = e2;
				s3 = s2;
				e2 = e1;
				s2 = s1;
				e1 = entry;
				s1 = size;
			}
			else if (size > s2)
			{
				e3 = e2;
				s3 = s2;
				e2 = entry;
				s2 = size;
			}
			else if (size > s3)
			{
				e3 = entry;
				s3 = size;
			}
		}
		Logger.Info(kind + "s: count=" + objects.Length + " total=" + (total / 1048576.0).ToString("F1") + "MB top=[" + e1 + " | " + e2 + " | " + e3 + "]");
	}
}
