using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

public static class GlobalStateManagerController
{
	private static readonly Logger Logger = new Logger("GlobalStateManagerController");

	public static void Initialize()
	{
		EventManager.AddEventListener("Event_OnSceneLoaded", Event_OnSceneLoaded);
		EventManager.AddEventListener("Event_OnPopupClickOk", Event_OnPopupClickOk);
		EventManager.AddEventListener("Event_OnPopupClickClose", Event_OnPopupClickClose);
		EventManager.AddEventListener("Event_OnModStateChanged", Event_OnModStateChanged);
		EventManager.AddEventListener("Event_OnModEnableFailed", Event_OnModEnableFailed);
	}

	public static void Dispose()
	{
		EventManager.RemoveEventListener("Event_OnSceneLoaded", Event_OnSceneLoaded);
		EventManager.RemoveEventListener("Event_OnPopupClickOk", Event_OnPopupClickOk);
		EventManager.RemoveEventListener("Event_OnPopupClickClose", Event_OnPopupClickClose);
		EventManager.RemoveEventListener("Event_OnModStateChanged", Event_OnModStateChanged);
		EventManager.RemoveEventListener("Event_OnModEnableFailed", Event_OnModEnableFailed);
	}

	private static void Event_OnSceneLoaded(Dictionary<string, object> message)
	{
		if (((Scene)message["scene"]).name == "level_default")
		{
			GlobalStateManager.SetUIState(new Dictionary<string, object> { 
			{
				"phase",
				UIPhase.Playing
			} });
		}
		else
		{
			GlobalStateManager.SetUIState(new Dictionary<string, object> { 
			{
				"phase",
				UIPhase.LockerRoom
			} });
		}
	}

	private static void Event_OnPopupClickOk(Dictionary<string, object> message)
	{
		Popup popup = (Popup)message["popup"];
		string name = popup.Name;
		if (!(name == "missingPassword"))
		{
			if (name == "missingMods" && GlobalStateManager.ReconnectionState.Phase == ReconnectionPhase.AwaitingMods)
			{
				string[] clientRequiredModIds = GlobalStateManager.ReconnectionState.ClientRequiredModIds;
				string[] second = ModManager.EnabledMods.Select((Mod mod) => mod.Id).ToArray();
				string[] second2 = ModManager.ReadyMods.Select((Mod mod) => mod.Id).ToArray();
				string[] array = clientRequiredModIds.Except(second2).ToArray();
				string[] value = clientRequiredModIds.Except(array).Except(second).ToArray();
				GlobalStateManager.SetReconnectionState(new Dictionary<string, object>
				{
					{ "pendingReadinessModIds", array },
					{ "pendingEnablingModIds", value }
				});
			}
		}
		else if (GlobalStateManager.ReconnectionState.Phase == ReconnectionPhase.AwaitingPassword)
		{
			PopupMissingPasswordContent popupMissingPasswordContent = (PopupMissingPasswordContent)popup.Content;
			if (string.IsNullOrEmpty(popupMissingPasswordContent.Password))
			{
				GlobalStateManager.ClearReconnectionState();
				return;
			}
			GlobalStateManager.SetReconnectionState(new Dictionary<string, object> { { "password", popupMissingPasswordContent.Password } });
		}
	}

	private static void Event_OnPopupClickClose(Dictionary<string, object> message)
	{
		string name = ((Popup)message["popup"]).Name;
		if ((name == "missingPassword" || name == "missingMods") && (GlobalStateManager.ReconnectionState.Phase == ReconnectionPhase.AwaitingPassword || GlobalStateManager.ReconnectionState.Phase == ReconnectionPhase.AwaitingMods))
		{
			GlobalStateManager.ClearReconnectionState();
		}
	}

	private static void Event_OnModStateChanged(Dictionary<string, object> message)
	{
		Mod mod = (Mod)message["mod"];
		BasePluginState basePluginState = (BasePluginState)message["oldState"];
		BasePluginState basePluginState2 = (BasePluginState)message["newState"];
		if (GlobalStateManager.ReconnectionState.Phase != ReconnectionPhase.AwaitingMods)
		{
			return;
		}
		bool flag = GlobalStateManager.ReconnectionState.PendingReadinessModIds.Contains(mod.Id);
		bool flag2 = GlobalStateManager.ReconnectionState.PendingEnablingModIds.Contains(mod.Id);
		bool flag3 = basePluginState.IsEnabled != basePluginState2.IsEnabled;
		bool flag4 = basePluginState.IsReady != basePluginState2.IsReady;
		if ((flag & flag4) && basePluginState2.IsReady)
		{
			GlobalStateManager.SetReconnectionState(new Dictionary<string, object>
			{
				{
					"pendingReadinessModIds",
					GlobalStateManager.ReconnectionState.PendingReadinessModIds.Where((string id) => id != mod.Id).ToArray()
				},
				{
					"pendingEnablingModIds",
					GlobalStateManager.ReconnectionState.PendingEnablingModIds.Append(mod.Id).ToArray()
				}
			});
		}
		else if ((flag2 & flag3) && basePluginState2.IsEnabled)
		{
			GlobalStateManager.SetReconnectionState(new Dictionary<string, object> { 
			{
				"pendingEnablingModIds",
				GlobalStateManager.ReconnectionState.PendingEnablingModIds.Where((string id) => id != mod.Id).ToArray()
			} });
		}
	}

	private static void Event_OnModEnableFailed(Dictionary<string, object> message)
	{
		if (GlobalStateManager.ReconnectionState.Phase == ReconnectionPhase.AwaitingMods)
		{
			GlobalStateManager.ClearReconnectionState();
		}
	}
}
