using System;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
	private static readonly Logger Logger = new Logger("Player");

	[Header("Settings")]
	[SerializeField]
	private int maxChatTickets = 3;

	[SerializeField]
	public float ChatTicketsPerSecond = 3f;

	[Header("Prefabs")]
	[SerializeField]
	private PlayerCamera playerCameraPrefab;

	[SerializeField]
	private PlayerBody playerBodyAttackerPrefab;

	[SerializeField]
	private PlayerBody playerBodyGoaliePrefab;

	[SerializeField]
	private StickPositioner stickPositionerPrefab;

	[SerializeField]
	private SpectatorCamera spectatorCameraPrefab;

	[SerializeField]
	private Stick stickAttackerPrefab;

	[SerializeField]
	private Stick stickGoaliePrefab;

	[HideInInspector]
	public NetworkVariable<PlayerGameState> GameState;

	[HideInInspector]
	public NetworkVariable<PlayerCustomizationState> CustomizationState;

	[HideInInspector]
	public NetworkVariable<PlayerHandedness> Handedness;

	[HideInInspector]
	public NetworkVariable<FixedString32Bytes> SteamId;

	[HideInInspector]
	public NetworkVariable<FixedString32Bytes> Username;

	[HideInInspector]
	public NetworkVariable<int> Number;

	[HideInInspector]
	public NetworkVariable<int> PatreonLevel;

	[HideInInspector]
	public NetworkVariable<int> AdminLevel;

	[HideInInspector]
	public NetworkVariable<int> Goals;

	[HideInInspector]
	public NetworkVariable<int> Assists;

	[HideInInspector]
	public NetworkVariable<ulong> Ping;

	[HideInInspector]
	public NetworkVariable<NetworkObjectReference> PlayerPositionReference;

	[HideInInspector]
	public NetworkVariable<bool> IsMuted;

	[HideInInspector]
	public NetworkVariable<bool> IsReplay;

	[HideInInspector]
	public PlayerInput PlayerInput;

	[HideInInspector]
	public SpectatorCamera SpectatorCamera;

	[HideInInspector]
	public PlayerCamera PlayerCamera;

	[HideInInspector]
	public PlayerBody PlayerBody;

	[HideInInspector]
	public StickPositioner StickPositioner;

	[HideInInspector]
	public Stick Stick;

	[HideInInspector]
	public PlayerPosition PlayerPosition;

	private bool isNetworkVariablesInitialized;

	private Tween delayedGameStateTween;

	[HideInInspector]
	public float ChatTickets { get; private set; }

	[HideInInspector]
	public bool IsChatAvailable => ChatTickets >= 1f;

	[HideInInspector]
	public PlayerPhase Phase => GameState.Value.Phase;

	[HideInInspector]
	public PlayerTeam Team => GameState.Value.Team;

	[HideInInspector]
	public PlayerRole Role => GameState.Value.Role;

	[HideInInspector]
	public int FlagID => CustomizationState.Value.FlagID;

	[HideInInspector]
	public int HeadgearIDBlueAttacker => CustomizationState.Value.HeadgearIDBlueAttacker;

	[HideInInspector]
	public int HeadgearIDRedAttacker => CustomizationState.Value.HeadgearIDRedAttacker;

	[HideInInspector]
	public int HeadgearIDBlueGoalie => CustomizationState.Value.HeadgearIDBlueGoalie;

	[HideInInspector]
	public int HeadgearIDRedGoalie => CustomizationState.Value.HeadgearIDRedGoalie;

	[HideInInspector]
	public int MustacheID => CustomizationState.Value.MustacheID;

	[HideInInspector]
	public int BeardID => CustomizationState.Value.BeardID;

	[HideInInspector]
	public int JerseyIDBlueAttacker => CustomizationState.Value.JerseyIDBlueAttacker;

	[HideInInspector]
	public int JerseyIDRedAttacker => CustomizationState.Value.JerseyIDRedAttacker;

	[HideInInspector]
	public int JerseyIDBlueGoalie => CustomizationState.Value.JerseyIDBlueGoalie;

	[HideInInspector]
	public int JerseyIDRedGoalie => CustomizationState.Value.JerseyIDRedGoalie;

	[HideInInspector]
	public int StickSkinIDBlueAttacker => CustomizationState.Value.StickSkinIDBlueAttacker;

	[HideInInspector]
	public int StickSkinIDRedAttacker => CustomizationState.Value.StickSkinIDRedAttacker;

	[HideInInspector]
	public int StickSkinIDBlueGoalie => CustomizationState.Value.StickSkinIDBlueGoalie;

	[HideInInspector]
	public int StickSkinIDRedGoalie => CustomizationState.Value.StickSkinIDRedGoalie;

	[HideInInspector]
	public int StickShaftTapeIDBlueAttacker => CustomizationState.Value.StickShaftTapeIDBlueAttacker;

	[HideInInspector]
	public int StickShaftTapeIDRedAttacker => CustomizationState.Value.StickShaftTapeIDRedAttacker;

	[HideInInspector]
	public int StickShaftTapeIDBlueGoalie => CustomizationState.Value.StickShaftTapeIDBlueGoalie;

	[HideInInspector]
	public int StickShaftTapeIDRedGoalie => CustomizationState.Value.StickShaftTapeIDRedGoalie;

	[HideInInspector]
	public int StickBladeTapeIDBlueAttacker => CustomizationState.Value.StickBladeTapeIDBlueAttacker;

	[HideInInspector]
	public int StickBladeTapeIDRedAttacker => CustomizationState.Value.StickBladeTapeIDRedAttacker;

	[HideInInspector]
	public int StickBladeTapeIDBlueGoalie => CustomizationState.Value.StickBladeTapeIDBlueGoalie;

	[HideInInspector]
	public int StickBladeTapeIDRedGoalie => CustomizationState.Value.StickBladeTapeIDRedGoalie;

	[HideInInspector]
	public bool IsCharacterSpawned
	{
		get
		{
			if ((bool)PlayerCamera && (bool)PlayerBody && (bool)StickPositioner)
			{
				return Stick;
			}
			return false;
		}
	}

	[HideInInspector]
	public bool IsSpectatorCameraSpawned => SpectatorCamera != null;

	private void Awake()
	{
		PlayerInput = GetComponent<PlayerInput>();
		ChatTickets = maxChatTickets;
	}

	protected override void OnNetworkPreSpawn(ref NetworkManager networkManager)
	{
		InitializeNetworkVariables(default, default, PlayerHandedness.None, default, default, 0, 0, 0, 0, 0, 0uL);
		base.OnNetworkPreSpawn(ref networkManager);
	}

	public override void OnNetworkSpawn()
	{
		NetworkVariable<PlayerGameState> gameState = GameState;
		gameState.OnValueChanged = (NetworkVariable<PlayerGameState>.OnValueChangedDelegate)Delegate.Combine(gameState.OnValueChanged, new NetworkVariable<PlayerGameState>.OnValueChangedDelegate(OnPlayerGameStateChanged));
		NetworkVariable<PlayerCustomizationState> customizationState = CustomizationState;
		customizationState.OnValueChanged = (NetworkVariable<PlayerCustomizationState>.OnValueChangedDelegate)Delegate.Combine(customizationState.OnValueChanged, new NetworkVariable<PlayerCustomizationState>.OnValueChangedDelegate(OnPlayerCustomizationStateChanged));
		NetworkVariable<PlayerHandedness> handedness = Handedness;
		handedness.OnValueChanged = (NetworkVariable<PlayerHandedness>.OnValueChangedDelegate)Delegate.Combine(handedness.OnValueChanged, new NetworkVariable<PlayerHandedness>.OnValueChangedDelegate(OnPlayerHandednessChanged));
		NetworkVariable<FixedString32Bytes> steamId = SteamId;
		steamId.OnValueChanged = (NetworkVariable<FixedString32Bytes>.OnValueChangedDelegate)Delegate.Combine(steamId.OnValueChanged, new NetworkVariable<FixedString32Bytes>.OnValueChangedDelegate(OnPlayerSteamIdChanged));
		NetworkVariable<FixedString32Bytes> username = Username;
		username.OnValueChanged = (NetworkVariable<FixedString32Bytes>.OnValueChangedDelegate)Delegate.Combine(username.OnValueChanged, new NetworkVariable<FixedString32Bytes>.OnValueChangedDelegate(OnPlayerUsernameChanged));
		NetworkVariable<int> number = Number;
		number.OnValueChanged = (NetworkVariable<int>.OnValueChangedDelegate)Delegate.Combine(number.OnValueChanged, new NetworkVariable<int>.OnValueChangedDelegate(OnPlayerNumberChanged));
		NetworkVariable<int> patreonLevel = PatreonLevel;
		patreonLevel.OnValueChanged = (NetworkVariable<int>.OnValueChangedDelegate)Delegate.Combine(patreonLevel.OnValueChanged, new NetworkVariable<int>.OnValueChangedDelegate(OnPlayerPatreonLevelChanged));
		NetworkVariable<int> adminLevel = AdminLevel;
		adminLevel.OnValueChanged = (NetworkVariable<int>.OnValueChangedDelegate)Delegate.Combine(adminLevel.OnValueChanged, new NetworkVariable<int>.OnValueChangedDelegate(OnPlayerAdminLevelChanged));
		NetworkVariable<int> goals = Goals;
		goals.OnValueChanged = (NetworkVariable<int>.OnValueChangedDelegate)Delegate.Combine(goals.OnValueChanged, new NetworkVariable<int>.OnValueChangedDelegate(OnPlayerGoalsChanged));
		NetworkVariable<int> assists = Assists;
		assists.OnValueChanged = (NetworkVariable<int>.OnValueChangedDelegate)Delegate.Combine(assists.OnValueChanged, new NetworkVariable<int>.OnValueChangedDelegate(OnPlayerAssistsChanged));
		NetworkVariable<ulong> ping = Ping;
		ping.OnValueChanged = (NetworkVariable<ulong>.OnValueChangedDelegate)Delegate.Combine(ping.OnValueChanged, new NetworkVariable<ulong>.OnValueChangedDelegate(OnPlayerPingChanged));
		NetworkVariable<NetworkObjectReference> playerPositionReference = PlayerPositionReference;
		playerPositionReference.OnValueChanged = (NetworkVariable<NetworkObjectReference>.OnValueChangedDelegate)Delegate.Combine(playerPositionReference.OnValueChanged, new NetworkVariable<NetworkObjectReference>.OnValueChangedDelegate(OnPlayerPlayerPositionReferenceChanged));
		NetworkVariable<bool> isMuted = IsMuted;
		isMuted.OnValueChanged = (NetworkVariable<bool>.OnValueChangedDelegate)Delegate.Combine(isMuted.OnValueChanged, new NetworkVariable<bool>.OnValueChangedDelegate(OnPlayerIsMutedChanged));
		PlayerPosition = NetworkingUtils.GetPlayerPositionFromNetworkObjectReference(PlayerPositionReference.Value);
		base.OnNetworkSpawn();
	}

	protected override void OnNetworkPostSpawn()
	{
		EventManager.TriggerEvent("Event_Everyone_OnPlayerSpawned", new Dictionary<string, object> { { "player", this } });
		base.OnNetworkPostSpawn();
	}

	protected override void OnNetworkSessionSynchronized()
	{
		OnPlayerPlayerPositionReferenceChanged(default, PlayerPositionReference.Value);
		base.OnNetworkSessionSynchronized();
	}

	private void Update()
	{
		if (NetworkManager.Singleton.IsServer && ChatTickets < (float)maxChatTickets)
		{
			ChatTickets += ChatTicketsPerSecond * Time.deltaTime;
			if (ChatTickets > (float)maxChatTickets)
			{
				ChatTickets = maxChatTickets;
			}
		}
	}

	public override void OnNetworkDespawn()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Server_CancelDelayedGameState();
			if (IsCharacterSpawned)
			{
				Server_DespawnCharacter();
			}
			if (IsSpectatorCameraSpawned)
			{
				Server_DespawnSpectatorCamera();
			}
		}
		Logger.Info($"Despawning Player ({OwnerClientId})");
		EventManager.TriggerEvent("Event_Everyone_OnPlayerDespawned", new Dictionary<string, object> { { "player", this } });
		NetworkVariable<PlayerGameState> gameState = GameState;
		gameState.OnValueChanged = (NetworkVariable<PlayerGameState>.OnValueChangedDelegate)Delegate.Remove(gameState.OnValueChanged, new NetworkVariable<PlayerGameState>.OnValueChangedDelegate(OnPlayerGameStateChanged));
		NetworkVariable<PlayerCustomizationState> customizationState = CustomizationState;
		customizationState.OnValueChanged = (NetworkVariable<PlayerCustomizationState>.OnValueChangedDelegate)Delegate.Remove(customizationState.OnValueChanged, new NetworkVariable<PlayerCustomizationState>.OnValueChangedDelegate(OnPlayerCustomizationStateChanged));
		NetworkVariable<PlayerHandedness> handedness = Handedness;
		handedness.OnValueChanged = (NetworkVariable<PlayerHandedness>.OnValueChangedDelegate)Delegate.Remove(handedness.OnValueChanged, new NetworkVariable<PlayerHandedness>.OnValueChangedDelegate(OnPlayerHandednessChanged));
		NetworkVariable<FixedString32Bytes> steamId = SteamId;
		steamId.OnValueChanged = (NetworkVariable<FixedString32Bytes>.OnValueChangedDelegate)Delegate.Remove(steamId.OnValueChanged, new NetworkVariable<FixedString32Bytes>.OnValueChangedDelegate(OnPlayerSteamIdChanged));
		NetworkVariable<FixedString32Bytes> username = Username;
		username.OnValueChanged = (NetworkVariable<FixedString32Bytes>.OnValueChangedDelegate)Delegate.Remove(username.OnValueChanged, new NetworkVariable<FixedString32Bytes>.OnValueChangedDelegate(OnPlayerUsernameChanged));
		NetworkVariable<int> number = Number;
		number.OnValueChanged = (NetworkVariable<int>.OnValueChangedDelegate)Delegate.Remove(number.OnValueChanged, new NetworkVariable<int>.OnValueChangedDelegate(OnPlayerNumberChanged));
		NetworkVariable<int> patreonLevel = PatreonLevel;
		patreonLevel.OnValueChanged = (NetworkVariable<int>.OnValueChangedDelegate)Delegate.Remove(patreonLevel.OnValueChanged, new NetworkVariable<int>.OnValueChangedDelegate(OnPlayerPatreonLevelChanged));
		NetworkVariable<int> adminLevel = AdminLevel;
		adminLevel.OnValueChanged = (NetworkVariable<int>.OnValueChangedDelegate)Delegate.Remove(adminLevel.OnValueChanged, new NetworkVariable<int>.OnValueChangedDelegate(OnPlayerAdminLevelChanged));
		NetworkVariable<int> goals = Goals;
		goals.OnValueChanged = (NetworkVariable<int>.OnValueChangedDelegate)Delegate.Remove(goals.OnValueChanged, new NetworkVariable<int>.OnValueChangedDelegate(OnPlayerGoalsChanged));
		NetworkVariable<int> assists = Assists;
		assists.OnValueChanged = (NetworkVariable<int>.OnValueChangedDelegate)Delegate.Remove(assists.OnValueChanged, new NetworkVariable<int>.OnValueChangedDelegate(OnPlayerAssistsChanged));
		NetworkVariable<ulong> ping = Ping;
		ping.OnValueChanged = (NetworkVariable<ulong>.OnValueChangedDelegate)Delegate.Remove(ping.OnValueChanged, new NetworkVariable<ulong>.OnValueChangedDelegate(OnPlayerPingChanged));
		NetworkVariable<NetworkObjectReference> playerPositionReference = PlayerPositionReference;
		playerPositionReference.OnValueChanged = (NetworkVariable<NetworkObjectReference>.OnValueChangedDelegate)Delegate.Remove(playerPositionReference.OnValueChanged, new NetworkVariable<NetworkObjectReference>.OnValueChangedDelegate(OnPlayerPlayerPositionReferenceChanged));
		NetworkVariable<bool> isMuted = IsMuted;
		isMuted.OnValueChanged = (NetworkVariable<bool>.OnValueChangedDelegate)Delegate.Remove(isMuted.OnValueChanged, new NetworkVariable<bool>.OnValueChangedDelegate(OnPlayerIsMutedChanged));
		base.OnNetworkDespawn();
	}

	public void InitializeNetworkVariables(PlayerGameState gameState = default(PlayerGameState), PlayerCustomizationState customizationState = default(PlayerCustomizationState), PlayerHandedness handedness = PlayerHandedness.None, FixedString32Bytes steamID = default(FixedString32Bytes), FixedString32Bytes username = default(FixedString32Bytes), int number = 0, int patreonLevel = 0, int adminLevel = 0, int goals = 0, int assists = 0, ulong ping = 0uL, NetworkObjectReference playerPositionReference = default(NetworkObjectReference), bool isMuted = false, bool isReplay = false)
	{
		if (!isNetworkVariablesInitialized)
		{
			isNetworkVariablesInitialized = true;
			GameState = new NetworkVariable<PlayerGameState>(gameState);
			CustomizationState = new NetworkVariable<PlayerCustomizationState>(customizationState);
			Handedness = new NetworkVariable<PlayerHandedness>(handedness);
			SteamId = new NetworkVariable<FixedString32Bytes>(steamID);
			Username = new NetworkVariable<FixedString32Bytes>(username);
			Number = new NetworkVariable<int>(number);
			PatreonLevel = new NetworkVariable<int>(patreonLevel);
			AdminLevel = new NetworkVariable<int>(adminLevel);
			Goals = new NetworkVariable<int>(goals);
			Assists = new NetworkVariable<int>(assists);
			Ping = new NetworkVariable<ulong>(ping);
			PlayerPositionReference = new NetworkVariable<NetworkObjectReference>(playerPositionReference);
			IsMuted = new NetworkVariable<bool>(isMuted);
			IsReplay = new NetworkVariable<bool>(isReplay);
		}
	}

	public int GetPlayerJerseyID()
	{
		switch (Team)
		{
		case PlayerTeam.Blue:
			if (Role != PlayerRole.Attacker)
			{
				return JerseyIDBlueGoalie;
			}
			return JerseyIDBlueAttacker;
		case PlayerTeam.Red:
			if (Role != PlayerRole.Attacker)
			{
				return JerseyIDRedGoalie;
			}
			return JerseyIDRedAttacker;
		default:
			return 0;
		}
	}

	public int GetPlayerHeadgearID()
	{
		switch (Team)
		{
		case PlayerTeam.Blue:
			if (Role != PlayerRole.Attacker)
			{
				return HeadgearIDBlueGoalie;
			}
			return HeadgearIDBlueAttacker;
		case PlayerTeam.Red:
			if (Role != PlayerRole.Attacker)
			{
				return HeadgearIDRedGoalie;
			}
			return HeadgearIDRedAttacker;
		default:
			return 0;
		}
	}

	public int GetPlayerStickSkinID()
	{
		switch (Team)
		{
		case PlayerTeam.Blue:
			if (Role != PlayerRole.Attacker)
			{
				return StickSkinIDBlueGoalie;
			}
			return StickSkinIDBlueAttacker;
		case PlayerTeam.Red:
			if (Role != PlayerRole.Attacker)
			{
				return StickSkinIDRedGoalie;
			}
			return StickSkinIDRedAttacker;
		default:
			return 0;
		}
	}

	public int GetPlayerStickShaftTapeID()
	{
		switch (Team)
		{
		case PlayerTeam.Blue:
			if (Role != PlayerRole.Attacker)
			{
				return StickShaftTapeIDBlueGoalie;
			}
			return StickShaftTapeIDBlueAttacker;
		case PlayerTeam.Red:
			if (Role != PlayerRole.Attacker)
			{
				return StickShaftTapeIDRedGoalie;
			}
			return StickShaftTapeIDRedAttacker;
		default:
			return 0;
		}
	}

	public int GetPlayerStickBladeTapeID()
	{
		switch (Team)
		{
		case PlayerTeam.Blue:
			if (Role != PlayerRole.Attacker)
			{
				return StickBladeTapeIDBlueGoalie;
			}
			return StickBladeTapeIDBlueAttacker;
		case PlayerTeam.Red:
			if (Role != PlayerRole.Attacker)
			{
				return StickBladeTapeIDRedGoalie;
			}
			return StickBladeTapeIDRedAttacker;
		default:
			return 0;
		}
	}

	private void OnPlayerGameStateChanged(PlayerGameState oldGameState, PlayerGameState newGameState)
	{
		EventManager.TriggerEvent("Event_Everyone_OnPlayerGameStateChanged", new Dictionary<string, object>
		{
			{ "player", this },
			{ "oldGameState", oldGameState },
			{ "newGameState", newGameState }
		});
	}

	private void OnPlayerCustomizationStateChanged(PlayerCustomizationState oldCustomizationState, PlayerCustomizationState newCustomizationState)
	{
		EventManager.TriggerEvent("Event_Everyone_OnPlayerCustomizationStateChanged", new Dictionary<string, object>
		{
			{ "player", this },
			{ "oldCustomizationState", oldCustomizationState },
			{ "newCustomizationState", newCustomizationState }
		});
	}

	private void OnPlayerHandednessChanged(PlayerHandedness oldHandedness, PlayerHandedness newHandedness)
	{
		EventManager.TriggerEvent("Event_Everyone_OnPlayerHandednessChanged", new Dictionary<string, object>
		{
			{ "player", this },
			{ "oldHandedness", oldHandedness },
			{ "newHandedness", newHandedness }
		});
	}

	private void OnPlayerSteamIdChanged(FixedString32Bytes oldSteamId, FixedString32Bytes newSteamId)
	{
		EventManager.TriggerEvent("Event_Everyone_OnPlayerSteamIdChanged", new Dictionary<string, object>
		{
			{ "player", this },
			{ "oldSteamId", oldSteamId },
			{ "newSteamId", newSteamId }
		});
	}

	private void OnPlayerUsernameChanged(FixedString32Bytes oldUsername, FixedString32Bytes newUsername)
	{
		EventManager.TriggerEvent("Event_Everyone_OnPlayerUsernameChanged", new Dictionary<string, object>
		{
			{ "player", this },
			{ "oldUsername", oldUsername },
			{ "newUsername", newUsername }
		});
	}

	private void OnPlayerNumberChanged(int oldNumber, int newNumber)
	{
		EventManager.TriggerEvent("Event_Everyone_OnPlayerNumberChanged", new Dictionary<string, object>
		{
			{ "player", this },
			{ "oldNumber", oldNumber },
			{ "newNumber", newNumber }
		});
	}

	private void OnPlayerPatreonLevelChanged(int oldPatreonLevel, int newPatreonLevel)
	{
		EventManager.TriggerEvent("Event_Everyone_OnPlayerPatreonLevelChanged", new Dictionary<string, object>
		{
			{ "player", this },
			{ "oldPatreonLevel", oldPatreonLevel },
			{ "newPatreonLevel", newPatreonLevel }
		});
	}

	private void OnPlayerAdminLevelChanged(int oldAdminLevel, int newAdminLevel)
	{
		EventManager.TriggerEvent("Event_Everyone_OnPlayerAdminLevelChanged", new Dictionary<string, object>
		{
			{ "player", this },
			{ "oldAdminLevel", oldAdminLevel },
			{ "newAdminLevel", newAdminLevel }
		});
	}

	private void OnPlayerGoalsChanged(int oldGoals, int newGoals)
	{
		EventManager.TriggerEvent("Event_Everyone_OnPlayerGoalsChanged", new Dictionary<string, object>
		{
			{ "player", this },
			{ "oldGoals", oldGoals },
			{ "newGoals", newGoals }
		});
	}

	private void OnPlayerAssistsChanged(int oldAssists, int newAssists)
	{
		EventManager.TriggerEvent("Event_Everyone_OnPlayerAssistsChanged", new Dictionary<string, object>
		{
			{ "player", this },
			{ "oldAssists", oldAssists },
			{ "newAssists", newAssists }
		});
	}

	private void OnPlayerPingChanged(ulong oldPing, ulong newPing)
	{
		if (EventManager.HasEventListeners("Event_Everyone_OnPlayerPingChanged"))
		{
			EventManager.TriggerEvent("Event_Everyone_OnPlayerPingChanged", new Dictionary<string, object>
			{
				{ "player", this },
				{ "oldPing", oldPing },
				{ "newPing", newPing }
			});
		}
	}

	private void OnPlayerPlayerPositionReferenceChanged(NetworkObjectReference oldPlayerPositionReference, NetworkObjectReference newPlayerPositionReference)
	{
		PlayerPosition playerPositionFromNetworkObjectReference = NetworkingUtils.GetPlayerPositionFromNetworkObjectReference(oldPlayerPositionReference);
		PlayerPosition playerPositionFromNetworkObjectReference2 = NetworkingUtils.GetPlayerPositionFromNetworkObjectReference(newPlayerPositionReference);
		PlayerPosition = playerPositionFromNetworkObjectReference2;
		OnPlayerPositionChanged(playerPositionFromNetworkObjectReference, PlayerPosition);
	}

	private void OnPlayerPositionChanged(PlayerPosition oldPlayerPosition, PlayerPosition newPlayerPosition)
	{
		EventManager.TriggerEvent("Event_Everyone_OnPlayerPositionChanged", new Dictionary<string, object>
		{
			{ "player", this },
			{ "oldPlayerPosition", oldPlayerPosition },
			{ "newPlayerPosition", newPlayerPosition }
		});
	}

	private void OnPlayerIsMutedChanged(bool oldIsMuted, bool newIsMuted)
	{
		EventManager.TriggerEvent("Event_Everyone_OnPlayerIsMutedChanged", new Dictionary<string, object>
		{
			{ "player", this },
			{ "oldIsMuted", oldIsMuted },
			{ "newIsMuted", newIsMuted }
		});
	}

	[Rpc(SendTo.Server, DeferLocal = true)]
	public void Client_RequestTeamRpc(PlayerTeam team, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(2620210071u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in team, default(FastBufferWriter.ForEnums));
			__endSendRpc(ref bufferWriter, 2620210071u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (rpcParams.Receive.SenderClientId == OwnerClientId)
			{
				EventManager.TriggerEvent("Event_Server_OnPlayerRequestTeam", new Dictionary<string, object>
				{
					{ "player", this },
					{ "team", team }
				});
			}
		}
	}

	[Rpc(SendTo.Server, DeferLocal = true)]
	public void Client_RequestClaimPositionRpc(NetworkObjectReference playerPositionReference, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(949682089u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in playerPositionReference, default(FastBufferWriter.ForNetworkSerializable));
			__endSendRpc(ref bufferWriter, 949682089u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (rpcParams.Receive.SenderClientId == OwnerClientId)
			{
				PlayerPosition playerPositionFromNetworkObjectReference = NetworkingUtils.GetPlayerPositionFromNetworkObjectReference(playerPositionReference);
				EventManager.TriggerEvent("Event_Server_OnPlayerRequestPosition", new Dictionary<string, object>
				{
					{ "player", this },
					{ "playerPosition", playerPositionFromNetworkObjectReference }
				});
			}
		}
	}

	[Rpc(SendTo.Server, DeferLocal = true)]
	public void Client_RequestTeamSelectRpc(RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(4280154797u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Reliable);
			__endSendRpc(ref bufferWriter, 4280154797u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (rpcParams.Receive.SenderClientId == OwnerClientId)
			{
				EventManager.TriggerEvent("Event_Server_OnPlayerRequestTeamSelect", new Dictionary<string, object> { { "player", this } });
			}
		}
	}

	[Rpc(SendTo.Server, DeferLocal = true)]
	public void Client_RequestPositionSelectRpc(RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(3454979199u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Reliable);
			__endSendRpc(ref bufferWriter, 3454979199u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (rpcParams.Receive.SenderClientId == OwnerClientId)
			{
				EventManager.TriggerEvent("Event_Server_OnPlayerRequestPositionSelect", new Dictionary<string, object> { { "player", this } });
			}
		}
	}

	[Rpc(SendTo.Server, DeferLocal = true)]
	public void Client_RequestForfeitRpc(RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(3378576624u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Reliable);
			__endSendRpc(ref bufferWriter, 3378576624u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (rpcParams.Receive.SenderClientId == OwnerClientId)
			{
				EventManager.TriggerEvent("Event_Server_OnPlayerRequestForfeit", new Dictionary<string, object> { { "player", this } });
			}
		}
	}

	[Rpc(SendTo.Server, DeferLocal = true)]
	public void Client_RequestHandednessRpc(PlayerHandedness handedness, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(744616166u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in handedness, default(FastBufferWriter.ForEnums));
			__endSendRpc(ref bufferWriter, 744616166u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (rpcParams.Receive.SenderClientId == OwnerClientId)
			{
				EventManager.TriggerEvent("Event_Server_OnPlayerRequestHandedness", new Dictionary<string, object>
				{
					{ "player", this },
					{ "handedness", handedness }
				});
			}
		}
	}

	public void Server_SetGameState(PlayerPhase? phase = null, PlayerTeam? team = null, PlayerRole? role = null, float? delay = null)
	{
		if (!NetworkManager.Singleton.IsServer)
		{
			return;
		}
		Server_CancelDelayedGameState();
		if (!delay.HasValue)
		{
			PlayerGameState value = new PlayerGameState
			{
				Phase = (phase ?? Phase),
				Team = (team ?? Team),
				Role = (role ?? Role)
			};
			GameState.Value = value;
		}
		else
		{
			delayedGameStateTween = DOVirtual.DelayedCall(delay.Value, () =>
			{
				Server_SetGameState(phase, team, role);
			});
		}
	}

	public void Server_CancelDelayedGameState()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			delayedGameStateTween?.Kill();
		}
	}

	public void Server_SpawnCharacter(Vector3 position, Quaternion rotation, PlayerRole role)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Logger.Info($"Spawning character ({OwnerClientId})");
			if (IsCharacterSpawned)
			{
				Server_DespawnCharacter();
			}
			if (IsSpectatorCameraSpawned)
			{
				Server_DespawnSpectatorCamera();
			}
			Server_SpawnPlayerBody(position, rotation, role);
			Server_SpawnStick(StickPositioner.RaycastOriginPosition, Quaternion.LookRotation(PlayerBody.transform.right, Vector3.up), role);
		}
	}

	public void Server_SpawnPlayerCamera(PlayerBody playerBody)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Logger.Info($"Spawning PlayerCamera ({OwnerClientId})");
			PlayerCamera playerCamera = UnityEngine.Object.Instantiate(playerCameraPrefab);
			playerCamera.InitializeNetworkVariables(new NetworkObjectReference(NetworkObject));
			playerCamera.NetworkObject.SpawnWithOwnership(OwnerClientId);
			if (!playerCamera.NetworkObject.TrySetParent(playerBody.transform, worldPositionStays: false))
			{
				Logger.Error($"Failed to set parent for PlayerCamera ({OwnerClientId})");
			}
		}
	}

	public void Server_SpawnPlayerBody(Vector3 position, Quaternion rotation, PlayerRole role)
	{
		if (!NetworkManager.Singleton.IsServer)
		{
			return;
		}
		Logger.Info($"Spawning PlayerBody ({OwnerClientId})");
		PlayerBody playerBody = null;
		switch (role)
		{
		case PlayerRole.Attacker:
			playerBody = playerBodyAttackerPrefab;
			break;
		case PlayerRole.Goalie:
			playerBody = playerBodyGoaliePrefab;
			break;
		}
		if (playerBody == null)
		{
			Logger.Error($"Failed to spawn PlayerBody ({OwnerClientId}): Missing prefab for role {role}");
			return;
		}
		PlayerBody playerBody2 = UnityEngine.Object.Instantiate(playerBody, position, rotation);
		playerBody2.InitializeNetworkVariables(new NetworkObjectReference(NetworkObject), 1f);
		playerBody2.NetworkObject.SpawnWithOwnership(OwnerClientId);
		if (playerBody2.NetworkObject.TrySetParent(gameObject.transform))
		{
			Server_SpawnPlayerCamera(playerBody2);
			Server_SpawnStickPositioner(playerBody2);
		}
		else
		{
			Logger.Error($"Failed to set parent for PlayerBody ({OwnerClientId})");
		}
	}

	public void Server_SpawnStickPositioner(PlayerBody playerBody)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Logger.Info($"Spawning StickPositioner ({OwnerClientId})");
			StickPositioner stickPositioner = UnityEngine.Object.Instantiate(stickPositionerPrefab);
			stickPositioner.InitializeNetworkVariables(new NetworkObjectReference(NetworkObject));
			stickPositioner.NetworkObject.SpawnWithOwnership(OwnerClientId);
			if (!stickPositioner.NetworkObject.TrySetParent(playerBody.transform, worldPositionStays: false))
			{
				Logger.Error($"Failed to set parent for StickPositioner ({OwnerClientId})");
			}
		}
	}

	public void Server_SpawnStick(Vector3 position, Quaternion rotation, PlayerRole role)
	{
		if (!NetworkManager.Singleton.IsServer)
		{
			return;
		}
		Logger.Info($"Spawning Stick ({OwnerClientId})");
		Stick stick = null;
		switch (role)
		{
		case PlayerRole.Attacker:
			stick = stickAttackerPrefab;
			break;
		case PlayerRole.Goalie:
			stick = stickGoaliePrefab;
			break;
		}
		if (stick == null)
		{
			Logger.Error($"Failed to spawn Stick ({OwnerClientId}): Missing prefab for role {role}");
			return;
		}
		Stick stick2 = UnityEngine.Object.Instantiate(stick, position, rotation);
		stick2.InitializeNetworkVariables(new NetworkObjectReference(NetworkObject));
		stick2.NetworkObject.SpawnWithOwnership(OwnerClientId);
		if (!stick2.NetworkObject.TrySetParent(gameObject.transform))
		{
			Logger.Error($"Failed to set parent for Stick ({OwnerClientId})");
		}
	}

	public void Server_DespawnCharacter()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Logger.Info($"Despawning character ({OwnerClientId})");
			Server_DespawnPlayerBody();
			Server_DespawnStick();
		}
	}

	public void Server_DespawnPlayerCamera()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Logger.Info($"Despawning PlayerCamera ({OwnerClientId})");
			if ((bool)PlayerCamera && PlayerCamera.NetworkObject.IsSpawned)
			{
				PlayerCamera.NetworkObject.Despawn();
			}
		}
	}

	public void Server_DespawnPlayerBody()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Logger.Info($"Despawning PlayerBody ({OwnerClientId})");
			Server_DespawnPlayerCamera();
			Server_DespawnStickPositioner();
			if ((bool)PlayerBody && PlayerBody.NetworkObject.IsSpawned)
			{
				PlayerBody.NetworkObject.Despawn();
			}
		}
	}

	public void Server_DespawnStickPositioner()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Logger.Info($"Despawning StickPositioner ({OwnerClientId})");
			if ((bool)StickPositioner && StickPositioner.NetworkObject.IsSpawned)
			{
				StickPositioner.NetworkObject.Despawn();
			}
		}
	}

	public void Server_DespawnStick()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Logger.Info($"Despawning Stick ({OwnerClientId})");
			if ((bool)Stick && Stick.NetworkObject.IsSpawned)
			{
				Stick.NetworkObject.Despawn();
			}
		}
	}

	public void Server_SpawnSpectatorCamera(Vector3 position, Quaternion rotation)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Logger.Info($"Spawning SpectatorCamera ({OwnerClientId})");
			if (IsCharacterSpawned)
			{
				Server_DespawnCharacter();
			}
			if (IsSpectatorCameraSpawned)
			{
				Server_DespawnSpectatorCamera();
			}
			SpectatorCamera spectatorCamera = UnityEngine.Object.Instantiate(spectatorCameraPrefab, position, rotation);
			spectatorCamera.InitializeNetworkVariables(new NetworkObjectReference(NetworkObject));
			spectatorCamera.PlayerReference.Value = new NetworkObjectReference(NetworkObject);
			spectatorCamera.NetworkObject.SpawnWithOwnership(OwnerClientId);
			if (!spectatorCamera.NetworkObject.TrySetParent(gameObject.transform))
			{
				Logger.Error($"Failed to set parent for SpectatorCamera ({OwnerClientId})");
			}
		}
	}

	public void Server_DespawnSpectatorCamera()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Logger.Info($"Despawning SpectatorCamera ({OwnerClientId})");
			if ((bool)SpectatorCamera && SpectatorCamera.NetworkObject.IsSpawned)
			{
				SpectatorCamera.NetworkObject.Despawn();
			}
		}
	}

	public void Server_GoalScored()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Goals.Value++;
		}
	}

	public void Server_AssistScored()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Assists.Value++;
		}
	}

	public void Server_ResetPoints()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Goals.Value = 0;
			Assists.Value = 0;
		}
	}

	public void Server_UpdatePing()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Ping.Value = NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetCurrentRtt(OwnerClientId);
		}
	}

	public void Server_ConsumeChatTicket()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			ChatTickets = Mathf.Max(0f, ChatTickets - 1f);
		}
	}

	protected override void __initializeVariables()
	{
		if (GameState == null)
		{
			throw new Exception("Player.GameState cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		GameState.Initialize(this);
		__nameNetworkVariable(GameState, "GameState");
		NetworkVariableFields.Add(GameState);
		if (CustomizationState == null)
		{
			throw new Exception("Player.CustomizationState cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		CustomizationState.Initialize(this);
		__nameNetworkVariable(CustomizationState, "CustomizationState");
		NetworkVariableFields.Add(CustomizationState);
		if (Handedness == null)
		{
			throw new Exception("Player.Handedness cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		Handedness.Initialize(this);
		__nameNetworkVariable(Handedness, "Handedness");
		NetworkVariableFields.Add(Handedness);
		if (SteamId == null)
		{
			throw new Exception("Player.SteamId cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		SteamId.Initialize(this);
		__nameNetworkVariable(SteamId, "SteamId");
		NetworkVariableFields.Add(SteamId);
		if (Username == null)
		{
			throw new Exception("Player.Username cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		Username.Initialize(this);
		__nameNetworkVariable(Username, "Username");
		NetworkVariableFields.Add(Username);
		if (Number == null)
		{
			throw new Exception("Player.Number cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		Number.Initialize(this);
		__nameNetworkVariable(Number, "Number");
		NetworkVariableFields.Add(Number);
		if (PatreonLevel == null)
		{
			throw new Exception("Player.PatreonLevel cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		PatreonLevel.Initialize(this);
		__nameNetworkVariable(PatreonLevel, "PatreonLevel");
		NetworkVariableFields.Add(PatreonLevel);
		if (AdminLevel == null)
		{
			throw new Exception("Player.AdminLevel cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		AdminLevel.Initialize(this);
		__nameNetworkVariable(AdminLevel, "AdminLevel");
		NetworkVariableFields.Add(AdminLevel);
		if (Goals == null)
		{
			throw new Exception("Player.Goals cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		Goals.Initialize(this);
		__nameNetworkVariable(Goals, "Goals");
		NetworkVariableFields.Add(Goals);
		if (Assists == null)
		{
			throw new Exception("Player.Assists cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		Assists.Initialize(this);
		__nameNetworkVariable(Assists, "Assists");
		NetworkVariableFields.Add(Assists);
		if (Ping == null)
		{
			throw new Exception("Player.Ping cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		Ping.Initialize(this);
		__nameNetworkVariable(Ping, "Ping");
		NetworkVariableFields.Add(Ping);
		if (PlayerPositionReference == null)
		{
			throw new Exception("Player.PlayerPositionReference cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		PlayerPositionReference.Initialize(this);
		__nameNetworkVariable(PlayerPositionReference, "PlayerPositionReference");
		NetworkVariableFields.Add(PlayerPositionReference);
		if (IsMuted == null)
		{
			throw new Exception("Player.IsMuted cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		IsMuted.Initialize(this);
		__nameNetworkVariable(IsMuted, "IsMuted");
		NetworkVariableFields.Add(IsMuted);
		if (IsReplay == null)
		{
			throw new Exception("Player.IsReplay cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		IsReplay.Initialize(this);
		__nameNetworkVariable(IsReplay, "IsReplay");
		NetworkVariableFields.Add(IsReplay);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(2620210071u, __rpc_handler_2620210071, "Client_RequestTeamRpc", RpcInvokePermission.Everyone);
		__registerRpc(949682089u, __rpc_handler_949682089, "Client_RequestClaimPositionRpc", RpcInvokePermission.Everyone);
		__registerRpc(4280154797u, __rpc_handler_4280154797, "Client_RequestTeamSelectRpc", RpcInvokePermission.Everyone);
		__registerRpc(3454979199u, __rpc_handler_3454979199, "Client_RequestPositionSelectRpc", RpcInvokePermission.Everyone);
		__registerRpc(3378576624u, __rpc_handler_3378576624, "Client_RequestForfeitRpc", RpcInvokePermission.Everyone);
		__registerRpc(744616166u, __rpc_handler_744616166, "Client_RequestHandednessRpc", RpcInvokePermission.Everyone);
		base.__initializeRpcs();
	}

	private static void __rpc_handler_2620210071(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out PlayerTeam value, default(FastBufferWriter.ForEnums));
			RpcParams ext = rpcParams.Ext;
			((Player)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((Player)target).Client_RequestTeamRpc(value, ext);
			((Player)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_949682089(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out NetworkObjectReference value, default(FastBufferWriter.ForNetworkSerializable));
			RpcParams ext = rpcParams.Ext;
			((Player)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((Player)target).Client_RequestClaimPositionRpc(value, ext);
			((Player)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_4280154797(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			RpcParams ext = rpcParams.Ext;
			((Player)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((Player)target).Client_RequestTeamSelectRpc(ext);
			((Player)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3454979199(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			RpcParams ext = rpcParams.Ext;
			((Player)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((Player)target).Client_RequestPositionSelectRpc(ext);
			((Player)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3378576624(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			RpcParams ext = rpcParams.Ext;
			((Player)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((Player)target).Client_RequestForfeitRpc(ext);
			((Player)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_744616166(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out PlayerHandedness value, default(FastBufferWriter.ForEnums));
			RpcParams ext = rpcParams.Ext;
			((Player)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((Player)target).Client_RequestHandednessRpc(value, ext);
			((Player)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected override string __getTypeName()
	{
		return "Player";
	}
}
