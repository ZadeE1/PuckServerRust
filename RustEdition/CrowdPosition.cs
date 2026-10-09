using System.Collections.Generic;
using UnityEngine;

public class CrowdPosition : MonoBehaviour
{
	private void Start()
	{
		EventManager.TriggerEvent("Event_OnCrowdPositionSpawned", new Dictionary<string, object> { { "crowdPosition", this } });
	}

	private void OnDestroy()
	{
		EventManager.TriggerEvent("Event_OnCrowdPositionDespawned", new Dictionary<string, object> { { "crowdPosition", this } });
	}

	private void OnDrawGizmos()
	{
		if (Application.isEditor)
		{
			Gizmos.color = Color.white;
			Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.5f);
		}
	}
}
