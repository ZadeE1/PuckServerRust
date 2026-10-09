using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;
using UnityEngine.UIElements;

public class UINewServer : UIView
{
	private static readonly Logger Logger = new Logger("UINewServer");

	private VisualElement newServer;

	private IconButton closeIconButton;

	private Button startButton;

	private TabView tabView;

	private Tab dedicatedTab;

	private Tab selfHostedTab;

	private TextField dedicatedNameTextField;

	private DropdownField dedicatedProbeDropdown;

	private Slider dedicatedMaxPlayerSlider;

	private Toggle dedicatedPasswordProtectedToggle;

	private TextField dedicatedPasswordTextField;

	private TextField selfHostedNameTextField;

	private IntegerField selfHostedPortIntegerField;

	private Slider selfHostedMaxPlayerSlider;

	private Toggle selfHostedPasswordProtectedToggle;

	private TextField selfHostedPasswordTextField;

	private Toggle selfHostedVoipToggle;

	private VisualElement dedicatedForm;

	private VisualElement cloudBannerIcon;

	private Label cloudBannerLabel;

	private Button cloudPatreonButton;

	private bool isPatreon;

	private bool dedicatedProbesLoaded;

	private Probe[] dedicatedLauncherProbes = new Probe[0];

	private string dedicatedName = "MY PUCK SERVER";

	private Probe dedicatedProbe;

	private int dedicatedMaxPlayers = 6;

	private string dedicatedPassword;

	private int selfHostedPort = 30609;

	private string selfHostedName = "MY PUCK SERVER";

	private int selfHostedMaxPlayers = 12;

	private string selfHostedPassword;

	private bool selfHostedUseVoip;

	public void Initialize(VisualElement rootVisualElement)
	{
		View = rootVisualElement.Query<VisualElement>("NewServerView");
		newServer = View.Query<VisualElement>("NewServer");
		closeIconButton = newServer.Query<TemplateContainer>("CloseIconButtonContainer").First().Query<IconButton>();
		closeIconButton.clicked += OnClickClose;
		startButton = newServer.Query<Button>("StartButton");
		startButton.clicked += OnClickStart;
		tabView = newServer.Query<TabView>("TabView");
		dedicatedTab = newServer.Query<Tab>("DedicatedTab");
		dedicatedNameTextField = dedicatedTab.Query<VisualElement>("NameTextFieldInput").First().Query<TextField>();
		dedicatedNameTextField.RegisterValueChangedCallback(OnDedicatedNameChanged);
		dedicatedNameTextField.RegisterCallback<FocusOutEvent>(OnDedicatedNameFocusOut);
		dedicatedNameTextField.value = dedicatedName;
		dedicatedProbeDropdown = dedicatedTab.Query<VisualElement>("ProbeDropdownInput").First().Query<DropdownField>();
		dedicatedProbeDropdown.RegisterValueChangedCallback(OnDedicatedProbeChanged);
		dedicatedMaxPlayerSlider = dedicatedTab.Query<VisualElement>("MaxPlayersSliderInput").First().Query<Slider>();
		dedicatedMaxPlayerSlider.RegisterValueChangedCallback(OnDedicatedMaxPlayersChanged);
		dedicatedMaxPlayerSlider.value = dedicatedMaxPlayers;
		dedicatedPasswordTextField = dedicatedTab.Query<VisualElement>("PasswordTextFieldInput").First().Query<TextField>();
		dedicatedPasswordTextField.RegisterValueChangedCallback(OnDedicatedPasswordChanged);
		dedicatedPasswordTextField.value = dedicatedPassword;
		dedicatedPasswordProtectedToggle = dedicatedTab.Query<VisualElement>("PasswordProtectedToggleInput").First().Query<Toggle>();
		dedicatedPasswordProtectedToggle.RegisterValueChangedCallback(OnDedicatedPasswordProtectedChanged);
		dedicatedPasswordProtectedToggle.value = !string.IsNullOrEmpty(dedicatedPassword);
		dedicatedForm = dedicatedTab.Query<VisualElement>("DedicatedForm");
		cloudBannerIcon = dedicatedTab.Query<VisualElement>("CloudBannerIcon");
		cloudBannerLabel = dedicatedTab.Query<Label>("CloudBannerLabel");
		cloudPatreonButton = dedicatedTab.Query<Button>("CloudPatreonButton");
		cloudPatreonButton.clicked += OnClickCloudPatreon;
		tabView.activeTabChanged += OnActiveTabChanged;
		selfHostedTab = newServer.Query<Tab>("SelfHostedTab");
		selfHostedNameTextField = selfHostedTab.Query<VisualElement>("NameTextFieldInput").First().Query<TextField>();
		selfHostedNameTextField.RegisterValueChangedCallback(OnSelfHostedNameChanged);
		selfHostedNameTextField.RegisterCallback<FocusOutEvent>(OnSelfHostedNameFocusOut);
		selfHostedNameTextField.value = selfHostedName;
		selfHostedPortIntegerField = selfHostedTab.Query<VisualElement>("PortIntegerFieldInput").First().Query<IntegerField>();
		selfHostedPortIntegerField.RegisterValueChangedCallback(OnSelfHostedPortChanged);
		selfHostedPortIntegerField.value = selfHostedPort;
		selfHostedMaxPlayerSlider = selfHostedTab.Query<VisualElement>("MaxPlayersSliderInput").First().Query<Slider>();
		selfHostedMaxPlayerSlider.RegisterValueChangedCallback(OnSelfHostedMaxPlayersChanged);
		selfHostedMaxPlayerSlider.value = selfHostedMaxPlayers;
		selfHostedPasswordTextField = selfHostedTab.Query<VisualElement>("PasswordTextFieldInput").First().Query<TextField>();
		selfHostedPasswordTextField.RegisterValueChangedCallback(OnSelfHostedPasswordChanged);
		selfHostedPasswordTextField.value = selfHostedPassword;
		selfHostedPasswordProtectedToggle = selfHostedTab.Query<VisualElement>("PasswordProtectedToggleInput").First().Query<Toggle>();
		selfHostedPasswordProtectedToggle.RegisterValueChangedCallback(OnSelfHostedPasswordProtectedChanged);
		selfHostedPasswordProtectedToggle.value = !string.IsNullOrEmpty(selfHostedPassword);
		selfHostedVoipToggle = selfHostedTab.Query<VisualElement>("VOIPToggleInput").First().Query<Toggle>();
		selfHostedVoipToggle.RegisterValueChangedCallback(OnSelfHostedVoipChanged);
		selfHostedVoipToggle.value = selfHostedUseVoip;
		AddTabHeaderIcons();
	}

	private void AddTabHeaderIcons()
	{
		List<VisualElement> list = tabView.Query<VisualElement>(null, "unity-tab__header").ToList();
		string[] array = new string[2] { "computer", "cloud" };
		for (int i = 0; i < list.Count && i < array.Length; i++)
		{
			VisualElement visualElement = new VisualElement
			{
				name = "TabHeaderIcon",
				pickingMode = PickingMode.Ignore
			};
			visualElement.AddToClassList("tabHeaderIcon");
			visualElement.AddToClassList(array[i]);
			list[i].Insert(0, visualElement);
		}
	}

	public override bool Show()
	{
		if (!IsVisible)
		{
			Refresh();
		}
		return base.Show();
	}

	public void Refresh()
	{
		SetDefaultServerName(BackendManager.PlayerState.PlayerData?.username);
		PlayerData playerData = BackendManager.PlayerState.PlayerData;
		SetCloudAvailable(playerData != null && playerData.patreonLevel >= 1);
		dedicatedProbesLoaded = false;
		RefreshStartButton();
		dedicatedLauncherProbes = new Probe[0];
		dedicatedProbeDropdown.choices = new List<string>();
		dedicatedProbeDropdown.value = null;
		dedicatedProbeDropdown.SetEnabled(value: false);
		WebSocketManager.Emit("playerGetProbesRequest", null, "playerGetProbesResponse");
	}

	private void SetDefaultServerName(string username)
	{
		if (!string.IsNullOrEmpty(username))
		{
			string valueWithoutNotify = username + "'s server";
			if (selfHostedName == "MY PUCK SERVER")
			{
				selfHostedName = valueWithoutNotify;
				selfHostedNameTextField.SetValueWithoutNotify(valueWithoutNotify);
			}
			if (dedicatedName == "MY PUCK SERVER")
			{
				dedicatedName = valueWithoutNotify;
				dedicatedNameTextField.SetValueWithoutNotify(valueWithoutNotify);
			}
		}
	}

	public void SetCloudAvailable(bool patreon)
	{
		isPatreon = patreon;
		dedicatedForm.SetEnabled(patreon);
		cloudPatreonButton.style.display = (patreon ? DisplayStyle.None : DisplayStyle.Flex);
		cloudBannerIcon.EnableInClassList("patreon", patreon);
		cloudBannerLabel.EnableInClassList("patreon", patreon);
		cloudBannerLabel.text = (patreon ? "Thanks for being a Patreon supporter!" : "Cloud servers are only available to Patreon supporters.\nYou can still self-host a server.");
		RefreshStartButton();
	}

	private void RefreshStartButton()
	{
		bool flag = tabView.activeTab == dedicatedTab && !isPatreon;
		startButton.SetEnabled(dedicatedProbesLoaded && !flag);
	}

	public void SetDedicatedProbes(Probe[] probes)
	{
		dedicatedLauncherProbes = probes.OrderBy((Probe probe) => probe.ToLabel()).ToArray();
		dedicatedProbeDropdown.choices = dedicatedLauncherProbes.Select((Probe probe) => probe.ToLabel()).ToList();
		dedicatedProbeDropdown.index = 0;
		dedicatedProbeDropdown.SetEnabled(value: true);
		dedicatedProbesLoaded = true;
		RefreshStartButton();
	}

	private void ResetDedicatedName()
	{
		dedicatedName = "MY PUCK SERVER";
		dedicatedNameTextField.value = dedicatedName;
	}

	private void ResetSelfHostedName()
	{
		selfHostedName = "MY PUCK SERVER";
		selfHostedNameTextField.value = selfHostedName;
	}

	private void OnClickClose()
	{
		EventManager.TriggerEvent("Event_OnNewServerClickClose");
	}

	private void OnClickCloudPatreon()
	{
		EventManager.TriggerEvent("Event_OnSocialClickPatreon");
	}

	private void OnActiveTabChanged(Tab previousTab, Tab currentTab)
	{
		RefreshStartButton();
	}

	private void OnClickStart()
	{
		if (tabView.activeTab == dedicatedTab)
		{
			if (isPatreon)
			{
				EventManager.TriggerEvent("Event_OnNewServerClickStart", new Dictionary<string, object>
				{
					{ "type", "dedicated" },
					{ "name", dedicatedName },
					{ "maxPlayers", dedicatedMaxPlayers },
					{ "password", dedicatedPassword },
					{ "probeId", dedicatedProbe.id }
				});
			}
		}
		else if (tabView.activeTab == selfHostedTab)
		{
			EventManager.TriggerEvent("Event_OnNewServerClickStart", new Dictionary<string, object>
			{
				{ "type", "selfHosted" },
				{ "port", selfHostedPort },
				{ "name", selfHostedName },
				{ "maxPlayers", selfHostedMaxPlayers },
				{ "password", selfHostedPassword },
				{ "useVoip", selfHostedUseVoip }
			});
		}
	}

	private void OnDedicatedNameChanged(ChangeEvent<string> changeEvent)
	{
		dedicatedName = StringUtils.FilterStringSpecialCharacters(changeEvent.newValue);
		dedicatedNameTextField.value = dedicatedName;
	}

	private void OnDedicatedNameFocusOut(FocusOutEvent focusOutEvent)
	{
		dedicatedName = StringUtils.FilterStringProfanity(dedicatedName);
		if (string.IsNullOrEmpty(dedicatedName))
		{
			ResetDedicatedName();
		}
		else
		{
			dedicatedNameTextField.value = dedicatedName;
		}
	}

	private void OnDedicatedProbeChanged(ChangeEvent<string> changeEvent)
	{
		int index = dedicatedProbeDropdown.index;
		if (index >= 0 && index < dedicatedLauncherProbes.Length)
		{
			dedicatedProbe = dedicatedLauncherProbes[index];
		}
	}

	private void OnDedicatedMaxPlayersChanged(ChangeEvent<float> changeEvent)
	{
		dedicatedMaxPlayers = Mathf.RoundToInt(changeEvent.newValue);
		dedicatedMaxPlayerSlider.value = dedicatedMaxPlayers;
	}

	private void OnDedicatedPasswordProtectedChanged(ChangeEvent<bool> changeEvent)
	{
		if (changeEvent.newValue)
		{
			dedicatedPasswordTextField.SetEnabled(value: true);
			return;
		}
		dedicatedPasswordTextField.SetEnabled(value: false);
		dedicatedPasswordTextField.value = string.Empty;
	}

	private void OnDedicatedPasswordChanged(ChangeEvent<string> changeEvent)
	{
		dedicatedPassword = changeEvent.newValue;
		dedicatedPasswordTextField.value = dedicatedPassword;
	}

	private void OnSelfHostedNameChanged(ChangeEvent<string> changeEvent)
	{
		selfHostedName = StringUtils.FilterStringSpecialCharacters(changeEvent.newValue);
		selfHostedNameTextField.value = selfHostedName;
	}

	private void OnSelfHostedNameFocusOut(FocusOutEvent focusOutEvent)
	{
		selfHostedName = StringUtils.FilterStringProfanity(selfHostedName);
		if (string.IsNullOrEmpty(selfHostedName))
		{
			ResetSelfHostedName();
		}
		else
		{
			selfHostedNameTextField.value = selfHostedName;
		}
	}

	private void OnSelfHostedPortChanged(ChangeEvent<int> changeEvent)
	{
		selfHostedPort = Mathf.Clamp(changeEvent.newValue, 1, 65535);
		selfHostedPortIntegerField.SetValueWithoutNotify(selfHostedPort);
	}

	private void OnSelfHostedMaxPlayersChanged(ChangeEvent<float> changeEvent)
	{
		selfHostedMaxPlayers = Mathf.RoundToInt(changeEvent.newValue);
		selfHostedMaxPlayerSlider.value = selfHostedMaxPlayers;
	}

	private void OnSelfHostedPasswordProtectedChanged(ChangeEvent<bool> changeEvent)
	{
		if (changeEvent.newValue)
		{
			selfHostedPasswordTextField.SetEnabled(value: true);
			return;
		}
		selfHostedPasswordTextField.SetEnabled(value: false);
		selfHostedPasswordTextField.value = string.Empty;
	}

	private void OnSelfHostedPasswordChanged(ChangeEvent<string> changeEvent)
	{
		selfHostedPassword = changeEvent.newValue;
		selfHostedPasswordTextField.value = selfHostedPassword;
	}

	private void OnSelfHostedVoipChanged(ChangeEvent<bool> changeEvent)
	{
		selfHostedUseVoip = changeEvent.newValue;
	}
}
