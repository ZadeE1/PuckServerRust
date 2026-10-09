using System.Collections.Generic;
using UnityEngine;

public static class SettingsManager
{
	public static DebugMode Debug;

	public static float CameraAngle;

	public static PlayerHandedness Handedness;

	public static bool ShowPuckSilhouette;

	public static bool ShowPuckOutline;

	public static bool ShowPuckElevation;

	public static bool ShowPlayerUsernames;

	public static float PlayerUsernamesFadeThreshold;

	public static int MaxMatchmakingPing;

	public static NetworkBuffering NetworkBuffering;

	public static bool FilterChatProfanity;

	public static Units Units;

	public static bool ShowGameUserInterface;

	public static bool ShowTeamColorBar;

	public static float UserInterfaceScale;

	public static float ChatOpacity;

	public static float ChatScale;

	public static float MinimapOpacity;

	public static float MinimapBackgroundOpacity;

	public static float MinimapHorizontalPosition;

	public static float MinimapVerticalPosition;

	public static float MinimapScale;

	public static bool ShowMinimapSticks;

	public static bool ShowMinimapPuckElevation;

	public static bool ShowMinimapFallenIndicator;

	public static float GlobalStickSensitivity;

	public static float HorizontalStickSensitivity;

	public static float VerticalStickSensitivity;

	public static float LookSensitivity;

	public static float GlobalVolume;

	public static float AmbientVolume;

	public static float GameVolume;

	public static float VoiceVolume;

	public static float UIVolume;

	public static FullScreenMode FullScreenMode;

	public static int DisplayIndex;

	public static int ResolutionIndex;

	public static bool VSync;

	public static int FpsLimit;

	public static float Fov;

	public static ApplicationQuality Quality;

	public static ShadowQuality ShadowQuality;

	public static bool MotionBlur;

	public static PlayerTeam Team;

	public static PlayerRole Role;

	public static bool ApplyForBothTeams;

	public static int FlagID;

	public static int HeadgearIDBlueAttacker;

	public static int HeadgearIDRedAttacker;

	public static int HeadgearIDBlueGoalie;

	public static int HeadgearIDRedGoalie;

	public static int MustacheID;

	public static int BeardID;

	public static int JerseyIDBlueAttacker;

	public static int JerseyIDRedAttacker;

	public static int JerseyIDBlueGoalie;

	public static int JerseyIDRedGoalie;

	public static int StickSkinIDBlueAttacker;

	public static int StickSkinIDRedAttacker;

	public static int StickSkinIDBlueGoalie;

	public static int StickSkinIDRedGoalie;

	public static int StickShaftTapeIDBlueAttacker;

	public static int StickShaftTapeIDRedAttacker;

	public static int StickShaftTapeIDBlueGoalie;

	public static int StickShaftTapeIDRedGoalie;

	public static int StickBladeTapeIDBlueAttacker;

	public static int StickBladeTapeIDRedAttacker;

	public static int StickBladeTapeIDBlueGoalie;

	public static int StickBladeTapeIDRedGoalie;

	public static void Initialize()
	{
		Debug = SaveManager.GetEnum("debug", DebugMode.Off);
		CameraAngle = SaveManager.GetFloat("cameraAngle", 30f);
		Handedness = SaveManager.GetEnum("handedness", PlayerHandedness.Right);
		ShowPuckSilhouette = SaveManager.GetBool("showPuckSilhouette", defaultValue: true);
		ShowPuckOutline = SaveManager.GetBool("showPuckOutline", defaultValue: false);
		ShowPuckElevation = SaveManager.GetBool("showPuckElevation", defaultValue: true);
		ShowPlayerUsernames = SaveManager.GetBool("showPlayerUsernames", defaultValue: false);
		PlayerUsernamesFadeThreshold = SaveManager.GetFloat("playerUsernamesFadeThreshold", 1f);
		MaxMatchmakingPing = SaveManager.GetInt("maxMatchmakingPing", 50);
		NetworkBuffering = SaveManager.GetEnum("networkBuffering", NetworkBuffering.Responsive);
		FilterChatProfanity = SaveManager.GetBool("filterChatProfanity", defaultValue: true);
		Units = SaveManager.GetEnum("units", Units.Metric);
		ShowGameUserInterface = SaveManager.GetBool("showGameUserInterface", defaultValue: true);
		ShowTeamColorBar = SaveManager.GetBool("showTeamColorBar", defaultValue: true);
		UserInterfaceScale = SaveManager.GetFloat("userInterfaceScale", 1f);
		ChatOpacity = SaveManager.GetFloat("chatOpacity", 1f);
		ChatScale = SaveManager.GetFloat("chatScale", 1f);
		MinimapOpacity = SaveManager.GetFloat("minimapOpacity", 1f);
		MinimapBackgroundOpacity = SaveManager.GetFloat("minimapBackgroundOpacity", 1f);
		MinimapHorizontalPosition = SaveManager.GetFloat("minimapHorizontalPosition", 100f);
		MinimapVerticalPosition = SaveManager.GetFloat("minimapVerticalPosition", 0f);
		MinimapScale = SaveManager.GetFloat("minimapScale", 1f);
		ShowMinimapSticks = SaveManager.GetBool("showMinimapSticks", defaultValue: true);
		ShowMinimapPuckElevation = SaveManager.GetBool("showMinimapPuckElevation", defaultValue: true);
		ShowMinimapFallenIndicator = SaveManager.GetBool("showMinimapFallenIndicator", defaultValue: true);
		GlobalStickSensitivity = SaveManager.GetFloat("globalStickSensitivity", 0.2f);
		HorizontalStickSensitivity = SaveManager.GetFloat("horizontalStickSensitivity", 1f);
		VerticalStickSensitivity = SaveManager.GetFloat("verticalStickSensitivity", 1f);
		LookSensitivity = SaveManager.GetFloat("lookSensitivity", 0.2f);
		GlobalVolume = SaveManager.GetFloat("globalVolume", 0.5f);
		AmbientVolume = SaveManager.GetFloat("ambientVolume", 1f);
		GameVolume = SaveManager.GetFloat("gameVolume", 1f);
		VoiceVolume = SaveManager.GetFloat("voiceVolume", 1f);
		UIVolume = SaveManager.GetFloat("uiVolume", 0.5f);
		FullScreenMode = SaveManager.GetEnum("fullScreenMode", FullScreenMode.FullScreenWindow);
		DisplayIndex = SaveManager.GetInt("displayIndex", 0);
		ResolutionIndex = SaveManager.GetInt("resolutionIndex", -1);
		VSync = SaveManager.GetBool("vSync", defaultValue: false);
		FpsLimit = SaveManager.GetInt("fpsLimit", 240);
		Fov = SaveManager.GetFloat("fov", 90f);
		Quality = SaveManager.GetEnum("quality", ApplicationQuality.High);
		ShadowQuality = SaveManager.GetEnum("shadowQuality", ShadowQuality.High);
		MotionBlur = SaveManager.GetBool("motionBlur", defaultValue: true);
		Team = SaveManager.GetEnum("team", PlayerTeam.Blue);
		Role = SaveManager.GetEnum("role", PlayerRole.Attacker);
		ApplyForBothTeams = SaveManager.GetBool("applyForBothTeams", defaultValue: false);
		FlagID = SaveManager.GetInt("flagID", -1);
		HeadgearIDBlueAttacker = SaveManager.GetInt("headgearIDBlueAttacker", 513);
		HeadgearIDRedAttacker = SaveManager.GetInt("headgearIDRedAttacker", 513);
		HeadgearIDBlueGoalie = SaveManager.GetInt("headgearIDBlueGoalie", 527);
		HeadgearIDRedGoalie = SaveManager.GetInt("headgearIDRedGoalie", 527);
		MustacheID = SaveManager.GetInt("mustacheID", -1);
		BeardID = SaveManager.GetInt("beardID", -1);
		JerseyIDBlueAttacker = SaveManager.GetInt("jerseyIDBlueAttacker", 2048);
		JerseyIDRedAttacker = SaveManager.GetInt("jerseyIDRedAttacker", 2048);
		JerseyIDBlueGoalie = SaveManager.GetInt("jerseyIDBlueGoalie", 2048);
		JerseyIDRedGoalie = SaveManager.GetInt("jerseyIDRedGoalie", 2048);
		StickSkinIDBlueAttacker = SaveManager.GetInt("stickSkinIDBlueAttacker", 2621);
		StickSkinIDRedAttacker = SaveManager.GetInt("stickSkinIDRedAttacker", 2621);
		StickSkinIDBlueGoalie = SaveManager.GetInt("stickSkinIDBlueGoalie", 2621);
		StickSkinIDRedGoalie = SaveManager.GetInt("stickSkinIDRedGoalie", 2621);
		StickShaftTapeIDBlueAttacker = SaveManager.GetInt("stickShaftTapeIDBlueAttacker", -1);
		StickShaftTapeIDRedAttacker = SaveManager.GetInt("stickShaftTapeIDRedAttacker", -1);
		StickShaftTapeIDBlueGoalie = SaveManager.GetInt("stickShaftTapeIDBlueGoalie", -1);
		StickShaftTapeIDRedGoalie = SaveManager.GetInt("stickShaftTapeIDRedGoalie", -1);
		StickBladeTapeIDBlueAttacker = SaveManager.GetInt("stickBladeTapeIDBlueAttacker", -1);
		StickBladeTapeIDRedAttacker = SaveManager.GetInt("stickBladeTapeIDRedAttacker", -1);
		StickBladeTapeIDBlueGoalie = SaveManager.GetInt("stickBladeTapeIDBlueGoalie", -1);
		StickBladeTapeIDRedGoalie = SaveManager.GetInt("stickBladeTapeIDRedGoalie", -1);
		SettingsManagerController.Initialize();
	}

	public static void Dispose()
	{
		SettingsManagerController.Dispose();
	}

	public static void ResetToDefault()
	{
		UpdateDebug(DebugMode.Off);
		UpdateCameraAngle(30f);
		UpdateHandedness(PlayerHandedness.Right);
		UpdateShowPuckSilhouette(value: true);
		UpdateShowPuckOutline(value: false);
		UpdateShowPuckElevation(value: true);
		UpdateShowPlayerUsernames(value: false);
		UpdatePlayerUsernamesFadeThreshold(1f);
		UpdateMaxMatchmakingPing(50);
		UpdateNetworkBuffering(NetworkBuffering.Responsive);
		UpdateFilterChatProfanity(value: true);
		UpdateUnits(Units.Metric);
		UpdateShowGameUserInterface(value: true);
		UpdateShowTeamColorBar(value: true);
		UpdateUserInterfaceScale(1f);
		UpdateChatOpacity(1f);
		UpdateChatScale(1f);
		UpdateMinimapOpacity(1f);
		UpdateMinimapBackgroundOpacity(1f);
		UpdateMinimapHorizontalPosition(100f);
		UpdateMinimapVerticalPosition(0f);
		UpdateMinimapScale(1f);
		UpdateShowMinimapSticks(value: true);
		UpdateShowMinimapPuckElevation(value: true);
		UpdateShowMinimapFallenIndicator(value: true);
		UpdateGlobalStickSensitivity(0.2f);
		UpdateHorizontalStickSensitivity(1f);
		UpdateVerticalStickSensitivity(1f);
		UpdateLookSensitivity(0.2f);
		UpdateGlobalVolume(0.5f);
		UpdateAmbientVolume(1f);
		UpdateGameVolume(1f);
		UpdateVoiceVolume(1f);
		UpdateUIVolume(0.5f);
		UpdateFullScreenMode(FullScreenMode.FullScreenWindow);
		UpdateDisplayIndex(0);
		UpdateResolutionIndex(-1);
		UpdateVSync(value: false);
		UpdateFpsLimit(240);
		UpdateFov(90f);
		UpdateQuality(ApplicationQuality.High);
		UpdateShadowQuality(ShadowQuality.High);
		UpdateMotionBlur(value: true);
		UpdateTeam(PlayerTeam.Blue);
		UpdateRole(PlayerRole.Attacker);
		UpdateApplyForBothTeams(value: false);
		UpdateFlagID(-1);
		UpdateHeadgearID(PlayerTeam.Blue, PlayerRole.Attacker, 513);
		UpdateHeadgearID(PlayerTeam.Red, PlayerRole.Attacker, 513);
		UpdateHeadgearID(PlayerTeam.Blue, PlayerRole.Goalie, 527);
		UpdateHeadgearID(PlayerTeam.Red, PlayerRole.Goalie, 527);
		UpdateMustacheID(-1);
		UpdateBeardID(-1);
		UpdateJerseyID(PlayerTeam.Blue, PlayerRole.Attacker, 2048);
		UpdateJerseyID(PlayerTeam.Red, PlayerRole.Attacker, 2048);
		UpdateJerseyID(PlayerTeam.Blue, PlayerRole.Goalie, 2048);
		UpdateJerseyID(PlayerTeam.Red, PlayerRole.Goalie, 2048);
		UpdateStickSkinID(PlayerTeam.Blue, PlayerRole.Attacker, 2621);
		UpdateStickSkinID(PlayerTeam.Red, PlayerRole.Attacker, 2621);
		UpdateStickSkinID(PlayerTeam.Blue, PlayerRole.Goalie, 2621);
		UpdateStickSkinID(PlayerTeam.Red, PlayerRole.Goalie, 2621);
		UpdateStickShaftTapeID(PlayerTeam.Blue, PlayerRole.Attacker, -1);
		UpdateStickShaftTapeID(PlayerTeam.Red, PlayerRole.Attacker, -1);
		UpdateStickShaftTapeID(PlayerTeam.Blue, PlayerRole.Goalie, -1);
		UpdateStickShaftTapeID(PlayerTeam.Red, PlayerRole.Goalie, -1);
		UpdateStickBladeTapeID(PlayerTeam.Blue, PlayerRole.Attacker, -1);
		UpdateStickBladeTapeID(PlayerTeam.Red, PlayerRole.Attacker, -1);
		UpdateStickBladeTapeID(PlayerTeam.Blue, PlayerRole.Goalie, -1);
		UpdateStickBladeTapeID(PlayerTeam.Red, PlayerRole.Goalie, -1);
	}

	public static int GetHeadgearID(PlayerTeam team, PlayerRole role)
	{
		switch (role)
		{
		case PlayerRole.Attacker:
			switch (team)
			{
			case PlayerTeam.Blue:
				return HeadgearIDBlueAttacker;
			case PlayerTeam.Red:
				return HeadgearIDRedAttacker;
			}
			break;
		case PlayerRole.Goalie:
			switch (team)
			{
			case PlayerTeam.Blue:
				return HeadgearIDBlueGoalie;
			case PlayerTeam.Red:
				return HeadgearIDRedGoalie;
			}
			break;
		}
		return -1;
	}

	public static int GetJerseyID(PlayerTeam team, PlayerRole role)
	{
		switch (role)
		{
		case PlayerRole.Attacker:
			switch (team)
			{
			case PlayerTeam.Blue:
				return JerseyIDBlueAttacker;
			case PlayerTeam.Red:
				return JerseyIDRedAttacker;
			}
			break;
		case PlayerRole.Goalie:
			switch (team)
			{
			case PlayerTeam.Blue:
				return JerseyIDBlueGoalie;
			case PlayerTeam.Red:
				return JerseyIDRedGoalie;
			}
			break;
		}
		return 2048;
	}

	public static int GetStickSkinID(PlayerTeam team, PlayerRole role)
	{
		switch (role)
		{
		case PlayerRole.Attacker:
			switch (team)
			{
			case PlayerTeam.Blue:
				return StickSkinIDBlueAttacker;
			case PlayerTeam.Red:
				return StickSkinIDRedAttacker;
			}
			break;
		case PlayerRole.Goalie:
			switch (team)
			{
			case PlayerTeam.Blue:
				return StickSkinIDBlueGoalie;
			case PlayerTeam.Red:
				return StickSkinIDRedGoalie;
			}
			break;
		}
		return 2621;
	}

	public static int GetStickShaftTapeID(PlayerTeam team, PlayerRole role)
	{
		switch (role)
		{
		case PlayerRole.Attacker:
			switch (team)
			{
			case PlayerTeam.Blue:
				return StickShaftTapeIDBlueAttacker;
			case PlayerTeam.Red:
				return StickShaftTapeIDRedAttacker;
			}
			break;
		case PlayerRole.Goalie:
			switch (team)
			{
			case PlayerTeam.Blue:
				return StickShaftTapeIDBlueGoalie;
			case PlayerTeam.Red:
				return StickShaftTapeIDRedGoalie;
			}
			break;
		}
		return -1;
	}

	public static int GetStickBladeTapeID(PlayerTeam team, PlayerRole role)
	{
		switch (role)
		{
		case PlayerRole.Attacker:
			switch (team)
			{
			case PlayerTeam.Blue:
				return StickBladeTapeIDBlueAttacker;
			case PlayerTeam.Red:
				return StickBladeTapeIDRedAttacker;
			}
			break;
		case PlayerRole.Goalie:
			switch (team)
			{
			case PlayerTeam.Blue:
				return StickBladeTapeIDBlueGoalie;
			case PlayerTeam.Red:
				return StickBladeTapeIDRedGoalie;
			}
			break;
		}
		return -1;
	}

	public static void UpdateDebug(DebugMode value)
	{
		if (Debug != value)
		{
			Debug = value;
			SaveManager.SetEnum("debug", Debug);
			EventManager.TriggerEvent("Event_OnDebugChanged", new Dictionary<string, object> { { "value", Debug } });
		}
	}

	public static void UpdateCameraAngle(float value)
	{
		if (CameraAngle != value)
		{
			CameraAngle = value;
			SaveManager.SetFloat("cameraAngle", CameraAngle);
			EventManager.TriggerEvent("Event_OnCameraAngleChanged", new Dictionary<string, object> { { "value", CameraAngle } });
		}
	}

	public static void UpdateHandedness(PlayerHandedness value)
	{
		if (Handedness != value)
		{
			Handedness = value;
			SaveManager.SetEnum("handedness", Handedness);
			EventManager.TriggerEvent("Event_OnHandednessChanged", new Dictionary<string, object> { { "value", Handedness } });
		}
	}

	public static void UpdateShowPuckSilhouette(bool value)
	{
		if (ShowPuckSilhouette != value)
		{
			ShowPuckSilhouette = value;
			SaveManager.SetBool("showPuckSilhouette", ShowPuckSilhouette);
			EventManager.TriggerEvent("Event_OnShowPuckSilhouetteChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateShowPuckOutline(bool value)
	{
		if (ShowPuckOutline != value)
		{
			ShowPuckOutline = value;
			SaveManager.SetBool("showPuckOutline", ShowPuckOutline);
			EventManager.TriggerEvent("Event_OnShowPuckOutlineChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateShowPuckElevation(bool value)
	{
		if (ShowPuckElevation != value)
		{
			ShowPuckElevation = value;
			SaveManager.SetBool("showPuckElevation", ShowPuckElevation);
			EventManager.TriggerEvent("Event_OnShowPuckElevationChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateShowMinimapSticks(bool value)
	{
		if (ShowMinimapSticks != value)
		{
			ShowMinimapSticks = value;
			SaveManager.SetBool("showMinimapSticks", ShowMinimapSticks);
			EventManager.TriggerEvent("Event_OnShowMinimapSticksChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateShowMinimapPuckElevation(bool value)
	{
		if (ShowMinimapPuckElevation != value)
		{
			ShowMinimapPuckElevation = value;
			SaveManager.SetBool("showMinimapPuckElevation", ShowMinimapPuckElevation);
			EventManager.TriggerEvent("Event_OnShowMinimapPuckElevationChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateShowMinimapFallenIndicator(bool value)
	{
		if (ShowMinimapFallenIndicator != value)
		{
			ShowMinimapFallenIndicator = value;
			SaveManager.SetBool("showMinimapFallenIndicator", ShowMinimapFallenIndicator);
			EventManager.TriggerEvent("Event_OnShowMinimapFallenIndicatorChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateShowPlayerUsernames(bool value)
	{
		if (ShowPlayerUsernames != value)
		{
			ShowPlayerUsernames = value;
			SaveManager.SetBool("showPlayerUsernames", ShowPlayerUsernames);
			EventManager.TriggerEvent("Event_OnShowPlayerUsernamesChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdatePlayerUsernamesFadeThreshold(float value)
	{
		if (PlayerUsernamesFadeThreshold != value)
		{
			PlayerUsernamesFadeThreshold = value;
			SaveManager.SetFloat("playerUsernamesFadeThreshold", PlayerUsernamesFadeThreshold);
			EventManager.TriggerEvent("Event_OnPlayerUsernamesFadeThresholdChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateMaxMatchmakingPing(int value)
	{
		if (MaxMatchmakingPing != value)
		{
			MaxMatchmakingPing = value;
			SaveManager.SetInt("maxMatchmakingPing", MaxMatchmakingPing);
			EventManager.TriggerEvent("Event_OnMaxMatchmakingPingChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateNetworkBuffering(NetworkBuffering value)
	{
		if (NetworkBuffering != value)
		{
			NetworkBuffering = value;
			SaveManager.SetEnum("networkBuffering", NetworkBuffering);
			EventManager.TriggerEvent("Event_OnNetworkBufferingChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateFilterChatProfanity(bool value)
	{
		if (FilterChatProfanity != value)
		{
			FilterChatProfanity = value;
			SaveManager.SetBool("filterChatProfanity", FilterChatProfanity);
			EventManager.TriggerEvent("Event_OnFilterChatProfanityChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateUnits(Units value)
	{
		if (Units != value)
		{
			Units = value;
			SaveManager.SetEnum("units", Units);
			EventManager.TriggerEvent("Event_OnUnitsChanged", new Dictionary<string, object> { { "value", Units } });
		}
	}

	public static void UpdateShowGameUserInterface(bool value)
	{
		if (ShowGameUserInterface != value)
		{
			ShowGameUserInterface = value;
			SaveManager.SetBool("showGameUserInterface", ShowGameUserInterface);
			EventManager.TriggerEvent("Event_OnShowGameUserInterfaceChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateShowTeamColorBar(bool value)
	{
		if (ShowTeamColorBar != value)
		{
			ShowTeamColorBar = value;
			SaveManager.SetBool("showTeamColorBar", ShowTeamColorBar);
			EventManager.TriggerEvent("Event_OnShowTeamColorBarChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateUserInterfaceScale(float value)
	{
		if (UserInterfaceScale != value)
		{
			UserInterfaceScale = value;
			SaveManager.SetFloat("userInterfaceScale", UserInterfaceScale);
			EventManager.TriggerEvent("Event_OnUserInterfaceScaleChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateChatOpacity(float value)
	{
		if (ChatOpacity != value)
		{
			ChatOpacity = value;
			SaveManager.SetFloat("chatOpacity", ChatOpacity);
			EventManager.TriggerEvent("Event_OnChatOpacityChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateChatScale(float value)
	{
		if (ChatScale != value)
		{
			ChatScale = value;
			SaveManager.SetFloat("chatScale", ChatScale);
			EventManager.TriggerEvent("Event_OnChatScaleChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateMinimapOpacity(float value)
	{
		if (MinimapOpacity != value)
		{
			MinimapOpacity = value;
			SaveManager.SetFloat("minimapOpacity", MinimapOpacity);
			EventManager.TriggerEvent("Event_OnMinimapOpacityChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateMinimapBackgroundOpacity(float value)
	{
		if (MinimapBackgroundOpacity != value)
		{
			MinimapBackgroundOpacity = value;
			SaveManager.SetFloat("minimapBackgroundOpacity", MinimapBackgroundOpacity);
			EventManager.TriggerEvent("Event_OnMinimapBackgroundOpacityChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateMinimapHorizontalPosition(float value)
	{
		if (MinimapHorizontalPosition != value)
		{
			MinimapHorizontalPosition = value;
			SaveManager.SetFloat("minimapHorizontalPosition", MinimapHorizontalPosition);
			EventManager.TriggerEvent("Event_OnMinimapHorizontalPositionChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateMinimapVerticalPosition(float value)
	{
		if (MinimapVerticalPosition != value)
		{
			MinimapVerticalPosition = value;
			SaveManager.SetFloat("minimapVerticalPosition", MinimapVerticalPosition);
			EventManager.TriggerEvent("Event_OnMinimapVerticalPositionChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateMinimapScale(float value)
	{
		if (MinimapScale != value)
		{
			MinimapScale = value;
			SaveManager.SetFloat("minimapScale", MinimapScale);
			EventManager.TriggerEvent("Event_OnMinimapScaleChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateGlobalStickSensitivity(float value)
	{
		if (GlobalStickSensitivity != value)
		{
			GlobalStickSensitivity = value;
			SaveManager.SetFloat("globalStickSensitivity", GlobalStickSensitivity);
			EventManager.TriggerEvent("Event_OnGlobalStickSensitivityChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateHorizontalStickSensitivity(float value)
	{
		if (HorizontalStickSensitivity != value)
		{
			HorizontalStickSensitivity = value;
			SaveManager.SetFloat("horizontalStickSensitivity", HorizontalStickSensitivity);
			EventManager.TriggerEvent("Event_OnHorizontalStickSensitivityChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateVerticalStickSensitivity(float value)
	{
		if (VerticalStickSensitivity != value)
		{
			VerticalStickSensitivity = value;
			SaveManager.SetFloat("verticalStickSensitivity", VerticalStickSensitivity);
			EventManager.TriggerEvent("Event_OnVerticalStickSensitivityChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateLookSensitivity(float value)
	{
		if (LookSensitivity != value)
		{
			LookSensitivity = value;
			SaveManager.SetFloat("lookSensitivity", LookSensitivity);
			EventManager.TriggerEvent("Event_OnLookSensitivityChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateGlobalVolume(float value)
	{
		if (GlobalVolume != value)
		{
			GlobalVolume = value;
			SaveManager.SetFloat("globalVolume", GlobalVolume);
			EventManager.TriggerEvent("Event_OnGlobalVolumeChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateAmbientVolume(float value)
	{
		if (AmbientVolume != value)
		{
			AmbientVolume = value;
			SaveManager.SetFloat("ambientVolume", AmbientVolume);
			EventManager.TriggerEvent("Event_OnAmbientVolumeChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateGameVolume(float value)
	{
		if (GameVolume != value)
		{
			GameVolume = value;
			SaveManager.SetFloat("gameVolume", GameVolume);
			EventManager.TriggerEvent("Event_OnGameVolumeChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateVoiceVolume(float value)
	{
		if (VoiceVolume != value)
		{
			VoiceVolume = value;
			SaveManager.SetFloat("voiceVolume", VoiceVolume);
			EventManager.TriggerEvent("Event_OnVoiceVolumeChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateUIVolume(float value)
	{
		if (UIVolume != value)
		{
			UIVolume = value;
			SaveManager.SetFloat("uiVolume", UIVolume);
			EventManager.TriggerEvent("Event_OnUIVolumeChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateFullScreenMode(FullScreenMode value)
	{
		if (FullScreenMode != value)
		{
			FullScreenMode = value;
			SaveManager.SetEnum("fullScreenMode", FullScreenMode);
			EventManager.TriggerEvent("Event_OnFullScreenModeChanged", new Dictionary<string, object> { { "value", FullScreenMode } });
		}
	}

	public static void UpdateDisplayIndex(int value)
	{
		if (DisplayIndex != value)
		{
			DisplayIndex = value;
			SaveManager.SetInt("displayIndex", DisplayIndex);
			EventManager.TriggerEvent("Event_OnDisplayIndexChanged", new Dictionary<string, object> { { "value", DisplayIndex } });
		}
	}

	public static void UpdateResolutionIndex(int value)
	{
		if (ResolutionIndex != value)
		{
			ResolutionIndex = value;
			SaveManager.SetInt("resolutionIndex", ResolutionIndex);
			EventManager.TriggerEvent("Event_OnResolutionIndexChanged", new Dictionary<string, object> { { "value", ResolutionIndex } });
		}
	}

	public static void UpdateVSync(bool value)
	{
		if (VSync != value)
		{
			VSync = value;
			SaveManager.SetBool("vSync", VSync);
			EventManager.TriggerEvent("Event_OnVSyncChanged", new Dictionary<string, object> { { "value", VSync } });
		}
	}

	public static void UpdateFpsLimit(int value)
	{
		if (FpsLimit != value)
		{
			FpsLimit = value;
			SaveManager.SetInt("fpsLimit", FpsLimit);
			EventManager.TriggerEvent("Event_OnFpsLimitChanged", new Dictionary<string, object> { { "value", FpsLimit } });
		}
	}

	public static void UpdateFov(float value)
	{
		if (Fov != value)
		{
			Fov = value;
			SaveManager.SetFloat("fov", Fov);
			EventManager.TriggerEvent("Event_OnFovChanged", new Dictionary<string, object> { { "value", Fov } });
		}
	}

	public static void UpdateQuality(ApplicationQuality value)
	{
		if (Quality != value)
		{
			Quality = value;
			SaveManager.SetEnum("quality", Quality);
			EventManager.TriggerEvent("Event_OnQualityChanged", new Dictionary<string, object> { { "value", Quality } });
		}
	}

	public static void UpdateShadowQuality(ShadowQuality value)
	{
		if (ShadowQuality != value)
		{
			ShadowQuality = value;
			SaveManager.SetEnum("shadowQuality", ShadowQuality);
			EventManager.TriggerEvent("Event_OnShadowQualityChanged", new Dictionary<string, object> { { "value", ShadowQuality } });
		}
	}

	public static void UpdateMotionBlur(bool value)
	{
		if (MotionBlur != value)
		{
			MotionBlur = value;
			SaveManager.SetBool("motionBlur", MotionBlur);
			EventManager.TriggerEvent("Event_OnMotionBlurChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateTeam(PlayerTeam team)
	{
		if (Team != team)
		{
			Team = team;
			SaveManager.SetEnum("team", Team);
			EventManager.TriggerEvent("Event_OnTeamChanged", new Dictionary<string, object> { { "value", team } });
		}
	}

	public static void UpdateRole(PlayerRole role)
	{
		if (Role != role)
		{
			Role = role;
			SaveManager.SetEnum("role", Role);
			EventManager.TriggerEvent("Event_OnRoleChanged", new Dictionary<string, object> { { "value", role } });
		}
	}

	public static void UpdateApplyForBothTeams(bool value)
	{
		if (ApplyForBothTeams != value)
		{
			ApplyForBothTeams = value;
			SaveManager.SetBool("applyForBothTeams", ApplyForBothTeams);
			EventManager.TriggerEvent("Event_OnApplyForBothTeamsChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateFlagID(int value)
	{
		if (FlagID != value)
		{
			FlagID = value;
			SaveManager.SetInt("flagID", FlagID);
			EventManager.TriggerEvent("Event_OnFlagIDChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateHeadgearID(PlayerTeam team, PlayerRole role, int value)
	{
		if (GetHeadgearID(team, role) == value)
		{
			return;
		}
		switch (role)
		{
		case PlayerRole.Attacker:
			switch (team)
			{
			case PlayerTeam.Blue:
				HeadgearIDBlueAttacker = value;
				SaveManager.SetInt("headgearIDBlueAttacker", HeadgearIDBlueAttacker);
				break;
			case PlayerTeam.Red:
				HeadgearIDRedAttacker = value;
				SaveManager.SetInt("headgearIDRedAttacker", HeadgearIDRedAttacker);
				break;
			}
			break;
		case PlayerRole.Goalie:
			switch (team)
			{
			case PlayerTeam.Blue:
				HeadgearIDBlueGoalie = value;
				SaveManager.SetInt("headgearIDBlueGoalie", HeadgearIDBlueGoalie);
				break;
			case PlayerTeam.Red:
				HeadgearIDRedGoalie = value;
				SaveManager.SetInt("headgearIDRedGoalie", HeadgearIDRedGoalie);
				break;
			}
			break;
		}
		EventManager.TriggerEvent("Event_OnHeadgearIDChanged", new Dictionary<string, object>
		{
			{ "team", team },
			{ "role", role },
			{ "value", value }
		});
	}

	public static void UpdateMustacheID(int value)
	{
		if (MustacheID != value)
		{
			MustacheID = value;
			SaveManager.SetInt("mustacheID", MustacheID);
			EventManager.TriggerEvent("Event_OnMustacheIDChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateBeardID(int value)
	{
		if (BeardID != value)
		{
			BeardID = value;
			SaveManager.SetInt("beardID", BeardID);
			EventManager.TriggerEvent("Event_OnBeardIDChanged", new Dictionary<string, object> { { "value", value } });
		}
	}

	public static void UpdateJerseyID(PlayerTeam team, PlayerRole role, int value)
	{
		if (GetJerseyID(team, role) == value)
		{
			return;
		}
		switch (role)
		{
		case PlayerRole.Attacker:
			switch (team)
			{
			case PlayerTeam.Blue:
				JerseyIDBlueAttacker = value;
				SaveManager.SetInt("jerseyIDBlueAttacker", JerseyIDBlueAttacker);
				break;
			case PlayerTeam.Red:
				JerseyIDRedAttacker = value;
				SaveManager.SetInt("jerseyIDRedAttacker", JerseyIDRedAttacker);
				break;
			}
			break;
		case PlayerRole.Goalie:
			switch (team)
			{
			case PlayerTeam.Blue:
				JerseyIDBlueGoalie = value;
				SaveManager.SetInt("jerseyIDBlueGoalie", JerseyIDBlueGoalie);
				break;
			case PlayerTeam.Red:
				JerseyIDRedGoalie = value;
				SaveManager.SetInt("jerseyIDRedGoalie", JerseyIDRedGoalie);
				break;
			}
			break;
		}
		EventManager.TriggerEvent("Event_OnJerseyIDChanged", new Dictionary<string, object>
		{
			{ "team", team },
			{ "role", role },
			{ "value", value }
		});
	}

	public static void UpdateStickSkinID(PlayerTeam team, PlayerRole role, int value)
	{
		if (GetStickSkinID(team, role) == value)
		{
			return;
		}
		switch (role)
		{
		case PlayerRole.Attacker:
			switch (team)
			{
			case PlayerTeam.Blue:
				StickSkinIDBlueAttacker = value;
				SaveManager.SetInt("stickSkinIDBlueAttacker", StickSkinIDBlueAttacker);
				break;
			case PlayerTeam.Red:
				StickSkinIDRedAttacker = value;
				SaveManager.SetInt("stickSkinIDRedAttacker", StickSkinIDRedAttacker);
				break;
			}
			break;
		case PlayerRole.Goalie:
			switch (team)
			{
			case PlayerTeam.Blue:
				StickSkinIDBlueGoalie = value;
				SaveManager.SetInt("stickSkinIDBlueGoalie", StickSkinIDBlueGoalie);
				break;
			case PlayerTeam.Red:
				StickSkinIDRedGoalie = value;
				SaveManager.SetInt("stickSkinIDRedGoalie", StickSkinIDRedGoalie);
				break;
			}
			break;
		}
		EventManager.TriggerEvent("Event_OnStickSkinIDChanged", new Dictionary<string, object>
		{
			{ "team", team },
			{ "role", role },
			{ "value", value }
		});
	}

	public static void UpdateStickShaftTapeID(PlayerTeam team, PlayerRole role, int value)
	{
		if (GetStickShaftTapeID(team, role) == value)
		{
			return;
		}
		switch (role)
		{
		case PlayerRole.Attacker:
			switch (team)
			{
			case PlayerTeam.Blue:
				StickShaftTapeIDBlueAttacker = value;
				SaveManager.SetInt("stickShaftTapeIDBlueAttacker", StickShaftTapeIDBlueAttacker);
				break;
			case PlayerTeam.Red:
				StickShaftTapeIDRedAttacker = value;
				SaveManager.SetInt("stickShaftTapeIDRedAttacker", StickShaftTapeIDRedAttacker);
				break;
			}
			break;
		case PlayerRole.Goalie:
			switch (team)
			{
			case PlayerTeam.Blue:
				StickShaftTapeIDBlueGoalie = value;
				SaveManager.SetInt("stickShaftTapeIDBlueGoalie", StickShaftTapeIDBlueGoalie);
				break;
			case PlayerTeam.Red:
				StickShaftTapeIDRedGoalie = value;
				SaveManager.SetInt("stickShaftTapeIDRedGoalie", StickShaftTapeIDRedGoalie);
				break;
			}
			break;
		}
		EventManager.TriggerEvent("Event_OnStickShaftTapeIDChanged", new Dictionary<string, object>
		{
			{ "team", team },
			{ "role", role },
			{ "value", value }
		});
	}

	public static void UpdateStickBladeTapeID(PlayerTeam team, PlayerRole role, int value)
	{
		if (GetStickBladeTapeID(team, role) == value)
		{
			return;
		}
		switch (role)
		{
		case PlayerRole.Attacker:
			switch (team)
			{
			case PlayerTeam.Blue:
				StickBladeTapeIDBlueAttacker = value;
				SaveManager.SetInt("stickBladeTapeIDBlueAttacker", StickBladeTapeIDBlueAttacker);
				break;
			case PlayerTeam.Red:
				StickBladeTapeIDRedAttacker = value;
				SaveManager.SetInt("stickBladeTapeIDRedAttacker", StickBladeTapeIDRedAttacker);
				break;
			}
			break;
		case PlayerRole.Goalie:
			switch (team)
			{
			case PlayerTeam.Blue:
				StickBladeTapeIDBlueGoalie = value;
				SaveManager.SetInt("stickBladeTapeIDBlueGoalie", StickBladeTapeIDBlueGoalie);
				break;
			case PlayerTeam.Red:
				StickBladeTapeIDRedGoalie = value;
				SaveManager.SetInt("stickBladeTapeIDRedGoalie", StickBladeTapeIDRedGoalie);
				break;
			}
			break;
		}
		EventManager.TriggerEvent("Event_OnStickBladeTapeIDChanged", new Dictionary<string, object>
		{
			{ "team", team },
			{ "role", role },
			{ "value", value }
		});
	}
}
