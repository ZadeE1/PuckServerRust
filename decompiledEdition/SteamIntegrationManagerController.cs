using System.Collections.Generic;
using Steamworks;

public static class SteamIntegrationManagerController
{
	public static void Initialize()
	{
		EventManager.AddEventListener("Event_Everyone_OnGamePhaseChanged", Event_Everyone_OnGamePhaseChanged);
		EventManager.AddEventListener("Event_Everyone_OnPlayerGameStateChanged", Event_Everyone_OnPlayerGameStateChanged);
		EventManager.AddEventListener("Event_Everyone_OnServerChanged", Event_Everyone_OnServerChanged);
		EventManager.AddEventListener("Event_OnClientStopped", Event_OnClientStopped);
		EventManager.AddEventListener("Event_OnPlayerDataChanged", Event_OnPlayerDataChanged);
		EventManager.AddEventListener("Event_OnPlayerPartyDataChanged", Event_OnPlayerPartyDataChanged);
		EventManager.AddEventListener("Event_OnScoreboardClickPlayer", Event_OnScoreboardClickPlayer);
		EventManager.AddEventListener("Event_OnModsClickFindMods", Event_OnModsClickFindMods);
		EventManager.AddEventListener("Event_OnModPreviewLinkClicked", Event_OnModPreviewLinkClicked);
		EventManager.AddEventListener("Event_OnFriendInviteButtonClicked", Event_OnFriendInviteButtonClicked);
		EventManager.AddEventListener("Event_OnGameLobbyJoinRequested", Event_OnGameLobbyJoinRequested);
		EventManager.AddEventListener("Event_OnAppearanceClickPurchaseItem", Event_OnAppearanceClickPurchaseItem);
		EventManager.AddEventListener("Event_OnMicroTxnAuthorizationResponse", Event_OnMicroTxnAuthorizationResponse);
		WebSocketManager.AddMessageListener("connected", WebSocket_Event_OnConnected);
		WebSocketManager.AddMessageListener("disconnected", WebSocket_Event_OnDisconnected);
		WebSocketManager.AddMessageListener("playerAuthenticateResponse", WebSocket_Event_OnPlayerAuthenticateResponse);
		WebSocketManager.AddMessageListener("playerJoinPartyResponse", WebSocket_Event_OnPlayerJoinPartyResponse);
	}

	public static void Dispose()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnGamePhaseChanged", Event_Everyone_OnGamePhaseChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerGameStateChanged", Event_Everyone_OnPlayerGameStateChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnServerChanged", Event_Everyone_OnServerChanged);
		EventManager.RemoveEventListener("Event_OnClientStopped", Event_OnClientStopped);
		EventManager.RemoveEventListener("Event_OnPlayerDataChanged", Event_OnPlayerDataChanged);
		EventManager.RemoveEventListener("Event_OnPlayerPartyDataChanged", Event_OnPlayerPartyDataChanged);
		EventManager.RemoveEventListener("Event_OnScoreboardClickPlayer", Event_OnScoreboardClickPlayer);
		EventManager.RemoveEventListener("Event_OnModsClickFindMods", Event_OnModsClickFindMods);
		EventManager.RemoveEventListener("Event_OnModPreviewLinkClicked", Event_OnModPreviewLinkClicked);
		EventManager.RemoveEventListener("Event_OnFriendInviteButtonClicked", Event_OnFriendInviteButtonClicked);
		EventManager.RemoveEventListener("Event_OnGameLobbyJoinRequested", Event_OnGameLobbyJoinRequested);
		EventManager.RemoveEventListener("Event_OnAppearanceClickPurchaseItem", Event_OnAppearanceClickPurchaseItem);
		EventManager.RemoveEventListener("Event_OnMicroTxnAuthorizationResponse", Event_OnMicroTxnAuthorizationResponse);
		WebSocketManager.RemoveMessageListener("connected", WebSocket_Event_OnConnected);
		WebSocketManager.RemoveMessageListener("disconnected", WebSocket_Event_OnDisconnected);
		WebSocketManager.RemoveMessageListener("playerAuthenticateResponse", WebSocket_Event_OnPlayerAuthenticateResponse);
		WebSocketManager.RemoveMessageListener("playerJoinPartyResponse", WebSocket_Event_OnPlayerJoinPartyResponse);
	}

	private static void Event_Everyone_OnGamePhaseChanged(Dictionary<string, object> message)
	{
		GameState gameState = (GameState)message["gameState"];
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			SteamIntegrationManager.UpdateRichPresencePhase(gameState.Phase);
			SteamIntegrationManager.UpdateRichPresenceScore(gameState.Phase != GamePhase.Warmup, gameState.Period, gameState.BlueScore, gameState.RedScore);
		}
	}

	private static void Event_Everyone_OnPlayerGameStateChanged(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		Server value = NetworkBehaviourSingleton<ServerManager>.Instance.Server.Value;
		GameState value2 = NetworkBehaviourSingleton<GameManager>.Instance.GameState.Value;
		if (!ApplicationManager.IsDedicatedGameServer && player.IsLocalPlayer)
		{
			PlayerTeam team = player.Team;
			if ((uint)(team - 1) <= 1u)
			{
				SteamIntegrationManager.UpdateRichPresencePhase(value2.Phase);
				SteamIntegrationManager.UpdateRichPresenceTeam(player.Team);
				SteamIntegrationManager.UpdateRichPresenceRole(player.Role);
				SteamIntegrationManager.UpdateRichPresenceScore(value2.Phase != GamePhase.Warmup, value2.Period, value2.BlueScore, value2.RedScore);
				SteamIntegrationManager.SetRichPresencePlaying(value, MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayers().Count);
			}
			else
			{
				SteamIntegrationManager.UpdateRichPresenceScore(value2.Phase != GamePhase.Warmup, value2.Period, value2.BlueScore, value2.RedScore);
				SteamIntegrationManager.SetRichPresenceSpectating(value, MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayers().Count);
			}
		}
	}

	private static void Event_Everyone_OnServerChanged(Dictionary<string, object> message)
	{
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			SteamIntegrationManager.UpdateRichPresenceScore(show: false, 0, 0, 0);
			SteamIntegrationManager.SetRichPresenceSpectating(NetworkBehaviourSingleton<ServerManager>.Instance.Server.Value, 1);
		}
	}

	private static void Event_OnClientStopped(Dictionary<string, object> message)
	{
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			SteamIntegrationManager.SetRichPresenceMainMenu();
		}
	}

	private static void Event_OnPlayerDataChanged(Dictionary<string, object> message)
	{
		PlayerData playerData = (PlayerData)message["oldPlayerData"];
		PlayerData playerData2 = (PlayerData)message["newPlayerData"];
		if (playerData == null && playerData2 != null)
		{
			SteamIntegrationManager.GetLaunchCommandLine();
		}
	}

	private static void Event_OnPlayerPartyDataChanged(Dictionary<string, object> message)
	{
		PlayerPartyData playerPartyData = (PlayerPartyData)message["oldPlayerPartyData"];
		PlayerPartyData playerPartyData2 = (PlayerPartyData)message["newPlayerPartyData"];
		bool flag = playerPartyData != null;
		bool flag2 = playerPartyData2 != null;
		bool flag3 = flag && playerPartyData.steamLobbyId != null;
		bool flag4 = flag2 && playerPartyData2.steamLobbyId != null;
		bool flag5 = flag2 && playerPartyData2.ownerSteamId == BackendManager.PlayerState.PlayerData.steamId;
		bool flag6 = playerPartyData?.steamLobbyId != playerPartyData2?.steamLobbyId;
		bool num = flag4 & flag6;
		bool num2 = flag3 & flag6;
		bool flag7 = (flag2 && !flag4) & flag5;
		if (num2)
		{
			SteamIntegrationManager.LeaveLobby(playerPartyData.steamLobbyId);
		}
		if (flag7)
		{
			SteamIntegrationManager.CreateLobby();
		}
		if (num)
		{
			SteamIntegrationManager.JoinLobby(playerPartyData2.steamLobbyId);
		}
	}

	private static void Event_OnScoreboardClickPlayer(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		SteamFriends.ActivateGameOverlayToUser("steamID", new CSteamID(ulong.Parse(player.SteamId.Value.ToString())));
	}

	private static void Event_OnModsClickFindMods(Dictionary<string, object> message)
	{
		SteamFriends.ActivateGameOverlayToWebPage("https://steamcommunity.com/app/2994020/workshop/");
	}

	private static void Event_OnModPreviewLinkClicked(Dictionary<string, object> message)
	{
		string text = (string)message["id"];
		SteamFriends.ActivateGameOverlayToWebPage("https://steamcommunity.com/sharedfiles/filedetails/?id=" + text);
	}

	private static void Event_OnFriendInviteButtonClicked(Dictionary<string, object> message)
	{
		string invitedSteamId = (string)message["steamId"];
		PlayerPartyData partyData = BackendManager.PlayerState.PartyData;
		if (partyData != null && partyData.steamLobbyId != null)
		{
			SteamIntegrationManager.InviteToLobby(BackendManager.PlayerState.PartyData.steamLobbyId, invitedSteamId);
		}
	}

	private static void Event_OnGameLobbyJoinRequested(Dictionary<string, object> message)
	{
		SteamIntegrationManager.JoinLobby((string)message["lobbyId"]);
	}

	private static void Event_OnAppearanceClickPurchaseItem(Dictionary<string, object> message)
	{
		Item item = (Item)message["item"];
		EventManager.TriggerEvent("Event_OnTransactionStarting", new Dictionary<string, object> { { "itemId", item.id } });
		if (!SteamUtils.IsOverlayEnabled())
		{
			EventManager.TriggerEvent("Event_OnTransactionStartFailed", new Dictionary<string, object> { { "error", "Steam overlay disabled" } });
		}
		else
		{
			WebSocketManager.Emit("playerStartPurchaseRequest", new Dictionary<string, object> { { "itemId", item.id } }, "playerStartPurchaseResponse");
		}
	}

	private static void Event_OnMicroTxnAuthorizationResponse(Dictionary<string, object> message)
	{
		bool flag = (bool)message["authorized"];
		ulong num = (ulong)message["orderId"];
		if (flag)
		{
			WebSocketManager.Emit("playerCompletePurchaseRequest", new Dictionary<string, object> { { "orderId", num } }, "playerCompletePurchaseResponse");
		}
	}

	private static void WebSocket_Event_OnConnected(Dictionary<string, object> message)
	{
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			SteamIntegrationManager.GetTicketForWebApi();
		}
	}

	private static void WebSocket_Event_OnDisconnected(Dictionary<string, object> message)
	{
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			SteamIntegrationManager.StopTicketForWebApi();
		}
	}

	private static void WebSocket_Event_OnPlayerAuthenticateResponse(Dictionary<string, object> message)
	{
		if (((InMessage)message["inMessage"]).GetData<PlayerAuthenticateResponse>().success)
		{
			SteamIntegrationManager.SetRichPresenceMainMenu();
		}
	}

	private static void WebSocket_Event_OnPlayerJoinPartyResponse(Dictionary<string, object> message)
	{
		OutMessage outMessage = (OutMessage)message["outMessage"];
		PlayerJoinPartyResponse data = ((InMessage)message["inMessage"]).GetData<PlayerJoinPartyResponse>();
		string text = (string)outMessage.Data["steamLobbyId"];
		if (!data.success && text != null)
		{
			SteamIntegrationManager.LeaveLobby(text);
		}
	}
}
