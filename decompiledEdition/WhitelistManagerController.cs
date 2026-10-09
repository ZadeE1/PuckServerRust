using System.Collections.Generic;
using UnityEngine;

public class WhitelistManagerController : MonoBehaviour
{
	private WhitelistManager whitelistManager;

	public void Awake()
	{
		whitelistManager = GetComponent<WhitelistManager>();
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
		whitelistManager.LoadWhitelistedSteamIds();
	}

	private void Event_Server_OnServerStopped(Dictionary<string, object> message)
	{
		whitelistManager.Dispose();
	}
}
