using System.Collections.Generic;
using System.Linq;

public static class SteamWorkshopManagerController
{
	private static readonly Logger Logger = new Logger("SteamWorkshopManagerController");

	public static void Initialize()
	{
		EventManager.AddEventListener("Event_OnSteamConnected", Event_OnSteamConnected);
		EventManager.AddEventListener("Event_OnSteamWorkshopSubscribedItemsListChanged", Event_OnSteamWorkshopSubscribedItemsListChanged);
		EventManager.AddEventListener("Event_OnSteamWorkshopItemDownloaded", Event_OnSteamWorkshopItemDownloaded);
		EventManager.AddEventListener("Event_OnModsClickRefresh", Event_OnModsClickRefresh);
		EventManager.AddEventListener("Event_OnReconnectionStateChanged", Event_OnReconnectionStateChanged);
		EventManager.AddEventListener("Event_Server_OnServerStarted", Event_Server_OnServerStarted);
	}

	public static void Dispose()
	{
		EventManager.RemoveEventListener("Event_OnSteamConnected", Event_OnSteamConnected);
		EventManager.RemoveEventListener("Event_OnSteamWorkshopSubscribedItemsListChanged", Event_OnSteamWorkshopSubscribedItemsListChanged);
		EventManager.RemoveEventListener("Event_OnSteamWorkshopItemDownloaded", Event_OnSteamWorkshopItemDownloaded);
		EventManager.RemoveEventListener("Event_OnModsClickRefresh", Event_OnModsClickRefresh);
		EventManager.RemoveEventListener("Event_OnReconnectionStateChanged", Event_OnReconnectionStateChanged);
		EventManager.RemoveEventListener("Event_Server_OnServerStarted", Event_Server_OnServerStarted);
	}

	private static void Event_OnSteamConnected(Dictionary<string, object> message)
	{
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			SteamWorkshopManager.VerifyIntegrity();
		}
	}

	private static void Event_OnSteamWorkshopSubscribedItemsListChanged(Dictionary<string, object> message)
	{
		SteamWorkshopManager.VerifyIntegrity();
	}

	private static void Event_OnSteamWorkshopItemDownloaded(Dictionary<string, object> message)
	{
		SteamWorkshopManager.VerifyItemIntegrity((string)message["itemId"]);
	}

	private static void Event_OnModsClickRefresh(Dictionary<string, object> message)
	{
		SteamWorkshopManager.VerifyIntegrity();
	}

	private static void Event_OnReconnectionStateChanged(Dictionary<string, object> message)
	{
		ReconnectionState reconnectionState = (ReconnectionState)message["oldReconnectionState"];
		ReconnectionState reconnectionState2 = (ReconnectionState)message["newReconnectionState"];
		if (reconnectionState2.Phase != ReconnectionPhase.AwaitingMods || reconnectionState.PendingReadinessModIds.SequenceEqual(reconnectionState2.PendingReadinessModIds))
		{
			return;
		}
		string[] array = reconnectionState2.PendingReadinessModIds.Except(reconnectionState.PendingReadinessModIds).ToArray();
		foreach (string text in array)
		{
			if (ModManager.GetModById(text) == null)
			{
				SteamWorkshopManager.SubscribeItem(text);
			}
		}
	}

	private static void Event_Server_OnServerStarted(Dictionary<string, object> message)
	{
		ServerConfig serverConfig = (ServerConfig)message["serverConfig"];
		if (ApplicationManager.IsDedicatedGameServer)
		{
			string[] enabledModIds = serverConfig.EnabledModIds;
			for (int i = 0; i < enabledModIds.Length; i++)
			{
				SteamWorkshopManager.DownloadItem(enabledModIds[i]);
			}
		}
	}
}
