using System;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.UIElements;

public class UIMods : UIView
{
	private static readonly Logger Logger = new Logger("UIMods");

	[Header("References")]
	[SerializeField]
	private VisualTreeAsset modAsset;

	[SerializeField]
	private StyleBackground defaultPreviewTexture;

	private VisualElement mods;

	private VisualElement modsList;

	private VisualElement noMods;

	private IconButton closeIconButton;

	private Button findModsButton;

	private Button refreshButton;

	private Dictionary<Mod, TemplateContainer> modTemplateContainerMap = new Dictionary<Mod, TemplateContainer>();

	private Dictionary<Plugin, TemplateContainer> pluginTemplateContainerMap = new Dictionary<Plugin, TemplateContainer>();

	public void Initialize(VisualElement rootVisualElement)
	{
		View = rootVisualElement.Query<VisualElement>("ModsView");
		mods = View.Query<VisualElement>("Mods");
		modsList = mods.Query<VisualElement>("ModsList");
		noMods = mods.Query<VisualElement>("NoMods");
		closeIconButton = mods.Query<TemplateContainer>("CloseIconButtonContainer").First().Query<IconButton>();
		closeIconButton.clicked += OnClickClose;
		findModsButton = mods.Query<Button>("FindModsButton");
		findModsButton.clicked += OnClickFindMods;
		refreshButton = mods.Query<Button>("RefreshButton");
		refreshButton.clicked += OnClickRefresh;
		modsList.Clear();
	}

	public override bool Show()
	{
		bool flag = base.Show();
		if (flag)
		{
			EventManager.TriggerEvent("Event_OnModsShow");
		}
		return flag;
	}

	public void AddPlugin(Plugin plugin)
	{
		if (pluginTemplateContainerMap.ContainsKey(plugin))
		{
			return;
		}
		TemplateContainer templateContainer = modAsset.Instantiate();
		UI.Mod uiMod = templateContainer.Query<UI.Mod>();
		UI.Mod mod = uiMod;
		mod.Ready = (Action)Delegate.Combine(mod.Ready, (Action)(() =>
		{
			uiMod.Toggle.value = plugin.IsEnabled;
			uiMod.Toggle.RegisterValueChangedCallback((ChangeEvent<bool> e) =>
			{
				OnPluginToggleChanged(plugin, e.newValue);
			});
			UpdatePlugin(plugin);
		}));
		pluginTemplateContainerMap.Add(plugin, templateContainer);
		modsList.Add(templateContainer);
		UpdateNoMods();
	}

	public void UpdatePlugin(Plugin plugin)
	{
		if (pluginTemplateContainerMap.ContainsKey(plugin))
		{
			UI.Mod mod = pluginTemplateContainerMap[plugin].Query<UI.Mod>();
			mod.Description = string.Empty;
			mod.PreviewTexture = defaultPreviewTexture;
			mod.HasFiles = true;
			mod.Toggle.enabledSelf = plugin.HasAssembly;
			mod.Toggle.value = plugin.IsEnabled;
			mod.ModPreview.Link.enabledSelf = false;
			mod.ModPreview.Link.Text = plugin.Id;
			mod.ModPreview.IsStatisticsVisible = false;
			mod.Refresh();
		}
	}

	public void RemovePlugin(Plugin plugin)
	{
		if (pluginTemplateContainerMap.ContainsKey(plugin))
		{
			TemplateContainer element = pluginTemplateContainerMap[plugin];
			modsList.Remove(element);
			pluginTemplateContainerMap.Remove(plugin);
			UpdateNoMods();
		}
	}

	public void AddMod(Mod mod)
	{
		if (modTemplateContainerMap.ContainsKey(mod))
		{
			return;
		}
		TemplateContainer templateContainer = modAsset.Instantiate();
		UI.Mod uiMod = templateContainer.Query<UI.Mod>();
		UI.Mod mod2 = uiMod;
		mod2.Ready = (Action)Delegate.Combine(mod2.Ready, (Action)(() =>
		{
			uiMod.Toggle.value = mod.IsEnabled;
			uiMod.Toggle.RegisterValueChangedCallback((ChangeEvent<bool> e) =>
			{
				OnModToggleChanged(mod, e.newValue);
			});
			Link link = uiMod.ModPreview.Link;
			link.Clicked = (Action)Delegate.Combine(link.Clicked, (Action)(() =>
			{
				OnModPreviewLinkClicked(mod);
			}));
			UpdateMod(mod);
		}));
		modTemplateContainerMap.Add(mod, templateContainer);
		modsList.Add(templateContainer);
		UpdateNoMods();
	}

	public void UpdateMod(Mod mod)
	{
		if (modTemplateContainerMap.ContainsKey(mod))
		{
			UI.Mod mod2 = modTemplateContainerMap[mod].Query<UI.Mod>();
			mod2.Description = mod.SteamWorkshopItem?.Details?.Description ?? string.Empty;
			Texture2D texture2D = mod.SteamWorkshopItem?.Details?.PreviewTexture;
			mod2.PreviewTexture = (((object)texture2D != null) ? ((StyleBackground)texture2D) : defaultPreviewTexture);
			mod2.HasFiles = mod.HasLocalFiles;
			mod2.Toggle.enabledSelf = mod.HasAssembly;
			mod2.Toggle.value = mod.IsEnabled;
			mod2.ModPreview.Link.Text = mod.SteamWorkshopItem?.Details?.Title ?? mod.Id;
			mod2.ModPreview.IsStatisticsVisible = mod.SteamWorkshopItem?.Details != null;
			mod2.ModPreview.Subscriptions = (mod.SteamWorkshopItem?.Details?.Subscriptions).GetValueOrDefault();
			mod2.ModPreview.Upvotes = (mod.SteamWorkshopItem?.Details?.Upvotes).GetValueOrDefault();
			mod2.ModPreview.Downvotes = (mod.SteamWorkshopItem?.Details?.Downvotes).GetValueOrDefault();
			mod2.Refresh();
		}
	}

	public void RemoveMod(Mod mod)
	{
		if (modTemplateContainerMap.ContainsKey(mod))
		{
			TemplateContainer element = modTemplateContainerMap[mod];
			modsList.Remove(element);
			modTemplateContainerMap.Remove(mod);
			UpdateNoMods();
		}
	}

	private void UpdateNoMods()
	{
		noMods.style.display = ((modTemplateContainerMap.Count > 0 || pluginTemplateContainerMap.Count > 0) ? DisplayStyle.None : DisplayStyle.Flex);
	}

	private void OnClickClose()
	{
		EventManager.TriggerEvent("Event_OnModsClickClose");
	}

	private void OnClickFindMods()
	{
		EventManager.TriggerEvent("Event_OnModsClickFindMods");
	}

	private void OnClickRefresh()
	{
		EventManager.TriggerEvent("Event_OnModsClickRefresh");
	}

	private void OnModToggleChanged(Mod mod, bool value)
	{
		if (value)
		{
			EventManager.TriggerEvent("Event_OnModsModEnabled", new Dictionary<string, object> { { "mod", mod } });
		}
		else
		{
			EventManager.TriggerEvent("Event_OnModsModDisabled", new Dictionary<string, object> { { "mod", mod } });
		}
	}

	private void OnModPreviewLinkClicked(Mod mod)
	{
		EventManager.TriggerEvent("Event_OnModPreviewLinkClicked", new Dictionary<string, object> { { "id", mod.Id } });
	}

	private void OnPluginToggleChanged(Plugin plugin, bool value)
	{
		if (value)
		{
			EventManager.TriggerEvent("Event_OnModsPluginEnabled", new Dictionary<string, object> { { "plugin", plugin } });
		}
		else
		{
			EventManager.TriggerEvent("Event_OnModsPluginDisabled", new Dictionary<string, object> { { "plugin", plugin } });
		}
	}
}
