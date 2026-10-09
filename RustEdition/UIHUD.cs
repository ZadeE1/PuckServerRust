using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UIElements;

public class UIHUD : UIView
{
	private ProgressBar staminaProgressBar;

	private VisualElement speed;

	private Label speedLabel;

	private Label unitsLabel;

	private VisualElement teamColorBar;

	private VisualElement teamIndicator;

	private Label teamIndicatorLabel;

	private Label teamIndicatorPosition;

	private VisualElement voiceTransmitIndicator;

	private VisualElement voiceTransmitFill;

	public void Initialize(VisualElement rootVisualElement)
	{
		View = rootVisualElement.Query<VisualElement>("HUDView");
		staminaProgressBar = View.Query<ProgressBar>("StaminaProgressBar");
		teamColorBar = View.Query<VisualElement>("TeamColorBar");
		teamIndicator = View.Query<VisualElement>("TeamIndicator");
		teamIndicatorLabel = teamIndicator.Query<Label>("TeamIndicatorLabel");
		teamIndicatorPosition = teamIndicator.Query<Label>("TeamIndicatorPosition");
		speed = View.Query<VisualElement>("Speed");
		speedLabel = speed.Query<Label>("SpeedLabel");
		unitsLabel = speed.Query<Label>("UnitsLabel");
		voiceTransmitIndicator = View.Query<VisualElement>("VoiceTransmitIndicator");
		voiceTransmitFill = voiceTransmitIndicator.Query<VisualElement>("VoiceTransmitFill");
	}

	public void SetVoiceTransmitting(bool value)
	{
		voiceTransmitIndicator.style.display = ((!value) ? DisplayStyle.None : DisplayStyle.Flex);
	}

	public void SetVoiceLevel(float level)
	{
		voiceTransmitFill.style.height = Length.Percent(Mathf.Clamp01(level) * 100f);
	}

	public void SetStamina(float value)
	{
		staminaProgressBar.EnableInClassList("warning", value < 0.25f);
		staminaProgressBar.value = value;
	}

	public void SetSpeed(float value)
	{
		float num = (float)Math.Round((SettingsManager.Units == Units.Metric) ? Utils.GameUnitsToMetric(value) : Utils.GameUnitsToImperial(value), 1);
		speedLabel.text = num.ToString("F1", CultureInfo.InvariantCulture);
	}

	public void SetUnits(string units)
	{
		unitsLabel.text = units;
	}

	public void SetTeam(PlayerTeam team)
	{
		UIUtils.SetTeamClass(teamColorBar, team);
		UIUtils.SetTeamClass(teamIndicator, team);
		Label label = teamIndicatorLabel;
		label.text = team switch
		{
			PlayerTeam.Blue => "BLUE", 
			PlayerTeam.Red => "RED", 
			_ => string.Empty, 
		};
	}

	public void SetPosition(string positionName)
	{
		teamIndicatorPosition.text = positionName;
	}
}
