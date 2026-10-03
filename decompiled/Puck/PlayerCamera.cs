using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerCamera : BaseCamera
{
	[HideInInspector]
	public NetworkVariable<NetworkObjectReference> PlayerReference;

	[HideInInspector]
	public Player Player;

	private bool isNetworkVariablesInitialized;

	[HideInInspector]
	public PlayerBody PlayerBody
	{
		get
		{
			if (!(Player == null))
			{
				return Player.PlayerBody;
			}
			return null;
		}
	}

	protected override void OnNetworkPreSpawn(ref NetworkManager networkManager)
	{
		InitializeNetworkVariables();
		base.OnNetworkPreSpawn(ref networkManager);
	}

	public override void OnNetworkSpawn()
	{
		NetworkVariable<NetworkObjectReference> playerReference = PlayerReference;
		playerReference.OnValueChanged = (NetworkVariable<NetworkObjectReference>.OnValueChangedDelegate)Delegate.Combine(playerReference.OnValueChanged, new NetworkVariable<NetworkObjectReference>.OnValueChangedDelegate(OnPlayerReferenceChanged));
		base.OnNetworkSpawn();
	}

	protected override void OnNetworkPostSpawn()
	{
		NetworkObjectReference value = PlayerReference.Value;
		HandlePlayerReference(default, value);
		EventManager.TriggerEvent("Event_Everyone_OnPlayerCameraSpawned", new Dictionary<string, object> { { "playerCamera", this } });
		base.OnNetworkPostSpawn();
	}

	public override void OnNetworkDespawn()
	{
		NetworkVariable<NetworkObjectReference> playerReference = PlayerReference;
		playerReference.OnValueChanged = (NetworkVariable<NetworkObjectReference>.OnValueChangedDelegate)Delegate.Remove(playerReference.OnValueChanged, new NetworkVariable<NetworkObjectReference>.OnValueChangedDelegate(OnPlayerReferenceChanged));
		base.OnNetworkDespawn();
	}

	public void InitializeNetworkVariables(NetworkObjectReference playerReference = default(NetworkObjectReference))
	{
		if (!isNetworkVariablesInitialized)
		{
			isNetworkVariablesInitialized = true;
			PlayerReference = new NetworkVariable<NetworkObjectReference>(playerReference);
		}
	}

	public override bool Enable()
	{
		bool flag = base.Enable();
		if (flag)
		{
			EventManager.TriggerEvent("Event_OnPlayerCameraEnabled", new Dictionary<string, object> { { "playerCamera", this } });
		}
		return flag;
	}

	public override bool Disable()
	{
		bool flag = base.Disable();
		if (flag)
		{
			EventManager.TriggerEvent("Event_OnPlayerCameraDisabled", new Dictionary<string, object> { { "playerCamera", this } });
		}
		return flag;
	}

	private void Update()
	{
		if ((bool)Player)
		{
			PlayerInput playerInput = Player.PlayerInput;
			if ((bool)playerInput)
			{
				playerInput.UpdateLookAngle(Time.deltaTime);
				transform.localRotation = Quaternion.Euler(Player.IsLocalPlayer ? playerInput.LookAngleInput.ClientValue : playerInput.LookAngleInput.ServerValue);
			}
		}
	}

	private void HandlePlayerReference(NetworkObjectReference oldPlayerReference = default(NetworkObjectReference), NetworkObjectReference newPlayerReference = default(NetworkObjectReference))
	{
		Player player = (oldPlayerReference.TryGet(out var networkObject) ? networkObject.GetComponent<Player>() : null);
		Player player2 = (newPlayerReference.TryGet(out var networkObject2) ? networkObject2.GetComponent<Player>() : null);
		if ((bool)player)
		{
			player.PlayerCamera = null;
		}
		if ((bool)player2)
		{
			Player = player2;
			Player.PlayerCamera = this;
		}
		else
		{
			Player = null;
		}
	}

	private void OnPlayerReferenceChanged(NetworkObjectReference oldPlayerReference, NetworkObjectReference newPlayerReference)
	{
		HandlePlayerReference(oldPlayerReference, newPlayerReference);
	}

	protected override void __initializeVariables()
	{
		if (PlayerReference == null)
		{
			throw new Exception("PlayerCamera.PlayerReference cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		PlayerReference.Initialize(this);
		__nameNetworkVariable(PlayerReference, "PlayerReference");
		NetworkVariableFields.Add(PlayerReference);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected override string __getTypeName()
	{
		return "PlayerCamera";
	}
}
