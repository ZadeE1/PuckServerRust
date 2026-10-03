using System.Collections.Generic;

public class UIMatchmakingController : UIViewController<UIMatchmaking>
{
	private UIMatchmaking uiMatchmaking;

	public override void Awake()
	{
		base.Awake();
		uiMatchmaking = GetComponent<UIMatchmaking>();
		EventManager.AddEventListener("Event_OnPlaySelectedPoolsChanged", Event_OnPlaySelectedPoolsChanged);
		EventManager.AddEventListener("Event_OnPlayerGroupDataChanged", Event_OnPlayerGroupDataChanged);
		EventManager.AddEventListener("Event_OnPlayerMatchDataChanged", Event_OnPlayerMatchDataChanged);
		EventManager.AddEventListener("Event_OnConnectionStateChanged", Event_OnConnectionStateChanged);
		EventManager.AddEventListener("Event_OnMatchingTickerStarted", Event_OnMatchingTickerStarted);
		EventManager.AddEventListener("Event_OnMatchingTickerTick", Event_OnMatchingTickerTick);
		EventManager.AddEventListener("Event_OnMatchingTickerStopped", Event_OnMatchingTickerStopped);
		EventManager.AddEventListener("Event_OnMatchJoinTimeoutTickerStarted", Event_OnMatchJoinTimeoutTickerStarted);
		EventManager.AddEventListener("Event_OnMatchJoinTimeoutTickerTick", Event_OnMatchJoinTimeoutTickerTick);
		EventManager.AddEventListener("Event_OnMatchJoinTimeoutTickerStopped", Event_OnMatchJoinTimeoutTickerStopped);
	}

	private void Start()
	{
		uiMatchmaking.SetMatchingVisibility(isVisible: false);
		uiMatchmaking.SetMatchingPhaseText(string.Empty);
		uiMatchmaking.SetMatchingTimeVisibility(isVisible: false);
		uiMatchmaking.SetMatchingTimeText(0);
		uiMatchmaking.SetMatchingStartMatchmakingButtonVisibility(isVisible: false);
		uiMatchmaking.SetMatchingConnectButtonVisibility(isVisible: false);
		uiMatchmaking.SetMatchingCloseButtonVisibility(isVisible: false);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		EventManager.RemoveEventListener("Event_OnPlaySelectedPoolsChanged", Event_OnPlaySelectedPoolsChanged);
		EventManager.RemoveEventListener("Event_OnPlayerGroupDataChanged", Event_OnPlayerGroupDataChanged);
		EventManager.RemoveEventListener("Event_OnPlayerMatchDataChanged", Event_OnPlayerMatchDataChanged);
		EventManager.RemoveEventListener("Event_OnConnectionStateChanged", Event_OnConnectionStateChanged);
		EventManager.RemoveEventListener("Event_OnMatchingTickerStarted", Event_OnMatchingTickerStarted);
		EventManager.RemoveEventListener("Event_OnMatchingTickerTick", Event_OnMatchingTickerTick);
		EventManager.RemoveEventListener("Event_OnMatchingTickerStopped", Event_OnMatchingTickerStopped);
		EventManager.RemoveEventListener("Event_OnMatchJoinTimeoutTickerStarted", Event_OnMatchJoinTimeoutTickerStarted);
		EventManager.RemoveEventListener("Event_OnMatchJoinTimeoutTickerTick", Event_OnMatchJoinTimeoutTickerTick);
		EventManager.RemoveEventListener("Event_OnMatchJoinTimeoutTickerStopped", Event_OnMatchJoinTimeoutTickerStopped);
	}

	private void UpdateMatching()
	{
		PlayerGroupData groupData = BackendManager.PlayerState.GroupData;
		PlayerMatchData matchData = BackendManager.PlayerState.MatchData;
		if (groupData != null)
		{
			uiMatchmaking.SetMatchingVisibility(isVisible: true);
			uiMatchmaking.SetMatchingPhaseText("LOOKING FOR A MATCH...");
			uiMatchmaking.SetMatchingStartMatchmakingButtonVisibility(isVisible: false);
			uiMatchmaking.SetMatchingConnectButtonVisibility(isVisible: false);
			uiMatchmaking.SetMatchingCloseButtonVisibility(isVisible: true);
		}
		else if (matchData != null && !BackendUtils.IsConnectedToMatchEndPoint())
		{
			uiMatchmaking.SetMatchingVisibility(isVisible: true);
			uiMatchmaking.SetMatchingStartMatchmakingButtonVisibility(isVisible: false);
			uiMatchmaking.SetMatchingCloseButtonVisibility(isVisible: false);
			if (matchData.endPoint == null)
			{
				uiMatchmaking.SetMatchingPhaseText("MATCH FOUND! DEPLOYING MATCH SERVER...");
				uiMatchmaking.SetMatchingConnectButtonVisibility(isVisible: false);
			}
			else
			{
				uiMatchmaking.SetMatchingPhaseText("MATCH READY!");
				uiMatchmaking.SetMatchingConnectButtonVisibility(isVisible: true);
			}
		}
		else if (uiMatchmaking.MatchingSelectedPoolIds.Length != 0)
		{
			uiMatchmaking.SetMatchingVisibility(isVisible: true);
			uiMatchmaking.SetMatchingPhaseText("READY TO QUEUE");
			uiMatchmaking.SetMatchingStartMatchmakingButtonVisibility(isVisible: true);
			uiMatchmaking.SetMatchingConnectButtonVisibility(isVisible: false);
			uiMatchmaking.SetMatchingCloseButtonVisibility(isVisible: false);
		}
		else
		{
			uiMatchmaking.SetMatchingVisibility(isVisible: false);
			uiMatchmaking.SetMatchingPhaseText(string.Empty);
			uiMatchmaking.SetMatchingStartMatchmakingButtonVisibility(isVisible: false);
			uiMatchmaking.SetMatchingConnectButtonVisibility(isVisible: false);
			uiMatchmaking.SetMatchingCloseButtonVisibility(isVisible: false);
		}
	}

	private void Event_OnPlaySelectedPoolsChanged(Dictionary<string, object> message)
	{
		string[] matchingSelectedPoolIds = (string[])message["poolIds"];
		uiMatchmaking.SetMatchingSelectedPoolIds(matchingSelectedPoolIds);
		UpdateMatching();
	}

	private void Event_OnPlayerGroupDataChanged(Dictionary<string, object> message)
	{
		UpdateMatching();
	}

	private void Event_OnPlayerMatchDataChanged(Dictionary<string, object> message)
	{
		UpdateMatching();
	}

	private void Event_OnConnectionStateChanged(Dictionary<string, object> message)
	{
		UpdateMatching();
	}

	private void Event_OnMatchingTickerStarted(Dictionary<string, object> message)
	{
		int matchingTimeText = (int)message["startingTick"];
		uiMatchmaking.SetMatchingTimeVisibility(isVisible: true);
		uiMatchmaking.SetMatchingTimeText(matchingTimeText);
	}

	private void Event_OnMatchingTickerTick(Dictionary<string, object> message)
	{
		int matchingTimeText = (int)message["tick"];
		uiMatchmaking.SetMatchingTimeText(matchingTimeText);
	}

	private void Event_OnMatchingTickerStopped(Dictionary<string, object> message)
	{
		uiMatchmaking.SetMatchingTimeVisibility(isVisible: false);
		uiMatchmaking.SetMatchingTimeText(0);
	}

	private void Event_OnMatchJoinTimeoutTickerStarted(Dictionary<string, object> message)
	{
		int matchingTimeText = (int)message["startingTick"];
		uiMatchmaking.SetMatchingTimeVisibility(isVisible: true);
		uiMatchmaking.SetMatchingTimeText(matchingTimeText);
	}

	private void Event_OnMatchJoinTimeoutTickerTick(Dictionary<string, object> message)
	{
		int matchingTimeText = (int)message["tick"];
		uiMatchmaking.SetMatchingTimeText(matchingTimeText);
	}

	private void Event_OnMatchJoinTimeoutTickerStopped(Dictionary<string, object> message)
	{
		uiMatchmaking.SetMatchingTimeVisibility(isVisible: false);
		uiMatchmaking.SetMatchingTimeText(0);
	}
}
