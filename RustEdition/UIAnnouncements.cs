using UnityEngine.UIElements;

public class UIAnnouncements : UIView
{
	private VisualElement announcements;

	private VisualElement score;

	private Label headerLabel;

	private Label goalLabel;

	private Label assistLabel;

	public void Initialize(VisualElement rootVisualElement)
	{
		View = rootVisualElement.Query<VisualElement>("AnnouncementsView");
		announcements = View.Query<VisualElement>("Announcements");
		score = announcements.Query<VisualElement>("Score");
		headerLabel = score.Query<Label>("HeaderLabel");
		goalLabel = score.Query<Label>("GoalLabel");
		assistLabel = score.Query<Label>("AssistLabel");
		HideScore();
	}

	public override bool Show()
	{
		if (!SettingsManager.ShowGameUserInterface)
		{
			return false;
		}
		return base.Show();
	}

	public void ShowScore(PlayerTeam team, Player goalPlayer, Player assistPlayer, Player secondAssistPlayer)
	{
		headerLabel.text = string.Empty;
		goalLabel.text = string.Empty;
		assistLabel.text = string.Empty;
		score.style.display = DisplayStyle.Flex;
		UIUtils.SetTeamClass(score, team);
		switch (team)
		{
		case PlayerTeam.Blue:
			headerLabel.text = "BLUE SCORES!";
			break;
		case PlayerTeam.Red:
			headerLabel.text = "RED SCORES!";
			break;
		}
		if ((bool)goalPlayer)
		{
			goalLabel.text = $"#{goalPlayer.Number.Value} {goalPlayer.Username.Value}";
		}
		if ((bool)assistPlayer)
		{
			assistLabel.text = $"#{assistPlayer.Number.Value} {assistPlayer.Username.Value}";
		}
		if ((bool)secondAssistPlayer)
		{
			assistLabel.text += $" & #{secondAssistPlayer.Number.Value} {secondAssistPlayer.Username.Value}";
		}
	}

	public void HideScore()
	{
		headerLabel.text = string.Empty;
		goalLabel.text = string.Empty;
		assistLabel.text = string.Empty;
		score.style.display = DisplayStyle.None;
	}
}
