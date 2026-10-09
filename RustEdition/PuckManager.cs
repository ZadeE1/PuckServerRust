using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class PuckManager : MonoBehaviourSingleton<PuckManager>
{
	private static readonly Logger Logger = new Logger("PuckManager");

	[Header("Prefabs")]
	[SerializeField]
	private Puck puckPrefab;

	private List<PuckPosition> puckPositions = new List<PuckPosition>();

	private List<Puck> pucks = new List<Puck>();

	private List<Puck> nonReplayPucks = new List<Puck>();

	public void AddPuckPosition(PuckPosition puckPosition)
	{
		Logger.Info($"Added puck position for phase {puckPosition.Phase}");
		puckPositions.Add(puckPosition);
	}

	public void RemovePuckPosition(PuckPosition puckPosition)
	{
		puckPositions.Remove(puckPosition);
	}

	public void AddPuck(Puck puck)
	{
		pucks.Add(puck);
		if (!puck.IsReplay.Value)
		{
			nonReplayPucks.Add(puck);
		}
	}

	public void RemovePuck(Puck puck)
	{
		pucks.Remove(puck);
		nonReplayPucks.Remove(puck);
	}

	public List<Puck> GetPucks(bool includeReplay = false)
	{
		if (includeReplay)
		{
			return pucks;
		}
		return nonReplayPucks;
	}

	public List<Puck> GetReplayPucks()
	{
		return pucks.Where((Puck puck) => puck.IsReplay.Value).ToList();
	}

	public Puck GetPuck(bool includeReplay = false)
	{
		return GetPucks(includeReplay).FirstOrDefault((Puck puck) => puck);
	}

	public Puck GetPlayerPuck(ulong clientId)
	{
		Player playerByClientId = MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayerByClientId(clientId);
		if (!playerByClientId)
		{
			return null;
		}
		if (!playerByClientId.Stick)
		{
			return null;
		}
		NetworkList<NetworkObjectCollision> buffer = playerByClientId.Stick.NetworkObjectCollisionRecorder.Buffer;
		if (buffer.Count == 0)
		{
			return null;
		}
		return NetworkingUtils.GetPuckFromNetworkObjectReference(buffer[buffer.Count - 1].NetworkObjectReference);
	}

	public Puck GetPuckByNetworkObjectId(ulong networkObjectId)
	{
		return GetPucks().FirstOrDefault((Puck puck) => puck.NetworkObjectId == networkObjectId);
	}

	public Puck GetReplayPuckByNetworkObjectId(ulong networkObjectId)
	{
		return GetReplayPucks().FirstOrDefault((Puck puck) => puck.NetworkObjectId == networkObjectId);
	}

	public Puck Server_SpawnPuck(Vector3 position, Quaternion rotation, bool isReplay = false)
	{
		if (!NetworkManager.Singleton.IsServer)
		{
			return null;
		}
		Puck puck = Object.Instantiate(puckPrefab, position, rotation);
		puck.InitializeNetworkVariables(isReplay);
		puck.NetworkObject.Spawn();
		Logger.Info($"Spawned puck {puck.NetworkObjectId}");
		return puck;
	}

	public void Server_DespawnPuck(Puck puck)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			puck.NetworkObject.Despawn();
			Logger.Info($"Despawned puck {puck.NetworkObjectId}");
		}
	}

	public void Server_DespawnPucks(bool includeReplay = false)
	{
		if (!NetworkManager.Singleton.IsServer)
		{
			return;
		}
		Logger.Info($"Despawning {pucks.Count} pucks (includeReplay: {includeReplay})");
		foreach (Puck item in pucks.ToList())
		{
			if (includeReplay || !item.IsReplay.Value)
			{
				Server_DespawnPuck(item);
			}
		}
	}

	public void Server_SpawnPucksForPhase(GamePhase phase)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Logger.Info($"Spawning pucks for phase {phase}");
			puckPositions.FindAll((PuckPosition puckPosition) => puckPosition.Phase == phase).ForEach((PuckPosition puckPosition) =>
			{
				Server_SpawnPuck(puckPosition.transform.position, puckPosition.transform.rotation);
			});
		}
	}
}
