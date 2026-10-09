using System.Collections.Generic;
using Unity.Netcode;

public class StickController : NetworkBehaviour
{
	private Stick stick;

	private void Awake()
	{
		stick = GetComponent<Stick>();
	}

	public override void OnNetworkSpawn()
	{
		EventManager.AddEventListener("Event_Everyone_OnPlayerGameStateChanged", Event_Everyone_OnPlayerGameStateChanged);
		EventManager.AddEventListener("Event_Everyone_OnPlayerCustomizationStateChanged", Event_Everyone_OnPlayerCustomizationStateChanged);
		base.OnNetworkSpawn();
	}

	public override void OnNetworkDespawn()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerGameStateChanged", Event_Everyone_OnPlayerGameStateChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerCustomizationStateChanged", Event_Everyone_OnPlayerCustomizationStateChanged);
		base.OnNetworkDespawn();
	}

	private void Event_Everyone_OnPlayerGameStateChanged(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		PlayerGameState playerGameState = (PlayerGameState)message["oldGameState"];
		PlayerGameState playerGameState2 = (PlayerGameState)message["newGameState"];
		if (OwnerClientId == player.OwnerClientId && (playerGameState.Team != playerGameState2.Team || playerGameState.Role != playerGameState2.Role))
		{
			stick.ApplyCustomizations();
		}
	}

	private void Event_Everyone_OnPlayerCustomizationStateChanged(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		if (OwnerClientId == player.OwnerClientId)
		{
			stick.ApplyCustomizations();
		}
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected override string __getTypeName()
	{
		return "StickController";
	}
}
