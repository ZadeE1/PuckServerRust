using System.Collections.Generic;
using UnityEngine;

public class AdminManagerController : MonoBehaviour
{
	private AdminManager adminManager;

	public void Awake()
	{
		adminManager = GetComponent<AdminManager>();
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
		adminManager.LoadAdminSteamIds();
	}

	private void Event_Server_OnServerStopped(Dictionary<string, object> message)
	{
		adminManager.Dispose();
	}
}
