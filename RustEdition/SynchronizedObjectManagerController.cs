using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SynchronizedObjectManagerController : MonoBehaviour
{
	private SynchronizedObjectManager synchronizedObjectManager;

	private void Awake()
	{
		synchronizedObjectManager = GetComponent<SynchronizedObjectManager>();
		EventManager.AddEventListener("Event_Everyone_OnSynchronizedObjectSpawned", Event_Everyone_OnSynchronizedObjectSpawned);
		EventManager.AddEventListener("Event_Everyone_OnSynchronizedObjectDespawned", Event_Everyone_OnSynchronizedObjectDespawned);
		EventManager.AddEventListener("Event_Everyone_OnPlayerSpawned", Event_Everyone_OnPlayerSpawned);
		EventManager.AddEventListener("Event_Everyone_OnPlayerDespawned", Event_Everyone_OnPlayerDespawned);
		EventManager.AddEventListener("Event_OnPhysicsManagerInitialized", Event_OnPhysicsManagerInitialized);
		EventManager.AddEventListener("Event_OnPhysicsManagerTickRateChanged", Event_OnPhysicsManagerTickRateChanged);
		EventManager.AddEventListener("Event_OnNetworkBufferingChanged", Event_OnNetworkBufferingChanged);
		EventManager.AddEventListener("Event_OnClientStopped", Event_OnClientStopped);
		EventManager.AddEventListener("Event_Server_OnServerStopped", Event_Server_OnServerStopped);
		EventManager.AddEventListener("Event_Server_OnClientSceneSynchronizeComplete", Event_Server_OnClientSceneSynchronizeComplete);
	}

	private void Start()
	{
		synchronizedObjectManager.Client_SetTargetTimelinePosition(Utils.GetTargetTimelinePositionFromNetworkBuffering(SettingsManager.NetworkBuffering));
	}

	private void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnSynchronizedObjectSpawned", Event_Everyone_OnSynchronizedObjectSpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnSynchronizedObjectDespawned", Event_Everyone_OnSynchronizedObjectDespawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerSpawned", Event_Everyone_OnPlayerSpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerDespawned", Event_Everyone_OnPlayerDespawned);
		EventManager.RemoveEventListener("Event_OnPhysicsManagerInitialized", Event_OnPhysicsManagerInitialized);
		EventManager.RemoveEventListener("Event_OnPhysicsManagerTickRateChanged", Event_OnPhysicsManagerTickRateChanged);
		EventManager.RemoveEventListener("Event_OnNetworkBufferingChanged", Event_OnNetworkBufferingChanged);
		EventManager.RemoveEventListener("Event_OnClientStopped", Event_OnClientStopped);
		EventManager.RemoveEventListener("Event_Server_OnServerStopped", Event_Server_OnServerStopped);
		EventManager.RemoveEventListener("Event_Server_OnClientSceneSynchronizeComplete", Event_Server_OnClientSceneSynchronizeComplete);
	}

	private void Event_Everyone_OnSynchronizedObjectSpawned(Dictionary<string, object> message)
	{
		SynchronizedObject synchronizedObject = (SynchronizedObject)message["synchronizedObject"];
		synchronizedObjectManager.AddSynchronizedObject(synchronizedObject);
	}

	private void Event_Everyone_OnSynchronizedObjectDespawned(Dictionary<string, object> message)
	{
		SynchronizedObject synchronizedObject = (SynchronizedObject)message["synchronizedObject"];
		synchronizedObjectManager.RemoveSynchronizedObject(synchronizedObject);
	}

	private void Event_Everyone_OnPlayerSpawned(Dictionary<string, object> message)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Player player = (Player)message["player"];
			if (!player.IsReplay.Value && player.OwnerClientId != 0L)
			{
				synchronizedObjectManager.Server_AddSynchronizedPlayer(player);
			}
		}
	}

	private void Event_Everyone_OnPlayerDespawned(Dictionary<string, object> message)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Player player = (Player)message["player"];
			if (!player.IsReplay.Value && player.OwnerClientId != 0L)
			{
				synchronizedObjectManager.Server_RemoveSynchronizedPlayer(player);
			}
		}
	}

	private void Event_OnPhysicsManagerInitialized(Dictionary<string, object> message)
	{
		int tickRate = (int)message["tickRate"];
		synchronizedObjectManager.TickRate = tickRate;
	}

	private void Event_OnPhysicsManagerTickRateChanged(Dictionary<string, object> message)
	{
		int tickRate = (int)message["value"];
		synchronizedObjectManager.TickRate = tickRate;
	}

	private void Event_OnNetworkBufferingChanged(Dictionary<string, object> message)
	{
		NetworkBuffering networkBuffering = (NetworkBuffering)message["value"];
		synchronizedObjectManager.Client_SetTargetTimelinePosition(Utils.GetTargetTimelinePositionFromNetworkBuffering(networkBuffering));
	}

	private void Event_OnClientStopped(Dictionary<string, object> message)
	{
		synchronizedObjectManager.Client_Dispose();
	}

	private void Event_Server_OnServerStopped(Dictionary<string, object> message)
	{
		synchronizedObjectManager.Server_Dispose();
	}

	private void Event_Server_OnClientSceneSynchronizeComplete(Dictionary<string, object> message)
	{
		ulong num = (ulong)message["clientId"];
		if (num != 0L)
		{
			synchronizedObjectManager.Server_ForceSynchronizeClientId(num);
		}
	}
}
