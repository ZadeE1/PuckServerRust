using System.Collections.Generic;
using Unity.Netcode;

public class UIToastManagerController : UIViewController<UIToastManager>
{
	private UIToastManager uiToastManager;

	public override void Awake()
	{
		base.Awake();
		uiToastManager = GetComponent<UIToastManager>();
		EventManager.AddEventListener("Event_Everyone_OnClientConnected", Event_Everyone_OnClientConnected);
		EventManager.AddEventListener("Event_OnSteamInitializationStarted", Event_OnSteamInitializationStarted);
		EventManager.AddEventListener("Event_OnSteamInitializationFailed", Event_OnSteamInitializationFailed);
		EventManager.AddEventListener("Event_OnSteamInitialized", Event_OnSteamInitialized);
		EventManager.AddEventListener("Event_OnSteamConnectionFailed", Event_OnSteamConnectionFailed);
		EventManager.AddEventListener("Event_OnSteamConnected", Event_OnSteamConnected);
		EventManager.AddEventListener("Event_OnSteamDisconnected", Event_OnSteamDisconnected);
		EventManager.AddEventListener("Event_OnSteamAuthenticationStarted", Event_OnSteamAuthenticationStarted);
		EventManager.AddEventListener("Event_OnSteamAuthenticationStalled", Event_OnSteamAuthenticationStalled);
		EventManager.AddEventListener("Event_OnPlayerStateChanged", Event_OnPlayerStateChanged);
		EventManager.AddEventListener("Event_OnTransportFailure", Event_OnTransportFailure);
		EventManager.AddEventListener("Event_OnServerStartFailed", Event_OnServerStartFailed);
		EventManager.AddEventListener("Event_OnClientStarted", Event_OnClientStarted);
		EventManager.AddEventListener("Event_OnClientStopped", Event_OnClientStopped);
		EventManager.AddEventListener("Event_OnConnectionRejected", Event_OnConnectionRejected);
		EventManager.AddEventListener("Event_OnDisconnected", Event_OnDisconnected);
		EventManager.AddEventListener("Event_OnPluginEnableFailed", Event_OnPluginEnableFailed);
		EventManager.AddEventListener("Event_OnPluginDisableFailed", Event_OnPluginDisableFailed);
		EventManager.AddEventListener("Event_OnModEnableFailed", Event_OnModEnableFailed);
		EventManager.AddEventListener("Event_OnModDisableFailed", Event_OnModDisableFailed);
		WebSocketManager.AddMessageListener("emit", WebSocket_Event_OnEmit);
		WebSocketManager.AddMessageListener("connecting", WebSocket_Event_OnConnecting);
		WebSocketManager.AddMessageListener("connected", WebSocket_Event_OnConnected);
		WebSocketManager.AddMessageListener("disconnected", WebSocket_Event_OnDisconnected);
		WebSocketManager.AddMessageListener("PlayerStartTransactionResponse", WebSocket_Event_OnPlayerStartTransactionResponse);
		WebSocketManager.AddMessageListener("playerDeployServerResponse", WebSocket_Event_OnPlayerDeployServerResponse);
		WebSocketManager.AddMessageListener("playerSetIdentityResponse", WebSocket_Event_OnPlayerSetIdentityResponse);
		WebSocketManager.AddMessageListener("playerJoinPartyResponse", WebSocket_Event_OnPlayerJoinPartyResponse);
		WebSocketManager.AddMessageListener("playerStartMatchmakingResponse", WebSocket_Event_OnPlayerStartMatchmakingResponse);
	}

	public override void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnClientConnected", Event_Everyone_OnClientConnected);
		EventManager.RemoveEventListener("Event_OnSteamInitializationStarted", Event_OnSteamInitializationStarted);
		EventManager.RemoveEventListener("Event_OnSteamInitializationFailed", Event_OnSteamInitializationFailed);
		EventManager.RemoveEventListener("Event_OnSteamInitialized", Event_OnSteamInitialized);
		EventManager.RemoveEventListener("Event_OnSteamConnectionFailed", Event_OnSteamConnectionFailed);
		EventManager.RemoveEventListener("Event_OnSteamConnected", Event_OnSteamConnected);
		EventManager.RemoveEventListener("Event_OnSteamDisconnected", Event_OnSteamDisconnected);
		EventManager.RemoveEventListener("Event_OnSteamAuthenticationStarted", Event_OnSteamAuthenticationStarted);
		EventManager.RemoveEventListener("Event_OnSteamAuthenticationStalled", Event_OnSteamAuthenticationStalled);
		EventManager.RemoveEventListener("Event_OnPlayerStateChanged", Event_OnPlayerStateChanged);
		EventManager.RemoveEventListener("Event_OnTransportFailure", Event_OnTransportFailure);
		EventManager.RemoveEventListener("Event_OnServerStartFailed", Event_OnServerStartFailed);
		EventManager.RemoveEventListener("Event_OnClientStarted", Event_OnClientStarted);
		EventManager.RemoveEventListener("Event_OnClientStopped", Event_OnClientStopped);
		EventManager.RemoveEventListener("Event_OnConnectionRejected", Event_OnConnectionRejected);
		EventManager.RemoveEventListener("Event_OnDisconnected", Event_OnDisconnected);
		EventManager.RemoveEventListener("Event_OnPluginEnableFailed", Event_OnPluginEnableFailed);
		EventManager.RemoveEventListener("Event_OnPluginDisableFailed", Event_OnPluginDisableFailed);
		EventManager.RemoveEventListener("Event_OnModEnableFailed", Event_OnModEnableFailed);
		EventManager.RemoveEventListener("Event_OnModDisableFailed", Event_OnModDisableFailed);
		WebSocketManager.RemoveMessageListener("emit", WebSocket_Event_OnEmit);
		WebSocketManager.RemoveMessageListener("connecting", WebSocket_Event_OnConnecting);
		WebSocketManager.RemoveMessageListener("connected", WebSocket_Event_OnConnected);
		WebSocketManager.RemoveMessageListener("disconnected", WebSocket_Event_OnDisconnected);
		WebSocketManager.RemoveMessageListener("PlayerStartTransactionResponse", WebSocket_Event_OnPlayerStartTransactionResponse);
		WebSocketManager.RemoveMessageListener("playerDeployServerResponse", WebSocket_Event_OnPlayerDeployServerResponse);
		WebSocketManager.RemoveMessageListener("playerSetIdentityResponse", WebSocket_Event_OnPlayerSetIdentityResponse);
		WebSocketManager.RemoveMessageListener("playerJoinPartyResponse", WebSocket_Event_OnPlayerJoinPartyResponse);
		WebSocketManager.RemoveMessageListener("playerStartMatchmakingResponse", WebSocket_Event_OnPlayerStartMatchmakingResponse);
		base.OnDestroy();
	}

	private void Event_Everyone_OnClientConnected(Dictionary<string, object> message)
	{
		ulong num = (ulong)message["clientId"];
		if (NetworkManager.Singleton.LocalClientId == num)
		{
			uiToastManager.HideToast("serverConnection");
		}
	}

	private void Event_OnSteamInitializationStarted(Dictionary<string, object> message)
	{
		uiToastManager.ShowToast("steamInitialization", "Initializing Steam...", float.PositiveInfinity);
	}

	private void Event_OnSteamInitializationFailed(Dictionary<string, object> message)
	{
		string content = ((((message != null && message.ContainsKey("attempts")) ? ((int)message["attempts"]) : 0) >= 3) ? "Can't connect to Steam. Make sure the Steam client is running and signed in, or try restarting it." : "Failed to initialize Steam, retrying...");
		uiToastManager.ShowToast("steamInitialization", content, float.PositiveInfinity);
	}

	private void Event_OnSteamInitialized(Dictionary<string, object> message)
	{
		uiToastManager.HideToast("steamInitialization");
		uiToastManager.ShowToast("steamConnection", "Connecting to Steam...", float.PositiveInfinity);
	}

	private void Event_OnSteamConnectionFailed(Dictionary<string, object> message)
	{
		uiToastManager.ShowToast("steamConnection", "Failed to connect to Steam, retrying...", float.PositiveInfinity);
	}

	private void Event_OnSteamConnected(Dictionary<string, object> message)
	{
		uiToastManager.HideToast("steamConnection");
	}

	private void Event_OnSteamDisconnected(Dictionary<string, object> message)
	{
		uiToastManager.ShowToast("steamConnection", "Disconnected from Steam, reconnecting...", float.PositiveInfinity);
	}

	private void Event_OnSteamAuthenticationStarted(Dictionary<string, object> message)
	{
		uiToastManager.ShowToast("steamAuthentication", "Signing in to Steam...", float.PositiveInfinity);
	}

	private void Event_OnSteamAuthenticationStalled(Dictionary<string, object> message)
	{
		uiToastManager.ShowToast("steamAuthentication", "Can't reach Steam. Try restarting the Steam client.", float.PositiveInfinity);
	}

	private void Event_OnPlayerStateChanged(Dictionary<string, object> message)
	{
		if (((PlayerState)message["newPlayerState"]).AuthenticationPhase == AuthenticationPhase.Authenticating)
		{
			uiToastManager.HideToast("steamAuthentication");
			uiToastManager.ShowToast("playerAuthentication", "Authenticating...", float.PositiveInfinity);
		}
		else
		{
			uiToastManager.HideToast("playerAuthentication");
		}
	}

	private void Event_OnTransportFailure(Dictionary<string, object> message)
	{
		uiToastManager.ShowToast("transportFailure", "Network transport failure");
	}

	private void Event_OnServerStartFailed(Dictionary<string, object> message)
	{
		ushort num = (ushort)message["port"];
		uiToastManager.ShowToast("serverStartFailed", $"Failed to start server (is port {num} already in use?)", 6f);
	}

	private void Event_OnClientStarted(Dictionary<string, object> message)
	{
		uiToastManager.ShowToast("serverConnection", "Connecting to server...", float.PositiveInfinity);
	}

	private void Event_OnClientStopped(Dictionary<string, object> message)
	{
		uiToastManager.HideToast("serverConnection");
	}

	private void Event_OnConnectionRejected(Dictionary<string, object> message)
	{
		ConnectionRejection connectionRejection = (ConnectionRejection)message["connectionRejection"];
		if (connectionRejection.code != ConnectionRejectionCode.MissingPassword && connectionRejection.code != ConnectionRejectionCode.MissingMods)
		{
			uiToastManager.ShowToast("connectionRejected", "Connection rejected: " + Utils.GetConnectionRejectionMessage(connectionRejection.code, connectionRejection.message));
		}
	}

	private void Event_OnDisconnected(Dictionary<string, object> message)
	{
		Disconnection disconnection = (Disconnection)message["disconnection"];
		if (disconnection.code != DisconnectionCode.Disconnected)
		{
			uiToastManager.ShowToast("disconnected", "Disconnected: " + Utils.GetDisconnectionMessage(disconnection.code, disconnection.message));
		}
	}

	private void Event_OnPluginEnableFailed(Dictionary<string, object> message)
	{
		Plugin plugin = (Plugin)message["plugin"];
		uiToastManager.ShowToast("pluginEnableFailed_" + plugin.Id, "Failed to enable plugin " + plugin.Id);
	}

	private void Event_OnPluginDisableFailed(Dictionary<string, object> message)
	{
		Plugin plugin = (Plugin)message["plugin"];
		uiToastManager.ShowToast("pluginDisableFailed_" + plugin.Id, "Failed to disable plugin " + plugin.Id);
	}

	private void Event_OnModEnableFailed(Dictionary<string, object> message)
	{
		Mod mod = (Mod)message["mod"];
		string text = mod.SteamWorkshopItem.Details?.Title ?? mod.Id;
		uiToastManager.ShowToast("modEnableFailed_" + mod.Id, "Failed to enable mod " + text);
	}

	private void Event_OnModDisableFailed(Dictionary<string, object> message)
	{
		Mod mod = (Mod)message["mod"];
		string text = mod.SteamWorkshopItem.Details?.Title ?? mod.Id;
		uiToastManager.ShowToast("modDisableFailed_" + mod.Id, "Failed to disable mod " + text);
	}

	private void WebSocket_Event_OnEmit(Dictionary<string, object> message)
	{
		if ((string)message["messageName"] == "playerDeployServerRequest")
		{
			uiToastManager.ShowToast("playerDeployServer", "Deploying server...", float.PositiveInfinity);
		}
	}

	private void WebSocket_Event_OnConnecting(Dictionary<string, object> message)
	{
		uiToastManager.ShowToast("webSocketConnection", "Connecting to Puck backend...", float.PositiveInfinity);
	}

	private void WebSocket_Event_OnConnected(Dictionary<string, object> message)
	{
		uiToastManager.HideToast("webSocketConnection");
	}

	private void WebSocket_Event_OnDisconnected(Dictionary<string, object> message)
	{
		uiToastManager.HideToast("steamAuthentication");
		uiToastManager.ShowToast("webSocketConnection", "Disconnected from Puck backend, reconnecting...", float.PositiveInfinity);
	}

	private void WebSocket_Event_OnPlayerStartTransactionResponse(Dictionary<string, object> message)
	{
		PlayerStartTransactionResponse data = ((InMessage)message["inMessage"]).GetData<PlayerStartTransactionResponse>();
		if (!data.success)
		{
			uiToastManager.ShowToast("playerStartTransaction", "Failed to start transaction: " + data.errorData.message);
		}
	}

	private void WebSocket_Event_OnPlayerDeployServerResponse(Dictionary<string, object> message)
	{
		PlayerDeployServerResponse data = ((InMessage)message["inMessage"]).GetData<PlayerDeployServerResponse>();
		if (data.success)
		{
			uiToastManager.ShowToast("playerDeployServer", "Server deployed!");
		}
		else
		{
			uiToastManager.ShowToast("playerDeployServer", "Failed to deploy server: " + data.errorData.message);
		}
	}

	private void WebSocket_Event_OnPlayerSetIdentityResponse(Dictionary<string, object> message)
	{
		PlayerSetIdentityResponse data = ((InMessage)message["inMessage"]).GetData<PlayerSetIdentityResponse>();
		if (!data.success)
		{
			uiToastManager.ShowToast("playerSetIdentity", "Failed to set identity: " + data.errorData.message);
		}
	}

	private void WebSocket_Event_OnPlayerJoinPartyResponse(Dictionary<string, object> message)
	{
		PlayerJoinPartyResponse data = ((InMessage)message["inMessage"]).GetData<PlayerJoinPartyResponse>();
		if (!data.success)
		{
			uiToastManager.ShowToast("playerJoinParty", "Failed to join party: " + data.errorData.message);
		}
	}

	private void WebSocket_Event_OnPlayerStartMatchmakingResponse(Dictionary<string, object> message)
	{
		PlayerStartMatchmakingResponse data = ((InMessage)message["inMessage"]).GetData<PlayerStartMatchmakingResponse>();
		if (!data.success)
		{
			uiToastManager.ShowToast("playerStartMatchmaking", "Failed to start matchmaking: " + data.errorData.message);
		}
	}
}
