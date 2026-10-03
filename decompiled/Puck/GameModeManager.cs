using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameModeManager : NetworkBehaviourSingleton<GameModeManager>
{
	private static readonly Logger Logger = new Logger("GameModeManager");

	[Header("References")]
	[SerializeField]
	private ServerManager serverManager;

	[SerializeField]
	private GameManager gameManager;

	[SerializeField]
	private PlayerManager playerManager;

	[SerializeField]
	private PuckManager puckManager;

	[SerializeField]
	private ChatManager chatManager;

	[SerializeField]
	private ReplayManager replayManager;

	[SerializeField]
	private VoteManager voteManager;

	[HideInInspector]
	public Level Level;

	private Dictionary<string, IGameMode> gameModeMap = new Dictionary<string, IGameMode>();

	private IGameMode selectedGameMode;

	[HideInInspector]
	public NetworkVariable<GameModeClientConfig> ClientConfig;

	protected override void OnNetworkPreSpawn(ref NetworkManager networkManager)
	{
		if (ClientConfig == null)
		{
			ClientConfig = new NetworkVariable<GameModeClientConfig>();
		}
		if (networkManager.IsServer)
		{
			ClientConfig.Value = default;
		}
		base.OnNetworkPreSpawn(ref networkManager);
	}

	public override void OnNetworkSpawn()
	{
		NetworkVariable<GameModeClientConfig> clientConfig = ClientConfig;
		clientConfig.OnValueChanged = (NetworkVariable<GameModeClientConfig>.OnValueChangedDelegate)Delegate.Combine(clientConfig.OnValueChanged, new NetworkVariable<GameModeClientConfig>.OnValueChangedDelegate(OnClientConfigChanged));
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
		NetworkVariable<GameModeClientConfig> clientConfig = ClientConfig;
		clientConfig.OnValueChanged = (NetworkVariable<GameModeClientConfig>.OnValueChangedDelegate)Delegate.Remove(clientConfig.OnValueChanged, new NetworkVariable<GameModeClientConfig>.OnValueChangedDelegate(OnClientConfigChanged));
		base.OnNetworkDespawn();
	}

	private void ProcessInitialNetworkVariableValues()
	{
		OnClientConfigChanged(default, ClientConfig.Value);
	}

	private void OnClientConfigChanged(GameModeClientConfig oldClientConfig, GameModeClientConfig newClientConfig)
	{
		EventManager.TriggerEvent("Event_Everyone_OnGameModeClientConfigChanged", new Dictionary<string, object>
		{
			{ "oldClientConfig", oldClientConfig },
			{ "newClientConfig", newClientConfig }
		});
	}

	public override void Awake()
	{
		base.Awake();
		RegisterGameModes();
	}

	private void RegisterGameModes()
	{
		PublicGameMode<PublicGameModeConfig> value = new PublicGameMode<PublicGameModeConfig>("./public_game_mode_config.json", "--publicGameModeConfigPath", "--publicGameModeConfig", "PUCK_PUBLIC_GAME_MODE_CONFIG");
		CompetitiveGameMode<CompetitiveGameModeConfig> value2 = new CompetitiveGameMode<CompetitiveGameModeConfig>("./competitive_game_mode_config.json", "--competitiveGameModeConfigPath", "--competitiveGameModeConfig", "PUCK_COMPETITIVE_GAME_MODE_CONFIG");
		gameModeMap.Add("public", value);
		gameModeMap.Add("competitive", value2);
	}

	public void SelectGameMode(string name)
	{
		if (gameModeMap.ContainsKey(name))
		{
			selectedGameMode = gameModeMap[name];
			Logger.Info("Selected game mode " + Utils.GetReadableTypeName(selectedGameMode.GetType()));
		}
	}

	public void DeselectGameMode()
	{
		selectedGameMode = null;
		Logger.Info("Deselected game mode");
	}

	public void EnableSelectedGameMode()
	{
		if (selectedGameMode != null && !selectedGameMode.IsInitialized)
		{
			Logger.Info("Enabling game mode " + Utils.GetReadableTypeName(selectedGameMode.GetType()));
			selectedGameMode.Initialize(Level, serverManager, gameManager, playerManager, puckManager, chatManager, replayManager, voteManager);
		}
	}

	public void DisableSelectedGameMode()
	{
		if (selectedGameMode != null && selectedGameMode.IsInitialized)
		{
			Logger.Info("Disabling game mode " + Utils.GetReadableTypeName(selectedGameMode.GetType()));
			selectedGameMode.Dispose();
		}
	}

	protected override void __initializeVariables()
	{
		if (ClientConfig == null)
		{
			throw new Exception("GameModeManager.ClientConfig cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		ClientConfig.Initialize(this);
		__nameNetworkVariable(ClientConfig, "ClientConfig");
		NetworkVariableFields.Add(ClientConfig);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected override string __getTypeName()
	{
		return "GameModeManager";
	}
}
