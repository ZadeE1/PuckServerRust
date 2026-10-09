using System;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviourSingleton<GameManager>
{
	[HideInInspector]
	public NetworkVariable<GameState> GameState;

	private Tween tickTween;

	private Sequence slowMotionSequence;

	[HideInInspector]
	public GamePhase Phase => GameState.Value.Phase;

	[HideInInspector]
	public int Tick => GameState.Value.Tick;

	[HideInInspector]
	public int Period => GameState.Value.Period;

	[HideInInspector]
	public int BlueScore => GameState.Value.BlueScore;

	[HideInInspector]
	public int RedScore => GameState.Value.RedScore;

	[HideInInspector]
	public bool IsOvertime => GameState.Value.IsOvertime;

	public bool IsTicking
	{
		get
		{
			if (tickTween != null)
			{
				return tickTween.IsActive();
			}
			return false;
		}
	}

	protected override void OnNetworkPreSpawn(ref NetworkManager networkManager)
	{
		if (GameState == null)
		{
			GameState = new NetworkVariable<GameState>();
		}
		if (networkManager.IsServer)
		{
			GameState.Value = default;
		}
		base.OnNetworkPreSpawn(ref networkManager);
	}

	public override void OnNetworkSpawn()
	{
		NetworkVariable<GameState> gameState = GameState;
		gameState.OnValueChanged = (NetworkVariable<GameState>.OnValueChangedDelegate)Delegate.Combine(gameState.OnValueChanged, new NetworkVariable<GameState>.OnValueChangedDelegate(OnGameStateChanged));
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
		if (NetworkManager.Singleton.IsServer)
		{
			tickTween?.Kill();
			Server_StopSlowMotion(notifyClients: false);
		}
		NetworkVariable<GameState> gameState = GameState;
		gameState.OnValueChanged = (NetworkVariable<GameState>.OnValueChangedDelegate)Delegate.Remove(gameState.OnValueChanged, new NetworkVariable<GameState>.OnValueChangedDelegate(OnGameStateChanged));
		base.OnNetworkDespawn();
	}

	private void ProcessInitialNetworkVariableValues()
	{
		OnGameStateChanged(default, GameState.Value);
	}

	private void OnGameStateChanged(GameState oldGameState, GameState newGameState)
	{
		EventManager.TriggerEvent("Event_Everyone_OnGameStateChanged", new Dictionary<string, object>
		{
			{ "oldGameState", oldGameState },
			{ "newGameState", newGameState }
		});
	}

	private void Server_Tick()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			if (GameState.Value.Tick <= 0)
			{
				Server_ExpirePhase();
				return;
			}
			int? tick = GameState.Value.Tick - 1;
			Server_SetGameState(null, tick);
		}
	}

	public void Server_ExpirePhase()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			EventManager.TriggerEvent("Event_Server_OnPhaseExpired", new Dictionary<string, object> { 
			{
				"phase",
				GameState.Value.Phase
			} });
		}
	}

	public void Server_StartTicking()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			tickTween?.Kill();
			tickTween = DOVirtual.DelayedCall(1f, Server_Tick).SetLoops(-1);
		}
	}

	public void Server_StopTicking()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			tickTween?.Kill();
		}
	}

	public void Server_StartSlowMotion(float scale, float rampInSeconds, float holdSeconds, float rampOutSeconds, float audioPitchFloor = 1f, float cameraFovPunch = 0f)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			float timeScale = Time.timeScale;
			slowMotionSequence?.Kill();
			scale = Mathf.Clamp(scale, 0.01f, 1f);
			slowMotionSequence = TweenUtils.RampHoldRamp(timeScale, scale, 1f, rampInSeconds, holdSeconds, rampOutSeconds, Ease.InQuad, Ease.OutQuad, (float value) =>
			{
				Time.timeScale = value;
			}).SetTarget(this);
			slowMotionSequence.OnKill(() =>
			{
				slowMotionSequence = null;
			});
			Server_NotifySlowMotionRpc(scale, rampInSeconds, holdSeconds, rampOutSeconds, audioPitchFloor, cameraFovPunch);
		}
	}

	[Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server, DeferLocal = true)]
	private void Server_NotifySlowMotionRpc(float scale, float rampInSeconds, float holdSeconds, float rampOutSeconds, float audioPitchFloor, float cameraFovPunch)
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
				DeferLocal = true
			};
			RpcParams rpcParams = default;
			FastBufferWriter bufferWriter = __beginSendRpc(1445539067u, rpcParams, attributeParams, SendTo.Everyone, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in scale, default(FastBufferWriter.ForPrimitives));
			bufferWriter.WriteValueSafe(in rampInSeconds, default(FastBufferWriter.ForPrimitives));
			bufferWriter.WriteValueSafe(in holdSeconds, default(FastBufferWriter.ForPrimitives));
			bufferWriter.WriteValueSafe(in rampOutSeconds, default(FastBufferWriter.ForPrimitives));
			bufferWriter.WriteValueSafe(in audioPitchFloor, default(FastBufferWriter.ForPrimitives));
			bufferWriter.WriteValueSafe(in cameraFovPunch, default(FastBufferWriter.ForPrimitives));
			__endSendRpc(ref bufferWriter, 1445539067u, rpcParams, attributeParams, SendTo.Everyone, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			EventManager.TriggerEvent("Event_Everyone_OnSlowMotionStarted", new Dictionary<string, object>
			{
				{ "scale", scale },
				{ "rampInSeconds", rampInSeconds },
				{ "holdSeconds", holdSeconds },
				{ "rampOutSeconds", rampOutSeconds },
				{ "audioPitchFloor", audioPitchFloor },
				{ "cameraFovPunch", cameraFovPunch }
			});
		}
	}

	[Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server, DeferLocal = true)]
	public void Server_NotifyReplayCellyCamRpc(ulong scorerClientId)
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
				DeferLocal = true
			};
			RpcParams rpcParams = default;
			FastBufferWriter bufferWriter = __beginSendRpc(3581845443u, rpcParams, attributeParams, SendTo.Everyone, RpcDelivery.Reliable);
			BytePacker.WriteValueBitPacked(bufferWriter, scorerClientId);
			__endSendRpc(ref bufferWriter, 3581845443u, rpcParams, attributeParams, SendTo.Everyone, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			EventManager.TriggerEvent("Event_Everyone_OnReplayCellyCam", new Dictionary<string, object> { { "scorerClientId", scorerClientId } });
		}
	}

	public void Server_StopSlowMotion(bool notifyClients = true)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			bool flag = slowMotionSequence != null;
			slowMotionSequence?.Kill();
			Time.timeScale = 1f;
			if (notifyClients & flag)
			{
				Server_NotifySlowMotionStopRpc();
			}
		}
	}

	[Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server, DeferLocal = true)]
	private void Server_NotifySlowMotionStopRpc()
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
				DeferLocal = true
			};
			RpcParams rpcParams = default;
			FastBufferWriter bufferWriter = __beginSendRpc(2324051025u, rpcParams, attributeParams, SendTo.Everyone, RpcDelivery.Reliable);
			__endSendRpc(ref bufferWriter, 2324051025u, rpcParams, attributeParams, SendTo.Everyone, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			EventManager.TriggerEvent("Event_Everyone_OnSlowMotionStopped", new Dictionary<string, object>());
		}
	}

	public void Server_SetGameState(GamePhase? phase = null, int? tick = null, int? period = null, int? blueScore = null, int? redScore = null, bool? isOvertime = null)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			GameState value = new GameState
			{
				Phase = (phase ?? GameState.Value.Phase),
				Tick = (tick ?? GameState.Value.Tick),
				Period = (period ?? GameState.Value.Period),
				BlueScore = (blueScore ?? GameState.Value.BlueScore),
				RedScore = (redScore ?? GameState.Value.RedScore),
				IsOvertime = (isOvertime ?? GameState.Value.IsOvertime)
			};
			GameState.Value = value;
		}
	}

	[Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server, DeferLocal = true)]
	public void Server_NotifyGoalScoredRpc(PlayerTeam byTeam, NetworkObjectReference goalPlayerNetworkObjectReference, NetworkObjectReference assistPlayerNetworkObjectReference, NetworkObjectReference secondAssistPlayerNetworkObjectReference, NetworkObjectReference puckNetworkObjectReference)
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
				DeferLocal = true
			};
			RpcParams rpcParams = default;
			FastBufferWriter bufferWriter = __beginSendRpc(1809670267u, rpcParams, attributeParams, SendTo.Everyone, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in byTeam, default(FastBufferWriter.ForEnums));
			bufferWriter.WriteValueSafe(in goalPlayerNetworkObjectReference, default(FastBufferWriter.ForNetworkSerializable));
			bufferWriter.WriteValueSafe(in assistPlayerNetworkObjectReference, default(FastBufferWriter.ForNetworkSerializable));
			bufferWriter.WriteValueSafe(in secondAssistPlayerNetworkObjectReference, default(FastBufferWriter.ForNetworkSerializable));
			bufferWriter.WriteValueSafe(in puckNetworkObjectReference, default(FastBufferWriter.ForNetworkSerializable));
			__endSendRpc(ref bufferWriter, 1809670267u, rpcParams, attributeParams, SendTo.Everyone, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			Player playerFromNetworkObjectReference = NetworkingUtils.GetPlayerFromNetworkObjectReference(goalPlayerNetworkObjectReference);
			Player playerFromNetworkObjectReference2 = NetworkingUtils.GetPlayerFromNetworkObjectReference(assistPlayerNetworkObjectReference);
			Player playerFromNetworkObjectReference3 = NetworkingUtils.GetPlayerFromNetworkObjectReference(secondAssistPlayerNetworkObjectReference);
			Puck puckFromNetworkObjectReference = NetworkingUtils.GetPuckFromNetworkObjectReference(puckNetworkObjectReference);
			EventManager.TriggerEvent("Event_Everyone_OnGoalScored", new Dictionary<string, object>
			{
				{ "byTeam", byTeam },
				{ "goalPlayer", playerFromNetworkObjectReference },
				{ "assistPlayer", playerFromNetworkObjectReference2 },
				{ "secondAssistPlayer", playerFromNetworkObjectReference3 },
				{ "puck", puckFromNetworkObjectReference }
			});
		}
	}

	protected override void __initializeVariables()
	{
		if (GameState == null)
		{
			throw new Exception("GameManager.GameState cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		GameState.Initialize(this);
		__nameNetworkVariable(GameState, "GameState");
		NetworkVariableFields.Add(GameState);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(1445539067u, __rpc_handler_1445539067, "Server_NotifySlowMotionRpc", RpcInvokePermission.Server);
		__registerRpc(3581845443u, __rpc_handler_3581845443, "Server_NotifyReplayCellyCamRpc", RpcInvokePermission.Server);
		__registerRpc(2324051025u, __rpc_handler_2324051025, "Server_NotifySlowMotionStopRpc", RpcInvokePermission.Server);
		__registerRpc(1809670267u, __rpc_handler_1809670267, "Server_NotifyGoalScoredRpc", RpcInvokePermission.Server);
		base.__initializeRpcs();
	}

	private static void __rpc_handler_1445539067(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out float value, default(FastBufferWriter.ForPrimitives));
			reader.ReadValueSafe(out float value2, default(FastBufferWriter.ForPrimitives));
			reader.ReadValueSafe(out float value3, default(FastBufferWriter.ForPrimitives));
			reader.ReadValueSafe(out float value4, default(FastBufferWriter.ForPrimitives));
			reader.ReadValueSafe(out float value5, default(FastBufferWriter.ForPrimitives));
			reader.ReadValueSafe(out float value6, default(FastBufferWriter.ForPrimitives));
			((GameManager)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((GameManager)target).Server_NotifySlowMotionRpc(value, value2, value3, value4, value5, value6);
			((GameManager)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_3581845443(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out ulong value);
			((GameManager)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((GameManager)target).Server_NotifyReplayCellyCamRpc(value);
			((GameManager)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2324051025(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			((GameManager)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((GameManager)target).Server_NotifySlowMotionStopRpc();
			((GameManager)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_1809670267(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out PlayerTeam value, default(FastBufferWriter.ForEnums));
			reader.ReadValueSafe(out NetworkObjectReference value2, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out NetworkObjectReference value3, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out NetworkObjectReference value4, default(FastBufferWriter.ForNetworkSerializable));
			reader.ReadValueSafe(out NetworkObjectReference value5, default(FastBufferWriter.ForNetworkSerializable));
			((GameManager)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((GameManager)target).Server_NotifyGoalScoredRpc(value, value2, value3, value4, value5);
			((GameManager)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected override string __getTypeName()
	{
		return "GameManager";
	}
}
