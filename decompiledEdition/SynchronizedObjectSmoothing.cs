using UnityEngine;

public class SynchronizedObjectSmoothing
{
	private bool hasNewSample;

	private bool hasRenderedPose;

	private double lastSampleTime;

	private float lastProjectionTime;

	private Vector3 renderedPosition = Vector3.zero;

	private Quaternion renderedRotation = Quaternion.identity;

	private Vector3 positionCorrection = Vector3.zero;

	private Quaternion rotationCorrection = Quaternion.identity;

	public void MarkNewSample()
	{
		hasNewSample = true;
	}

	public bool TryBeginAbsorb(out double sampleTime, out float projectionTime)
	{
		sampleTime = lastSampleTime;
		projectionTime = lastProjectionTime;
		if (!hasNewSample)
		{
			return false;
		}
		hasNewSample = false;
		return hasRenderedPose;
	}

	public void Absorb(in SynchronizedObjectPose poseAtLastRenderedTime)
	{
		positionCorrection = renderedPosition - poseAtLastRenderedTime.Position;
		rotationCorrection = renderedRotation * Quaternion.Inverse(poseAtLastRenderedTime.Rotation);
	}

	public SynchronizedObjectPose Apply(SynchronizedObjectPose pose, float deltaTime, float correctionTimeConstant)
	{
		float num = Mathf.Exp((0f - deltaTime) / correctionTimeConstant);
		positionCorrection *= num;
		rotationCorrection = Quaternion.Slerp(Quaternion.identity, rotationCorrection, num);
		pose.Position += positionCorrection;
		pose.Rotation = rotationCorrection * pose.Rotation;
		return pose;
	}

	public void CaptureRenderedPose(in SynchronizedObjectPose pose, double sampleTime, float projectionTime)
	{
		renderedPosition = pose.Position;
		renderedRotation = pose.Rotation;
		hasRenderedPose = true;
		lastSampleTime = sampleTime;
		lastProjectionTime = projectionTime;
	}

	public void Reset()
	{
		hasNewSample = false;
		hasRenderedPose = false;
		lastSampleTime = 0.0;
		lastProjectionTime = 0f;
		positionCorrection = Vector3.zero;
		rotationCorrection = Quaternion.identity;
	}
}
