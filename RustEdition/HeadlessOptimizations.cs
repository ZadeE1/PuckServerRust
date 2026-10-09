using UnityEngine;

// ponytail: headless has no screen, mic, or viewers. Disabling display-only
// MonoBehaviours makes Unity skip their Update dispatch entirely, which is
// cheaper than per-frame early-out guards at uncapped batchmode frame rates.
// Methods on these components stay callable; only Unity messages stop.
public static class HeadlessOptimizations
{
	public static void Apply()
	{
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			return;
		}
		foreach (UIMinimap c in Object.FindObjectsOfType<UIMinimap>())
		{
			c.enabled = false;
		}
		foreach (UIManager c in Object.FindObjectsOfType<UIManager>())
		{
			c.enabled = false;
		}
		foreach (UIPositionSelect c in Object.FindObjectsOfType<UIPositionSelect>())
		{
			c.enabled = false;
		}
		foreach (UIUsernames c in Object.FindObjectsOfType<UIUsernames>())
		{
			c.enabled = false;
		}
		foreach (MicTester c in Object.FindObjectsOfType<MicTester>())
		{
			c.enabled = false;
		}
	}
}
