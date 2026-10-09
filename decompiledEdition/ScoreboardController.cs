using System.Collections.Generic;
using UnityEngine;

public class ScoreboardController : MonoBehaviour
{
	private Scoreboard scoreboard;

	private void Awake()
	{
		scoreboard = GetComponent<Scoreboard>();
		EventManager.AddEventListener("Event_Everyone_OnGameStateChanged", Event_Everyone_OnGameStateChanged);
	}

	private void Start()
	{
		scoreboard.TurnOff();
	}

	private void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnGameStateChanged", Event_Everyone_OnGameStateChanged);
	}

	private void Event_Everyone_OnGameStateChanged(Dictionary<string, object> message)
	{
		GameState gameState = (GameState)message["newGameState"];
		GamePhase phase = gameState.Phase;
		if ((uint)(phase - 2) <= 8u)
		{
			scoreboard.TurnOn();
		}
		else
		{
			scoreboard.TurnOff();
		}
		scoreboard.SetTick(gameState.Tick);
		scoreboard.SetPeriod(gameState.Period);
		scoreboard.SetBlueScore(gameState.BlueScore);
		scoreboard.SetRedScore(gameState.RedScore);
	}
}
