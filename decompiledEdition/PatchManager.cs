using System.Diagnostics;
using UnityEngine;

public static class PatchManager
{
	// ponytail: headless server renders nothing (Null device, gfxDriver=0) but was
	// holding 110MB of decoded textures. Stripping mips is invisible server-side
	// and client-safe (batchmode gate; this DLL also ships in the client).
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void ApplyDedicatedServerTuning()
	{
		if (ApplicationManager.IsDedicatedGameServer)
		{
			QualitySettings.globalTextureMipmapLimit = 3;
		}
	}
	private static readonly Logger Logger = new Logger("PatchManager");

	public static void Initialize()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		VisualElementHarmonyPatch.Patch();
		PerformanceProfiler.MaybeStart();
		MemoryReporter.MaybeStart();
		stopwatch.Stop();
		Logger.Info($"Patching took {stopwatch.ElapsedMilliseconds}ms");
	}

	public static void Dispose()
	{
		PerformanceProfiler.Stop();
		Stopwatch stopwatch = Stopwatch.StartNew();
		VisualElementHarmonyPatch.Unpatch();
		stopwatch.Stop();
		Logger.Info($"Unpatching took {stopwatch.ElapsedMilliseconds}ms");
	}
}
