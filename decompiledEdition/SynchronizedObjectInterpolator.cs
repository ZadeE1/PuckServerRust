using UnityEngine;

public class SynchronizedObjectInterpolator
{
	private SynchronizedObjectSampleRing samples;

	private bool hasReceivedData;

	private double lastReceivedServerTime;

	private float sampleInterval;

	private float extraLag;

	private float velocityLeadFraction = 0.5f;

	private bool isAsleep;

	public bool HasSamples => samples.Count > 0;

	public float SampleInterval => sampleInterval;

	public float ExtraLag => extraLag;

	public SynchronizedObjectInterpolator(int sampleCapacity)
	{
		samples = new SynchronizedObjectSampleRing(sampleCapacity);
	}

	public bool HasDataAtLeastAsNewAs(double serverTime)
	{
		if (hasReceivedData)
		{
			return serverTime <= lastReceivedServerTime;
		}
		return false;
	}

	public void MarkReceived(double serverTime)
	{
		hasReceivedData = true;
		lastReceivedServerTime = serverTime;
		isAsleep = false;
	}

	public bool Append(Vector3 position, Quaternion rotation, Vector3 linearVelocity, Vector3 angularVelocity, double serverTime, float tickInterval, byte tickRateDivisor)
	{
		if (samples.Count > 0 && serverTime <= samples.Newest.ServerTime)
		{
			return false;
		}
		sampleInterval = (float)(int)tickRateDivisor * tickInterval;
		extraLag = sampleInterval - tickInterval;
		velocityLeadFraction = 0.5f / (float)Mathf.Max(1, tickRateDivisor);
		samples.Append(new SynchronizedObjectSample
		{
			ServerTime = serverTime,
			Position = position,
			Rotation = rotation,
			LinearVelocity = linearVelocity,
			AngularVelocity = angularVelocity
		});
		MarkReceived(serverTime);
		return true;
	}

	public void SetAsleep()
	{
		isAsleep = true;
	}

	public void Reset()
	{
		hasReceivedData = false;
		lastReceivedServerTime = 0.0;
		samples.Reset();
		sampleInterval = 0f;
		extraLag = 0f;
		velocityLeadFraction = 0.5f;
		isAsleep = false;
	}

	public SynchronizedObjectPose Evaluate(double sampleTime, float projectionTime, float maxPositionExtrapolationTicks, float maxRotationExtrapolationTicks, out SynchronizedObjectPlayback playback)
	{
		SynchronizedObjectSample newestSample = samples.Newest;
		if (sampleTime >= newestSample.ServerTime)
		{
			return EvaluateAfterNewestSample(in newestSample, sampleTime, projectionTime, maxPositionExtrapolationTicks, maxRotationExtrapolationTicks, out playback);
		}
		playback = SynchronizedObjectPlayback.Interpolated;
		SynchronizedObjectSample sample = samples.Oldest;
		if (sampleTime <= sample.ServerTime)
		{
			return GetSamplePose(in sample);
		}
		return EvaluateBetweenSamples(in newestSample, sampleTime, projectionTime);
	}

	private SynchronizedObjectPose EvaluateAfterNewestSample(in SynchronizedObjectSample newestSample, double sampleTime, float projectionTime, float maxPositionExtrapolationTicks, float maxRotationExtrapolationTicks, out SynchronizedObjectPlayback playback)
	{
		if (isAsleep)
		{
			playback = SynchronizedObjectPlayback.None;
			return new SynchronizedObjectPose
			{
				Position = newestSample.Position,
				Rotation = newestSample.Rotation
			};
		}
		SynchronizedObjectPose pose = GetSamplePose(in newestSample);
		pose.LinearVelocity = GetInstantaneousLinearVelocity(samples.Count - 1);
		float pastTime = (float)(sampleTime - newestSample.ServerTime);
		playback = SynchronizedObjectPlayback.Extrapolated;
		ApplyPositionExtrapolation(ref pose, projectionTime);
		ApplyRotationExtrapolation(ref pose, projectionTime);
		ExtrapolatePositionWithFalloff(ref pose, pastTime, maxPositionExtrapolationTicks * sampleInterval);
		ExtrapolateRotationWithFalloff(ref pose, pastTime, maxRotationExtrapolationTicks * sampleInterval);
		return pose;
	}

	private SynchronizedObjectPose EvaluateBetweenSamples(in SynchronizedObjectSample newestSample, double sampleTime, float projectionTime)
	{
		SynchronizedObjectPose pose = GetSamplePose(in newestSample);
		for (int num = samples.Count - 1; num > 0; num--)
		{
			SynchronizedObjectSample synchronizedObjectSample = samples[num - 1];
			if (!(sampleTime < synchronizedObjectSample.ServerTime))
			{
				SynchronizedObjectSample synchronizedObjectSample2 = samples[num];
				double num2 = synchronizedObjectSample2.ServerTime - synchronizedObjectSample.ServerTime;
				float t = (float)((sampleTime - synchronizedObjectSample.ServerTime) / num2);
				float sampleDeltaTime = (float)num2;
				Vector3 instantaneousLinearVelocity = GetInstantaneousLinearVelocity(num - 1);
				Vector3 instantaneousLinearVelocity2 = GetInstantaneousLinearVelocity(num);
				pose.Position = GetHermitePosition(synchronizedObjectSample.Position, instantaneousLinearVelocity, synchronizedObjectSample2.Position, instantaneousLinearVelocity2, t, sampleDeltaTime);
				pose.Rotation = Quaternion.Slerp(synchronizedObjectSample.Rotation, synchronizedObjectSample2.Rotation, t);
				pose.LinearVelocity = GetHermiteLinearVelocity(synchronizedObjectSample.Position, instantaneousLinearVelocity, synchronizedObjectSample2.Position, instantaneousLinearVelocity2, t, sampleDeltaTime);
				pose.AngularVelocity = Vector3.Lerp(synchronizedObjectSample.AngularVelocity, synchronizedObjectSample2.AngularVelocity, t);
				ApplyPositionExtrapolation(ref pose, projectionTime);
				ApplyRotationExtrapolation(ref pose, projectionTime);
				return pose;
			}
		}
		return pose;
	}

	private Vector3 GetInstantaneousLinearVelocity(int index)
	{
		if (index + 1 < samples.Count)
		{
			return Vector3.LerpUnclamped(samples[index].LinearVelocity, samples[index + 1].LinearVelocity, velocityLeadFraction);
		}
		if (index > 0)
		{
			return Vector3.LerpUnclamped(samples[index - 1].LinearVelocity, samples[index].LinearVelocity, 1f + velocityLeadFraction);
		}
		return samples[index].LinearVelocity;
	}

	private static Vector3 GetHermitePosition(Vector3 fromPosition, Vector3 fromLinearVelocity, Vector3 toPosition, Vector3 toLinearVelocity, float t, float sampleDeltaTime)
	{
		float num = t * t;
		float num2 = num * t;
		return (2f * num2 - 3f * num + 1f) * fromPosition + (num2 - 2f * num + t) * sampleDeltaTime * fromLinearVelocity + (-2f * num2 + 3f * num) * toPosition + (num2 - num) * sampleDeltaTime * toLinearVelocity;
	}

	private static Vector3 GetHermiteLinearVelocity(Vector3 fromPosition, Vector3 fromLinearVelocity, Vector3 toPosition, Vector3 toLinearVelocity, float t, float sampleDeltaTime)
	{
		float num = t * t;
		return (6f * t - 6f * num) * (toPosition - fromPosition) / sampleDeltaTime + (3f * num - 4f * t + 1f) * fromLinearVelocity + (3f * num - 2f * t) * toLinearVelocity;
	}

	private static SynchronizedObjectPose GetSamplePose(in SynchronizedObjectSample sample)
	{
		return new SynchronizedObjectPose
		{
			Position = sample.Position,
			Rotation = sample.Rotation,
			LinearVelocity = sample.LinearVelocity,
			AngularVelocity = sample.AngularVelocity
		};
	}

	private static void ExtrapolatePositionWithFalloff(ref SynchronizedObjectPose pose, float pastTime, float maxExtrapolationTime)
	{
		float velocityFalloff = GetVelocityFalloff(pastTime, maxExtrapolationTime);
		ApplyPositionExtrapolation(ref pose, maxExtrapolationTime * (1f - velocityFalloff));
		pose.LinearVelocity *= velocityFalloff;
	}

	private static void ExtrapolateRotationWithFalloff(ref SynchronizedObjectPose pose, float pastTime, float maxExtrapolationTime)
	{
		float velocityFalloff = GetVelocityFalloff(pastTime, maxExtrapolationTime);
		ApplyRotationExtrapolation(ref pose, maxExtrapolationTime * (1f - velocityFalloff));
		pose.AngularVelocity *= velocityFalloff;
	}

	private static float GetVelocityFalloff(float pastTime, float maxExtrapolationTime)
	{
		if (pastTime <= 0f || maxExtrapolationTime <= 0f)
		{
			return 1f;
		}
		return Mathf.Exp((0f - pastTime) / maxExtrapolationTime);
	}

	private static void ApplyPositionExtrapolation(ref SynchronizedObjectPose pose, float pastTime)
	{
		if (!(pastTime <= 0f))
		{
			pose.Position += pose.LinearVelocity * pastTime;
		}
	}

	private static void ApplyRotationExtrapolation(ref SynchronizedObjectPose pose, float pastTime)
	{
		if (!(pastTime <= 0f))
		{
			float magnitude = pose.AngularVelocity.magnitude;
			if (!(magnitude <= 0f))
			{
				pose.Rotation = Quaternion.AngleAxis(magnitude * pastTime * 57.29578f, pose.AngularVelocity / magnitude) * pose.Rotation;
			}
		}
	}
}
