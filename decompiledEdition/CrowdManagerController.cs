using System.Collections.Generic;
using UnityEngine;

public class CrowdManagerController : MonoBehaviour
{
	private CrowdManager crowdManager;

	private void Awake()
	{
		crowdManager = GetComponent<CrowdManager>();
		EventManager.AddEventListener("Event_OnCrowdPositionSpawned", Event_OnCrowdPositionSpawned);
		EventManager.AddEventListener("Event_OnCrowdPositionDespawned", Event_OnCrowdPositionDespawned);
		EventManager.AddEventListener("Event_Everyone_OnPuckSpawned", Event_Everyone_OnPuckSpawned);
		EventManager.AddEventListener("Event_Everyone_OnGameStateChanged", Event_Everyone_OnGameStateChanged);
	}

	private void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_OnCrowdPositionSpawned", Event_OnCrowdPositionSpawned);
		EventManager.RemoveEventListener("Event_OnCrowdPositionDespawned", Event_OnCrowdPositionDespawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPuckSpawned", Event_Everyone_OnPuckSpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnGameStateChanged", Event_Everyone_OnGameStateChanged);
	}

	private void Event_OnCrowdPositionSpawned(Dictionary<string, object> message)
	{
		CrowdPosition position = (CrowdPosition)message["crowdPosition"];
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			crowdManager.RegisterCrowdPosition(position);
		}
	}

	private void Event_OnCrowdPositionDespawned(Dictionary<string, object> message)
	{
		CrowdPosition position = (CrowdPosition)message["crowdPosition"];
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			crowdManager.UnregisterCrowdPosition(position);
		}
	}

	private void Event_Everyone_OnPuckSpawned(Dictionary<string, object> message)
	{
		Puck puck = (Puck)message["puck"];
		crowdManager.SetCrowdLookTarget(puck.transform);
	}

	private void Event_Everyone_OnGameStateChanged(Dictionary<string, object> message)
	{
		GameState gameState = (GameState)message["oldGameState"];
		GameState gameState2 = (GameState)message["newGameState"];
		if (gameState.Phase != gameState2.Phase)
		{
			GamePhase phase = gameState2.Phase;
			if ((uint)(phase - 5) <= 1u)
			{
				crowdManager.SetCrowdAnimation("Cheering");
			}
			else
			{
				crowdManager.SetCrowdAnimation("Seated");
			}
		}
	}
}
