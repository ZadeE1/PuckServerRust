using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public static class SceneManager
{
	private static readonly Logger Logger = new Logger("SceneManager");

	public static bool IsSceneLoadInProgress;

	public static bool IsInitialSceneLoaded;

	public static bool IsNetworkSceneManagerAvailable
	{
		get
		{
			if (NetworkManager.Singleton != null && NetworkManager.Singleton.SceneManager != null)
			{
				return NetworkManager.Singleton.IsServer;
			}
			return false;
		}
	}

	public static void Initialize()
	{
		UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
		UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;
		SceneManagerController.Initialize();
	}

	public static void Dispose()
	{
		UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
		UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded;
		SceneManagerController.Dispose();
	}

	public static void InitializeServer()
	{
		if (IsNetworkSceneManagerAvailable)
		{
			IsSceneLoadInProgress = false;
			IsInitialSceneLoaded = false;
			NetworkManager.Singleton.SceneManager.OnSceneEvent += Server_OnSceneEvent;
		}
	}

	public static void DisposeServer()
	{
		if (IsNetworkSceneManagerAvailable)
		{
			NetworkManager.Singleton.SceneManager.OnSceneEvent -= Server_OnSceneEvent;
			IsSceneLoadInProgress = false;
			IsInitialSceneLoaded = false;
		}
	}

	public static void LoadScene(string sceneName)
	{
		if (IsNetworkSceneManagerAvailable)
		{
			Logger.Info("Loading server scene " + sceneName);
			NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
		}
		else
		{
			Logger.Info("Loading scene " + sceneName);
			UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
		}
	}

	private static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
	{
		EventManager.TriggerEvent("Event_OnSceneLoaded", new Dictionary<string, object> { { "scene", scene } });
	}

	private static void OnSceneUnloaded(Scene scene)
	{
		EventManager.TriggerEvent("Event_OnSceneUnloaded", new Dictionary<string, object> { { "scene", scene } });
	}

	private static void Server_OnSceneEvent(SceneEvent sceneEvent)
	{
		switch (sceneEvent.SceneEventType)
		{
		case SceneEventType.Load:
			Server_OnLoadScene();
			break;
		case SceneEventType.LoadComplete:
			Server_OnClientSceneLoadComplete(sceneEvent.ClientId);
			break;
		case SceneEventType.SynchronizeComplete:
			Server_OnClientSceneSynchronizeComplete(sceneEvent.ClientId);
			break;
		case SceneEventType.UnloadComplete:
			Server_OnClientSceneUnloadComplete(sceneEvent.ClientId);
			break;
		case SceneEventType.LoadEventCompleted:
			Server_OnLoadSceneEventCompleted(sceneEvent.ClientsThatCompleted, sceneEvent.ClientsThatTimedOut);
			break;
		case SceneEventType.Unload:
		case SceneEventType.Synchronize:
		case SceneEventType.ReSynchronize:
		case SceneEventType.UnloadEventCompleted:
			break;
		}
	}

	private static void Server_OnLoadScene()
	{
		IsSceneLoadInProgress = true;
		Logger.Info("Server started loading scene");
		EventManager.TriggerEvent("Event_Server_OnLoadScene");
	}

	private static void Server_OnClientSceneLoadComplete(ulong clientId)
	{
		Logger.Info($"Client {clientId} completed scene load");
		EventManager.TriggerEvent("Event_Server_OnClientSceneLoadComplete", new Dictionary<string, object> { { "clientId", clientId } });
	}

	private static void Server_OnClientSceneUnloadComplete(ulong clientId)
	{
		Logger.Info($"Client {clientId} completed scene unload");
		EventManager.TriggerEvent("Event_Server_OnClientSceneUnloadComplete", new Dictionary<string, object> { { "clientId", clientId } });
	}

	private static void Server_OnClientSceneSynchronizeComplete(ulong clientId)
	{
		Logger.Info($"Client {clientId} completed scene synchronization");
		EventManager.TriggerEvent("Event_Server_OnClientSceneSynchronizeComplete", new Dictionary<string, object> { { "clientId", clientId } });
	}

	private static void Server_OnLoadSceneEventCompleted(List<ulong> clientsThatCompleted, List<ulong> clientsThatTimedOut)
	{
		Logger.Info("Scene load event completed on server");
		IsSceneLoadInProgress = false;
		bool flag = !IsInitialSceneLoaded;
		IsInitialSceneLoaded = true;
		EventManager.TriggerEvent("Event_Server_OnLoadSceneEventCompleted", new Dictionary<string, object>
		{
			{ "clientsThatCompleted", clientsThatCompleted },
			{ "clientsThatTimedOut", clientsThatTimedOut },
			{ "isInitialScene", flag }
		});
	}
}
