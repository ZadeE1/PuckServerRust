using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerPosition : NetworkBehaviour
{
	private static readonly Logger Logger = new Logger("PlayerPosition");

	[Header("Settings")]
	public string Name;

	public PlayerTeam Team;

	public PlayerRole Role;

	[HideInInspector]
	public NetworkVariable<NetworkObjectReference> ClaimedByPlayerReference;

	[HideInInspector]
	public Player ClaimedByPlayer;

	private bool isNetworkVariablesInitialized;

	[HideInInspector]
	public bool IsClaimed => ClaimedByPlayer != null;

	protected override void OnNetworkPreSpawn(ref NetworkManager networkManager)
	{
		InitializeNetworkVariables();
		base.OnNetworkPreSpawn(ref networkManager);
	}

	public override void OnNetworkSpawn()
	{
		NetworkVariable<NetworkObjectReference> claimedByPlayerReference = ClaimedByPlayerReference;
		claimedByPlayerReference.OnValueChanged = (NetworkVariable<NetworkObjectReference>.OnValueChangedDelegate)Delegate.Combine(claimedByPlayerReference.OnValueChanged, new NetworkVariable<NetworkObjectReference>.OnValueChangedDelegate(OnClaimedByReferenceChanged));
		base.OnNetworkSpawn();
	}

	protected override void OnNetworkPostSpawn()
	{
		EventManager.TriggerEvent("Event_Everyone_OnPlayerPositionSpawned", new Dictionary<string, object> { { "playerPosition", this } });
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
		EventManager.TriggerEvent("Event_Everyone_OnPlayerPositionDespawned", new Dictionary<string, object> { { "playerPosition", this } });
		NetworkVariable<NetworkObjectReference> claimedByPlayerReference = ClaimedByPlayerReference;
		claimedByPlayerReference.OnValueChanged = (NetworkVariable<NetworkObjectReference>.OnValueChangedDelegate)Delegate.Remove(claimedByPlayerReference.OnValueChanged, new NetworkVariable<NetworkObjectReference>.OnValueChangedDelegate(OnClaimedByReferenceChanged));
		base.OnNetworkDespawn();
	}

	public void InitializeNetworkVariables(NetworkObjectReference claimedByPlayerReference = default(NetworkObjectReference))
	{
		if (!isNetworkVariablesInitialized)
		{
			isNetworkVariablesInitialized = true;
			ClaimedByPlayerReference = new NetworkVariable<NetworkObjectReference>(claimedByPlayerReference);
		}
	}

	private void ProcessInitialNetworkVariableValues()
	{
		OnClaimedByReferenceChanged(default, ClaimedByPlayerReference.Value);
	}

	private void OnClaimedByReferenceChanged(NetworkObjectReference oldClaimedByReferece, NetworkObjectReference newClaimedByReferece)
	{
		Player playerFromNetworkObjectReference = NetworkingUtils.GetPlayerFromNetworkObjectReference(oldClaimedByReferece);
		Player value = (ClaimedByPlayer = NetworkingUtils.GetPlayerFromNetworkObjectReference(newClaimedByReferece));
		EventManager.TriggerEvent("Event_Everyone_OnPlayerPositionClaimedByPlayerChanged", new Dictionary<string, object>
		{
			{ "playerPosition", this },
			{ "oldClaimedByPlayer", playerFromNetworkObjectReference },
			{ "newClaimedByPlayer", value }
		});
	}

	public void Server_Claim(Player player)
	{
		Logger.Info($"Position {Name} claimed by {player.OwnerClientId}");
		ClaimedByPlayerReference.Value = new NetworkObjectReference(player.NetworkObject);
	}

	public void Server_Unclaim()
	{
		Logger.Info("Position " + Name + " unclaimed");
		ClaimedByPlayerReference.Value = default;
	}

	protected override void __initializeVariables()
	{
		if (ClaimedByPlayerReference == null)
		{
			throw new Exception("PlayerPosition.ClaimedByPlayerReference cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		ClaimedByPlayerReference.Initialize(this);
		__nameNetworkVariable(ClaimedByPlayerReference, "ClaimedByPlayerReference");
		NetworkVariableFields.Add(ClaimedByPlayerReference);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected override string __getTypeName()
	{
		return "PlayerPosition";
	}
}
