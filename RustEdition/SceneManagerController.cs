using System.Collections.Generic;

public static class SceneManagerController
{
	public static void Initialize()
	{
		EventManager.AddEventListener("Event_Server_OnServerStarted", Event_Server_OnServerStarted);
		EventManager.AddEventListener("Event_Server_OnReady", Event_Server_OnReady);
		EventManager.AddEventListener("Event_Server_OnServerStopped", Event_Server_OnServerStopped);
		EventManager.AddEventListener("Event_OnClientStopped", Event_OnClientStopped);
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			SceneManager.LoadScene("locker_room");
		}
	}

	public static void Dispose()
	{
		EventManager.RemoveEventListener("Event_Server_OnServerStarted", Event_Server_OnServerStarted);
		EventManager.RemoveEventListener("Event_Server_OnReady", Event_Server_OnReady);
		EventManager.RemoveEventListener("Event_Server_OnServerStopped", Event_Server_OnServerStopped);
		EventManager.RemoveEventListener("Event_OnClientStopped", Event_OnClientStopped);
	}

	private static void Event_Server_OnServerStarted(Dictionary<string, object> message)
	{
		SceneManager.InitializeServer();
	}

	private static void Event_Server_OnReady(Dictionary<string, object> message)
	{
		if (((ServerConfig)message["serverConfig"]).level == "default")
		{
			SceneManager.LoadScene("level_default");
		}
	}

	private static void Event_Server_OnServerStopped(Dictionary<string, object> message)
	{
		SceneManager.DisposeServer();
	}

	private static void Event_OnClientStopped(Dictionary<string, object> message)
	{
		SceneManager.LoadScene("locker_room");
	}
}
