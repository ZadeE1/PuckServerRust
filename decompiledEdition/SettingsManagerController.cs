using System.Collections.Generic;
using UnityEngine.InputSystem;

public static class SettingsManagerController
{
	private static readonly Logger Logger = new Logger("SettingsManagerController");

	public static void Initialize()
	{
		EventManager.AddEventListener("Event_OnDisplayIndexChanged", Event_OnDisplayIndexChanged);
		EventManager.AddEventListener("Event_OnIsDisplayChangeInProgressChanged", Event_OnIsDisplayChangeInProgressChanged);
		EventManager.AddEventListener("Event_OnBaseCameraEnabled", Event_OnBaseCameraEnabled);
		EventManager.AddEventListener("Event_OnPopupClickOk", Event_OnPopupClickOk);
		AddSettingsEventListeners();
		AddAppearanceEventListeners();
		InputManager.Debug1Action.performed += OnDebug1ActionPerformed;
	}

	public static void Dispose()
	{
		EventManager.RemoveEventListener("Event_OnDisplayIndexChanged", Event_OnDisplayIndexChanged);
		EventManager.RemoveEventListener("Event_OnIsDisplayChangeInProgressChanged", Event_OnIsDisplayChangeInProgressChanged);
		EventManager.RemoveEventListener("Event_OnBaseCameraEnabled", Event_OnBaseCameraEnabled);
		EventManager.RemoveEventListener("Event_OnPopupClickOk", Event_OnPopupClickOk);
		RemoveSettingsEventListeners();
		RemoveAppearanceEventListeners();
		InputManager.Debug1Action.performed -= OnDebug1ActionPerformed;
	}

	private static void AddSettingsEventListeners()
	{
		EventManager.AddEventListener("Event_OnSettingsCameraAngleChanged", Event_OnSettingsCameraAngleChanged);
		EventManager.AddEventListener("Event_OnSettingsHandednessChanged", Event_OnSettingsHandednessChanged);
		EventManager.AddEventListener("Event_OnSettingsShowPuckSilhouetteChanged", Event_OnSettingsShowPuckSilhouetteChanged);
		EventManager.AddEventListener("Event_OnSettingsShowTeamColorBarChanged", Event_OnSettingsShowTeamColorBarChanged);
		EventManager.AddEventListener("Event_OnSettingsShowPuckOutlineChanged", Event_OnSettingsShowPuckOutlineChanged);
		EventManager.AddEventListener("Event_OnSettingsShowPuckElevationChanged", Event_OnSettingsShowPuckElevationChanged);
		EventManager.AddEventListener("Event_OnSettingsShowMinimapSticksChanged", Event_OnSettingsShowMinimapSticksChanged);
		EventManager.AddEventListener("Event_OnSettingsShowMinimapPuckElevationChanged", Event_OnSettingsShowMinimapPuckElevationChanged);
		EventManager.AddEventListener("Event_OnSettingsShowMinimapFallenIndicatorChanged", Event_OnSettingsShowMinimapFallenIndicatorChanged);
		EventManager.AddEventListener("Event_OnSettingsShowPlayerUsernamesChanged", Event_OnSettingsShowPlayerUsernamesChanged);
		EventManager.AddEventListener("Event_OnSettingsPlayerUsernamesFadeThresholdChanged", Event_OnSettingsPlayerUsernamesFadeThresholdChanged);
		EventManager.AddEventListener("Event_OnSettingsMaxMatchmakingPingChanged", Event_OnSettingsMaxMatchmakingPingChanged);
		EventManager.AddEventListener("Event_OnSettingsNetworkBufferingChanged", Event_OnSettingsNetworkBufferingChanged);
		EventManager.AddEventListener("Event_OnSettingsFilterChatProfanityChanged", Event_OnSettingsFilterChatProfanityChanged);
		EventManager.AddEventListener("Event_OnSettingsUnitsChanged", Event_OnSettingsUnitsChanged);
		EventManager.AddEventListener("Event_OnSettingsShowGameUserInterfaceChanged", Event_OnSettingsShowGameUserInterfaceChanged);
		EventManager.AddEventListener("Event_OnSettingsUserInterfaceScaleChanged", Event_OnSettingsUserInterfaceScaleChanged);
		EventManager.AddEventListener("Event_OnSettingsChatOpacityChanged", Event_OnSettingsChatOpacityChanged);
		EventManager.AddEventListener("Event_OnSettingsChatScaleChanged", Event_OnSettingsChatScaleChanged);
		EventManager.AddEventListener("Event_OnSettingsMinimapOpacityChanged", Event_OnSettingsMinimapOpacityChanged);
		EventManager.AddEventListener("Event_OnSettingsMinimapBackgroundOpacityChanged", Event_OnSettingsMinimapBackgroundOpacityChanged);
		EventManager.AddEventListener("Event_OnSettingsMinimapHorizontalPositionChanged", Event_OnSettingsMinimapHorizontalPositionChanged);
		EventManager.AddEventListener("Event_OnSettingsMinimapVerticalPositionChanged", Event_OnSettingsMinimapVerticalPositionChanged);
		EventManager.AddEventListener("Event_OnSettingsMinimapScaleChanged", Event_OnSettingsMinimapScaleChanged);
		EventManager.AddEventListener("Event_OnSettingsGlobalStickSensitivityChanged", Event_OnSettingsGlobalStickSensitivityChanged);
		EventManager.AddEventListener("Event_OnSettingsHorizontalStickSensitivityChanged", Event_OnSettingsHorizontalStickSensitivityChanged);
		EventManager.AddEventListener("Event_OnSettingsVerticalStickSensitivityChanged", Event_OnSettingsVerticalStickSensitivityChanged);
		EventManager.AddEventListener("Event_OnSettingsLookSensitivityChanged", Event_OnSettingsLookSensitivityChanged);
		EventManager.AddEventListener("Event_OnSettingsGlobalVolumeChanged", Event_OnSettingsGlobalVolumeChanged);
		EventManager.AddEventListener("Event_OnSettingsAmbientVolumeChanged", Event_OnSettingsAmbientVolumeChanged);
		EventManager.AddEventListener("Event_OnSettingsGameVolumeChanged", Event_OnSettingsGameVolumeChanged);
		EventManager.AddEventListener("Event_OnSettingsVoiceVolumeChanged", Event_OnSettingsVoiceVolumeChanged);
		EventManager.AddEventListener("Event_OnSettingsUIVolumeChanged", Event_OnSettingsUIVolumeChanged);
		EventManager.AddEventListener("Event_OnSettingsFullScreenModeChanged", Event_OnSettingsFullScreenModeChanged);
		EventManager.AddEventListener("Event_OnSettingsDisplayChanged", Event_OnSettingsDisplayChanged);
		EventManager.AddEventListener("Event_OnSettingsResolutionChanged", Event_OnSettingsResolutionChanged);
		EventManager.AddEventListener("Event_OnSettingsVSyncChanged", Event_OnSettingsVSyncChanged);
		EventManager.AddEventListener("Event_OnSettingsFpsLimitChanged", Event_OnSettingsFpsLimitChanged);
		EventManager.AddEventListener("Event_OnSettingsFovChanged", Event_OnSettingsFovChanged);
		EventManager.AddEventListener("Event_OnSettingsQualityChanged", Event_OnSettingsQualityChanged);
		EventManager.AddEventListener("Event_OnSettingsShadowQualityChanged", Event_OnSettingsShadowQualityChanged);
		EventManager.AddEventListener("Event_OnSettingsMotionBlurChanged", Event_OnSettingsMotionBlurChanged);
	}

	private static void AddAppearanceEventListeners()
	{
		EventManager.AddEventListener("Event_OnAppearanceTeamChanged", Event_OnAppearanceTeamChanged);
		EventManager.AddEventListener("Event_OnAppearanceRoleChanged", Event_OnAppearanceRoleChanged);
		EventManager.AddEventListener("Event_OnAppearanceApplyForBothTeamsChanged", Event_OnAppearanceApplyForBothTeamsChanged);
		EventManager.AddEventListener("Event_OnAppearanceClickItem", Event_OnAppearanceClickItem);
	}

	private static void RemoveSettingsEventListeners()
	{
		EventManager.RemoveEventListener("Event_OnSettingsCameraAngleChanged", Event_OnSettingsCameraAngleChanged);
		EventManager.RemoveEventListener("Event_OnSettingsHandednessChanged", Event_OnSettingsHandednessChanged);
		EventManager.RemoveEventListener("Event_OnSettingsShowPuckSilhouetteChanged", Event_OnSettingsShowPuckSilhouetteChanged);
		EventManager.RemoveEventListener("Event_OnSettingsShowTeamColorBarChanged", Event_OnSettingsShowTeamColorBarChanged);
		EventManager.RemoveEventListener("Event_OnSettingsShowPuckOutlineChanged", Event_OnSettingsShowPuckOutlineChanged);
		EventManager.RemoveEventListener("Event_OnSettingsShowPuckElevationChanged", Event_OnSettingsShowPuckElevationChanged);
		EventManager.RemoveEventListener("Event_OnSettingsShowMinimapSticksChanged", Event_OnSettingsShowMinimapSticksChanged);
		EventManager.RemoveEventListener("Event_OnSettingsShowMinimapPuckElevationChanged", Event_OnSettingsShowMinimapPuckElevationChanged);
		EventManager.RemoveEventListener("Event_OnSettingsShowMinimapFallenIndicatorChanged", Event_OnSettingsShowMinimapFallenIndicatorChanged);
		EventManager.RemoveEventListener("Event_OnSettingsShowPlayerUsernamesChanged", Event_OnSettingsShowPlayerUsernamesChanged);
		EventManager.RemoveEventListener("Event_OnSettingsPlayerUsernamesFadeThresholdChanged", Event_OnSettingsPlayerUsernamesFadeThresholdChanged);
		EventManager.RemoveEventListener("Event_OnSettingsMaxMatchmakingPingChanged", Event_OnSettingsMaxMatchmakingPingChanged);
		EventManager.RemoveEventListener("Event_OnSettingsNetworkBufferingChanged", Event_OnSettingsNetworkBufferingChanged);
		EventManager.RemoveEventListener("Event_OnSettingsFilterChatProfanityChanged", Event_OnSettingsFilterChatProfanityChanged);
		EventManager.RemoveEventListener("Event_OnSettingsUnitsChanged", Event_OnSettingsUnitsChanged);
		EventManager.RemoveEventListener("Event_OnSettingsShowGameUserInterfaceChanged", Event_OnSettingsShowGameUserInterfaceChanged);
		EventManager.RemoveEventListener("Event_OnSettingsUserInterfaceScaleChanged", Event_OnSettingsUserInterfaceScaleChanged);
		EventManager.RemoveEventListener("Event_OnSettingsChatOpacityChanged", Event_OnSettingsChatOpacityChanged);
		EventManager.RemoveEventListener("Event_OnSettingsChatScaleChanged", Event_OnSettingsChatScaleChanged);
		EventManager.RemoveEventListener("Event_OnSettingsMinimapOpacityChanged", Event_OnSettingsMinimapOpacityChanged);
		EventManager.RemoveEventListener("Event_OnSettingsMinimapBackgroundOpacityChanged", Event_OnSettingsMinimapBackgroundOpacityChanged);
		EventManager.RemoveEventListener("Event_OnSettingsMinimapHorizontalPositionChanged", Event_OnSettingsMinimapHorizontalPositionChanged);
		EventManager.RemoveEventListener("Event_OnSettingsMinimapVerticalPositionChanged", Event_OnSettingsMinimapVerticalPositionChanged);
		EventManager.RemoveEventListener("Event_OnSettingsMinimapScaleChanged", Event_OnSettingsMinimapScaleChanged);
		EventManager.RemoveEventListener("Event_OnSettingsGlobalStickSensitivityChanged", Event_OnSettingsGlobalStickSensitivityChanged);
		EventManager.RemoveEventListener("Event_OnSettingsHorizontalStickSensitivityChanged", Event_OnSettingsHorizontalStickSensitivityChanged);
		EventManager.RemoveEventListener("Event_OnSettingsVerticalStickSensitivityChanged", Event_OnSettingsVerticalStickSensitivityChanged);
		EventManager.RemoveEventListener("Event_OnSettingsLookSensitivityChanged", Event_OnSettingsLookSensitivityChanged);
		EventManager.RemoveEventListener("Event_OnSettingsGlobalVolumeChanged", Event_OnSettingsGlobalVolumeChanged);
		EventManager.RemoveEventListener("Event_OnSettingsAmbientVolumeChanged", Event_OnSettingsAmbientVolumeChanged);
		EventManager.RemoveEventListener("Event_OnSettingsGameVolumeChanged", Event_OnSettingsGameVolumeChanged);
		EventManager.RemoveEventListener("Event_OnSettingsVoiceVolumeChanged", Event_OnSettingsVoiceVolumeChanged);
		EventManager.RemoveEventListener("Event_OnSettingsUIVolumeChanged", Event_OnSettingsUIVolumeChanged);
		EventManager.RemoveEventListener("Event_OnSettingsFullScreenModeChanged", Event_OnSettingsFullScreenModeChanged);
		EventManager.RemoveEventListener("Event_OnSettingsDisplayChanged", Event_OnSettingsDisplayChanged);
		EventManager.RemoveEventListener("Event_OnSettingsResolutionChanged", Event_OnSettingsResolutionChanged);
		EventManager.RemoveEventListener("Event_OnSettingsVSyncChanged", Event_OnSettingsVSyncChanged);
		EventManager.RemoveEventListener("Event_OnSettingsFpsLimitChanged", Event_OnSettingsFpsLimitChanged);
		EventManager.RemoveEventListener("Event_OnSettingsFovChanged", Event_OnSettingsFovChanged);
		EventManager.RemoveEventListener("Event_OnSettingsQualityChanged", Event_OnSettingsQualityChanged);
		EventManager.RemoveEventListener("Event_OnSettingsShadowQualityChanged", Event_OnSettingsShadowQualityChanged);
		EventManager.RemoveEventListener("Event_OnSettingsMotionBlurChanged", Event_OnSettingsMotionBlurChanged);
	}

	private static void RemoveAppearanceEventListeners()
	{
		EventManager.RemoveEventListener("Event_OnAppearanceTeamChanged", Event_OnAppearanceTeamChanged);
		EventManager.RemoveEventListener("Event_OnAppearanceRoleChanged", Event_OnAppearanceRoleChanged);
		EventManager.RemoveEventListener("Event_OnAppearanceApplyForBothTeamsChanged", Event_OnAppearanceApplyForBothTeamsChanged);
		EventManager.RemoveEventListener("Event_OnAppearanceClickItem", Event_OnAppearanceClickItem);
	}

	private static DebugMode GetNextDebugMode(DebugMode debugMode)
	{
		return debugMode switch
		{
			DebugMode.Off => DebugMode.Simple, 
			DebugMode.Simple => DebugMode.Advanced, 
			_ => DebugMode.Off, 
		};
	}

	private static void OnDebug1ActionPerformed(InputAction.CallbackContext context)
	{
		SettingsManager.UpdateDebug(GetNextDebugMode(SettingsManager.Debug));
	}

	private static void Event_OnSettingsCameraAngleChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateCameraAngle((float)message["value"]);
	}

	private static void Event_OnSettingsHandednessChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateHandedness(Utils.GetHandednessFromName((string)message["value"]));
	}

	private static void Event_OnSettingsShowPuckSilhouetteChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateShowPuckSilhouette((bool)message["value"]);
	}

	private static void Event_OnSettingsShowTeamColorBarChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateShowTeamColorBar((bool)message["value"]);
	}

	private static void Event_OnSettingsShowPuckOutlineChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateShowPuckOutline((bool)message["value"]);
	}

	private static void Event_OnSettingsShowPuckElevationChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateShowPuckElevation((bool)message["value"]);
	}

	private static void Event_OnSettingsShowMinimapSticksChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateShowMinimapSticks((bool)message["value"]);
	}

	private static void Event_OnSettingsShowMinimapPuckElevationChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateShowMinimapPuckElevation((bool)message["value"]);
	}

	private static void Event_OnSettingsShowMinimapFallenIndicatorChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateShowMinimapFallenIndicator((bool)message["value"]);
	}

	private static void Event_OnSettingsShowPlayerUsernamesChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateShowPlayerUsernames((bool)message["value"]);
	}

	private static void Event_OnSettingsPlayerUsernamesFadeThresholdChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdatePlayerUsernamesFadeThreshold((float)message["value"]);
	}

	private static void Event_OnSettingsMaxMatchmakingPingChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateMaxMatchmakingPing((int)message["value"]);
	}

	private static void Event_OnSettingsNetworkBufferingChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateNetworkBuffering(Utils.GetNetworkBufferingFromName((string)message["value"]));
	}

	private static void Event_OnSettingsFilterChatProfanityChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateFilterChatProfanity((bool)message["value"]);
	}

	private static void Event_OnSettingsUnitsChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateUnits(Utils.GetUnitsFromName((string)message["value"]));
	}

	private static void Event_OnSettingsShowGameUserInterfaceChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateShowGameUserInterface((bool)message["value"]);
	}

	private static void Event_OnSettingsUserInterfaceScaleChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateUserInterfaceScale((float)message["value"]);
	}

	private static void Event_OnSettingsChatOpacityChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateChatOpacity((float)message["value"]);
	}

	private static void Event_OnSettingsChatScaleChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateChatScale((float)message["value"]);
	}

	private static void Event_OnSettingsMinimapOpacityChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateMinimapOpacity((float)message["value"]);
	}

	private static void Event_OnSettingsMinimapBackgroundOpacityChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateMinimapBackgroundOpacity((float)message["value"]);
	}

	private static void Event_OnSettingsMinimapHorizontalPositionChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateMinimapHorizontalPosition((float)message["value"]);
	}

	private static void Event_OnSettingsMinimapVerticalPositionChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateMinimapVerticalPosition((float)message["value"]);
	}

	private static void Event_OnSettingsMinimapScaleChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateMinimapScale((float)message["value"]);
	}

	private static void Event_OnSettingsGlobalStickSensitivityChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateGlobalStickSensitivity((float)message["value"]);
	}

	private static void Event_OnSettingsHorizontalStickSensitivityChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateHorizontalStickSensitivity((float)message["value"]);
	}

	private static void Event_OnSettingsVerticalStickSensitivityChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateVerticalStickSensitivity((float)message["value"]);
	}

	private static void Event_OnSettingsLookSensitivityChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateLookSensitivity((float)message["value"]);
	}

	private static void Event_OnSettingsGlobalVolumeChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateGlobalVolume((float)message["value"]);
	}

	private static void Event_OnSettingsAmbientVolumeChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateAmbientVolume((float)message["value"]);
	}

	private static void Event_OnSettingsGameVolumeChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateGameVolume((float)message["value"]);
	}

	private static void Event_OnSettingsVoiceVolumeChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateVoiceVolume((float)message["value"]);
	}

	private static void Event_OnSettingsUIVolumeChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateUIVolume((float)message["value"]);
	}

	private static void Event_OnSettingsFullScreenModeChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateFullScreenMode(Utils.GetFullScreenModeFromName((string)message["value"]));
	}

	private static void Event_OnSettingsDisplayChanged(Dictionary<string, object> message)
	{
		string text = (string)message["value"];
		int displayIndexFromName = Utils.GetDisplayIndexFromName(text);
		if (displayIndexFromName == -1)
		{
			Logger.Warning("Could not find display index for display name " + text + ", skipping update");
		}
		else
		{
			SettingsManager.UpdateDisplayIndex(displayIndexFromName);
		}
	}

	private static void Event_OnSettingsResolutionChanged(Dictionary<string, object> message)
	{
		string text = (string)message["value"];
		int resolutionIndexFromName = Utils.GetResolutionIndexFromName(text);
		if (resolutionIndexFromName == -1)
		{
			Logger.Warning("Could not find resolution index for resolution name " + text + ", skipping update");
		}
		else
		{
			SettingsManager.UpdateResolutionIndex(resolutionIndexFromName);
		}
	}

	private static void Event_OnSettingsVSyncChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateVSync((bool)message["value"]);
	}

	private static void Event_OnSettingsFpsLimitChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateFpsLimit((int)message["value"]);
	}

	private static void Event_OnSettingsFovChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateFov((float)message["value"]);
	}

	private static void Event_OnSettingsQualityChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateQuality(Utils.GetApplicationQualityFromName((string)message["value"]));
	}

	private static void Event_OnSettingsShadowQualityChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateShadowQuality(Utils.GetShadowQualityFromName((string)message["value"]));
	}

	private static void Event_OnSettingsMotionBlurChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateMotionBlur((bool)message["value"]);
	}

	private static void Event_OnAppearanceTeamChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateTeam(Utils.GetTeamFromName((string)message["value"]));
	}

	private static void Event_OnAppearanceRoleChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateRole(Utils.GetRoleFromName((string)message["value"]));
	}

	private static void Event_OnAppearanceApplyForBothTeamsChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateApplyForBothTeams((bool)message["value"]);
	}

	private static void Event_OnAppearanceClickItem(Dictionary<string, object> message)
	{
		Item item = (Item)message["item"];
		_ = (AppearanceCategory)message["category"];
		AppearanceSubcategory appearanceSubcategory = (AppearanceSubcategory)message["subcategory"];
		PlayerTeam playerTeam = (PlayerTeam)message["team"];
		PlayerRole role = (PlayerRole)message["role"];
		if (!item.IsOwned)
		{
			return;
		}
		PlayerTeam team = ((playerTeam != PlayerTeam.Blue) ? PlayerTeam.Blue : PlayerTeam.Red);
		switch (appearanceSubcategory)
		{
		case AppearanceSubcategory.Headgear:
			SettingsManager.UpdateHeadgearID(playerTeam, role, item.id);
			if (SettingsManager.ApplyForBothTeams)
			{
				SettingsManager.UpdateHeadgearID(team, role, item.id);
			}
			break;
		case AppearanceSubcategory.Flags:
			SettingsManager.UpdateFlagID(item.id);
			break;
		case AppearanceSubcategory.Mustaches:
			SettingsManager.UpdateMustacheID(item.id);
			break;
		case AppearanceSubcategory.Beards:
			SettingsManager.UpdateBeardID(item.id);
			break;
		case AppearanceSubcategory.Jerseys:
			SettingsManager.UpdateJerseyID(playerTeam, role, item.id);
			if (SettingsManager.ApplyForBothTeams)
			{
				SettingsManager.UpdateJerseyID(team, role, item.id);
			}
			break;
		case AppearanceSubcategory.StickSkins:
			SettingsManager.UpdateStickSkinID(playerTeam, role, item.id);
			if (SettingsManager.ApplyForBothTeams)
			{
				SettingsManager.UpdateStickSkinID(team, role, item.id);
			}
			break;
		case AppearanceSubcategory.StickShaftTapes:
			SettingsManager.UpdateStickShaftTapeID(playerTeam, role, item.id);
			if (SettingsManager.ApplyForBothTeams)
			{
				SettingsManager.UpdateStickShaftTapeID(team, role, item.id);
			}
			break;
		case AppearanceSubcategory.StickBladeTapes:
			SettingsManager.UpdateStickBladeTapeID(playerTeam, role, item.id);
			if (SettingsManager.ApplyForBothTeams)
			{
				SettingsManager.UpdateStickBladeTapeID(team, role, item.id);
			}
			break;
		}
	}

	private static void Event_OnDisplayIndexChanged(Dictionary<string, object> message)
	{
		SettingsManager.UpdateResolutionIndex(-1);
	}

	private static void Event_OnIsDisplayChangeInProgressChanged(Dictionary<string, object> message)
	{
		if (!(bool)message["isDisplayChangeInProgress"] && SettingsManager.ResolutionIndex == -1)
		{
			SettingsManager.UpdateResolutionIndex(Utils.GetResolutions().Count - 1);
		}
	}

	private static void Event_OnBaseCameraEnabled(Dictionary<string, object> message)
	{
		((BaseCamera)message["baseCamera"]).SetFieldOfView(SettingsManager.Fov);
	}

	private static void Event_OnPopupClickOk(Dictionary<string, object> message)
	{
		if (((Popup)message["popup"]).Name == "settingsResetToDefault")
		{
			SettingsManager.ResetToDefault();
			EventManager.TriggerEvent("Event_OnSettingsResetToDefault");
		}
	}
}
