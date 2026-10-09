using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameModeManagerController : MonoBehaviour
{
	private GameModeManager gameModeManager;

	private void Awake()
	{
		gameModeManager = GetComponent<GameModeManager>();
		EventManager.AddEventListener("Event_Server_OnServerStarted", Event_Server_OnServerStarted);
		EventManager.AddEventListener("Event_Server_OnServerStopped", Event_Server_OnServerStopped);
		EventManager.AddEventListener("Event_Server_OnLoadSceneEventCompleted", Event_Server_OnLoadSceneEventCompleted);
		EventManager.AddEventListener("Event_Everyone_OnLevelSpawned", Event_Everyone_OnLevelSpawned);
		EventManager.AddEventListener("Event_Everyone_OnLevelDespawned", Event_Everyone_OnLevelDespawned);
	}

	private void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_Server_OnServerStarted", Event_Server_OnServerStarted);
		EventManager.RemoveEventListener("Event_Server_OnServerStopped", Event_Server_OnServerStopped);
		EventManager.RemoveEventListener("Event_Server_OnLoadSceneEventCompleted", Event_Server_OnLoadSceneEventCompleted);
		EventManager.RemoveEventListener("Event_Everyone_OnLevelSpawned", Event_Everyone_OnLevelSpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnLevelDespawned", Event_Everyone_OnLevelDespawned);
	}

	private void Event_Server_OnServerStarted(Dictionary<string, object> message)
	{
		ServerConfig serverConfig = (ServerConfig)message["serverConfig"];
		gameModeManager.SelectGameMode(serverConfig.gameMode);
	}

	private void Event_Server_OnServerStopped(Dictionary<string, object> message)
	{
		gameModeManager.DisableSelectedGameMode();
		gameModeManager.DeselectGameMode();
	}

	private void Event_Server_OnLoadSceneEventCompleted(Dictionary<string, object> message)
	{
		gameModeManager.EnableSelectedGameMode();
	}

	private void Event_Everyone_OnLevelSpawned(Dictionary<string, object> message)
	{
		Level level = (Level)message["level"];
		if (NetworkManager.Singleton.IsServer)
		{
			gameModeManager.Level = level;
		}
	}

	private void Event_Everyone_OnLevelDespawned(Dictionary<string, object> message)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			gameModeManager.Level = null;
		}
	}
}
