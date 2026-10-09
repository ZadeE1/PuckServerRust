using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

public static class ModManager
{
	private static readonly Logger Logger = new Logger("ModManager");

	public static List<Plugin> Plugins = new List<Plugin>();

	public static List<Mod> Mods = new List<Mod>();

	public static Dictionary<string, bool> ModsStatus = new Dictionary<string, bool>();

	public static Plugin[] ReadyPlugins => Plugins.Where((Plugin plugin) => plugin.IsReady).ToArray();

	public static Plugin[] EnabledPlugins => Plugins.Where((Plugin plugin) => plugin.IsEnabled).ToArray();

	public static Mod[] ReadyMods => Mods.Where((Mod mod) => mod.IsReady).ToArray();

	public static Mod[] EnabledMods => Mods.Where((Mod mod) => mod.IsEnabled).ToArray();

	private static string pluginsDirectoryPath => Path.Combine(Path.GetFullPath("."), "Plugins");

	public static void Initialize()
	{
		ModManagerController.Initialize();
	}

	public static void Dispose()
	{
		ModManagerController.Dispose();
	}

	public static void LoadModsStatus()
	{
		try
		{
			string text = SaveManager.GetString("modsStatus", null);
			if (string.IsNullOrEmpty(text))
			{
				throw new Exception("No mods status found");
			}
			ModsStatus = JsonSerializer.Deserialize<Dictionary<string, bool>>(text);
		}
		catch (Exception ex)
		{
			Logger.Error("Failed to load mods status: " + ex.Message);
			SaveModsStatus();
			LoadModsStatus();
		}
	}

	public static bool GetModStatus(string id)
	{
		if (ModsStatus.TryGetValue(id, out var value))
		{
			return value;
		}
		return false;
	}

	public static void SetModStatus(string id, bool isEnabled)
	{
		ModsStatus[id] = isEnabled;
		SaveModsStatus();
	}

	private static void SaveModsStatus()
	{
		try
		{
			string value = JsonSerializer.Serialize(ModsStatus);
			SaveManager.SetString("modsStatus", value);
		}
		catch (Exception ex)
		{
			Logger.Error("Failed to save mods status: " + ex.Message);
		}
	}

	public static void ApplyModStatus()
	{
		foreach (Mod mod in Mods)
		{
			if (GetModStatus(mod.Id))
			{
				mod.Enable();
			}
			if (!GetModStatus(mod.Id))
			{
				mod.Disable();
			}
		}
	}

	public static void LoadPlugins()
	{
		Logger.Info("Loading plugins from " + pluginsDirectoryPath);
		if (!Directory.Exists(pluginsDirectoryPath))
		{
			Directory.CreateDirectory(pluginsDirectoryPath);
		}
		string[] directories = Directory.GetDirectories(pluginsDirectoryPath);
		foreach (string path in directories)
		{
			AddPlugin(Path.GetFileName(path), path);
		}
	}

	public static Plugin AddPlugin(string id, string path)
	{
		Logger.Info("Adding plugin " + id);
		if (GetPluginById(id) != null)
		{
			return null;
		}
		Plugin plugin = new Plugin(id, path);
		Plugins.Add(plugin);
		EventManager.TriggerEvent("Event_OnPluginAdded", new Dictionary<string, object> { { "plugin", plugin } });
		plugin.Initialize();
		return plugin;
	}

	public static Plugin RemovePlugin(string id)
	{
		Logger.Info("Removing plugin " + id);
		Plugin pluginById = GetPluginById(id);
		if (pluginById == null)
		{
			return null;
		}
		Plugins.Remove(pluginById);
		EventManager.TriggerEvent("Event_OnPluginRemoved", new Dictionary<string, object> { { "plugin", pluginById } });
		pluginById.Dispose();
		return pluginById;
	}

	public static Mod AddMod(SteamWorkshopItem item)
	{
		Logger.Info("Adding mod " + item.Id);
		if (GetModById(item.Id) != null)
		{
			return null;
		}
		Mod mod = new Mod(item);
		Mods.Add(mod);
		EventManager.TriggerEvent("Event_OnModAdded", new Dictionary<string, object> { { "mod", mod } });
		mod.Initialize();
		return mod;
	}

	public static Mod RemoveMod(string id)
	{
		Logger.Info("Removing mod " + id);
		Mod modById = GetModById(id);
		if (modById == null)
		{
			return null;
		}
		Mods.Remove(modById);
		EventManager.TriggerEvent("Event_OnModRemoved", new Dictionary<string, object> { { "mod", modById } });
		modById.Dispose();
		return modById;
	}

	public static Plugin GetPluginById(string id)
	{
		return Plugins.Find((Plugin plugin) => plugin.Id == id);
	}

	public static Mod GetModById(string id)
	{
		return Mods.Find((Mod mod) => mod.Id == id);
	}
}
