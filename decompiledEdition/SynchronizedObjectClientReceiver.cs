using System.Collections.Generic;
using UnityEngine;

public class SynchronizedObjectClientReceiver
{
	private SynchronizedObjectRegistry registry;

	private SynchronizedObjectNetworkSmoothingSettings smoothingSettings = SynchronizedObjectNetworkSmoothingSettings.Default;

	private float fallbackTickInterval;

	private Dictionary<ulong, SynchronizedObjectReceivedState> receivedStates = new Dictionary<ulong, SynchronizedObjectReceivedState>();

	private ushort lastReceivedSequenceNumber;

	private double lastReceivedServerTime;

	private double lastReceivedLocalTime;

	private bool hasReceivedFirstTick;

	private bool hasPendingTimelineAdjustment;

	private float receivedTickInterval;

	private float timeScale = 1f;

	private SynchronizedObjectClientDiagnostics diagnostics = new SynchronizedObjectClientDiagnostics();

	private SynchronizedObjectTimeline timeline = new SynchronizedObjectTimeline();

	private double StallTime
	{
		get
		{
			if (!hasReceivedFirstTick)
			{
				return 0.0;
			}
			return Time.realtimeSinceStartupAsDouble - lastReceivedLocalTime;
		}
	}

	private float TickInterval
	{
		get
		{
			if (!(receivedTickInterval > 0f))
			{
				return fallbackTickInterval;
			}
			return receivedTickInterval;
		}
	}

	private float TargetTimelinePosition => smoothingSettings.TargetTimelinePosition * TickInterval;

	public SynchronizedObjectClientReceiver(SynchronizedObjectRegistry registry)
	{
		this.registry = registry;
	}

	public void Configure(SynchronizedObjectNetworkSmoothingSettings smoothingSettings, float diagnosticsInterval)
	{
		this.smoothingSettings = smoothingSettings;
		diagnostics.Configure(diagnosticsInterval);
	}

	public void SetTickInterval(float tickInterval)
	{
		fallbackTickInterval = tickInterval;
		ConfigureTimeline();
	}

	public void Forget(ulong networkObjectId)
	{
		receivedStates.Remove(networkObjectId);
	}

	public void ReceiveTick(SynchronizedObjectTickHeader tickHeader, SynchronizedObjectData[] synchronizedObjectsData)
	{
		double serverTime = tickHeader.ServerTime;
		if (hasReceivedFirstTick && serverTime <= lastReceivedServerTime)
		{
			diagnostics.RecordOutOfOrderTick();
			return;
		}
		ApplyTickHeader(tickHeader);
		RecordTickDiagnostics(tickHeader.SequenceNumber, serverTime, out var serverDeltaTime);
		hasPendingTimelineAdjustment = true;
		BufferObjects(synchronizedObjectsData, serverTime);
		TriggerSynchronizeEvents(serverDeltaTime);
	}

	public void ReceiveReliable(SynchronizedObjectTickHeader tickHeader, SynchronizedObjectData[] synchronizedObjectsData)
	{
		ApplyTickHeader(tickHeader);
		double serverTime = tickHeader.ServerTime;
		float tickInterval = TickInterval;
		for (int i = 0; i < synchronizedObjectsData.Length; i++)
		{
			SynchronizedObjectData synchronizedObjectData = synchronizedObjectsData[i];
			if (registry.TryGet(synchronizedObjectData.NetworkObjectId, out var synchronizedObject) && !synchronizedObject.Client_HasNewerDataThan(serverTime) && TryMergeReceivedData(synchronizedObjectData, serverTime, out var mergedData))
			{
				Vector3 position = mergedData.GetPosition();
				Quaternion rotation = mergedData.Rotation;
				if (timeline.IsInitialized)
				{
					synchronizedObject.Client_BufferSynchronizedObjectData(position, rotation, mergedData.LinearVelocity, mergedData.AngularVelocity, serverTime, tickInterval, mergedData.TickRateDivisor);
				}
				else
				{
					synchronizedObject.Client_ApplyReliableSynchronizedObjectData(position, rotation, serverTime);
				}
				if (synchronizedObjectData.IsAsleep)
				{
					synchronizedObject.Client_SetAsleep();
				}
			}
		}
		TriggerSynchronizeEvents(0f);
	}

	public void Update(float deltaTime)
	{
		float num = deltaTime * timeScale;
		AdjustTimeline();
		if (hasReceivedFirstTick && diagnostics.IsEnabled)
		{
			diagnostics.RecordTimeline(GetTimelinePosition() / (double)TickInterval, timeline.Timescale);
		}
		TriggerDiagnosticsEvents(deltaTime);
		if (!timeline.IsInitialized)
		{
			return;
		}
		timeline.Step(num);
		float targetTimelinePosition = TargetTimelinePosition;
		float maxPositionExtrapolationTicks = smoothingSettings.MaxPositionExtrapolationTicks;
		float maxRotationExtrapolationTicks = smoothingSettings.MaxRotationExtrapolationTicks;
		foreach (SynchronizedObject @object in registry.Objects)
		{
			if ((bool)@object)
			{
				diagnostics.RecordPlayback(@object.Client_Interpolate(timeline.Time, num, targetTimelinePosition, maxPositionExtrapolationTicks, maxRotationExtrapolationTicks));
			}
		}
	}

	public void Dispose()
	{
		receivedStates.Clear();
		lastReceivedSequenceNumber = 0;
		lastReceivedServerTime = 0.0;
		lastReceivedLocalTime = 0.0;
		hasReceivedFirstTick = false;
		hasPendingTimelineAdjustment = false;
		receivedTickInterval = 0f;
		timeScale = 1f;
		diagnostics.Reset();
		ConfigureTimeline();
		ResetInterpolationState();
	}

	private void AdjustTimeline()
	{
		if (hasPendingTimelineAdjustment)
		{
			hasPendingTimelineAdjustment = false;
			if (timeline.Adjust(lastReceivedServerTime))
			{
				diagnostics.RecordSnap();
			}
		}
	}

	private void ApplyTickHeader(SynchronizedObjectTickHeader tickHeader)
	{
		if (tickHeader.HasTimeScale)
		{
			timeScale = tickHeader.TimeScale;
		}
		if (tickHeader.HasTickInterval && tickHeader.TickInterval != receivedTickInterval)
		{
			receivedTickInterval = tickHeader.TickInterval;
			ConfigureTimeline();
		}
	}

	private void RecordTickDiagnostics(ushort sequenceNumber, double serverTime, out float serverDeltaTime)
	{
		int lostTickCount = (hasReceivedFirstTick ? ((ushort)(sequenceNumber - lastReceivedSequenceNumber) - 1) : 0);
		double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
		serverDeltaTime = (hasReceivedFirstTick ? ((float)(serverTime - lastReceivedServerTime)) : 0f);
		float tickDeltaTime = (hasReceivedFirstTick ? ((float)(realtimeSinceStartupAsDouble - lastReceivedLocalTime)) : 0f);
		diagnostics.RecordTick(tickDeltaTime, lostTickCount);
		lastReceivedSequenceNumber = sequenceNumber;
		lastReceivedServerTime = serverTime;
		lastReceivedLocalTime = realtimeSinceStartupAsDouble;
		hasReceivedFirstTick = true;
	}

	private double GetTimelinePosition()
	{
		double num = lastReceivedServerTime + StallTime * (double)timeScale;
		if (!timeline.IsInitialized)
		{
			return lastReceivedServerTime - num;
		}
		return timeline.Time + (double)TargetTimelinePosition - num;
	}

	private void BufferObjects(SynchronizedObjectData[] synchronizedObjectsData, double serverTime)
	{
		float tickInterval = TickInterval;
		for (int i = 0; i < synchronizedObjectsData.Length; i++)
		{
			SynchronizedObjectData synchronizedObjectData = synchronizedObjectsData[i];
			if (registry.TryGet(synchronizedObjectData.NetworkObjectId, out var synchronizedObject) && TryMergeReceivedData(synchronizedObjectData, serverTime, out var mergedData))
			{
				synchronizedObject.Client_BufferSynchronizedObjectData(mergedData.GetPosition(), mergedData.Rotation, mergedData.LinearVelocity, mergedData.AngularVelocity, serverTime, tickInterval, mergedData.TickRateDivisor);
			}
		}
	}

	private bool TryMergeReceivedData(SynchronizedObjectData synchronizedObjectData, double serverTime, out SynchronizedObjectData mergedData)
	{
		ulong key = synchronizedObjectData.NetworkObjectId;
		mergedData = default;
		if (receivedStates.TryGetValue(key, out var value))
		{
			if (serverTime <= value.ServerTime)
			{
				return false;
			}
			mergedData = value.Data.Merge(synchronizedObjectData);
		}
		else
		{
			if (!synchronizedObjectData.HasAllComponents)
			{
				return false;
			}
			mergedData = synchronizedObjectData;
		}
		receivedStates[key] = new SynchronizedObjectReceivedState
		{
			Data = mergedData,
			ServerTime = serverTime
		};
		return true;
	}

	private void ConfigureTimeline()
	{
		timeline.Configure(Mathf.RoundToInt(1f / TickInterval), smoothingSettings.TimescaleCorrectionGain, smoothingSettings.MaxTimescaleCorrection, smoothingSettings.DriftEmaDuration, smoothingSettings.MaxTimelineDriftTime);
	}

	private void ResetInterpolationState()
	{
		timeline.Reset();
		foreach (SynchronizedObject @object in registry.Objects)
		{
			if ((bool)@object)
			{
				@object.Client_ResetInterpolation();
			}
		}
	}

	private void TriggerDiagnosticsEvents(float deltaTime)
	{
		if (diagnostics.TryGetData(deltaTime, out var data) && EventManager.HasEventListeners("Event_Client_OnObjectSynchronizationDiagnostics"))
		{
			data.TickInterval = TickInterval;
			data.TargetTimelinePosition = smoothingSettings.TargetTimelinePosition;
			EventManager.TriggerEvent("Event_Client_OnObjectSynchronizationDiagnostics", new Dictionary<string, object> { { "diagnosticsData", data } });
		}
	}

	private void TriggerSynchronizeEvents(float serverDeltaTime)
	{
		if (EventManager.HasEventListeners("Event_Client_OnSynchronizeObjects"))
		{
			EventManager.TriggerEvent("Event_Client_OnSynchronizeObjects", new Dictionary<string, object> { { "serverDeltaTime", serverDeltaTime } });
		}
	}
}
