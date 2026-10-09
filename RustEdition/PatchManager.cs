using System.Diagnostics;
using UnityEngine;

public static class PatchManager
{
	// ponytail: headless server renders nothing (gfxDriver=0) but holds ~50MB of UI
	// icons. Shrink them after every scene load; client-safe (batchmode gate; UI
	// updates already early-return on dedicated servers, so nothing reads pixels).
	// Font atlases are skipped. Old mipmap-limit attempt removed (no-op on
	// non-mipmapped icons, verified by measurement).
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void ApplyDedicatedServerTuning()
	{
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			return;
		}
		if (!sceneHookInstalled)
		{
			sceneHookInstalled = true;
			UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnServerSceneLoaded;
		}
		ShrinkServerTextures();
	}

	private static void OnServerSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
	{
		ShrinkServerTextures();
	}

	private static void ShrinkServerTextures()
	{
		int shrunk = 0;
		int skipped = 0;
		int failed = 0;
		foreach (Object o in Resources.FindObjectsOfTypeAll(typeof(Texture2D)))
		{
			Texture2D t = o as Texture2D;
			if (t == null || (t.width <= 8 && t.height <= 8))
			{
				continue;
			}
			try
			{
				string lower = (t.name ?? "").ToLowerInvariant();
				if (lower.Contains("font") || lower.Contains("sdf"))
				{
					skipped++;
					continue;
				}
				t.Reinitialize(Mathf.Max(4, t.width / 8), Mathf.Max(4, t.height / 8));
				shrunk++;
			}
			catch
			{
				failed++;
			}
		}
		Logger.Info("Shrunk " + shrunk + " server textures (skipped fonts: " + skipped + ", failed: " + failed + ")");
	}
	private static readonly Logger Logger = new Logger("PatchManager");

	private static bool sceneHookInstalled;

	public static void Initialize()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		VisualElementHarmonyPatch.Patch();
		PerformanceProfiler.MaybeStart();
		SyncPerf.MaybeStart();
		MemoryReporter.MaybeStart();
		ApplyDedicatedServerTuning();
		stopwatch.Stop();
		Logger.Info($"Patching took {stopwatch.ElapsedMilliseconds}ms");
	}

	public static void Dispose()
	{
		PerformanceProfiler.Stop();
		SyncPerf.Stop();
		Stopwatch stopwatch = Stopwatch.StartNew();
		VisualElementHarmonyPatch.Unpatch();
		stopwatch.Stop();
		Logger.Info($"Unpatching took {stopwatch.ElapsedMilliseconds}ms");
	}
}
