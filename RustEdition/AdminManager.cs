using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class AdminManager : MonoBehaviourSingleton<AdminManager>
{
	private static readonly Logger Logger = new Logger("AdminManager");

	private List<string> adminSteamIds = new List<string>();

	private string adminSteamIdsFilePath;

	private FileSystemWatcher adminSteamIdsWatcher;

	public override void Awake()
	{
		base.Awake();
		adminSteamIdsFilePath = Utils.GetCommandLineArgument("--adminSteamIdsPath") ?? "./admin_steam_ids.json";
		adminSteamIdsFilePath = Path.GetFullPath(adminSteamIdsFilePath);
	}

	public void Dispose()
	{
		adminSteamIds.Clear();
		if (adminSteamIdsWatcher != null)
		{
			adminSteamIdsWatcher.Dispose();
			adminSteamIdsWatcher = null;
		}
	}

	public void LoadAdminSteamIds()
	{
		if (!File.Exists(adminSteamIdsFilePath))
		{
			Logger.Warning("Admin steam ids file not found at " + adminSteamIdsFilePath + ", creating default...");
			File.AppendAllText(adminSteamIdsFilePath, JsonSerializer.Serialize(new List<string>(), new JsonSerializerOptions
			{
				WriteIndented = true
			}));
		}
		ReadAdminSteamIds();
		WatchAdminSteamIds(adminSteamIdsFilePath);
	}

	public void SaveAdminSteamIds()
	{
		File.WriteAllText(adminSteamIdsFilePath, JsonSerializer.Serialize(adminSteamIds, new JsonSerializerOptions
		{
			WriteIndented = true
		}));
	}

	public void ReadAdminSteamIds()
	{
		if (ConfigUtils.TryLoadFromFile<List<string>>(adminSteamIdsFilePath, out var result))
		{
			adminSteamIds = result ?? new List<string>();
		}
		else
		{
			Logger.Error("Could not load admin Steam IDs (see the error above); keeping the previous list.");
		}
	}

	public void WatchAdminSteamIds(string adminSteamIdsFilePath)
	{
		if (adminSteamIdsWatcher == null)
		{
			string fileName = Path.GetFileName(adminSteamIdsFilePath);
			string path = adminSteamIdsFilePath.Replace(fileName, string.Empty);
			adminSteamIdsWatcher = new FileSystemWatcher(path);
			adminSteamIdsWatcher.NotifyFilter = NotifyFilters.LastWrite;
			adminSteamIdsWatcher.Filter = fileName;
			adminSteamIdsWatcher.EnableRaisingEvents = true;
			adminSteamIdsWatcher.Changed += OnAdminSteamIdsFileChanged;
			Logger.Info("Watching admin Steam IDs file " + fileName);
		}
	}

	private void OnAdminSteamIdsFileChanged(object sender, FileSystemEventArgs e)
	{
		Logger.Info("Admin Steam IDs file changed: " + e.FullPath);
		if (ConfigUtils.TryLoadFromFile<List<string>>(e.FullPath, out var result))
		{
			adminSteamIds = result ?? new List<string>();
		}
	}

	public void AddAdminSteamId(string steamId)
	{
		if (!adminSteamIds.Contains(steamId))
		{
			adminSteamIds.Add(steamId);
			SaveAdminSteamIds();
		}
	}

	public void RemoveAdminSteamId(string steamId)
	{
		if (adminSteamIds.Contains(steamId))
		{
			adminSteamIds.Remove(steamId);
			SaveAdminSteamIds();
		}
	}

	public bool IsSteamIdAdmin(string steamId)
	{
		return adminSteamIds.Contains(steamId);
	}
}
