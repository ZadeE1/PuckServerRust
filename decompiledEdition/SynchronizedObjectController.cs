using Unity.Netcode;

public class SynchronizedObjectController : NetworkBehaviour
{
	private SynchronizedObject synchronizedObject;

	private void Awake()
	{
		synchronizedObject = GetComponent<SynchronizedObject>();
	}

	public override void OnNetworkSpawn()
	{
		if (!NetworkManager.Singleton.IsServer)
		{
			synchronizedObject.DriveExternally(SynchronizedObjectDriveCadence.PerFrame);
		}
		base.OnNetworkSpawn();
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
		return "SynchronizedObjectController";
	}
}
