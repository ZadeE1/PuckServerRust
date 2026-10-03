using System.Collections.Generic;
using UnityEngine;

public class ReplayManagerController : MonoBehaviour
{
	private ReplayManager replayManager;

	private void Awake()
	{
		replayManager = GetComponent<ReplayManager>();
		EventManager.AddEventListener("Event_Server_OnServerStopped", Event_Server_OnServerStopped);
	}

	private void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_Server_OnServerStopped", Event_Server_OnServerStopped);
	}

	private void Event_Server_OnServerStopped(Dictionary<string, object> message)
	{
		replayManager.Server_StopReplaying();
		replayManager.Server_StopRecording();
	}
}
