using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class BanManager : MonoBehaviourSingleton<BanManager>
{
	private static readonly Logger Logger = new Logger("BanManager");

	private List<string> bannedSteamIds = new List<string>();

	private string bannedSteamIdsFilePath;

	private FileSystemWatcher bannedSteamIdsWatcher;

	private List<string> bannedIpAddresses = new List<string>();

	private string bannedIpAddressesFilePath;

	private FileSystemWatcher bannedIpAddressesWatcher;

	public override void Awake()
	{
		base.Awake();
		bannedSteamIdsFilePath = Utils.GetCommandLineArgument("--bannedSteamIdsPath") ?? "./banned_steam_ids.json";
		bannedSteamIdsFilePath = Path.GetFullPath(bannedSteamIdsFilePath);
		bannedIpAddressesFilePath = Utils.GetCommandLineArgument("--bannedIpAddressesPath") ?? "./banned_ip_addresses.json";
		bannedIpAddressesFilePath = Path.GetFullPath(bannedIpAddressesFilePath);
	}

	public void Dispose()
	{
		bannedSteamIds.Clear();
		if (bannedSteamIdsWatcher != null)
		{
			bannedSteamIdsWatcher.Dispose();
			bannedSteamIdsWatcher = null;
		}
		bannedIpAddresses.Clear();
		if (bannedIpAddressesWatcher != null)
		{
			bannedIpAddressesWatcher.Dispose();
			bannedIpAddressesWatcher = null;
		}
	}

	public void LoadBannedSteamIds()
	{
		if (!File.Exists(bannedSteamIdsFilePath))
		{
			Logger.Warning("Banned steam ids file not found at " + bannedSteamIdsFilePath + ", creating default...");
			File.AppendAllText(bannedSteamIdsFilePath, JsonSerializer.Serialize(new List<string>(), new JsonSerializerOptions
			{
				WriteIndented = true
			}));
		}
		ReadBannedSteamIds();
		WatchBannedSteamIds(bannedSteamIdsFilePath);
	}

	public void SaveBannedSteamIds()
	{
		File.WriteAllText(bannedSteamIdsFilePath, JsonSerializer.Serialize(bannedSteamIds, new JsonSerializerOptions
		{
			WriteIndented = true
		}));
	}

	public void ReadBannedSteamIds()
	{
		if (ConfigUtils.TryLoadFromFile<List<string>>(bannedSteamIdsFilePath, out var result))
		{
			bannedSteamIds = result ?? new List<string>();
		}
		else
		{
			Logger.Error("Could not load banned Steam IDs (see the error above); keeping the previous list.");
		}
	}

	public void WatchBannedSteamIds(string bannedSteamIdsFilePath)
	{
		if (bannedSteamIdsWatcher == null)
		{
			string fileName = Path.GetFileName(bannedSteamIdsFilePath);
			string path = bannedSteamIdsFilePath.Replace(fileName, string.Empty);
			bannedSteamIdsWatcher = new FileSystemWatcher(path);
			bannedSteamIdsWatcher.NotifyFilter = NotifyFilters.LastWrite;
			bannedSteamIdsWatcher.Filter = fileName;
			bannedSteamIdsWatcher.EnableRaisingEvents = true;
			bannedSteamIdsWatcher.Changed += OnBannedSteamIdsFileChanged;
			Logger.Info("Watching banned Steam IDs file " + fileName);
		}
	}

	public void AddBannedSteamId(string steamId)
	{
		if (!bannedSteamIds.Contains(steamId))
		{
			bannedSteamIds.Add(steamId);
			SaveBannedSteamIds();
		}
	}

	public void RemoveBannedSteamId(string steamId)
	{
		if (bannedSteamIds.Contains(steamId))
		{
			bannedSteamIds.Remove(steamId);
			SaveBannedSteamIds();
		}
	}

	public bool IsSteamIdBanned(string steamId)
	{
		return bannedSteamIds.Contains(steamId);
	}

	public void LoadBannedIpAddresses()
	{
		if (!File.Exists(bannedIpAddressesFilePath))
		{
			Logger.Warning("Banned IP addresses file not found at " + bannedIpAddressesFilePath + ", creating default...");
			File.AppendAllText(bannedIpAddressesFilePath, JsonSerializer.Serialize(new List<string>(), new JsonSerializerOptions
			{
				WriteIndented = true
			}));
		}
		ReadBannedIpAddresses();
		WatchBannedIpAddresses(bannedIpAddressesFilePath);
	}

	public void SaveBannedIpAddresses()
	{
		File.WriteAllText(bannedIpAddressesFilePath, JsonSerializer.Serialize(bannedIpAddresses, new JsonSerializerOptions
		{
			WriteIndented = true
		}));
	}

	public void ReadBannedIpAddresses()
	{
		if (ConfigUtils.TryLoadFromFile<List<string>>(bannedIpAddressesFilePath, out var result))
		{
			bannedIpAddresses = result ?? new List<string>();
		}
		else
		{
			Logger.Error("Could not load banned IP addresses (see the error above); keeping the previous list.");
		}
	}

	public void WatchBannedIpAddresses(string bannedIpAddressesFilePath)
	{
		if (bannedIpAddressesWatcher == null)
		{
			string fileName = Path.GetFileName(bannedIpAddressesFilePath);
			string path = bannedIpAddressesFilePath.Replace(fileName, string.Empty);
			bannedIpAddressesWatcher = new FileSystemWatcher(path);
			bannedIpAddressesWatcher.NotifyFilter = NotifyFilters.LastWrite;
			bannedIpAddressesWatcher.Filter = fileName;
			bannedIpAddressesWatcher.Changed += OnBannedIpAddressesFileChanged;
			bannedIpAddressesWatcher.EnableRaisingEvents = true;
			Logger.Info("Watching banned IP addresses file " + fileName);
		}
	}

	public void AddBannedIpAddress(string ipAddress)
	{
		if (!bannedIpAddresses.Contains(ipAddress))
		{
			bannedIpAddresses.Add(ipAddress);
			SaveBannedIpAddresses();
		}
	}

	public void RemoveBannedIpAddress(string ipAddress)
	{
		if (bannedIpAddresses.Contains(ipAddress))
		{
			bannedIpAddresses.Remove(ipAddress);
			SaveBannedIpAddresses();
		}
	}

	public bool IsIpAddressBanned(string ipAddress)
	{
		return bannedIpAddresses.Contains(ipAddress);
	}

	private void OnBannedSteamIdsFileChanged(object sender, FileSystemEventArgs e)
	{
		Logger.Info("Banned Steam IDs file changed: " + e.FullPath);
		if (ConfigUtils.TryLoadFromFile<List<string>>(e.FullPath, out var result))
		{
			bannedSteamIds = result ?? new List<string>();
		}
	}

	private void OnBannedIpAddressesFileChanged(object sender, FileSystemEventArgs e)
	{
		Logger.Info("Banned IP addresses file changed: " + e.FullPath);
		if (ConfigUtils.TryLoadFromFile<List<string>>(e.FullPath, out var result))
		{
			bannedIpAddresses = result ?? new List<string>();
		}
	}
}
