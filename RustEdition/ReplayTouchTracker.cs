using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ReplayTouchTracker : MonoBehaviour
{
	private HashSet<ulong> activeContactObjectIds = new HashSet<ulong>();

	private Dictionary<Player, float> lastContactTimeByPlayer = new Dictionary<Player, float>();

	public IReadOnlyDictionary<Player, float> LastContactTimeByPlayer => lastContactTimeByPlayer;

	private void OnCollisionEnter(Collision collision)
	{
		RecordContact(collision.gameObject, entering: true);
	}

	private void OnCollisionExit(Collision collision)
	{
		RecordContact(collision.gameObject, entering: false);
	}

	private void RecordContact(GameObject collisionObject, bool entering)
	{
		if (!NetworkManager.Singleton.IsServer || !collisionObject.TryGetComponent<NetworkObject>(out var component))
		{
			return;
		}
		Player player = ResolvePlayer(component);
		if ((bool)player)
		{
			if (entering)
			{
				activeContactObjectIds.Add(component.NetworkObjectId);
			}
			else
			{
				activeContactObjectIds.Remove(component.NetworkObjectId);
			}
			lastContactTimeByPlayer[player] = Time.time;
		}
	}

	private static Player ResolvePlayer(NetworkObject networkObject)
	{
		if (networkObject.TryGetComponent<PlayerBody>(out var component))
		{
			return component.Player;
		}
		if (networkObject.TryGetComponent<Stick>(out var component2))
		{
			return component2.Player;
		}
		return null;
	}

	public bool IsActivelyTouching(NetworkObject networkObject)
	{
		if (networkObject == null)
		{
			return false;
		}
		activeContactObjectIds.RemoveWhere((ulong id) => !NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(id));
		return activeContactObjectIds.Contains(networkObject.NetworkObjectId);
	}
}
