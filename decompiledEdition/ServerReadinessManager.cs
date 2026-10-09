using System.Collections.Generic;
using System.Linq;
using DG.Tweening;

public static class ServerReadinessManager
{
	private static readonly Logger Logger = new Logger("ServerReadinessManager");

	private static ServerConfig serverConfig;

	private static string[] requiredModIds = new string[0];

	private static bool modWaitTimedOut = false;

	private static Tween progressPollTween;

	private static Tween timeoutTween;

	public static ServerReadinessPhase Phase { get; private set; } = ServerReadinessPhase.NotReady;

	public static bool IsReady => Phase == ServerReadinessPhase.Ready;

	public static void Initialize()
	{
		ServerReadinessManagerController.Initialize();
	}

	public static void Dispose()
	{
		Reset();
		ServerReadinessManagerController.Dispose();
	}

	public static void BeginTracking(ServerConfig config)
	{
		serverConfig = config;
		requiredModIds = config.EnabledModIds;
		modWaitTimedOut = false;
		SetPhase(ServerReadinessPhase.Starting);
		Logger.Info($"Waiting for backend authentication and {requiredModIds.Length} server mod(s) to download and enable");
		if (!Evaluate())
		{
			StartProgressPoll();
			StartTimeout();
		}
	}

	public static bool Evaluate()
	{
		if (Phase == ServerReadinessPhase.Ready)
		{
			return true;
		}
		if (!IsAuthenticated())
		{
			return false;
		}
		if (modWaitTimedOut || requiredModIds.All(IsModReady))
		{
			MarkReady();
			return true;
		}
		return false;
	}

	public static void Reset()
	{
		Stop();
		serverConfig = null;
		requiredModIds = new string[0];
		modWaitTimedOut = false;
		SetPhase(ServerReadinessPhase.NotReady);
	}

	private static bool IsAuthenticated()
	{
		return BackendManager.ServerState.AuthenticationPhase == AuthenticationPhase.Authenticated;
	}

	private static bool IsModReady(string modId)
	{
		Mod modById = ModManager.GetModById(modId);
		if (modById != null && modById.IsReady)
		{
			return modById.IsEnabled;
		}
		return false;
	}

	private static void MarkReady()
	{
		if (Phase != ServerReadinessPhase.Ready)
		{
			Stop();
			Logger.Info("Backend authenticated and all server mods ready; loading level");
			SetPhase(ServerReadinessPhase.Ready);
			EventManager.TriggerEvent("Event_Server_OnReady", new Dictionary<string, object> { { "serverConfig", serverConfig } });
		}
	}

	public static void AnnounceReady()
	{
		if (Phase == ServerReadinessPhase.Ready && serverConfig != null)
		{
			string arg = BackendManager.ServerState.ServerData?.ipAddress ?? "0.0.0.0";
			ushort num = BackendManager.ServerState.ServerData?.port ?? serverConfig.port;
			Logger.Info($"Puck B{ApplicationManager.Version} is ready to accept clients on {arg}:{num}!");
		}
	}

	private static void SetPhase(ServerReadinessPhase phase)
	{
		if (Phase != phase)
		{
			ServerReadinessPhase phase2 = Phase;
			Phase = phase;
			EventManager.TriggerEvent("Event_Server_OnReadinessChanged", new Dictionary<string, object>
			{
				{ "oldPhase", phase2 },
				{ "newPhase", phase }
			});
		}
	}

	private static void Stop()
	{
		progressPollTween?.Kill();
		progressPollTween = null;
		timeoutTween?.Kill();
		timeoutTween = null;
	}

	private static void StartProgressPoll()
	{
		progressPollTween?.Kill();
		progressPollTween = DOVirtual.DelayedCall(2f, LogProgress).SetLoops(-1);
	}

	private static void StartTimeout()
	{
		timeoutTween?.Kill();
		timeoutTween = DOVirtual.DelayedCall(300f, OnTimeout);
	}

	private static void OnTimeout()
	{
		modWaitTimedOut = true;
		string[] value = requiredModIds.Where((string modId) => !IsModReady(modId)).ToArray();
		if (!IsAuthenticated())
		{
			Logger.Error(string.Format("Timed out after {0}s but the server is not authenticated with the backend; staying closed until it authenticates (pending mods: [{1}])", 300f, string.Join(", ", value)));
			return;
		}
		Logger.Error(string.Format("Timed out after {0}s waiting for server mod(s) [{1}]; accepting players anyway", 300f, string.Join(", ", value)));
		MarkReady();
	}

	private static void LogProgress()
	{
		if (!IsAuthenticated())
		{
			Logger.Info("Waiting for backend authentication...");
		}
		int num = requiredModIds.Count(IsModReady);
		Logger.Info($"Server mods: {num}/{requiredModIds.Length} ready");
		string[] array = requiredModIds;
		foreach (string text in array)
		{
			if (!IsModReady(text))
			{
				if (SteamWorkshopManager.GetItemDownloadInfo(text, out var bytesDownloaded, out var bytesTotal) && bytesTotal != 0)
				{
					float num2 = (float)bytesDownloaded / 1048576f;
					float num3 = (float)bytesTotal / 1048576f;
					float num4 = (float)bytesDownloaded / (float)bytesTotal * 100f;
					Logger.Info($"Downloading {text}: {num4:0.0}% ({num2:0.0}/{num3:0.0} MB)");
				}
				else
				{
					Logger.Info("Downloading " + text + ": waiting for download to start");
				}
			}
		}
	}
}
