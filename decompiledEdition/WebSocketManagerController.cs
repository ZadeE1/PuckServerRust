using System.Collections.Generic;

public static class WebSocketManagerController
{
	public static void Initialize()
	{
		EventManager.AddEventListener("Event_OnSteamConnected", Event_OnSteamConnected);
	}

	public static void Dispose()
	{
		WebSocketManager.Disconnect();
		EventManager.RemoveEventListener("Event_OnSteamConnected", Event_OnSteamConnected);
	}

	private static void Event_OnSteamConnected(Dictionary<string, object> message)
	{
		if (!WebSocketManager.IsConnected && !WebSocketManager.IsConnectionInProgress && !WebSocketManager.IsReconnecting)
		{
			WebSocketManager.Connect(WebSocketManager.BackendUrl);
		}
	}
}
