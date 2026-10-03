using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Zone : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField]
	private string zoneId;

	private readonly Dictionary<PlayerBody, HashSet<Collider>> occupants = new Dictionary<PlayerBody, HashSet<Collider>>();

	public string ZoneId => zoneId;

	public Vector3 Origin => transform.position;

	public IEnumerable<PlayerBody> Occupants => occupants.Keys;

	private void OnTriggerEnter(Collider other)
	{
		TryAddOccupant(other);
	}

	private void OnTriggerStay(Collider other)
	{
		TryAddOccupant(other);
	}

	private void TryAddOccupant(Collider other)
	{
		PlayerBody componentInParent = other.GetComponentInParent<PlayerBody>();
		if (!(componentInParent == null))
		{
			if (!occupants.TryGetValue(componentInParent, out var value))
			{
				value = new HashSet<Collider>();
				occupants[componentInParent] = value;
			}
			bool flag = value.Count == 0;
			value.Add(other);
			if (flag)
			{
				Fire("Event_Everyone_OnPlayerEnterZone", componentInParent);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		PlayerBody componentInParent = other.GetComponentInParent<PlayerBody>();
		if (!(componentInParent == null) && occupants.TryGetValue(componentInParent, out var value))
		{
			value.Remove(other);
			if (value.Count == 0)
			{
				occupants.Remove(componentInParent);
				Fire("Event_Everyone_OnPlayerExitZone", componentInParent);
			}
		}
	}

	public void ClearOccupant(PlayerBody playerBody)
	{
		occupants.Remove(playerBody);
	}

	private void OnDisable()
	{
		occupants.Clear();
	}

	private void Fire(string eventName, PlayerBody playerBody)
	{
		EventManager.TriggerEvent(eventName, new Dictionary<string, object>
		{
			{ "zone", this },
			{ "playerBody", playerBody }
		});
	}
}
