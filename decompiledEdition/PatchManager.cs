using System.Diagnostics;

public static class PatchManager
{
	private static readonly Logger Logger = new Logger("PatchManager");

	public static void Initialize()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		VisualElementHarmonyPatch.Patch();
		stopwatch.Stop();
		Logger.Info($"Patching took {stopwatch.ElapsedMilliseconds}ms");
	}

	public static void Dispose()
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		VisualElementHarmonyPatch.Unpatch();
		stopwatch.Stop();
		Logger.Info($"Unpatching took {stopwatch.ElapsedMilliseconds}ms");
	}
}
