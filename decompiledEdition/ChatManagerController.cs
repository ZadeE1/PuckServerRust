using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChatManagerController : MonoBehaviour
{
	private ChatManager chatManager;

	private void Awake()
	{
		chatManager = GetComponent<ChatManager>();
		InputManager.QuickChat1Action.performed += OnQuickChatAction1Performed;
		InputManager.QuickChat2Action.performed += OnQuickChatAction2Performed;
		InputManager.QuickChat3Action.performed += OnQuickChatAction3Performed;
		InputManager.QuickChat4Action.performed += OnQuickChatAction4Performed;
		InputManager.QuickChat5Action.performed += OnQuickChatAction5Performed;
		EventManager.AddEventListener("Event_OnChatSubmitMessage", Event_OnChatSubmitMessage);
		EventManager.AddEventListener("Event_OnQuickChatCancel", Event_OnQuickChatCancel);
		EventManager.AddEventListener("Event_OnClientStopped", Event_OnClientStopped);
		EventManager.AddEventListener("Event_Server_OnChatMessageReceived", Event_Server_OnChatMessageReceived);
		WebSocketManager.AddMessageListener("playerAnnouncement", WebSocket_Event_OnPlayerAnnouncement);
	}

	private void OnDestroy()
	{
		InputManager.QuickChat1Action.performed -= OnQuickChatAction1Performed;
		InputManager.QuickChat2Action.performed -= OnQuickChatAction2Performed;
		InputManager.QuickChat3Action.performed -= OnQuickChatAction3Performed;
		InputManager.QuickChat4Action.performed -= OnQuickChatAction4Performed;
		InputManager.QuickChat5Action.performed -= OnQuickChatAction5Performed;
		EventManager.RemoveEventListener("Event_OnChatSubmitMessage", Event_OnChatSubmitMessage);
		EventManager.RemoveEventListener("Event_OnQuickChatCancel", Event_OnQuickChatCancel);
		EventManager.RemoveEventListener("Event_OnClientStopped", Event_OnClientStopped);
		EventManager.RemoveEventListener("Event_Server_OnChatMessageReceived", Event_Server_OnChatMessageReceived);
		WebSocketManager.RemoveMessageListener("playerAnnouncement", WebSocket_Event_OnPlayerAnnouncement);
	}

	private void OnQuickChatAction1Performed(InputAction.CallbackContext context)
	{
		if (GlobalStateManager.UIState.Phase == UIPhase.Playing && !GlobalStateManager.UIState.IsInteracting)
		{
			chatManager.Client_QuickChatAction(0);
		}
	}

	private void OnQuickChatAction2Performed(InputAction.CallbackContext context)
	{
		if (GlobalStateManager.UIState.Phase == UIPhase.Playing && !GlobalStateManager.UIState.IsInteracting)
		{
			chatManager.Client_QuickChatAction(1);
		}
	}

	private void OnQuickChatAction3Performed(InputAction.CallbackContext context)
	{
		if (GlobalStateManager.UIState.Phase == UIPhase.Playing && !GlobalStateManager.UIState.IsInteracting)
		{
			chatManager.Client_QuickChatAction(2);
		}
	}

	private void OnQuickChatAction4Performed(InputAction.CallbackContext context)
	{
		if (GlobalStateManager.UIState.Phase == UIPhase.Playing && !GlobalStateManager.UIState.IsInteracting)
		{
			chatManager.Client_QuickChatAction(3);
		}
	}

	private void OnQuickChatAction5Performed(InputAction.CallbackContext context)
	{
		if (GlobalStateManager.UIState.Phase == UIPhase.Playing && !GlobalStateManager.UIState.IsInteracting)
		{
			chatManager.Client_QuickChatAction(4);
		}
	}

	private void Event_OnChatSubmitMessage(Dictionary<string, object> message)
	{
		string content = (string)message["content"];
		bool isTeamChat = (bool)message["isTeamChat"];
		chatManager.Client_SendChatMessage(content, isQuickChat: false, isTeamChat);
	}

	private void Event_OnQuickChatCancel(Dictionary<string, object> message)
	{
		chatManager.SetQuickChatEnabled(isEnabled: false);
	}

	private void Event_OnClientStopped(Dictionary<string, object> message)
	{
		chatManager.ClearChatMessages();
	}

	private void Event_Server_OnChatMessageReceived(Dictionary<string, object> message)
	{
		ChatMessage chatMessage = (ChatMessage)message["chatMessage"];
		if (chatMessage.IsTeamChat)
		{
			if (!chatMessage.Team.HasValue)
			{
				return;
			}
			List<ulong> list = new List<ulong>();
			if (chatMessage.Team.Value == PlayerTeam.Spectator || chatMessage.Team.Value == PlayerTeam.None)
			{
				list.AddRange(MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayersByTeams(new PlayerTeam[2]
				{
					PlayerTeam.None,
					PlayerTeam.Spectator
				}).ConvertAll((Player player) => player.OwnerClientId));
			}
			else
			{
				list.AddRange(MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayersByTeam(chatMessage.Team.Value).ConvertAll((Player player) => player.OwnerClientId));
			}
			chatManager.Server_SendChatMessage(chatMessage, list.ToArray());
		}
		else
		{
			chatManager.Server_BroadcastChatMessage(chatMessage);
		}
	}

	private void WebSocket_Event_OnPlayerAnnouncement(Dictionary<string, object> message)
	{
		InMessage inMessage = (InMessage)message["inMessage"];
		ChatMessage chatMessage = new ChatMessage
		{
			SteamID = null,
			Username = null,
			Team = null,
			Content = inMessage.GetData<PlayerAnnouncementMessage>().message,
			Timestamp = Utils.GetTimestamp(),
			IsQuickChat = false,
			IsTeamChat = false,
			IsSystem = true
		};
		chatManager.AddChatMessage(chatMessage);
	}
}
