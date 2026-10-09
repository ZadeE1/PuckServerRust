using System.Collections.Generic;
using UnityEngine;

public class BanManagerController : MonoBehaviour
{
	private BanManager banManager;

	public void Awake()
	{
		banManager = GetComponent<BanManager>();
		EventManager.AddEventListener("Event_Server_OnServerStarted", Event_Server_OnServerStarted);
		EventManager.AddEventListener("Event_Server_OnServerStopped", Event_Server_OnServerStopped);
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_Server_OnServerStarted", Event_Server_OnServerStarted);
		EventManager.RemoveEventListener("Event_Server_OnServerStopped", Event_Server_OnServerStopped);
	}

	private void Event_Server_OnServerStarted(Dictionary<string, object> message)
	{
		banManager.LoadBannedSteamIds();
		banManager.LoadBannedIpAddresses();
	}

	private void Event_Server_OnServerStopped(Dictionary<string, object> message)
	{
		banManager.Dispose();
	}
}
