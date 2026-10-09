using System.Collections.Generic;

public class UIModsController : UIViewController<UIMods>
{
	private UIMods uiMods;

	public override void Awake()
	{
		base.Awake();
		uiMods = GetComponent<UIMods>();
		EventManager.AddEventListener("Event_OnPluginAdded", Event_OnPluginAdded);
		EventManager.AddEventListener("Event_OnPluginStateChanged", Event_OnPluginStateChanged);
		EventManager.AddEventListener("Event_OnPluginEnableFailed", Event_OnPluginEnableFailed);
		EventManager.AddEventListener("Event_OnPluginDisableFailed", Event_OnPluginDisableFailed);
		EventManager.AddEventListener("Event_OnPluginRemoved", Event_OnPluginRemoved);
		EventManager.AddEventListener("Event_OnModAdded", Event_OnModAdded);
		EventManager.AddEventListener("Event_OnModStateChanged", Event_OnModStateChanged);
		EventManager.AddEventListener("Event_OnModSteamWorkshopItemStateChanged", Event_OnModSteamWorkshopItemStateChanged);
		EventManager.AddEventListener("Event_OnModSteamWorkshopItemDetailsStateChanged", Event_OnModSteamWorkshopItemDetailsStateChanged);
		EventManager.AddEventListener("Event_OnModEnableFailed", Event_OnModEnableFailed);
		EventManager.AddEventListener("Event_OnModDisableFailed", Event_OnModDisableFailed);
		EventManager.AddEventListener("Event_OnModRemoved", Event_OnModRemoved);
		EventManager.AddEventListener("Event_OnModsShow", Event_OnModsShow);
	}

	private void Start()
	{
		ModManager.Plugins.ForEach((Plugin plugin) =>
		{
			uiMods.AddPlugin(plugin);
		});
		ModManager.Mods.ForEach((Mod mod) =>
		{
			uiMods.AddMod(mod);
		});
	}

	public override void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_OnPluginAdded", Event_OnPluginAdded);
		EventManager.RemoveEventListener("Event_OnPluginStateChanged", Event_OnPluginStateChanged);
		EventManager.RemoveEventListener("Event_OnPluginEnableFailed", Event_OnPluginEnableFailed);
		EventManager.RemoveEventListener("Event_OnPluginDisableFailed", Event_OnPluginDisableFailed);
		EventManager.RemoveEventListener("Event_OnPluginRemoved", Event_OnPluginRemoved);
		EventManager.RemoveEventListener("Event_OnModAdded", Event_OnModAdded);
		EventManager.RemoveEventListener("Event_OnModStateChanged", Event_OnModStateChanged);
		EventManager.RemoveEventListener("Event_OnModSteamWorkshopItemStateChanged", Event_OnModSteamWorkshopItemStateChanged);
		EventManager.RemoveEventListener("Event_OnModSteamWorkshopItemDetailsStateChanged", Event_OnModSteamWorkshopItemDetailsStateChanged);
		EventManager.RemoveEventListener("Event_OnModEnableFailed", Event_OnModEnableFailed);
		EventManager.RemoveEventListener("Event_OnModDisableFailed", Event_OnModDisableFailed);
		EventManager.RemoveEventListener("Event_OnModRemoved", Event_OnModRemoved);
		EventManager.RemoveEventListener("Event_OnModsShow", Event_OnModsShow);
		base.OnDestroy();
	}

	private void Event_OnModsShow(Dictionary<string, object> message)
	{
		ModManager.Plugins.ForEach((Plugin plugin) =>
		{
			uiMods.UpdatePlugin(plugin);
		});
		ModManager.Mods.ForEach((Mod mod) =>
		{
			uiMods.UpdateMod(mod);
		});
	}

	private void Event_OnPluginAdded(Dictionary<string, object> message)
	{
		Plugin plugin = (Plugin)message["plugin"];
		uiMods.AddPlugin(plugin);
	}

	private void Event_OnPluginStateChanged(Dictionary<string, object> message)
	{
		Plugin plugin = (Plugin)message["plugin"];
		uiMods.UpdatePlugin(plugin);
	}

	private void Event_OnPluginEnableFailed(Dictionary<string, object> message)
	{
		Plugin plugin = (Plugin)message["plugin"];
		uiMods.UpdatePlugin(plugin);
	}

	private void Event_OnPluginDisableFailed(Dictionary<string, object> message)
	{
		Plugin plugin = (Plugin)message["plugin"];
		uiMods.UpdatePlugin(plugin);
	}

	private void Event_OnPluginRemoved(Dictionary<string, object> message)
	{
		Plugin plugin = (Plugin)message["plugin"];
		uiMods.RemovePlugin(plugin);
	}

	private void Event_OnModAdded(Dictionary<string, object> message)
	{
		Mod mod = (Mod)message["mod"];
		uiMods.AddMod(mod);
	}

	private void Event_OnModStateChanged(Dictionary<string, object> message)
	{
		Mod mod = (Mod)message["mod"];
		uiMods.UpdateMod(mod);
	}

	private void Event_OnModSteamWorkshopItemStateChanged(Dictionary<string, object> message)
	{
		Mod mod = (Mod)message["mod"];
		uiMods.UpdateMod(mod);
	}

	private void Event_OnModSteamWorkshopItemDetailsStateChanged(Dictionary<string, object> message)
	{
		Mod mod = (Mod)message["mod"];
		uiMods.UpdateMod(mod);
	}

	private void Event_OnModEnableFailed(Dictionary<string, object> message)
	{
		Mod mod = (Mod)message["mod"];
		uiMods.UpdateMod(mod);
	}

	private void Event_OnModDisableFailed(Dictionary<string, object> message)
	{
		Mod mod = (Mod)message["mod"];
		uiMods.UpdateMod(mod);
	}

	private void Event_OnModRemoved(Dictionary<string, object> message)
	{
		Mod mod = (Mod)message["mod"];
		uiMods.RemoveMod(mod);
	}
}
