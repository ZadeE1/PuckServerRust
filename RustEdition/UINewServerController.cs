using System.Collections.Generic;
using System.Linq;

public class UINewServerController : UIViewController<UINewServer>
{
	private static readonly Logger Logger = new Logger("UINewServerController");

	private UINewServer uiNewServer;

	public override void Awake()
	{
		base.Awake();
		uiNewServer = GetComponent<UINewServer>();
		EventManager.AddEventListener("Event_OnPlayerDataChanged", Event_OnPlayerDataChanged);
		WebSocketManager.AddMessageListener("playerGetProbesResponse", WebSocket_Event_OnPlayerGetProbesResponse);
	}

	public override void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_OnPlayerDataChanged", Event_OnPlayerDataChanged);
		WebSocketManager.RemoveMessageListener("playerGetProbesResponse", WebSocket_Event_OnPlayerGetProbesResponse);
		base.OnDestroy();
	}

	private void Event_OnPlayerDataChanged(Dictionary<string, object> message)
	{
		PlayerData playerData = (PlayerData)message["newPlayerData"];
		if (playerData != null)
		{
			uiNewServer.SetCloudAvailable(playerData.patreonLevel >= 1);
		}
	}

	private void WebSocket_Event_OnPlayerGetProbesResponse(Dictionary<string, object> message)
	{
		Probe[] dedicatedProbes = (from probe in ((InMessage)message["inMessage"]).GetData<PlayerGetProbesResponse>().data.probes
			group probe by probe.ToLabel() into @group
			select @group.OrderByDescending((Probe probe) => probe.priority).First()).ToArray();
		uiNewServer.SetDedicatedProbes(dedicatedProbes);
	}
}
