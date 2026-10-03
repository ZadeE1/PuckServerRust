using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class Mod : BasePlugin<BasePluginState>
{
	private static readonly Logger Logger = new Logger("Mod");

	public readonly SteamWorkshopItem SteamWorkshopItem;

	public string Id => SteamWorkshopItem.Id;

	public bool HasLocalFiles
	{
		get
		{
			if (!string.IsNullOrEmpty(Path) && Directory.Exists(Path))
			{
				return Directory.EnumerateFileSystemEntries(Path).Any();
			}
			return false;
		}
	}

	public Mod(SteamWorkshopItem steamWorkshopItem)
		: base(new BasePluginState
		{
			Path = null,
			IsReady = false,
			IsEnabled = false
		})
	{
		SteamWorkshopItem = steamWorkshopItem;
	}

	public override void Initialize()
	{
		base.Initialize();
		SteamWorkshopItem steamWorkshopItem = SteamWorkshopItem;
		steamWorkshopItem.StateChanged = (Action<SteamWorkshopItemState, SteamWorkshopItemState>)Delegate.Combine(steamWorkshopItem.StateChanged, new Action<SteamWorkshopItemState, SteamWorkshopItemState>(OnSteamWorkshopItemStateChanged));
		SteamWorkshopItem steamWorkshopItem2 = SteamWorkshopItem;
		steamWorkshopItem2.DetailsStateChanged = (Action<SteamWorkshopItemDetailsState, SteamWorkshopItemDetailsState>)Delegate.Combine(steamWorkshopItem2.DetailsStateChanged, new Action<SteamWorkshopItemDetailsState, SteamWorkshopItemDetailsState>(OnSteamWorkshopItemDetailsStateChanged));
		SetState(new Dictionary<string, object>
		{
			{ "path", SteamWorkshopItem.Path },
			{
				"isReady",
				SteamWorkshopItem.Phase == SteamWorkshopItemPhase.Installed
			}
		});
	}

	public override void OnEnableFailed(Exception exception)
	{
		base.OnEnableFailed(exception);
		Logger.Error("Failed to enable mod " + Id + ": " + exception.Message);
		EventManager.TriggerEvent("Event_OnModEnableFailed", new Dictionary<string, object> { { "mod", this } });
	}

	public override void OnDisableFailed(Exception exception)
	{
		base.OnDisableFailed(exception);
		Logger.Error("Failed to disable mod " + Id + ": " + exception.Message);
		EventManager.TriggerEvent("Event_OnModDisableFailed", new Dictionary<string, object> { { "mod", this } });
	}

	protected override void OnStateChanged(BasePluginState oldState, BasePluginState newState)
	{
		base.OnStateChanged(oldState, newState);
		EventManager.TriggerEvent("Event_OnModStateChanged", new Dictionary<string, object>
		{
			{ "mod", this },
			{ "oldState", oldState },
			{ "newState", newState }
		});
	}

	private void OnSteamWorkshopItemStateChanged(SteamWorkshopItemState oldState, SteamWorkshopItemState newState)
	{
		EventManager.TriggerEvent("Event_OnModSteamWorkshopItemStateChanged", new Dictionary<string, object>
		{
			{ "mod", this },
			{ "oldState", oldState },
			{ "newState", newState }
		});
		SetState(new Dictionary<string, object>
		{
			{ "path", newState.Path },
			{
				"isReady",
				newState.Phase == SteamWorkshopItemPhase.Installed
			}
		});
	}

	private void OnSteamWorkshopItemDetailsStateChanged(SteamWorkshopItemDetailsState oldState, SteamWorkshopItemDetailsState newState)
	{
		EventManager.TriggerEvent("Event_OnModSteamWorkshopItemDetailsStateChanged", new Dictionary<string, object>
		{
			{ "mod", this },
			{ "oldState", oldState },
			{ "newState", newState }
		});
	}
}
