using System;
using System.Collections.Generic;
using System.Net;
using DG.Tweening;
using Steamworks;

public static class SteamManager
{
	private static readonly Logger Logger = new Logger("SteamManager");

	public static bool IsInitialized = false;

	public static float RunCallbackInterval = 1f / 30f;

	public static bool IsConnected = false;

	private static Callback<SteamServersConnected_t> steamServersConnectedCallback;

	private static Callback<SteamServerConnectFailure_t> steamServerConnectFailureCallback;

	private static Callback<SteamServersDisconnected_t> steamServersDisconnectedCallback;

	private static Tween callbackTween;

	private static Tween steamInitializationRetryTween;

	private static int steamInitializationAttempts = 0;

	public static void Initialize()
	{
		SteamManagerController.Initialize();
		InitializeSteam();
	}

	public static void Dispose()
	{
		DisposeSteam();
		SteamManagerController.Dispose();
	}

	private static void InitializeSteam()
	{
		if (IsInitialized)
		{
			return;
		}
		Logger.Info("Initializing Steam");
		EventManager.TriggerEvent("Event_OnSteamInitializationStarted");
		if (ApplicationManager.IsDedicatedGameServer)
		{
			IsInitialized = GameServer.Init(BitConverter.ToUInt32(IPAddress.Any.GetAddressBytes(), 0), 0, 0, EServerMode.eServerModeNoAuthentication, null);
		}
		else
		{
			IsInitialized = SteamAPI.Init();
		}
		if (IsInitialized)
		{
			steamInitializationAttempts = 0;
			RegisterCallbacks();
			StartCallbackLoop();
			if (ApplicationManager.IsDedicatedGameServer)
			{
				Logger.Info("Initialized as game server");
				EventManager.TriggerEvent("Event_OnSteamInitialized");
				SteamGameServer.LogOnAnonymous();
			}
			else
			{
				Logger.Info("Initialized as client");
				EventManager.TriggerEvent("Event_OnSteamInitialized");
				OnSteamServersConnected(default);
			}
		}
		else
		{
			steamInitializationAttempts++;
			if (ApplicationManager.IsDedicatedGameServer)
			{
				Logger.Info($"Failed to initialize as game server (attempt {steamInitializationAttempts})");
			}
			else
			{
				Logger.Info($"Failed to initialize as client (attempt {steamInitializationAttempts})");
			}
			EventManager.TriggerEvent("Event_OnSteamInitializationFailed", new Dictionary<string, object> { { "attempts", steamInitializationAttempts } });
			steamInitializationRetryTween?.Kill();
			steamInitializationRetryTween = DOVirtual.DelayedCall(5f, () =>
			{
				Logger.Info("Retrying Steam initialization");
				InitializeSteam();
			});
		}
	}

	private static void DisposeSteam()
	{
		if (IsInitialized)
		{
			steamInitializationRetryTween?.Kill();
			if (ApplicationManager.IsDedicatedGameServer)
			{
				GameServer.Shutdown();
			}
			else
			{
				SteamAPI.Shutdown();
			}
			StopCallbackLoop();
			UnregisterCallbacks();
			IsInitialized = false;
		}
	}

	private static void RegisterCallbacks()
	{
		if (IsInitialized)
		{
			if (ApplicationManager.IsDedicatedGameServer)
			{
				steamServersConnectedCallback = Callback<SteamServersConnected_t>.CreateGameServer(OnSteamServersConnected);
				steamServerConnectFailureCallback = Callback<SteamServerConnectFailure_t>.CreateGameServer(OnSteamServerConnectFailure);
				steamServersDisconnectedCallback = Callback<SteamServersDisconnected_t>.CreateGameServer(OnSteamServersDisconnected);
			}
			else
			{
				steamServersConnectedCallback = Callback<SteamServersConnected_t>.Create(OnSteamServersConnected);
				steamServerConnectFailureCallback = Callback<SteamServerConnectFailure_t>.Create(OnSteamServerConnectFailure);
				steamServersDisconnectedCallback = Callback<SteamServersDisconnected_t>.Create(OnSteamServersDisconnected);
			}
		}
	}

	private static void UnregisterCallbacks()
	{
		if (IsInitialized)
		{
			steamServersConnectedCallback.Unregister();
			steamServerConnectFailureCallback.Unregister();
			steamServersDisconnectedCallback.Unregister();
		}
	}

	private static void StartCallbackLoop()
	{
		if (!IsInitialized)
		{
			return;
		}
		callbackTween?.Kill();
		callbackTween = DOVirtual.DelayedCall(RunCallbackInterval, () =>
		{
			if (ApplicationManager.IsDedicatedGameServer)
			{
				GameServer.RunCallbacks();
			}
			else
			{
				SteamAPI.RunCallbacks();
			}
			StartCallbackLoop();
		});
	}

	private static void StopCallbackLoop()
	{
		if (IsInitialized)
		{
			callbackTween?.Kill();
			callbackTween = null;
		}
	}

	private static void OnSteamServersConnected(SteamServersConnected_t callback)
	{
		Logger.Info("Connected to Steam");
		SteamIntegrationManager.Initialize();
		SteamWorkshopManager.Initialize();
		IsConnected = true;
		EventManager.TriggerEvent("Event_OnSteamConnected");
	}

	private static void OnSteamServerConnectFailure(SteamServerConnectFailure_t callback)
	{
		Logger.Error($"Failed to connect to Steam: {callback.m_eResult}");
		EventManager.TriggerEvent("Event_OnSteamConnectionFailed");
	}

	private static void OnSteamServersDisconnected(SteamServersDisconnected_t callback)
	{
		Logger.Warning($"Disconnected from Steam: {callback.m_eResult}");
		IsConnected = false;
		SteamWorkshopManager.Dispose();
		SteamIntegrationManager.Dispose();
		EventManager.TriggerEvent("Event_OnSteamDisconnected");
	}
}
