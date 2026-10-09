using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SynchronizedObjectManager : NetworkBehaviourSingleton<SynchronizedObjectManager>
{
	private static readonly Logger Logger = new Logger("SynchronizedObjectManager");

	[Header("Settings")]
	[SerializeField]
	[Tooltip("Sends rotation at full 8-byte precision instead of 4-byte smallest-three compression for objects that no LOD band and no culling applies to.")]
	private bool serverUseHighPrecisionRotation = true;

	[SerializeField]
	[Range(0f, 0.5f)]
	[Tooltip("Fraction of a band's MinDistance an object must move past the boundary before it changes LOD band or culling state, so objects hovering near a boundary do not switch every tick. 0 disables hysteresis.")]
	private float serverLodHysteresis = 0.1f;

	[SerializeField]
	[Tooltip("Distance bands ordered ascending by MinDistance, overriding the defaults above for objects farther than a band's MinDistance from a player's LOD origin. Objects nearer than the first band, and every object while empty, update every tick.")]
	private SynchronizedObjectBandSettings[] serverLodBands;

	[SerializeField]
	[Tooltip("Overrides the defaults above and any LOD band for objects farther than MinDistance from a player's LOD origin and behind the player's camera. A TickRateDivisor of 1 disables culling.")]
	private SynchronizedObjectBandSettings serverCulling;

	[SerializeField]
	[Range(1f, 16f)]
	[Tooltip("Replaces the LOD bands for players with no LOD origin, meaning spectators and players whose body has not spawned. Their viewpoint is unknown to the server, so neither distance nor culling can be measured.")]
	private byte serverNoOriginTickRateDivisor = 4;

	[SerializeField]
	[Tooltip("Seconds between Event_Server_OnObjectSynchronizationDiagnostics events, one per player, naming how many objects landed on the defaults, in each LOD band, culled and on the no-origin fallback. 0 disables diagnostics.")]
	private float serverDiagnosticsInterval;

	[SerializeField]
	[Tooltip("Client-side snapshot interpolation tuning.")]
	private SynchronizedObjectNetworkSmoothingSettings clientNetworkSmoothingSettings = SynchronizedObjectNetworkSmoothingSettings.Default;

	[SerializeField]
	[Tooltip("Seconds between Event_Client_OnObjectSynchronizationDiagnostics events measuring the incoming synchronization message stream for UI display. 0 disables diagnostics.")]
	private float clientDiagnosticsInterval = 1f;

	private int tickRate = 100;

	private SynchronizedObjectRegistry synchronizedObjectRegistry = new SynchronizedObjectRegistry();

	private List<SynchronizedPlayerState> serverSynchronizedPlayerStates = new List<SynchronizedPlayerState>();

	private uint serverTickCount;

	private double serverTickTime;

	private float serverTickTimeScale = 1f;

	private SynchronizedObjectSnapshot serverSnapshot = new SynchronizedObjectSnapshot();

	private SynchronizedObjectSendPlanner serverSendPlanner = new SynchronizedObjectSendPlanner();

	private SynchronizedObjectSendBuffers serverSendBuffers = new SynchronizedObjectSendBuffers();

	private SynchronizedObjectServerDiagnostics serverDiagnostics = new SynchronizedObjectServerDiagnostics();

	private SynchronizedObjectSnapshot serverForceSnapshot = new SynchronizedObjectSnapshot();

	private SynchronizedObjectSendBuffers serverForceSendBuffers = new SynchronizedObjectSendBuffers();

	private SynchronizedObjectClientReceiver clientReceiver;

	[HideInInspector]
	public int TickRate
	{
		get
		{
			return tickRate;
		}
		set
		{
			if (tickRate != value)
			{
				tickRate = value;
				clientReceiver.SetTickInterval(TickInterval);
			}
		}
	}

	[HideInInspector]
	public float TickInterval => 1f / (float)TickRate;

	public override void Awake()
	{
		base.Awake();
		clientReceiver = new SynchronizedObjectClientReceiver(synchronizedObjectRegistry);
		clientReceiver.Configure(clientNetworkSmoothingSettings, clientDiagnosticsInterval);
		clientReceiver.SetTickInterval(TickInterval);
		PhysicsManager.OnAfterSimulate += OnAfterSimulate;
	}

	public override void OnDestroy()
	{
		PhysicsManager.OnAfterSimulate -= OnAfterSimulate;
		base.OnDestroy();
	}

	private void Update()
	{
		if (IsSpawned && !NetworkManager.Singleton.IsServer)
		{
			clientReceiver.Update(Time.unscaledDeltaTime);
		}
	}

	private void OnAfterSimulate(float deltaTime)
	{
		if (!IsSpawned || !NetworkManager.Singleton.IsServer)
		{
			return;
		}
		try
		{
			Server_Tick(deltaTime);
		}
		catch (Exception arg)
		{
			Logger.Error($"Synchronization tick failed and was skipped: {arg}");
		}
	}

	public void AddSynchronizedObject(SynchronizedObject synchronizedObject)
	{
		synchronizedObjectRegistry.Add(synchronizedObject);
	}

	public void RemoveSynchronizedObject(SynchronizedObject synchronizedObject)
	{
		synchronizedObjectRegistry.Remove(synchronizedObject);
		clientReceiver.Forget(synchronizedObject.NetworkObjectId);
		foreach (SynchronizedPlayerState serverSynchronizedPlayerState in serverSynchronizedPlayerStates)
		{
			serverSynchronizedPlayerState.Forget(synchronizedObject.NetworkObjectId);
		}
	}

	public bool TryGetSynchronizedObject(ulong networkObjectId, out SynchronizedObject synchronizedObject)
	{
		return synchronizedObjectRegistry.TryGet(networkObjectId, out synchronizedObject);
	}

	public void Client_SetTargetTimelinePosition(float value)
	{
		clientNetworkSmoothingSettings.TargetTimelinePosition = value;
		clientReceiver.Configure(clientNetworkSmoothingSettings, clientDiagnosticsInterval);
	}

	public void Client_Dispose()
	{
		clientReceiver.Dispose();
		synchronizedObjectRegistry.Clear();
	}

	public void Server_Dispose()
	{
		serverTickCount = 0u;
		serverTickTime = 0.0;
		serverTickTimeScale = 1f;
		serverSnapshot.Clear();
		serverSendBuffers.Reset();
		serverForceSnapshot.Clear();
		serverForceSendBuffers.Reset();
		serverDiagnostics.Reset();
		synchronizedObjectRegistry.Clear();
		Server_ClearSynchronizedPlayerStates();
	}

	public void Server_AddSynchronizedPlayer(Player player)
	{
		if (Server_TryGetSynchronizedPlayerState(player.OwnerClientId, out var synchronizedPlayerState))
		{
			synchronizedPlayerState.Player = player;
			return;
		}
		serverSynchronizedPlayerStates.Add(new SynchronizedPlayerState
		{
			Player = player,
			RpcTarget = RpcTarget.Single(player.OwnerClientId, RpcTargetUse.Persistent)
		});
	}

	public void Server_RemoveSynchronizedPlayer(Player player)
	{
		if (Server_TryGetSynchronizedPlayerState(player.OwnerClientId, out var synchronizedPlayerState))
		{
			synchronizedPlayerState.Dispose();
			serverSynchronizedPlayerStates.Remove(synchronizedPlayerState);
		}
	}

	public void Server_ForceSynchronizeClientId(ulong clientId)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			serverForceSnapshot.Capture(synchronizedObjectRegistry);
			serverForceSendBuffers.Clear();
			for (int i = 0; i < serverForceSnapshot.Count; i++)
			{
				serverForceSendBuffers.AddReliable(serverForceSnapshot.GetData(i).WithAsleep());
			}
			SynchronizedObjectData[] array = serverForceSendBuffers.TakeReliables();
			if (array.Length != 0)
			{
				Server_ReliableSynchronizeObjectsRpc(Server_GetTickHeader(0), array, RpcTarget.Single(clientId, RpcTargetUse.Temp));
			}
		}
	}

	private bool Server_TryGetSynchronizedPlayerState(ulong clientId, out SynchronizedPlayerState synchronizedPlayerState)
	{
		foreach (SynchronizedPlayerState serverSynchronizedPlayerState in serverSynchronizedPlayerStates)
		{
			if (!(serverSynchronizedPlayerState.Player == null) && serverSynchronizedPlayerState.Player.OwnerClientId == clientId)
			{
				synchronizedPlayerState = serverSynchronizedPlayerState;
				return true;
			}
		}
		synchronizedPlayerState = null;
		return false;
	}

	private void Server_ClearSynchronizedPlayerStates()
	{
		foreach (SynchronizedPlayerState serverSynchronizedPlayerState in serverSynchronizedPlayerStates)
		{
			serverSynchronizedPlayerState.Dispose();
		}
		serverSynchronizedPlayerStates.Clear();
	}

	private void Server_Tick(float deltaTime)
	{
		serverTickCount++;
		serverTickTime += deltaTime;
		serverTickTimeScale = Time.timeScale;
		serverDiagnostics.Configure(serverDiagnosticsInterval, serverLodBands);
		serverDiagnostics.Step(deltaTime);
		if (serverSynchronizedPlayerStates.Count == 0)
		{
			return;
		}
		serverSendPlanner.Configure(serverLodBands, serverCulling, serverNoOriginTickRateDivisor, serverLodHysteresis, serverUseHighPrecisionRotation);
		serverSnapshot.Capture(synchronizedObjectRegistry);
		foreach (SynchronizedPlayerState serverSynchronizedPlayerState in serverSynchronizedPlayerStates)
		{
			Server_SynchronizePlayer(serverSynchronizedPlayerState);
		}
		if (serverDiagnostics.TryGetData(out var data))
		{
			Server_ReportDiagnostics(in data);
		}
	}

	private void Server_SynchronizePlayer(SynchronizedPlayerState synchronizedPlayerState)
	{
		if (synchronizedPlayerState.RpcTarget != null)
		{
			serverSendPlanner.Plan(serverSnapshot, synchronizedPlayerState, serverTickCount, TickRate, serverSendBuffers, serverDiagnostics.BeginPlayer());
			synchronizedPlayerState.SendSequenceNumber++;
			SynchronizedObjectTickHeader tickHeader = Server_TakeTickHeader(synchronizedPlayerState);
			Server_SynchronizeObjectsRpc(tickHeader, serverSendBuffers.TakeDeltas(), synchronizedPlayerState.RpcTarget);
			SynchronizedObjectData[] array = serverSendBuffers.TakeReliables();
			if (array.Length != 0 || tickHeader.ChangeMask != 0)
			{
				Server_ReliableSynchronizeObjectsRpc(tickHeader, array, synchronizedPlayerState.RpcTarget);
			}
			serverDiagnostics.EndPlayer(synchronizedPlayerState);
		}
	}

	private void Server_ReportDiagnostics(in SynchronizedObjectServerDiagnosticsData diagnosticsData)
	{
		if (EventManager.HasEventListeners("Event_Server_OnObjectSynchronizationDiagnostics"))
		{
			EventManager.TriggerEvent("Event_Server_OnObjectSynchronizationDiagnostics", new Dictionary<string, object> { { "diagnosticsData", diagnosticsData } });
		}
	}

	private SynchronizedObjectTickHeader Server_GetTickHeader(ushort sequenceNumber)
	{
		return new SynchronizedObjectTickHeader(sequenceNumber, serverTickTime, serverTickTimeScale, TickInterval);
	}

	private SynchronizedObjectTickHeader Server_TakeTickHeader(SynchronizedPlayerState synchronizedPlayerState)
	{
		SynchronizedObjectTickHeader lastSentTickHeader = Server_GetTickHeader(synchronizedPlayerState.SendSequenceNumber);
		byte componentMask = (byte)(Server_IsTickHeaderFullSendDue(synchronizedPlayerState) ? 3 : lastSentTickHeader.GetChangeMask(synchronizedPlayerState.LastSentTickHeader));
		synchronizedPlayerState.LastSentTickHeader = lastSentTickHeader;
		synchronizedPlayerState.HasSentTickHeader = true;
		return lastSentTickHeader.WithComponentMask(componentMask);
	}

	private bool Server_IsTickHeaderFullSendDue(SynchronizedPlayerState synchronizedPlayerState)
	{
		if (!synchronizedPlayerState.HasSentTickHeader)
		{
			return true;
		}
		return serverTickCount % (uint)TickRate == 0;
	}

	[Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server, Delivery = RpcDelivery.Unreliable)]
	private void Server_SynchronizeObjectsRpc(SynchronizedObjectTickHeader tickHeader, SynchronizedObjectData[] synchronizedObjectsData, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				InvokePermission = RpcInvokePermission.Server,
				Delivery = RpcDelivery.Unreliable
			};
			FastBufferWriter bufferWriter = __beginSendRpc(2889824980u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Unreliable);
			bufferWriter.WriteValueSafe(in tickHeader, default(FastBufferWriter.ForNetworkSerializable));
			bool value = synchronizedObjectsData != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(synchronizedObjectsData, default(FastBufferWriter.ForNetworkSerializable));
			}
			__endSendRpc(ref bufferWriter, 2889824980u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Unreliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			clientReceiver.ReceiveTick(tickHeader, synchronizedObjectsData);
		}
	}

	[Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server, Delivery = RpcDelivery.Reliable)]
	private void Server_ReliableSynchronizeObjectsRpc(SynchronizedObjectTickHeader tickHeader, SynchronizedObjectData[] synchronizedObjectsData, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				InvokePermission = RpcInvokePermission.Server,
				Delivery = RpcDelivery.Reliable
			};
			FastBufferWriter bufferWriter = __beginSendRpc(3545231968u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in tickHeader, default(FastBufferWriter.ForNetworkSerializable));
			bool value = synchronizedObjectsData != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(synchronizedObjectsData, default(FastBufferWriter.ForNetworkSerializable));
			}
			__endSendRpc(ref bufferWriter, 3545231968u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			clientReceiver.ReceiveReliable(tickHeader, synchronizedObjectsData);
		}
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(2889824980u, __rpc_handler_2889824980, "Server_SynchronizeObjectsRpc", RpcInvokePermission.Server);
		__registerRpc(3545231968u, __rpc_handler_3545231968, "Server_ReliableSynchronizeObjectsRpc", RpcInvokePermission.Server);
		base.__initializeRpcs();
	}

	private static void __rpc_handler_2889824980(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SynchronizedObjectTickHeader value, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out bool value2, default(FastBufferWriter.ForPrimitives));
			SynchronizedObjectData[] value3 = null;
			if (value2)
			{
				reader.ReadValueSafe(out value3, default(FastBufferWriter.ForNetworkSerializable));
			}
			RpcParams ext = rpcParams.Ext;
			((SynchronizedObjectManager)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((SynchronizedObjectManager)target).Server_SynchronizeObjectsRpc(value, value3, ext);
			((SynchronizedObjectManager)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3545231968(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out SynchronizedObjectTickHeader value, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out bool value2, default(FastBufferWriter.ForPrimitives));
			SynchronizedObjectData[] value3 = null;
			if (value2)
			{
				reader.ReadValueSafe(out value3, default(FastBufferWriter.ForNetworkSerializable));
			}
			RpcParams ext = rpcParams.Ext;
			((SynchronizedObjectManager)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((SynchronizedObjectManager)target).Server_ReliableSynchronizeObjectsRpc(value, value3, ext);
			((SynchronizedObjectManager)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected override string __getTypeName()
	{
		return "SynchronizedObjectManager";
	}
}
