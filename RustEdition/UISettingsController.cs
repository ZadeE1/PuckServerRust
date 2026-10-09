using System;
using System.Collections.Generic;
using UnityEngine;

public class UISettingsController : UIViewController<UISettings>
{
	private UISettings uiSettings;

	private MicTester micTester;

	public override void Awake()
	{
		base.Awake();
		uiSettings = GetComponent<UISettings>();
		micTester = gameObject.AddComponent<MicTester>();
		micTester.OnLevelChanged += OnMicTestLevelChanged;
		UISettings uISettings = uiSettings;
		uISettings.OnVisibility = (Action<UIView>)Delegate.Combine(uISettings.OnVisibility, new Action<UIView>(OnSettingsVisibilityChanged));
		UISettings uISettings2 = uiSettings;
		uISettings2.OnActiveTabChanged = (Action)Delegate.Combine(uISettings2.OnActiveTabChanged, new Action(RefreshMicTest));
		EventManager.AddEventListener("Event_OnCameraAngleChanged", Event_OnCameraAngleChanged);
		EventManager.AddEventListener("Event_OnHandednessChanged", Event_OnHandednessChanged);
		EventManager.AddEventListener("Event_OnShowPuckSilhouetteChanged", Event_OnShowPuckSilhouetteChanged);
		EventManager.AddEventListener("Event_OnShowTeamColorBarChanged", Event_OnShowTeamColorBarChanged);
		EventManager.AddEventListener("Event_OnShowPuckOutlineChanged", Event_OnShowPuckOutlineChanged);
		EventManager.AddEventListener("Event_OnShowPuckElevationChanged", Event_OnShowPuckElevationChanged);
		EventManager.AddEventListener("Event_OnShowMinimapSticksChanged", Event_OnShowMinimapSticksChanged);
		EventManager.AddEventListener("Event_OnShowMinimapPuckElevationChanged", Event_OnShowMinimapPuckElevationChanged);
		EventManager.AddEventListener("Event_OnShowMinimapFallenIndicatorChanged", Event_OnShowMinimapFallenIndicatorChanged);
		EventManager.AddEventListener("Event_OnShowPlayerUsernamesChanged", Event_OnShowPlayerUsernamesChanged);
		EventManager.AddEventListener("Event_OnPlayerUsernamesFadeThresholdChanged", Event_OnPlayerUsernamesFadeThresholdChanged);
		EventManager.AddEventListener("Event_OnMaxMatchmakingPingChanged", Event_OnMaxMatchmakingPingChanged);
		EventManager.AddEventListener("Event_OnNetworkBufferingChanged", Event_OnNetworkBufferingChanged);
		EventManager.AddEventListener("Event_OnFilterChatProfanityChanged", Event_OnFilterChatProfanityChanged);
		EventManager.AddEventListener("Event_OnUnitsChanged", Event_OnUnitsChanged);
		EventManager.AddEventListener("Event_OnShowGameUserInterfaceChanged", Event_OnShowGameUserInterfaceChanged);
		EventManager.AddEventListener("Event_OnUserInterfaceScaleChanged", Event_OnUserInterfaceScaleChanged);
		EventManager.AddEventListener("Event_OnChatOpacityChanged", Event_OnChatOpacityChanged);
		EventManager.AddEventListener("Event_OnChatScaleChanged", Event_OnChatScaleChanged);
		EventManager.AddEventListener("Event_OnMinimapOpacityChanged", Event_OnMinimapOpacityChanged);
		EventManager.AddEventListener("Event_OnMinimapBackgroundOpacityChanged", Event_OnMinimapBackgroundOpacityChanged);
		EventManager.AddEventListener("Event_OnMinimapHorizontalPositionChanged", Event_OnMinimapHorizontalPositionChanged);
		EventManager.AddEventListener("Event_OnMinimapVerticalPositionChanged", Event_OnMinimapVerticalPositionChanged);
		EventManager.AddEventListener("Event_OnMinimapScaleChanged", Event_OnMinimapScaleChanged);
		EventManager.AddEventListener("Event_OnGlobalStickSensitivityChanged", Event_OnGlobalStickSensitivityChanged);
		EventManager.AddEventListener("Event_OnHorizontalStickSensitivityChanged", Event_OnHorizontalStickSensitivityChanged);
		EventManager.AddEventListener("Event_OnVerticalStickSensitivityChanged", Event_OnVerticalStickSensitivityChanged);
		EventManager.AddEventListener("Event_OnLookSensitivityChanged", Event_OnLookSensitivityChanged);
		EventManager.AddEventListener("Event_OnKeyBindsLoaded", Event_OnKeyBindsLoaded);
		EventManager.AddEventListener("Event_OnKeyBindsSaved", Event_OnKeyBindsSaved);
		EventManager.AddEventListener("Event_OnGlobalVolumeChanged", Event_OnGlobalVolumeChanged);
		EventManager.AddEventListener("Event_OnAmbientVolumeChanged", Event_OnAmbientVolumeChanged);
		EventManager.AddEventListener("Event_OnGameVolumeChanged", Event_OnGameVolumeChanged);
		EventManager.AddEventListener("Event_OnVoiceVolumeChanged", Event_OnVoiceVolumeChanged);
		EventManager.AddEventListener("Event_OnUIVolumeChanged", Event_OnUIVolumeChanged);
		EventManager.AddEventListener("Event_OnFullScreenModeChanged", Event_OnFullScreenModeChanged);
		EventManager.AddEventListener("Event_OnDisplayIndexChanged", Event_OnDisplayIndexChanged);
		EventManager.AddEventListener("Event_OnResolutionIndexChanged", Event_OnResolutionIndexChanged);
		EventManager.AddEventListener("Event_OnVSyncChanged", Event_OnVSyncChanged);
		EventManager.AddEventListener("Event_OnFpsLimitChanged", Event_OnFpsLimitChanged);
		EventManager.AddEventListener("Event_OnFovChanged", Event_OnFovChanged);
		EventManager.AddEventListener("Event_OnQualityChanged", Event_OnQualityChanged);
		EventManager.AddEventListener("Event_OnShadowQualityChanged", Event_OnShadowQualityChanged);
		EventManager.AddEventListener("Event_OnMotionBlurChanged", Event_OnMotionBlurChanged);
		EventManager.AddEventListener("Event_OnIsDisplayChangeInProgressChanged", Event_OnIsDisplayChangeInProgressChanged);
	}

	public override void OnDestroy()
	{
		micTester.OnLevelChanged -= OnMicTestLevelChanged;
		UISettings uISettings = uiSettings;
		uISettings.OnVisibility = (Action<UIView>)Delegate.Remove(uISettings.OnVisibility, new Action<UIView>(OnSettingsVisibilityChanged));
		UISettings uISettings2 = uiSettings;
		uISettings2.OnActiveTabChanged = (Action)Delegate.Remove(uISettings2.OnActiveTabChanged, new Action(RefreshMicTest));
		EventManager.RemoveEventListener("Event_OnCameraAngleChanged", Event_OnCameraAngleChanged);
		EventManager.RemoveEventListener("Event_OnHandednessChanged", Event_OnHandednessChanged);
		EventManager.RemoveEventListener("Event_OnShowPuckSilhouetteChanged", Event_OnShowPuckSilhouetteChanged);
		EventManager.RemoveEventListener("Event_OnShowTeamColorBarChanged", Event_OnShowTeamColorBarChanged);
		EventManager.RemoveEventListener("Event_OnShowPuckOutlineChanged", Event_OnShowPuckOutlineChanged);
		EventManager.RemoveEventListener("Event_OnShowPuckElevationChanged", Event_OnShowPuckElevationChanged);
		EventManager.RemoveEventListener("Event_OnShowMinimapSticksChanged", Event_OnShowMinimapSticksChanged);
		EventManager.RemoveEventListener("Event_OnShowMinimapPuckElevationChanged", Event_OnShowMinimapPuckElevationChanged);
		EventManager.RemoveEventListener("Event_OnShowMinimapFallenIndicatorChanged", Event_OnShowMinimapFallenIndicatorChanged);
		EventManager.RemoveEventListener("Event_OnShowPlayerUsernamesChanged", Event_OnShowPlayerUsernamesChanged);
		EventManager.RemoveEventListener("Event_OnPlayerUsernamesFadeThresholdChanged", Event_OnPlayerUsernamesFadeThresholdChanged);
		EventManager.RemoveEventListener("Event_OnMaxMatchmakingPingChanged", Event_OnMaxMatchmakingPingChanged);
		EventManager.RemoveEventListener("Event_OnNetworkBufferingChanged", Event_OnNetworkBufferingChanged);
		EventManager.RemoveEventListener("Event_OnFilterChatProfanityChanged", Event_OnFilterChatProfanityChanged);
		EventManager.RemoveEventListener("Event_OnUnitsChanged", Event_OnUnitsChanged);
		EventManager.RemoveEventListener("Event_OnShowGameUserInterfaceChanged", Event_OnShowGameUserInterfaceChanged);
		EventManager.RemoveEventListener("Event_OnUserInterfaceScaleChanged", Event_OnUserInterfaceScaleChanged);
		EventManager.RemoveEventListener("Event_OnChatOpacityChanged", Event_OnChatOpacityChanged);
		EventManager.RemoveEventListener("Event_OnChatScaleChanged", Event_OnChatScaleChanged);
		EventManager.RemoveEventListener("Event_OnMinimapOpacityChanged", Event_OnMinimapOpacityChanged);
		EventManager.RemoveEventListener("Event_OnMinimapBackgroundOpacityChanged", Event_OnMinimapBackgroundOpacityChanged);
		EventManager.RemoveEventListener("Event_OnMinimapHorizontalPositionChanged", Event_OnMinimapHorizontalPositionChanged);
		EventManager.RemoveEventListener("Event_OnMinimapVerticalPositionChanged", Event_OnMinimapVerticalPositionChanged);
		EventManager.RemoveEventListener("Event_OnMinimapScaleChanged", Event_OnMinimapScaleChanged);
		EventManager.RemoveEventListener("Event_OnGlobalStickSensitivityChanged", Event_OnGlobalStickSensitivityChanged);
		EventManager.RemoveEventListener("Event_OnHorizontalStickSensitivityChanged", Event_OnHorizontalStickSensitivityChanged);
		EventManager.RemoveEventListener("Event_OnVerticalStickSensitivityChanged", Event_OnVerticalStickSensitivityChanged);
		EventManager.RemoveEventListener("Event_OnLookSensitivityChanged", Event_OnLookSensitivityChanged);
		EventManager.RemoveEventListener("Event_OnKeyBindsLoaded", Event_OnKeyBindsLoaded);
		EventManager.RemoveEventListener("Event_OnKeyBindsSaved", Event_OnKeyBindsSaved);
		EventManager.RemoveEventListener("Event_OnGlobalVolumeChanged", Event_OnGlobalVolumeChanged);
		EventManager.RemoveEventListener("Event_OnAmbientVolumeChanged", Event_OnAmbientVolumeChanged);
		EventManager.RemoveEventListener("Event_OnGameVolumeChanged", Event_OnGameVolumeChanged);
		EventManager.RemoveEventListener("Event_OnVoiceVolumeChanged", Event_OnVoiceVolumeChanged);
		EventManager.RemoveEventListener("Event_OnUIVolumeChanged", Event_OnUIVolumeChanged);
		EventManager.RemoveEventListener("Event_OnFullScreenModeChanged", Event_OnFullScreenModeChanged);
		EventManager.RemoveEventListener("Event_OnDisplayIndexChanged", Event_OnDisplayIndexChanged);
		EventManager.RemoveEventListener("Event_OnResolutionIndexChanged", Event_OnResolutionIndexChanged);
		EventManager.RemoveEventListener("Event_OnVSyncChanged", Event_OnVSyncChanged);
		EventManager.RemoveEventListener("Event_OnFpsLimitChanged", Event_OnFpsLimitChanged);
		EventManager.RemoveEventListener("Event_OnFovChanged", Event_OnFovChanged);
		EventManager.RemoveEventListener("Event_OnQualityChanged", Event_OnQualityChanged);
		EventManager.RemoveEventListener("Event_OnShadowQualityChanged", Event_OnShadowQualityChanged);
		EventManager.RemoveEventListener("Event_OnMotionBlurChanged", Event_OnMotionBlurChanged);
		EventManager.RemoveEventListener("Event_OnIsDisplayChangeInProgressChanged", Event_OnIsDisplayChangeInProgressChanged);
		base.OnDestroy();
	}

	private void Event_OnCameraAngleChanged(Dictionary<string, object> message)
	{
		float value = (float)message["value"];
		uiSettings.UpdateCameraAngle(value);
	}

	private void Event_OnHandednessChanged(Dictionary<string, object> message)
	{
		string nameFromHandedness = Utils.GetNameFromHandedness((PlayerHandedness)message["value"]);
		uiSettings.UpdateHandedness(nameFromHandedness);
	}

	private void Event_OnShowPuckSilhouetteChanged(Dictionary<string, object> message)
	{
		bool value = (bool)message["value"];
		uiSettings.UpdateShowPuckSilhouette(value);
	}

	private void Event_OnShowTeamColorBarChanged(Dictionary<string, object> message)
	{
		bool value = (bool)message["value"];
		uiSettings.UpdateShowTeamColorBar(value);
	}

	private void Event_OnShowPuckOutlineChanged(Dictionary<string, object> message)
	{
		bool value = (bool)message["value"];
		uiSettings.UpdateShowPuckOutline(value);
	}

	private void Event_OnShowPuckElevationChanged(Dictionary<string, object> message)
	{
		bool value = (bool)message["value"];
		uiSettings.UpdateShowPuckElevation(value);
	}

	private void Event_OnShowMinimapSticksChanged(Dictionary<string, object> message)
	{
		bool value = (bool)message["value"];
		uiSettings.UpdateShowMinimapSticks(value);
	}

	private void Event_OnShowMinimapPuckElevationChanged(Dictionary<string, object> message)
	{
		bool value = (bool)message["value"];
		uiSettings.UpdateShowMinimapPuckElevation(value);
	}

	private void Event_OnShowMinimapFallenIndicatorChanged(Dictionary<string, object> message)
	{
		bool value = (bool)message["value"];
		uiSettings.UpdateShowMinimapFallenIndicator(value);
	}

	private void Event_OnShowPlayerUsernamesChanged(Dictionary<string, object> message)
	{
		bool value = (bool)message["value"];
		uiSettings.UpdateShowPlayerUsernames(value);
	}

	private void Event_OnPlayerUsernamesFadeThresholdChanged(Dictionary<string, object> message)
	{
		float value = (float)message["value"];
		uiSettings.UpdatePlayerUsernamesFadeThreshold(value);
	}

	private void Event_OnMaxMatchmakingPingChanged(Dictionary<string, object> message)
	{
		int value = (int)message["value"];
		uiSettings.UpdateMaxMatchmakingPing(value);
	}

	private void Event_OnNetworkBufferingChanged(Dictionary<string, object> message)
	{
		string nameFromNetworkBuffering = Utils.GetNameFromNetworkBuffering((NetworkBuffering)message["value"]);
		uiSettings.UpdateNetworkBuffering(nameFromNetworkBuffering);
	}

	private void Event_OnFilterChatProfanityChanged(Dictionary<string, object> message)
	{
		bool value = (bool)message["value"];
		uiSettings.UpdateFilterChatProfanity(value);
	}

	private void Event_OnUnitsChanged(Dictionary<string, object> message)
	{
		string nameFromUnits = Utils.GetNameFromUnits((Units)message["value"]);
		uiSettings.UpdateUnits(nameFromUnits);
	}

	private void Event_OnShowGameUserInterfaceChanged(Dictionary<string, object> message)
	{
		bool value = (bool)message["value"];
		uiSettings.UpdateShowGameUserInterface(value);
	}

	private void Event_OnUserInterfaceScaleChanged(Dictionary<string, object> message)
	{
		float value = (float)message["value"];
		uiSettings.UpdateUserInterfaceScale(value);
	}

	private void Event_OnChatOpacityChanged(Dictionary<string, object> message)
	{
		float value = (float)message["value"];
		uiSettings.UpdateChatOpacity(value);
	}

	private void Event_OnChatScaleChanged(Dictionary<string, object> message)
	{
		float value = (float)message["value"];
		uiSettings.UpdateChatScale(value);
	}

	private void Event_OnMinimapOpacityChanged(Dictionary<string, object> message)
	{
		float value = (float)message["value"];
		uiSettings.UpdateMinimapOpacity(value);
	}

	private void Event_OnMinimapBackgroundOpacityChanged(Dictionary<string, object> message)
	{
		float value = (float)message["value"];
		uiSettings.UpdateMinimapBackgroundOpacity(value);
	}

	private void Event_OnMinimapHorizontalPositionChanged(Dictionary<string, object> message)
	{
		float value = (float)message["value"];
		uiSettings.UpdateMinimapHorizontalPosition(value);
	}

	private void Event_OnMinimapVerticalPositionChanged(Dictionary<string, object> message)
	{
		float value = (float)message["value"];
		uiSettings.UpdateMinimapVerticalPosition(value);
	}

	private void Event_OnMinimapScaleChanged(Dictionary<string, object> message)
	{
		float value = (float)message["value"];
		uiSettings.UpdateMinimapScale(value);
	}

	private void Event_OnGlobalStickSensitivityChanged(Dictionary<string, object> message)
	{
		float value = (float)message["value"];
		uiSettings.UpdateGlobalStickSensitivity(value);
	}

	private void Event_OnHorizontalStickSensitivityChanged(Dictionary<string, object> message)
	{
		float value = (float)message["value"];
		uiSettings.UpdateHorizontalStickSensitivity(value);
	}

	private void Event_OnVerticalStickSensitivityChanged(Dictionary<string, object> message)
	{
		float value = (float)message["value"];
		uiSettings.UpdateVerticalStickSensitivity(value);
	}

	private void Event_OnLookSensitivityChanged(Dictionary<string, object> message)
	{
		float value = (float)message["value"];
		uiSettings.UpdateLookSensitivity(value);
	}

	private void Event_OnKeyBindsLoaded(Dictionary<string, object> message)
	{
		Dictionary<string, KeyBind> keyBinds = (Dictionary<string, KeyBind>)message["keyBinds"];
		uiSettings.UpdateKeyBindInputs(keyBinds);
	}

	private void Event_OnKeyBindsSaved(Dictionary<string, object> message)
	{
		Dictionary<string, KeyBind> keyBinds = (Dictionary<string, KeyBind>)message["keyBinds"];
		uiSettings.UpdateKeyBindInputs(keyBinds);
	}

	private void Event_OnGlobalVolumeChanged(Dictionary<string, object> message)
	{
		float value = (float)message["value"];
		uiSettings.UpdateGlobalVolume(value);
	}

	private void Event_OnAmbientVolumeChanged(Dictionary<string, object> message)
	{
		float value = (float)message["value"];
		uiSettings.UpdateAmbientVolume(value);
	}

	private void Event_OnGameVolumeChanged(Dictionary<string, object> message)
	{
		float value = (float)message["value"];
		uiSettings.UpdateGameVolume(value);
	}

	private void Event_OnVoiceVolumeChanged(Dictionary<string, object> message)
	{
		float value = (float)message["value"];
		uiSettings.UpdateVoiceVolume(value);
	}

	private void Event_OnUIVolumeChanged(Dictionary<string, object> message)
	{
		float value = (float)message["value"];
		uiSettings.UpdateUIVolume(value);
	}

	private void OnSettingsVisibilityChanged(UIView view)
	{
		RefreshMicTest();
	}

	private void RefreshMicTest()
	{
		micTester.SetActive(uiSettings.IsVisible && uiSettings.IsAudioTabActive);
	}

	private void OnMicTestLevelChanged(float level)
	{
		uiSettings.SetMicTestLevel(level);
	}

	private void Event_OnFullScreenModeChanged(Dictionary<string, object> message)
	{
		string nameFromFullScreenMode = Utils.GetNameFromFullScreenMode((FullScreenMode)message["value"]);
		uiSettings.UpdateFullScreenMode(nameFromFullScreenMode);
	}

	private void Event_OnDisplayIndexChanged(Dictionary<string, object> message)
	{
		string displayNameFromIndex = Utils.GetDisplayNameFromIndex((int)message["value"]);
		uiSettings.UpdateDisplay(displayNameFromIndex);
	}

	private void Event_OnResolutionIndexChanged(Dictionary<string, object> message)
	{
		string resolutionNameFromIndex = Utils.GetResolutionNameFromIndex((int)message["value"]);
		uiSettings.UpdateResolution(resolutionNameFromIndex);
	}

	private void Event_OnVSyncChanged(Dictionary<string, object> message)
	{
		bool value = (bool)message["value"];
		uiSettings.UpdateVSync(value);
	}

	private void Event_OnFpsLimitChanged(Dictionary<string, object> message)
	{
		int value = (int)message["value"];
		uiSettings.UpdateFpsLimit(value);
	}

	private void Event_OnFovChanged(Dictionary<string, object> message)
	{
		float value = (float)message["value"];
		uiSettings.UpdateFov(value);
	}

	private void Event_OnQualityChanged(Dictionary<string, object> message)
	{
		string nameFromApplicationQuality = Utils.GetNameFromApplicationQuality((ApplicationQuality)message["value"]);
		uiSettings.UpdateQuality(nameFromApplicationQuality);
	}

	private void Event_OnShadowQualityChanged(Dictionary<string, object> message)
	{
		string nameFromShadowQuality = Utils.GetNameFromShadowQuality((ShadowQuality)message["value"]);
		uiSettings.UpdateShadowQuality(nameFromShadowQuality);
	}

	private void Event_OnMotionBlurChanged(Dictionary<string, object> message)
	{
		bool value = (bool)message["value"];
		uiSettings.UpdateMotionBlur(value);
	}

	private void Event_OnIsDisplayChangeInProgressChanged(Dictionary<string, object> message)
	{
		if (!(bool)message["isDisplayChangeInProgress"])
		{
			List<string> resolutionNames = Utils.GetResolutionNames();
			uiSettings.UpdateResolutionChoices(resolutionNames);
		}
	}
}
