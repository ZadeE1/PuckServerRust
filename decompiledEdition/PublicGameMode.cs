using System.Collections.Generic;

public class PublicGameMode<TConfig> : StandardGameMode<TConfig> where TConfig : PublicGameModeConfig, new()
{
	public PublicGameMode(string defaultConfigFilePath, string configFilePathCliArgument = null, string configCliArgument = null, string configEnvVariable = null)
		: base(defaultConfigFilePath, configFilePathCliArgument, configCliArgument, configEnvVariable)
	{
	}

	protected override void OnWarmupTimedOut()
	{
		if (PlayerManager.GetPlayersByTeam(PlayerTeam.Blue).Count == 0 || PlayerManager.GetPlayersByTeam(PlayerTeam.Red).Count == 0)
		{
			GameManager.Server_SetGameState(GamePhase.Warmup, Config.phaseDurationMap[GamePhase.Warmup]);
			ChatManager.Server_BroadcastChatMessage("Not enough players to start the game. Extending warmup...", "#ffe97f");
		}
		else
		{
			base.OnWarmupTimedOut();
		}
	}

	protected override void OnPlayerJoined(Player player)
	{
		base.OnPlayerJoined(player);
		if (CanPlayerEnterPhase(player, PlayerPhase.TeamSelect))
		{
			player.Server_SetGameState(PlayerPhase.TeamSelect);
		}
	}

	protected override void OnPlayerRequestTeamSelect(Player player)
	{
		base.OnPlayerRequestTeamSelect(player);
		if (CanPlayerEnterPhase(player, PlayerPhase.TeamSelect))
		{
			player.Server_SetGameState(PlayerPhase.TeamSelect, PlayerTeam.None);
		}
	}

	protected override void OnPlayerRequestTeam(Player player, PlayerTeam team)
	{
		base.OnPlayerRequestTeam(player, team);
		switch (team)
		{
		case PlayerTeam.Blue:
		case PlayerTeam.Red:
		{
			PlayerTeam? team2 = team;
			player.Server_SetGameState(null, team2);
			if (CanPlayerEnterPhase(player, PlayerPhase.PositionSelect))
			{
				player.Server_SetGameState(PlayerPhase.PositionSelect);
			}
			break;
		}
		case PlayerTeam.Spectator:
		{
			PlayerTeam? team2 = team;
			player.Server_SetGameState(null, team2);
			if (CanPlayerEnterPhase(player, PlayerPhase.Spectate))
			{
				player.Server_SetGameState(PlayerPhase.Spectate);
			}
			break;
		}
		}
	}

	protected override void OnVoteRemoved(Vote vote)
	{
		base.OnVoteRemoved(vote);
		if (!vote.Passed)
		{
			return;
		}
		string name = vote.Name;
		if (!(name == "start"))
		{
			if (name == "warmup")
			{
				StartGame();
			}
		}
		else
		{
			StartGame(GamePhase.PreGame);
		}
	}

	protected override List<ChatCommandInfo> GetChatCommands()
	{
		List<ChatCommandInfo> chatCommands = base.GetChatCommands();
		chatCommands.Add(new ChatCommandInfo("/vs, /votestart", "Start a vote to start the game"));
		chatCommands.Add(new ChatCommandInfo("/vw, /votewarmup", "Start a vote to return to warmup"));
		return chatCommands;
	}

	protected override void OnChatCommand(Player player, string command, string[] args)
	{
		switch (command)
		{
		case "/vs":
		case "/votestart":
		{
			if (player.Team != PlayerTeam.Blue && player.Team != PlayerTeam.Red)
			{
				ChatManager.Server_SendChatMessage("You must be on a team to start this vote", "#e74c3c", player.OwnerClientId);
				break;
			}
			Vote vote2 = VoteManager.Server_GetTeamVoteByName("start", player.Team);
			if (vote2 != null)
			{
				vote2.CastVote(player.SteamId.Value.ToString(), inFavour: true);
				break;
			}
			VoteManager.Server_AddVote("start", "Start", "use /vs or /votestart", new PlayerTeam[2]
			{
				PlayerTeam.Blue,
				PlayerTeam.Red
			}, 30f, player.SteamId.Value.ToString(), Utils.GetVoteMajority(PlayerManager.GetPlayersByTeams(new PlayerTeam[2]
			{
				PlayerTeam.Blue,
				PlayerTeam.Red
			}).Count));
			break;
		}
		case "/vw":
		case "/votewarmup":
		{
			if (player.Team != PlayerTeam.Blue && player.Team != PlayerTeam.Red)
			{
				ChatManager.Server_SendChatMessage("You must be on a team to start this vote", "#e74c3c", player.OwnerClientId);
				break;
			}
			Vote vote = VoteManager.Server_GetTeamVoteByName("warmup", player.Team);
			if (vote != null)
			{
				vote.CastVote(player.SteamId.Value.ToString(), inFavour: true);
				break;
			}
			VoteManager.Server_AddVote("warmup", "Warmup", "use /vw or /votewarmup", new PlayerTeam[2]
			{
				PlayerTeam.Blue,
				PlayerTeam.Red
			}, 30f, player.SteamId.Value.ToString(), Utils.GetVoteMajority(PlayerManager.GetPlayersByTeams(new PlayerTeam[2]
			{
				PlayerTeam.Blue,
				PlayerTeam.Red
			}).Count));
			break;
		}
		default:
			base.OnChatCommand(player, command, args);
			break;
		}
	}
}
