using System;

public class SynchronizedObjectClientDiagnostics
{
	private float reportInterval;

	private float reportTimer;

	private int tickCount;

	private int lostTickCount;

	private int outOfOrderTickCount;

	private TimeHistogram tickDeltaTimeHistogram = new TimeHistogram();

	private double timelinePositionSum;

	private double timelineTimescaleSum;

	private int timelineSampleCount;

	private int snapCount;

	private int playbackSampleCount;

	private int extrapolatedPlaybackCount;

	public bool IsEnabled => reportInterval > 0f;

	public void Configure(float reportInterval)
	{
		this.reportInterval = reportInterval;
	}

	public void RecordTick(float tickDeltaTime, int lostTickCount)
	{
		if (IsEnabled)
		{
			tickCount++;
			this.lostTickCount += lostTickCount;
			if (!(tickDeltaTime <= 0f))
			{
				tickDeltaTimeHistogram.Add(tickDeltaTime);
			}
		}
	}

	public void RecordOutOfOrderTick()
	{
		if (IsEnabled)
		{
			outOfOrderTickCount++;
		}
	}

	public void RecordTimeline(double timelinePosition, double timelineTimescale)
	{
		if (IsEnabled)
		{
			timelinePositionSum += timelinePosition;
			timelineTimescaleSum += timelineTimescale;
			timelineSampleCount++;
		}
	}

	public void RecordSnap()
	{
		if (IsEnabled)
		{
			snapCount++;
		}
	}

	public void RecordPlayback(SynchronizedObjectPlayback playback)
	{
		if (IsEnabled && playback != SynchronizedObjectPlayback.None)
		{
			playbackSampleCount++;
			if (playback == SynchronizedObjectPlayback.Extrapolated)
			{
				extrapolatedPlaybackCount++;
			}
		}
	}

	public bool TryGetData(float deltaTime, out SynchronizedObjectClientDiagnosticsData data)
	{
		data = default;
		if (!IsEnabled)
		{
			return false;
		}
		reportTimer += deltaTime;
		if (reportTimer < reportInterval)
		{
			return false;
		}
		if (tickCount == 0)
		{
			Reset();
			return false;
		}
		data = BuildData();
		Reset();
		return true;
	}

	public void Reset()
	{
		reportTimer = 0f;
		tickCount = 0;
		lostTickCount = 0;
		outOfOrderTickCount = 0;
		tickDeltaTimeHistogram.Reset();
		timelinePositionSum = 0.0;
		timelineTimescaleSum = 0.0;
		timelineSampleCount = 0;
		snapCount = 0;
		playbackSampleCount = 0;
		extrapolatedPlaybackCount = 0;
	}

	private SynchronizedObjectClientDiagnosticsData BuildData()
	{
		int num = Math.Max(0, lostTickCount - outOfOrderTickCount);
		int num2 = tickCount + num;
		return new SynchronizedObjectClientDiagnosticsData
		{
			WindowTime = reportTimer,
			TickCount = tickCount,
			AverageTickDeltaTime = tickDeltaTimeHistogram.Average,
			P95TickDeltaTime = tickDeltaTimeHistogram.GetPercentile(0.95),
			MaxTickDeltaTime = tickDeltaTimeHistogram.Max,
			TickLossPercentage = ((num2 > 0) ? (100f * (float)num / (float)num2) : 0f),
			OutOfOrderTickCount = outOfOrderTickCount,
			AverageTimelinePosition = ((timelineSampleCount > 0) ? (timelinePositionSum / (double)timelineSampleCount) : 0.0),
			AverageTimelineTimescale = ((timelineSampleCount > 0) ? (timelineTimescaleSum / (double)timelineSampleCount) : 1.0),
			SnapCount = snapCount,
			ExtrapolationPercentage = ((playbackSampleCount > 0) ? (100f * (float)extrapolatedPlaybackCount / (float)playbackSampleCount) : 0f)
		};
	}
}
