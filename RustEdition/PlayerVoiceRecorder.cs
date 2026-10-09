using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Steamworks;
using Unity.Netcode;
using UnityEngine;

public class PlayerVoiceRecorder : NetworkBehaviour
{
	private static readonly Logger Logger = new Logger("PlayerVoiceRecorder");

	[HideInInspector]
	public Player Player;

	[HideInInspector]
	public bool IsEnabled;

	[HideInInspector]
	public bool IsRecording;

	[HideInInspector]
	public uint SampleRate;

	private ConcurrentQueue<float> audioQueue;

	private byte[] levelDecompressBuffer;

	private readonly VoiceLevelMeter transmitMeter = new VoiceLevelMeter();

	public float TransmitLevel { get; private set; }

	public event Action<float> OnTransmitLevelChanged;

	private void Awake()
	{
		Player = GetComponent<Player>();
	}

	private void Update()
	{
		if (Player.IsLocalPlayer && IsRecording)
		{
			Client_SendVoiceData();
		}
	}

	private AudioClip InitializeAudioClip(uint sampleRate)
	{
		audioQueue = new ConcurrentQueue<float>();
		return AudioClip.Create("VoiceData", (int)sampleRate, 1, (int)sampleRate, stream: true, OnAudioRead, null);
	}

	private void OnAudioRead(float[] data)
	{
		for (int i = 0; i < data.Length; i++)
		{
			if (audioQueue != null && audioQueue.TryDequeue(out var result))
			{
				data[i] = result;
			}
			else
			{
				data[i] = 0f;
			}
		}
	}

	private void WriteToClip(byte[] decompressed, int bytesWritten)
	{
		for (int i = 0; i < bytesWritten - 1 && i + 1 < decompressed.Length; i += 2)
		{
			float f = (float)(short)(decompressed[i] | (decompressed[i + 1] << 8)) / 32768f;
			WriteToClip(f);
		}
	}

	private void WriteToClip(float f)
	{
		if (audioQueue != null)
		{
			audioQueue.Enqueue(f);
		}
	}

	private void Client_SendVoiceData()
	{
		SteamUser.GetAvailableVoice(out var pcbCompressed);
		float rawLevel = 0f;
		bool hasVoice = false;
		if (pcbCompressed != 0)
		{
			byte[] array = new byte[pcbCompressed];
			SteamUser.GetVoice(bWantCompressed: true, array, pcbCompressed, out var nBytesWritten);
			if (nBytesWritten != 0)
			{
				Logger.Info($"Sending voice data to server ({array.Length}b)");
				Client_VoiceDataRpc(array);
				if (SampleRate != 0)
				{
					if (levelDecompressBuffer == null)
					{
						levelDecompressBuffer = new byte[SampleRate * 2];
					}
					rawLevel = VoiceUtils.ComputeLevel(array, nBytesWritten, SampleRate, levelDecompressBuffer);
					hasVoice = true;
				}
			}
		}
		SetTransmitLevel(transmitMeter.Update(rawLevel, hasVoice, Time.deltaTime));
	}

	private void SetTransmitLevel(float level)
	{
		TransmitLevel = level;
		OnTransmitLevelChanged?.Invoke(level);
	}

	[Rpc(SendTo.Server, DeferLocal = true)]
	public void Client_RequestVoiceStartRpc(uint sampleRate, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(1666941424u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Reliable);
			BytePacker.WriteValueBitPacked(bufferWriter, sampleRate);
			__endSendRpc(ref bufferWriter, 1666941424u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (rpcParams.Receive.SenderClientId == OwnerClientId && IsEnabled && !IsRecording)
			{
				Server_VoiceStartRpc(sampleRate);
			}
		}
	}

	[Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server, DeferLocal = true)]
	public void Server_VoiceStartRpc(uint sampleRate)
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
			FastBufferWriter bufferWriter = __beginSendRpc(236455107u, rpcParams, attributeParams, SendTo.Everyone, RpcDelivery.Reliable);
			BytePacker.WriteValueBitPacked(bufferWriter, sampleRate);
			__endSendRpc(ref bufferWriter, 236455107u, rpcParams, attributeParams, SendTo.Everyone, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		if (Player.IsMuted.Value)
		{
			Logger.Info($"Player is muted, not starting voice recording ({OwnerClientId})");
			return;
		}
		AudioClip value = InitializeAudioClip(sampleRate);
		if (Player.IsLocalPlayer)
		{
			Logger.Info($"Recording Steam voice at {sampleRate}Hz");
			SteamUser.StartVoiceRecording();
		}
		Logger.Info($"Player started voice recording ({OwnerClientId})");
		IsRecording = true;
		SampleRate = sampleRate;
		EventManager.TriggerEvent("Event_Everyone_OnPlayerVoiceStarted", new Dictionary<string, object>
		{
			{ "player", Player },
			{ "audioClip", value }
		});
	}

	[Rpc(SendTo.Server, DeferLocal = true)]
	public void Client_VoiceDataRpc(byte[] voiceData, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(198182109u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Reliable);
			bool value = voiceData != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(voiceData, default(FastBufferWriter.ForPrimitives));
			}
			__endSendRpc(ref bufferWriter, 198182109u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (!Player.IsMuted.Value && rpcParams.Receive.SenderClientId == OwnerClientId && IsRecording)
			{
				Server_VoiceDataRpc(voiceData);
			}
		}
	}

	[Rpc(SendTo.ClientsAndHost, InvokePermission = RpcInvokePermission.Server, DeferLocal = true)]
	public void Server_VoiceDataRpc(byte[] voiceData)
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
			FastBufferWriter bufferWriter = __beginSendRpc(2054004997u, rpcParams, attributeParams, SendTo.ClientsAndHost, RpcDelivery.Reliable);
			bool value = voiceData != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(voiceData, default(FastBufferWriter.ForPrimitives));
			}
			__endSendRpc(ref bufferWriter, 2054004997u, rpcParams, attributeParams, SendTo.ClientsAndHost, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			byte[] array = new byte[SampleRate * 2];
			SteamUser.DecompressVoice(voiceData, (uint)voiceData.Length, array, (uint)array.Length, out var nBytesWritten, SampleRate);
			if (nBytesWritten != 0)
			{
				WriteToClip(array, (int)nBytesWritten);
			}
		}
	}

	[Rpc(SendTo.Server, DeferLocal = true)]
	public void Client_RequestVoiceStopRpc(RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(2728922553u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Reliable);
			__endSendRpc(ref bufferWriter, 2728922553u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (rpcParams.Receive.SenderClientId == OwnerClientId && IsRecording)
			{
				Server_VoiceStopRpc();
			}
		}
	}

	[Rpc(SendTo.Everyone, InvokePermission = RpcInvokePermission.Server, DeferLocal = true)]
	public void Server_VoiceStopRpc()
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
			FastBufferWriter bufferWriter = __beginSendRpc(824274507u, rpcParams, attributeParams, SendTo.Everyone, RpcDelivery.Reliable);
			__endSendRpc(ref bufferWriter, 824274507u, rpcParams, attributeParams, SendTo.Everyone, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			if (Player.IsLocalPlayer)
			{
				Logger.Info("Stopping Steam voice recording");
				SteamUser.StopVoiceRecording();
			}
			Logger.Info($"Player stopped voice recording ({OwnerClientId})");
			IsRecording = false;
			if (Player.IsLocalPlayer)
			{
				transmitMeter.Reset();
				SetTransmitLevel(0f);
			}
			EventManager.TriggerEvent("Event_Everyone_OnPlayerVoiceStopped", new Dictionary<string, object> { { "player", Player } });
		}
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(1666941424u, __rpc_handler_1666941424, "Client_RequestVoiceStartRpc", RpcInvokePermission.Everyone);
		__registerRpc(236455107u, __rpc_handler_236455107, "Server_VoiceStartRpc", RpcInvokePermission.Server);
		__registerRpc(198182109u, __rpc_handler_198182109, "Client_VoiceDataRpc", RpcInvokePermission.Everyone);
		__registerRpc(2054004997u, __rpc_handler_2054004997, "Server_VoiceDataRpc", RpcInvokePermission.Server);
		__registerRpc(2728922553u, __rpc_handler_2728922553, "Client_RequestVoiceStopRpc", RpcInvokePermission.Everyone);
		__registerRpc(824274507u, __rpc_handler_824274507, "Server_VoiceStopRpc", RpcInvokePermission.Server);
		base.__initializeRpcs();
	}

	private static void __rpc_handler_1666941424(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out uint value);
			RpcParams ext = rpcParams.Ext;
			((PlayerVoiceRecorder)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerVoiceRecorder)target).Client_RequestVoiceStartRpc(value, ext);
			((PlayerVoiceRecorder)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_236455107(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			ByteUnpacker.ReadValueBitPacked(reader, out uint value);
			((PlayerVoiceRecorder)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerVoiceRecorder)target).Server_VoiceStartRpc(value);
			((PlayerVoiceRecorder)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_198182109(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			byte[] value2 = null;
			if (value)
			{
				reader.ReadValueSafe(out value2, default(FastBufferWriter.ForPrimitives));
			}
			RpcParams ext = rpcParams.Ext;
			((PlayerVoiceRecorder)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerVoiceRecorder)target).Client_VoiceDataRpc(value2, ext);
			((PlayerVoiceRecorder)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2054004997(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			byte[] value2 = null;
			if (value)
			{
				reader.ReadValueSafe(out value2, default(FastBufferWriter.ForPrimitives));
			}
			((PlayerVoiceRecorder)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerVoiceRecorder)target).Server_VoiceDataRpc(value2);
			((PlayerVoiceRecorder)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_2728922553(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			RpcParams ext = rpcParams.Ext;
			((PlayerVoiceRecorder)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerVoiceRecorder)target).Client_RequestVoiceStopRpc(ext);
			((PlayerVoiceRecorder)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_824274507(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			((PlayerVoiceRecorder)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((PlayerVoiceRecorder)target).Server_VoiceStopRpc();
			((PlayerVoiceRecorder)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected override string __getTypeName()
	{
		return "PlayerVoiceRecorder";
	}
}
