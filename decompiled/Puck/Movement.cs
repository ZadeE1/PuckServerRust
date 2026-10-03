using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerBody))]
[RequireComponent(typeof(Hover))]
public class Movement : MonoBehaviour
{
	[Header("Settings")]
	[Tooltip("Acceleration (m/s²) applied while skating forwards without sprinting, until maxForwardsSpeed is reached.")]
	[SerializeField]
	private float forwardsAcceleration = 2f;

	[Tooltip("Acceleration (m/s²) applied while sprinting forwards below maxForwardsSpeed.")]
	[SerializeField]
	private float forwardsSprintAcceleration = 4.75f;

	[Tooltip("Acceleration (m/s²) applied while sprinting forwards once already above maxForwardsSpeed — controls how quickly the sprint band between maxForwardsSpeed and maxForwardsSprintSpeed fills up.")]
	[SerializeField]
	private float forwardsSprintOverspeedAcceleration = 1f;

	[Tooltip("Acceleration (m/s²) applied while skating backwards without sprinting, until maxBackwardsSpeed is reached.")]
	[SerializeField]
	private float backwardsAcceleration = 1.8f;

	[Tooltip("Acceleration (m/s²) applied while sprinting backwards below maxForwardsSpeed.")]
	[SerializeField]
	private float backwardsSprintAcceleration = 2f;

	[Tooltip("Acceleration (m/s²) applied while sprinting backwards once already above maxForwardsSpeed.")]
	[SerializeField]
	private float backwardsSprintOverspeedAcceleration = 1f;

	[Tooltip("Deceleration (m/s²) applied when movement input opposes the current direction of travel (e.g. holding back while still gliding forwards). Higher = sharper stops.")]
	[SerializeField]
	private float brakeAcceleration = 5f;

	[Tooltip("Baseline velocity damping applied every physics tick (velocity *= 1 - drag * dt) — the 'ice friction' that makes a coasting player slowly glide to a stop.")]
	[SerializeField]
	private float drag = 0.025f;

	[Tooltip("Velocity damping applied instead of drag while above MaximumSpeed (e.g. after an external boost or collision) — how quickly excess speed bleeds off.")]
	[SerializeField]
	private float overspeedDrag = 0.025f;

	[Space(20f)]
	[Tooltip("Speed (m/s) above which forwards acceleration cuts off when not sprinting.")]
	[SerializeField]
	private float maxForwardsSpeed = 7.5f;

	[Tooltip("Speed (m/s) above which forwards acceleration cuts off while sprinting.")]
	[SerializeField]
	private float maxForwardsSprintSpeed = 8.75f;

	[Tooltip("Speed (m/s) above which backwards acceleration cuts off when not sprinting.")]
	[SerializeField]
	private float maxBackwardsSpeed = 7.25f;

	[Tooltip("Speed (m/s) above which backwards acceleration cuts off while sprinting.")]
	[SerializeField]
	private float maxBackwardsSprintSpeed = 7.25f;

	[Space(20f)]
	[Tooltip("Angular acceleration (rad/s²) applied while turning, until the turn speed cap is reached.")]
	[SerializeField]
	private float turnAcceleration = 1.625f;

	[Tooltip("Angular deceleration (rad/s²) applied when turn input opposes the current rotation direction — how sharply a turn can be reversed.")]
	[SerializeField]
	private float turnBrakeAcceleration = 3.25f;

	[Tooltip("Maximum turn rate (rad/s) around the player's up axis. Scaled at runtime by TurnMultiplier.")]
	[SerializeField]
	private float turnMaxSpeed = 1.375f;

	[Tooltip("Angular velocity damping applied every physics tick while no turn input is held — how quickly rotation settles once the keys are released.")]
	[SerializeField]
	private float turnDrag = 3f;

	[Tooltip("Angular velocity damping applied while spinning faster than the turn speed cap — bleeds off excess rotation.")]
	[SerializeField]
	private float turnOverspeedDrag = 2.25f;

	[HideInInspector]
	public Rigidbody Rigidbody;

	[HideInInspector]
	public SynchronizedObject SynchronizedObject;

	[HideInInspector]
	public PlayerBody PlayerBody;

	[HideInInspector]
	public Hover Hover;

	[HideInInspector]
	public bool MoveForwards;

	[HideInInspector]
	public bool MoveBackwards;

	[HideInInspector]
	public bool TurnLeft;

	[HideInInspector]
	public bool TurnRight;

	[HideInInspector]
	public float TurnMultiplier;

	[HideInInspector]
	public bool Sprint;

	[HideInInspector]
	public float AmbientDrag;

	[HideInInspector]
	public Transform MovementDirection;

	private float currentMaxSpeed;

	private float currentAcceleration;

	[HideInInspector]
	public float Speed => new Vector3(Rigidbody.linearVelocity.x, 0f, Rigidbody.linearVelocity.z).magnitude;

	[HideInInspector]
	public float NormalizedMaximumSpeed => Speed / MaximumSpeed;

	[HideInInspector]
	public float NormalizedMinimumSpeed => Speed / MinimumSpeed;

	[HideInInspector]
	public float TurnSpeed => Math.Abs(transform.InverseTransformVector(Rigidbody.angularVelocity).y);

	[HideInInspector]
	public float MaximumSpeed => Mathf.Max(Mathf.Max(maxForwardsSpeed, maxForwardsSprintSpeed), Mathf.Max(maxBackwardsSpeed, maxBackwardsSprintSpeed));

	[HideInInspector]
	public float MinimumSpeed => Mathf.Min(Mathf.Min(maxForwardsSpeed, maxForwardsSprintSpeed), Mathf.Min(maxBackwardsSpeed, maxBackwardsSprintSpeed));

	[HideInInspector]
	public bool IsMovingForwards => MovementDirection.InverseTransformVector(Rigidbody.linearVelocity).z > 0f;

	[HideInInspector]
	public bool IsMovingBackwards => MovementDirection.InverseTransformVector(Rigidbody.linearVelocity).z < 0f;

	[HideInInspector]
	public bool IsTurningLeft => transform.InverseTransformVector(Rigidbody.angularVelocity).y < 0f;

	[HideInInspector]
	public bool IsTurningRight => transform.InverseTransformVector(Rigidbody.angularVelocity).y > 0f;

	private void Awake()
	{
		Rigidbody = GetComponent<Rigidbody>();
		SynchronizedObject = GetComponent<SynchronizedObject>();
		PlayerBody = GetComponent<PlayerBody>();
		Hover = GetComponent<Hover>();
	}

	private void Start()
	{
		currentMaxSpeed = maxForwardsSpeed;
		currentAcceleration = forwardsAcceleration;
	}

	private void FixedUpdate()
	{
		if (!SynchronizedObject.IsDrivenExternally)
		{
			Move();
			Turn();
		}
	}

	private void Move()
	{
		if (!Hover.IsGrounded)
		{
			return;
		}
		if (IsMovingForwards)
		{
			if (Sprint)
			{
				currentMaxSpeed = maxForwardsSprintSpeed;
				currentAcceleration = ((Speed < maxForwardsSpeed) ? forwardsSprintAcceleration : forwardsSprintOverspeedAcceleration);
			}
			else
			{
				currentMaxSpeed = maxForwardsSpeed;
				currentAcceleration = forwardsAcceleration;
			}
		}
		else if (IsMovingBackwards)
		{
			if (Sprint)
			{
				currentMaxSpeed = maxBackwardsSprintSpeed;
				currentAcceleration = ((Speed < maxForwardsSpeed) ? backwardsSprintAcceleration : backwardsSprintOverspeedAcceleration);
			}
			else
			{
				currentMaxSpeed = maxBackwardsSpeed;
				currentAcceleration = backwardsAcceleration;
			}
		}
		if (MoveForwards)
		{
			if (IsMovingForwards)
			{
				float num = ((Speed < currentMaxSpeed) ? currentAcceleration : 0f);
				Rigidbody.AddForce(MovementDirection.forward * num, ForceMode.Acceleration);
			}
			else if (IsMovingBackwards)
			{
				float num2 = brakeAcceleration;
				Rigidbody.AddForce(MovementDirection.forward * num2, ForceMode.Acceleration);
			}
		}
		else if (MoveBackwards)
		{
			if (IsMovingBackwards)
			{
				float num3 = ((Speed < currentMaxSpeed) ? currentAcceleration : 0f);
				Rigidbody.AddForce(-MovementDirection.forward * num3, ForceMode.Acceleration);
			}
			else if (IsMovingForwards)
			{
				float num4 = brakeAcceleration;
				Rigidbody.AddForce(-MovementDirection.forward * num4, ForceMode.Acceleration);
			}
		}
		if (Speed > MaximumSpeed)
		{
			Rigidbody.linearVelocity *= 1f - overspeedDrag * Time.fixedDeltaTime;
		}
		else
		{
			Rigidbody.linearVelocity *= 1f - drag * Time.fixedDeltaTime;
		}
		Rigidbody.linearVelocity *= 1f - AmbientDrag * Time.fixedDeltaTime;
	}

	private void Turn()
	{
		if (TurnLeft)
		{
			if (IsTurningLeft)
			{
				float num = ((TurnSpeed < turnMaxSpeed * TurnMultiplier) ? turnAcceleration : 0f);
				Rigidbody.AddTorque(transform.up * (0f - num) * TurnMultiplier, ForceMode.Acceleration);
			}
			else if (IsTurningRight)
			{
				float num2 = turnBrakeAcceleration;
				Rigidbody.AddTorque(transform.up * (0f - num2) * TurnMultiplier, ForceMode.Acceleration);
			}
		}
		else if (TurnRight)
		{
			if (IsTurningRight)
			{
				float num3 = ((TurnSpeed < turnMaxSpeed * TurnMultiplier) ? turnAcceleration : 0f);
				Rigidbody.AddTorque(transform.up * num3 * TurnMultiplier, ForceMode.Acceleration);
			}
			else if (IsTurningLeft)
			{
				float num4 = turnBrakeAcceleration;
				Rigidbody.AddTorque(transform.up * num4 * TurnMultiplier, ForceMode.Acceleration);
			}
		}
		else if (TurnSpeed < turnMaxSpeed * TurnMultiplier)
		{
			Rigidbody.angularVelocity *= 1f - turnDrag * Time.fixedDeltaTime;
		}
		if (TurnSpeed > turnMaxSpeed * TurnMultiplier)
		{
			Rigidbody.angularVelocity *= 1f - turnOverspeedDrag * Time.fixedDeltaTime;
		}
	}
}
