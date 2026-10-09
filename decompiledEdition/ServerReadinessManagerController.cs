using System.Collections.Generic;

public static class ServerReadinessManagerController
{
	public static void Initialize()
	{
		EventManager.AddEventListener("Event_Server_OnServerStarted", Event_Server_OnServerStarted);
		EventManager.AddEventListener("Event_Server_OnServerStopped", Event_Server_OnServerStopped);
		EventManager.AddEventListener("Event_OnModStateChanged", Event_OnModStateChanged);
		EventManager.AddEventListener("Event_OnServerStateChanged", Event_OnServerStateChanged);
		EventManager.AddEventListener("Event_Server_OnLoadSceneEventCompleted", Event_Server_OnLoadSceneEventCompleted);
	}

	public static void Dispose()
	{
		EventManager.RemoveEventListener("Event_Server_OnServerStarted", Event_Server_OnServerStarted);
		EventManager.RemoveEventListener("Event_Server_OnServerStopped", Event_Server_OnServerStopped);
		EventManager.RemoveEventListener("Event_OnModStateChanged", Event_OnModStateChanged);
		EventManager.RemoveEventListener("Event_OnServerStateChanged", Event_OnServerStateChanged);
		EventManager.RemoveEventListener("Event_Server_OnLoadSceneEventCompleted", Event_Server_OnLoadSceneEventCompleted);
	}

	private static void Event_Server_OnServerStarted(Dictionary<string, object> message)
	{
		ServerReadinessManager.BeginTracking((ServerConfig)message["serverConfig"]);
	}

	private static void Event_Server_OnServerStopped(Dictionary<string, object> message)
	{
		ServerReadinessManager.Reset();
	}

	private static void Event_OnModStateChanged(Dictionary<string, object> message)
	{
		if (ServerReadinessManager.Phase == ServerReadinessPhase.Starting)
		{
			ServerReadinessManager.Evaluate();
		}
	}

	private static void Event_OnServerStateChanged(Dictionary<string, object> message)
	{
		if (ServerReadinessManager.Phase == ServerReadinessPhase.Starting)
		{
			ServerReadinessManager.Evaluate();
		}
	}

	private static void Event_Server_OnLoadSceneEventCompleted(Dictionary<string, object> message)
	{
		if ((bool)message["isInitialScene"])
		{
			ServerReadinessManager.AnnounceReady();
		}
	}
}
