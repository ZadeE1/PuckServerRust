using System;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class PlayerBody : NetworkBehaviour
{
	[Header("Settings")]
	[SerializeField]
	private float gravityMultiplier = 2f;

	[SerializeField]
	private float hoverDistance = 1.2f;

	[Space(20f)]
	[SerializeField]
	private float upwardnessThreshold = 0.8f;

	[SerializeField]
	private float sidewaysThreshold = 0.2f;

	[Space(20f)]
	[SerializeField]
	private float balanceLossTime = 0.25f;

	[SerializeField]
	private float balanceRecoveryTime = 5f;

	[Space(20f)]
	[SerializeField]
	private float staminaRegenerationRate = 10f;

	[Space(20f)]
	[SerializeField]
	private float sprintStaminaDrainRate = 1.4f;

	[Space(20f)]
	[SerializeField]
	private float slideTurnMultiplier = 2f;

	[SerializeField]
	private float slideHoverDistance = 0.8f;

	[Space(20f)]
	[SerializeField]
	private float jumpVelocity = 6f;

	[SerializeField]
	private float jumpStaminaDrain = 0.125f;

	[SerializeField]
	private float jumpTurnMultiplier = 5f;

	[Space(20f)]
	[SerializeField]
	private float twistVelocity = 5f;

	[SerializeField]
	private float twistStaminaDrain = 0.125f;

	[Space(20f)]
	[SerializeField]
	private bool canDash = true;

	[SerializeField]
	private float dashVelocity = 6f;

	[SerializeField]
	private float dashStaminaDrain = 0.125f;

	[SerializeField]
	private float dashDrag = 5f;

	[SerializeField]
	private float dashDragTime = 1f;

	[Space(20f)]
	[SerializeField]
	private float slideDrag = 0.2f;

	[SerializeField]
	private float stopDrag = 2.5f;

	[SerializeField]
	private float fallenDrag = 0.2f;

	[Space(20f)]
	[SerializeField]
	private float tackleSpeedThreshold = 7.6f;

	[SerializeField]
	private float tackleForceThreshold = 7f;

	[SerializeField]
	private float tackleForceMultiplier = 0.3f;

	[SerializeField]
	private float tackleBounceMaximumMagnitude = 10f;

	[Space(20f)]
	[SerializeField]
	private float stretchSpeed = 10f;

	[Space(20f)]
	[SerializeField]
	private float maximumLaterality = 1f;

	[SerializeField]
	private float minimumLaterality = 0.5f;

	[SerializeField]
	private float minimumLateralitySpeed = 2f;

	[SerializeField]
	private float maximumLateralitySpeed = 5f;

	[Space(20f)]
	[SerializeField]
	private AnimationCurve windVolumeCurve;

	[SerializeField]
	private AnimationCurve iceVolumeCurve;

	[SerializeField]
	private AnimationCurve icePitchCurve;

	[SerializeField]
	private AnimationCurve gruntVolumeCurve;

	[SerializeField]
	private AnimationCurve gruntPitchCurve;

	[Header("References")]
	[SerializeField]
	private Transform movementDirection;

	[SerializeField]
	private PlayerMesh playerMesh;

	[SerializeField]
	private SynchronizedAudio windAudioSource;

	[SerializeField]
	private SynchronizedAudio iceAudioSource;

	[SerializeField]
	private SynchronizedAudio gruntAudioSource;

	[SerializeField]
	private AudioSource voiceAudioSource;

	[HideInInspector]
	public NetworkVariable<NetworkObjectReference> PlayerReference;

	[HideInInspector]
	public CompressedNetworkVariable<float, byte> Stamina;

	[HideInInspector]
	public CompressedNetworkVariable<float, byte> Speed;

	[HideInInspector]
	public NetworkVariable<bool> IsSprinting;

	[HideInInspector]
	public NetworkVariable<bool> IsSliding;

	[HideInInspector]
	public NetworkVariable<bool> IsStopping;

	[HideInInspector]
	public NetworkVariable<bool> IsExtendedLeft;

	[HideInInspector]
	public NetworkVariable<bool> IsExtendedRight;

	[HideInInspector]
	public Player Player;

	[HideInInspector]
	public Rigidbody Rigidbody;

	[HideInInspector]
	public SynchronizedObject SynchronizedObject;

	[HideInInspector]
	public Movement Movement;

	[HideInInspector]
	public VelocityLean VelocityLean;

	[HideInInspector]
	public Hover Hover;

	[HideInInspector]
	public Skate Skate;

	[HideInInspector]
	public KeepUpright KeepUpright;

	[HideInInspector]
	public MeshRendererHider MeshRendererHider;

	[HideInInspector]
	public CollisionRecorder CollisionRecorder;

	[HideInInspector]
	public bool HasDashed;

	[HideInInspector]
	public bool HasDashExtended;

	[HideInInspector]
	public bool HasSlipped;

	[HideInInspector]
	public NetworkVariable<bool> HasFallen;

	[HideInInspector]
	public float Laterality;

	private bool isNetworkVariablesInitialized;

	private Collider[] bodyColliders;

	private SoftCollider softCollider;

	private Tween balanceLossTween;

	private Tween balanceRecoveryTween;

	private Tween dashDragTween;

	private Tween dashLegPadTween;

	[HideInInspector]
	public PlayerCamera PlayerCamera
	{
		get
		{
			if (!(Player == null))
			{
				return Player.PlayerCamera;
			}
			return null;
		}
	}

	[HideInInspector]
	public Stick Stick
	{
		get
		{
			if (!(Player == null))
			{
				return Player.Stick;
			}
			return null;
		}
	}

	[HideInInspector]
	public PlayerMesh PlayerMesh => playerMesh;

	[HideInInspector]
	public AudioSource VoiceAudioSource => voiceAudioSource;

	[HideInInspector]
	public float Upwardness => Vector3.Dot(transform.up, Vector3.up);

	[HideInInspector]
	public bool IsUpright => Upwardness > upwardnessThreshold;

	[HideInInspector]
	public bool IsSlipping => Upwardness < upwardnessThreshold;

	[HideInInspector]
	public bool IsSideways => Upwardness < sidewaysThreshold;

	[HideInInspector]
	public bool IsGrounded
	{
		get
		{
			if (Hover.IsGrounded)
			{
				return IsUpright;
			}
			return false;
		}
	}

	[HideInInspector]
	public bool IsJumping
	{
		get
		{
			if (!Hover.IsGrounded)
			{
				return IsUpright;
			}
			return false;
		}
	}

	[HideInInspector]
	public bool IsBalanced => KeepUpright.Balance >= 1f;

	[HideInInspector]
	public Transform MovementDirection => movementDirection;

	public virtual void Awake()
	{
		Rigidbody = GetComponent<Rigidbody>();
		SynchronizedObject = GetComponent<SynchronizedObject>();
		bodyColliders = GetComponents<Collider>();
		softCollider = GetComponent<SoftCollider>();
		MeshRendererHider = GetComponent<MeshRendererHider>();
		CollisionRecorder = GetComponent<CollisionRecorder>();
		CollisionRecorder collisionRecorder = CollisionRecorder;
		collisionRecorder.CollisionDeferred = (Action<GameObject, float>)Delegate.Combine(collisionRecorder.CollisionDeferred, new Action<GameObject, float>(Server_OnCollisionDeferred));
		Movement = GetComponent<Movement>();
		Movement.MovementDirection = MovementDirection;
		VelocityLean = GetComponent<VelocityLean>();
		VelocityLean.MovementDirection = MovementDirection;
		Hover = GetComponent<Hover>();
		Skate = GetComponent<Skate>();
		Skate.MovementDirection = MovementDirection;
		KeepUpright = GetComponent<KeepUpright>();
	}

	public void SetCollisionFilteredWith(PlayerBody other, bool filtered)
	{
		if (other == null || bodyColliders == null || other.bodyColliders == null)
		{
			return;
		}
		Collider[] array = bodyColliders;
		foreach (Collider collider in array)
		{
			if (collider == null)
			{
				continue;
			}
			Collider[] array2 = other.bodyColliders;
			foreach (Collider collider2 in array2)
			{
				if (!(collider2 == null))
				{
					Physics.IgnoreCollision(collider, collider2, filtered);
				}
			}
		}
		if (softCollider != null)
		{
			softCollider.SetSoftCollisionIgnored(other.Rigidbody, filtered);
		}
		if (other.softCollider != null)
		{
			other.softCollider.SetSoftCollisionIgnored(Rigidbody, filtered);
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
		Stamina.OnRawValueChanged += OnStaminaChanged;
		Speed.OnRawValueChanged += OnSpeedChanged;
		NetworkVariable<bool> isSprinting = IsSprinting;
		isSprinting.OnValueChanged = (NetworkVariable<bool>.OnValueChangedDelegate)Delegate.Combine(isSprinting.OnValueChanged, new NetworkVariable<bool>.OnValueChangedDelegate(OnIsSprintingChanged));
		NetworkVariable<bool> isSliding = IsSliding;
		isSliding.OnValueChanged = (NetworkVariable<bool>.OnValueChangedDelegate)Delegate.Combine(isSliding.OnValueChanged, new NetworkVariable<bool>.OnValueChangedDelegate(OnIsSlidingChanged));
		NetworkVariable<bool> isStopping = IsStopping;
		isStopping.OnValueChanged = (NetworkVariable<bool>.OnValueChangedDelegate)Delegate.Combine(isStopping.OnValueChanged, new NetworkVariable<bool>.OnValueChangedDelegate(OnIsStoppingChanged));
		NetworkVariable<bool> isExtendedLeft = IsExtendedLeft;
		isExtendedLeft.OnValueChanged = (NetworkVariable<bool>.OnValueChangedDelegate)Delegate.Combine(isExtendedLeft.OnValueChanged, new NetworkVariable<bool>.OnValueChangedDelegate(OnIsExtendedLeftChanged));
		NetworkVariable<bool> isExtendedRight = IsExtendedRight;
		isExtendedRight.OnValueChanged = (NetworkVariable<bool>.OnValueChangedDelegate)Delegate.Combine(isExtendedRight.OnValueChanged, new NetworkVariable<bool>.OnValueChangedDelegate(OnIsExtendedRightChanged));
		base.OnNetworkSpawn();
	}

	protected override void OnNetworkPostSpawn()
	{
		NetworkObjectReference value = PlayerReference.Value;
		HandlePlayerReference(default, value);
		EventManager.TriggerEvent("Event_Everyone_OnPlayerBodySpawned", new Dictionary<string, object> { { "playerBody", this } });
		base.OnNetworkPostSpawn();
	}

	public override void OnNetworkDespawn()
	{
		EventManager.TriggerEvent("Event_Everyone_OnPlayerBodyDespawned", new Dictionary<string, object> { { "playerBody", this } });
		NetworkVariable<NetworkObjectReference> playerReference = PlayerReference;
		playerReference.OnValueChanged = (NetworkVariable<NetworkObjectReference>.OnValueChangedDelegate)Delegate.Remove(playerReference.OnValueChanged, new NetworkVariable<NetworkObjectReference>.OnValueChangedDelegate(OnPlayerReferenceChanged));
		Stamina.OnRawValueChanged -= OnStaminaChanged;
		Speed.OnRawValueChanged -= OnSpeedChanged;
		NetworkVariable<bool> isSprinting = IsSprinting;
		isSprinting.OnValueChanged = (NetworkVariable<bool>.OnValueChangedDelegate)Delegate.Remove(isSprinting.OnValueChanged, new NetworkVariable<bool>.OnValueChangedDelegate(OnIsSprintingChanged));
		NetworkVariable<bool> isSliding = IsSliding;
		isSliding.OnValueChanged = (NetworkVariable<bool>.OnValueChangedDelegate)Delegate.Remove(isSliding.OnValueChanged, new NetworkVariable<bool>.OnValueChangedDelegate(OnIsSlidingChanged));
		NetworkVariable<bool> isStopping = IsStopping;
		isStopping.OnValueChanged = (NetworkVariable<bool>.OnValueChangedDelegate)Delegate.Remove(isStopping.OnValueChanged, new NetworkVariable<bool>.OnValueChangedDelegate(OnIsStoppingChanged));
		NetworkVariable<bool> isExtendedLeft = IsExtendedLeft;
		isExtendedLeft.OnValueChanged = (NetworkVariable<bool>.OnValueChangedDelegate)Delegate.Remove(isExtendedLeft.OnValueChanged, new NetworkVariable<bool>.OnValueChangedDelegate(OnIsExtendedLeftChanged));
		NetworkVariable<bool> isExtendedRight = IsExtendedRight;
		isExtendedRight.OnValueChanged = (NetworkVariable<bool>.OnValueChangedDelegate)Delegate.Remove(isExtendedRight.OnValueChanged, new NetworkVariable<bool>.OnValueChangedDelegate(OnIsExtendedRightChanged));
		base.OnNetworkDespawn();
	}

	public override void OnDestroy()
	{
		balanceLossTween?.Kill();
		balanceRecoveryTween?.Kill();
		dashDragTween?.Kill();
		dashLegPadTween?.Kill();
		CollisionRecorder collisionRecorder = CollisionRecorder;
		collisionRecorder.CollisionDeferred = (Action<GameObject, float>)Delegate.Remove(collisionRecorder.CollisionDeferred, new Action<GameObject, float>(Server_OnCollisionDeferred));
		base.OnDestroy();
	}

	public void InitializeNetworkVariables(NetworkObjectReference playerReference = default(NetworkObjectReference), float stamina = 0f, float speed = 0f, bool isSprinting = false, bool isSliding = false, bool isStopping = false, bool isExtendedLeft = false, bool isExtendedRight = false, bool hasFallen = false)
	{
		if (!isNetworkVariablesInitialized)
		{
			isNetworkVariablesInitialized = true;
			PlayerReference = new NetworkVariable<NetworkObjectReference>(playerReference);
			Stamina = CompressedNetworkVariable<float, byte>.CreateFloatToByte(0f, 1f, stamina, NetworkVariableReadPermission.Owner);
			Speed = CompressedNetworkVariable<float, byte>.CreateFloatToByte(0f, 16f, speed, NetworkVariableReadPermission.Owner);
			IsSprinting = new NetworkVariable<bool>(isSprinting);
			IsSliding = new NetworkVariable<bool>(isSliding);
			IsStopping = new NetworkVariable<bool>(isStopping);
			IsExtendedLeft = new NetworkVariable<bool>(isExtendedLeft);
			IsExtendedRight = new NetworkVariable<bool>(isExtendedRight);
			HasFallen = new NetworkVariable<bool>(hasFallen);
		}
	}

	private void Update()
	{
		if ((bool)Player && (bool)Player.PlayerInput)
		{
			HandleMeshLookAt(Player.PlayerInput);
			HandleMeshStretch();
		}
	}

	private void HandleMeshLookAt(PlayerInput playerInput)
	{
		if (playerInput.LookInput.ServerValue || playerInput.TrackInput.ServerValue)
		{
			if ((bool)PlayerCamera)
			{
				playerMesh.LookAt(PlayerCamera.transform.position + PlayerCamera.transform.forward * 10f, Time.deltaTime);
			}
		}
		else if ((bool)Stick)
		{
			playerMesh.LookAt(Stick.BladeHandlePosition, Time.deltaTime);
		}
	}

	private void HandleMeshStretch()
	{
		float b;
		if (IsJumping)
		{
			b = 1.05f;
		}
		else
		{
			b = (IsSliding.Value ? 0.95f : 1f);
		}
		PlayerMesh.Stretch = Mathf.Lerp(PlayerMesh.Stretch, b, Time.deltaTime * stretchSpeed);
	}

	private void FixedUpdate()
	{
		if (!Player)
		{
			return;
		}
		PlayerInput playerInput = Player.PlayerInput;
		if ((bool)playerInput && NetworkManager.Singleton.IsServer)
		{
			if (!Player.IsReplay.Value)
			{
				HandleInputs(playerInput);
			}
			Speed.Value = Movement.Speed;
			if (IsSprinting.Value)
			{
				Stamina.Value -= (IsSprinting.Value ? (Time.deltaTime / sprintStaminaDrainRate) : 0f);
			}
			else if (Stamina.Value < 1f)
			{
				Stamina.Value += Time.fixedDeltaTime / staminaRegenerationRate;
				Stamina.Value = Mathf.Clamp(Stamina.Value, 0f, 1f);
			}
			if (IsUpright)
			{
				Rigidbody.AddForce(Vector3.up * (0f - Physics.gravity.y), ForceMode.Acceleration);
				Rigidbody.AddForce(Vector3.down * (0f - Physics.gravity.y) * gravityMultiplier, ForceMode.Acceleration);
			}
			MovementDirection.localRotation = Quaternion.FromToRotation(transform.forward, Utils.Vector3Slerp3(-transform.right, transform.forward, transform.right, Laterality));
			Movement.Sprint = IsSprinting.Value;
			ref float turnMultiplier = ref Movement.TurnMultiplier;
			float num;
			if (IsSliding.Value)
			{
				num = slideTurnMultiplier;
			}
			else
			{
				num = (IsJumping ? jumpTurnMultiplier : 1f);
			}
			turnMultiplier = num;
			ref float ambientDrag = ref Movement.AmbientDrag;
			float num2;
			if (HasFallen.Value)
			{
				num2 = fallenDrag;
			}
			else if (HasDashed)
			{
				num2 = Movement.AmbientDrag;
			}
			else if (IsStopping.Value)
			{
				num2 = stopDrag;
			}
			else
			{
				num2 = (IsSliding.Value ? slideDrag : 0f);
			}
			ambientDrag = num2;
			Hover.TargetDistance = (IsSliding.Value ? slideHoverDistance : (KeepUpright.Balance * hoverDistance));
			Skate.Intensity = ((IsSliding.Value || IsStopping.Value || !IsGrounded) ? 0f : KeepUpright.Balance);
			VelocityLean.AngularIntensity = Mathf.Max(0.1f, Movement.NormalizedMaximumSpeed) / (IsSliding.Value ? 2f : (IsJumping ? 2f : 1f));
			VelocityLean.Inverted = !IsJumping && !IsSliding.Value && Movement.IsMovingBackwards;
			VelocityLean.UseWorldLinearVelocity = IsJumping || IsSliding.Value;
			if (!HasSlipped && !HasFallen.Value && IsSlipping)
			{
				OnSlip();
			}
			else if (HasSlipped && !HasFallen.Value && IsSideways)
			{
				OnFall();
			}
			else if (HasFallen.Value && !HasSlipped && IsUpright)
			{
				OnStandUp();
			}
			Server_UpdateAudio();
		}
	}

	public void ApplyCustomizations()
	{
		if (Player.Team != PlayerTeam.None && Player.Role != PlayerRole.None)
		{
			PlayerMesh.SetUsername(Player.Username.Value.ToString());
			PlayerMesh.SetNumber(Player.Number.Value.ToString());
			PlayerMesh.SetLegsPadsActive(Player.Role == PlayerRole.Goalie);
			PlayerMesh.SetFlagID(Player.FlagID);
			PlayerMesh.SetHeadgearID(Player.GetPlayerHeadgearID(), Player.Role);
			PlayerMesh.SetMustacheID(Player.MustacheID);
			PlayerMesh.SetBeardID(Player.BeardID);
			PlayerMesh.SetJerseyID(Player.GetPlayerJerseyID(), Player.Team);
		}
	}

	private void HandleInputs(PlayerInput playerInput)
	{
		if (!IsSprinting.Value && playerInput.SprintInput.ServerValue && !IsSliding.Value && IsGrounded && Stamina.Value > 0.25f)
		{
			IsSprinting.Value = true;
		}
		else if (IsSprinting.Value && !playerInput.SprintInput.ServerValue)
		{
			IsSprinting.Value = false;
		}
		else if (IsSprinting.Value)
		{
			IsSprinting.Value = !IsSliding.Value && IsGrounded && Stamina.Value > 0f;
		}
		IsSliding.Value = playerInput.SlideInput.ServerValue && IsGrounded;
		IsStopping.Value = playerInput.StopInput.ServerValue && IsGrounded;
		if (!HasDashExtended)
		{
			IsExtendedLeft.Value = playerInput.ExtendLeftInput.ServerValue && IsGrounded && IsSliding.Value;
			IsExtendedRight.Value = playerInput.ExtendRightInput.ServerValue && IsGrounded && IsSliding.Value;
		}
		Movement.MoveForwards = !IsSliding.Value && playerInput.MoveInput.ServerValue.y > 0.05f;
		Movement.MoveBackwards = !IsSliding.Value && playerInput.MoveInput.ServerValue.y < -0.05f;
		Movement.TurnRight = playerInput.MoveInput.ServerValue.x > 0.05f;
		Movement.TurnLeft = playerInput.MoveInput.ServerValue.x < -0.05f;
		float t = Mathf.Clamp01(1f - Movement.NormalizedMinimumSpeed);
		float num = Mathf.Lerp(minimumLateralitySpeed, maximumLateralitySpeed, t);
		float num2 = Mathf.Lerp(minimumLaterality, maximumLaterality, t);
		if (playerInput.LateralLeftInput.ServerValue)
		{
			Laterality = Mathf.Lerp(Laterality, 0f - num2, Time.fixedDeltaTime * num);
		}
		else if (playerInput.LateralRightInput.ServerValue)
		{
			Laterality = Mathf.Lerp(Laterality, num2, Time.fixedDeltaTime * num);
		}
		else
		{
			Laterality = Mathf.Lerp(Laterality, 0f, Time.fixedDeltaTime * num);
		}
	}

	public void OnSlip()
	{
		HasSlipped = true;
		HasFallen.Value = false;
		balanceRecoveryTween?.Kill();
		balanceLossTween?.Kill();
		balanceLossTween = DOTween.To(() => KeepUpright.Balance, (float value) =>
		{
			KeepUpright.Balance = value;
		}, 0f, balanceLossTime).SetEase(Ease.Linear);
	}

	public void OnFall()
	{
		HasSlipped = false;
		HasFallen.Value = true;
		balanceRecoveryTween?.Kill();
		balanceRecoveryTween = DOTween.To(() => KeepUpright.Balance, (float value) =>
		{
			KeepUpright.Balance = value;
		}, 1f, balanceRecoveryTime).SetEase(Ease.Linear);
	}

	public void OnStandUp()
	{
		HasSlipped = false;
		HasFallen.Value = false;
	}

	public void Jump()
	{
		if (NetworkManager.Singleton.IsServer && IsGrounded && !(Stamina.Value < jumpStaminaDrain))
		{
			Rigidbody.AddForce(Vector3.up * jumpVelocity, ForceMode.VelocityChange);
			Stamina.Value = Mathf.Max(0f, Stamina.Value - jumpStaminaDrain);
		}
	}

	public void TwistLeft()
	{
		if (NetworkManager.Singleton.IsServer && IsJumping && !(Stamina.Value < twistStaminaDrain))
		{
			Rigidbody.AddTorque(-transform.up * twistVelocity, ForceMode.VelocityChange);
			Stamina.Value = Mathf.Max(0f, Stamina.Value - twistStaminaDrain);
		}
	}

	public void TwistRight()
	{
		if (NetworkManager.Singleton.IsServer && IsJumping && !(Stamina.Value < twistStaminaDrain))
		{
			Rigidbody.AddTorque(transform.up * twistVelocity, ForceMode.VelocityChange);
			Stamina.Value = Mathf.Max(0f, Stamina.Value - twistStaminaDrain);
		}
	}

	public void DashLeft()
	{
		if (NetworkManager.Singleton.IsServer && canDash && IsSliding.Value && !(Stamina.Value < dashStaminaDrain))
		{
			Rigidbody.AddForce(-transform.right * dashVelocity, ForceMode.VelocityChange);
			Stamina.Value = Mathf.Max(0f, Stamina.Value - dashStaminaDrain);
			HasDashed = true;
			Movement.AmbientDrag = dashDrag;
			dashDragTween?.Kill();
			dashDragTween = DOTween.To(() => Movement.AmbientDrag, (float value) =>
			{
				Movement.AmbientDrag = value;
			}, 0f, dashDragTime).OnComplete(() =>
			{
				HasDashed = false;
			}).SetEase(Ease.Linear);
			HasDashExtended = true;
			IsExtendedRight.Value = false;
			IsExtendedRight.Value = true;
			dashLegPadTween?.Kill();
			dashLegPadTween = DOVirtual.DelayedCall(dashDragTime / 4f, () =>
			{
				HasDashExtended = false;
			});
		}
	}

	public void DashRight()
	{
		if (NetworkManager.Singleton.IsServer && canDash && IsSliding.Value && !(Stamina.Value < dashStaminaDrain))
		{
			Rigidbody.AddForce(transform.right * dashVelocity, ForceMode.VelocityChange);
			Stamina.Value = Mathf.Max(0f, Stamina.Value - dashStaminaDrain);
			HasDashed = true;
			Movement.AmbientDrag = dashDrag;
			dashDragTween?.Kill();
			dashDragTween = DOTween.To(() => Movement.AmbientDrag, (float value) =>
			{
				Movement.AmbientDrag = value;
			}, 0f, dashDragTime).OnComplete(() =>
			{
				HasDashed = false;
			}).SetEase(Ease.Linear);
			HasDashExtended = true;
			IsExtendedLeft.Value = false;
			IsExtendedLeft.Value = true;
			dashLegPadTween?.Kill();
			dashLegPadTween = DOVirtual.DelayedCall(dashDragTime / 4f, () =>
			{
				HasDashExtended = false;
			});
		}
	}

	public void CancelDash()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			dashDragTween?.Kill();
			HasDashed = false;
			dashLegPadTween?.Kill();
			HasDashExtended = false;
			IsExtendedLeft.Value = false;
			IsExtendedRight.Value = false;
		}
	}

	private void HandlePlayerReference(NetworkObjectReference oldPlayerReference = default(NetworkObjectReference), NetworkObjectReference newPlayerReference = default(NetworkObjectReference))
	{
		Player player = (oldPlayerReference.TryGet(out var networkObject) ? networkObject.GetComponent<Player>() : null);
		Player player2 = (newPlayerReference.TryGet(out var networkObject2) ? networkObject2.GetComponent<Player>() : null);
		if ((bool)player)
		{
			player.PlayerBody = null;
		}
		if ((bool)player2)
		{
			Player = player2;
			Player.PlayerBody = this;
		}
		else
		{
			Player = null;
		}
		if ((bool)Player)
		{
			ApplyCustomizations();
		}
	}

	private void OnPlayerReferenceChanged(NetworkObjectReference oldReference, NetworkObjectReference newReference)
	{
		HandlePlayerReference(oldReference, newReference);
		EventManager.TriggerEvent("Event_Everyone_OnPlayerBodyPlayerReferenceChanged", new Dictionary<string, object>
		{
			{ "playerBody", this },
			{ "oldPlayerReference", oldReference },
			{ "newPlayerReference", newReference }
		});
	}

	private void OnStaminaChanged(float oldStamina, float newStamina)
	{
		if (EventManager.HasEventListeners("Event_Everyone_OnPlayerBodyStaminaChanged"))
		{
			EventManager.TriggerEvent("Event_Everyone_OnPlayerBodyStaminaChanged", new Dictionary<string, object>
			{
				{ "playerBody", this },
				{ "oldStamina", oldStamina },
				{ "newStamina", newStamina }
			});
		}
	}

	private void OnSpeedChanged(float oldSpeed, float newSpeed)
	{
		if (EventManager.HasEventListeners("Event_Everyone_OnPlayerBodySpeedChanged"))
		{
			EventManager.TriggerEvent("Event_Everyone_OnPlayerBodySpeedChanged", new Dictionary<string, object>
			{
				{ "playerBody", this },
				{ "oldSpeed", oldSpeed },
				{ "newSpeed", newSpeed }
			});
		}
	}

	private void OnIsSprintingChanged(bool oldIsSprinting, bool newIsSprinting)
	{
		EventManager.TriggerEvent("Event_Everyone_OnPlayerBodyIsSprintingChanged", new Dictionary<string, object>
		{
			{ "playerBody", this },
			{ "oldIsSprinting", oldIsSprinting },
			{ "newIsSprinting", newIsSprinting }
		});
	}

	private void OnIsSlidingChanged(bool oldIsSliding, bool newIsSliding)
	{
		PlayerMesh.PlayerLegPadLeft.State = (newIsSliding ? ((!IsExtendedLeft.Value) ? PlayerLegPadState.Butterfly : PlayerLegPadState.ButterflyExtended) : PlayerLegPadState.Idle);
		PlayerMesh.PlayerLegPadRight.State = (newIsSliding ? ((!IsExtendedRight.Value) ? PlayerLegPadState.Butterfly : PlayerLegPadState.ButterflyExtended) : PlayerLegPadState.Idle);
		CancelDash();
		EventManager.TriggerEvent("Event_Everyone_OnPlayerBodyIsSlidingChanged", new Dictionary<string, object>
		{
			{ "playerBody", this },
			{ "oldIsSliding", oldIsSliding },
			{ "newIsSliding", newIsSliding }
		});
	}

	private void OnIsStoppingChanged(bool oldIsStopping, bool newIsStopping)
	{
		EventManager.TriggerEvent("Event_Everyone_OnPlayerBodyIsStoppingChanged", new Dictionary<string, object>
		{
			{ "playerBody", this },
			{ "oldIsStopping", oldIsStopping },
			{ "newIsStopping", newIsStopping }
		});
	}

	private void OnIsExtendedLeftChanged(bool oldIsExtendedLeft, bool newIsExtendedLeft)
	{
		PlayerMesh.PlayerLegPadLeft.State = (IsSliding.Value ? ((!newIsExtendedLeft) ? PlayerLegPadState.Butterfly : PlayerLegPadState.ButterflyExtended) : PlayerLegPadState.Idle);
		EventManager.TriggerEvent("Event_Everyone_OnPlayerBodyIsExtendedLeftChanged", new Dictionary<string, object>
		{
			{ "playerBody", this },
			{ "oldIsExtendedLeft", oldIsExtendedLeft },
			{ "newIsExtendedLeft", newIsExtendedLeft }
		});
	}

	private void OnIsExtendedRightChanged(bool oldIsExtendedRight, bool newIsExtendedRight)
	{
		PlayerMesh.PlayerLegPadRight.State = (IsSliding.Value ? ((!newIsExtendedRight) ? PlayerLegPadState.Butterfly : PlayerLegPadState.ButterflyExtended) : PlayerLegPadState.Idle);
		EventManager.TriggerEvent("Event_Everyone_OnPlayerBodyIsExtendedRightChanged", new Dictionary<string, object>
		{
			{ "playerBody", this },
			{ "oldIsExtendedRight", oldIsExtendedRight },
			{ "newIsExtendedRight", newIsExtendedRight }
		});
	}

	public void Server_Teleport(Vector3 position, Quaternion rotation)
	{
		transform.position = position;
		transform.rotation = rotation;
		Rigidbody.position = position;
		Rigidbody.rotation = rotation;
		Rigidbody.linearVelocity = Vector3.zero;
		Rigidbody.angularVelocity = Vector3.zero;
		Stick.Server_Teleport(position, rotation);
	}

	public void Server_Freeze(RigidbodyConstraints contstraints = RigidbodyConstraints.FreezeAll)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Rigidbody.constraints = contstraints;
		}
	}

	public void Server_Unfreeze()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Rigidbody.constraints = RigidbodyConstraints.None;
		}
	}

	private void Server_UpdateAudio()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			if (Rigidbody.constraints != RigidbodyConstraints.None)
			{
				windAudioSource.Server_SetVolume(0f);
				iceAudioSource.Server_SetVolume(0f);
				return;
			}
			float time = Mathf.Min(Movement.NormalizedMaximumSpeed, 1f);
			windAudioSource.Server_SetVolume(windVolumeCurve.Evaluate(time));
			float num;
			if (IsGrounded)
			{
				if (IsStopping.Value)
				{
					num = 3f;
				}
				else if (IsSliding.Value)
				{
					num = 1.5f;
				}
				else
				{
					num = (Skate.IsTractionLost ? 2f : 1f);
				}
			}
			else
			{
				num = 0f;
			}
			float num2;
			if (IsStopping.Value)
			{
				num2 = 3f;
			}
			else if (IsSliding.Value)
			{
				num2 = 1.5f;
			}
			else
			{
				num2 = (Skate.IsTractionLost ? 2f : 1f);
			}
			float time2 = Mathf.Min(Movement.NormalizedMaximumSpeed, 1f);
			iceAudioSource.Server_SetVolume(iceVolumeCurve.Evaluate(time2) * num);
			float time3 = Mathf.Min(Movement.NormalizedMaximumSpeed, 1f);
			iceAudioSource.Server_SetPitch(icePitchCurve.Evaluate(time3) * num2);
		}
	}

	private void Server_OnCollisionDeferred(GameObject gameObject, float force)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			gruntAudioSource.Server_Play(gruntVolumeCurve.Evaluate(force), gruntPitchCurve.Evaluate(force), isOneShot: true, -1, 0f, randomClip: true);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (NetworkManager.Singleton.IsServer && collision.gameObject.layer == LayerMask.NameToLayer("Player") && collision.gameObject.TryGetComponent<PlayerBody>(out var component))
		{
			float normalizedMaximumSpeed = Movement.NormalizedMaximumSpeed;
			float normalizedMaximumSpeed2 = component.Movement.NormalizedMaximumSpeed;
			float collisionForce = Utils.GetCollisionForce(collision);
			if (!(Speed.Value < tackleSpeedThreshold) && !(normalizedMaximumSpeed < normalizedMaximumSpeed2) && !(collisionForce < tackleForceThreshold) && !IsGrounded && IsBalanced && !HasSlipped && !HasFallen.Value)
			{
				component.OnSlip();
				Vector3 vector = Vector3.ClampMagnitude(collision.relativeVelocity, tackleBounceMaximumMagnitude);
				component.Rigidbody.AddForceAtPosition(-vector * tackleForceMultiplier, Rigidbody.worldCenterOfMass + transform.up * 0.5f, ForceMode.VelocityChange);
			}
		}
	}

	protected override void __initializeVariables()
	{
		if (PlayerReference == null)
		{
			throw new Exception("PlayerBody.PlayerReference cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		PlayerReference.Initialize(this);
		__nameNetworkVariable(PlayerReference, "PlayerReference");
		NetworkVariableFields.Add(PlayerReference);
		if (Stamina == null)
		{
			throw new Exception("PlayerBody.Stamina cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		Stamina.Initialize(this);
		__nameNetworkVariable(Stamina, "Stamina");
		NetworkVariableFields.Add(Stamina);
		if (Speed == null)
		{
			throw new Exception("PlayerBody.Speed cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		Speed.Initialize(this);
		__nameNetworkVariable(Speed, "Speed");
		NetworkVariableFields.Add(Speed);
		if (IsSprinting == null)
		{
			throw new Exception("PlayerBody.IsSprinting cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		IsSprinting.Initialize(this);
		__nameNetworkVariable(IsSprinting, "IsSprinting");
		NetworkVariableFields.Add(IsSprinting);
		if (IsSliding == null)
		{
			throw new Exception("PlayerBody.IsSliding cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		IsSliding.Initialize(this);
		__nameNetworkVariable(IsSliding, "IsSliding");
		NetworkVariableFields.Add(IsSliding);
		if (IsStopping == null)
		{
			throw new Exception("PlayerBody.IsStopping cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		IsStopping.Initialize(this);
		__nameNetworkVariable(IsStopping, "IsStopping");
		NetworkVariableFields.Add(IsStopping);
		if (IsExtendedLeft == null)
		{
			throw new Exception("PlayerBody.IsExtendedLeft cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		IsExtendedLeft.Initialize(this);
		__nameNetworkVariable(IsExtendedLeft, "IsExtendedLeft");
		NetworkVariableFields.Add(IsExtendedLeft);
		if (IsExtendedRight == null)
		{
			throw new Exception("PlayerBody.IsExtendedRight cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		IsExtendedRight.Initialize(this);
		__nameNetworkVariable(IsExtendedRight, "IsExtendedRight");
		NetworkVariableFields.Add(IsExtendedRight);
		if (HasFallen == null)
		{
			throw new Exception("PlayerBody.HasFallen cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		HasFallen.Initialize(this);
		__nameNetworkVariable(HasFallen, "HasFallen");
		NetworkVariableFields.Add(HasFallen);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected override string __getTypeName()
	{
		return "PlayerBody";
	}
}
