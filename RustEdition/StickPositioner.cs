using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StickPositioner : NetworkBehaviour
{
	[Header("Settings")]
	[SerializeField]
	private float proportionalGain = 0.75f;

	[SerializeField]
	private float integralGain = 5f;

	[SerializeField]
	private float integralSaturation = 5f;

	[SerializeField]
	private DerivativeMeasurement derivativeMeasurement;

	[SerializeField]
	private float derivativeGain;

	[SerializeField]
	private float derivativeSmoothing;

	[SerializeField]
	private float outputMin = -15f;

	[SerializeField]
	private float outputMax = 15f;

	[Space(20f)]
	[SerializeField]
	private float maximumReach = 2.5f;

	[Space(20f)]
	[SerializeField]
	private float bladeTargetRotationThreshold = 25f;

	[SerializeField]
	private float bladeTargetMaxAngle = 45f;

	[Space(20f)]
	[SerializeField]
	private LayerMask raycastLayerMask;

	[Space(20f)]
	[SerializeField]
	private float raycastOriginPadding = 0.2f;

	[Space(20f)]
	[SerializeField]
	private bool applySoftCollision = true;

	[SerializeField]
	private float softCollisionForce = 1f;

	[Space(20f)]
	[SerializeField]
	private AnimationCurve windVolumeCurve;

	[SerializeField]
	private AnimationCurve windPitchCurve;

	[SerializeField]
	private AnimationCurve iceHitVolumeCurve;

	[SerializeField]
	private AnimationCurve iceHitPitchCurve;

	[SerializeField]
	private AnimationCurve iceDragVolumeCurve;

	[SerializeField]
	private AnimationCurve iceDragPitchCurve;

	[Header("References")]
	[SerializeField]
	private GameObject raycastOrigin;

	[SerializeField]
	private GameObject bladeTargetFocusPoint;

	[SerializeField]
	private GameObject bladeTarget;

	[SerializeField]
	private GameObject shaftTarget;

	[Space(20f)]
	[SerializeField]
	private SynchronizedAudio windAudioSource;

	[SerializeField]
	private SynchronizedAudio iceHitAudioSource;

	[SerializeField]
	private SynchronizedAudio iceDragAudioSource;

	[SerializeField]
	private float iceDragVolumeFallOffSpeed = 10f;

	[HideInInspector]
	public NetworkVariable<NetworkObjectReference> PlayerReference;

	[HideInInspector]
	public Player Player;

	[HideInInspector]
	public bool IsGrounded;

	[HideInInspector]
	public PlayerHandedness Handedness;

	private bool isNetworkVariablesInitialized;

	private Vector3 lastBladeTargetPosition = Vector3.zero;

	private Vector3 bladeTargetVelocity = Vector3.zero;

	private Vector3 bladeTargetFocusPointInitialLocalPosition = Vector3.zero;

	private Vector3 raycastOriginInitialLocalPosition = Vector3.zero;

	private Vector2 raycastOriginAngleTarget = Vector2.zero;

	private Vector2 raycastOriginAngle = Vector2.zero;

	private Vector2 raycastOriginAngleDelta = Vector3.zero;

	private float iceDragVolume;

	private float iceDragPitch;

	private Vector3PIDController pidController = new Vector3PIDController();

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
	public Vector3 BladeTargetPosition => bladeTarget.transform.position;

	[HideInInspector]
	public Vector3 BladeTargetVelocity => bladeTargetVelocity;

	[HideInInspector]
	public Vector3 ShaftTargetPosition => shaftTarget.transform.position;

	[HideInInspector]
	public Vector3 RaycastOriginPosition => raycastOrigin.transform.position;

	private Vector3 BladeTargetFocusPointInitialLocalPosition
	{
		get
		{
			if (Handedness != PlayerHandedness.Left)
			{
				return bladeTargetFocusPointInitialLocalPosition;
			}
			return new Vector3(0f - bladeTargetFocusPointInitialLocalPosition.x, bladeTargetFocusPointInitialLocalPosition.y, bladeTargetFocusPointInitialLocalPosition.z);
		}
	}

	private Vector3 RaycastOriginInitialLocalPosition
	{
		get
		{
			if (Handedness != PlayerHandedness.Left)
			{
				return raycastOriginInitialLocalPosition;
			}
			return new Vector3(0f - raycastOriginInitialLocalPosition.x, raycastOriginInitialLocalPosition.y, raycastOriginInitialLocalPosition.z);
		}
	}

	private void Awake()
	{
		bladeTargetFocusPointInitialLocalPosition = bladeTargetFocusPoint.transform.localPosition;
		raycastOriginInitialLocalPosition = raycastOrigin.transform.localPosition;
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
		EventManager.TriggerEvent("Event_Everyone_OnStickPositionerSpawned", new Dictionary<string, object> { { "stickPositioner", this } });
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

	private void FixedUpdate()
	{
		if ((bool)Player)
		{
			pidController.proportionalGain = proportionalGain;
			pidController.integralGain = integralGain;
			pidController.integralSaturation = integralSaturation;
			pidController.derivativeMeasurement = derivativeMeasurement;
			pidController.derivativeGain = derivativeGain;
			pidController.derivativeSmoothing = derivativeSmoothing;
			pidController.outputMin = outputMin;
			pidController.outputMax = outputMax;
			raycastOriginAngleTarget = Player.PlayerInput.StickRaycastOriginAngleInput.ServerValue;
			ShootPaddingRay();
			RotateRaycastOrigin();
			ShootRaycast();
			UpdateAudio();
		}
	}

	private void ShootPaddingRay()
	{
		Vector3 vector = new Vector3(0f, RaycastOriginInitialLocalPosition.y, 0f);
		Vector3 normalized = (RaycastOriginInitialLocalPosition - vector).normalized;
		float num = Vector3.Distance(vector, RaycastOriginInitialLocalPosition) + raycastOriginPadding;
		Vector3 vector2 = transform.TransformPoint(vector);
		Vector3 vector3 = transform.TransformDirection(normalized);
		Debug.DrawRay(vector2, vector3 * num, Color.yellow);
		if (Physics.Raycast(vector2, vector3, out var hitInfo, num, raycastLayerMask))
		{
			raycastOrigin.transform.localPosition = vector + normalized * (hitInfo.distance - raycastOriginPadding);
		}
		else
		{
			raycastOrigin.transform.localPosition = RaycastOriginInitialLocalPosition;
		}
	}

	private void RotateRaycastOrigin()
	{
		raycastOriginAngleDelta = pidController.Update(Time.fixedDeltaTime, raycastOriginAngle, raycastOriginAngleTarget);
		raycastOriginAngle += raycastOriginAngleDelta * Time.fixedDeltaTime;
		raycastOrigin.transform.localRotation = Quaternion.Euler(raycastOriginAngle);
	}

	private void ShootRaycast()
	{
		Vector3 hitPosition;
		if (Physics.Raycast(raycastOrigin.transform.position, raycastOrigin.transform.forward, out var hitInfo, maximumReach, raycastLayerMask))
		{
			OnGrounded(hitInfo.transform.gameObject);
			Vector3 vector = raycastOrigin.transform.position + raycastOrigin.transform.forward * maximumReach;
			Vector3 vector2 = Vector3.Scale(Utils.Vector3Abs(hitInfo.normal), hitInfo.point);
			Vector3 vector3 = vector - Vector3.Scale(Utils.Vector3Abs(hitInfo.normal), vector) + vector2;
			Debug.DrawRay(raycastOrigin.transform.position, raycastOrigin.transform.forward * hitInfo.distance, Color.red);
			Vector3 normalized = (vector3 - raycastOrigin.transform.position).normalized;
			if (Physics.Raycast(raycastOrigin.transform.position, normalized, out var hitInfo2, maximumReach, raycastLayerMask))
			{
				Vector3 vector4 = raycastOrigin.transform.position + raycastOrigin.transform.forward * maximumReach;
				Vector3 vector5 = Vector3.Scale(Utils.Vector3Abs(hitInfo2.normal), hitInfo2.point);
				Vector3 vector6 = vector4 - Vector3.Scale(Utils.Vector3Abs(hitInfo2.normal), vector4) + vector5;
				if (hitInfo.normal == Vector3.up && hitInfo2.normal == Vector3.up)
				{
					hitPosition = vector3;
				}
				else if (hitInfo.normal == Vector3.up && hitInfo2.normal != Vector3.up)
				{
					hitPosition = vector6;
					hitPosition.y = Mathf.Max(0f, hitPosition.y);
				}
				else
				{
					hitPosition = vector3;
					hitPosition.y = Mathf.Max(0f, hitPosition.y);
				}
				Debug.DrawRay(raycastOrigin.transform.position, normalized * hitInfo2.distance, Color.blue);
			}
			else
			{
				hitPosition = hitInfo.point;
			}
			ApplySoftCollision(hitInfo, hitPosition);
		}
		else
		{
			OnUngrounded();
			hitPosition = raycastOrigin.transform.position + raycastOrigin.transform.forward * maximumReach;
			Debug.DrawRay(raycastOrigin.transform.position, raycastOrigin.transform.forward * maximumReach, Color.red);
		}
		PositionBladeTarget(hitPosition);
		PositionBladeTargetFocusPoint(hitPosition);
		RotateBladeTargetFocusPoint();
	}

	private void PositionBladeTarget(Vector3 hitPosition)
	{
		bladeTarget.transform.position = hitPosition;
		bladeTarget.transform.rotation = Quaternion.LookRotation(bladeTarget.transform.position - bladeTargetFocusPoint.transform.position);
		bladeTargetVelocity = (bladeTarget.transform.position - lastBladeTargetPosition) / Time.fixedDeltaTime;
		lastBladeTargetPosition = bladeTarget.transform.position;
	}

	private void PositionBladeTargetFocusPoint(Vector3 hitPosition)
	{
		float num = Vector3.Distance(raycastOrigin.transform.position, new Vector3(hitPosition.x, raycastOrigin.transform.position.y, hitPosition.z));
		float num2 = maximumReach - num;
		Vector3 vector = raycastOrigin.transform.localPosition - transform.InverseTransformPoint(hitPosition);
		Vector3 normalized = new Vector3(vector.x, 0f, vector.z).normalized;
		Vector3 vector2 = transform.TransformDirection(normalized);
		Debug.DrawRay(bladeTargetFocusPoint.transform.position, -vector2 * num2, Color.grey);
		Vector3 vector3 = normalized * num2;
		bladeTargetFocusPoint.transform.localPosition = BladeTargetFocusPointInitialLocalPosition + vector3;
	}

	private void RotateBladeTargetFocusPoint()
	{
		PlayerInput playerInput = Player.PlayerInput;
		if ((bool)playerInput)
		{
			float num = Mathf.Lerp(1f, 0f, (playerInput.MaximumStickRaycastOriginAngle.x - raycastOriginAngle.x) / bladeTargetRotationThreshold);
			num *= (float)((Handedness != PlayerHandedness.Left) ? 1 : (-1));
			bladeTargetFocusPoint.transform.localPosition = Utils.RotatePointAroundPivot(bladeTargetFocusPoint.transform.localPosition, bladeTarget.transform.localPosition, new Vector3(0f, bladeTargetMaxAngle * num, 0f));
		}
	}

	public void PrepareShaftTarget(Stick stick)
	{
		shaftTarget.transform.localPosition = stick.ShaftHandleLocalPosition - stick.BladeHandleLocalPosition;
	}

	private void ApplySoftCollision(RaycastHit hit, Vector3 hitPosition)
	{
		if (applySoftCollision && (bool)PlayerBody && !PlayerBody.SynchronizedObject.IsDrivenExternally && hit.collider.CompareTag("Soft Collider"))
		{
			float num = maximumReach - hit.distance;
			float magnitude = Vector3.Cross(hit.normal, raycastOrigin.transform.forward).magnitude;
			float num2 = 1f - magnitude;
			Debug.DrawRay(hitPosition, hit.normal * num * softCollisionForce, Color.green);
			PlayerBody.Rigidbody.AddForceAtPosition(hit.normal * num * (softCollisionForce * num2), hitPosition, ForceMode.Acceleration);
		}
	}

	private void UpdateAudio()
	{
		windAudioSource.transform.position = BladeTargetPosition;
		iceHitAudioSource.transform.position = BladeTargetPosition;
		iceDragAudioSource.transform.position = BladeTargetPosition;
		if ((bool)PlayerBody && PlayerBody.Rigidbody.constraints != RigidbodyConstraints.None)
		{
			windAudioSource.Server_SetVolume(0f);
			iceDragAudioSource.Server_SetVolume(0f);
			return;
		}
		float num = (IsGrounded ? iceDragVolumeCurve.Evaluate(BladeTargetVelocity.magnitude) : 0f);
		if (num > iceDragVolume)
		{
			iceDragVolume = num;
		}
		else
		{
			iceDragVolume = Mathf.Lerp(iceDragVolume, num, Time.fixedDeltaTime * iceDragVolumeFallOffSpeed);
		}
		iceDragPitch = iceDragPitchCurve.Evaluate(BladeTargetVelocity.magnitude);
		iceDragAudioSource.Server_SetVolume(iceDragVolume);
		iceDragAudioSource.Server_SetPitch(iceDragPitch);
		float volume = windVolumeCurve.Evaluate(raycastOriginAngleDelta.magnitude);
		float pitch = windPitchCurve.Evaluate(raycastOriginAngleDelta.magnitude);
		windAudioSource.Server_SetVolume(volume);
		windAudioSource.Server_SetPitch(pitch);
	}

	private void OnGrounded(GameObject ground)
	{
		if (!IsGrounded)
		{
			if (ground.layer == LayerMask.NameToLayer("Ice"))
			{
				float volume = iceHitVolumeCurve.Evaluate(Mathf.Abs(raycastOriginAngleDelta.x));
				float pitch = iceHitPitchCurve.Evaluate(Mathf.Abs(raycastOriginAngleDelta.x));
				iceHitAudioSource.Server_Play(volume, pitch, isOneShot: true, -1, 0f, randomClip: true);
			}
			IsGrounded = true;
		}
	}

	private void OnUngrounded()
	{
		if (IsGrounded)
		{
			IsGrounded = false;
		}
	}

	private void HandlePlayerReference(NetworkObjectReference oldPlayerReference = default(NetworkObjectReference), NetworkObjectReference newPlayerReference = default(NetworkObjectReference))
	{
		Player player = (oldPlayerReference.TryGet(out var networkObject) ? networkObject.GetComponent<Player>() : null);
		Player player2 = (newPlayerReference.TryGet(out var networkObject2) ? networkObject2.GetComponent<Player>() : null);
		if ((bool)player)
		{
			player.StickPositioner = null;
		}
		if ((bool)player2)
		{
			Player = player2;
			Player.StickPositioner = this;
		}
		else
		{
			Player = null;
		}
		if ((bool)Player)
		{
			if ((bool)Player.Stick)
			{
				PrepareShaftTarget(Player.Stick);
			}
			Handedness = Player.Handedness.Value;
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
			throw new Exception("StickPositioner.PlayerReference cannot be null. All NetworkVariableBase instances must be initialized.");
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
		return "StickPositioner";
	}
}
