using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SynchronizedObject : NetworkBehaviour
{
	private const int CLIENT_SAMPLE_CAPACITY = 64;

	[Header("Settings")]
	[Tooltip("Lets Server Lod Bands on Synchronized Object Manager override the server defaults for this object by its distance from each player. Disabling it keeps the defaults at every distance.")]
	public bool UseLod = true;

	[Tooltip("Lets Server Culling on Synchronized Object Manager override the server defaults and any LOD band for this object while it is behind a player's camera. Disabling it keeps whatever the distance produced.")]
	public bool UseCulling = true;

	[HideInInspector]
	public Rigidbody Rigidbody;

	[HideInInspector]
	public Vector3 PredictedLinearVelocity = Vector3.zero;

	[HideInInspector]
	public Vector3 PredictedAngularVelocity = Vector3.zero;

	private SynchronizedObjectInterpolator clientInterpolator;

	private SynchronizedObjectSmoothing clientSmoothing;

	private bool hasDrivenPose;

	private SynchronizedObjectPose drivenPose;

	public bool IsDrivenExternally => Rigidbody.isKinematic;

	private void Awake()
	{
		Rigidbody = GetComponent<Rigidbody>();
		clientInterpolator = new SynchronizedObjectInterpolator(64);
		clientSmoothing = new SynchronizedObjectSmoothing();
	}

	protected override void OnNetworkPostSpawn()
	{
		EventManager.TriggerEvent("Event_Everyone_OnSynchronizedObjectSpawned", new Dictionary<string, object> { { "synchronizedObject", this } });
		base.OnNetworkPostSpawn();
	}

	public override void OnNetworkDespawn()
	{
		EventManager.TriggerEvent("Event_Everyone_OnSynchronizedObjectDespawned", new Dictionary<string, object> { { "synchronizedObject", this } });
		base.OnNetworkDespawn();
	}

	public void DriveExternally(SynchronizedObjectDriveCadence cadence)
	{
		Rigidbody.isKinematic = true;
		Rigidbody.interpolation = GetDrivenInterpolation(cadence);
	}

	private static RigidbodyInterpolation GetDrivenInterpolation(SynchronizedObjectDriveCadence cadence)
	{
		if (cadence != SynchronizedObjectDriveCadence.PerTick || ApplicationManager.IsDedicatedGameServer)
		{
			return RigidbodyInterpolation.None;
		}
		return RigidbodyInterpolation.Interpolate;
	}

	public void SetDrivenPose(in SynchronizedObjectPose pose)
	{
		drivenPose = pose;
		hasDrivenPose = true;
		Rigidbody.Move(pose.Position, pose.Rotation);
	}

	public SynchronizedObjectPose GetPose()
	{
		if (hasDrivenPose)
		{
			return drivenPose;
		}
		return new SynchronizedObjectPose
		{
			Position = Rigidbody.position,
			Rotation = Rigidbody.rotation,
			LinearVelocity = GetLinearVelocityAtPosition(),
			AngularVelocity = Rigidbody.angularVelocity
		};
	}

	public Vector3 GetLinearVelocityAtPosition()
	{
		return Rigidbody.GetPointVelocity(Rigidbody.position);
	}

	public bool Client_HasNewerDataThan(double serverTime)
	{
		return clientInterpolator.HasDataAtLeastAsNewAs(serverTime);
	}

	public void Client_ApplyReliableSynchronizedObjectData(Vector3 position, Quaternion rotation, double serverTime)
	{
		if (!Client_HasNewerDataThan(serverTime))
		{
			clientInterpolator.MarkReceived(serverTime);
			transform.SetPositionAndRotation(position, rotation);
		}
	}

	public void Client_BufferSynchronizedObjectData(Vector3 position, Quaternion rotation, Vector3 linearVelocity, Vector3 angularVelocity, double serverTime, float tickInterval, byte tickRateDivisor)
	{
		if (clientInterpolator.Append(position, rotation, linearVelocity, angularVelocity, serverTime, tickInterval, tickRateDivisor))
		{
			clientSmoothing.MarkNewSample();
		}
	}

	public void Client_SetAsleep()
	{
		clientInterpolator.SetAsleep();
		PredictedLinearVelocity = Vector3.zero;
		PredictedAngularVelocity = Vector3.zero;
	}

	public void Client_ResetInterpolation()
	{
		clientInterpolator.Reset();
		clientSmoothing.Reset();
	}

	public SynchronizedObjectPlayback Client_Interpolate(double localTimeline, float deltaTime, float latencyCompensationTime, float maxPositionExtrapolationTicks, float maxRotationExtrapolationTicks)
	{
		if (!clientInterpolator.HasSamples)
		{
			return SynchronizedObjectPlayback.None;
		}
		double sampleTime = localTimeline + (double)latencyCompensationTime - (double)clientInterpolator.ExtraLag;
		float extraLag = clientInterpolator.ExtraLag;
		if (clientSmoothing.TryBeginAbsorb(out var sampleTime2, out var projectionTime))
		{
			clientSmoothing.Absorb(clientInterpolator.Evaluate(sampleTime2, projectionTime, maxPositionExtrapolationTicks, maxRotationExtrapolationTicks, out var _));
		}
		SynchronizedObjectPose pose = clientSmoothing.Apply(clientInterpolator.Evaluate(sampleTime, extraLag, maxPositionExtrapolationTicks, maxRotationExtrapolationTicks, out var playback2), deltaTime, clientInterpolator.SampleInterval);
		transform.SetPositionAndRotation(pose.Position, pose.Rotation);
		PredictedLinearVelocity = pose.LinearVelocity;
		PredictedAngularVelocity = pose.AngularVelocity;
		clientSmoothing.CaptureRenderedPose(in pose, sampleTime, extraLag);
		return playback2;
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected override string __getTypeName()
	{
		return "SynchronizedObject";
	}
}
