using System.Collections.Generic;
using System.Linq;

public class MatchableGameMode<TConfig> : StandardGameMode<TConfig> where TConfig : StandardGameModeConfig, new()
{
	protected bool isMatchStarted;

	private Dictionary<string, MatchAbandonment> steamIdMatchAbandonmentMap = new Dictionary<string, MatchAbandonment>();

	protected MatchData matchData => BackendManager.ServerState.MatchData;

	protected bool isMatch => matchData != null;

	protected IEnumerable<string> abandonedSteamIds => from m in steamIdMatchAbandonmentMap.Values
		where m.HasAbandoned
		select m.SteamId;

	public MatchableGameMode(string defaultConfigFilePath, string configFilePathCliArgument = null, string configCliArgument = null, string configEnvVariable = null)
		: base(defaultConfigFilePath, configFilePathCliArgument, configCliArgument, configEnvVariable)
	{
	}

	public override bool Initialize(Level level, ServerManager serverManager, GameManager gameManager, PlayerManager playerManager, PuckManager puckManager, ChatManager chatManager, ReplayManager replayManager, VoteManager voteManager)
	{
		if (!base.Initialize(level, serverManager, gameManager, playerManager, puckManager, chatManager, replayManager, voteManager))
		{
			return false;
		}
		if (isMatch)
		{
			StartMatch();
		}
		return true;
	}

	protected override void SubscribeEvents()
	{
		base.SubscribeEvents();
		EventManager.AddEventListener("Event_OnServerMatchDataChanged", Event_OnServerMatchDataChanged);
	}

	protected override void UnsubscribeEvents()
	{
		base.UnsubscribeEvents();
		EventManager.RemoveEventListener("Event_OnServerMatchDataChanged", Event_OnServerMatchDataChanged);
	}

	private void StartMatch()
	{
		if (!isMatchStarted)
		{
			isMatchStarted = true;
			OnMatchStarted();
		}
	}

	private void AddMatchAbandonment(string steamId)
	{
		if (!steamIdMatchAbandonmentMap.ContainsKey(steamId))
		{
			MatchAbandonment matchAbandonment = new MatchAbandonment(steamId, 120f);
			matchAbandonment.Abandoned += OnMatchAbandoned;
			steamIdMatchAbandonmentMap.Add(steamId, matchAbandonment);
		}
	}

	private void ClearMatchAbandonments()
	{
		foreach (MatchAbandonment value in steamIdMatchAbandonmentMap.Values)
		{
			value.Dispose();
		}
		steamIdMatchAbandonmentMap.Clear();
	}

	private MatchAbandonment GetMatchAbandonment(Player player)
	{
		string key = player.SteamId.Value.ToString();
		if (!steamIdMatchAbandonmentMap.ContainsKey(key))
		{
			return null;
		}
		return steamIdMatchAbandonmentMap[key];
	}

	private void EndMatch()
	{
		if (isMatchStarted)
		{
			isMatchStarted = false;
			OnMatchEnded(gameResult);
		}
	}

	private void CancelMatch()
	{
		if (isMatchStarted)
		{
			isMatchStarted = false;
			OnMatchCancelled();
		}
	}

	private void IssueCooldown(string steamId)
	{
		if (isMatchStarted)
		{
			WebSocketManager.Emit("serverMatchIssueCooldown", new Dictionary<string, object> { { "steamId", steamId } });
			MatchPlayer matchPlayerBySteamId = matchData.GetMatchPlayerBySteamId(steamId);
			PlayerTeam? teamBySteamId = matchData.GetTeamBySteamId(steamId);
			if (matchPlayerBySteamId != null && teamBySteamId.HasValue)
			{
				ChatManager.Server_BroadcastChatMessage(StringUtils.WrapInTeamColor(matchPlayerBySteamId.username, teamBySteamId.Value) + " has been issued a matchmaking cooldown", "#b8b8b8");
			}
		}
	}

	private bool BroadcastForfeitWarning(Player player)
	{
		PlayerTeam? teamBySteamId = matchData.GetTeamBySteamId(player.SteamId.Value.ToString());
		if (!teamBySteamId.HasValue)
		{
			return false;
		}
		MatchAbandonment[] array = (from m in (from id in matchData.GetTeamSteamIds(teamBySteamId.Value)
				where steamIdMatchAbandonmentMap.ContainsKey(id)
				select steamIdMatchAbandonmentMap[id]).ToArray()
			where !m.HasAbandoned
			select m).ToArray();
		if (array.Length == 0 || array.Any((MatchAbandonment m) => m.IsPaused))
		{
			return false;
		}
		int num = (int)array.Max((MatchAbandonment m) => m.RemainingSeconds);
		ChatManager.Server_BroadcastChatMessage($"{StringUtils.WrapInTeamColor(Utils.GetNameFromTeam(teamBySteamId.Value), teamBySteamId.Value)} team will auto-forfeit in {num} seconds, once all of its players have received a matchmaking cooldown", "#b8b8b8");
		return true;
	}

	private void BroadcastAbandonmentWarning(Player player, MatchAbandonment matchAbandonment)
	{
		ChatManager.Server_BroadcastChatMessage($"{StringUtils.WrapInTeamColor(player.Username.Value.ToString(), player.Team)} has {(int)matchAbandonment.RemainingSeconds} seconds to reconnect before receiving a matchmaking cooldown", "#b8b8b8");
	}

	protected virtual void OnMatchStarted()
	{
		WebSocketManager.Emit("serverMatchStart");
		UpdatePlayerResult(matchData.SteamIds);
		ServerManager.WhitelistManager.AddWhitelistedSteamIds(matchData.SteamIds);
	}

	protected virtual void OnMatchEnded(GameResult gameResult)
	{
		WebSocketManager.Emit("serverMatchEnd", new Dictionary<string, object> { { "gameResult", gameResult } });
		ClearMatchAbandonments();
	}

	protected virtual void OnMatchCancelled()
	{
		WebSocketManager.Emit("serverMatchCancel");
		ClearMatchAbandonments();
		GameManager.Server_StopTicking();
	}

	protected virtual void OnMatchAbandoned(MatchAbandonment matchAbandonment)
	{
		if (isMatchStarted)
		{
			IssueCooldown(matchAbandonment.SteamId);
			string[] array = matchData.HomeSteamIds.Except(abandonedSteamIds).ToArray();
			string[] array2 = matchData.AwaySteamIds.Except(abandonedSteamIds).ToArray();
			if (array.Length == 0)
			{
				ForfeitGame(PlayerTeam.Blue);
			}
			else if (array2.Length == 0)
			{
				ForfeitGame(PlayerTeam.Red);
			}
		}
	}

	protected override void OnWarmupTimedOut()
	{
		if (isMatchStarted)
		{
			string[] second = (from p in PlayerManager.GetPlayers()
				select p.SteamId.Value.ToString()).ToArray();
			string[] array = matchData.SteamIds.Except(second).ToArray();
			string[] array2;
			if (array.Length != 0)
			{
				ChatManager.Server_BroadcastChatMessage("Cancelling match due to players failing to join in time", "#b8b8b8");
				if (array.Length < matchData.SteamIds.Length)
				{
					array2 = array;
					foreach (string steamId in array2)
					{
						IssueCooldown(steamId);
					}
				}
				CancelMatch();
				return;
			}
			array2 = matchData.SteamIds;
			foreach (string text in array2)
			{
				AddMatchAbandonment(text);
				steamIdMatchAbandonmentMap[text].Stop();
			}
		}
		base.OnWarmupTimedOut();
	}

	protected override void OnGameOverStarted()
	{
		base.OnGameOverStarted();
		ClearMatchAbandonments();
	}

	protected override void OnGameOverEnded()
	{
		base.OnGameOverEnded();
		if (isMatch)
		{
			EndMatch();
		}
	}

	protected override void OnPlayerJoined(Player player)
	{
		base.OnPlayerJoined(player);
		GetMatchAbandonment(player)?.Stop();
	}

	protected override void OnPlayerLeft(Player player)
	{
		base.OnPlayerLeft(player);
		MatchAbandonment matchAbandonment = GetMatchAbandonment(player);
		if (matchAbandonment != null)
		{
			matchAbandonment.Start();
			if (!matchAbandonment.HasAbandoned && !BroadcastForfeitWarning(player))
			{
				BroadcastAbandonmentWarning(player, matchAbandonment);
			}
		}
	}

	private void Event_OnServerMatchDataChanged(Dictionary<string, object> message)
	{
		MatchData matchData = (MatchData)message["oldServerMatchData"];
		MatchData matchData2 = (MatchData)message["newServerMatchData"];
		if (matchData == null && matchData2 != null)
		{
			StartMatch();
		}
	}
}
