using System;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SynchronizedAudio : NetworkBehaviour
{
	[Header("Settings")]
	[SerializeField]
	private bool stopOnDespawn = true;

	[Header("References")]
	[SerializeField]
	private List<AudioClip> audioClips = new List<AudioClip>();

	private NetworkVariable<byte> Volume;

	private NetworkVariable<byte> Pitch;

	private bool isNetworkVariablesInitialized;

	private AudioSource audioSource;

	private int clipIndex;

	private float time;

	private bool fadeIn;

	private float fadeInDuration;

	private bool fadeOut;

	private float fadeOutDuration;

	private float duration;

	private bool isPlaying;

	private float initialVolume;

	private float initialPitch;

	private Sequence audioSourceSequence;

	private void Awake()
	{
		audioSource = GetComponent<AudioSource>();
		initialVolume = audioSource.volume;
		initialPitch = audioSource.pitch;
	}

	protected override void OnNetworkPreSpawn(ref NetworkManager networkManager)
	{
		InitializeNetworkVariables(NetworkingUtils.CompressFloatToByte(initialVolume, 0f, 1f), NetworkingUtils.CompressFloatToByte(initialPitch, 0f, 1f));
		base.OnNetworkPreSpawn(ref networkManager);
	}

	public override void OnNetworkSpawn()
	{
		NetworkVariable<byte> volume = Volume;
		volume.OnValueChanged = (NetworkVariable<byte>.OnValueChangedDelegate)Delegate.Combine(volume.OnValueChanged, new NetworkVariable<byte>.OnValueChangedDelegate(OnVolumeChanged));
		NetworkVariable<byte> pitch = Pitch;
		pitch.OnValueChanged = (NetworkVariable<byte>.OnValueChangedDelegate)Delegate.Combine(pitch.OnValueChanged, new NetworkVariable<byte>.OnValueChangedDelegate(OnPitchChanged));
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
		NetworkVariable<byte> volume = Volume;
		volume.OnValueChanged = (NetworkVariable<byte>.OnValueChangedDelegate)Delegate.Remove(volume.OnValueChanged, new NetworkVariable<byte>.OnValueChangedDelegate(OnVolumeChanged));
		NetworkVariable<byte> pitch = Pitch;
		pitch.OnValueChanged = (NetworkVariable<byte>.OnValueChangedDelegate)Delegate.Remove(pitch.OnValueChanged, new NetworkVariable<byte>.OnValueChangedDelegate(OnPitchChanged));
		if (stopOnDespawn && audioSource != null)
		{
			audioSource.Stop();
			if (audioSourceSequence != null)
			{
				audioSourceSequence.Kill();
			}
		}
		base.OnNetworkDespawn();
	}

	public override void OnDestroy()
	{
		this.DOKill();
	}

	public void InitializeNetworkVariables(byte volume = 0, byte pitch = 0)
	{
		if (!isNetworkVariablesInitialized)
		{
			isNetworkVariablesInitialized = true;
			Volume = new NetworkVariable<byte>(volume);
			Pitch = new NetworkVariable<byte>(pitch);
		}
	}

	private void ProcessInitialNetworkVariableValues()
	{
		OnVolumeChanged(0, Volume.Value);
		OnPitchChanged(0, Pitch.Value);
	}

	private void Update()
	{
		if (IsSpawned && NetworkManager.Singleton.IsServer)
		{
			if (isPlaying && !audioSource.isPlaying)
			{
				isPlaying = false;
			}
			if (isPlaying)
			{
				time += Time.deltaTime;
			}
		}
	}

	private void OnVolumeChanged(byte oldVolume, byte newVolume)
	{
		audioSource.volume = NetworkingUtils.DecompressByteToFloat(newVolume, 0f, 1f);
	}

	private void OnPitchChanged(byte oldPitch, byte newPitch)
	{
		audioSource.pitch = NetworkingUtils.DecompressByteToFloat(newPitch, 0f, 1f);
	}

	public void Server_SetVolume(float volume)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Volume.Value = NetworkingUtils.CompressFloatToByte(volume, 0f, 1f);
		}
	}

	public void Server_SetPitch(float pitch)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Pitch.Value = NetworkingUtils.CompressFloatToByte(pitch, 0f, 1f);
		}
	}

	public void Server_Play(float volume = -1f, float pitch = -1f, bool isOneShot = false, int clipIndex = -1, float time = 0f, bool randomClip = false, bool randomTime = false, bool fadeIn = false, float fadeInDuration = 0f, bool fadeOut = false, float fadeOutDuration = 0f, float duration = -1f)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			volume = ((volume == -1f) ? initialVolume : volume);
			pitch = ((pitch == -1f) ? initialPitch : pitch);
			if (randomClip && audioClips.Count > 0)
			{
				clipIndex = UnityEngine.Random.Range(0, audioClips.Count);
			}
			float num = ((clipIndex == -1) ? audioSource.clip.length : audioClips[clipIndex].length);
			if (randomTime)
			{
				time = UnityEngine.Random.Range(0f, Mathf.Max(num - duration - fadeInDuration - fadeOutDuration, 0f));
			}
			this.duration = ((duration == -1f) ? num : duration);
			if (!isOneShot)
			{
				Volume.Value = NetworkingUtils.CompressFloatToByte(volume, 0f, 1f);
				Pitch.Value = NetworkingUtils.CompressFloatToByte(pitch, 0f, 1f);
				this.clipIndex = clipIndex;
				this.time = time;
				this.fadeIn = fadeIn;
				this.fadeInDuration = fadeInDuration;
				this.fadeOut = fadeOut;
				this.fadeOutDuration = fadeOutDuration;
				isPlaying = true;
			}
			Server_PlayRpc(volume, pitch, isOneShot, clipIndex, time, fadeIn, fadeInDuration, fadeOut, fadeOutDuration, duration, RpcTarget.Everyone);
		}
	}

	[Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server, DeferLocal = true)]
	private void Server_PlayRpc(float volume, float pitch, bool isOneShot, int clipIndex, float time, bool fadeIn, float fadeInDuration, bool fadeOut, float fadeOutDuration, float duration, RpcParams rpcParams = default(RpcParams))
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
			FastBufferWriter bufferWriter = __beginSendRpc(408477299u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
			bufferWriter.WriteValueSafe(in volume, default(FastBufferWriter.ForPrimitives));
			bufferWriter.WriteValueSafe(in pitch, default(FastBufferWriter.ForPrimitives));
			bufferWriter.WriteValueSafe(in isOneShot, default(FastBufferWriter.ForPrimitives));
			BytePacker.WriteValueBitPacked(bufferWriter, clipIndex);
			bufferWriter.WriteValueSafe(in time, default(FastBufferWriter.ForPrimitives));
			bufferWriter.WriteValueSafe(in fadeIn, default(FastBufferWriter.ForPrimitives));
			bufferWriter.WriteValueSafe(in fadeInDuration, default(FastBufferWriter.ForPrimitives));
			bufferWriter.WriteValueSafe(in fadeOut, default(FastBufferWriter.ForPrimitives));
			bufferWriter.WriteValueSafe(in fadeOutDuration, default(FastBufferWriter.ForPrimitives));
			bufferWriter.WriteValueSafe(in duration, default(FastBufferWriter.ForPrimitives));
			__endSendRpc(ref bufferWriter, 408477299u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		audioSource.volume = volume;
		audioSource.pitch = pitch;
		if (clipIndex != -1 && audioClips.Count > 0)
		{
			audioSource.clip = audioClips[clipIndex];
		}
		if (isOneShot)
		{
			audioSource.PlayOneShot(audioSource.clip);
			return;
		}
		audioSourceSequence = DOTween.Sequence(this);
		if (fadeIn)
		{
			audioSource.volume = 0f;
			audioSourceSequence.Append(DOTween.To(() => audioSource.volume, (float x) =>
			{
				audioSource.volume = x;
			}, volume, fadeInDuration)).SetEase(Ease.Linear);
		}
		audioSourceSequence.AppendInterval(duration - fadeInDuration - fadeOutDuration - time);
		if (fadeOut)
		{
			audioSourceSequence.Append(DOTween.To(() => audioSource.volume, (float x) =>
			{
				audioSource.volume = x;
			}, 0f, fadeOutDuration).OnComplete(() =>
			{
				audioSource.Stop();
			})).SetEase(Ease.Linear);
		}
		audioSource.Play();
		audioSource.time = time;
	}

	public void Server_ForceSynchronizeClientId(ulong clientId)
	{
		if (NetworkManager.Singleton.IsServer && isPlaying)
		{
			Server_PlayRpc(NetworkingUtils.DecompressByteToFloat(Volume.Value, 0f, 1f), NetworkingUtils.DecompressByteToFloat(Pitch.Value, 0f, 1f), isOneShot: false, clipIndex, time, fadeIn: false, fadeInDuration, fadeOut, fadeOutDuration, duration, RpcTarget.Single(clientId, RpcTargetUse.Temp));
		}
	}

	protected override void __initializeVariables()
	{
		if (Volume == null)
		{
			throw new Exception("SynchronizedAudio.Volume cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		Volume.Initialize(this);
		__nameNetworkVariable(Volume, "Volume");
		NetworkVariableFields.Add(Volume);
		if (Pitch == null)
		{
			throw new Exception("SynchronizedAudio.Pitch cannot be null. All NetworkVariableBase instances must be initialized.");
		}
		Pitch.Initialize(this);
		__nameNetworkVariable(Pitch, "Pitch");
		NetworkVariableFields.Add(Pitch);
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(408477299u, __rpc_handler_408477299, "Server_PlayRpc", RpcInvokePermission.Server);
		base.__initializeRpcs();
	}

	private static void __rpc_handler_408477299(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out float value, default(FastBufferWriter.ForPrimitives));
			reader.ReadValueSafe(out float value2, default(FastBufferWriter.ForPrimitives));
			reader.ReadValueSafe(out bool value3, default(FastBufferWriter.ForPrimitives));
			ByteUnpacker.ReadValueBitPacked(reader, out int value4);
			reader.ReadValueSafe(out float value5, default(FastBufferWriter.ForPrimitives));
			reader.ReadValueSafe(out bool value6, default(FastBufferWriter.ForPrimitives));
			reader.ReadValueSafe(out float value7, default(FastBufferWriter.ForPrimitives));
			reader.ReadValueSafe(out bool value8, default(FastBufferWriter.ForPrimitives));
			reader.ReadValueSafe(out float value9, default(FastBufferWriter.ForPrimitives));
			reader.ReadValueSafe(out float value10, default(FastBufferWriter.ForPrimitives));
			RpcParams ext = rpcParams.Ext;
			((SynchronizedAudio)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((SynchronizedAudio)target).Server_PlayRpc(value, value2, value3, value4, value5, value6, value7, value8, value9, value10, ext);
			((SynchronizedAudio)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected override string __getTypeName()
	{
		return "SynchronizedAudio";
	}
}
