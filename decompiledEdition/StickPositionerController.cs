using System.Collections.Generic;
using Unity.Netcode;

public class StickPositionerController : NetworkBehaviour
{
	private StickPositioner stickPositioner;

	private void Awake()
	{
		stickPositioner = GetComponent<StickPositioner>();
	}

	public override void OnNetworkSpawn()
	{
		EventManager.AddEventListener("Event_Everyone_OnStickSpawned", Event_Everyone_OnStickSpawned);
		EventManager.AddEventListener("Event_Everyone_OnPlayerHandednessChanged", Event_Everyone_OnPlayerHandednessChanged);
		base.OnNetworkSpawn();
	}

	public override void OnNetworkDespawn()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnStickSpawned", Event_Everyone_OnStickSpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerHandednessChanged", Event_Everyone_OnPlayerHandednessChanged);
		base.OnNetworkDespawn();
	}

	private void Event_Everyone_OnStickSpawned(Dictionary<string, object> message)
	{
		Stick stick = (Stick)message["stick"];
		if (OwnerClientId == stick.OwnerClientId)
		{
			stickPositioner.PrepareShaftTarget(stick);
		}
	}

	private void Event_Everyone_OnPlayerHandednessChanged(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		if (OwnerClientId == player.OwnerClientId)
		{
			stickPositioner.Handedness = player.Handedness.Value;
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
		return "StickPositionerController";
	}
}
