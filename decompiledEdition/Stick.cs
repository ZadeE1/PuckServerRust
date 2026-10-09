using System;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class Stick : NetworkBehaviour
{
	private static readonly Logger Logger = new Logger("Stick");

	[Header("Settings")]
	[SerializeField]
	private float bladeAngleStep = 12.5f;

	[Space(20f)]
	[SerializeField]
	private bool transferAngularVelocity = true;

	[SerializeField]
	private float angularVelocityTransferMultiplier = 0.25f;

	[Space(20f)]
	[SerializeField]
	private float shaftHandleProportionalGain = 500f;

	[SerializeField]
	private float shaftHandleIntegralGain;

	[SerializeField]
	private float shaftHandleIntegralSaturation;

	[SerializeField]
	private float shaftHandleDerivativeGain = 20f;

	[SerializeField]
	private float shaftHandleDerivativeSmoothing = 0.1f;

	[SerializeField]
	private float minShaftHandleProportionalGainMultiplier = 0.25f;

	[Space(20f)]
	[SerializeField]
	private float bladeHandleProportionalGain = 500f;

	[SerializeField]
	private float bladeHandleIntegralGain;

	[SerializeField]
	private float bladeHandleIntegralSaturation;

	[SerializeField]
	private float bladeHandleDerivativeGain = 20f;

	[SerializeField]
	private float bladeHandleDerivativeSmoothing = 0.1f;

	[Space(20f)]
	[SerializeField]
	private float linearVelocityTransferMultiplier = 0.25f;

	[Header("References")]
	[SerializeField]
	private GameObject shaftHandle;

	[SerializeField]
	private GameObject bladeHandle;

	[SerializeField]
	private GameObject rotationContainer;

	[SerializeField]
	private StickMesh stickMesh;

	[HideInInspector]
	public NetworkVariable<NetworkObjectReference> PlayerReference;

	[HideInInspector]
	public Player Player;

	[HideInInspector]
	public Rigidbody Rigidbody;

	[HideInInspector]
	public SynchronizedObject SynchronizedObject;

	[HideInInspector]
	public NetworkObjectCollisionRecorder NetworkObjectCollisionRecorder;

	[HideInInspector]
	public float Length;

	private bool isNetworkVariablesInitialized;

	private Vector3PIDController shaftHandlePIDController = new Vector3PIDController();

	private Vector3PIDController bladeHandlePIDController = new Vector3PIDController();

	private float shaftHandleProportionalGainMultiplier = 1f;

	private float bladeHandleProportionalGainMultiplier = 1f;

	private Vector3 shaftHandleForce = Vector3.zero;

	private Vector3 bladeHandleForce = Vector3.zero;

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
	public StickPositioner StickPositioner
	{
		get
		{
			if (!(Player == null))
			{
				return Player.StickPositioner;
			}
			return null;
		}
	}

	[HideInInspector]
	public StickMesh StickMesh => stickMesh;

	[HideInInspector]
	public Vector3 ShaftHandleLocalPosition => shaftHandle.transform.localPosition;

	[HideInInspector]
	public Vector3 BladeHandleLocalPosition => bladeHandle.transform.localPosition;

	[HideInInspector]
	public Vector3 ShaftHandlePosition => shaftHandle.transform.position;

	[HideInInspector]
	public Vector3 BladeHandlePosition => bladeHandle.transform.position;

	[HideInInspector]
	public Quaternion BladeHandleRotation => bladeHandle.transform.rotation;

	[HideInInspector]
	public float BladeAngle
	{
		get
		{
			if (!(Player == null))
			{
				return (float)Player.PlayerInput.BladeAngleInput.ServerValue * bladeAngleStep;
			}
			return 0f;
		}
	}

	private void Awake()
	{
		Rigidbody = GetComponent<Rigidbody>();
		SynchronizedObject = GetComponent<SynchronizedObject>();
		NetworkObjectCollisionRecorder = GetComponent<NetworkObjectCollisionRecorder>();
		Length = Vector3.Distance(ShaftHandlePosition, BladeHandlePosition);
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
		EventManager.TriggerEvent("Event_Everyone_OnStickSpawned", new Dictionary<string, object> { { "stick", this } });
		base.OnNetworkPostSpawn();
	}

	public override void OnNetworkDespawn()
	{
		EventManager.TriggerEvent("Event_Everyone_OnStickDespawned", new Dictionary<string, object> { { "stick", this } });
		NetworkVariable<NetworkObjectReference> playerReference = PlayerReference;
		playerReference.OnValueChanged = (NetworkVariable<NetworkObjectReference>.OnValueChangedDelegate)Delegate.Remove(playerReference.OnValueChanged, new NetworkVariable<NetworkObjectReference>.OnValueChangedDelegate(OnPlayerReferenceChanged));
		base.OnNetworkDespawn();
	}

	public override void OnDestroy()
	{
		transform.DOKill();
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
		Server_FixedUpdate();
		if ((bool)Player)
		{
			float angle = (float)Player.PlayerInput.BladeAngleInput.ServerValue * bladeAngleStep;
			rotationContainer.transform.localRotation = Quaternion.AngleAxis(angle, Vector3.forward);
		}
	}

	public void ApplyCustomizations()
	{
		if (Player.Team != PlayerTeam.None && Player.Role != PlayerRole.None)
		{
			SetSkinID(Player.GetPlayerStickSkinID(), Player.Team);
			SetShaftTapeID(Player.GetPlayerStickShaftTapeID());
			SetBladeTapeID(Player.GetPlayerStickBladeTapeID());
		}
	}

	public void SetSkinID(int skinID, PlayerTeam team)
	{
		stickMesh.SetSkinID(skinID, team);
	}

	public void SetShaftTapeID(int shaftTapeID)
	{
		stickMesh.SetShaftTapeID(shaftTapeID);
	}

	public void SetBladeTapeID(int bladeTapeID)
	{
		stickMesh.SetBladeTapeID(bladeTapeID);
	}

	private void OnCollisionStay(Collision collision)
	{
		Server_OnCollisionStay(collision);
	}

	private void HandlePlayerReference(NetworkObjectReference oldPlayerReference = default(NetworkObjectReference), NetworkObjectReference newPlayerReference = default(NetworkObjectReference))
	{
		Player player = (oldPlayerReference.TryGet(out var networkObject) ? networkObject.GetComponent<Player>() : null);
		Player player2 = (newPlayerReference.TryGet(out var networkObject2) ? networkObject2.GetComponent<Player>() : null);
		if ((bool)player)
		{
			player.Stick = null;
		}
		if ((bool)player2)
		{
			Player = player2;
			Player.Stick = this;
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

	private void OnPlayerReferenceChanged(NetworkObjectReference oldPlayerReference, NetworkObjectReference newPlayerReference)
	{
		HandlePlayerReference(oldPlayerReference, newPlayerReference);
		EventManager.TriggerEvent("Event_Everyone_OnStickPlayerReferenceChanged", new Dictionary<string, object>
		{
			{ "stick", this },
			{ "oldPlayerReference", oldPlayerReference },
			{ "newPlayerReference", newPlayerReference }
		});
	}

	private void Server_FixedUpdate()
	{
		if (NetworkManager.Singleton.IsServer && (bool)Player && !SynchronizedObject.IsDrivenExternally)
		{
			Server_UpdatePidControllers(Time.fixedDeltaTime);
			Server_ApplyForces();
			Server_ClearRollVelocity();
			Server_ResetRoll();
			Server_ApplyFeedbackForces();
			bladeHandleProportionalGainMultiplier = 1f;
		}
	}

	private void Server_UpdatePidControllers(float deltaTime)
	{
		if (NetworkManager.Singleton.IsServer && (bool)StickPositioner)
		{
			shaftHandlePIDController.proportionalGain = shaftHandleProportionalGain * shaftHandleProportionalGainMultiplier;
			shaftHandlePIDController.integralGain = shaftHandleIntegralGain;
			shaftHandlePIDController.integralSaturation = shaftHandleIntegralSaturation;
			shaftHandlePIDController.derivativeGain = shaftHandleDerivativeGain;
			shaftHandlePIDController.derivativeSmoothing = shaftHandleDerivativeSmoothing;
			bladeHandlePIDController.proportionalGain = bladeHandleProportionalGain * bladeHandleProportionalGainMultiplier;
			bladeHandlePIDController.integralGain = bladeHandleIntegralGain;
			bladeHandlePIDController.integralSaturation = bladeHandleIntegralSaturation;
			bladeHandlePIDController.derivativeGain = bladeHandleDerivativeGain;
			bladeHandlePIDController.derivativeSmoothing = bladeHandleDerivativeSmoothing;
			shaftHandleForce = shaftHandlePIDController.Update(deltaTime, ShaftHandlePosition, StickPositioner.ShaftTargetPosition);
			bladeHandleForce = bladeHandlePIDController.Update(deltaTime, BladeHandlePosition, StickPositioner.BladeTargetPosition);
		}
	}

	private void Server_ApplyForces()
	{
		if (NetworkManager.Singleton.IsServer && (bool)PlayerBody)
		{
			Rigidbody.AddForceAtPosition(PlayerBody.Rigidbody.GetPointVelocity(shaftHandle.transform.position) * linearVelocityTransferMultiplier * Time.fixedDeltaTime, shaftHandle.transform.position, ForceMode.VelocityChange);
			Rigidbody.AddForceAtPosition(PlayerBody.Rigidbody.GetPointVelocity(bladeHandle.transform.position) * linearVelocityTransferMultiplier * Time.fixedDeltaTime, bladeHandle.transform.position, ForceMode.VelocityChange);
			Rigidbody.AddForceAtPosition(shaftHandleForce * Time.fixedDeltaTime, ShaftHandlePosition, ForceMode.VelocityChange);
			Rigidbody.AddForceAtPosition(bladeHandleForce * Time.fixedDeltaTime, BladeHandlePosition, ForceMode.VelocityChange);
		}
	}

	private void Server_ClearRollVelocity()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Vector3 direction = transform.InverseTransformVector(Rigidbody.angularVelocity);
			direction.z = 0f;
			Rigidbody.angularVelocity = transform.TransformDirection(direction);
		}
	}

	private void Server_ResetRoll()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Vector3 vector = Utils.WrapEulerAngles(transform.eulerAngles);
			Quaternion rotation = Quaternion.Euler(new Vector3(vector.x, vector.y, 0f));
			Rigidbody.MoveRotation(rotation);
		}
	}

	private void Server_ApplyFeedbackForces()
	{
		if (NetworkManager.Singleton.IsServer && (bool)PlayerBody)
		{
			Vector3 vector = Vector3.Scale(Rigidbody.angularVelocity, new Vector3(0.5f, 1f, 0f)) * angularVelocityTransferMultiplier;
			if (transferAngularVelocity)
			{
				PlayerBody.Rigidbody.AddTorque(-vector, ForceMode.Acceleration);
			}
		}
	}

	public void Server_Teleport(Vector3 position, Quaternion rotation)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Rigidbody.position = position;
			Rigidbody.rotation = rotation;
			Rigidbody.linearVelocity = Vector3.zero;
			Rigidbody.angularVelocity = Vector3.zero;
			shaftHandlePIDController.Reset();
			bladeHandlePIDController.Reset();
		}
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

	private void Server_OnCollisionStay(Collision collision)
	{
		if (NetworkManager.Singleton.IsServer && collision.gameObject.TryGetComponent<Stick>(out var component) && collision.contactCount != 0)
		{
			ContactPoint contact = collision.GetContact(0);
			Collider thisCollider = contact.thisCollider;
			Collider otherCollider = contact.otherCollider;
			if (thisCollider.CompareTag("Stick Blade") && otherCollider.CompareTag("Stick Shaft"))
			{
				Vector3 point = contact.point;
				float num = Mathf.Clamp(Vector3.Distance(component.ShaftHandlePosition, point) / Length, minShaftHandleProportionalGainMultiplier, 1f);
				bladeHandleProportionalGainMultiplier = num;
			}
		}
	}

	protected override void __initializeVariables()
	{
		if (PlayerReference == null)
		{
			throw new Exception("Stick.PlayerReference cannot be null. All NetworkVariableBase instances must be initialized.");
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
		return "Stick";
	}
}
