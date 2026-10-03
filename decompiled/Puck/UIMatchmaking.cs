using System;
using System.Collections.Generic;
using UI;
using UnityEngine.UIElements;

public class UIMatchmaking : UIView
{
	private VisualElement matching;

	private Label matchingPhaseLabel;

	private Label matchingTimeLabel;

	private Button matchingStartMatchmakingButton;

	private Button matchingConnectButton;

	private TemplateContainer matchingCloseIconButtonContainer;

	private IconButton matchingCloseIconButton;

	public string[] MatchingSelectedPoolIds { get; private set; } = new string[0];

	public void Initialize(VisualElement rootVisualElement)
	{
		View = rootVisualElement.Query<VisualElement>("MatchmakingView");
		matching = View.Query<VisualElement>("Matching");
		matchingPhaseLabel = matching.Query<Label>("PhaseLabel");
		matchingTimeLabel = matching.Query<Label>("TimeLabel");
		matchingCloseIconButtonContainer = matching.Query<TemplateContainer>("CloseIconButtonContainer");
		matchingCloseIconButton = matchingCloseIconButtonContainer.Query<IconButton>("IconButton");
		matchingCloseIconButton.clicked += OnClickMatchingClose;
		matchingStartMatchmakingButton = View.Query<Button>("StartMatchmakingButton");
		matchingStartMatchmakingButton.clicked += OnClickMatchingStartMatchmaking;
		matchingConnectButton = matching.Query<Button>("ConnectButton");
		matchingConnectButton.clicked += OnClickMatchingConnect;
	}

	public void SetMatchingVisibility(bool isVisible)
	{
		matching.style.display = ((!isVisible) ? DisplayStyle.None : DisplayStyle.Flex);
	}

	public void SetMatchingPhaseText(string text)
	{
		matchingPhaseLabel.text = text;
	}

	public void SetMatchingTimeVisibility(bool isVisible)
	{
		matchingTimeLabel.style.display = ((!isVisible) ? DisplayStyle.None : DisplayStyle.Flex);
	}

	public void SetMatchingTimeText(int seconds)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
		if (timeSpan.TotalHours < 1.0)
		{
			matchingTimeLabel.text = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
		}
		else
		{
			matchingTimeLabel.text = $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
		}
	}

	public void SetMatchingSelectedPoolIds(string[] poolIds)
	{
		MatchingSelectedPoolIds = poolIds;
	}

	public void SetMatchingStartMatchmakingButtonVisibility(bool isVisible)
	{
		matchingStartMatchmakingButton.style.display = ((!isVisible) ? DisplayStyle.None : DisplayStyle.Flex);
	}

	public void SetMatchingConnectButtonVisibility(bool isVisible)
	{
		matchingConnectButton.style.display = ((!isVisible) ? DisplayStyle.None : DisplayStyle.Flex);
	}

	public void SetMatchingCloseButtonVisibility(bool isVisible)
	{
		matchingCloseIconButtonContainer.style.display = ((!isVisible) ? DisplayStyle.None : DisplayStyle.Flex);
	}

	private void OnClickMatchingClose()
	{
		EventManager.TriggerEvent("Event_OnMatchmakingMatchingClickClose");
	}

	private void OnClickMatchingStartMatchmaking()
	{
		EventManager.TriggerEvent("Event_OnMatchmakingMatchingClickStartMatchmaking", new Dictionary<string, object> { { "poolIds", MatchingSelectedPoolIds } });
	}

	private void OnClickMatchingConnect()
	{
		EventManager.TriggerEvent("Event_OnMatchmakingMatchingClickConnect");
	}
}
