using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class NetworkObjectCollisionRecorder : NetworkBehaviour
{
	[Header("Settings")]
	[SerializeField]
	private int bufferSize = 10;

	[SerializeField]
	private LayerMask collisionLayers;

	[HideInInspector]
	public NetworkList<NetworkObjectCollision> Buffer;

	private List<NetworkObjectCollision> cachedNetworkObjectCollisions;

	private bool isNetworkVariablesInitialized;

	[HideInInspector]
	public List<NetworkObjectCollision> NetworkObjectCollisions
	{
		get
		{
			if (cachedNetworkObjectCollisions == null)
			{
				cachedNetworkObjectCollisions = Buffer.AsNativeArray().ToList();
			}
			return cachedNetworkObjectCollisions;
		}
	}

	protected override void OnNetworkPreSpawn(ref NetworkManager networkManager)
	{
		InitializeNetworkVariables();
		base.OnNetworkPreSpawn(ref networkManager);
	}

	public override void OnNetworkSpawn()
	{
		Buffer.OnListChanged += OnBufferChanged;
		base.OnNetworkSpawn();
	}

	protected override void OnNetworkPostSpawn()
	{
		if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsConnectedClient)
		{
			ProcessInitialNetworkVariableValues();
		}
		base.OnNetworkPostSpawn();
	}

	protected override void OnNetworkSessionSynchronized()
	{
		ProcessInitialNetworkVariableValues();
		base.OnNetworkSessionSynchronized();
	}

	public override void OnNetworkDespawn()
	{
		Buffer.OnListChanged -= OnBufferChanged;
		base.OnNetworkDespawn();
	}

	public void InitializeNetworkVariables(List<NetworkObjectCollision> buffer = null)
	{
		if (!isNetworkVariablesInitialized)
		{
			isNetworkVariablesInitialized = true;
			Buffer = new NetworkList<NetworkObjectCollision>(buffer);
		}
	}

	private void ProcessInitialNetworkVariableValues()
	{
		OnBufferChanged(new NetworkListEvent<NetworkObjectCollision>
		{
			Type = NetworkListEvent<NetworkObjectCollision>.EventType.Full
		});
	}

	private void OnBufferChanged(NetworkListEvent<NetworkObjectCollision> changeEvent)
	{
		cachedNetworkObjectCollisions = null;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (!NetworkManager.Singleton.IsServer || (collisionLayers.value & (1 << collision.gameObject.layer)) == 0)
		{
			return;
		}
		NetworkObject component = collision.gameObject.GetComponent<NetworkObject>();
		if (!component)
		{
			return;
		}
		NetworkObjectReference networkObjectReference = new NetworkObjectReference(component);
		NetworkObjectCollision? networkObjectCollision = null;
		foreach (NetworkObjectCollision item in Buffer)
		{
			NetworkObjectReference networkObjectReference2 = item.NetworkObjectReference;
			if (networkObjectReference2.Equals(networkObjectReference))
			{
				networkObjectCollision = item;
				break;
			}
		}
		if (networkObjectCollision.HasValue && Buffer.Contains(networkObjectCollision.Value))
		{
			Buffer.Remove(networkObjectCollision.Value);
		}
		if (Buffer.Count >= bufferSize)
		{
			Buffer.RemoveAt(0);
		}
		Buffer.Add(new NetworkObjectCollision
		{
			NetworkObjectReference = networkObjectReference,
			Time = Time.time
		});
	}

	protected override void __initializeVariables()
	{
		if (Buffer == null)
		{
			throw new Exception("NetworkObjectCollisionRecorder.Buffer cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		Buffer.Initialize(this);
		__nameNetworkVariable(Buffer, "Buffer");
		NetworkVariableFields.Add(Buffer);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected override string __getTypeName()
	{
		return "NetworkObjectCollisionRecorder";
	}
}
