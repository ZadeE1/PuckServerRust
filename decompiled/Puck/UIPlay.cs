using System.Collections.Generic;
using UI;
using UnityEngine.UIElements;

public class UIPlay : UIView
{
	private VisualElement play;

	private IconButton closeIconButton;

	private PlayTile threeVsThreeTile;

	private PlayTile fiveVsFiveTile;

	private Toggle threeVsThreeToggle;

	private Toggle fiveVsFiveToggle;

	private Button practiceButton;

	private Button serverBrowserButton;

	private VisualElement statistics;

	private Label playersLabel;

	public void Initialize(VisualElement rootVisualElement)
	{
		View = rootVisualElement.Query<VisualElement>("PlayView");
		play = View.Query<VisualElement>("Play");
		closeIconButton = View.Query<TemplateContainer>("CloseIconButtonContainer").First().Query<IconButton>();
		closeIconButton.clicked += OnClickClose;
		threeVsThreeTile = play.Query<TemplateContainer>("ThreeVsThreePlayTile").First().Query<PlayTile>();
		threeVsThreeToggle = threeVsThreeTile.Query<Toggle>();
		threeVsThreeToggle.RegisterValueChangedCallback(OnPoolSelectionChanged);
		fiveVsFiveTile = play.Query<TemplateContainer>("FiveVsFivePlayTile").First().Query<PlayTile>();
		fiveVsFiveToggle = fiveVsFiveTile.Query<Toggle>();
		fiveVsFiveToggle.RegisterValueChangedCallback(OnPoolSelectionChanged);
		practiceButton = play.Query<TemplateContainer>("PracticePlayTile").First().Query<Button>();
		practiceButton.clicked += OnClickPractice;
		serverBrowserButton = play.Query<TemplateContainer>("ServerBrowserPlayTile").First().Query<Button>();
		serverBrowserButton.clicked += OnClickServerBrowser;
		statistics = play.Query<VisualElement>("Statistics");
		playersLabel = statistics.Query<Label>("PlayersLabel");
	}

	public override bool Show()
	{
		bool flag = base.Show();
		if (flag)
		{
			RefreshPoolSelection();
		}
		return flag;
	}

	public override bool Hide()
	{
		bool flag = base.Hide();
		if (flag)
		{
			RefreshPoolSelection();
		}
		return flag;
	}

	public void SetPoolsAvailable(bool isMatchmakingAvailable, bool threeVsThreeFitsParty, bool fiveVsFiveFitsParty)
	{
		SetPoolAvailable(threeVsThreeTile, threeVsThreeToggle, isMatchmakingAvailable, threeVsThreeFitsParty);
		SetPoolAvailable(fiveVsFiveTile, fiveVsFiveToggle, isMatchmakingAvailable, fiveVsFiveFitsParty);
		RefreshPoolSelection();
	}

	private static void SetPoolAvailable(PlayTile tile, Toggle toggle, bool isMatchmakingAvailable, bool fitsParty)
	{
		if (!fitsParty)
		{
			toggle.SetValueWithoutNotify(newValue: false);
		}
		tile.SetEnabled(isMatchmakingAvailable & fitsParty);
	}

	public void SetThreeVsThreePoolDescription(string description)
	{
		threeVsThreeTile.Description = description;
	}

	public void SetFiveVsFivePoolDescription(string description)
	{
		fiveVsFiveTile.Description = description;
	}

	private bool IsPoolSelected(PlayTile tile, Toggle toggle)
	{
		if (IsVisible && tile.enabledSelf)
		{
			return toggle.value;
		}
		return false;
	}

	private string[] GetSelectedPoolIds()
	{
		List<string> list = new List<string>();
		if (IsPoolSelected(threeVsThreeTile, threeVsThreeToggle))
		{
			list.Add("3v3");
		}
		if (IsPoolSelected(fiveVsFiveTile, fiveVsFiveToggle))
		{
			list.Add("5v5");
		}
		return list.ToArray();
	}

	private void RefreshPoolSelection()
	{
		threeVsThreeTile.EnableInClassList("selected", IsPoolSelected(threeVsThreeTile, threeVsThreeToggle));
		fiveVsFiveTile.EnableInClassList("selected", IsPoolSelected(fiveVsFiveTile, fiveVsFiveToggle));
		EventManager.TriggerEvent("Event_OnPlaySelectedPoolsChanged", new Dictionary<string, object> { 
		{
			"poolIds",
			GetSelectedPoolIds()
		} });
	}

	public void SetStatistics(int players)
	{
		playersLabel.text = $"PLAYERS ONLINE: {players}";
	}

	private void OnClickClose()
	{
		EventManager.TriggerEvent("Event_OnPlayClickClose");
	}

	private void OnPoolSelectionChanged(ChangeEvent<bool> changeEvent)
	{
		RefreshPoolSelection();
	}

	private void OnClickPractice()
	{
		EventManager.TriggerEvent("Event_OnPlayClickPractice");
	}

	private void OnClickServerBrowser()
	{
		EventManager.TriggerEvent("Event_OnPlayClickServerBrowser");
	}
}
