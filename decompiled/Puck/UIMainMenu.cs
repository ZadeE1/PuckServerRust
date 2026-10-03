using System.Collections.Generic;
using UnityEngine.UIElements;

public class UIMainMenu : UIView
{
	private VisualElement mainMenu;

	private VisualElement debug;

	private TextField ipAddressTextField;

	private IntegerField portIntegerField;

	private TextField passwordTextField;

	private Button joinServerButton;

	private Button hostServerButton;

	private Button playButton;

	private Button playerButton;

	private Button settingsButton;

	private Button modsButton;

	private Button exitGameButton;

	private VisualElement social;

	private Button discordButton;

	private Button patreonButton;

	private string ipAddress = "127.0.0.1";

	private ushort port = 30609;

	private string password;

	public void Initialize(VisualElement rootVisualElement)
	{
		View = rootVisualElement.Query<VisualElement>("MainMenuView");
		mainMenu = View.Query<VisualElement>("MainMenu");
		debug = View.Query<VisualElement>("Debug");
		social = View.Query<VisualElement>("Social");
		playButton = mainMenu.Query<Button>("PlayButton");
		playButton.clicked += OnClickPlay;
		playerButton = mainMenu.Query<Button>("PlayerButton");
		playerButton.clicked += OnClickPlayer;
		settingsButton = mainMenu.Query<Button>("SettingsButton");
		settingsButton.clicked += OnClickSettings;
		modsButton = mainMenu.Query<Button>("ModsButton");
		modsButton.clicked += OnClickMods;
		exitGameButton = mainMenu.Query<Button>("ExitGameButton");
		exitGameButton.clicked += OnClickExitGame;
		discordButton = social.Query<Button>("DiscordButton");
		discordButton.clicked += OnClickDiscord;
		patreonButton = social.Query<Button>("PatreonButton");
		patreonButton.clicked += OnClickPatreon;
		ipAddressTextField = debug.Query<VisualElement>("IpAddressTextField").First().Query<TextField>();
		ipAddressTextField.RegisterValueChangedCallback(OnIpAddressChanged);
		ipAddressTextField.value = ipAddress;
		portIntegerField = debug.Query<VisualElement>("PortIntegerField").First().Query<IntegerField>();
		portIntegerField.RegisterValueChangedCallback(OnPortChanged);
		portIntegerField.value = port;
		passwordTextField = debug.Query<VisualElement>("PasswordTextField").First().Query<TextField>();
		passwordTextField.RegisterValueChangedCallback(OnPasswordChanged);
		passwordTextField.value = password;
		joinServerButton = debug.Query<Button>("JoinServerButton");
		joinServerButton.clicked += OnClickJoinServer;
		hostServerButton = debug.Query<Button>("HostServerButton");
		hostServerButton.clicked += OnClickHostServer;
	}

	public override bool Show()
	{
		bool flag = base.Show();
		if (flag)
		{
			EventManager.TriggerEvent("Event_OnMainMenuShow");
		}
		return flag;
	}

	public override bool Hide()
	{
		bool flag = base.Hide();
		if (flag)
		{
			EventManager.TriggerEvent("Event_OnMainMenuHide");
		}
		return flag;
	}

	public void ShowDebug()
	{
		debug.style.display = DisplayStyle.Flex;
	}

	public void HideDebug()
	{
		debug.style.display = DisplayStyle.None;
	}

	private void OnIpAddressChanged(ChangeEvent<string> changeEvent)
	{
		ipAddress = changeEvent.newValue;
	}

	private void OnPortChanged(ChangeEvent<int> changeEvent)
	{
		port = (ushort)changeEvent.newValue;
	}

	private void OnPasswordChanged(ChangeEvent<string> changeEvent)
	{
		password = changeEvent.newValue;
	}

	private void OnClickJoinServer()
	{
		EventManager.TriggerEvent("Event_OnMainMenuClickJoinServer", new Dictionary<string, object>
		{
			{ "ipAddress", ipAddress },
			{ "port", port },
			{ "password", password }
		});
	}

	private void OnClickHostServer()
	{
		EventManager.TriggerEvent("Event_OnMainMenuClickHostServer", new Dictionary<string, object>
		{
			{ "port", port },
			{ "password", password }
		});
	}

	private void OnClickPlay()
	{
		EventManager.TriggerEvent("Event_OnMainMenuClickPlay");
	}

	private void OnClickPlayer()
	{
		EventManager.TriggerEvent("Event_OnMainMenuClickPlayer");
	}

	private void OnClickSettings()
	{
		EventManager.TriggerEvent("Event_OnMainMenuClickSettings");
	}

	private void OnClickMods()
	{
		EventManager.TriggerEvent("Event_OnMainMenuClickMods");
	}

	private void OnClickExitGame()
	{
		EventManager.TriggerEvent("Event_OnMainMenuClickExitGame");
	}

	private void OnClickDiscord()
	{
		EventManager.TriggerEvent("Event_OnSocialClickDiscord");
	}

	private void OnClickPatreon()
	{
		EventManager.TriggerEvent("Event_OnSocialClickPatreon");
	}
}
