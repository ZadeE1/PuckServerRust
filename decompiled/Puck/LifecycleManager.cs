using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

public static class LifecycleManager
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void SubsystemRegistration()
	{
		LogManager.Initialize();
		Application.quitting += Dispose;
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void BeforeSceneLoad()
	{
		PerformanceMonitor.Initialize();
		PatchManager.Initialize();
		EventManager.Initialize();
		GlobalStateManager.Initialize();
		SaveManager.Initialize();
		InputManager.Initialize();
		SettingsManager.Initialize();
		ApplicationManager.Initialize();
		BackendManager.Initialize();
		ItemManager.Initialize();
		CameraManager.Initialize();
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void AfterSceneLoad()
	{
		SceneManager.Initialize();
		WebSocketManager.Initialize();
		SteamManager.Initialize();
		ModManager.Initialize();
		ServerReadinessManager.Initialize();
		RegisterUpdate();
	}

	private static void Update()
	{
		PerformanceMonitor.Tick();
		ApplicationManager.Update(Time.unscaledDeltaTime);
	}

	public static void Dispose()
	{
		UnregisterUpdate();
		ServerReadinessManager.Dispose();
		ModManager.Dispose();
		SteamManager.Dispose();
		WebSocketManager.Dispose();
		SceneManager.Dispose();
		CameraManager.Dispose();
		ItemManager.Dispose();
		BackendManager.Dispose();
		ApplicationManager.Dispose();
		SettingsManager.Dispose();
		InputManager.Dispose();
		SaveManager.Dispose();
		GlobalStateManager.Dispose();
		EventManager.Dispose();
		PerformanceMonitor.Dispose();
		PatchManager.Dispose();
		LogManager.Dispose();
	}

	private static void RegisterUpdate()
	{
		PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
		if (TryGetUpdateIndex(currentPlayerLoop, out var index))
		{
			List<PlayerLoopSystem> list = new List<PlayerLoopSystem>(currentPlayerLoop.subSystemList[index].subSystemList)
			{
				new PlayerLoopSystem
				{
					type = typeof(LifecycleManager),
					updateDelegate = Update
				}
			};
			currentPlayerLoop.subSystemList[index].subSystemList = list.ToArray();
			PlayerLoop.SetPlayerLoop(currentPlayerLoop);
		}
	}

	private static void UnregisterUpdate()
	{
		PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
		if (TryGetUpdateIndex(currentPlayerLoop, out var index))
		{
			currentPlayerLoop.subSystemList[index].subSystemList = Array.FindAll(currentPlayerLoop.subSystemList[index].subSystemList, (PlayerLoopSystem subSystem) => subSystem.type != typeof(LifecycleManager));
			PlayerLoop.SetPlayerLoop(currentPlayerLoop);
		}
	}

	private static bool TryGetUpdateIndex(PlayerLoopSystem playerLoop, out int index)
	{
		index = Array.FindIndex(playerLoop.subSystemList, (PlayerLoopSystem subSystem) => subSystem.type == typeof(Update));
		return index >= 0;
	}
}
