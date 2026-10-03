using System.Collections.Generic;
using UnityEngine;

public class TimeoutManagerController : MonoBehaviour
{
	private TimeoutManager timeoutManager;

	public void Awake()
	{
		timeoutManager = GetComponent<TimeoutManager>();
		EventManager.AddEventListener("Event_Server_OnServerStopped", Event_Server_OnServerStopped);
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_Server_OnServerStopped", Event_Server_OnServerStopped);
	}

	private void Event_Server_OnServerStopped(Dictionary<string, object> message)
	{
		timeoutManager.Dispose();
	}
}
