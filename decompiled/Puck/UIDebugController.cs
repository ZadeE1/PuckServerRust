using System;
using System.Collections.Generic;

public class UIDebugController : UIViewController<UIDebug>
{
	private const double LOSS_WARNING_PERCENTAGE = 0.01;

	private const double LOSS_ERROR_PERCENTAGE = 0.02;

	private const double REORDERED_WARNING_COUNT = 1.0;

	private const double REORDERED_ERROR_COUNT = 2.0;

	private const double TIMELINE_WARNING_DRIFT_TICKS = 0.5;

	private const double TIMELINE_ERROR_DRIFT_TICKS = 1.0;

	private const double SNAP_WARNING_COUNT = 1.0;

	private const double SNAP_ERROR_COUNT = 2.0;

	private UIDebug uiDebug;

	private Player localPlayer;

	public override void Awake()
	{
		base.Awake();
		uiDebug = GetComponent<UIDebug>();
		EventManager.AddEventListener("Event_OnDebugChanged", Event_OnDebugChanged);
		EventManager.AddEventListener("Event_OnKeyBindsLoaded", Event_OnKeyBindsChanged);
		EventManager.AddEventListener("Event_OnKeyBindsSaved", Event_OnKeyBindsChanged);
		EventManager.AddEventListener("Event_Client_OnObjectSynchronizationDiagnostics", Event_Client_OnObjectSynchronizationDiagnostics);
		EventManager.AddEventListener("Event_OnApplicationDiagnostics", Event_OnApplicationDiagnostics);
		EventManager.AddEventListener("Event_Server_OnNetworkDiagnostics", Event_Server_OnNetworkDiagnostics);
		EventManager.AddEventListener("Event_Client_OnNetworkDiagnostics", Event_Client_OnNetworkDiagnostics);
		EventManager.AddEventListener("Event_OnClientStopped", Event_OnClientStopped);
		EventManager.AddEventListener("Event_Everyone_OnPlayerSpawned", Event_Everyone_OnPlayerSpawned);
		EventManager.AddEventListener("Event_Everyone_OnPlayerDespawned", Event_Everyone_OnPlayerDespawned);
	}

	private void Start()
	{
		uiDebug.SetBuild($"PUCK B{ApplicationManager.Version} {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");
		RefreshHint();
		uiDebug.SetMode(SettingsManager.Debug);
	}

	public override void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_OnDebugChanged", Event_OnDebugChanged);
		EventManager.RemoveEventListener("Event_OnKeyBindsLoaded", Event_OnKeyBindsChanged);
		EventManager.RemoveEventListener("Event_OnKeyBindsSaved", Event_OnKeyBindsChanged);
		EventManager.RemoveEventListener("Event_Client_OnObjectSynchronizationDiagnostics", Event_Client_OnObjectSynchronizationDiagnostics);
		EventManager.RemoveEventListener("Event_OnApplicationDiagnostics", Event_OnApplicationDiagnostics);
		EventManager.RemoveEventListener("Event_Server_OnNetworkDiagnostics", Event_Server_OnNetworkDiagnostics);
		EventManager.RemoveEventListener("Event_Client_OnNetworkDiagnostics", Event_Client_OnNetworkDiagnostics);
		EventManager.RemoveEventListener("Event_OnClientStopped", Event_OnClientStopped);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerSpawned", Event_Everyone_OnPlayerSpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerDespawned", Event_Everyone_OnPlayerDespawned);
		base.OnDestroy();
	}

	private void Event_OnKeyBindsChanged(Dictionary<string, object> message)
	{
		RefreshHint();
	}

	private void RefreshHint()
	{
		string text = (InputManager.KeyBinds.TryGetValue(InputManager.Debug1Action.name, out var value) ? InputManager.GetKeyBindDisplayString(value) : "F5");
		uiDebug.SetHint("[" + text + "]");
	}

	private void Event_OnDebugChanged(Dictionary<string, object> message)
	{
		DebugMode debugMode = (DebugMode)message["value"];
		uiDebug.SetMode(debugMode);
		if (debugMode != DebugMode.Off)
		{
			uiDebug.Show();
		}
		else
		{
			uiDebug.Hide();
		}
	}

	private void Event_Everyone_OnPlayerSpawned(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		if (player.IsLocalPlayer)
		{
			localPlayer = player;
		}
	}

	private void Event_Everyone_OnPlayerDespawned(Dictionary<string, object> message)
	{
		if (!((Player)message["player"] != localPlayer))
		{
			localPlayer = null;
		}
	}

	private void Event_OnClientStopped(Dictionary<string, object> message)
	{
		uiDebug.SetNetworkVisible(value: false);
		uiDebug.SetObjectSynchronizationVisible(value: false);
		uiDebug.SetSimpleRoundTripTime(string.Empty);
		uiDebug.SetSimpleLoss(string.Empty);
	}

	private void Event_OnApplicationDiagnostics(Dictionary<string, object> message)
	{
		ApplicationDiagnostics applicationDiagnostics = (ApplicationDiagnostics)message["applicationDiagnostics"];
		uiDebug.SetApplicationVisible(value: true);
		string text = $"{applicationDiagnostics.FramesPerSecond:0.0}/s";
		string milliseconds = GetMilliseconds(applicationDiagnostics.P95FrameTime);
		uiDebug.SetApplicationFrames(text);
		uiDebug.SetApplicationFrameTime(GetMilliseconds(applicationDiagnostics.AverageFrameTime), milliseconds, GetMilliseconds(applicationDiagnostics.MaxFrameTime));
		uiDebug.SetSimpleApplication(text, milliseconds);
	}

	private void Event_Server_OnNetworkDiagnostics(Dictionary<string, object> message)
	{
		RefreshNetworkDiagnostics(message);
	}

	private void Event_Client_OnNetworkDiagnostics(Dictionary<string, object> message)
	{
		RefreshNetworkDiagnostics(message);
	}

	private void RefreshNetworkDiagnostics(Dictionary<string, object> message)
	{
		NetworkDiagnostics networkDiagnostics = (NetworkDiagnostics)message["networkDiagnostics"];
		uiDebug.SetNetworkVisible(value: true);
		uiDebug.SetNetworkSent(GetKilobytesPerSecond(networkDiagnostics.SentKilobytesPerSecond), GetPacketsPerSecond(networkDiagnostics.SentPacketsPerSecond));
		uiDebug.SetNetworkReceived(GetKilobytesPerSecond(networkDiagnostics.ReceivedKilobytesPerSecond), GetPacketsPerSecond(networkDiagnostics.ReceivedPacketsPerSecond));
		string roundTripTimeText = GetRoundTripTimeText();
		uiDebug.SetNetworkRoundTripTime(roundTripTimeText);
		uiDebug.SetNetworkStall(GetMilliseconds(networkDiagnostics.MaxStallTime));
		uiDebug.SetSimpleRoundTripTime(roundTripTimeText);
	}

	private static string GetKilobytesPerSecond(float kilobytesPerSecond)
	{
		return $"{kilobytesPerSecond:0.0}KB/s";
	}

	private static string GetPacketsPerSecond(float packetsPerSecond)
	{
		return $"{packetsPerSecond:0}/s";
	}

	private void Event_Client_OnObjectSynchronizationDiagnostics(Dictionary<string, object> message)
	{
		SynchronizedObjectClientDiagnosticsData diagnosticsData = (SynchronizedObjectClientDiagnosticsData)message["diagnosticsData"];
		uiDebug.SetObjectSynchronizationVisible(value: true);
		uiDebug.SetObjectSynchronizationTicks($"{GetTicksPerSecond(diagnosticsData):0.0}/s");
		uiDebug.SetObjectSynchronizationTickDelta(GetMilliseconds(diagnosticsData.AverageTickDeltaTime), GetMilliseconds(diagnosticsData.P95TickDeltaTime), GetMilliseconds(diagnosticsData.MaxTickDeltaTime));
		string lossText = GetLossText(diagnosticsData);
		uiDebug.SetObjectSynchronizationLoss(lossText);
		uiDebug.SetSimpleLoss(lossText);
		uiDebug.SetObjectSynchronizationReordered(GetReorderedText(diagnosticsData));
		uiDebug.SetObjectSynchronizationTimeline(GetTimelineText(diagnosticsData));
		uiDebug.SetObjectSynchronizationSpeed($"{diagnosticsData.AverageTimelineTimescale:0.000}");
		uiDebug.SetObjectSynchronizationSnaps(GetSnapsText(diagnosticsData));
		uiDebug.SetObjectSynchronizationExtrapolation($"{diagnosticsData.ExtrapolationPercentage:0.0}%");
	}

	private static string GetLossText(SynchronizedObjectClientDiagnosticsData diagnosticsData)
	{
		return GetThresholdText($"{diagnosticsData.TickLossPercentage:0.00}%", diagnosticsData.TickLossPercentage, 0.01, 0.02);
	}

	private static string GetReorderedText(SynchronizedObjectClientDiagnosticsData diagnosticsData)
	{
		return GetThresholdText($"{diagnosticsData.OutOfOrderTickCount}", diagnosticsData.OutOfOrderTickCount, 1.0, 2.0);
	}

	private string GetRoundTripTimeText()
	{
		if (!localPlayer)
		{
			return string.Empty;
		}
		return $"{localPlayer.Ping.Value}ms";
	}

	private static string GetSnapsText(SynchronizedObjectClientDiagnosticsData diagnosticsData)
	{
		return GetThresholdText($"{diagnosticsData.SnapCount}", diagnosticsData.SnapCount, 1.0, 2.0);
	}

	private static string GetTimelineMilliseconds(SynchronizedObjectClientDiagnosticsData diagnosticsData, double timelinePosition)
	{
		return GetMilliseconds(timelinePosition * (double)diagnosticsData.TickInterval);
	}

	private static string GetTimelineText(SynchronizedObjectClientDiagnosticsData diagnosticsData)
	{
		double value = Math.Abs(diagnosticsData.AverageTimelinePosition - diagnosticsData.TargetTimelinePosition);
		string thresholdText = GetThresholdText($"{diagnosticsData.AverageTimelinePosition:0.00} ({GetTimelineMilliseconds(diagnosticsData, diagnosticsData.AverageTimelinePosition)})", value, 0.5, 1.0);
		string text = StringUtils.WrapInColor($"/ {diagnosticsData.TargetTimelinePosition:0.00} ({GetTimelineMilliseconds(diagnosticsData, diagnosticsData.TargetTimelinePosition)})", "#b8b8b8");
		return thresholdText + " " + text;
	}

	private static string GetThresholdText(string text, double value, double warningThreshold, double errorThreshold)
	{
		if (value >= errorThreshold)
		{
			return StringUtils.WrapInColor(text, "#e74c3c");
		}
		if (value >= warningThreshold)
		{
			return StringUtils.WrapInColor(text, "#ffe97f");
		}
		return text;
	}

	private static float GetTicksPerSecond(SynchronizedObjectClientDiagnosticsData diagnosticsData)
	{
		if (diagnosticsData.WindowTime <= 0f)
		{
			return 0f;
		}
		return (float)diagnosticsData.TickCount / diagnosticsData.WindowTime;
	}

	private static string GetMilliseconds(double seconds)
	{
		return $"{seconds * 1000.0:0.0}ms";
	}
}
