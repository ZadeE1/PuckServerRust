using System.Collections.Generic;

public class UIServerBrowserController : UIViewController<UIServerBrowser>
{
	private UIServerBrowser uiServerBrowser;

	public override void Awake()
	{
		base.Awake();
		uiServerBrowser = GetComponent<UIServerBrowser>();
		EventManager.AddEventListener("Event_OnServerBrowserShow", Event_OnServerBrowserShow);
		EventManager.AddEventListener("Event_OnServerBrowserClickRefresh", Event_OnServerBrowserClickRefresh);
		WebSocketManager.AddMessageListener("playerGetServerBrowserEndPointsResponse", WebSocket_Event_OnPlayerGetServerBrowserEndPointsResponse);
	}

	public override void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_OnServerBrowserShow", Event_OnServerBrowserShow);
		EventManager.RemoveEventListener("Event_OnServerBrowserClickRefresh", Event_OnServerBrowserClickRefresh);
		WebSocketManager.RemoveMessageListener("playerGetServerBrowserEndPointsResponse", WebSocket_Event_OnPlayerGetServerBrowserEndPointsResponse);
		base.OnDestroy();
	}

	private void Event_OnServerBrowserShow(Dictionary<string, object> message)
	{
		if (uiServerBrowser.ServerCount == 0)
		{
			uiServerBrowser.Refresh();
		}
	}

	private void Event_OnServerBrowserClickRefresh(Dictionary<string, object> message)
	{
		uiServerBrowser.Refresh();
	}

	private void WebSocket_Event_OnPlayerGetServerBrowserEndPointsResponse(Dictionary<string, object> message)
	{
		ServerBrowserEndPointsResponse data = ((InMessage)message["inMessage"]).GetData<ServerBrowserEndPointsResponse>();
		List<EndPoint> list = new List<EndPoint>(data.data.endPoints);
		if (data.success)
		{
			uiServerBrowser.UpdateEndPoints(list.ToArray());
		}
	}
}
