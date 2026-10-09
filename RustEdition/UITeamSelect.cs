using System.Collections.Generic;
using UnityEngine.UIElements;

public class UITeamSelect : UIView
{
	private VisualElement teamSelect;

	private Button blueButton;

	private Button redButton;

	private Button spectatorButton;

	private Label blueCountLabel;

	private Label redCountLabel;

	public void Initialize(VisualElement rootVisualElement)
	{
		View = rootVisualElement.Query<VisualElement>("TeamSelectView");
		teamSelect = View.Query<VisualElement>("TeamSelect");
		blueButton = teamSelect.Query<Button>("BlueButton");
		blueButton.clicked += OnClickTeamBlue;
		blueCountLabel = blueButton.Query<Label>("CountLabel");
		redButton = teamSelect.Query<Button>("RedButton");
		redButton.clicked += OnClickTeamRed;
		redCountLabel = redButton.Query<Label>("CountLabel");
		spectatorButton = teamSelect.Query<Button>("SpectatorButton");
		spectatorButton.clicked += OnClickTeamSpectator;
	}

	public override bool Show()
	{
		bool flag = base.Show();
		if (flag)
		{
			EventManager.TriggerEvent("Event_OnTeamSelectShow");
		}
		return flag;
	}

	public void StyleTeamCounts(int blueCount, int redCount)
	{
		blueCountLabel.text = FormatPlayerCount(blueCount);
		redCountLabel.text = FormatPlayerCount(redCount);
	}

	private string FormatPlayerCount(int count)
	{
		if (count != 1)
		{
			return $"{count} PLAYERS";
		}
		return "1 PLAYER";
	}

	private void OnClickTeamBlue()
	{
		EventManager.TriggerEvent("Event_OnTeamSelectClickTeam", new Dictionary<string, object> { 
		{
			"team",
			PlayerTeam.Blue
		} });
	}

	private void OnClickTeamRed()
	{
		EventManager.TriggerEvent("Event_OnTeamSelectClickTeam", new Dictionary<string, object> { 
		{
			"team",
			PlayerTeam.Red
		} });
	}

	private void OnClickTeamSpectator()
	{
		EventManager.TriggerEvent("Event_OnTeamSelectClickTeam", new Dictionary<string, object> { 
		{
			"team",
			PlayerTeam.Spectator
		} });
	}
}
