using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class StandardGameMode<TConfig> : BaseGameMode<TConfig> where TConfig : StandardGameModeConfig, new()
{
	protected int tickRemainder;

	protected GameResult gameResult = new GameResult();

	protected CreaseProtection creaseProtection;

	protected bool isGameInProgress => Utils.IsGameInProgress(GameManager.Phase);

	protected bool isReplayable
	{
		get
		{
			if (GameManager.Phase != GamePhase.FaceOff && GameManager.Phase != GamePhase.Play && GameManager.Phase != GamePhase.BlueScore)
			{
				return GameManager.Phase == GamePhase.RedScore;
			}
			return true;
		}
	}

	protected virtual bool AllowVotekick => true;

	public StandardGameMode(string defaultConfigFilePath, string configFilePathCliArgument = null, string configCliArgument = null, string configEnvVariable = null)
		: base(defaultConfigFilePath, configFilePathCliArgument, configCliArgument, configEnvVariable)
	{
	}

	public override bool Initialize(Level level, ServerManager serverManager, GameManager gameManager, PlayerManager playerManager, PuckManager puckManager, ChatManager chatManager, ReplayManager replayManager, VoteManager voteManager)
	{
		if (!base.Initialize(level, serverManager, gameManager, playerManager, puckManager, chatManager, replayManager, voteManager))
		{
			return false;
		}
		NetworkBehaviourSingleton<GameModeManager>.Instance.ClientConfig.Value = new GameModeClientConfig
		{
			GoalieCreaseProtection = Config.goalieCreaseProtection,
			MaxPeriods = Config.maxPeriods
		};
		if (Config.goalieCreaseProtection)
		{
			creaseProtection = new CreaseProtection();
		}
		StartGame();
		GameManager.Server_StartTicking();
		return true;
	}

	public override bool Dispose()
	{
		if (!base.Dispose())
		{
			return false;
		}
		creaseProtection?.Dispose();
		creaseProtection = null;
		return true;
	}

	protected override void OnPlayerEnterZone(Zone zone, PlayerBody playerBody)
	{
		base.OnPlayerEnterZone(zone, playerBody);
		creaseProtection?.OnPlayerEnterZone(zone, playerBody);
	}

	protected override void OnPlayerExitZone(Zone zone, PlayerBody playerBody)
	{
		base.OnPlayerExitZone(zone, playerBody);
		creaseProtection?.OnPlayerExitZone(zone, playerBody);
	}

	protected bool CanPlayerEnterPhase(Player player, PlayerPhase phase)
	{
		switch (phase)
		{
		case PlayerPhase.TeamSelect:
			return true;
		case PlayerPhase.PositionSelect:
			if (player.Team != PlayerTeam.Blue)
			{
				return player.Team == PlayerTeam.Red;
			}
			return true;
		case PlayerPhase.Play:
		case PlayerPhase.Replay:
			if ((player.Team == PlayerTeam.Blue || player.Team == PlayerTeam.Red) && (player.Role == PlayerRole.Attacker || player.Role == PlayerRole.Goalie))
			{
				return player.PlayerPosition != null;
			}
			return false;
		case PlayerPhase.Spectate:
			return player.Team == PlayerTeam.Spectator;
		default:
			return false;
		}
	}

	protected virtual void PreparePlayersForGamePhase(GamePhase gamePhase)
	{
		foreach (Player player in PlayerManager.GetPlayers())
		{
			PreparePlayerForGamePhase(player, gamePhase);
		}
	}

	protected virtual void PreparePlayerForGamePhase(Player player, GamePhase gamePhase)
	{
		player.Server_CancelDelayedGameState();
		switch (gamePhase)
		{
		case GamePhase.Warmup:
			if (CanPlayerEnterPhase(player, PlayerPhase.Play))
			{
				player.Server_SetGameState(PlayerPhase.Play);
			}
			break;
		case GamePhase.PreGame:
			if (CanPlayerEnterPhase(player, PlayerPhase.PositionSelect))
			{
				player.Server_SetGameState(PlayerPhase.PositionSelect);
			}
			break;
		case GamePhase.FaceOff:
			if (player.Phase == PlayerPhase.Play)
			{
				player.Server_SpawnCharacter(player.PlayerPosition.transform.position, player.PlayerPosition.transform.rotation, player.Role);
			}
			else if (CanPlayerEnterPhase(player, PlayerPhase.Play))
			{
				player.Server_SetGameState(PlayerPhase.Play);
			}
			break;
		case GamePhase.Play:
			if (player.Phase != PlayerPhase.Play && CanPlayerEnterPhase(player, PlayerPhase.Play))
			{
				PlayerPhase? phase = PlayerPhase.Play;
				float? delay = Config.spawnDelay;
				player.Server_SetGameState(phase, null, null, delay);
				ChatManager.Server_SendChatMessage($"Spawning in {Config.spawnDelay} seconds...", "#ffe97f", player.OwnerClientId);
			}
			break;
		case GamePhase.Replay:
			if (CanPlayerEnterPhase(player, PlayerPhase.Replay))
			{
				player.Server_SetGameState(PlayerPhase.Replay);
			}
			break;
		}
		if (player.IsCharacterSpawned)
		{
			if (gamePhase == GamePhase.FaceOff)
			{
				player.PlayerBody.Server_Freeze((RigidbodyConstraints)10);
			}
			else
			{
				player.PlayerBody.Server_Unfreeze();
			}
		}
	}

	protected virtual void ClearGameResult()
	{
		PlayerManager.GetPlayers().ForEach((Player player) =>
		{
			player.Server_ResetPoints();
			if (CanPlayerEnterPhase(player, PlayerPhase.PositionSelect))
			{
				player.Server_SetGameState(PlayerPhase.PositionSelect);
				if (player.PlayerPosition != null)
				{
					player.PlayerPosition.Server_Unclaim();
				}
			}
		});
		gameResult = new GameResult();
	}

	protected virtual void UpdateGameResult(PlayerTeam? forfeitingTeam = null)
	{
		PlayerTeam winningTeam = (forfeitingTeam.HasValue ? Utils.GetOpposingTeam(forfeitingTeam.Value).GetValueOrDefault() : ((GameManager.BlueScore > GameManager.RedScore) ? PlayerTeam.Blue : ((GameManager.BlueScore < GameManager.RedScore) ? PlayerTeam.Red : PlayerTeam.None)));
		gameResult.winningTeam = winningTeam;
		gameResult.blueScore = GameManager.BlueScore;
		gameResult.redScore = GameManager.RedScore;
		gameResult.forfeit = forfeitingTeam.HasValue;
		foreach (Player player in PlayerManager.GetPlayers())
		{
			string key = player.SteamId.Value.ToString();
			if (!gameResult.playerResults.ContainsKey(key))
			{
				gameResult.playerResults[key] = new PlayerResult();
			}
			gameResult.playerResults[key].goals = player.Goals.Value;
			gameResult.playerResults[key].assists = player.Assists.Value;
		}
		UpdatePlayerResult();
	}

	protected virtual void UpdatePlayerResult(params string[] steamIds)
	{
		foreach (string key in steamIds)
		{
			if (!gameResult.playerResults.ContainsKey(key))
			{
				gameResult.playerResults[key] = new PlayerResult();
			}
		}
		foreach (Player player in PlayerManager.GetPlayers())
		{
			string key2 = player.SteamId.Value.ToString();
			if (!gameResult.playerResults.ContainsKey(key2))
			{
				gameResult.playerResults[key2] = new PlayerResult();
			}
			gameResult.playerResults[key2].goals = player.Goals.Value;
			gameResult.playerResults[key2].assists = player.Assists.Value;
		}
	}

	protected virtual void StartGame(GamePhase phase = GamePhase.Warmup)
	{
		ClearGameResult();
		UpdateGameResult();
		GameManager.Server_SetGameState(phase, Config.phaseDurationMap[phase], 1, 0, 0, false);
	}

	protected virtual void EndGame()
	{
		UpdateGameResult();
		GameManager.Server_SetGameState(GamePhase.GameOver, Config.phaseDurationMap[GamePhase.GameOver]);
	}

	protected virtual void ForfeitGame(PlayerTeam forfeitingTeam)
	{
		UpdateGameResult(forfeitingTeam);
		GameManager.Server_SetGameState(GamePhase.GameOver, Config.phaseDurationMap[GamePhase.GameOver]);
	}

	protected override void OnWarmupTimedOut()
	{
		base.OnWarmupTimedOut();
		GameManager.Server_SetGameState(GamePhase.PreGame, Config.phaseDurationMap[GamePhase.PreGame]);
	}

	protected override void OnPreGameTimedOut()
	{
		base.OnPreGameTimedOut();
		GameManager.Server_SetGameState(GamePhase.FaceOff, Config.phaseDurationMap[GamePhase.FaceOff]);
	}

	protected override void OnFaceOffTimedOut()
	{
		base.OnFaceOffTimedOut();
		GameManager.Server_SetGameState(GamePhase.Play, tickRemainder);
	}

	protected override void OnPlayTimedOut()
	{
		base.OnPlayTimedOut();
		if (GameManager.Period < Config.maxPeriods)
		{
			tickRemainder = Config.phaseDurationMap[GamePhase.Play];
			GameManager.Server_SetGameState(GamePhase.Intermission, Config.phaseDurationMap[GamePhase.Intermission], GameManager.Period + 1);
		}
		else if (GameManager.BlueScore == GameManager.RedScore)
		{
			tickRemainder = Config.phaseDurationMap[GamePhase.Play];
			GameManager gameManager = GameManager;
			GamePhase? phase = GamePhase.Intermission;
			int? tick = Config.phaseDurationMap[GamePhase.Intermission];
			int? period = GameManager.Period + 1;
			bool? isOvertime = true;
			gameManager.Server_SetGameState(phase, tick, period, null, null, isOvertime);
		}
		else
		{
			EndGame();
		}
	}

	protected override void OnBlueScoreTimedOut()
	{
		base.OnBlueScoreTimedOut();
		GameManager.Server_SetGameState(GamePhase.Replay, Config.phaseDurationMap[GamePhase.Replay]);
	}

	protected override void OnRedScoreTimedOut()
	{
		base.OnRedScoreTimedOut();
		GameManager.Server_SetGameState(GamePhase.Replay, Config.phaseDurationMap[GamePhase.Replay]);
	}

	protected override void OnReplayTimedOut()
	{
		base.OnReplayTimedOut();
		if (GameManager.IsOvertime)
		{
			EndGame();
		}
		else
		{
			GameManager.Server_SetGameState(GamePhase.FaceOff, Config.phaseDurationMap[GamePhase.FaceOff]);
		}
	}

	protected override void OnIntermissionTimedOut()
	{
		base.OnIntermissionTimedOut();
		GameManager.Server_SetGameState(GamePhase.FaceOff, Config.phaseDurationMap[GamePhase.FaceOff]);
	}

	protected override void OnGameOverTimedOut()
	{
		base.OnGameOverTimedOut();
		GameManager.Server_SetGameState(GamePhase.PostGame, Config.phaseDurationMap[GamePhase.PostGame]);
	}

	protected override void OnPostGameTimedOut()
	{
		base.OnPostGameTimedOut();
		StartGame();
	}

	protected override void OnGamePhaseStarted(GamePhase gamePhase)
	{
		base.OnGamePhaseStarted(gamePhase);
		if (isReplayable)
		{
			ReplayManager.Server_StartRecording();
		}
		else
		{
			ReplayManager.Server_StopRecording();
		}
		if (!isGameInProgress)
		{
			Vote[] array = VoteManager.Server_GetVotesByName("forfeit");
			foreach (Vote vote in array)
			{
				VoteManager.Server_RemoveVote(vote);
			}
		}
		PreparePlayersForGamePhase(gamePhase);
	}

	protected override void OnWarmupStarted()
	{
		base.OnWarmupStarted();
		PuckManager.Server_DespawnPucks();
		PuckManager.Server_SpawnPucksForPhase(GamePhase.Warmup);
	}

	protected override void OnPreGameStarted()
	{
		base.OnPreGameStarted();
		PuckManager.Server_DespawnPucks();
		tickRemainder = Config.phaseDurationMap[GamePhase.Play];
	}

	protected override void OnFaceOffStarted()
	{
		base.OnFaceOffStarted();
		PuckManager.Server_DespawnPucks();
	}

	protected override void OnPlayStarted()
	{
		base.OnPlayStarted();
		PuckManager.Server_SpawnPucksForPhase(GamePhase.Play);
	}

	protected override void OnBlueScoreStarted()
	{
		base.OnBlueScoreStarted();
		Level.Server_PlayerCheerSound(Config.phaseDurationMap[GamePhase.BlueScore] + Config.phaseDurationMap[GamePhase.Replay]);
		Level.Server_PlayRedGoalSound();
		Server_StartGoalSlowMotion();
	}

	protected override void OnRedScoreStarted()
	{
		base.OnRedScoreStarted();
		Level.Server_PlayerCheerSound(Config.phaseDurationMap[GamePhase.RedScore] + Config.phaseDurationMap[GamePhase.Replay]);
		Level.Server_PlayBlueGoalSound();
		Server_StartGoalSlowMotion();
	}

	protected override void OnIntermissionStarted()
	{
		base.OnIntermissionStarted();
		Level.Server_PlayHornSound();
	}

	protected virtual void Server_StartGoalSlowMotion()
	{
		if (Config.goalSlowMotion)
		{
			GameManager.Server_StartSlowMotion(0.35f, 0.08f, 0.28f, 0.42f);
		}
	}

	protected virtual void Server_ScheduleReplaySlowMotion(Player goalPlayer, Puck puck)
	{
		if (Config.goalSlowMotion && (bool)puck)
		{
			float? num = Server_GetReplaySlowMotionTouchTime(goalPlayer, puck);
			if (num.HasValue)
			{
				Server_ScheduleReplayScorerTouchSlowMotion(num.Value);
			}
			Server_ScheduleReplayGoalEntrySlowMotion(Time.time);
		}
	}

	protected virtual void Server_ScheduleReplayScorerTouchSlowMotion(float touchTime)
	{
		ReplayManager.Server_ScheduleSlowMotionAtWorldTime(touchTime - 0.6f, 0.3f, 0.5f, 0.45f, 0.5f, 0.95f, 12f);
	}

	protected virtual void Server_ScheduleReplayGoalEntrySlowMotion(float goalTime)
	{
		ReplayManager.Server_ScheduleSlowMotionAtWorldTime(goalTime - 0.4f, 0.3f, 0.4f, 0.4f, 0.5f, 0.95f, 12f);
	}

	protected virtual float? Server_GetReplaySlowMotionTouchTime(Player goalPlayer, Puck puck)
	{
		IReadOnlyDictionary<Player, float> lastContactTimeByPlayer = puck.ReplayTouchTracker.LastContactTimeByPlayer;
		if (lastContactTimeByPlayer.Count == 0)
		{
			return null;
		}
		if (puck.IsActivelyTouchedBy(goalPlayer))
		{
			return Time.time;
		}
		if (goalPlayer != null && lastContactTimeByPlayer.TryGetValue(goalPlayer, out var value) && Time.time - value <= 3f)
		{
			return value;
		}
		Player player = null;
		float num = float.MinValue;
		foreach (KeyValuePair<Player, float> item in lastContactTimeByPlayer)
		{
			if (item.Value > num)
			{
				num = item.Value;
				player = item.Key;
			}
		}
		if (puck.IsActivelyTouchedBy(player))
		{
			return Time.time;
		}
		return num;
	}

	protected override void OnReplayStarted()
	{
		base.OnReplayStarted();
		GameManager.Server_StopSlowMotion();
		PuckManager.Server_DespawnPucks();
		ReplayManager.Server_StartReplaying(Config.phaseDurationMap[GamePhase.Replay]);
	}

	protected override void OnGameOverStarted()
	{
		base.OnGameOverStarted();
		PlayerTeam winningTeam = gameResult.winningTeam;
		if (gameResult.forfeit)
		{
			string text = StringUtils.WrapInTeamColor(winningTeam.ToString(), winningTeam);
			ChatManager.Server_BroadcastChatMessage("Game Over! " + text + " team wins by forfeit!", "#ffe97f");
		}
		else
		{
			PlayerTeam team = ((winningTeam != PlayerTeam.Blue) ? PlayerTeam.Blue : PlayerTeam.Red);
			int num = Mathf.Max(gameResult.blueScore, gameResult.redScore);
			int num2 = Mathf.Min(gameResult.blueScore, gameResult.redScore);
			string content = "Game Over! " + StringUtils.WrapInTeamColor(winningTeam.ToString(), winningTeam) + " team wins with a score of " + StringUtils.WrapInTeamColor(num.ToString(), winningTeam) + " to " + StringUtils.WrapInTeamColor(num2.ToString(), team) + "!";
			ChatManager.Server_BroadcastChatMessage(content, "#ffe97f");
		}
		Level.Server_PlayBlueGoalSound();
		Level.Server_PlayRedGoalSound();
		Level.Server_PlayHornSound();
	}

	protected override void OnReplayEnded()
	{
		base.OnReplayEnded();
		ReplayManager.Server_StopReplaying();
	}

	protected override void OnPlayerJoined(Player player)
	{
		base.OnPlayerJoined(player);
		string text = player.SteamId.Value.ToString();
		UpdatePlayerResult(text);
		if (gameResult.playerResults.ContainsKey(text))
		{
			PlayerResult playerResult = gameResult.playerResults[text];
			player.Goals.Value = playerResult.goals;
			player.Assists.Value = playerResult.assists;
		}
	}

	protected override void OnPlayerLeft(Player player)
	{
		base.OnPlayerLeft(player);
		string text = player.SteamId.Value.ToString();
		UpdatePlayerResult(text);
		if (player.PlayerPosition != null)
		{
			player.PlayerPosition.Server_Unclaim();
		}
	}

	protected override void OnPlayerPhaseChanged(Player player, PlayerPhase oldPhase, PlayerPhase newPhase)
	{
		base.OnPlayerPhaseChanged(player, oldPhase, newPhase);
		switch (newPhase)
		{
		case PlayerPhase.PositionSelect:
			if (player.IsCharacterSpawned)
			{
				player.Server_DespawnCharacter();
			}
			return;
		case PlayerPhase.Play:
			player.Server_SpawnCharacter(player.PlayerPosition.transform.position, player.PlayerPosition.transform.rotation, player.Role);
			return;
		case PlayerPhase.Replay:
			if (player.IsCharacterSpawned)
			{
				player.Server_DespawnCharacter();
			}
			return;
		case PlayerPhase.Spectate:
			player.Server_SpawnSpectatorCamera(Vector3.zero, Quaternion.identity);
			return;
		}
		if (player.IsCharacterSpawned)
		{
			player.Server_DespawnCharacter();
		}
		if (player.PlayerPosition != null)
		{
			player.PlayerPosition.Server_Unclaim();
		}
	}

	protected override void OnPlayerPositionChanged(Player player, PlayerPosition oldPlayerPosition, PlayerPosition newPlayerPosition)
	{
		base.OnPlayerPositionChanged(player, oldPlayerPosition, newPlayerPosition);
		if (newPlayerPosition == null)
		{
			PlayerRole? role = PlayerRole.None;
			player.Server_SetGameState(null, null, role);
		}
		else
		{
			PlayerRole? role = newPlayerPosition.Role;
			player.Server_SetGameState(null, null, role);
			PreparePlayerForGamePhase(player, GameManager.Phase);
		}
	}

	protected override void OnGoalScored(PlayerTeam byTeam, Player goalPlayer, Player assistPlayer, Player secondAssistPlayer, Puck puck)
	{
		base.OnGoalScored(byTeam, goalPlayer, assistPlayer, secondAssistPlayer, puck);
		tickRemainder = GameManager.Tick;
		Server_ScheduleReplaySlowMotion(goalPlayer, puck);
		if ((bool)goalPlayer)
		{
			ReplayManager.Server_ScheduleCellyCam(goalPlayer.OwnerClientId, 0.75f);
		}
		if ((bool)goalPlayer)
		{
			goalPlayer.Server_GoalScored();
		}
		if ((bool)assistPlayer)
		{
			assistPlayer.Server_AssistScored();
		}
		if ((bool)secondAssistPlayer)
		{
			secondAssistPlayer.Server_AssistScored();
		}
		switch (byTeam)
		{
		case PlayerTeam.Blue:
		{
			GameManager gameManager2 = GameManager;
			GamePhase? phase2 = GamePhase.BlueScore;
			int? tick2 = Config.phaseDurationMap[GamePhase.BlueScore];
			int? redScore = GameManager.GameState.Value.BlueScore + 1;
			gameManager2.Server_SetGameState(phase2, tick2, null, redScore);
			break;
		}
		case PlayerTeam.Red:
		{
			GameManager gameManager = GameManager;
			GamePhase? phase = GamePhase.RedScore;
			int? tick = Config.phaseDurationMap[GamePhase.RedScore];
			int? redScore = GameManager.GameState.Value.RedScore + 1;
			gameManager.Server_SetGameState(phase, tick, null, null, redScore);
			break;
		}
		}
		if ((bool)puck)
		{
			string text = puck.Speed.ToString(CultureInfo.InvariantCulture);
			string text2 = puck.ShotSpeed.ToString(CultureInfo.InvariantCulture);
			ChatManager.Server_BroadcastChatMessage("Goal scored! <united>" + text + "</united> &units across the line, <united>" + text2 + "</united> &units from the stick.", "#ffe97f");
		}
		else
		{
			ChatManager.Server_BroadcastChatMessage("Goal scored!", "#ffe97f");
		}
		UpdateGameResult();
	}

	protected override void OnPuckEnterGoal(PlayerTeam team, Puck puck)
	{
		base.OnPuckEnterGoal(team, puck);
		if (GameManager.Phase != GamePhase.Play)
		{
			return;
		}
		PlayerTeam? opposingTeam = Utils.GetOpposingTeam(team);
		if (!opposingTeam.HasValue)
		{
			return;
		}
		List<KeyValuePair<Player, float>> playerCollisionsByTeam = puck.GetPlayerCollisionsByTeam(opposingTeam.Value);
		Player goalPlayer = null;
		Player assistPlayer = null;
		Player secondAssistPlayer = null;
		if (playerCollisionsByTeam.Count >= 1)
		{
			goalPlayer = playerCollisionsByTeam[playerCollisionsByTeam.Count - 1].Key;
			if (playerCollisionsByTeam.Count >= 2)
			{
				assistPlayer = playerCollisionsByTeam[playerCollisionsByTeam.Count - 2].Key;
			}
			if (playerCollisionsByTeam.Count >= 3)
			{
				secondAssistPlayer = playerCollisionsByTeam[playerCollisionsByTeam.Count - 3].Key;
			}
		}
		ScoreGoal(opposingTeam.Value, goalPlayer, assistPlayer, secondAssistPlayer, puck);
	}

	protected override void OnPlayerRequestPositionSelect(Player player)
	{
		base.OnPlayerRequestPositionSelect(player);
		if (CanPlayerEnterPhase(player, PlayerPhase.PositionSelect))
		{
			player.Server_SetGameState(PlayerPhase.PositionSelect);
			if (player.PlayerPosition != null)
			{
				player.PlayerPosition.Server_Unclaim();
			}
		}
	}

	protected override void OnPlayerRequestPosition(Player player, PlayerPosition position)
	{
		base.OnPlayerRequestPosition(player, position);
		if (position.IsClaimed)
		{
			if (player == position.ClaimedByPlayer)
			{
				position.Server_Unclaim();
				if (CanPlayerEnterPhase(player, PlayerPhase.PositionSelect))
				{
					player.Server_SetGameState(PlayerPhase.PositionSelect);
				}
			}
		}
		else if (position.Team == player.Team)
		{
			PlayerPosition playerPosition = player.PlayerPosition;
			position.Server_Claim(player);
			if (playerPosition != null)
			{
				playerPosition.Server_Unclaim();
			}
		}
	}

	protected override void OnPlayerRequestForfeit(Player player)
	{
		base.OnPlayerRequestForfeit(player);
		StartForfeitVote(player);
	}

	protected virtual void StartForfeitVote(Player player)
	{
		if (player.Team != PlayerTeam.Blue && player.Team != PlayerTeam.Red)
		{
			ChatManager.Server_SendChatMessage("You must be on a team to start this vote", "#e74c3c", player.OwnerClientId);
			return;
		}
		if (!isGameInProgress)
		{
			ChatManager.Server_SendChatMessage("You can not forfeit right now", "#e74c3c", player.OwnerClientId);
			return;
		}
		Vote vote = VoteManager.Server_GetTeamVoteByName("forfeit", player.Team);
		if (vote != null)
		{
			vote.CastVote(player.SteamId.Value.ToString(), inFavour: true);
			return;
		}
		VoteManager.Server_AddVote("forfeit", "Forfeit", "use /ff or /forfeit", new PlayerTeam[1] { player.Team }, 30f, player.SteamId.Value.ToString(), Utils.GetVoteMajority(PlayerManager.GetPlayersByTeam(player.Team).Count), player.Team);
	}

	protected override void OnVoteRemoved(Vote vote)
	{
		base.OnVoteRemoved(vote);
		if (!vote.Passed)
		{
			return;
		}
		string name = vote.Name;
		if (!(name == "kick"))
		{
			if (name == "forfeit")
			{
				PlayerTeam forfeitingTeam = (PlayerTeam)vote.Data;
				ForfeitGame(forfeitingTeam);
			}
			return;
		}
		string text = (string)vote.Data;
		Player playerBySteamId = PlayerManager.GetPlayerBySteamId(text);
		if ((bool)playerBySteamId)
		{
			ServerManager.Server_KickPlayer(playerBySteamId);
		}
		else
		{
			ServerManager.TimeoutManager.AddSteamIdTimeout(text, 60f);
		}
	}

	protected override List<ChatCommandInfo> GetChatCommands()
	{
		List<ChatCommandInfo> chatCommands = base.GetChatCommands();
		if (AllowVotekick)
		{
			chatCommands.Add(new ChatCommandInfo("/vk, /votekick <player>", "Start a vote to kick a player"));
		}
		chatCommands.Add(new ChatCommandInfo("/ff, /forfeit", "Start a vote to forfeit the game"));
		chatCommands.Add(new ChatCommandInfo("/kick <player>", "Kick a player", 1));
		chatCommands.Add(new ChatCommandInfo("/ban <player|steamId>", "Ban a player", 1));
		chatCommands.Add(new ChatCommandInfo("/unban <steamId>", "Unban a Steam ID", 1));
		chatCommands.Add(new ChatCommandInfo("/start", "Start the game", 1));
		chatCommands.Add(new ChatCommandInfo("/warmup", "Return to warmup", 1));
		chatCommands.Add(new ChatCommandInfo("/pause", "Pause the game", 1));
		chatCommands.Add(new ChatCommandInfo("/resume, /unpause", "Resume the game", 1));
		chatCommands.Add(new ChatCommandInfo("/skip", "Skip to the next phase", 1));
		chatCommands.Add(new ChatCommandInfo("/bots", "Spawn and manage bots", 3));
		return chatCommands;
	}

	protected override void OnChatCommand(Player player, string command, string[] args)
	{
		bool flag = player.AdminLevel.Value > 0 || ServerManager.AdminManager.IsSteamIdAdmin(player.SteamId.Value.ToString());
		switch (command)
		{
		case "/vk":
		case "/votekick":
		{
			if (!AllowVotekick)
			{
				ChatManager.Server_SendChatMessage("Votekick is disabled in this game mode", "#e74c3c", player.OwnerClientId);
				break;
			}
			Vote vote = VoteManager.Server_GetTeamVoteByName("kick", player.Team);
			if (vote != null)
			{
				vote.CastVote(player.SteamId.Value.ToString(), inFavour: true);
				break;
			}
			string needle = ((args.Length != 0) ? args[0] : string.Empty);
			Player playerByNeedle = PlayerManager.GetPlayerByNeedle(needle);
			if (!playerByNeedle)
			{
				ChatManager.Server_SendChatMessage("Player not found", "#e74c3c", player.OwnerClientId);
				break;
			}
			if (playerByNeedle.AdminLevel.Value > 0 || ServerManager.AdminManager.IsSteamIdAdmin(playerByNeedle.SteamId.Value.ToString()))
			{
				ChatManager.Server_SendChatMessage("You can't votekick an admin", "#e74c3c", player.OwnerClientId);
				break;
			}
			string text = StringUtils.WrapInTeamColor(playerByNeedle.Username.Value.ToString(), playerByNeedle.Team);
			PlayerTeam[] array = new PlayerTeam[4]
			{
				PlayerTeam.None,
				PlayerTeam.Blue,
				PlayerTeam.Red,
				PlayerTeam.Spectator
			};
			VoteManager.Server_AddVote("kick", "Kick " + text, "use /vk or /votekick", array, 30f, player.SteamId.Value.ToString(), Utils.GetVoteMajority(PlayerManager.GetPlayersByTeams(array).Count), playerByNeedle.SteamId.Value.ToString());
			break;
		}
		case "/ff":
		case "/forfeit":
			StartForfeitVote(player);
			break;
		case "/kick":
		{
			if (!flag)
			{
				ChatManager.Server_SendChatMessage("You do not have permissions to use this command", "#e74c3c", player.OwnerClientId);
				break;
			}
			string needle2 = ((args.Length != 0) ? args[0] : string.Empty);
			Player playerByNeedle2 = PlayerManager.GetPlayerByNeedle(needle2);
			if (!playerByNeedle2)
			{
				ChatManager.Server_SendChatMessage("Player not found", "#e74c3c", player.OwnerClientId);
			}
			else
			{
				ServerManager.Server_KickPlayer(playerByNeedle2);
			}
			break;
		}
		case "/ban":
		{
			if (!flag)
			{
				ChatManager.Server_SendChatMessage("You do not have permissions to use this command", "#e74c3c", player.OwnerClientId);
				break;
			}
			string text3 = ((args.Length != 0) ? args[0] : string.Empty);
			Player playerByNeedle3 = PlayerManager.GetPlayerByNeedle(text3);
			if ((bool)playerByNeedle3)
			{
				ServerManager.Server_BanPlayer(playerByNeedle3);
				ChatManager.Server_SendChatMessage($"Banned {playerByNeedle3.Username.Value}", "#b8b8b8", player.OwnerClientId);
			}
			else if (Utils.IsValidSteamId64(text3))
			{
				ServerManager.Server_BanSteamId(text3);
				ChatManager.Server_SendChatMessage("Banned Steam ID " + text3, "#b8b8b8", player.OwnerClientId);
			}
			else
			{
				ChatManager.Server_SendChatMessage("Player not found", "#e74c3c", player.OwnerClientId);
			}
			break;
		}
		case "/unban":
		{
			if (!flag)
			{
				ChatManager.Server_SendChatMessage("You do not have permissions to use this command", "#e74c3c", player.OwnerClientId);
				break;
			}
			string text2 = ((args.Length != 0) ? args[0] : string.Empty);
			if (!Utils.IsValidSteamId64(text2))
			{
				ChatManager.Server_SendChatMessage("Provide a SteamID64 to unban", "#e74c3c", player.OwnerClientId);
			}
			else if (!ServerManager.Server_IsSteamIdBanned(text2))
			{
				ChatManager.Server_SendChatMessage("Steam ID " + text2 + " is not banned", "#e74c3c", player.OwnerClientId);
			}
			else
			{
				ServerManager.Server_UnbanSteamId(text2);
				ChatManager.Server_SendChatMessage("Unbanned Steam ID " + text2, "#b8b8b8", player.OwnerClientId);
			}
			break;
		}
		case "/bots":
			if (player.AdminLevel.Value < 3)
			{
				ChatManager.Server_SendChatMessage("You do not have permissions to use this command", "#e74c3c", player.OwnerClientId);
			}
			else if (args.Length != 0 && args[0].ToLower() == "mode")
			{
				if (args.Length > 1)
				{
					switch (args[1].ToLower())
					{
					case "chase":
						BotManager.Server_SetBotBehavior(BotManager.BotBehavior.Chase);
						break;
					case "rookie":
						BotManager.Server_SetBotBehavior(BotManager.BotBehavior.Rookie);
						break;
					case "destroy":
						BotManager.Server_SetBotBehavior(BotManager.BotBehavior.Destroy);
						break;
					default:
						ChatManager.Server_SendChatMessage("Usage: /bots mode <chase|rookie|destroy>", "#e74c3c", player.OwnerClientId);
						return;
					}
				}
				ChatManager.Server_SendChatMessage("Bot mode: " + BotManager.DefaultBehavior.ToString().ToLower(), "#ffe97f", player.OwnerClientId);
			}
			else if (args.Length != 0 && args[0].ToLower() == "clear")
			{
				int num = BotManager.Server_DespawnBots();
				ChatManager.Server_SendChatMessage($"Despawned {num} bots", "#ffe97f", player.OwnerClientId);
			}
			else
			{
				PlayerTeam playerTeam = PlayerTeam.None;
				int num2 = 0;
				if (args.Length != 0 && args[0].ToLower() == "red")
				{
					playerTeam = PlayerTeam.Red;
					num2 = 1;
				}
				else if (args.Length != 0 && args[0].ToLower() == "blue")
				{
					playerTeam = PlayerTeam.Blue;
					num2 = 1;
				}
				int result = 1;
				if (args.Length > num2 && !int.TryParse(args[num2], out result))
				{
					ChatManager.Server_SendChatMessage("Usage: /bots [red|blue] <count> | clear | mode <chase|rookie|destroy>", "#e74c3c", player.OwnerClientId);
					break;
				}
				int num3 = BotManager.Server_SpawnBots(result, playerTeam);
				string arg = ((playerTeam == PlayerTeam.None) ? "" : (" on " + playerTeam.ToString().ToLower()));
				ChatManager.Server_SendChatMessage($"Spawned {num3} bots{arg} (use /bots clear to remove)", "#ffe97f", player.OwnerClientId);
			}
			break;
		case "/start":
			if (!flag)
			{
				ChatManager.Server_SendChatMessage("You do not have permissions to use this command", "#e74c3c", player.OwnerClientId);
			}
			else
			{
				StartGame(GamePhase.PreGame);
				ChatManager.Server_BroadcastChatMessage(StringUtils.WrapInTeamColor(player.Username.Value.ToString(), player.Team) + " started the game", "#b8b8b8");
			}
			break;
		case "/warmup":
			if (!flag)
			{
				ChatManager.Server_SendChatMessage("You do not have permissions to use this command", "#e74c3c", player.OwnerClientId);
			}
			else
			{
				StartGame();
				ChatManager.Server_BroadcastChatMessage(StringUtils.WrapInTeamColor(player.Username.Value.ToString(), player.Team) + " returned to warmup", "#b8b8b8");
			}
			break;
		case "/pause":
			if (!flag)
			{
				ChatManager.Server_SendChatMessage("You do not have permissions to use this command", "#e74c3c", player.OwnerClientId);
			}
			else if (!GameManager.IsTicking)
			{
				ChatManager.Server_SendChatMessage("The game is already paused", "#e74c3c", player.OwnerClientId);
			}
			else
			{
				GameManager.Server_StopTicking();
				ChatManager.Server_BroadcastChatMessage(StringUtils.WrapInTeamColor(player.Username.Value.ToString(), player.Team) + " paused the game", "#b8b8b8");
			}
			break;
		case "/resume":
		case "/unpause":
			if (!flag)
			{
				ChatManager.Server_SendChatMessage("You do not have permissions to use this command", "#e74c3c", player.OwnerClientId);
			}
			else if (GameManager.IsTicking)
			{
				ChatManager.Server_SendChatMessage("The game is not paused", "#e74c3c", player.OwnerClientId);
			}
			else
			{
				GameManager.Server_StartTicking();
				ChatManager.Server_BroadcastChatMessage(StringUtils.WrapInTeamColor(player.Username.Value.ToString(), player.Team) + " resumed the game", "#b8b8b8");
			}
			break;
		case "/skip":
			if (!flag)
			{
				ChatManager.Server_SendChatMessage("You do not have permissions to use this command", "#e74c3c", player.OwnerClientId);
			}
			else
			{
				GameManager.Server_ExpirePhase();
				ChatManager.Server_BroadcastChatMessage(StringUtils.WrapInTeamColor(player.Username.Value.ToString(), player.Team) + " skipped to the next phase", "#b8b8b8");
			}
			break;
		default:
			base.OnChatCommand(player, command, args);
			break;
		}
	}
}
