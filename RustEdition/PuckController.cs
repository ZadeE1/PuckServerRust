using Unity.Netcode;

public class PuckController : NetworkBehaviour
{
	private Puck puck;

	private void Awake()
	{
		puck = GetComponent<Puck>();
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();
	}

	public override void OnNetworkDespawn()
	{
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
		return "PuckController";
	}
}
