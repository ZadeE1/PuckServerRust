using System;
using System.Collections.Generic;

public class Plugin : BasePlugin<BasePluginState>
{
	private static readonly Logger Logger = new Logger("Plugin");

	public readonly string Id;

	public Plugin(string id, string path)
		: base(new BasePluginState
		{
			Path = path,
			IsEnabled = false
		})
	{
		Id = id;
	}

	public override void Initialize()
	{
		base.Initialize();
		SetState(new Dictionary<string, object> { { "isReady", true } });
	}

	public override void OnEnableFailed(Exception exception)
	{
		base.OnEnableFailed(exception);
		Logger.Error("Failed to enable plugin " + Id + ": " + exception.Message);
		EventManager.TriggerEvent("Event_OnPluginEnableFailed", new Dictionary<string, object> { { "plugin", this } });
	}

	public override void OnDisableFailed(Exception exception)
	{
		base.OnDisableFailed(exception);
		Logger.Error("Failed to disable plugin " + Id + ": " + exception.Message);
		EventManager.TriggerEvent("Event_OnPluginDisableFailed", new Dictionary<string, object> { { "plugin", this } });
	}

	protected override void OnStateChanged(BasePluginState oldState, BasePluginState newState)
	{
		base.OnStateChanged(oldState, newState);
		EventManager.TriggerEvent("Event_OnPluginStateChanged", new Dictionary<string, object>
		{
			{ "plugin", this },
			{ "oldState", oldState },
			{ "newState", newState }
		});
	}
}
