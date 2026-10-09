using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class WhitelistManager : MonoBehaviourSingleton<WhitelistManager>
{
	private static readonly Logger Logger = new Logger("WhitelistManager");

	private List<string> whitelistedSteamIds = new List<string>();

	private string whitelistedSteamIdsFilePath;

	private FileSystemWatcher whitelistedSteamIdsWatcher;

	public override void Awake()
	{
		base.Awake();
		whitelistedSteamIdsFilePath = Utils.GetCommandLineArgument("--whitelistedSteamIdsPath") ?? "./whitelisted_steam_ids.json";
		whitelistedSteamIdsFilePath = Path.GetFullPath(whitelistedSteamIdsFilePath);
	}

	public void Dispose()
	{
		whitelistedSteamIds.Clear();
		if (whitelistedSteamIdsWatcher != null)
		{
			whitelistedSteamIdsWatcher.Dispose();
			whitelistedSteamIdsWatcher = null;
		}
	}

	public void LoadWhitelistedSteamIds()
	{
		if (!File.Exists(whitelistedSteamIdsFilePath))
		{
			Logger.Warning("Whitelisted Steam IDs file not found at " + whitelistedSteamIdsFilePath + ", creating default...");
			File.AppendAllText(whitelistedSteamIdsFilePath, JsonSerializer.Serialize(new List<string>(), new JsonSerializerOptions
			{
				WriteIndented = true
			}));
		}
		ReadWhitelistedSteamIds();
		WatchWhitelistedSteamIds(whitelistedSteamIdsFilePath);
	}

	public void SaveWhitelistedSteamIds()
	{
		File.WriteAllText(whitelistedSteamIdsFilePath, JsonSerializer.Serialize(whitelistedSteamIds, new JsonSerializerOptions
		{
			WriteIndented = true
		}));
	}

	public void ReadWhitelistedSteamIds()
	{
		if (ConfigUtils.TryLoadFromFile<List<string>>(whitelistedSteamIdsFilePath, out var result))
		{
			whitelistedSteamIds = result ?? new List<string>();
		}
		else
		{
			Logger.Error("Could not load whitelisted Steam IDs (see the error above); keeping the previous list.");
		}
	}

	public void WatchWhitelistedSteamIds(string whitelistedSteamIdsFilePath)
	{
		if (whitelistedSteamIdsWatcher == null)
		{
			string fileName = Path.GetFileName(whitelistedSteamIdsFilePath);
			string path = whitelistedSteamIdsFilePath.Replace(fileName, string.Empty);
			whitelistedSteamIdsWatcher = new FileSystemWatcher(path);
			whitelistedSteamIdsWatcher.NotifyFilter = NotifyFilters.LastWrite;
			whitelistedSteamIdsWatcher.Filter = fileName;
			whitelistedSteamIdsWatcher.EnableRaisingEvents = true;
			whitelistedSteamIdsWatcher.Changed += OnWhitelistedSteamIdsFileChanged;
			Logger.Info("Watching whitelisted Steam IDs file " + fileName);
		}
	}

	public void AddWhitelistedSteamId(string steamId)
	{
		if (!whitelistedSteamIds.Contains(steamId))
		{
			whitelistedSteamIds.Add(steamId);
			SaveWhitelistedSteamIds();
		}
	}

	public void AddWhitelistedSteamIds(params string[] steamIds)
	{
		bool flag = false;
		foreach (string item in steamIds)
		{
			if (!whitelistedSteamIds.Contains(item))
			{
				whitelistedSteamIds.Add(item);
				flag = true;
			}
		}
		if (flag)
		{
			SaveWhitelistedSteamIds();
		}
	}

	public void RemoveWhitelistedSteamId(string steamId)
	{
		if (whitelistedSteamIds.Contains(steamId))
		{
			whitelistedSteamIds.Remove(steamId);
			SaveWhitelistedSteamIds();
		}
	}

	public void RemoveWhitelistedSteamIds(params string[] steamIds)
	{
		bool flag = false;
		foreach (string item in steamIds)
		{
			if (whitelistedSteamIds.Contains(item))
			{
				whitelistedSteamIds.Remove(item);
				flag = true;
			}
		}
		if (flag)
		{
			SaveWhitelistedSteamIds();
		}
	}

	public bool IsSteamIdWhitelisted(string steamId)
	{
		return whitelistedSteamIds.Contains(steamId);
	}

	private void OnWhitelistedSteamIdsFileChanged(object sender, FileSystemEventArgs e)
	{
		Logger.Info("Whitelisted Steam IDs file changed: " + e.FullPath);
		if (ConfigUtils.TryLoadFromFile<List<string>>(e.FullPath, out var result))
		{
			whitelistedSteamIds = result ?? new List<string>();
		}
	}
}
