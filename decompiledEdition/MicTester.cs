using System;
using Steamworks;
using UnityEngine;

public class MicTester : MonoBehaviour
{
	private uint sampleRate;

	private byte[] decompressBuffer;

	private bool ownsRecording;

	private float level;

	private readonly VoiceLevelMeter meter = new VoiceLevelMeter();

	public bool IsActive { get; private set; }

	public event Action<float> OnLevelChanged;

	private void Update()
	{
		if (!IsActive)
		{
			return;
		}
		PlayerVoiceRecorder localPlayerRecorder = GetLocalPlayerRecorder();
		if (localPlayerRecorder != null && localPlayerRecorder.IsRecording)
		{
			ReleaseRecording(playerStillRecording: true);
			meter.Reset();
			SetLevel(localPlayerRecorder.TransmitLevel);
			return;
		}
		if (!ownsRecording)
		{
			SteamUser.StartVoiceRecording();
			ownsRecording = true;
		}
		float rawLevel = Capture(out var hasVoice);
		SetLevel(meter.Update(rawLevel, hasVoice, Time.deltaTime));
	}

	private void OnDestroy()
	{
		Stop();
	}

	public void SetActive(bool value)
	{
		if (value)
		{
			Begin();
		}
		else
		{
			Stop();
		}
	}

	private void Begin()
	{
		if (!IsActive && SteamManager.IsInitialized)
		{
			IsActive = true;
			sampleRate = SteamUser.GetVoiceOptimalSampleRate();
			if (sampleRate != 0)
			{
				decompressBuffer = new byte[sampleRate * 2];
			}
		}
	}

	private void Stop()
	{
		if (IsActive)
		{
			IsActive = false;
			ReleaseRecording(playerStillRecording: false);
			meter.Reset();
			SetLevel(0f);
		}
	}

	private float Capture(out bool hasVoice)
	{
		hasVoice = false;
		SteamUser.GetAvailableVoice(out var pcbCompressed);
		if (pcbCompressed == 0)
		{
			return 0f;
		}
		byte[] array = new byte[pcbCompressed];
		SteamUser.GetVoice(bWantCompressed: true, array, pcbCompressed, out var nBytesWritten);
		if (nBytesWritten == 0)
		{
			return 0f;
		}
		hasVoice = true;
		return VoiceUtils.ComputeLevel(array, nBytesWritten, sampleRate, decompressBuffer);
	}

	private void ReleaseRecording(bool playerStillRecording)
	{
		if (ownsRecording && !playerStillRecording)
		{
			SteamUser.StopVoiceRecording();
		}
		ownsRecording = false;
	}

	private void SetLevel(float value)
	{
		level = value;
		OnLevelChanged?.Invoke(value);
	}

	private PlayerVoiceRecorder GetLocalPlayerRecorder()
	{
		Player player = ((MonoBehaviourSingleton<PlayerManager>.Instance != null) ? MonoBehaviourSingleton<PlayerManager>.Instance.GetLocalPlayer() : null);
		if (!(player != null))
		{
			return null;
		}
		return player.GetComponent<PlayerVoiceRecorder>();
	}
}
