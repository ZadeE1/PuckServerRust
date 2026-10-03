using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviourSingleton<AudioManager>
{
	[Header("References")]
	[SerializeField]
	private AudioMixer mixer;

	public void SetGlobalVolume(float volume)
	{
		mixer.SetFloat("globalVolume", Mathf.Log(volume + 0.001f) * 20f);
	}

	public void SetAmbientVolume(float volume)
	{
		mixer.SetFloat("ambientVolume", Mathf.Log(volume + 0.001f) * 20f);
	}

	public void SetGameVolume(float volume)
	{
		float value = Mathf.Log(volume + 0.001f) * 20f;
		mixer.SetFloat("gameVolume", value);
		mixer.SetFloat("hornVolume", value);
	}

	public void SetVoiceVolume(float volume)
	{
		mixer.SetFloat("voiceVolume", Mathf.Log(volume + 0.001f) * 20f);
	}

	public void SetUIVolume(float volume)
	{
		mixer.SetFloat("uiVolume", Mathf.Log(volume + 0.001f) * 20f);
	}

	public void SetSlowMotionLowpassCutoff(float cutoffHz)
	{
		mixer.SetFloat("gameLowpassCutoff", cutoffHz);
		mixer.SetFloat("ambientLowpassCutoff", cutoffHz);
	}

	public void SetSlowMotionPitch(float pitch)
	{
		mixer.SetFloat("gamePitch", pitch);
		mixer.SetFloat("ambientPitch", pitch);
	}
}
