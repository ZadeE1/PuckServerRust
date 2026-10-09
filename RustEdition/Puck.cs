using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class Puck : NetworkBehaviour
{
	[Header("Settings")]
	[SerializeField]
	private float maxSpeed = 30f;

	[SerializeField]
	private float maxAngularSpeed = 30f;

	[Space(20f)]
	[SerializeField]
	private Vector3 stickTensor = new Vector3(0.006f, 0.002f, 0.006f);

	[SerializeField]
	private Vector3 defaultTensor = new Vector3(0.002f, 0.002f, 0.002f);

	[Space(20f)]
	[SerializeField]
	private float groundedCheckSphereRadius = 0.075f;

	[SerializeField]
	private LayerMask groundedCheckSphereLayerMask;

	[Space(20f)]
	[SerializeField]
	private Vector3 groundedCenterOfMass = new Vector3(0f, -0.01f, 0f);

	[Space(20f)]
	[SerializeField]
	private float goalNetLinearVelocityMaximumMagnitude = 2f;

	[SerializeField]
	private float goalNetAngularVelocityMaximumMagnitude = 2f;

	[Space(20f)]
	[SerializeField]
	private AnimationCurve hitIceVolumeCurve;

	[SerializeField]
	private AnimationCurve hitIcePitchCurve;

	[SerializeField]
	private AnimationCurve hitBoardsVolumeCurve;

	[SerializeField]
	private AnimationCurve hitBoardsPitchCurve;

	[SerializeField]
	private AnimationCurve hitGoalPostVolumeCurve;

	[SerializeField]
	private AnimationCurve hitGoalPostPitchCurve;

	[SerializeField]
	private AnimationCurve windVolumeCurve;

	[SerializeField]
	private AnimationCurve windPitchCurve;

	[Header("References")]
	[SerializeField]
	private PuckElevationIndicator verticalityIndicator;

	[SerializeField]
	private SphereCollider netSphereCollider;

	[SerializeField]
	private Collider stickCollider;

	[SerializeField]
	private Collider iceCollider;

	[Space(20f)]
	[SerializeField]
	private SynchronizedAudio hitIceAudioSource;

	[SerializeField]
	private SynchronizedAudio hitBoardsAudioSource;

	[SerializeField]
	private SynchronizedAudio hitGoalPostAudioSource;

	[SerializeField]
	private SynchronizedAudio windAudioSource;

	[HideInInspector]
	public NetworkVariable<bool> IsReplay;

	[HideInInspector]
	public Rigidbody Rigidbody;

	[HideInInspector]
	public SynchronizedObject SynchronizedObject;

	[HideInInspector]
	public NetworkObjectCollisionRecorder NetworkObjectCollisionRecorder;

	[HideInInspector]
	public ReplayTouchTracker ReplayTouchTracker;

	[HideInInspector]
	public CollisionRecorder CollisionRecorder;

	[HideInInspector]
	public float Speed;

	[HideInInspector]
	public float AngularSpeed;

	private bool isNetworkVariablesInitialized;

	private int lastTensorSel = -1;

	[HideInInspector]
	public float PredictedSpeed => SynchronizedObject.PredictedLinearVelocity.magnitude;

	[HideInInspector]
	public float PredictedAngularSpeed => SynchronizedObject.PredictedAngularVelocity.magnitude;

	[HideInInspector]
	public float ShotSpeed { get; private set; }

	[HideInInspector]
	public bool IsGrounded { get; private set; }

	[HideInInspector]
	public SphereCollider NetSphereCollider => netSphereCollider;

	[HideInInspector]
	public Collider StickCollider => stickCollider;

	[HideInInspector]
	public Collider IceCollider => iceCollider;

	[HideInInspector]
	public Stick TouchingStick { get; private set; }

	[HideInInspector]
	public bool IsTouchingStick => TouchingStick != null;

	[HideInInspector]
	public float MaxSpeed => maxSpeed;

	[HideInInspector]
	public float MaxAngularSpeed => maxAngularSpeed;

	private void Awake()
	{
		Rigidbody = GetComponent<Rigidbody>();
		SynchronizedObject = GetComponent<SynchronizedObject>();
		NetworkObjectCollisionRecorder = GetComponent<NetworkObjectCollisionRecorder>();
		ReplayTouchTracker = GetComponent<ReplayTouchTracker>();
		CollisionRecorder = GetComponent<CollisionRecorder>();
		CollisionRecorder collisionRecorder = CollisionRecorder;
		collisionRecorder.CollisionDeferred = (Action<GameObject, float>)Delegate.Combine(collisionRecorder.CollisionDeferred, new Action<GameObject, float>(OnCollisionDeferred));
		NetSphereCollider.enabled = false;
		NativeAudio.Init(windVolumeCurve, windPitchCurve);
	}

	private void FixedUpdate()
	{
		Speed = Rigidbody.linearVelocity.magnitude;
		AngularSpeed = Rigidbody.angularVelocity.magnitude;
		IsGrounded = Physics.CheckSphere(transform.position, groundedCheckSphereRadius, groundedCheckSphereLayerMask);
		if (IsGrounded)
		{
			Rigidbody.centerOfMass = transform.TransformVector(groundedCenterOfMass);
		}
		else
		{
			Rigidbody.centerOfMass = Vector3.zero;
		}
		float newRadius;
		int radiusChanged = NativeAudio.EvaluateRadius(IsGrounded, PredictedSpeed, NetSphereCollider.radius, Time.fixedDeltaTime, out newRadius);
		if (radiusChanged < 0)
		{
			float num = (IsGrounded ? 0f : Mathf.Clamp(PredictedSpeed * 0.025f, 0.15f, 0.75f));
			if (NetSphereCollider.radius < num)
			{
				NetSphereCollider.radius = num;
			}
			else if (NetSphereCollider.radius > num)
			{
				NetSphereCollider.radius = Mathf.Lerp(NetSphereCollider.radius, num, Time.fixedDeltaTime * 5f);
			}
		}
		else if (radiusChanged > 0)
		{
			NetSphereCollider.radius = newRadius;
		}
		int tensorSel = (IsTouchingStick ? 1 : 0);
		if (tensorSel != lastTensorSel)
		{
			lastTensorSel = tensorSel;
			if (IsTouchingStick)
			{
				Server_UpdateStickTensor(stickTensor, Quaternion.identity);
				TouchingStick = null;
			}
			else
			{
				Server_UpdateStickTensor(defaultTensor, Quaternion.identity);
			}
		}
		else if (IsTouchingStick)
		{
			TouchingStick = null;
		}
		Server_UpdateAudio();
	}

	protected override void OnNetworkPreSpawn(ref NetworkManager networkManager)
	{
		InitializeNetworkVariables();
		base.OnNetworkPreSpawn(ref networkManager);
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
	}

	protected override void OnNetworkPostSpawn()
	{
		EventManager.TriggerEvent("Event_Everyone_OnPuckSpawned", new Dictionary<string, object> { { "puck", this } });
		base.OnNetworkPostSpawn();
	}

	public override void OnNetworkDespawn()
	{
		EventManager.TriggerEvent("Event_Everyone_OnPuckDespawned", new Dictionary<string, object> { { "puck", this } });
		base.OnNetworkDespawn();
	}

	public override void OnDestroy()
	{
		CollisionRecorder collisionRecorder = CollisionRecorder;
		collisionRecorder.CollisionDeferred = (Action<GameObject, float>)Delegate.Remove(collisionRecorder.CollisionDeferred, new Action<GameObject, float>(OnCollisionDeferred));
		transform.DOKill();
	}

	public void InitializeNetworkVariables(bool isReplay = false)
	{
		if (!isNetworkVariablesInitialized)
		{
			isNetworkVariablesInitialized = true;
			IsReplay = new NetworkVariable<bool>(isReplay);
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

	public List<KeyValuePair<Player, float>> GetPlayerCollisions()
	{
		List<KeyValuePair<Player, float>> list = NetworkObjectCollisionRecorder.NetworkObjectCollisions.Select((NetworkObjectCollision collision) =>
		{
			if (collision.NetworkObjectReference.TryGet(out var networkObject))
			{
				networkObject.TryGetComponent<PlayerBody>(out var component);
				networkObject.TryGetComponent<Stick>(out var component2);
				if ((bool)component)
				{
					return new KeyValuePair<Player, float>(component.Player, collision.Time);
				}
				if ((bool)component2)
				{
					return new KeyValuePair<Player, float>(component2.Player, collision.Time);
				}
			}
			return new KeyValuePair<Player, float>(null, collision.Time);
		}).ToList();
		list.RemoveAll((KeyValuePair<Player, float> collision) => collision.Key == null);
		return list;
	}

	public List<KeyValuePair<Player, float>> GetPlayerCollisionsByTeam(PlayerTeam team)
	{
		return GetPlayerCollisions().Where((KeyValuePair<Player, float> collision) =>
		{
			Player key = collision.Key;
			return (object)key != null && key.Team == team;
		}).ToList();
	}

	public bool IsActivelyTouchedBy(Player player)
	{
		if (!player)
		{
			return false;
		}
		if ((bool)player.Stick && ReplayTouchTracker.IsActivelyTouching(player.Stick.NetworkObject))
		{
			return true;
		}
		if ((bool)player.PlayerBody && ReplayTouchTracker.IsActivelyTouching(player.PlayerBody.NetworkObject))
		{
			return true;
		}
		return false;
	}

	private void Server_UpdateStickTensor(Vector3 inertiaTensor, Quaternion inertiaTensorRotation)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Rigidbody.inertiaTensor = inertiaTensor;
			Rigidbody.inertiaTensorRotation = inertiaTensorRotation;
		}
	}

	private void Server_UpdateAudio()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			if (Rigidbody.constraints != RigidbodyConstraints.None)
			{
				windAudioSource.Server_SetVolume(0f);
				return;
			}
			float volume;
			float pitch;
			int changed = NativeAudio.EvaluateWind(Speed, MaxSpeed, out volume, out pitch);
			if (changed < 0)
			{
				float time = Mathf.Min(Speed / MaxSpeed, 1f);
				windAudioSource.Server_SetVolume(windVolumeCurve.Evaluate(time));
				float time2 = Mathf.Min(Speed / MaxSpeed, 1f);
				windAudioSource.Server_SetPitch(windPitchCurve.Evaluate(time2));
			}
			else if (changed > 0)
			{
				windAudioSource.Server_SetVolume(volume);
				windAudioSource.Server_SetPitch(pitch);
			}
		}
	}

	private void OnCollisionDeferred(GameObject gameObject, float force)
	{
		if (!NetworkManager.Singleton.IsServer || !gameObject)
		{
			return;
		}
		string text = LayerMask.LayerToName(gameObject.layer);
		if (!(text == "Goal Post"))
		{
			if (text == "Barrier")
			{
				hitBoardsAudioSource.Server_Play(hitBoardsVolumeCurve.Evaluate(force), hitBoardsPitchCurve.Evaluate(force), isOneShot: true, -1, 0f, randomClip: true);
			}
			else
			{
				hitIceAudioSource.Server_Play(hitIceVolumeCurve.Evaluate(force), hitIcePitchCurve.Evaluate(force), isOneShot: true, -1, 0f, randomClip: true);
			}
		}
		else
		{
			hitGoalPostAudioSource.Server_Play(hitGoalPostVolumeCurve.Evaluate(force), hitGoalPostPitchCurve.Evaluate(force), isOneShot: true, -1, 0f, randomClip: true);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.TryGetComponent<Stick>(out var component))
		{
			TouchingStick = component;
			ShotSpeed = 0f;
		}
		if (SynchronizedObject.IsDrivenExternally || IsGrounded)
		{
			return;
		}
		string text = LayerMask.LayerToName(collision.gameObject.layer);
		Vector3 zero = Vector3.zero;
		int contactCount = collision.contactCount;
		if (contactCount == 0)
		{
			return;
		}
		for (int i = 0; i < contactCount; i++)
		{
			zero += collision.GetContact(i).normal;
		}
		zero /= (float)contactCount;
		float t = Mathf.Abs(Vector3.Dot(collision.relativeVelocity.normalized, zero.normalized));
		if (!(text == "Goal Net"))
		{
			_ = text == "Goal Post";
			return;
		}
		if (Rigidbody.linearVelocity.magnitude > goalNetLinearVelocityMaximumMagnitude)
		{
			Vector3 b = Rigidbody.linearVelocity.normalized * goalNetLinearVelocityMaximumMagnitude;
			b.y = 0f;
			Rigidbody.linearVelocity = Vector3.Lerp(Rigidbody.linearVelocity, b, t);
		}
		if (Rigidbody.angularVelocity.magnitude > goalNetAngularVelocityMaximumMagnitude)
		{
			Vector3 b2 = Rigidbody.angularVelocity.normalized * goalNetAngularVelocityMaximumMagnitude;
			Rigidbody.angularVelocity = Vector3.Lerp(Rigidbody.angularVelocity, b2, t);
		}
	}

	private void OnCollisionStay(Collision collision)
	{
		if (collision.gameObject.TryGetComponent<Stick>(out var component))
		{
			TouchingStick = component;
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (collision.gameObject.TryGetComponent<Stick>(out var _))
		{
			ShotSpeed = Speed;
			if (!SynchronizedObject.IsDrivenExternally)
			{
				Rigidbody.linearVelocity = Vector3.ClampMagnitude(Rigidbody.linearVelocity, MaxSpeed);
				Rigidbody.angularVelocity = Vector3.ClampMagnitude(Rigidbody.angularVelocity, MaxAngularSpeed);
				Vector3 force = new Vector3(0f, Mathf.Min(0f, 0f - Rigidbody.linearVelocity.y), 0f) * 5f;
				Rigidbody.AddForce(force, ForceMode.Acceleration);
			}
		}
	}

	public void OnDrawGizmos()
	{
		if (Application.isEditor)
		{
			Gizmos.color = Color.black;
			Gizmos.DrawWireSphere(transform.position, groundedCheckSphereRadius);
		}
	}

	protected override void __initializeVariables()
	{
		if (IsReplay == null)
		{
			throw new Exception("Puck.IsReplay cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		IsReplay.Initialize(this);
		__nameNetworkVariable(IsReplay, "IsReplay");
		NetworkVariableFields.Add(IsReplay);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected override string __getTypeName()
	{
		return "Puck";
	}
}
