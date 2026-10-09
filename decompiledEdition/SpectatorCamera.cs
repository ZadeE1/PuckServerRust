using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpectatorCamera : BaseCamera
{
	[Header("Movement Settings")]
	[SerializeField]
	private float movementSpeed = 5f;

	[SerializeField]
	private float positionSmoothTime = 0.25f;

	[SerializeField]
	private float lookSmoothing = 10f;

	[SerializeField]
	private float pitchMin = -89f;

	[SerializeField]
	private float pitchMax = 89f;

	[HideInInspector]
	public NetworkVariable<NetworkObjectReference> PlayerReference;

	[HideInInspector]
	public Player Player;

	private Vector3 position = Vector3.zero;

	private Vector3 positionVelocity = Vector3.zero;

	private float pitch;

	private float yaw;

	private float targetPitch;

	private float targetYaw;

	private bool isNetworkVariablesInitialized;

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
		position = transform.position;
		pitch = transform.eulerAngles.x;
		yaw = transform.eulerAngles.y;
		targetPitch = pitch;
		targetYaw = yaw;
		NetworkObjectReference value = PlayerReference.Value;
		HandlePlayerReference(default, value);
		EventManager.TriggerEvent("Event_Everyone_OnSpectatorCameraSpawned", new Dictionary<string, object> { { "spectatorCamera", this } });
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

	private void Update()
	{
		if (IsOwner)
		{
			float deltaTime = Time.deltaTime;
			float num = (InputManager.TurnRightAction.IsPressed() ? 1 : 0) + (InputManager.TurnLeftAction.IsPressed() ? (-1) : 0);
			float num2 = (InputManager.JumpAction.IsPressed() ? 1 : (InputManager.SlideAction.IsPressed() ? (-1) : 0));
			float num3 = (InputManager.MoveForwardAction.IsPressed() ? 1 : 0) + (InputManager.MoveBackwardAction.IsPressed() ? (-1) : 0);
			bool flag = InputManager.SprintAction.IsPressed();
			Vector2 vector = InputManager.StickAction.ReadValue<Vector2>();
			vector = new Vector2(float.IsNaN(vector.x) ? 0f : (vector.x * SettingsManager.LookSensitivity), float.IsNaN(vector.y) ? 0f : (vector.y * SettingsManager.LookSensitivity));
			if (GlobalStateManager.UIState.IsMouseRequired)
			{
				num = 0f;
				num2 = 0f;
				num3 = 0f;
				flag = false;
				vector = Vector2.zero;
			}
			float num4 = (flag ? (movementSpeed * 2f) : movementSpeed);
			position += transform.right * num * num4 * deltaTime;
			position += transform.up * num2 * num4 * deltaTime;
			position += transform.forward * num3 * num4 * deltaTime;
			transform.position = Vector3.SmoothDamp(transform.position, position, ref positionVelocity, positionSmoothTime, float.PositiveInfinity, deltaTime);
			targetPitch = Mathf.Clamp(targetPitch - vector.y, pitchMin, pitchMax);
			targetYaw += vector.x;
			pitch = Mathf.Lerp(pitch, targetPitch, lookSmoothing * deltaTime);
			yaw = Mathf.Lerp(yaw, targetYaw, lookSmoothing * deltaTime);
			transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
		}
	}

	private void HandlePlayerReference(NetworkObjectReference oldPlayerReference = default(NetworkObjectReference), NetworkObjectReference newPlayerReference = default(NetworkObjectReference))
	{
		Player player = (oldPlayerReference.TryGet(out var networkObject) ? networkObject.GetComponent<Player>() : null);
		Player player2 = (newPlayerReference.TryGet(out var networkObject2) ? networkObject2.GetComponent<Player>() : null);
		if ((bool)player)
		{
			player.SpectatorCamera = null;
		}
		if ((bool)player2)
		{
			Player = player2;
			Player.SpectatorCamera = this;
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
			throw new Exception("SpectatorCamera.PlayerReference cannot be null. All NetworkVariableBase instances must be initialized.");
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
		return "SpectatorCamera";
	}
}
