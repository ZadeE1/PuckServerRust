using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ServerManagerController : MonoBehaviour
{
	private static readonly Logger Logger = new Logger("ServerManagerController");

	private ServerManager serverManager;

	private void Awake()
	{
		serverManager = GetComponent<ServerManager>();
		EventManager.AddEventListener("Event_Everyone_OnClientConnected", Event_Everyone_OnClientConnected);
		EventManager.AddEventListener("Event_Everyone_OnClientDisconnected", Event_Everyone_OnClientDisconnected);
		EventManager.AddEventListener("Event_Server_OnServerStarted", Event_Server_OnServerStarted);
		EventManager.AddEventListener("Event_Server_OnServerStopped", Event_Server_OnServerStopped);
		EventManager.AddEventListener("Event_Server_OnNetworkDiagnostics", Event_Server_OnNetworkDiagnostics);
		EventManager.AddEventListener("Event_Server_OnObjectSynchronizationDiagnostics", Event_Server_OnObjectSynchronizationDiagnostics);
		EventManager.AddEventListener("Event_OnServerStateChanged", Event_OnServerStateChanged);
		EventManager.AddEventListener("Event_OnTransportFailure", Event_OnTransportFailure);
		EventManager.AddEventListener("Event_OnMainMenuClickHostServer", Event_OnMainMenuClickHostServer);
		EventManager.AddEventListener("Event_OnNewServerClickStart", Event_OnNewServerClickStart);
		EventManager.AddEventListener("Event_OnPlayClickPractice", Event_OnPlayClickPractice);
		WebSocketManager.AddMessageListener("connected", WebSocket_Event_OnConnected);
		WebSocketManager.AddMessageListener("serverKickPlayer", WebSocket_Event_OnServerKickPlayer);
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnClientConnected", Event_Everyone_OnClientConnected);
		EventManager.RemoveEventListener("Event_Everyone_OnClientDisconnected", Event_Everyone_OnClientDisconnected);
		EventManager.RemoveEventListener("Event_Server_OnServerStarted", Event_Server_OnServerStarted);
		EventManager.RemoveEventListener("Event_Server_OnServerStopped", Event_Server_OnServerStopped);
		EventManager.RemoveEventListener("Event_Server_OnNetworkDiagnostics", Event_Server_OnNetworkDiagnostics);
		EventManager.RemoveEventListener("Event_Server_OnObjectSynchronizationDiagnostics", Event_Server_OnObjectSynchronizationDiagnostics);
		EventManager.RemoveEventListener("Event_OnServerStateChanged", Event_OnServerStateChanged);
		EventManager.RemoveEventListener("Event_OnTransportFailure", Event_OnTransportFailure);
		EventManager.RemoveEventListener("Event_OnMainMenuClickHostServer", Event_OnMainMenuClickHostServer);
		EventManager.RemoveEventListener("Event_OnNewServerClickStart", Event_OnNewServerClickStart);
		EventManager.RemoveEventListener("Event_OnPlayClickPractice", Event_OnPlayClickPractice);
		WebSocketManager.RemoveMessageListener("connected", WebSocket_Event_OnConnected);
		WebSocketManager.RemoveMessageListener("serverKickPlayer", WebSocket_Event_OnServerKickPlayer);
	}

	private void Event_Everyone_OnClientConnected(Dictionary<string, object> message)
	{
		ulong num = (ulong)message["clientId"];
		if (NetworkManager.Singleton.IsServer)
		{
			Logger.Info($"Client connected ({num}) {NetworkManager.Singleton.ConnectedClientsList.Count}/{serverManager.ServerConfig.maxPlayers}");
		}
	}

	private void Event_Everyone_OnClientDisconnected(Dictionary<string, object> message)
	{
		ulong num = (ulong)message["clientId"];
		if (NetworkManager.Singleton.IsServer)
		{
			Logger.Info($"Client disconnected ({num}) {NetworkManager.Singleton.ConnectedClientsList.Count}/{serverManager.ServerConfig.maxPlayers}");
		}
	}

	private void Event_OnServerStateChanged(Dictionary<string, object> message)
	{
		ServerState serverState = (ServerState)message["oldServerState"];
		ServerState serverState2 = (ServerState)message["newServerState"];
		if (serverState.AuthenticationPhase == AuthenticationPhase.None && serverState2.AuthenticationPhase == AuthenticationPhase.Authenticated && serverManager.IsHostStartInProgress)
		{
			serverManager.IsHostStartInProgress = false;
			if (!NetworkManager.Singleton.StartHost())
			{
				serverManager.HandleStartFailure();
			}
		}
	}

	private void Event_OnTransportFailure(Dictionary<string, object> message)
	{
		if (serverManager.IsHostStartInProgress)
		{
			serverManager.IsHostStartInProgress = false;
		}
	}

	private void Event_Server_OnServerStarted(Dictionary<string, object> message)
	{
		serverManager.Server.Value = new Server
		{
			IpAddress = serverManager.IpAddress,
			Port = serverManager.PublicPort,
			Name = serverManager.ServerConfig.name,
			MaxPlayers = serverManager.ServerConfig.maxPlayers,
			TickRate = serverManager.ServerConfig.tickRate,
			UseVoip = serverManager.ServerConfig.useVoip,
			GameMode = serverManager.ServerConfig.gameMode
		};
		serverManager.StartTcpServer(serverManager.BindPort);
	}

	private void Event_Server_OnServerStopped(Dictionary<string, object> message)
	{
		serverManager.StopTcpServer();
		serverManager.StopPortForwarding();
		serverManager.Unauthenticate();
	}

	private void Event_Server_OnNetworkDiagnostics(Dictionary<string, object> message)
	{
		NetworkDiagnostics networkDiagnostics = (NetworkDiagnostics)message["networkDiagnostics"];
		Logger.Info(NetworkDiagnostics.DescribeHeader());
		Logger.Info(networkDiagnostics.Describe());
	}

	private void Event_Server_OnObjectSynchronizationDiagnostics(Dictionary<string, object> message)
	{
		SynchronizedObjectServerDiagnosticsData synchronizedObjectServerDiagnosticsData = (SynchronizedObjectServerDiagnosticsData)message["diagnosticsData"];
		Logger.Info(synchronizedObjectServerDiagnosticsData.DescribeHeader());
		SynchronizedObjectPlayerDiagnosticsData[] players = synchronizedObjectServerDiagnosticsData.Players;
		foreach (SynchronizedObjectPlayerDiagnosticsData synchronizedObjectPlayerDiagnosticsData in players)
		{
			Logger.Info(synchronizedObjectPlayerDiagnosticsData.Describe());
		}
	}

	private void Event_OnMainMenuClickHostServer(Dictionary<string, object> message)
	{
		ushort port = (ushort)message["port"];
		string password = (string)message["password"];
		serverManager.StartHost(port, "MY PUCK SERVER", 12, password, isPublic: true, useVoip: true, forwardPorts: true);
	}

	private void Event_OnNewServerClickStart(Dictionary<string, object> message)
	{
		if (!((string)message["type"] != "selfHosted"))
		{
			int num = (int)message["port"];
			string text = (string)message["name"];
			int maxPlayers = (int)message["maxPlayers"];
			string password = (string)message["password"];
			bool useVoip = (bool)message["useVoip"];
			serverManager.StartHost((ushort)num, text, maxPlayers, password, isPublic: true, useVoip, forwardPorts: true);
		}
	}

	private void Event_OnPlayClickPractice(Dictionary<string, object> message)
	{
		serverManager.StartHost(30609, "PRACTICE", 1, null, isPublic: false, useVoip: false);
	}

	private void WebSocket_Event_OnConnected(Dictionary<string, object> message)
	{
		if (ApplicationManager.IsDedicatedGameServer)
		{
			if (NetworkManager.Singleton.IsServer)
			{
				serverManager.Authenticate();
			}
			else
			{
				serverManager.StartServer(serverManager.BindPort, forwardPorts: true);
			}
		}
	}

	private void WebSocket_Event_OnServerKickPlayer(Dictionary<string, object> message)
	{
		ServerKickPlayerMessage data = ((InMessage)message["inMessage"]).GetData<ServerKickPlayerMessage>();
		Player playerBySteamId = MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerBySteamId(data.steamId);
		if ((bool)playerBySteamId)
		{
			serverManager.Server_KickPlayer(playerBySteamId, DisconnectionCode.Kicked, null, applyTimeout: false);
		}
	}
}
