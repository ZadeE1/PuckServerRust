using System.Collections.Generic;

public class UIChatController : UIViewController<UIChat>
{
	private UIChat uiChat;

	public override void Awake()
	{
		base.Awake();
		uiChat = GetComponent<UIChat>();
		EventManager.AddEventListener("Event_OnChatMessageAdded", Event_OnChatMessageAdded);
		EventManager.AddEventListener("Event_OnChatMessageRemoved", Event_OnChatMessageRemoved);
		EventManager.AddEventListener("Event_OnChatMessagesCleared", Event_OnChatMessagesCleared);
		EventManager.AddEventListener("Event_OnQuickChatEnabled", Event_OnQuickChatEnabled);
		EventManager.AddEventListener("Event_OnQuickChatDisabled", Event_OnQuickChatDisabled);
		EventManager.AddEventListener("Event_OnShowGameUserInterfaceChanged", Event_OnShowGameUserInterfaceChanged);
		EventManager.AddEventListener("Event_OnChatOpacityChanged", Event_OnChatOpacityChanged);
		EventManager.AddEventListener("Event_OnChatScaleChanged", Event_OnChatScaleChanged);
		EventManager.AddEventListener("Event_OnClientStopped", Event_OnClientStopped);
	}

	private void Start()
	{
		uiChat.SetOpacity(SettingsManager.ChatOpacity);
		uiChat.SetScale(SettingsManager.ChatScale);
	}

	public override void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_OnChatMessageAdded", Event_OnChatMessageAdded);
		EventManager.RemoveEventListener("Event_OnChatMessageRemoved", Event_OnChatMessageRemoved);
		EventManager.RemoveEventListener("Event_OnChatMessagesCleared", Event_OnChatMessagesCleared);
		EventManager.RemoveEventListener("Event_OnQuickChatEnabled", Event_OnQuickChatEnabled);
		EventManager.RemoveEventListener("Event_OnQuickChatDisabled", Event_OnQuickChatDisabled);
		EventManager.RemoveEventListener("Event_OnShowGameUserInterfaceChanged", Event_OnShowGameUserInterfaceChanged);
		EventManager.RemoveEventListener("Event_OnChatOpacityChanged", Event_OnChatOpacityChanged);
		EventManager.RemoveEventListener("Event_OnChatScaleChanged", Event_OnChatScaleChanged);
		EventManager.RemoveEventListener("Event_OnClientStopped", Event_OnClientStopped);
		base.OnDestroy();
	}

	private void Event_OnChatMessageAdded(Dictionary<string, object> message)
	{
		ChatMessage chatMessage = (ChatMessage)message["chatMessage"];
		uiChat.AddChatMessage(chatMessage, SettingsManager.Units, SettingsManager.FilterChatProfanity);
	}

	private void Event_OnChatMessageRemoved(Dictionary<string, object> message)
	{
		ChatMessage chatMessage = (ChatMessage)message["chatMessage"];
		uiChat.RemoveChatMessage(chatMessage);
	}

	private void Event_OnChatMessagesCleared(Dictionary<string, object> message)
	{
		uiChat.ClearChatMessages();
	}

	private void Event_OnQuickChatEnabled(Dictionary<string, object> message)
	{
		QuickChatCategory category = (QuickChatCategory)message["category"];
		QuickChat[] quickChats = (QuickChat[])message["quickChats"];
		uiChat.ShowQuickChat(category, quickChats);
	}

	private void Event_OnQuickChatDisabled(Dictionary<string, object> message)
	{
		uiChat.HideQuickChat();
	}

	private void Event_OnShowGameUserInterfaceChanged(Dictionary<string, object> message)
	{
		if (GlobalStateManager.UIState.Phase != UIPhase.LockerRoom)
		{
			if ((bool)message["value"])
			{
				uiChat.Show();
			}
			else
			{
				uiChat.Hide();
			}
		}
	}

	private void Event_OnChatOpacityChanged(Dictionary<string, object> message)
	{
		float opacity = (float)message["value"];
		uiChat.SetOpacity(opacity);
	}

	private void Event_OnChatScaleChanged(Dictionary<string, object> message)
	{
		float scale = (float)message["value"];
		uiChat.SetScale(scale);
	}

	private void Event_OnClientStopped(Dictionary<string, object> message)
	{
		uiChat.HideQuickChat();
	}
}
