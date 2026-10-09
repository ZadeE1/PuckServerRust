using System;
using System.Collections.Generic;
using DG.Tweening;
using UI;
using UnityEngine;
using UnityEngine.UIElements;

public class UISettings : UIView
{
	private VisualElement settings;

	private TabView tabView;

	private Tab audioTab;

	public Action OnActiveTabChanged;

	private VisualElement micTestFill;

	private IconButton closeIconButton;

	private Slider cameraAngleSlider;

	private DropdownField handednessDropdown;

	private Toggle showPuckSilhouetteToggle;

	private Toggle showTeamColorBarToggle;

	private Toggle showPuckOutlineToggle;

	private Toggle showPuckElevationToggle;

	private Toggle showPlayerUsernamesToggle;

	private Slider playerUsernamesFadeThresholdSlider;

	private SliderInt maxMatchmakingPingSliderInt;

	private DropdownField networkBufferingDropdown;

	private Toggle filterChatProfanityToggle;

	private DropdownField unitsDropdown;

	private Toggle showGameUserInterfaceToggle;

	private Slider userInterfaceScaleSlider;

	private Slider chatOpacitySlider;

	private Slider chatScaleSlider;

	private Slider minimapOpacitySlider;

	private Slider minimapBackgroundOpacitySlider;

	private Slider minimapHorizontalPositionSlider;

	private Slider minimapVerticalPositionSlider;

	private Slider minimapScaleSlider;

	private Toggle showMinimapSticksToggle;

	private Toggle showMinimapPuckElevationToggle;

	private Toggle showMinimapFallenIndicatorToggle;

	private Slider globalStickSensitivitySlider;

	private Slider horizontalStickSensitivitySlider;

	private Slider verticalStickSensitivitySlider;

	private Slider lookSensitivitySlider;

	private Dictionary<string, KeyBindField> actionNameKeyBindFieldMap;

	private Slider globalVolumeSlider;

	private Slider ambientVolumeSlider;

	private Slider gameVolumeSlider;

	private Slider voiceVolumeSlider;

	private Slider uiVolumeSlider;

	private DropdownField fullScreenModeDropdown;

	private DropdownField displayDropdown;

	private DropdownField resolutionDropdown;

	private Toggle vSyncToggle;

	private Slider fpsLimitSlider;

	private Slider fovSlider;

	private DropdownField qualityDropdown;

	private DropdownField shadowQualityDropdown;

	private Toggle motionBlurToggle;

	private Button resetToDefaultButton;

	private Tween debounceTween;

	public bool IsAudioTabActive
	{
		get
		{
			if (tabView != null)
			{
				return tabView.activeTab == audioTab;
			}
			return false;
		}
	}

	public void Initialize(VisualElement rootVisualElement)
	{
		View = rootVisualElement.Query<VisualElement>("SettingsView");
		settings = View.Query<VisualElement>("Settings");
		tabView = settings.Query<TabView>();
		audioTab = settings.Query<Tab>("AudioTab");
		tabView.activeTabChanged += OnActiveTabChangedInternal;
		micTestFill = settings.Query<VisualElement>("MicTestFill");
		closeIconButton = settings.Query<TemplateContainer>("CloseIconButtonContainer").First().Query<IconButton>();
		closeIconButton.clicked += OnClickClose;
		cameraAngleSlider = settings.Query<VisualElement>("CameraAngleSlider").First().Query<Slider>();
		cameraAngleSlider.value = SettingsManager.CameraAngle;
		cameraAngleSlider.RegisterValueChangedCallback(OnCameraAngleChanged);
		handednessDropdown = settings.Query<VisualElement>("HandednessDropdown").First().Query<DropdownField>();
		handednessDropdown.value = Utils.GetNameFromHandedness(SettingsManager.Handedness);
		handednessDropdown.RegisterValueChangedCallback(OnHandednessChanged);
		showPuckSilhouetteToggle = settings.Query<VisualElement>("ShowPuckSilhouetteToggle").First().Query<Toggle>();
		showPuckSilhouetteToggle.value = SettingsManager.ShowPuckSilhouette;
		showPuckSilhouetteToggle.RegisterValueChangedCallback(OnShowPuckSilhouetteChanged);
		showTeamColorBarToggle = settings.Query<VisualElement>("ShowTeamColorBarToggle").First().Query<Toggle>();
		showTeamColorBarToggle.value = SettingsManager.ShowTeamColorBar;
		showTeamColorBarToggle.RegisterValueChangedCallback(OnShowTeamColorBarChanged);
		showPuckOutlineToggle = settings.Query<VisualElement>("ShowPuckOutlineToggle").First().Query<Toggle>();
		showPuckOutlineToggle.value = SettingsManager.ShowPuckOutline;
		showPuckOutlineToggle.RegisterValueChangedCallback(OnShowPuckOutlineChanged);
		showPuckElevationToggle = settings.Query<VisualElement>("ShowPuckElevationToggle").First().Query<Toggle>();
		showPuckElevationToggle.value = SettingsManager.ShowPuckElevation;
		showPuckElevationToggle.RegisterValueChangedCallback(OnShowPuckEleveationChanged);
		showPlayerUsernamesToggle = settings.Query<VisualElement>("ShowPlayerUsernamesToggle").First().Query<Toggle>();
		showPlayerUsernamesToggle.value = SettingsManager.ShowPlayerUsernames;
		showPlayerUsernamesToggle.RegisterValueChangedCallback(OnShowPlayerUsernamesChanged);
		playerUsernamesFadeThresholdSlider = settings.Query<VisualElement>("PlayerUsernamesFadeThresholdSlider").First().Query<Slider>();
		playerUsernamesFadeThresholdSlider.value = SettingsManager.PlayerUsernamesFadeThreshold;
		playerUsernamesFadeThresholdSlider.RegisterValueChangedCallback(OnPlayerUsernamesFadeThresholdChanged);
		maxMatchmakingPingSliderInt = settings.Query<VisualElement>("MaxMatchmakingPingSliderInt").First().Query<SliderInt>();
		maxMatchmakingPingSliderInt.value = SettingsManager.MaxMatchmakingPing;
		maxMatchmakingPingSliderInt.RegisterValueChangedCallback(OnMaxMatchmakingPingChanged);
		networkBufferingDropdown = settings.Query<VisualElement>("NetworkBufferingDropdown").First().Query<DropdownField>();
		networkBufferingDropdown.value = Utils.GetNameFromNetworkBuffering(SettingsManager.NetworkBuffering);
		networkBufferingDropdown.RegisterValueChangedCallback(OnNetworkBufferingChanged);
		filterChatProfanityToggle = settings.Query<VisualElement>("FilterChatProfanityToggle").First().Query<Toggle>();
		filterChatProfanityToggle.value = SettingsManager.FilterChatProfanity;
		filterChatProfanityToggle.RegisterValueChangedCallback(OnFilterChatProfanityChanged);
		unitsDropdown = settings.Query<VisualElement>("UnitsDropdown").First().Query<DropdownField>();
		unitsDropdown.value = Utils.GetNameFromUnits(SettingsManager.Units);
		unitsDropdown.RegisterValueChangedCallback(OnUnitsChanged);
		showGameUserInterfaceToggle = settings.Query<VisualElement>("ShowGameUserInterfaceToggle").First().Query<Toggle>();
		showGameUserInterfaceToggle.value = SettingsManager.ShowGameUserInterface;
		showGameUserInterfaceToggle.RegisterValueChangedCallback(OnShowGameUserInterfaceChanged);
		userInterfaceScaleSlider = settings.Query<VisualElement>("UserInterfaceScaleSlider").First().Query<Slider>();
		userInterfaceScaleSlider.value = SettingsManager.UserInterfaceScale;
		userInterfaceScaleSlider.RegisterValueChangedCallback(OnUserInterfaceScaleChanged);
		chatOpacitySlider = settings.Query<VisualElement>("ChatOpacitySlider").First().Query<Slider>();
		chatOpacitySlider.value = SettingsManager.ChatOpacity;
		chatOpacitySlider.RegisterValueChangedCallback(OnChatOpacityChanged);
		chatScaleSlider = settings.Query<VisualElement>("ChatScaleSlider").First().Query<Slider>();
		chatScaleSlider.value = SettingsManager.ChatScale;
		chatScaleSlider.RegisterValueChangedCallback(OnChatScaleChanged);
		minimapOpacitySlider = settings.Query<VisualElement>("MinimapOpacitySlider").First().Query<Slider>();
		minimapOpacitySlider.value = SettingsManager.MinimapOpacity;
		minimapOpacitySlider.RegisterValueChangedCallback(OnMinimapOpacityChanged);
		minimapBackgroundOpacitySlider = settings.Query<VisualElement>("MinimapBackgroundOpacitySlider").First().Query<Slider>();
		minimapBackgroundOpacitySlider.value = SettingsManager.MinimapBackgroundOpacity;
		minimapBackgroundOpacitySlider.RegisterValueChangedCallback(OnMinimapBackgroundOpacityChanged);
		minimapHorizontalPositionSlider = settings.Query<VisualElement>("MinimapHorizontalPositionSlider").First().Query<Slider>();
		minimapHorizontalPositionSlider.value = SettingsManager.MinimapHorizontalPosition;
		minimapHorizontalPositionSlider.RegisterValueChangedCallback(OnMinimapHorizontalPositionChanged);
		minimapVerticalPositionSlider = settings.Query<VisualElement>("MinimapVerticalPositionSlider").First().Query<Slider>();
		minimapVerticalPositionSlider.value = SettingsManager.MinimapVerticalPosition;
		minimapVerticalPositionSlider.RegisterValueChangedCallback(OnMinimapVerticalPositionChanged);
		minimapScaleSlider = settings.Query<VisualElement>("MinimapScaleSlider").First().Query<Slider>();
		minimapScaleSlider.value = SettingsManager.MinimapScale;
		minimapScaleSlider.RegisterValueChangedCallback(OnMinimapScaleChanged);
		showMinimapSticksToggle = settings.Query<VisualElement>("ShowMinimapSticksToggle").First().Query<Toggle>();
		showMinimapSticksToggle.value = SettingsManager.ShowMinimapSticks;
		showMinimapSticksToggle.RegisterValueChangedCallback(OnShowMinimapSticksChanged);
		showMinimapPuckElevationToggle = settings.Query<VisualElement>("ShowMinimapPuckElevationToggle").First().Query<Toggle>();
		showMinimapPuckElevationToggle.value = SettingsManager.ShowMinimapPuckElevation;
		showMinimapPuckElevationToggle.RegisterValueChangedCallback(OnShowMinimapPuckElevationChanged);
		showMinimapFallenIndicatorToggle = settings.Query<VisualElement>("ShowMinimapFallenIndicatorToggle").First().Query<Toggle>();
		showMinimapFallenIndicatorToggle.value = SettingsManager.ShowMinimapFallenIndicator;
		showMinimapFallenIndicatorToggle.RegisterValueChangedCallback(OnShowMinimapFallenIndicatorChanged);
		globalStickSensitivitySlider = settings.Query<VisualElement>("GlobalStickSensitivitySlider").First().Query<Slider>();
		globalStickSensitivitySlider.value = SettingsManager.GlobalStickSensitivity;
		globalStickSensitivitySlider.RegisterValueChangedCallback(OnGlobalStickSensitivityChanged);
		horizontalStickSensitivitySlider = settings.Query<VisualElement>("HorizontalStickSensitivitySlider").First().Query<Slider>();
		horizontalStickSensitivitySlider.value = SettingsManager.HorizontalStickSensitivity;
		horizontalStickSensitivitySlider.RegisterValueChangedCallback(OnHorizontalStickSensitivityChanged);
		verticalStickSensitivitySlider = settings.Query<VisualElement>("VerticalStickSensitivitySlider").First().Query<Slider>();
		verticalStickSensitivitySlider.value = SettingsManager.VerticalStickSensitivity;
		verticalStickSensitivitySlider.RegisterValueChangedCallback(OnVerticalStickSensitivityChanged);
		lookSensitivitySlider = settings.Query<VisualElement>("LookSensitivitySlider").First().Query<Slider>();
		lookSensitivitySlider.value = SettingsManager.LookSensitivity;
		lookSensitivitySlider.RegisterValueChangedCallback(OnLookSensitivityChanged);
		actionNameKeyBindFieldMap = new Dictionary<string, KeyBindField>
		{
			{
				"Move Forward",
				settings.Query<VisualElement>("MoveForwardKeyBindInput").First().Query<KeyBindField>()
			},
			{
				"Move Backward",
				settings.Query<VisualElement>("MoveBackwardKeyBindInput").First().Query<KeyBindField>()
			},
			{
				"Turn Left",
				settings.Query<VisualElement>("TurnLeftKeyBindInput").First().Query<KeyBindField>()
			},
			{
				"Turn Right",
				settings.Query<VisualElement>("TurnRightKeyBindInput").First().Query<KeyBindField>()
			},
			{
				"Blade Angle Up",
				settings.Query<VisualElement>("BladeAngleUpKeyBindInput").First().Query<KeyBindField>()
			},
			{
				"Blade Angle Down",
				settings.Query<VisualElement>("BladeAngleDownKeyBindInput").First().Query<KeyBindField>()
			},
			{
				"Slide",
				settings.Query<VisualElement>("SlideKeyBindInput").First().Query<KeyBindField>()
			},
			{
				"Sprint",
				settings.Query<VisualElement>("SprintKeyBindInput").First().Query<KeyBindField>()
			},
			{
				"Track",
				settings.Query<VisualElement>("TrackKeyBindInput").First().Query<KeyBindField>()
			},
			{
				"Look",
				settings.Query<VisualElement>("LookKeyBindInput").First().Query<KeyBindField>()
			},
			{
				"Jump",
				settings.Query<VisualElement>("JumpKeyBindInput").First().Query<KeyBindField>()
			},
			{
				"Stop",
				settings.Query<VisualElement>("StopKeyBindInput").First().Query<KeyBindField>()
			},
			{
				"Twist Left",
				settings.Query<VisualElement>("TwistLeftKeyBindInput").First().Query<KeyBindField>()
			},
			{
				"Twist Right",
				settings.Query<VisualElement>("TwistRightKeyBindInput").First().Query<KeyBindField>()
			},
			{
				"Dash Left",
				settings.Query<VisualElement>("DashLeftKeyBindInput").First().Query<KeyBindField>()
			},
			{
				"Dash Right",
				settings.Query<VisualElement>("DashRightKeyBindInput").First().Query<KeyBindField>()
			},
			{
				"Extend Left",
				settings.Query<VisualElement>("ExtendLeftKeyBindInput").First().Query<KeyBindField>()
			},
			{
				"Extend Right",
				settings.Query<VisualElement>("ExtendRightKeyBindInput").First().Query<KeyBindField>()
			},
			{
				"Lateral Left",
				settings.Query<VisualElement>("LateralLeftKeyBindInput").First().Query<KeyBindField>()
			},
			{
				"Lateral Right",
				settings.Query<VisualElement>("LateralRightKeyBindInput").First().Query<KeyBindField>()
			},
			{
				"Talk",
				settings.Query<VisualElement>("TalkKeyBindInput").First().Query<KeyBindField>()
			},
			{
				"All Chat",
				settings.Query<VisualElement>("AllChatKeyBindInput").First().Query<KeyBindField>()
			},
			{
				"Team Chat",
				settings.Query<VisualElement>("TeamChatKeyBindInput").First().Query<KeyBindField>()
			},
			{
				"Position Select",
				settings.Query<VisualElement>("PositionSelectKeyBindInput").First().Query<KeyBindField>()
			},
			{
				"Scoreboard",
				settings.Query<VisualElement>("ScoreboardKeyBindInput").First().Query<KeyBindField>()
			},
			{
				"Debug 1",
				settings.Query<VisualElement>("DebugOverlayKeyBindInput").First().Query<KeyBindField>()
			}
		};
		UpdateKeyBindInputs(InputManager.KeyBinds);
		foreach (KeyValuePair<string, KeyBindField> item in actionNameKeyBindFieldMap)
		{
			string actionName = item.Key;
			KeyBindField value = item.Value;
			value.Click = () =>
			{
				OnKeyBindInputClicked(actionName);
			};
			value.InteractionChange = (KeyBindInteraction interaction) =>
			{
				OnKeyBindInputInteractionChanged(actionName, interaction);
			};
		}
		globalVolumeSlider = settings.Query<VisualElement>("GlobalVolumeSlider").First().Query<Slider>();
		globalVolumeSlider.value = SettingsManager.GlobalVolume;
		globalVolumeSlider.RegisterValueChangedCallback(OnGlobalVolumeChanged);
		ambientVolumeSlider = settings.Query<VisualElement>("AmbientVolumeSlider").First().Query<Slider>();
		ambientVolumeSlider.value = SettingsManager.AmbientVolume;
		ambientVolumeSlider.RegisterValueChangedCallback(OnAmbientVolumeChanged);
		gameVolumeSlider = settings.Query<VisualElement>("GameVolumeSlider").First().Query<Slider>();
		gameVolumeSlider.value = SettingsManager.GameVolume;
		gameVolumeSlider.RegisterValueChangedCallback(OnGameVolumeChanged);
		voiceVolumeSlider = settings.Query<VisualElement>("VoiceVolumeSlider").First().Query<Slider>();
		voiceVolumeSlider.value = SettingsManager.VoiceVolume;
		voiceVolumeSlider.RegisterValueChangedCallback(OnVoiceVolumeChanged);
		uiVolumeSlider = settings.Query<VisualElement>("UIVolumeSlider").First().Query<Slider>();
		uiVolumeSlider.value = SettingsManager.UIVolume;
		uiVolumeSlider.RegisterValueChangedCallback(OnUIVolumeChanged);
		fullScreenModeDropdown = settings.Query<VisualElement>("FullScreenModeDropdown").First().Query<DropdownField>();
		fullScreenModeDropdown.choices = Utils.GetFullScreenModeNames();
		fullScreenModeDropdown.value = Utils.GetNameFromFullScreenMode(SettingsManager.FullScreenMode);
		fullScreenModeDropdown.RegisterValueChangedCallback(OnFullScreenModeChanged);
		displayDropdown = settings.Query<VisualElement>("DisplayDropdown").First().Query<DropdownField>();
		displayDropdown.choices = Utils.GetDisplayNames();
		displayDropdown.value = Utils.GetDisplayNameFromIndex(SettingsManager.DisplayIndex);
		displayDropdown.RegisterValueChangedCallback(OnDisplayChanged);
		resolutionDropdown = settings.Query<VisualElement>("ResolutionDropdown").First().Query<DropdownField>();
		resolutionDropdown.choices = Utils.GetResolutionNames();
		resolutionDropdown.value = Utils.GetResolutionNameFromIndex(SettingsManager.ResolutionIndex);
		resolutionDropdown.RegisterValueChangedCallback(OnResolutionChanged);
		vSyncToggle = settings.Query<VisualElement>("VSyncToggle").First().Query<Toggle>();
		vSyncToggle.value = SettingsManager.VSync;
		vSyncToggle.RegisterValueChangedCallback(OnVSyncChanged);
		fpsLimitSlider = settings.Query<VisualElement>("FPSLimitSlider").First().Query<Slider>();
		fpsLimitSlider.value = SettingsManager.FpsLimit;
		fpsLimitSlider.RegisterValueChangedCallback(OnFpsLimitChanged);
		fovSlider = settings.Query<VisualElement>("FOVSlider").First().Query<Slider>();
		fovSlider.value = SettingsManager.Fov;
		fovSlider.RegisterValueChangedCallback(OnFovChanged);
		qualityDropdown = settings.Query<VisualElement>("QualityDropdown").First().Query<DropdownField>();
		qualityDropdown.choices = Utils.GetApplicationQualityNames();
		qualityDropdown.value = Utils.GetNameFromApplicationQuality(SettingsManager.Quality);
		qualityDropdown.RegisterValueChangedCallback(OnQualityChanged);
		shadowQualityDropdown = settings.Query<VisualElement>("ShadowQualityDropdown").First().Query<DropdownField>();
		shadowQualityDropdown.choices = Utils.GetShadowQualityNames();
		shadowQualityDropdown.value = Utils.GetNameFromShadowQuality(SettingsManager.ShadowQuality);
		shadowQualityDropdown.RegisterValueChangedCallback(OnShadowQualityChanged);
		motionBlurToggle = settings.Query<VisualElement>("MotionBlurToggle").First().Query<Toggle>();
		motionBlurToggle.value = SettingsManager.MotionBlur;
		motionBlurToggle.RegisterValueChangedCallback(OnMotionBlurChanged);
		resetToDefaultButton = settings.Query<Button>("ResetToDefaultButton");
		resetToDefaultButton.clicked += OnClickResetToDefault;
	}

	private void OnClickClose()
	{
		EventManager.TriggerEvent("Event_OnSettingsClickClose");
	}

	private void OnClickResetToDefault()
	{
		EventManager.TriggerEvent("Event_OnSettingsClickResetToDefault");
	}

	public void UpdateKeyBindInputs(Dictionary<string, KeyBind> keyBinds)
	{
		foreach (KeyValuePair<string, KeyBind> keyBind in keyBinds)
		{
			string key = keyBind.Key;
			KeyBind value = keyBind.Value;
			if (actionNameKeyBindFieldMap.ContainsKey(key))
			{
				KeyBindField keyBindField = actionNameKeyBindFieldMap[key];
				keyBindField.Path = InputManager.GetKeyBindDisplayString(value);
				KeyBindInteraction keyBindInteractionFromInteraction = Utils.GetKeyBindInteractionFromInteraction(value.Interactions, keyBindField.InteractionType);
				keyBindField.Interaction = keyBindInteractionFromInteraction;
			}
		}
	}

	private void OnCameraAngleChanged(ChangeEvent<float> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsCameraAngleChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateCameraAngle(float value)
	{
		cameraAngleSlider.value = value;
	}

	private void OnHandednessChanged(ChangeEvent<string> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsHandednessChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateHandedness(string value)
	{
		handednessDropdown.value = value;
	}

	private void OnShowPuckSilhouetteChanged(ChangeEvent<bool> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsShowPuckSilhouetteChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateShowPuckSilhouette(bool value)
	{
		showPuckSilhouetteToggle.value = value;
	}

	private void OnShowTeamColorBarChanged(ChangeEvent<bool> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsShowTeamColorBarChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateShowTeamColorBar(bool value)
	{
		showTeamColorBarToggle.value = value;
	}

	private void OnShowPuckOutlineChanged(ChangeEvent<bool> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsShowPuckOutlineChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateShowPuckOutline(bool value)
	{
		showPuckOutlineToggle.value = value;
	}

	private void OnShowPuckEleveationChanged(ChangeEvent<bool> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsShowPuckElevationChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateShowPuckElevation(bool value)
	{
		showPuckElevationToggle.value = value;
	}

	private void OnShowMinimapSticksChanged(ChangeEvent<bool> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsShowMinimapSticksChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateShowMinimapSticks(bool value)
	{
		showMinimapSticksToggle.value = value;
	}

	private void OnShowMinimapPuckElevationChanged(ChangeEvent<bool> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsShowMinimapPuckElevationChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateShowMinimapPuckElevation(bool value)
	{
		showMinimapPuckElevationToggle.value = value;
	}

	private void OnShowMinimapFallenIndicatorChanged(ChangeEvent<bool> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsShowMinimapFallenIndicatorChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateShowMinimapFallenIndicator(bool value)
	{
		showMinimapFallenIndicatorToggle.value = value;
	}

	private void OnShowPlayerUsernamesChanged(ChangeEvent<bool> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsShowPlayerUsernamesChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateShowPlayerUsernames(bool value)
	{
		showPlayerUsernamesToggle.value = value;
	}

	private void OnPlayerUsernamesFadeThresholdChanged(ChangeEvent<float> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsPlayerUsernamesFadeThresholdChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdatePlayerUsernamesFadeThreshold(float value)
	{
		playerUsernamesFadeThresholdSlider.value = value;
	}

	private void OnMaxMatchmakingPingChanged(ChangeEvent<int> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsMaxMatchmakingPingChanged", new Dictionary<string, object> { { "value", maxMatchmakingPingSliderInt.value } });
	}

	public void UpdateMaxMatchmakingPing(int value)
	{
		maxMatchmakingPingSliderInt.value = value;
	}

	private void OnNetworkBufferingChanged(ChangeEvent<string> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsNetworkBufferingChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateNetworkBuffering(string value)
	{
		networkBufferingDropdown.value = value;
	}

	private void OnFilterChatProfanityChanged(ChangeEvent<bool> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsFilterChatProfanityChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateFilterChatProfanity(bool value)
	{
		filterChatProfanityToggle.value = value;
	}

	private void OnUnitsChanged(ChangeEvent<string> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsUnitsChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateUnits(string value)
	{
		unitsDropdown.value = value;
	}

	private void OnShowGameUserInterfaceChanged(ChangeEvent<bool> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsShowGameUserInterfaceChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateShowGameUserInterface(bool value)
	{
		showGameUserInterfaceToggle.value = value;
	}

	private void OnUserInterfaceScaleChanged(ChangeEvent<float> changeEvent)
	{
		float newValue = changeEvent.newValue;
		debounceTween?.Kill();
		debounceTween = DOVirtual.DelayedCall(1f, () =>
		{
			EventManager.TriggerEvent("Event_OnSettingsUserInterfaceScaleChanged", new Dictionary<string, object> { { "value", newValue } });
		});
	}

	public void UpdateUserInterfaceScale(float value)
	{
		userInterfaceScaleSlider.value = value;
	}

	private void OnChatOpacityChanged(ChangeEvent<float> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsChatOpacityChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateChatOpacity(float value)
	{
		chatOpacitySlider.value = value;
	}

	private void OnChatScaleChanged(ChangeEvent<float> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsChatScaleChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateChatScale(float value)
	{
		chatScaleSlider.value = value;
	}

	private void OnMinimapOpacityChanged(ChangeEvent<float> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsMinimapOpacityChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateMinimapOpacity(float value)
	{
		minimapOpacitySlider.value = value;
	}

	private void OnMinimapBackgroundOpacityChanged(ChangeEvent<float> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsMinimapBackgroundOpacityChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateMinimapBackgroundOpacity(float value)
	{
		minimapBackgroundOpacitySlider.value = value;
	}

	private void OnMinimapHorizontalPositionChanged(ChangeEvent<float> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsMinimapHorizontalPositionChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateMinimapHorizontalPosition(float value)
	{
		minimapHorizontalPositionSlider.value = value;
	}

	private void OnMinimapVerticalPositionChanged(ChangeEvent<float> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsMinimapVerticalPositionChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateMinimapVerticalPosition(float value)
	{
		minimapVerticalPositionSlider.value = value;
	}

	private void OnMinimapScaleChanged(ChangeEvent<float> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsMinimapScaleChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateMinimapScale(float value)
	{
		minimapScaleSlider.value = value;
	}

	private void OnGlobalStickSensitivityChanged(ChangeEvent<float> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsGlobalStickSensitivityChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateGlobalStickSensitivity(float value)
	{
		globalStickSensitivitySlider.value = value;
	}

	private void OnHorizontalStickSensitivityChanged(ChangeEvent<float> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsHorizontalStickSensitivityChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateHorizontalStickSensitivity(float value)
	{
		horizontalStickSensitivitySlider.value = value;
	}

	private void OnVerticalStickSensitivityChanged(ChangeEvent<float> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsVerticalStickSensitivityChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateVerticalStickSensitivity(float value)
	{
		verticalStickSensitivitySlider.value = value;
	}

	private void OnKeyBindInputClicked(string actionName)
	{
		EventManager.TriggerEvent("Event_OnSettingsKeyBindInputClicked", new Dictionary<string, object> { { "actionName", actionName } });
	}

	private void OnKeyBindInputInteractionChanged(string actionName, KeyBindInteraction interaction)
	{
		EventManager.TriggerEvent("Event_OnSettingsKeyBindInputInteractionChanged", new Dictionary<string, object>
		{
			{ "actionName", actionName },
			{ "interaction", interaction }
		});
	}

	private void OnLookSensitivityChanged(ChangeEvent<float> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsLookSensitivityChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateLookSensitivity(float value)
	{
		lookSensitivitySlider.value = value;
	}

	private void OnGlobalVolumeChanged(ChangeEvent<float> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsGlobalVolumeChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateGlobalVolume(float value)
	{
		globalVolumeSlider.value = value;
	}

	private void OnAmbientVolumeChanged(ChangeEvent<float> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsAmbientVolumeChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateAmbientVolume(float value)
	{
		ambientVolumeSlider.value = value;
	}

	private void OnGameVolumeChanged(ChangeEvent<float> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsGameVolumeChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateGameVolume(float value)
	{
		gameVolumeSlider.value = value;
	}

	private void OnVoiceVolumeChanged(ChangeEvent<float> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsVoiceVolumeChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateVoiceVolume(float value)
	{
		voiceVolumeSlider.value = value;
	}

	private void OnUIVolumeChanged(ChangeEvent<float> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsUIVolumeChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateUIVolume(float value)
	{
		uiVolumeSlider.value = value;
	}

	public void SetMicTestLevel(float level)
	{
		micTestFill.style.width = Length.Percent(Mathf.Clamp01(level) * 100f);
	}

	private void OnActiveTabChangedInternal(Tab previous, Tab current)
	{
		OnActiveTabChanged?.Invoke();
	}

	private void OnFullScreenModeChanged(ChangeEvent<string> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsFullScreenModeChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateFullScreenMode(string value)
	{
		fullScreenModeDropdown.value = value;
	}

	private void OnDisplayChanged(ChangeEvent<string> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsDisplayChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateDisplay(string value)
	{
		displayDropdown.value = value;
	}

	public void UpdateDisplayChoices(List<string> choices)
	{
		displayDropdown.choices = choices;
	}

	private void OnResolutionChanged(ChangeEvent<string> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsResolutionChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateResolution(string value)
	{
		resolutionDropdown.value = value;
	}

	public void UpdateResolutionChoices(List<string> choices)
	{
		resolutionDropdown.choices = choices;
	}

	private void OnVSyncChanged(ChangeEvent<bool> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsVSyncChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateVSync(bool value)
	{
		vSyncToggle.value = value;
	}

	private void OnFpsLimitChanged(ChangeEvent<float> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsFpsLimitChanged", new Dictionary<string, object> { 
		{
			"value",
			(int)changeEvent.newValue
		} });
	}

	public void UpdateFpsLimit(int value)
	{
		fpsLimitSlider.value = value;
	}

	private void OnFovChanged(ChangeEvent<float> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsFovChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateFov(float value)
	{
		fovSlider.value = value;
	}

	private void OnQualityChanged(ChangeEvent<string> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsQualityChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateQuality(string value)
	{
		qualityDropdown.value = value;
	}

	private void OnShadowQualityChanged(ChangeEvent<string> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsShadowQualityChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateShadowQuality(string value)
	{
		shadowQualityDropdown.value = value;
	}

	private void OnMotionBlurChanged(ChangeEvent<bool> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnSettingsMotionBlurChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	public void UpdateMotionBlur(bool value)
	{
		motionBlurToggle.value = value;
	}
}
