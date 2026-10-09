using System.Collections.Generic;
using UnityEngine;

public class DeploymentManagerController : MonoBehaviour
{
	private DeploymentManager deploymentManager;

	public void Awake()
	{
		deploymentManager = GetComponent<DeploymentManager>();
		EventManager.AddEventListener("Event_OnServerStateChanged", Event_OnServerStateChanged);
		EventManager.AddEventListener("Event_Everyone_OnPlayerAdded", Event_Everyone_OnPlayerAdded);
		EventManager.AddEventListener("Event_Everyone_OnPlayerRemoved", Event_Everyone_OnPlayerRemoved);
	}

	private void Start()
	{
		deploymentManager.StartEmptyTimeout();
		deploymentManager.StartAuthenticationTimeout();
	}

	private void OnDestroy()
	{
		deploymentManager.StopEmptyTimeout();
		deploymentManager.StopAuthenticationTimeout();
		EventManager.RemoveEventListener("Event_OnServerStateChanged", Event_OnServerStateChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerAdded", Event_Everyone_OnPlayerAdded);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerRemoved", Event_Everyone_OnPlayerRemoved);
	}

	private void Event_OnServerStateChanged(Dictionary<string, object> message)
	{
		ServerState serverState = (ServerState)message["oldServerState"];
		ServerState serverState2 = (ServerState)message["newServerState"];
		if (serverState.AuthenticationPhase != serverState2.AuthenticationPhase)
		{
			if (serverState2.AuthenticationPhase == AuthenticationPhase.Authenticated)
			{
				deploymentManager.StopAuthenticationTimeout();
			}
			else
			{
				deploymentManager.StopDeployment();
			}
		}
	}

	private void Event_Everyone_OnPlayerAdded(Dictionary<string, object> message)
	{
		deploymentManager.StopEmptyTimeout();
	}

	private void Event_Everyone_OnPlayerRemoved(Dictionary<string, object> message)
	{
		if (MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayers().Count == 0)
		{
			deploymentManager.StartEmptyTimeout();
		}
	}
}
