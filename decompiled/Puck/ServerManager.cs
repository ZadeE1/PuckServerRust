using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

[RequireComponent(typeof(DeploymentManager))]
[RequireComponent(typeof(ConnectionApprovalManager))]
[RequireComponent(typeof(TimeoutManager))]
[RequireComponent(typeof(BanManager))]
[RequireComponent(typeof(AdminManager))]
public class ServerManager : NetworkBehaviourSingleton<ServerManager>
{
	private static readonly Logger Logger = new Logger("ServerManager");

	[HideInInspector]
	public UnityTransport UnityTransport;

	[HideInInspector]
	public DeploymentManager DeploymentManager;

	[HideInInspector]
	public ConnectionApprovalManager ConnectionApprovalManager;

	[HideInInspector]
	public TimeoutManager TimeoutManager;

	[HideInInspector]
	public BanManager BanManager;

	[HideInInspector]
	public AdminManager AdminManager;

	[HideInInspector]
	public WhitelistManager WhitelistManager;

	[HideInInspector]
	public NetworkVariable<Server> Server;

	[HideInInspector]
	public bool IsHostStartInProgress;

	[HideInInspector]
	public TCPServer TcpServer;

	[HideInInspector]
	public string IpAddress;

	[HideInInspector]
	public ServerConfig ServerConfig;

	public ushort PublicPort
	{
		get
		{
			if (!DeploymentManager.IsDeployment)
			{
				return ServerConfig.port;
			}
			return DeploymentManager.DeploymentPort;
		}
	}

	public ushort BindPort
	{
		get
		{
			if (!DeploymentManager.BindsDeploymentPort)
			{
				return ServerConfig.port;
			}
			return DeploymentManager.DeploymentPort;
		}
	}

	public override void Awake()
	{
		base.Awake();
		if (ApplicationManager.IsDedicatedGameServer)
		{
			LoadConfig("./server_config.json", "--serverConfigPath", "--serverConfig", "PUCK_SERVER_CONFIG");
		}
		DeploymentManager = GetComponent<DeploymentManager>();
		ConnectionApprovalManager = GetComponent<ConnectionApprovalManager>();
		TimeoutManager = GetComponent<TimeoutManager>();
		BanManager = GetComponent<BanManager>();
		AdminManager = GetComponent<AdminManager>();
		WhitelistManager = GetComponent<WhitelistManager>();
		uPnPHelper.DebugMode = true;
		uPnPHelper.LogErrors = true;
	}

	private void Start()
	{
		UnityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
		NetworkManager.Singleton.OnServerStarted += Server_OnServerStarted;
		NetworkManager.Singleton.OnServerStopped += Server_OnServerStopped;
		NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
		NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
		NetworkManager.Singleton.OnTransportFailure += OnTransportFailure;
	}

	protected override void OnNetworkPreSpawn(ref NetworkManager networkManager)
	{
		if (Server == null)
		{
			Server = new NetworkVariable<Server>();
		}
		if (networkManager.IsServer)
		{
			Server.Value = default;
		}
		base.OnNetworkPreSpawn(ref networkManager);
	}

	public override void OnNetworkSpawn()
	{
		NetworkVariable<Server> server = Server;
		server.OnValueChanged = (NetworkVariable<Server>.OnValueChangedDelegate)Delegate.Combine(server.OnValueChanged, new NetworkVariable<Server>.OnValueChangedDelegate(OnServerChanged));
		base.OnNetworkSpawn();
	}

	protected override void OnNetworkPostSpawn()
	{
		if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsConnectedClient)
		{
			ProcessInitialNetworkVariableValues();
		}
		base.OnNetworkPostSpawn();
	}

	protected override void OnNetworkSessionSynchronized()
	{
		ProcessInitialNetworkVariableValues();
		base.OnNetworkSessionSynchronized();
	}

	public override void OnNetworkDespawn()
	{
		NetworkVariable<Server> server = Server;
		server.OnValueChanged = (NetworkVariable<Server>.OnValueChangedDelegate)Delegate.Remove(server.OnValueChanged, new NetworkVariable<Server>.OnValueChangedDelegate(OnServerChanged));
		base.OnNetworkDespawn();
	}

	public override void OnDestroy()
	{
		if (NetworkManager.Singleton != null)
		{
			NetworkManager.Singleton.OnServerStarted -= Server_OnServerStarted;
			NetworkManager.Singleton.OnServerStopped -= Server_OnServerStopped;
			NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
			NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
			NetworkManager.Singleton.OnTransportFailure -= OnTransportFailure;
		}
		uPnPHelper.CloseAll();
		Utils.PrintUPnPLogs();
	}

	private void ProcessInitialNetworkVariableValues()
	{
		OnServerChanged(default, Server.Value);
	}

	private void LoadConfig(string defaultFilePath, string filePathCliArgument = null, string cliArgument = null, string envVariable = null)
	{
		string environmentVariable = Environment.GetEnvironmentVariable(envVariable);
		string commandLineArgument = Utils.GetCommandLineArgument(cliArgument);
		if (!string.IsNullOrEmpty(environmentVariable))
		{
			Logger.Info("Deserializing server config from environment variable (" + envVariable + ")");
			ServerConfig = ConfigUtils.LoadConfigFromSerializedString<ServerConfig>(environmentVariable, "environment variable " + envVariable);
		}
		else if (!string.IsNullOrEmpty(commandLineArgument))
		{
			Logger.Info("Deserializing server config from CLI argument (" + cliArgument + ")");
			ServerConfig = ConfigUtils.LoadConfigFromSerializedString<ServerConfig>(commandLineArgument, "CLI argument " + cliArgument);
		}
		else
		{
			string text = Utils.GetCommandLineArgument(filePathCliArgument) ?? defaultFilePath;
			Logger.Info("Deserializing server config from file (" + text + ")");
			ServerConfig = ConfigUtils.LoadConfigFromFile<ServerConfig>(text);
		}
		if (ServerConfig == null)
		{
			Logger.Error("Server config is invalid (see the error above); refusing to start. Fix the config and restart.");
			Application.Quit(1);
		}
	}

	private bool StartListener(ushort port, bool forwardPorts = false)
	{
		if (NetworkManager.Singleton.IsListening)
		{
			return false;
		}
		Logger.Info($"Puck B{ApplicationManager.Version}");
		if (forwardPorts)
		{
			StartPortForwarding(port);
		}
		UnityTransport.SetConnectionData("0.0.0.0", port);
		Logger.Info(string.Format("Started UDP listener on {0}:{1}", "0.0.0.0", port));
		return true;
	}

	public void StartHost(ushort port, string name, int maxPlayers, string password, bool isPublic, bool useVoip, bool forwardPorts = false)
	{
		if (StartListener(port, forwardPorts))
		{
			ServerConfig = new ServerConfig
			{
				port = port,
				name = name,
				maxPlayers = maxPlayers,
				password = password,
				isPublic = isPublic,
				useVoip = useVoip
			};
			string s = JsonSerializer.Serialize(new ConnectionData
			{
				SteamId = BackendManager.PlayerState.PlayerData.steamId,
				Key = BackendManager.PlayerState.Key,
				Password = password,
				EnabledModIds = ModManager.EnabledMods.Select((Mod mod) => mod.Id).ToArray(),
				Handedness = SettingsManager.Handedness,
				FlagID = SettingsManager.FlagID,
				HeadgearIDBlueAttacker = SettingsManager.HeadgearIDBlueAttacker,
				HeadgearIDRedAttacker = SettingsManager.HeadgearIDRedAttacker,
				HeadgearIDBlueGoalie = SettingsManager.HeadgearIDBlueGoalie,
				HeadgearIDRedGoalie = SettingsManager.HeadgearIDRedGoalie,
				MustacheID = SettingsManager.MustacheID,
				BeardID = SettingsManager.BeardID,
				JerseyIDBlueAttacker = SettingsManager.JerseyIDBlueAttacker,
				JerseyIDRedAttacker = SettingsManager.JerseyIDRedAttacker,
				JerseyIDBlueGoalie = SettingsManager.JerseyIDBlueGoalie,
				JerseyIDRedGoalie = SettingsManager.JerseyIDRedGoalie,
				StickSkinIDBlueAttacker = SettingsManager.StickSkinIDBlueAttacker,
				StickSkinIDRedAttacker = SettingsManager.StickSkinIDRedAttacker,
				StickSkinIDBlueGoalie = SettingsManager.StickSkinIDBlueGoalie,
				StickSkinIDRedGoalie = SettingsManager.StickSkinIDRedGoalie,
				StickShaftTapeIDBlueAttacker = SettingsManager.StickShaftTapeIDBlueAttacker,
				StickShaftTapeIDRedAttacker = SettingsManager.StickShaftTapeIDRedAttacker,
				StickShaftTapeIDBlueGoalie = SettingsManager.StickShaftTapeIDBlueGoalie,
				StickShaftTapeIDRedGoalie = SettingsManager.StickShaftTapeIDRedGoalie,
				StickBladeTapeIDBlueAttacker = SettingsManager.StickBladeTapeIDBlueAttacker,
				StickBladeTapeIDRedAttacker = SettingsManager.StickBladeTapeIDRedAttacker,
				StickBladeTapeIDBlueGoalie = SettingsManager.StickBladeTapeIDBlueGoalie,
				StickBladeTapeIDRedGoalie = SettingsManager.StickBladeTapeIDRedGoalie
			});
			NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes(s);
			IsHostStartInProgress = true;
			Authenticate();
		}
	}

	public void StartServer(ushort port, bool forwardPorts = false)
	{
		if (StartListener(port, forwardPorts))
		{
			if (!NetworkManager.Singleton.StartServer())
			{
				HandleStartFailure();
			}
			else
			{
				Authenticate();
			}
		}
	}

	public void HandleStartFailure()
	{
		Logger.Error($"Failed to start server on port {ServerConfig.port} (is the port already in use?)");
		IsHostStartInProgress = false;
		StopPortForwarding();
		Unauthenticate();
		EventManager.TriggerEvent("Event_OnServerStartFailed", new Dictionary<string, object> { { "port", ServerConfig.port } });
	}

	public void StartPortForwarding(ushort port)
	{
		Logger.Info($"Starting uPnP port forwarding for TCP & UDP port {port}");
		uPnPHelper.Start(uPnPHelper.Protocol.UDP, port, 0, "Puck");
		Utils.PrintUPnPLogs();
		uPnPHelper.Start(uPnPHelper.Protocol.TCP, port, 0, "Puck");
		Utils.PrintUPnPLogs();
	}

	public void StopPortForwarding()
	{
		Logger.Info("Stopping uPnP port forwarding");
		uPnPHelper.CloseAll();
		Utils.PrintUPnPLogs();
	}

	public void Authenticate()
	{
		WebSocketManager.Emit("serverAuthenticateRequest", new Dictionary<string, object>
		{
			{ "port", PublicPort },
			{ "isPublic", ServerConfig.isPublic },
			{ "deploymentId", DeploymentManager.DeploymentId }
		}, "serverAuthenticateResponse");
	}

	public void Unauthenticate()
	{
		WebSocketManager.Emit("serverUnauthenticateRequest", null, "serverUnauthenticateResponse");
	}

	private void OnServerChanged(Server oldServer, Server newServer)
	{
		EventManager.TriggerEvent("Event_Everyone_OnServerChanged", new Dictionary<string, object>
		{
			{ "oldServer", oldServer },
			{ "newServer", newServer }
		});
	}

	public void StartTcpServer(ushort port)
	{
		TcpServer = new TCPServer(port);
		TcpServer.OnMessageReceived += (string ipPort, string message) =>
		{
			try
			{
				if (JsonSerializer.Deserialize<TCPServerMessage>(message).type == TCPServerMessageType.PreviewRequest)
				{
					JsonSerializer.Deserialize<TCPServerPreviewRequest>(message);
					string message2 = JsonSerializer.Serialize(new TCPServerPreviewResponse
					{
						name = ServerConfig.name,
						players = NetworkManager.Singleton.ConnectedClientsList.Count,
						maxPlayers = ServerConfig.maxPlayers,
						isPasswordProtected = !string.IsNullOrEmpty(ServerConfig.password),
						clientRequiredModIds = ServerConfig.ClientRequiredModIds
					});
					TcpServer.SendMessageAsync(ipPort, message2);
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Error parsing message from " + ipPort + ": " + ex.Message);
			}
		};
		TcpServer.StartAsync();
	}

	public void StopTcpServer()
	{
		TcpServer.StopAsync();
		TcpServer = null;
	}

	private void Server_OnServerStarted()
	{
		EventManager.TriggerEvent("Event_Server_OnServerStarted", new Dictionary<string, object> { { "serverConfig", ServerConfig } });
	}

	private void Server_OnServerStopped(bool wasHost)
	{
		EventManager.TriggerEvent("Event_Server_OnServerStopped", new Dictionary<string, object> { { "wasHost", wasHost } });
	}

	private void OnClientConnected(ulong clientId)
	{
		if (NetworkManager.Singleton.LocalClientId == clientId)
		{
			EventManager.TriggerEvent("Event_OnClientConnected");
		}
		EventManager.TriggerEvent("Event_Everyone_OnClientConnected", new Dictionary<string, object> { { "clientId", clientId } });
	}

	private void OnClientDisconnected(ulong clientId)
	{
		EventManager.TriggerEvent("Event_Everyone_OnClientDisconnected", new Dictionary<string, object> { { "clientId", clientId } });
		if (NetworkManager.Singleton.LocalClientId == clientId)
		{
			EventManager.TriggerEvent("Event_OnClientDisconnected");
		}
	}

	private void OnTransportFailure()
	{
		EventManager.TriggerEvent("Event_OnTransportFailure");
	}

	public void Server_KickPlayer(Player player, DisconnectionCode disconnectionCode = DisconnectionCode.Kicked, string message = null, bool applyTimeout = true)
	{
		if (!NetworkManager.Singleton.IsServer)
		{
			return;
		}
		string steamId = player.SteamId.Value.ToString();
		if (player.OwnerClientId == 0L)
		{
			NetworkManager.Singleton.Shutdown(discardMessageQueue: true);
			return;
		}
		if (applyTimeout)
		{
			TimeoutManager.AddSteamIdTimeout(steamId, 60f);
		}
		string reason = JsonSerializer.Serialize(new Disconnection
		{
			code = disconnectionCode,
			message = message
		});
		NetworkManager.Singleton.DisconnectClient(player.OwnerClientId, reason);
	}

	public void Server_BanPlayer(Player player)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			string steamId = player.SteamId.Value.ToString();
			Server_BanSteamId(steamId);
			Server_KickPlayer(player, DisconnectionCode.Banned, null, applyTimeout: false);
		}
	}

	public void Server_BanSteamId(string steamId)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			BanManager.AddBannedSteamId(steamId);
		}
	}

	public void Server_UnbanSteamId(string steamId)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			BanManager.RemoveBannedSteamId(steamId);
		}
	}

	public bool Server_IsSteamIdBanned(string steamId)
	{
		return BanManager.IsSteamIdBanned(steamId);
	}

	protected override void __initializeVariables()
	{
		if (Server == null)
		{
			throw new Exception("ServerManager.Server cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		Server.Initialize(this);
		__nameNetworkVariable(Server, "Server");
		NetworkVariableFields.Add(Server);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected override string __getTypeName()
	{
		return "ServerManager";
	}
}
