using System.Collections.Generic;
using UnityEngine;

public class AudioManagerController : MonoBehaviour
{
	private AudioManager audioManager;

	private void Awake()
	{
		audioManager = GetComponent<AudioManager>();
		EventManager.AddEventListener("Event_OnGlobalVolumeChanged", Event_OnGlobalVolumeChanged);
		EventManager.AddEventListener("Event_OnAmbientVolumeChanged", Event_OnAmbientVolumeChanged);
		EventManager.AddEventListener("Event_OnGameVolumeChanged", Event_OnGameVolumeChanged);
		EventManager.AddEventListener("Event_OnVoiceVolumeChanged", Event_OnVoiceVolumeChanged);
		EventManager.AddEventListener("Event_OnUIVolumeChanged", Event_OnUIVolumeChanged);
	}

	private void Start()
	{
		audioManager.SetGlobalVolume(SettingsManager.GlobalVolume);
		audioManager.SetAmbientVolume(SettingsManager.AmbientVolume);
		audioManager.SetGameVolume(SettingsManager.GameVolume);
		audioManager.SetVoiceVolume(SettingsManager.VoiceVolume);
		audioManager.SetUIVolume(SettingsManager.UIVolume);
	}

	private void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_OnGlobalVolumeChanged", Event_OnGlobalVolumeChanged);
		EventManager.RemoveEventListener("Event_OnAmbientVolumeChanged", Event_OnAmbientVolumeChanged);
		EventManager.RemoveEventListener("Event_OnGameVolumeChanged", Event_OnGameVolumeChanged);
		EventManager.RemoveEventListener("Event_OnVoiceVolumeChanged", Event_OnVoiceVolumeChanged);
		EventManager.RemoveEventListener("Event_OnUIVolumeChanged", Event_OnUIVolumeChanged);
	}

	private void Event_OnGlobalVolumeChanged(Dictionary<string, object> eventParams)
	{
		float globalVolume = (float)eventParams["value"];
		audioManager.SetGlobalVolume(globalVolume);
	}

	private void Event_OnAmbientVolumeChanged(Dictionary<string, object> eventParams)
	{
		float ambientVolume = (float)eventParams["value"];
		audioManager.SetAmbientVolume(ambientVolume);
	}

	private void Event_OnGameVolumeChanged(Dictionary<string, object> eventParams)
	{
		float gameVolume = (float)eventParams["value"];
		audioManager.SetGameVolume(gameVolume);
	}

	private void Event_OnVoiceVolumeChanged(Dictionary<string, object> eventParams)
	{
		float voiceVolume = (float)eventParams["value"];
		audioManager.SetVoiceVolume(voiceVolume);
	}

	private void Event_OnUIVolumeChanged(Dictionary<string, object> eventParams)
	{
		float uIVolume = (float)eventParams["value"];
		audioManager.SetUIVolume(uIVolume);
	}
}
