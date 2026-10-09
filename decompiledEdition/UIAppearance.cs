using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UI;
using UnityEngine;
using UnityEngine.UIElements;

public class UIAppearance : UIView
{
	private static readonly Logger Logger = new Logger("UIAppearance");

	[Header("Settings")]
	[SerializeField]
	private List<AppearanceItem> flags = new List<AppearanceItem>();

	[SerializeField]
	private List<AppearanceItem> headgear = new List<AppearanceItem>();

	[SerializeField]
	private List<AppearanceItem> mustaches = new List<AppearanceItem>();

	[SerializeField]
	private List<AppearanceItem> beards = new List<AppearanceItem>();

	[SerializeField]
	private List<AppearanceItem> jerseys = new List<AppearanceItem>();

	[SerializeField]
	private List<AppearanceItem> stickSkins = new List<AppearanceItem>();

	[SerializeField]
	private List<AppearanceItem> stickShaftTapes = new List<AppearanceItem>();

	[SerializeField]
	private List<AppearanceItem> stickBladeTapes = new List<AppearanceItem>();

	[Header("References")]
	public VisualTreeAsset appearanceItemAsset;

	private AppearanceCategory category;

	private Dictionary<AppearanceCategory, AppearanceSubcategory> categorySubcategoryMap = new Dictionary<AppearanceCategory, AppearanceSubcategory>
	{
		{
			AppearanceCategory.Head,
			AppearanceSubcategory.Flags
		},
		{
			AppearanceCategory.Body,
			AppearanceSubcategory.Jerseys
		},
		{
			AppearanceCategory.Stick,
			AppearanceSubcategory.StickSkins
		}
	};

	private PlayerTeam team;

	private PlayerRole role;

	private bool applyForBothTeams;

	private int flagID;

	private int headgearIDBlueAttacker;

	private int headgearIDRedAttacker;

	private int headgearIDBlueGoalie;

	private int headgearIDRedGoalie;

	private int mustacheID;

	private int beardID;

	private int jerseyIDBlueAttacker;

	private int jerseyIDRedAttacker;

	private int jerseyIDBlueGoalie;

	private int jerseyIDRedGoalie;

	private int stickSkinIDBlueAttacker;

	private int stickSkinIDRedAttacker;

	private int stickSkinIDBlueGoalie;

	private int stickSkinIDRedGoalie;

	private int stickShaftTapeIDBlueAttacker;

	private int stickShaftTapeIDRedAttacker;

	private int stickShaftTapeIDBlueGoalie;

	private int stickShaftTapeIDRedGoalie;

	private int stickBladeTapeIDBlueAttacker;

	private int stickBladeTapeIDRedAttacker;

	private int stickBladeTapeIDBlueGoalie;

	private int stickBladeTapeIDRedGoalie;

	private VisualElement appearance;

	private IconButton closeIconButton;

	private TabView categoryTabView;

	private Tab headTab;

	private Tab bodyTab;

	private Tab stickTab;

	private TabView headTabView;

	private Tab flagsTab;

	private Tab headgearTab;

	private Tab mustachesTab;

	private Tab beardsTab;

	private TabView bodyTabView;

	private Tab jerseysTab;

	private TabView stickTabView;

	private Tab stickSkinsTab;

	private Tab stickShaftTapesTab;

	private Tab stickBladeTapesTab;

	private Toggle applyForBothTeamsToggle;

	private DropdownField teamDropdown;

	private DropdownField roleDropdown;

	private RadioButtonGroup flagsRadioButtonGroup;

	private RadioButtonGroup headgearRadioButtonGroup;

	private RadioButtonGroup mustachesRadioButtonGroup;

	private RadioButtonGroup beardsRadioButtonGroup;

	private RadioButtonGroup jerseysRadioButtonGroup;

	private RadioButtonGroup stickSkinsRadioButtonGroup;

	private RadioButtonGroup stickShaftTapesRadioButtonGroup;

	private RadioButtonGroup stickBladeTapesRadioButtonGroup;

	public void Initialize(VisualElement rootVisualElement)
	{
		ValidateAppearanceItems();
		View = rootVisualElement.Query<VisualElement>("AppearanceView");
		appearance = View.Query<VisualElement>("Appearance");
		closeIconButton = appearance.Query<TemplateContainer>("CloseIconButtonContainer").First().Query<IconButton>();
		closeIconButton.clicked += OnClickClose;
		categoryTabView = appearance.Query<TabView>();
		categoryTabView.activeTabChanged += OnCategoryTabChanged;
		headTab = categoryTabView.Query<Tab>("HeadTab");
		headTabView = headTab.Query<TabView>();
		headTabView.activeTabChanged += OnSubcategoryTabChanged;
		flagsTab = headTabView.Query<Tab>("FlagsTab");
		flagsRadioButtonGroup = flagsTab.Query<RadioButtonGroup>("AppearanceItemRadioButtonGroup");
		headgearTab = headTabView.Query<Tab>("HeadgearTab");
		headgearRadioButtonGroup = headgearTab.Query<RadioButtonGroup>("AppearanceItemRadioButtonGroup");
		mustachesTab = headTabView.Query<Tab>("MustachesTab");
		mustachesRadioButtonGroup = mustachesTab.Query<RadioButtonGroup>("AppearanceItemRadioButtonGroup");
		beardsTab = headTabView.Query<Tab>("BeardsTab");
		beardsRadioButtonGroup = beardsTab.Query<RadioButtonGroup>("AppearanceItemRadioButtonGroup");
		bodyTab = categoryTabView.Query<Tab>("BodyTab");
		bodyTabView = bodyTab.Query<TabView>();
		bodyTabView.activeTabChanged += OnSubcategoryTabChanged;
		jerseysTab = bodyTabView.Query<Tab>("JerseysTab");
		jerseysRadioButtonGroup = jerseysTab.Query<RadioButtonGroup>("AppearanceItemRadioButtonGroup");
		stickTab = categoryTabView.Query<Tab>("StickTab");
		stickTabView = stickTab.Query<TabView>();
		stickTabView.activeTabChanged += OnSubcategoryTabChanged;
		stickSkinsTab = stickTabView.Query<Tab>("SkinsTab");
		stickSkinsRadioButtonGroup = stickSkinsTab.Query<RadioButtonGroup>("AppearanceItemRadioButtonGroup");
		stickShaftTapesTab = stickTabView.Query<Tab>("ShaftTapesTab");
		stickShaftTapesRadioButtonGroup = stickShaftTapesTab.Query<RadioButtonGroup>("AppearanceItemRadioButtonGroup");
		stickBladeTapesTab = stickTabView.Query<Tab>("BladeTapesTab");
		stickBladeTapesRadioButtonGroup = stickBladeTapesTab.Query<RadioButtonGroup>("AppearanceItemRadioButtonGroup");
		teamDropdown = appearance.Query<VisualElement>("TeamInput").First().Query<DropdownField>();
		teamDropdown.choices = Utils.GetTeamNames();
		teamDropdown.value = Utils.GetNameFromTeam(SettingsManager.Team);
		teamDropdown.RegisterValueChangedCallback(OnTeamDropdownChanged);
		roleDropdown = appearance.Query<VisualElement>("RoleInput").First().Query<DropdownField>();
		roleDropdown.choices = Utils.GetRoleNames();
		roleDropdown.value = Utils.GetNameFromRole(SettingsManager.Role);
		roleDropdown.RegisterValueChangedCallback(OnRoleDropdownChanged);
		applyForBothTeamsToggle = appearance.Query<VisualElement>("ApplyForBothTeamsInput").First().Query<Toggle>();
		applyForBothTeamsToggle.value = SettingsManager.ApplyForBothTeams;
		applyForBothTeamsToggle.RegisterValueChangedCallback(OnApplyForBothTeamsToggleChanged);
		categoryTabView.activeTab = headTab;
		headTabView.activeTab = flagsTab;
		PopulateRadioButtonGroups();
	}

	public override bool Show()
	{
		bool flag = base.Show();
		if (flag)
		{
			EventManager.TriggerEvent("Event_OnAppearanceShow", new Dictionary<string, object>
			{
				{ "category", category },
				{
					"subcategory",
					categorySubcategoryMap[category]
				}
			});
		}
		return flag;
	}

	public override bool Hide()
	{
		bool flag = base.Hide();
		if (flag)
		{
			EventManager.TriggerEvent("Event_OnAppearanceHide");
		}
		return flag;
	}

	public void SetTeam(PlayerTeam value)
	{
		team = value;
		StyleRadioButtonGroups();
		UpdateRadioButtons();
	}

	public void SetRole(PlayerRole value)
	{
		role = value;
		StyleRadioButtonGroups();
		UpdateRadioButtons();
	}

	public void SetApplyForBothTeams(bool value)
	{
		applyForBothTeams = value;
	}

	public void SetFlagID(int value)
	{
		flagID = value;
		UpdateFlagsRadioButtons();
	}

	public void SetHeadgearID(PlayerTeam team, PlayerRole role, int value)
	{
		if (team == PlayerTeam.Blue && role == PlayerRole.Attacker)
		{
			headgearIDBlueAttacker = value;
		}
		else if (team == PlayerTeam.Red && role == PlayerRole.Attacker)
		{
			headgearIDRedAttacker = value;
		}
		else if (team == PlayerTeam.Blue && role == PlayerRole.Goalie)
		{
			headgearIDBlueGoalie = value;
		}
		else if (team == PlayerTeam.Red && role == PlayerRole.Goalie)
		{
			headgearIDRedGoalie = value;
		}
		UpdateHeadgearRadioButtons();
	}

	public void SetMustacheID(int value)
	{
		mustacheID = value;
		UpdateMustacheRadioButtons();
	}

	public void SetBeardID(int value)
	{
		beardID = value;
		UpdateBeardRadioButtons();
	}

	public void SetJerseyID(PlayerTeam team, PlayerRole role, int value)
	{
		if (team == PlayerTeam.Blue && role == PlayerRole.Attacker)
		{
			jerseyIDBlueAttacker = value;
		}
		else if (team == PlayerTeam.Red && role == PlayerRole.Attacker)
		{
			jerseyIDRedAttacker = value;
		}
		else if (team == PlayerTeam.Blue && role == PlayerRole.Goalie)
		{
			jerseyIDBlueGoalie = value;
		}
		else if (team == PlayerTeam.Red && role == PlayerRole.Goalie)
		{
			jerseyIDRedGoalie = value;
		}
		UpdateJerseyRadioButtons();
	}

	public void SetStickSkinID(PlayerTeam team, PlayerRole role, int value)
	{
		if (team == PlayerTeam.Blue && role == PlayerRole.Attacker)
		{
			stickSkinIDBlueAttacker = value;
		}
		else if (team == PlayerTeam.Red && role == PlayerRole.Attacker)
		{
			stickSkinIDRedAttacker = value;
		}
		else if (team == PlayerTeam.Blue && role == PlayerRole.Goalie)
		{
			stickSkinIDBlueGoalie = value;
		}
		else if (team == PlayerTeam.Red && role == PlayerRole.Goalie)
		{
			stickSkinIDRedGoalie = value;
		}
		UpdateStickSkinRadioButtons();
	}

	public void SetStickShaftTapeID(PlayerTeam team, PlayerRole role, int value)
	{
		if (team == PlayerTeam.Blue && role == PlayerRole.Attacker)
		{
			stickShaftTapeIDBlueAttacker = value;
		}
		else if (team == PlayerTeam.Red && role == PlayerRole.Attacker)
		{
			stickShaftTapeIDRedAttacker = value;
		}
		else if (team == PlayerTeam.Blue && role == PlayerRole.Goalie)
		{
			stickShaftTapeIDBlueGoalie = value;
		}
		else if (team == PlayerTeam.Red && role == PlayerRole.Goalie)
		{
			stickShaftTapeIDRedGoalie = value;
		}
		UpdateStickShaftTapeRadioButtons();
	}

	public void SetStickBladeTapeID(PlayerTeam team, PlayerRole role, int value)
	{
		if (team == PlayerTeam.Blue && role == PlayerRole.Attacker)
		{
			stickBladeTapeIDBlueAttacker = value;
		}
		else if (team == PlayerTeam.Red && role == PlayerRole.Attacker)
		{
			stickBladeTapeIDRedAttacker = value;
		}
		else if (team == PlayerTeam.Blue && role == PlayerRole.Goalie)
		{
			stickBladeTapeIDBlueGoalie = value;
		}
		else if (team == PlayerTeam.Red && role == PlayerRole.Goalie)
		{
			stickBladeTapeIDRedGoalie = value;
		}
		UpdateStickBladeTapeRadioButtons();
	}

	public void StyleRadioButtonGroups()
	{
		StyleRadioButtonGroup(flagsRadioButtonGroup);
		StyleRadioButtonGroup(headgearRadioButtonGroup);
		StyleRadioButtonGroup(mustachesRadioButtonGroup);
		StyleRadioButtonGroup(beardsRadioButtonGroup);
		StyleRadioButtonGroup(jerseysRadioButtonGroup);
		StyleRadioButtonGroup(stickSkinsRadioButtonGroup);
		StyleRadioButtonGroup(stickShaftTapesRadioButtonGroup);
		StyleRadioButtonGroup(stickBladeTapesRadioButtonGroup);
	}

	public void UpdateRadioButtons()
	{
		UpdateFlagsRadioButtons();
		UpdateHeadgearRadioButtons();
		UpdateMustacheRadioButtons();
		UpdateBeardRadioButtons();
		UpdateJerseyRadioButtons();
		UpdateStickSkinRadioButtons();
		UpdateStickShaftTapeRadioButtons();
		UpdateStickBladeTapeRadioButtons();
	}

	private void ValidateAppearanceItems()
	{
		ItemManager.GetItemsByCategories(new string[1] { "flag" }).ForEach((Item item) =>
		{
			if (!flags.Any((AppearanceItem appearanceItem) => appearanceItem.Id == item.id))
			{
				Logger.Warning($"Flag item {item.name} ({item.id}) is missing from the appearance flags list");
			}
		});
		ItemManager.GetItemsByCategories(new string[1] { "headgear" }).ForEach((Item item) =>
		{
			if (!headgear.Any((AppearanceItem appearanceItem) => appearanceItem.Id == item.id))
			{
				Logger.Warning($"Headgear item {item.name} ({item.id}) is missing from the appearance headgear list");
			}
		});
		ItemManager.GetItemsByCategories(new string[1] { "mustache" }).ForEach((Item item) =>
		{
			if (!mustaches.Any((AppearanceItem appearanceItem) => appearanceItem.Id == item.id))
			{
				Logger.Warning($"Mustache item {item.name} ({item.id}) is missing from the appearance mustaches list");
			}
		});
		ItemManager.GetItemsByCategories(new string[1] { "beard" }).ForEach((Item item) =>
		{
			if (!beards.Any((AppearanceItem appearanceItem) => appearanceItem.Id == item.id))
			{
				Logger.Warning($"Beard item {item.name} ({item.id}) is missing from the appearance beards list");
			}
		});
		ItemManager.GetItemsByCategories(new string[1] { "jersey" }).ForEach((Item item) =>
		{
			if (!jerseys.Any((AppearanceItem appearanceItem) => appearanceItem.Id == item.id))
			{
				Logger.Warning($"Jersey item {item.name} ({item.id}) is missing from the appearance jerseys list");
			}
		});
		ItemManager.GetItemsByCategories(new string[1] { "stickSkin" }).ForEach((Item item) =>
		{
			if (!stickSkins.Any((AppearanceItem appearanceItem) => appearanceItem.Id == item.id))
			{
				Logger.Warning($"Stick skin item {item.name} ({item.id}) is missing from the appearance stick skins list");
			}
		});
		ItemManager.GetItemsByCategories(new string[1] { "stickShaftTape" }).ForEach((Item item) =>
		{
			if (!stickShaftTapes.Any((AppearanceItem appearanceItem) => appearanceItem.Id == item.id))
			{
				Logger.Warning($"Stick shaft tape item {item.name} ({item.id}) is missing from the appearance stick shaft tapes list");
			}
		});
		ItemManager.GetItemsByCategories(new string[1] { "stickBladeTape" }).ForEach((Item item) =>
		{
			if (!stickBladeTapes.Any((AppearanceItem appearanceItem) => appearanceItem.Id == item.id))
			{
				Logger.Warning($"Stick blade tape item {item.name} ({item.id}) is missing from the appearance stick blade tapes list");
			}
		});
	}

	private void PopulateRadioButtonGroups()
	{
		PopulateRadioButtonGroup(flagsRadioButtonGroup, flags);
		PopulateRadioButtonGroup(headgearRadioButtonGroup, headgear);
		PopulateRadioButtonGroup(mustachesRadioButtonGroup, mustaches);
		PopulateRadioButtonGroup(beardsRadioButtonGroup, beards);
		PopulateRadioButtonGroup(jerseysRadioButtonGroup, jerseys);
		PopulateRadioButtonGroup(stickSkinsRadioButtonGroup, stickSkins);
		PopulateRadioButtonGroup(stickShaftTapesRadioButtonGroup, stickShaftTapes);
		PopulateRadioButtonGroup(stickBladeTapesRadioButtonGroup, stickBladeTapes);
	}

	private void PopulateRadioButtonGroup(RadioButtonGroup radioButtonGroup, List<AppearanceItem> appearanceItems)
	{
		VisualElement visualElement = radioButtonGroup.Query<VisualElement>("AppearanceItemList");
		visualElement.Clear();
		foreach (AppearanceItem appearanceItem in appearanceItems)
		{
			RadioButton radioButton = appearanceItemAsset.Instantiate().Query<RadioButton>("AppearanceItemRadioButton");
			Button button = radioButton.Query<VisualElement>("PurchaseButton").First().Query<Button>();
			Item item;
			if (appearanceItem.Id == -1)
			{
				item = new Item
				{
					id = -1,
					name = "NONE"
				};
			}
			else
			{
				item = ItemManager.GetItemById(appearanceItem.Id);
				if (item == null)
				{
					Logger.Error($"Could not populate appearance item with ID {appearanceItem.Id} because the item was not found in ItemManager");
					continue;
				}
				button.RegisterCallback((ClickEvent _) =>
				{
					OnClickPurchase(item);
				});
				button.text = "BUY $" + ((float)item.price / 100f).ToString("F2", CultureInfo.InvariantCulture);
			}
			radioButton.label = item.name.ToUpper();
			radioButton.userData = new Dictionary<string, object> { { "item", item } };
			radioButton.RegisterCallback((ClickEvent _) =>
			{
				OnClickAppearanceItem(item);
			});
			visualElement.Add(radioButton);
		}
	}

	private void StyleRadioButtonGroup(RadioButtonGroup radioButtonGroup)
	{
		VisualElement visualElement = radioButtonGroup.Query<VisualElement>("AppearanceItemList");
		visualElement.hierarchy.Sort((VisualElement a, VisualElement b) =>
		{
			Item item = (a.userData as Dictionary<string, object>)["item"] as Item;
			Item item2 = (b.userData as Dictionary<string, object>)["item"] as Item;
			if ((item.IsOwned && !item2.IsOwned) || item.id == -1)
			{
				return -1;
			}
			return ((!item.IsOwned && item2.IsOwned) || item2.id == -1) ? 1 : string.Compare(item.name, item2.name, StringComparison.OrdinalIgnoreCase);
		});
		foreach (RadioButton item3 in visualElement.Query<RadioButton>().ToList())
		{
			StyleRadioButton(item3);
		}
	}

	private void StyleRadioButton(RadioButton radioButton)
	{
		Item item = (radioButton.userData as Dictionary<string, object>)["item"] as Item;
		bool flag = ((role == PlayerRole.Attacker) ? item.IsAttackerItem : item.IsGoalieItem);
		bool flag2 = (item.IsUnlisted && !item.IsPurchased) || !flag;
		radioButton.EnableInClassList("owned", item.IsOwned);
		radioButton.style.display = (flag2 ? DisplayStyle.None : DisplayStyle.Flex);
	}

	private void UpdateFlagsRadioButtons()
	{
		List<RadioButton> radioButtons = flagsRadioButtonGroup.Query<RadioButton>().ToList();
		radioButtons.ForEach((RadioButton radioButton) =>
		{
			if (((radioButton.userData as Dictionary<string, object>)["item"] as Item).id == flagID)
			{
				flagsRadioButtonGroup.value = radioButtons.IndexOf(radioButton);
			}
		});
	}

	private void UpdateHeadgearRadioButtons()
	{
		List<RadioButton> radioButtons = headgearRadioButtonGroup.Query<RadioButton>().ToList();
		radioButtons.ForEach((RadioButton radioButton) =>
		{
			Item item = (radioButton.userData as Dictionary<string, object>)["item"] as Item;
			if (team == PlayerTeam.Blue && role == PlayerRole.Attacker && item.id == headgearIDBlueAttacker)
			{
				headgearRadioButtonGroup.value = radioButtons.IndexOf(radioButton);
			}
			else if (team == PlayerTeam.Red && role == PlayerRole.Attacker && item.id == headgearIDRedAttacker)
			{
				headgearRadioButtonGroup.value = radioButtons.IndexOf(radioButton);
			}
			else if (team == PlayerTeam.Blue && role == PlayerRole.Goalie && item.id == headgearIDBlueGoalie)
			{
				headgearRadioButtonGroup.value = radioButtons.IndexOf(radioButton);
			}
			else if (team == PlayerTeam.Red && role == PlayerRole.Goalie && item.id == headgearIDRedGoalie)
			{
				headgearRadioButtonGroup.value = radioButtons.IndexOf(radioButton);
			}
		});
	}

	private void UpdateMustacheRadioButtons()
	{
		List<RadioButton> radioButtons = mustachesRadioButtonGroup.Query<RadioButton>().ToList();
		radioButtons.ForEach((RadioButton radioButton) =>
		{
			if (((radioButton.userData as Dictionary<string, object>)["item"] as Item).id == mustacheID)
			{
				mustachesRadioButtonGroup.value = radioButtons.IndexOf(radioButton);
			}
		});
	}

	private void UpdateBeardRadioButtons()
	{
		List<RadioButton> radioButtons = beardsRadioButtonGroup.Query<RadioButton>().ToList();
		radioButtons.ForEach((RadioButton radioButton) =>
		{
			if (((radioButton.userData as Dictionary<string, object>)["item"] as Item).id == beardID)
			{
				beardsRadioButtonGroup.value = radioButtons.IndexOf(radioButton);
			}
		});
	}

	private void UpdateJerseyRadioButtons()
	{
		List<RadioButton> radioButtons = jerseysRadioButtonGroup.Query<RadioButton>().ToList();
		radioButtons.ForEach((RadioButton radioButton) =>
		{
			Item item = (radioButton.userData as Dictionary<string, object>)["item"] as Item;
			if (team == PlayerTeam.Blue && role == PlayerRole.Attacker && item.id == jerseyIDBlueAttacker)
			{
				jerseysRadioButtonGroup.value = radioButtons.IndexOf(radioButton);
			}
			else if (team == PlayerTeam.Red && role == PlayerRole.Attacker && item.id == jerseyIDRedAttacker)
			{
				jerseysRadioButtonGroup.value = radioButtons.IndexOf(radioButton);
			}
			else if (team == PlayerTeam.Blue && role == PlayerRole.Goalie && item.id == jerseyIDBlueGoalie)
			{
				jerseysRadioButtonGroup.value = radioButtons.IndexOf(radioButton);
			}
			else if (team == PlayerTeam.Red && role == PlayerRole.Goalie && item.id == jerseyIDRedGoalie)
			{
				jerseysRadioButtonGroup.value = radioButtons.IndexOf(radioButton);
			}
		});
	}

	private void UpdateStickSkinRadioButtons()
	{
		List<RadioButton> radioButtons = stickSkinsRadioButtonGroup.Query<RadioButton>().ToList();
		radioButtons.ForEach((RadioButton radioButton) =>
		{
			Item item = (radioButton.userData as Dictionary<string, object>)["item"] as Item;
			if (team == PlayerTeam.Blue && role == PlayerRole.Attacker && item.id == stickSkinIDBlueAttacker)
			{
				stickSkinsRadioButtonGroup.value = radioButtons.IndexOf(radioButton);
			}
			else if (team == PlayerTeam.Red && role == PlayerRole.Attacker && item.id == stickSkinIDRedAttacker)
			{
				stickSkinsRadioButtonGroup.value = radioButtons.IndexOf(radioButton);
			}
			else if (team == PlayerTeam.Blue && role == PlayerRole.Goalie && item.id == stickSkinIDBlueGoalie)
			{
				stickSkinsRadioButtonGroup.value = radioButtons.IndexOf(radioButton);
			}
			else if (team == PlayerTeam.Red && role == PlayerRole.Goalie && item.id == stickSkinIDRedGoalie)
			{
				stickSkinsRadioButtonGroup.value = radioButtons.IndexOf(radioButton);
			}
		});
	}

	private void UpdateStickShaftTapeRadioButtons()
	{
		List<RadioButton> radioButtons = stickShaftTapesRadioButtonGroup.Query<RadioButton>().ToList();
		radioButtons.ForEach((RadioButton radioButton) =>
		{
			Item item = (radioButton.userData as Dictionary<string, object>)["item"] as Item;
			if (team == PlayerTeam.Blue && role == PlayerRole.Attacker && item.id == stickShaftTapeIDBlueAttacker)
			{
				stickShaftTapesRadioButtonGroup.value = radioButtons.IndexOf(radioButton);
			}
			else if (team == PlayerTeam.Red && role == PlayerRole.Attacker && item.id == stickShaftTapeIDRedAttacker)
			{
				stickShaftTapesRadioButtonGroup.value = radioButtons.IndexOf(radioButton);
			}
			else if (team == PlayerTeam.Blue && role == PlayerRole.Goalie && item.id == stickShaftTapeIDBlueGoalie)
			{
				stickShaftTapesRadioButtonGroup.value = radioButtons.IndexOf(radioButton);
			}
			else if (team == PlayerTeam.Red && role == PlayerRole.Goalie && item.id == stickShaftTapeIDRedGoalie)
			{
				stickShaftTapesRadioButtonGroup.value = radioButtons.IndexOf(radioButton);
			}
		});
	}

	private void UpdateStickBladeTapeRadioButtons()
	{
		List<RadioButton> radioButtons = stickBladeTapesRadioButtonGroup.Query<RadioButton>().ToList();
		radioButtons.ForEach((RadioButton radioButton) =>
		{
			Item item = (radioButton.userData as Dictionary<string, object>)["item"] as Item;
			if (team == PlayerTeam.Blue && role == PlayerRole.Attacker && item.id == stickBladeTapeIDBlueAttacker)
			{
				stickBladeTapesRadioButtonGroup.value = radioButtons.IndexOf(radioButton);
			}
			else if (team == PlayerTeam.Red && role == PlayerRole.Attacker && item.id == stickBladeTapeIDRedAttacker)
			{
				stickBladeTapesRadioButtonGroup.value = radioButtons.IndexOf(radioButton);
			}
			else if (team == PlayerTeam.Blue && role == PlayerRole.Goalie && item.id == stickBladeTapeIDBlueGoalie)
			{
				stickBladeTapesRadioButtonGroup.value = radioButtons.IndexOf(radioButton);
			}
			else if (team == PlayerTeam.Red && role == PlayerRole.Goalie && item.id == stickBladeTapeIDRedGoalie)
			{
				stickBladeTapesRadioButtonGroup.value = radioButtons.IndexOf(radioButton);
			}
		});
	}

	private void OnClickClose()
	{
		EventManager.TriggerEvent("Event_OnAppearanceClickClose");
	}

	private void OnClickPurchase(Item item)
	{
		EventManager.TriggerEvent("Event_OnAppearanceClickPurchaseItem", new Dictionary<string, object> { { "item", item } });
	}

	private void OnTeamDropdownChanged(ChangeEvent<string> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnAppearanceTeamChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	private void OnRoleDropdownChanged(ChangeEvent<string> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnAppearanceRoleChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	private void OnApplyForBothTeamsToggleChanged(ChangeEvent<bool> changeEvent)
	{
		EventManager.TriggerEvent("Event_OnAppearanceApplyForBothTeamsChanged", new Dictionary<string, object> { { "value", changeEvent.newValue } });
	}

	private void OnCategoryTabChanged(Tab oldTab, Tab newTab)
	{
		switch (newTab.name)
		{
		case "HeadTab":
			category = AppearanceCategory.Head;
			break;
		case "BodyTab":
			category = AppearanceCategory.Body;
			break;
		case "StickTab":
			category = AppearanceCategory.Stick;
			break;
		}
		EventManager.TriggerEvent("Event_OnAppearanceCategoryChanged", new Dictionary<string, object>
		{
			{ "category", category },
			{
				"subcategory",
				categorySubcategoryMap[category]
			}
		});
	}

	private void OnSubcategoryTabChanged(Tab oldTab, Tab newTab)
	{
		Dictionary<AppearanceCategory, AppearanceSubcategory> dictionary = categorySubcategoryMap;
		AppearanceCategory key = category;
		dictionary[key] = newTab.name switch
		{
			"FlagsTab" => AppearanceSubcategory.Flags, 
			"HeadgearTab" => AppearanceSubcategory.Headgear, 
			"MustachesTab" => AppearanceSubcategory.Mustaches, 
			"BeardsTab" => AppearanceSubcategory.Beards, 
			"JerseysTab" => AppearanceSubcategory.Jerseys, 
			"SkinsTab" => AppearanceSubcategory.StickSkins, 
			"ShaftTapesTab" => AppearanceSubcategory.StickShaftTapes, 
			"BladeTapesTab" => AppearanceSubcategory.StickBladeTapes, 
			_ => categorySubcategoryMap[category], 
		};
		EventManager.TriggerEvent("Event_OnAppearanceCategoryChanged", new Dictionary<string, object>
		{
			{ "category", category },
			{
				"subcategory",
				categorySubcategoryMap[category]
			}
		});
	}

	private void OnClickAppearanceItem(Item item)
	{
		EventManager.TriggerEvent("Event_OnAppearanceClickItem", new Dictionary<string, object>
		{
			{ "item", item },
			{ "category", category },
			{
				"subcategory",
				categorySubcategoryMap[category]
			},
			{ "team", team },
			{ "role", role }
		});
	}
}
