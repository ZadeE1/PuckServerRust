using System.Collections.Generic;
using UnityEngine;

public class PuckManagerController : MonoBehaviour
{
	private PuckManager puckManager;

	private void Awake()
	{
		puckManager = GetComponent<PuckManager>();
		EventManager.AddEventListener("Event_Everyone_OnPuckPositionSpawned", Event_Everyone_OnPuckPositionSpawned);
		EventManager.AddEventListener("Event_Everyone_OnPuckPositionDespawned", Event_Everyone_OnPuckPositionDespawned);
		EventManager.AddEventListener("Event_Everyone_OnPuckSpawned", Event_Everyone_OnPuckSpawned);
		EventManager.AddEventListener("Event_Everyone_OnPuckDespawned", Event_Everyone_OnPuckDespawned);
	}

	private void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnPuckPositionSpawned", Event_Everyone_OnPuckPositionSpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPuckPositionDespawned", Event_Everyone_OnPuckPositionDespawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPuckSpawned", Event_Everyone_OnPuckSpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPuckDespawned", Event_Everyone_OnPuckDespawned);
	}

	private void Event_Everyone_OnPuckPositionSpawned(Dictionary<string, object> message)
	{
		PuckPosition puckPosition = (PuckPosition)message["puckPosition"];
		puckManager.AddPuckPosition(puckPosition);
	}

	private void Event_Everyone_OnPuckPositionDespawned(Dictionary<string, object> message)
	{
		PuckPosition puckPosition = (PuckPosition)message["puckPosition"];
		puckManager.RemovePuckPosition(puckPosition);
	}

	private void Event_Everyone_OnPuckSpawned(Dictionary<string, object> message)
	{
		Puck puck = (Puck)message["puck"];
		puckManager.AddPuck(puck);
	}

	private void Event_Everyone_OnPuckDespawned(Dictionary<string, object> message)
	{
		Puck puck = (Puck)message["puck"];
		puckManager.RemovePuck(puck);
	}
}
