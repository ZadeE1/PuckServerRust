using System.Collections.Generic;
using UnityEngine.SceneManagement;

public static class CameraManagerController
{
	public static void Initialize()
	{
		EventManager.AddEventListener("Event_OnBaseCameraStarted", Event_OnBaseCameraStarted);
		EventManager.AddEventListener("Event_OnBaseCameraDestroyed", Event_OnBaseCameraDestroyed);
		EventManager.AddEventListener("Event_OnSceneLoaded", Event_OnSceneLoaded);
		EventManager.AddEventListener("Event_Everyone_OnPlayerSpawned", Event_Everyone_OnPlayerSpawned);
		EventManager.AddEventListener("Event_Everyone_OnPlayerGameStateChanged", Event_Everyone_OnPlayerGameStateChanged);
	}

	public static void Dispose()
	{
		EventManager.RemoveEventListener("Event_OnBaseCameraStarted", Event_OnBaseCameraStarted);
		EventManager.RemoveEventListener("Event_OnBaseCameraDestroyed", Event_OnBaseCameraDestroyed);
		EventManager.RemoveEventListener("Event_OnSceneLoaded", Event_OnSceneLoaded);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerSpawned", Event_Everyone_OnPlayerSpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerGameStateChanged", Event_Everyone_OnPlayerGameStateChanged);
	}

	private static void HandlePlayerGameState(Player player)
	{
		PlayerGameState value = player.GameState.Value;
		switch (value.Phase)
		{
		case PlayerPhase.TeamSelect:
			CameraManager.SetActiveCamera(CameraType.Cinematic);
			break;
		case PlayerPhase.PositionSelect:
			if (value.Team == PlayerTeam.Blue)
			{
				CameraManager.SetActiveCamera(CameraType.BluePositionSelection);
			}
			else if (value.Team == PlayerTeam.Red)
			{
				CameraManager.SetActiveCamera(CameraType.RedPositionSelection);
			}
			else
			{
				CameraManager.SetActiveCamera(CameraType.Cinematic);
			}
			break;
		case PlayerPhase.Play:
			CameraManager.SetActiveCamera(CameraType.Player, player.OwnerClientId);
			break;
		case PlayerPhase.Replay:
			CameraManager.SetActiveCamera(CameraType.Replay);
			break;
		case PlayerPhase.Spectate:
			CameraManager.SetActiveCamera(CameraType.Spectator, player.OwnerClientId);
			break;
		default:
			CameraManager.SetActiveCamera(CameraType.Cinematic);
			break;
		}
	}

	private static void Event_OnBaseCameraStarted(Dictionary<string, object> eventParams)
	{
		CameraManager.RegisterCamera((BaseCamera)eventParams["baseCamera"]);
	}

	private static void Event_OnBaseCameraDestroyed(Dictionary<string, object> eventParams)
	{
		CameraManager.UnregisterCamera((BaseCamera)eventParams["baseCamera"]);
	}

	private static void Event_OnSceneLoaded(Dictionary<string, object> eventParams)
	{
		if (((Scene)eventParams["scene"]).name == "locker_room")
		{
			CameraManager.SetActiveCamera(CameraType.LockerRoom);
		}
	}

	private static void Event_Everyone_OnPlayerSpawned(Dictionary<string, object> eventParams)
	{
		Player player = (Player)eventParams["player"];
		if (player.IsLocalPlayer)
		{
			HandlePlayerGameState(player);
		}
	}

	private static void Event_Everyone_OnPlayerGameStateChanged(Dictionary<string, object> eventParams)
	{
		Player player = (Player)eventParams["player"];
		PlayerGameState playerGameState = (PlayerGameState)eventParams["oldGameState"];
		PlayerGameState playerGameState2 = (PlayerGameState)eventParams["newGameState"];
		if (player.IsLocalPlayer && (playerGameState.Phase != playerGameState2.Phase || playerGameState.Team != playerGameState2.Team))
		{
			HandlePlayerGameState(player);
		}
	}
}
