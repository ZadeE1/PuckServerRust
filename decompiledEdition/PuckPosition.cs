using System.Collections.Generic;
using Unity.Netcode;

public class PuckPosition : NetworkBehaviour
{
	public GamePhase Phase;

	protected override void OnNetworkPostSpawn()
	{
		EventManager.TriggerEvent("Event_Everyone_OnPuckPositionSpawned", new Dictionary<string, object> { { "puckPosition", this } });
		base.OnNetworkPostSpawn();
	}

	public override void OnNetworkDespawn()
	{
		EventManager.TriggerEvent("Event_Everyone_OnPuckPositionDespawned", new Dictionary<string, object> { { "puckPosition", this } });
		base.OnNetworkDespawn();
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
		return "PuckPosition";
	}
}
