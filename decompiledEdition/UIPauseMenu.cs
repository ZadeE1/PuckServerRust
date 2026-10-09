using UnityEngine.UIElements;

public class UIPauseMenu : UIView
{
	private VisualElement pauseMenu;

	private Button returnToGameButton;

	private Button selectTeamButton;

	private Button selectPositionButton;

	private Button forfeitButton;

	private Button serverBrowserButton;

	private Button disconnectButton;

	private Button settingsButton;

	private Button exitGameButton;

	private VisualElement voiceStatus;

	private Label voiceStatusLabel;

	private Label tickRateLabel;

	public void Initialize(VisualElement rootVisualElement)
	{
		View = rootVisualElement.Query<VisualElement>("PauseMenuView");
		pauseMenu = View.Query<VisualElement>("PauseMenu");
		returnToGameButton = pauseMenu.Query<Button>("ReturnToGameButton");
		returnToGameButton.clicked += OnClickReturnToGame;
		selectTeamButton = pauseMenu.Query<Button>("SelectTeamButton");
		selectTeamButton.clicked += OnClickSelectTeam;
		selectPositionButton = pauseMenu.Query<Button>("SelectPositionButton");
		selectPositionButton.clicked += OnClickSelectPosition;
		selectPositionButton.SetEnabled(value: false);
		forfeitButton = pauseMenu.Query<Button>("ForfeitButton");
		forfeitButton.clicked += OnClickForfeit;
		forfeitButton.SetEnabled(value: false);
		serverBrowserButton = pauseMenu.Query<Button>("ServerBrowserButton");
		serverBrowserButton.clicked += OnClickServerBrowser;
		settingsButton = pauseMenu.Query<Button>("SettingsButton");
		settingsButton.clicked += OnClickSettings;
		disconnectButton = pauseMenu.Query<Button>("DisconnectButton");
		disconnectButton.clicked += OnClickDisconnect;
		exitGameButton = pauseMenu.Query<Button>("ExitGameButton");
		exitGameButton.clicked += OnClickExitGame;
		voiceStatus = View.Query<VisualElement>("VoiceStatus");
		voiceStatusLabel = View.Query<Label>("VoiceStatusLabel");
		tickRateLabel = View.Query<Label>("TickRateLabel");
	}

	public void SetVoiceEnabled(bool value)
	{
		voiceStatus.EnableInClassList("enabled", value);
		voiceStatus.EnableInClassList("disabled", !value);
		voiceStatusLabel.text = (value ? "Voice Enabled" : "Voice Disabled");
	}

	public void SetTickRate(int tickRate)
	{
		tickRateLabel.text = $"Tick Rate: {tickRate} Hz";
	}

	private void OnClickReturnToGame()
	{
		EventManager.TriggerEvent("Event_OnPauseMenuClickReturnToGame");
	}

	private void OnClickSelectTeam()
	{
		EventManager.TriggerEvent("Event_OnPauseMenuClickSelectTeam");
	}

	private void OnClickSelectPosition()
	{
		EventManager.TriggerEvent("Event_OnPauseMenuClickSelectPosition");
	}

	public void SetSelectTeamEnabled(bool value)
	{
		selectTeamButton.SetEnabled(value);
	}

	public void SetSelectPositionEnabled(bool value)
	{
		selectPositionButton.SetEnabled(value);
	}

	public void SetForfeitEnabled(bool value)
	{
		forfeitButton.SetEnabled(value);
	}

	public void SetForfeitVoted(bool value)
	{
		forfeitButton.text = (value ? "VOTED TO FORFEIT" : "VOTE TO FORFEIT");
	}

	private void OnClickForfeit()
	{
		EventManager.TriggerEvent("Event_OnPauseMenuClickForfeit");
	}

	private void OnClickServerBrowser()
	{
		EventManager.TriggerEvent("Event_OnPauseMenuClickServerBrowser");
	}

	private void OnClickSettings()
	{
		EventManager.TriggerEvent("Event_OnPauseMenuClickSettings");
	}

	private void OnClickDisconnect()
	{
		EventManager.TriggerEvent("Event_OnPauseMenuClickDisconnect");
	}

	private void OnClickExitGame()
	{
		EventManager.TriggerEvent("Event_OnPauseMenuClickExitGame");
	}
}
