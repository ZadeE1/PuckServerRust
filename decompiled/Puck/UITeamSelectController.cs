using System.Collections.Generic;

public class UITeamSelectController : UIViewController<UITeamSelect>
{
	private UITeamSelect uiTeamSelect;

	public override void Awake()
	{
		base.Awake();
		uiTeamSelect = GetComponent<UITeamSelect>();
		EventManager.AddEventListener("Event_OnTeamSelectShow", Event_OnTeamSelectShow);
		EventManager.AddEventListener("Event_Everyone_OnPlayerAdded", Event_Everyone_OnPlayerAdded);
		EventManager.AddEventListener("Event_Everyone_OnPlayerRemoved", Event_Everyone_OnPlayerRemoved);
		EventManager.AddEventListener("Event_Everyone_OnPlayerGameStateChanged", Event_Everyone_OnPlayerGameStateChanged);
	}

	public override void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_OnTeamSelectShow", Event_OnTeamSelectShow);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerAdded", Event_Everyone_OnPlayerAdded);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerRemoved", Event_Everyone_OnPlayerRemoved);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerGameStateChanged", Event_Everyone_OnPlayerGameStateChanged);
		base.OnDestroy();
	}

	private void StyleTeamCounts()
	{
		uiTeamSelect.StyleTeamCounts(MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayersByTeam(PlayerTeam.Blue).Count, MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayersByTeam(PlayerTeam.Red).Count);
	}

	private void Event_OnTeamSelectShow(Dictionary<string, object> message)
	{
		StyleTeamCounts();
	}

	private void Event_Everyone_OnPlayerAdded(Dictionary<string, object> message)
	{
		if (!((Player)message["player"]).IsReplay.Value)
		{
			StyleTeamCounts();
		}
	}

	private void Event_Everyone_OnPlayerRemoved(Dictionary<string, object> message)
	{
		if (!((Player)message["player"]).IsReplay.Value)
		{
			StyleTeamCounts();
		}
	}

	private void Event_Everyone_OnPlayerGameStateChanged(Dictionary<string, object> message)
	{
		StyleTeamCounts();
	}
}
