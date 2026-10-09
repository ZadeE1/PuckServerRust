using System.Collections.Generic;
using System.Linq;

public static class ModManagerController
{
	private static readonly Logger Logger = new Logger("ModManagerController");

	public static void Initialize()
	{
		EventManager.AddEventListener("Event_OnSteamWorkshopItemAdded", Event_OnSteamWorkshopItemAdded);
		EventManager.AddEventListener("Event_OnSteamWorkshopItemRemoved", Event_OnSteamWorkshopItemRemoved);
		EventManager.AddEventListener("Event_OnModsPluginEnabled", Event_OnModsPluginEnabled);
		EventManager.AddEventListener("Event_OnModsPluginDisabled", Event_OnModsPluginDisabled);
		EventManager.AddEventListener("Event_OnModsModEnabled", Event_OnModsModEnabled);
		EventManager.AddEventListener("Event_OnModsModDisabled", Event_OnModsModDisabled);
		EventManager.AddEventListener("Event_OnModStateChanged", Event_OnModStateChanged);
		EventManager.AddEventListener("Event_OnPluginStateChanged", Event_OnPluginStateChanged);
		EventManager.AddEventListener("Event_OnConnectionStateChanged", Event_OnConnectionStateChanged);
		EventManager.AddEventListener("Event_OnReconnectionStateChanged", Event_OnReconnectionStateChanged);
		EventManager.AddEventListener("Event_Server_OnServerStarted", Event_Server_OnServerStarted);
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			ModManager.LoadModsStatus();
			ModManager.LoadPlugins();
		}
		SteamWorkshopManager.Items.ForEach((SteamWorkshopItem item) =>
		{
			ModManager.AddMod(item);
		});
	}

	public static void Dispose()
	{
		EventManager.RemoveEventListener("Event_OnSteamWorkshopItemAdded", Event_OnSteamWorkshopItemAdded);
		EventManager.RemoveEventListener("Event_OnSteamWorkshopItemRemoved", Event_OnSteamWorkshopItemRemoved);
		EventManager.RemoveEventListener("Event_OnModsPluginEnabled", Event_OnModsPluginEnabled);
		EventManager.RemoveEventListener("Event_OnModsPluginDisabled", Event_OnModsPluginDisabled);
		EventManager.RemoveEventListener("Event_OnModsModEnabled", Event_OnModsModEnabled);
		EventManager.RemoveEventListener("Event_OnModsModDisabled", Event_OnModsModDisabled);
		EventManager.RemoveEventListener("Event_OnModStateChanged", Event_OnModStateChanged);
		EventManager.RemoveEventListener("Event_OnPluginStateChanged", Event_OnPluginStateChanged);
		EventManager.RemoveEventListener("Event_OnConnectionStateChanged", Event_OnConnectionStateChanged);
		EventManager.RemoveEventListener("Event_OnReconnectionStateChanged", Event_OnReconnectionStateChanged);
		EventManager.RemoveEventListener("Event_Server_OnServerStarted", Event_Server_OnServerStarted);
	}

	private static void Event_OnSteamWorkshopItemAdded(Dictionary<string, object> message)
	{
		ModManager.AddMod((SteamWorkshopItem)message["item"]);
	}

	private static void Event_OnSteamWorkshopItemRemoved(Dictionary<string, object> message)
	{
		ModManager.RemoveMod(((SteamWorkshopItem)message["item"]).Id);
	}

	private static void Event_OnModsPluginEnabled(Dictionary<string, object> message)
	{
		Plugin plugin = (Plugin)message["plugin"];
		bool? flag = ModManager.GetPluginById(plugin.Id)?.Enable();
		if (flag.HasValue && flag.Value)
		{
			ModManager.SetModStatus(plugin.Id, isEnabled: true);
		}
	}

	private static void Event_OnModsPluginDisabled(Dictionary<string, object> message)
	{
		Plugin plugin = (Plugin)message["plugin"];
		bool? flag = ModManager.GetPluginById(plugin.Id)?.Disable();
		if (flag.HasValue && flag.Value)
		{
			ModManager.SetModStatus(plugin.Id, isEnabled: false);
		}
	}

	private static void Event_OnModsModEnabled(Dictionary<string, object> message)
	{
		Mod mod = (Mod)message["mod"];
		bool? flag = ModManager.GetModById(mod.Id)?.Enable();
		if (flag.HasValue && flag.Value)
		{
			ModManager.SetModStatus(mod.Id, isEnabled: true);
		}
	}

	private static void Event_OnModsModDisabled(Dictionary<string, object> message)
	{
		Mod mod = (Mod)message["mod"];
		bool? flag = ModManager.GetModById(mod.Id)?.Disable();
		if (flag.HasValue && flag.Value)
		{
			ModManager.SetModStatus(mod.Id, isEnabled: false);
		}
	}

	private static void Event_OnModStateChanged(Dictionary<string, object> message)
	{
		Mod mod = (Mod)message["mod"];
		BasePluginState basePluginState = (BasePluginState)message["oldState"];
		BasePluginState basePluginState2 = (BasePluginState)message["newState"];
		if (basePluginState.IsReady != basePluginState2.IsReady && basePluginState2.IsReady)
		{
			if (ApplicationManager.IsDedicatedGameServer)
			{
				mod.Enable();
			}
			else if (ModManager.GetModStatus(mod.Id) && !GlobalStateManager.ReconnectionState.IsPendingModId(mod.Id))
			{
				mod.Enable();
			}
		}
	}

	private static void Event_OnPluginStateChanged(Dictionary<string, object> message)
	{
		Plugin plugin = (Plugin)message["plugin"];
		BasePluginState basePluginState = (BasePluginState)message["oldState"];
		BasePluginState basePluginState2 = (BasePluginState)message["newState"];
		if (basePluginState.IsReady != basePluginState2.IsReady && basePluginState2.IsReady)
		{
			if (ApplicationManager.IsDedicatedGameServer)
			{
				plugin.Enable();
			}
			else if (ModManager.GetModStatus(plugin.Id))
			{
				plugin.Enable();
			}
		}
	}

	private static void Event_OnConnectionStateChanged(Dictionary<string, object> message)
	{
		ConnectionState connectionState = (ConnectionState)message["newConnectionState"];
		if (((ConnectionState)message["oldConnectionState"]).Phase != connectionState.Phase && connectionState.Phase == ConnectionPhase.Disconnected)
		{
			ModManager.ApplyModStatus();
		}
	}

	private static void Event_OnReconnectionStateChanged(Dictionary<string, object> message)
	{
		ReconnectionState reconnectionState = (ReconnectionState)message["newReconnectionState"];
		ReconnectionState reconnectionState2 = (ReconnectionState)message["oldReconnectionState"];
		switch (reconnectionState.Phase)
		{
		case ReconnectionPhase.None:
			if (reconnectionState2.Phase == ReconnectionPhase.AwaitingMods && reconnectionState2.PendingModIds.Length != 0)
			{
				ModManager.ApplyModStatus();
			}
			break;
		case ReconnectionPhase.AwaitingMods:
			if (!reconnectionState2.PendingEnablingModIds.SequenceEqual(reconnectionState.PendingEnablingModIds) && reconnectionState.PendingReadinessModIds.Length == 0)
			{
				ModManager.GetModById(reconnectionState.PendingEnablingModIds.FirstOrDefault())?.Enable();
			}
			break;
		}
	}

	private static void Event_Server_OnServerStarted(Dictionary<string, object> message)
	{
		if (ApplicationManager.IsDedicatedGameServer)
		{
			ModManager.LoadPlugins();
		}
	}
}
