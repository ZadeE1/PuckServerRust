using UnityEngine;
using UnityEngine.UIElements;

// ponytail: headless has no screen, mic, or viewers. Disabling display-only
// MonoBehaviours makes Unity skip their Update dispatch entirely, which is
// cheaper than per-frame early-out guards at uncapped batchmode frame rates.
// Clearing the UIToolkit tree frees the view hierarchy; the detached elements
// make any stray view calls harmless no-ops. Methods stay callable; only
// Unity messages stop.
public static class HeadlessOptimizations
{
	public static void Apply()
	{
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			return;
		}
		foreach (UIView c in Object.FindObjectsOfType<UIView>())
		{
			c.enabled = false;
		}
		foreach (UIManager c in Object.FindObjectsOfType<UIManager>())
		{
			c.enabled = false;
		}
		foreach (MicTester c in Object.FindObjectsOfType<MicTester>())
		{
			c.enabled = false;
		}
		foreach (UIDocument doc in Object.FindObjectsOfType<UIDocument>())
		{
			if (doc != null && doc.rootVisualElement != null)
			{
				doc.rootVisualElement.Clear();
			}
		}
	}
}
