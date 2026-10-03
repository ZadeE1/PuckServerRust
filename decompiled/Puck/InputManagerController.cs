using System.Collections.Generic;

public static class InputManagerController
{
	public static void Initialize()
	{
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			InputManager.LoadKeyBinds();
			InputManager.MigrateChatOpenInteractionToPress();
		}
		AddSettingsEventListeners();
	}

	public static void Dispose()
	{
		RemoveSettingsEventListeners();
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			InputManager.SaveKeyBinds();
		}
	}

	private static void AddSettingsEventListeners()
	{
		EventManager.AddEventListener("Event_OnSettingsKeyBindInputClicked", Event_OnSettingsKeyBindInputClicked);
		EventManager.AddEventListener("Event_OnSettingsKeyBindInputInteractionChanged", Event_OnSettingsKeyBindInputInteractionChanged);
		EventManager.AddEventListener("Event_OnPopupClickOk", Event_OnPopupClickOk);
	}

	private static void RemoveSettingsEventListeners()
	{
		EventManager.RemoveEventListener("Event_OnSettingsKeyBindInputClicked", Event_OnSettingsKeyBindInputClicked);
		EventManager.RemoveEventListener("Event_OnSettingsKeyBindInputInteractionChanged", Event_OnSettingsKeyBindInputInteractionChanged);
		EventManager.RemoveEventListener("Event_OnPopupClickOk", Event_OnPopupClickOk);
	}

	private static void Event_OnSettingsKeyBindInputClicked(Dictionary<string, object> message)
	{
		InputManager.RebindButtonInteractively((string)message["actionName"]);
	}

	private static void Event_OnSettingsKeyBindInputInteractionChanged(Dictionary<string, object> message)
	{
		string actionName = (string)message["actionName"];
		KeyBindInteraction keyBindInteraction = (KeyBindInteraction)message["interaction"];
		InputManager.SetActionInteractions(actionName, Utils.GetInteractionFromKeyBindInteraction(keyBindInteraction));
	}

	private static void Event_OnPopupClickOk(Dictionary<string, object> message)
	{
		if (((Popup)message["popup"]).Name == "settingsResetToDefault")
		{
			InputManager.ResetToDefault();
		}
	}
}
