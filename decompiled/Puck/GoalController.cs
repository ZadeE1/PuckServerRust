using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GoalController : MonoBehaviour
{
	private Goal goal;

	private void Awake()
	{
		goal = GetComponent<Goal>();
		EventManager.AddEventListener("Event_Everyone_OnPuckSpawned", Event_Everyone_OnPuckSpawned);
		EventManager.AddEventListener("Event_Everyone_OnPuckDespawned", Event_Everyone_OnPuckDespawned);
	}

	public void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnPuckSpawned", Event_Everyone_OnPuckSpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPuckDespawned", Event_Everyone_OnPuckDespawned);
	}

	private void Event_Everyone_OnPuckSpawned(Dictionary<string, object> message)
	{
		Puck puck = (Puck)message["puck"];
		if (NetworkManager.Singleton.IsClient)
		{
			goal.Client_AddNetClothSphereCollider(puck.NetSphereCollider);
		}
	}

	private void Event_Everyone_OnPuckDespawned(Dictionary<string, object> message)
	{
		Puck puck = (Puck)message["puck"];
		if (NetworkManager.Singleton.IsClient)
		{
			goal.Client_RemoveNetClothSphereCollider(puck.NetSphereCollider);
		}
	}
}
