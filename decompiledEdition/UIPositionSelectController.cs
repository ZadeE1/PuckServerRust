using System.Collections.Generic;

internal class UIPositionSelectController : UIViewController<UIPositionSelect>
{
	private UIPositionSelect uiPositionSelect;

	public override void Awake()
	{
		base.Awake();
		uiPositionSelect = GetComponent<UIPositionSelect>();
		EventManager.AddEventListener("Event_Everyone_OnPlayerPositionSpawned", Event_Everyone_OnPlayerPositionSpawned);
		EventManager.AddEventListener("Event_Everyone_OnPlayerPositionDespawned", Event_Everyone_OnPlayerPositionDespawned);
		EventManager.AddEventListener("Event_Everyone_OnPlayerPositionClaimedByPlayerChanged", Event_Everyone_OnPlayerPositionClaimedByPlayerChanged);
		EventManager.AddEventListener("Event_Everyone_OnPlayerSpawned", Event_Everyone_OnPlayerSpawned);
		EventManager.AddEventListener("Event_Everyone_OnPlayerGameStateChanged", Event_Everyone_OnPlayerGameStateChanged);
	}

	public override void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerPositionSpawned", Event_Everyone_OnPlayerPositionSpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerPositionDespawned", Event_Everyone_OnPlayerPositionDespawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerPositionClaimedByPlayerChanged", Event_Everyone_OnPlayerPositionClaimedByPlayerChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerSpawned", Event_Everyone_OnPlayerSpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerGameStateChanged", Event_Everyone_OnPlayerGameStateChanged);
		base.OnDestroy();
	}

	private void HandlePlayerGameState(Player player)
	{
		PlayerGameState value = player.GameState.Value;
		uiPositionSelect.Team = value.Team;
	}

	private void Event_Everyone_OnPlayerPositionSpawned(Dictionary<string, object> message)
	{
		PlayerPosition playerPosition = (PlayerPosition)message["playerPosition"];
		uiPositionSelect.AddPosition(playerPosition);
	}

	private void Event_Everyone_OnPlayerPositionDespawned(Dictionary<string, object> message)
	{
		PlayerPosition playerPosition = (PlayerPosition)message["playerPosition"];
		uiPositionSelect.RemovePosition(playerPosition);
	}

	private void Event_Everyone_OnPlayerPositionClaimedByPlayerChanged(Dictionary<string, object> message)
	{
		PlayerPosition playerPosition = (PlayerPosition)message["playerPosition"];
		uiPositionSelect.StylePosition(playerPosition);
	}

	private void Event_Everyone_OnPlayerSpawned(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		if (player.IsLocalPlayer)
		{
			HandlePlayerGameState(player);
		}
	}

	private void Event_Everyone_OnPlayerGameStateChanged(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		PlayerGameState playerGameState = (PlayerGameState)message["oldGameState"];
		PlayerGameState playerGameState2 = (PlayerGameState)message["newGameState"];
		if (player.IsLocalPlayer && playerGameState.Team != playerGameState2.Team)
		{
			HandlePlayerGameState(player);
		}
	}
}
