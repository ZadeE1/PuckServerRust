using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Zone))]
public class ZoneController : MonoBehaviour
{
	private Zone zone;

	private void Awake()
	{
		zone = GetComponent<Zone>();
		EventManager.AddEventListener("Event_Everyone_OnPlayerBodyDespawned", Event_Everyone_OnPlayerBodyDespawned);
	}

	private void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerBodyDespawned", Event_Everyone_OnPlayerBodyDespawned);
	}

	private void Event_Everyone_OnPlayerBodyDespawned(Dictionary<string, object> message)
	{
		PlayerBody playerBody = (PlayerBody)message["playerBody"];
		zone.ClearOccupant(playerBody);
	}
}
