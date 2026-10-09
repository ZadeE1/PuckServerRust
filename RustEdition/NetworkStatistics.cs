using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Analytics;
using UnityEngine;

public class NetworkStatistics : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField]
	[Tooltip("Seconds between Event_Server_OnNetworkDiagnostics events measuring transport traffic while running as the server. 0 disables diagnostics.")]
	private float serverDiagnosticsInterval;

	[SerializeField]
	[Tooltip("Seconds between Event_Client_OnNetworkDiagnostics events measuring transport traffic while running as a client, for UI display. 0 disables diagnostics.")]
	private float clientDiagnosticsInterval = 1f;

	private float diagnosticsTime;

	private DriverStatistics? lastDriverStatistics;

	private ulong lastReceivedPacketCount;

	private double lastReceivedPacketTime;

	private bool hasPolledDriver;

	private double maxStallTime;

	private static bool IsServer
	{
		get
		{
			if (NetworkManager.Singleton != null)
			{
				return NetworkManager.Singleton.IsServer;
			}
			return false;
		}
	}

	private float DiagnosticsInterval
	{
		get
		{
			if (!IsServer)
			{
				return clientDiagnosticsInterval;
			}
			return serverDiagnosticsInterval;
		}
	}

	private void Update()
	{
		float diagnosticsInterval = DiagnosticsInterval;
		// ponytail: check the interval BEFORE polling the driver — the poll + stall
	// clock are Unity/transport externs that used to run ~11k times/sec headless
	// to emit a ~1Hz event.
	if (diagnosticsInterval <= 0f)
		{
			return;
		}
		diagnosticsTime += Time.unscaledDeltaTime;
		if (diagnosticsTime < diagnosticsInterval)
		{
			return;
		}
		if (!TryGetDriverStatistics(out var driverStatistics))
		{
			ResetDiagnostics();
			return;
		}
		RecordStall(driverStatistics.RxTotalPackets);
		TakeDiagnostics(in driverStatistics);
	}

	private void ResetDiagnostics()
	{
		lastDriverStatistics = null;
		hasPolledDriver = false;
		diagnosticsTime = 0f;
		maxStallTime = 0.0;
	}

	private void RecordStall(ulong receivedPacketCount)
	{
		double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
		if (!hasPolledDriver || receivedPacketCount != lastReceivedPacketCount)
		{
			lastReceivedPacketCount = receivedPacketCount;
			lastReceivedPacketTime = realtimeSinceStartupAsDouble;
			hasPolledDriver = true;
		}
		maxStallTime = Math.Max(maxStallTime, realtimeSinceStartupAsDouble - lastReceivedPacketTime);
	}

	private void TakeDiagnostics(in DriverStatistics driverStatistics)
	{
		if (lastDriverStatistics.HasValue)
		{
			TriggerDiagnosticsEvent(GetNetworkDiagnostics(in driverStatistics, lastDriverStatistics.Value));
		}
		lastDriverStatistics = driverStatistics;
		diagnosticsTime = 0f;
		maxStallTime = 0.0;
	}

	private NetworkDiagnostics GetNetworkDiagnostics(in DriverStatistics driverStatistics, in DriverStatistics lastStatistics)
	{
		return new NetworkDiagnostics
		{
			WindowTime = diagnosticsTime,
			MaxStallTime = maxStallTime,
			SentWireByteCount = GetDelta(driverStatistics.TxTotalBytes, lastStatistics.TxTotalBytes),
			ReceivedWireByteCount = GetDelta(driverStatistics.RxTotalBytes, lastStatistics.RxTotalBytes),
			SentPacketCount = GetDelta(driverStatistics.TxTotalPackets, lastStatistics.TxTotalPackets),
			ReceivedPacketCount = GetDelta(driverStatistics.RxTotalPackets, lastStatistics.RxTotalPackets)
		};
	}

	private void TriggerDiagnosticsEvent(NetworkDiagnostics networkDiagnostics)
	{
		string eventName = (IsServer ? "Event_Server_OnNetworkDiagnostics" : "Event_Client_OnNetworkDiagnostics");
		if (EventManager.HasEventListeners(eventName))
		{
			EventManager.TriggerEvent(eventName, new Dictionary<string, object> { { "networkDiagnostics", networkDiagnostics } });
		}
	}

	private static bool TryGetDriverStatistics(out DriverStatistics driverStatistics)
	{
		driverStatistics = default;
		NetworkManager singleton = NetworkManager.Singleton;
		if (singleton == null || !singleton.IsListening)
		{
			return false;
		}
		UnityTransport unityTransport = singleton.NetworkConfig.NetworkTransport as UnityTransport;
		if (!unityTransport)
		{
			return false;
		}
		ref NetworkDriver networkDriver = ref unityTransport.GetNetworkDriver();
		if (!networkDriver.IsCreated)
		{
			return false;
		}
		driverStatistics = networkDriver.GetStatistics();
		return true;
	}

	private static ulong GetDelta(ulong current, ulong last)
	{
		if (current < last)
		{
			return current;
		}
		return current - last;
	}
}
