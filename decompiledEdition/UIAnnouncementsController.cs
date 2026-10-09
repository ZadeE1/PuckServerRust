using System.Collections.Generic;

internal class UIAnnouncementsController : UIViewController<UIAnnouncements>
{
	private UIAnnouncements uiAnnouncements;

	public override void Awake()
	{
		base.Awake();
		uiAnnouncements = GetComponent<UIAnnouncements>();
		EventManager.AddEventListener("Event_Everyone_OnGoalScored", Event_Everyone_OnGoalScored);
		EventManager.AddEventListener("Event_Everyone_OnGameStateChanged", Event_Everyone_OnGameStateChanged);
		EventManager.AddEventListener("Event_OnClientStopped", Event_OnClientStopped);
	}

	public override void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnGoalScored", Event_Everyone_OnGoalScored);
		EventManager.RemoveEventListener("Event_Everyone_OnGameStateChanged", Event_Everyone_OnGameStateChanged);
		EventManager.RemoveEventListener("Event_OnClientStopped", Event_OnClientStopped);
		base.OnDestroy();
	}

	public void Event_Everyone_OnGoalScored(Dictionary<string, object> message)
	{
		PlayerTeam team = (PlayerTeam)message["byTeam"];
		Player goalPlayer = (Player)message["goalPlayer"];
		Player assistPlayer = (Player)message["assistPlayer"];
		Player secondAssistPlayer = (Player)message["secondAssistPlayer"];
		uiAnnouncements.ShowScore(team, goalPlayer, assistPlayer, secondAssistPlayer);
	}

	public void Event_Everyone_OnGameStateChanged(Dictionary<string, object> message)
	{
		GameState gameState = (GameState)message["newGameState"];
		if (gameState.Phase != GamePhase.BlueScore && gameState.Phase != GamePhase.RedScore)
		{
			uiAnnouncements.HideScore();
		}
	}

	private void Event_OnClientStopped(Dictionary<string, object> message)
	{
		uiAnnouncements.HideScore();
	}
}
