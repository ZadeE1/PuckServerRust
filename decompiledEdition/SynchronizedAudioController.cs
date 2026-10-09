using System.Collections.Generic;
using Unity.Netcode;

public class SynchronizedAudioController : NetworkBehaviour
{
	private SynchronizedAudio synchronizedAudio;

	private void Awake()
	{
		synchronizedAudio = GetComponent<SynchronizedAudio>();
	}

	public override void OnNetworkSpawn()
	{
		EventManager.AddEventListener("Event_Server_OnClientSceneSynchronizeComplete", Event_Server_OnClientSceneSynchronizeComplete);
		base.OnNetworkSpawn();
	}

	public override void OnNetworkDespawn()
	{
		EventManager.RemoveEventListener("Event_Server_OnClientSceneSynchronizeComplete", Event_Server_OnClientSceneSynchronizeComplete);
		base.OnNetworkDespawn();
	}

	private void Event_Server_OnClientSceneSynchronizeComplete(Dictionary<string, object> message)
	{
		ulong num = (ulong)message["clientId"];
		if (num != 0L)
		{
			synchronizedAudio.Server_ForceSynchronizeClientId(num);
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
		return "SynchronizedAudioController";
	}
}
