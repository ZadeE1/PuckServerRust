using System;
using HarmonyLib;
using UnityEngine.UIElements;

public static class VisualElementHarmonyPatch
{
	[HarmonyPatch(typeof(VisualElement), "IncrementVersion")]
	private static class IncrementVersionPatch
	{
		[HarmonyPostfix]
		public static void Postfix(VisualElement __instance, VersionChangeType changeType)
		{
			if (!IsEditor(__instance))
			{
				switch (changeType)
				{
				case VersionChangeType.DisableRendering:
				{
					RenderingToggledEvent pooled2 = EventBase<RenderingToggledEvent>.GetPooled();
					pooled2.target = __instance;
					__instance.SendEvent(pooled2);
					break;
				}
				case VersionChangeType.Hierarchy:
				{
					HierarchyChangedEvent pooled = EventBase<HierarchyChangedEvent>.GetPooled();
					pooled.target = __instance;
					__instance.SendEvent(pooled);
					break;
				}
				}
			}
		}
	}

	[HarmonyPatch(typeof(VisualElement.Hierarchy), "PutChildAtIndex")]
	private static class PutChildAtIndexPatch
	{
		[HarmonyPostfix]
		public static void Postfix(VisualElement.Hierarchy __instance, VisualElement child, int index)
		{
			VisualElement visualElement = AccessTools.Field(typeof(VisualElement.Hierarchy), "m_Owner")?.GetValue(__instance) as VisualElement;
			if (!IsEditor(visualElement) && visualElement != null)
			{
				ChildAddedEvent pooled = EventBase<ChildAddedEvent>.GetPooled();
				pooled.index = index;
				pooled.child = child;
				pooled.target = visualElement;
				visualElement.SendEvent(pooled);
			}
		}
	}

	[HarmonyPatch(typeof(VisualElement.Hierarchy), "RemoveChildAtIndex")]
	private static class RemoveChildAtIndexPatch
	{
		[HarmonyPrefix]
		public static bool Prefix(VisualElement.Hierarchy __instance, int index)
		{
			VisualElement visualElement = AccessTools.Field(typeof(VisualElement.Hierarchy), "m_Owner")?.GetValue(__instance) as VisualElement;
			if (IsEditor(visualElement))
			{
				return true;
			}
			if (visualElement == null || index < 0 || index >= visualElement.childCount)
			{
				return true;
			}
			try
			{
				BeforeChildRemovedEvent pooled = EventBase<BeforeChildRemovedEvent>.GetPooled();
				pooled.index = index;
				pooled.child = visualElement.ElementAt(index);
				pooled.target = visualElement;
				visualElement.SendEvent(pooled);
			}
			catch (Exception arg)
			{
				Logger.Error(string.Format("Exception in BeforeChildRemovedEvent on {0}: {1}", visualElement?.name ?? "null", arg));
			}
			return true;
		}

		[HarmonyPostfix]
		public static void Postfix(VisualElement.Hierarchy __instance, int index)
		{
			VisualElement visualElement = AccessTools.Field(typeof(VisualElement.Hierarchy), "m_Owner")?.GetValue(__instance) as VisualElement;
			if (!IsEditor(visualElement) && visualElement != null)
			{
				ChildRemovedEvent pooled = EventBase<ChildRemovedEvent>.GetPooled();
				pooled.index = index;
				pooled.target = visualElement;
				visualElement.SendEvent(pooled);
			}
		}
	}

	private static readonly Logger Logger = new Logger("VisualElementHarmonyPatch");

	private static readonly Harmony harmony = new Harmony("Puck.VisualElement");

	private static bool IsEditor(VisualElement element)
	{
		if (element == null)
		{
			return false;
		}
		if (element.panel == null)
		{
			return false;
		}
		return element.panel.contextType == ContextType.Editor;
	}

	public static void Patch()
	{
		Logger.Info("Applying patches");
		harmony.PatchAll();
	}

	public static void Unpatch()
	{
		Logger.Info("Removing patches");
		harmony.UnpatchSelf();
	}
}
