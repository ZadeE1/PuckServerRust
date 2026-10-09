using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class ApplicationManager
{
	private static readonly Logger Logger = new Logger("ApplicationManager");

	private static bool isDisplayChangeInProgress = false;

	private static Tween mouseVisibilityDebounceTween;

	private const float SERVER_DIAGNOSTICS_INTERVAL = 0f;

	private const float CLIENT_DIAGNOSTICS_INTERVAL = 1f;

	private const int FRAME_TIME_BUCKET_COUNT = 201;

	private static TimeHistogram frameTimeHistogram = new TimeHistogram(0.0005f, 201);

	private static float diagnosticsTime = 0f;

	// ponytail: cache the Unity extern call — this guard runs ~5x/frame at uncapped batchmode rates.
	private static readonly bool isDedicatedGameServer = Application.isBatchMode;

	public static bool IsDedicatedGameServer => isDedicatedGameServer;

	public static ushort Version
	{
		get
		{
			if (!ushort.TryParse(Application.version, out var result))
			{
				return 0;
			}
			return result;
		}
	}

	private static bool IsDisplayChangeInProgress
	{
		get
		{
			return isDisplayChangeInProgress;
		}
		set
		{
			if (isDisplayChangeInProgress != value)
			{
				isDisplayChangeInProgress = value;
				OnIsDisplayChangeInProgressChanged();
			}
		}
	}

	private static float DiagnosticsInterval
	{
		get
		{
			if (!IsDedicatedGameServer)
			{
				return 1f;
			}
			return 0f;
		}
	}

	public static void Initialize()
	{
		ApplicationManagerController.Initialize();
	}

	public static void Dispose()
	{
		ApplicationManagerController.Dispose();
		mouseVisibilityDebounceTween?.Kill();
	}

	public static void Update(float deltaTime)
	{
		float diagnosticsInterval = DiagnosticsInterval;
		if (!(diagnosticsInterval <= 0f))
		{
			RecordFrame(deltaTime);
			diagnosticsTime += deltaTime;
			if (!(diagnosticsTime < diagnosticsInterval))
			{
				TakeDiagnostics();
			}
		}
	}

	private static void RecordFrame(float frameTime)
	{
		if (!(frameTime <= 0f))
		{
			frameTimeHistogram.Add(frameTime);
		}
	}

	private static void TakeDiagnostics()
	{
		TriggerDiagnosticsEvent(GetApplicationDiagnostics());
		diagnosticsTime = 0f;
		frameTimeHistogram.Reset();
	}

	private static ApplicationDiagnostics GetApplicationDiagnostics()
	{
		return new ApplicationDiagnostics
		{
			WindowTime = diagnosticsTime,
			FrameCount = frameTimeHistogram.Count,
			AverageFrameTime = frameTimeHistogram.Average,
			P95FrameTime = frameTimeHistogram.GetPercentile(0.95),
			MaxFrameTime = frameTimeHistogram.Max
		};
	}

	private static void TriggerDiagnosticsEvent(ApplicationDiagnostics applicationDiagnostics)
	{
		if (EventManager.HasEventListeners("Event_OnApplicationDiagnostics"))
		{
			EventManager.TriggerEvent("Event_OnApplicationDiagnostics", new Dictionary<string, object> { { "applicationDiagnostics", applicationDiagnostics } });
		}
	}

	public static async Task SetDisplay(int index)
	{
		Logger.Info($"Setting display to {index}");
		if (!IsDisplayChangeInProgress)
		{
			List<DisplayInfo> displayLayout = Utils.GetDisplayLayout();
			if (index >= 0 && index < displayLayout.Count)
			{
				IsDisplayChangeInProgress = true;
				await Screen.MoveMainWindowTo(displayLayout[index], Vector2Int.zero);
				IsDisplayChangeInProgress = false;
			}
		}
	}

	public static void SetResolution(int index, FullScreenMode mode)
	{
		Logger.Info($"Setting resolution to {index} with full screen mode {mode}");
		if (!IsDisplayChangeInProgress)
		{
			List<Resolution> resolutions = Utils.GetResolutions();
			Resolution resolution = ((index < 0 || index >= resolutions.Count) ? Screen.currentResolution : resolutions[index]);
			Screen.SetResolution(resolution.width, resolution.height, mode, resolution.refreshRateRatio);
		}
	}

	public static void SetVSync(bool isEnabled)
	{
		Logger.Info($"Setting vSync to {isEnabled}");
		QualitySettings.vSyncCount = (isEnabled ? 1 : 0);
	}

	public static void SetTargetFrameRate(int targetFrameRate)
	{
		Logger.Info($"Setting target frame rate to {targetFrameRate}");
		Application.targetFrameRate = targetFrameRate;
	}

	public static void SetQuality(ApplicationQuality quality)
	{
		Logger.Info($"Setting quality to {quality}");
		int vSyncCount = QualitySettings.vSyncCount;
		switch (quality)
		{
		case ApplicationQuality.Low:
			QualitySettings.SetQualityLevel(0, applyExpensiveChanges: true);
			break;
		case ApplicationQuality.Medium:
			QualitySettings.SetQualityLevel(2, applyExpensiveChanges: true);
			break;
		case ApplicationQuality.High:
			QualitySettings.SetQualityLevel(4, applyExpensiveChanges: true);
			break;
		case ApplicationQuality.Ultra:
			QualitySettings.SetQualityLevel(5, applyExpensiveChanges: true);
			break;
		default:
			QualitySettings.SetQualityLevel(4, applyExpensiveChanges: true);
			break;
		}
		QualitySettings.vSyncCount = vSyncCount;
	}

	public static void SetShadowQuality(ShadowQuality shadowQuality)
	{
		Logger.Info($"Setting shadow quality to {shadowQuality}");
		UniversalRenderPipelineAsset asset = UniversalRenderPipeline.asset;
		if (asset == null)
		{
			Logger.Error("Render pipeline asset is null");
			return;
		}
		(int, float) tuple = shadowQuality switch
		{
			ShadowQuality.Low => (1024, 25f), 
			ShadowQuality.Medium => (2048, 40f), 
			ShadowQuality.High => (4096, 50f), 
			ShadowQuality.Ultra => (8192, 50f), 
			_ => (4096, 50f), 
		};
		int item = tuple.Item1;
		float item2 = tuple.Item2;
		asset.mainLightShadowmapResolution = item;
		asset.shadowDistance = item2;
		asset.shadowCascadeCount = 4;
		asset.cascade4Split = Constants.SHADOW_CASCADE_RANGES / item2;
	}

	public static void SetMouseVisibility(bool isVisible)
	{
		mouseVisibilityDebounceTween?.Kill();
		mouseVisibilityDebounceTween = DOVirtual.DelayedCall(0f, () =>
		{
			Logger.Info($"Setting mouse visibility to {isVisible}");
			if (isVisible)
			{
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
			}
			else
			{
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
			}
		});
	}

	private static void OnIsDisplayChangeInProgressChanged()
	{
		Logger.Info($"Display change in progress: {isDisplayChangeInProgress}");
		EventManager.TriggerEvent("Event_OnIsDisplayChangeInProgressChanged", new Dictionary<string, object> { { "isDisplayChangeInProgress", isDisplayChangeInProgress } });
	}
}
