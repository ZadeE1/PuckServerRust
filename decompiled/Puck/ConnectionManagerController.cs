using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Unity.Netcode;
using UnityEngine;

public class ConnectionManagerController : MonoBehaviour
{
	private static readonly Logger Logger = new Logger("ConnectionManagerController");

	private ConnectionManager connectionManager;

	private void Awake()
	{
		connectionManager = GetComponent<ConnectionManager>();
		EventManager.AddEventListener("Event_Server_OnConnectionRejected", Event_Server_OnConnectionRejected);
		EventManager.AddEventListener("Event_OnClientStarted", Event_OnClientStarted);
		EventManager.AddEventListener("Event_OnClientStopped", Event_OnClientStopped);
		EventManager.AddEventListener("Event_OnClientConnected", Event_OnClientConnected);
		EventManager.AddEventListener("Event_OnMainMenuClickJoinServer", Event_OnMainMenuClickJoinServer);
		EventManager.AddEventListener("Event_OnPauseMenuClickDisconnect", Event_OnPauseMenuClickDisconnect);
		EventManager.AddEventListener("Event_OnDebugChanged", Event_OnDebugChanged);
		EventManager.AddEventListener("Event_OnGotLaunchCommandLine", Event_OnGotLaunchCommandLine);
		EventManager.AddEventListener("Event_OnGameRichPresenceJoinRequested", Event_OnGameRichPresenceJoinRequested);
		EventManager.AddEventListener("Event_OnServerBrowserClickEndPoint", Event_OnServerBrowserClickEndPoint);
		EventManager.AddEventListener("Event_OnDirectConnectClickConnect", Event_OnDirectConnectClickConnect);
		EventManager.AddEventListener("Event_OnMatchmakingMatchingClickConnect", Event_OnMatchmakingMatchingClickConnect);
		EventManager.AddEventListener("Event_OnReconnectionStateChanged", Event_OnReconnectionStateChanged);
	}

	private void Start()
	{
		bool networkProfilingMetrics = SettingsManager.Debug != DebugMode.Off && !ApplicationManager.IsDedicatedGameServer;
		NetworkManager.Singleton.NetworkConfig.NetworkProfilingMetrics = networkProfilingMetrics;
	}

	private void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_Server_OnConnectionRejected", Event_Server_OnConnectionRejected);
		EventManager.RemoveEventListener("Event_OnClientStarted", Event_OnClientStarted);
		EventManager.RemoveEventListener("Event_OnClientStopped", Event_OnClientStopped);
		EventManager.RemoveEventListener("Event_OnClientConnected", Event_OnClientConnected);
		EventManager.RemoveEventListener("Event_OnMainMenuClickJoinServer", Event_OnMainMenuClickJoinServer);
		EventManager.RemoveEventListener("Event_OnPauseMenuClickDisconnect", Event_OnPauseMenuClickDisconnect);
		EventManager.RemoveEventListener("Event_OnDebugChanged", Event_OnDebugChanged);
		EventManager.RemoveEventListener("Event_OnGotLaunchCommandLine", Event_OnGotLaunchCommandLine);
		EventManager.RemoveEventListener("Event_OnGameRichPresenceJoinRequested", Event_OnGameRichPresenceJoinRequested);
		EventManager.RemoveEventListener("Event_OnServerBrowserClickEndPoint", Event_OnServerBrowserClickEndPoint);
		EventManager.RemoveEventListener("Event_OnDirectConnectClickConnect", Event_OnDirectConnectClickConnect);
		EventManager.RemoveEventListener("Event_OnMatchmakingMatchingClickConnect", Event_OnMatchmakingMatchingClickConnect);
		EventManager.RemoveEventListener("Event_OnReconnectionStateChanged", Event_OnReconnectionStateChanged);
	}

	private void HandleConnectionRejection(string reason)
	{
		ConnectionRejection connectionRejection;
		try
		{
			connectionRejection = JsonSerializer.Deserialize<ConnectionRejection>(reason);
		}
		catch
		{
			connectionRejection = new ConnectionRejection
			{
				code = ConnectionRejectionCode.Unreachable
			};
		}
		GlobalStateManager.SetConnectionState(new Dictionary<string, object>
		{
			{ "connection", null },
			{
				"lastConnection",
				GlobalStateManager.ConnectionState.Connection
			},
			{ "connectionRejection", connectionRejection },
			{ "disconnection", null },
			{
				"phase",
				ConnectionPhase.Disconnected
			}
		});
		switch (connectionRejection.code)
		{
		case ConnectionRejectionCode.MissingPassword:
		case ConnectionRejectionCode.InvalidPassword:
			GlobalStateManager.SetReconnectionState(new Dictionary<string, object>
			{
				{
					"phase",
					ReconnectionPhase.AwaitingPassword
				},
				{ "password", null }
			});
			break;
		case ConnectionRejectionCode.MissingMods:
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("phase", ReconnectionPhase.AwaitingMods);
			dictionary.Add("clientRequiredModIds", connectionRejection.data.clientRequiredModIds);
			dictionary.Add("pendingReadinessModIds", new string[0]);
			dictionary.Add("pendingEnablingModIds", new string[0]);
			GlobalStateManager.SetReconnectionState(dictionary);
			break;
		}
		}
		EventManager.TriggerEvent("Event_OnConnectionRejected", new Dictionary<string, object> { { "connectionRejection", connectionRejection } });
	}

	private void HandleDisconnection(string reason)
	{
		Disconnection value;
		try
		{
			value = JsonSerializer.Deserialize<Disconnection>(reason);
		}
		catch
		{
			value = ((NetworkManager.Singleton.DisconnectEvent != NetworkTransport.DisconnectEvents.TransportShutdown) ? new Disconnection
			{
				code = DisconnectionCode.ConnectionLost
			} : new Disconnection
			{
				code = DisconnectionCode.Disconnected
			});
		}
		GlobalStateManager.SetConnectionState(new Dictionary<string, object>
		{
			{ "connection", null },
			{
				"lastConnection",
				GlobalStateManager.ConnectionState.Connection
			},
			{ "connectionRejection", null },
			{ "disconnection", value },
			{
				"phase",
				ConnectionPhase.Disconnected
			}
		});
		EventManager.TriggerEvent("Event_OnDisconnected", new Dictionary<string, object> { { "disconnection", value } });
	}

	private void Event_Server_OnConnectionRejected(Dictionary<string, object> message)
	{
		ConnectionApproval connectionApproval = (ConnectionApproval)message["connectionApproval"];
		if (connectionApproval.IsHost)
		{
			connectionManager.Client_Disconnect();
			HandleConnectionRejection(connectionApproval.Response.Reason);
		}
	}

	private void Event_OnClientStarted(Dictionary<string, object> message)
	{
		GlobalStateManager.SetConnectionState(new Dictionary<string, object> { 
		{
			"phase",
			ConnectionPhase.Connecting
		} });
	}

	private void Event_OnClientStopped(Dictionary<string, object> message)
	{
		if (GlobalStateManager.ConnectionState.Phase == ConnectionPhase.Connecting)
		{
			HandleConnectionRejection(NetworkManager.Singleton.DisconnectReason);
		}
		else
		{
			HandleDisconnection(NetworkManager.Singleton.DisconnectReason);
		}
		Connection pendingConnection = GlobalStateManager.ConnectionState.PendingConnection;
		if (pendingConnection != null)
		{
			connectionManager.Client_StartClient(pendingConnection.EndPoint.ipAddress, pendingConnection.EndPoint.port, pendingConnection.Password);
		}
	}

	private void Event_OnClientConnected(Dictionary<string, object> message)
	{
		GlobalStateManager.SetConnectionState(new Dictionary<string, object> { 
		{
			"phase",
			ConnectionPhase.Connected
		} });
	}

	private void Event_OnMainMenuClickJoinServer(Dictionary<string, object> message)
	{
		string ipAddress = (string)message["ipAddress"];
		ushort port = (ushort)message["port"];
		string password = (string)message["password"];
		connectionManager.Client_StartClient(ipAddress, port, password);
	}

	private void Event_OnPauseMenuClickDisconnect(Dictionary<string, object> message)
	{
		connectionManager.Client_Disconnect();
	}

	private void Event_OnDebugChanged(Dictionary<string, object> message)
	{
		bool networkProfilingMetrics = (DebugMode)message["value"] != DebugMode.Off && !ApplicationManager.IsDedicatedGameServer;
		NetworkManager.Singleton.NetworkConfig.NetworkProfilingMetrics = networkProfilingMetrics;
	}

	private void Event_OnGotLaunchCommandLine(Dictionary<string, object> message)
	{
		string[] args = (string[])message["args"];
		string commandLineArgument = Utils.GetCommandLineArgument("+ipAddress", args);
		ushort port = (ushort)(ushort.TryParse(Utils.GetCommandLineArgument("+port", args), out var result) ? result : 30609);
		string commandLineArgument2 = Utils.GetCommandLineArgument("+password", args);
		if (!string.IsNullOrEmpty(commandLineArgument))
		{
			connectionManager.Client_StartClient(commandLineArgument, port, commandLineArgument2);
		}
	}

	private void Event_OnGameRichPresenceJoinRequested(Dictionary<string, object> message)
	{
		string[] args = (string[])message["args"];
		string commandLineArgument = Utils.GetCommandLineArgument("+ipAddress", args);
		ushort port = (ushort)(ushort.TryParse(Utils.GetCommandLineArgument("+port", args), out var result) ? result : 30609);
		string commandLineArgument2 = Utils.GetCommandLineArgument("+password", args);
		if (!string.IsNullOrEmpty(commandLineArgument))
		{
			connectionManager.Client_StartClient(commandLineArgument, port, commandLineArgument2);
		}
	}

	private void Event_OnServerBrowserClickEndPoint(Dictionary<string, object> message)
	{
		EndPoint endPoint = (EndPoint)message["endPoint"];
		connectionManager.Client_StartClient(endPoint.ipAddress, endPoint.port);
	}

	private void Event_OnDirectConnectClickConnect(Dictionary<string, object> message)
	{
		string ipAddress = (string)message["ipAddress"];
		ushort port = (ushort)(int)message["port"];
		connectionManager.Client_StartClient(ipAddress, port);
	}

	private void Event_OnMatchmakingMatchingClickConnect(Dictionary<string, object> message)
	{
		EndPoint endPoint = BackendManager.PlayerState.MatchData.endPoint;
		if (!(endPoint == null))
		{
			connectionManager.Client_StartClient(endPoint.ipAddress, endPoint.port);
		}
	}

	private void Event_OnReconnectionStateChanged(Dictionary<string, object> message)
	{
		ReconnectionState reconnectionState = (ReconnectionState)message["newReconnectionState"];
		ReconnectionState reconnectionState2 = (ReconnectionState)message["oldReconnectionState"];
		switch (reconnectionState.Phase)
		{
		case ReconnectionPhase.AwaitingPassword:
			if (reconnectionState2.Password != reconnectionState.Password && reconnectionState.Password != null)
			{
				connectionManager.Client_StartClient(GlobalStateManager.ConnectionState.LastConnection.EndPoint.ipAddress, GlobalStateManager.ConnectionState.LastConnection.EndPoint.port, reconnectionState.Password);
			}
			break;
		case ReconnectionPhase.AwaitingMods:
		{
			bool flag = !reconnectionState2.PendingEnablingModIds.SequenceEqual(reconnectionState.PendingEnablingModIds);
			bool flag2 = !reconnectionState2.PendingReadinessModIds.SequenceEqual(reconnectionState.PendingReadinessModIds);
			if ((flag | flag2) && reconnectionState.PendingModIds.Length == 0)
			{
				connectionManager.Client_StartClient(GlobalStateManager.ConnectionState.LastConnection.EndPoint.ipAddress, GlobalStateManager.ConnectionState.LastConnection.EndPoint.port, GlobalStateManager.ConnectionState.LastConnection.Password);
			}
			break;
		}
		}
	}
}
