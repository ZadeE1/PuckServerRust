using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
	private Level level;

	public virtual void Awake()
	{
		level = GetComponent<Level>();
		EventManager.AddEventListener("Event_Everyone_OnGameStateChanged", Event_Everyone_OnGameStateChanged);
	}

	public virtual void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnGameStateChanged", Event_Everyone_OnGameStateChanged);
	}

	private void Event_Everyone_OnGameStateChanged(Dictionary<string, object> eventParams)
	{
		GameState gameState = (GameState)eventParams["oldGameState"];
		GameState gameState2 = (GameState)eventParams["newGameState"];
		if (gameState.Phase != gameState2.Phase)
		{
			switch (gameState2.Phase)
			{
			case GamePhase.Warmup:
			case GamePhase.PreGame:
			case GamePhase.FaceOff:
				level.SetBlueGoalLightEnabled(isEnabled: false);
				level.SetRedGoalLightEnabled(isEnabled: false);
				break;
			case GamePhase.BlueScore:
				level.SetRedGoalLightEnabled(isEnabled: true);
				break;
			case GamePhase.RedScore:
				level.SetBlueGoalLightEnabled(isEnabled: true);
				break;
			case GamePhase.Play:
				break;
			}
		}
	}
}
