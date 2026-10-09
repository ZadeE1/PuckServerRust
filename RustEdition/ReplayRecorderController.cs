using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ReplayRecorderController : MonoBehaviour
{
	private ReplayRecorder replayRecorder;

	private void Awake()
	{
		replayRecorder = GetComponent<ReplayRecorder>();
		EventManager.AddEventListener("Event_Everyone_OnPlayerSpawned", Event_Everyone_OnPlayerSpawned);
		EventManager.AddEventListener("Event_Everyone_OnPlayerDespawned", Event_Everyone_OnPlayerDespawned);
		EventManager.AddEventListener("Event_Everyone_OnPlayerBodySpawned", Event_Everyone_OnPlayerBodySpawned);
		EventManager.AddEventListener("Event_Everyone_OnPlayerBodyDespawned", Event_Everyone_OnPlayerBodyDespawned);
		EventManager.AddEventListener("Event_Everyone_OnStickSpawned", Event_Everyone_OnStickSpawned);
		EventManager.AddEventListener("Event_Everyone_OnStickDespawned", Event_Everyone_OnStickDespawned);
		EventManager.AddEventListener("Event_Everyone_OnPuckSpawned", Event_Everyone_OnPuckSpawned);
		EventManager.AddEventListener("Event_Everyone_OnPuckDespawned", Event_Everyone_OnPuckDespawned);
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerSpawned", Event_Everyone_OnPlayerSpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerDespawned", Event_Everyone_OnPlayerDespawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerBodySpawned", Event_Everyone_OnPlayerBodySpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerBodyDespawned", Event_Everyone_OnPlayerBodyDespawned);
		EventManager.RemoveEventListener("Event_Everyone_OnStickSpawned", Event_Everyone_OnStickSpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnStickDespawned", Event_Everyone_OnStickDespawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPuckSpawned", Event_Everyone_OnPuckSpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPuckDespawned", Event_Everyone_OnPuckDespawned);
	}

	private void Event_Everyone_OnPlayerSpawned(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		if (NetworkManager.Singleton.IsServer && !player.IsReplay.Value)
		{
			replayRecorder.Server_AddPlayerSpawnedEvent(player);
		}
	}

	private void Event_Everyone_OnPlayerDespawned(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		if (NetworkManager.Singleton.IsServer && !player.IsReplay.Value)
		{
			replayRecorder.Server_AddPlayerDespawnedEvent(player);
		}
	}

	private void Event_Everyone_OnPlayerBodySpawned(Dictionary<string, object> message)
	{
		PlayerBody playerBody = (PlayerBody)message["playerBody"];
		if (NetworkManager.Singleton.IsServer && !playerBody.Player.IsReplay.Value)
		{
			replayRecorder.Server_AddPlayerBodySpawnedEvent(playerBody);
		}
	}

	private void Event_Everyone_OnPlayerBodyDespawned(Dictionary<string, object> message)
	{
		PlayerBody playerBody = (PlayerBody)message["playerBody"];
		if (NetworkManager.Singleton.IsServer && !playerBody.Player.IsReplay.Value)
		{
			replayRecorder.Server_AddPlayerBodyDespawnedEvent(playerBody);
		}
	}

	private void Event_Everyone_OnStickSpawned(Dictionary<string, object> message)
	{
		Stick stick = (Stick)message["stick"];
		if (NetworkManager.Singleton.IsServer && !stick.Player.IsReplay.Value)
		{
			replayRecorder.Server_AddStickSpawnedEvent(stick);
		}
	}

	private void Event_Everyone_OnStickDespawned(Dictionary<string, object> message)
	{
		Stick stick = (Stick)message["stick"];
		if (NetworkManager.Singleton.IsServer && !stick.Player.IsReplay.Value)
		{
			replayRecorder.Server_AddStickDespawnedEvent(stick);
		}
	}

	private void Event_Everyone_OnPuckSpawned(Dictionary<string, object> message)
	{
		Puck puck = (Puck)message["puck"];
		if (NetworkManager.Singleton.IsServer && !puck.IsReplay.Value)
		{
			replayRecorder.Server_AddPuckSpawnedEvent(puck);
		}
	}

	private void Event_Everyone_OnPuckDespawned(Dictionary<string, object> message)
	{
		Puck puck = (Puck)message["puck"];
		if (NetworkManager.Singleton.IsServer && !puck.IsReplay.Value)
		{
			replayRecorder.Server_AddPuckDespawnedEvent(puck);
		}
	}
}
