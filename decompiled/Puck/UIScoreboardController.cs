using System.Collections.Generic;

internal class UIScoreboardController : UIViewController<UIScoreboard>
{
	private UIScoreboard uiScoreboard;

	public override void Awake()
	{
		base.Awake();
		uiScoreboard = GetComponent<UIScoreboard>();
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			EventManager.AddEventListener("Event_Everyone_OnPlayerAdded", Event_Everyone_OnPlayerAdded);
			EventManager.AddEventListener("Event_Everyone_OnPlayerRemoved", Event_Everyone_OnPlayerRemoved);
			EventManager.AddEventListener("Event_Everyone_OnPlayerGameStateChanged", Event_Everyone_OnPlayerGameStateChanged);
			EventManager.AddEventListener("Event_Everyone_OnPlayerUsernameChanged", Event_Everyone_OnPlayerUsernameChanged);
			EventManager.AddEventListener("Event_Everyone_OnPlayerGoalsChanged", Event_Everyone_OnPlayerGoalsChanged);
			EventManager.AddEventListener("Event_Everyone_OnPlayerAssistsChanged", Event_Everyone_OnPlayerAssistsChanged);
			EventManager.AddEventListener("Event_Everyone_OnPlayerPingChanged", Event_Everyone_OnPlayerPingChanged);
			EventManager.AddEventListener("Event_Everyone_OnPlayerPositionChanged", Event_Everyone_OnPlayerPositionChanged);
			EventManager.AddEventListener("Event_Everyone_OnPlayerPatreonLevelChanged", Event_Everyone_OnPlayerPatreonLevelChanged);
			EventManager.AddEventListener("Event_Everyone_OnPlayerAdminLevelChanged", Event_Everyone_OnPlayerAdminLevelChanged);
			EventManager.AddEventListener("Event_Everyone_OnPlayerSteamIdChanged", Event_Everyone_OnPlayerSteamIdChanged);
			EventManager.AddEventListener("Event_Everyone_OnServerChanged", Event_Everyone_OnServerChanged);
		}
	}

	public override void OnDestroy()
	{
		if (ApplicationManager.IsDedicatedGameServer)
		{
			base.OnDestroy();
			return;
		}
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerAdded", Event_Everyone_OnPlayerAdded);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerRemoved", Event_Everyone_OnPlayerRemoved);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerGameStateChanged", Event_Everyone_OnPlayerGameStateChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerUsernameChanged", Event_Everyone_OnPlayerUsernameChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerGoalsChanged", Event_Everyone_OnPlayerGoalsChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerAssistsChanged", Event_Everyone_OnPlayerAssistsChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerPingChanged", Event_Everyone_OnPlayerPingChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerPositionChanged", Event_Everyone_OnPlayerPositionChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerPatreonLevelChanged", Event_Everyone_OnPlayerPatreonLevelChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerAdminLevelChanged", Event_Everyone_OnPlayerAdminLevelChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerSteamIdChanged", Event_Everyone_OnPlayerSteamIdChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnServerChanged", Event_Everyone_OnServerChanged);
		base.OnDestroy();
	}

	private void Event_Everyone_OnPlayerAdded(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		if (!player.IsReplay.Value)
		{
			uiScoreboard.AddPlayer(player);
			uiScoreboard.StyleServer(NetworkBehaviourSingleton<ServerManager>.Instance.Server.Value, MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayers().Count);
		}
	}

	private void Event_Everyone_OnPlayerRemoved(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		if (!player.IsReplay.Value)
		{
			uiScoreboard.RemovePlayer(player);
			uiScoreboard.StyleServer(NetworkBehaviourSingleton<ServerManager>.Instance.Server.Value, MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayers().Count);
		}
	}

	private void Event_Everyone_OnPlayerGameStateChanged(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		uiScoreboard.StylePlayer(player);
	}

	private void Event_Everyone_OnPlayerUsernameChanged(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		uiScoreboard.StylePlayer(player);
	}

	private void Event_Everyone_OnPlayerGoalsChanged(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		uiScoreboard.StylePlayer(player);
	}

	private void Event_Everyone_OnPlayerAssistsChanged(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		uiScoreboard.StylePlayer(player);
	}

	private void Event_Everyone_OnPlayerPingChanged(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		uiScoreboard.UpdatePlayerPing(player);
	}

	private void Event_Everyone_OnPlayerPositionChanged(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		uiScoreboard.StylePlayer(player);
	}

	private void Event_Everyone_OnPlayerPatreonLevelChanged(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		uiScoreboard.StylePlayer(player);
	}

	private void Event_Everyone_OnPlayerAdminLevelChanged(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		uiScoreboard.StylePlayer(player);
	}

	private void Event_Everyone_OnPlayerSteamIdChanged(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		uiScoreboard.StylePlayer(player);
	}

	private void Event_Everyone_OnServerChanged(Dictionary<string, object> message)
	{
		uiScoreboard.StyleServer(NetworkBehaviourSingleton<ServerManager>.Instance.Server.Value, MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayers().Count);
	}
}
