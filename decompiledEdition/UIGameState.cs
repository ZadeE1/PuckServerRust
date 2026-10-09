using System;
using UnityEngine.UIElements;

public class UIGameState : UIView
{
	private VisualElement gameState;

	private Label blueScoreLabel;

	private Label redScoreLabel;

	private Label timeLabel;

	private Label phaseLabel;

	public void Initialize(VisualElement rootVisualElement)
	{
		View = rootVisualElement.Query<VisualElement>("GameStateView");
		gameState = View.Query<VisualElement>("GameState");
		blueScoreLabel = gameState.Query<Label>("BlueScoreLabel");
		redScoreLabel = gameState.Query<Label>("RedScoreLabel");
		timeLabel = gameState.Query<Label>("TimeLabel");
		phaseLabel = gameState.Query<Label>("PhaseLabel");
	}

	public override bool Show()
	{
		if (!SettingsManager.ShowGameUserInterface)
		{
			return false;
		}
		return base.Show();
	}

	public void SetScore(PlayerTeam team, int score)
	{
		switch (team)
		{
		case PlayerTeam.Blue:
			blueScoreLabel.text = $"{score}";
			break;
		case PlayerTeam.Red:
			redScoreLabel.text = $"{score}";
			break;
		}
	}

	public void SetTick(int tick)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(tick);
		if (timeSpan.TotalHours < 1.0)
		{
			timeLabel.text = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
		}
		else
		{
			timeLabel.text = $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
		}
	}

	public void SetPhase(string text)
	{
		phaseLabel.text = text;
	}
}
