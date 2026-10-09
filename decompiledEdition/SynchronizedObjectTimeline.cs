using System;

public class SynchronizedObjectTimeline
{
	private double localTimeline;

	private double localTimescale = 1.0;

	private float timescaleCorrectionGain = 2f;

	private float maxTimescaleCorrection = 0.04f;

	private float maxDriftTime;

	private ExponentialMovingAverage driftEma;

	public bool IsInitialized { get; private set; }

	public double Time => localTimeline;

	public double Timescale => localTimescale;

	public void Configure(int sampleRate, float timescaleCorrectionGain, float maxTimescaleCorrection, int driftEmaDuration, float maxDriftTime)
	{
		this.timescaleCorrectionGain = timescaleCorrectionGain;
		this.maxTimescaleCorrection = maxTimescaleCorrection;
		this.maxDriftTime = maxDriftTime;
		driftEma = new ExponentialMovingAverage(Math.Max(1, sampleRate * driftEmaDuration));
	}

	public bool Adjust(double serverTime)
	{
		if (!IsInitialized)
		{
			localTimeline = serverTime;
			IsInitialized = true;
		}
		double num = serverTime - localTimeline;
		if (IsBeyondSnapThreshold(num))
		{
			Snap(serverTime);
			return true;
		}
		driftEma.Add(num);
		double num2 = Math.Clamp(driftEma.Value * (double)timescaleCorrectionGain, 0f - maxTimescaleCorrection, maxTimescaleCorrection);
		localTimescale = 1.0 + num2;
		return false;
	}

	private bool IsBeyondSnapThreshold(double driftTime)
	{
		if (maxDriftTime <= 0f)
		{
			return false;
		}
		return Math.Abs(driftTime) > (double)maxDriftTime;
	}

	private void Snap(double serverTime)
	{
		localTimeline = serverTime;
		localTimescale = 1.0;
		driftEma.Reset();
	}

	public void Step(double deltaTime)
	{
		NetworkingUtils.StepTime(deltaTime, ref localTimeline, localTimescale);
	}

	public void Reset()
	{
		localTimeline = 0.0;
		localTimescale = 1.0;
		IsInitialized = false;
		driftEma.Reset();
	}
}
