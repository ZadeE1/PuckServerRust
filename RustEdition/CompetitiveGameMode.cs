using System.Collections.Generic;
using System.Linq;

public class CompetitiveGameMode<TConfig> : MatchableGameMode<TConfig> where TConfig : CompetitiveGameModeConfig, new()
{
	private Dictionary<PlayerTeam, string[]> teamAssignments = new Dictionary<PlayerTeam, string[]>();

	protected override bool AllowVotekick => false;

	public CompetitiveGameMode(string defaultConfigFilePath, string configFilePathCliArgument = null, string configCliArgument = null, string configEnvVariable = null)
		: base(defaultConfigFilePath, configFilePathCliArgument, configCliArgument, configEnvVariable)
	{
	}

	private PlayerTeam GetAssignedPlayerTeam(Player player)
	{
		foreach (PlayerTeam key in teamAssignments.Keys)
		{
			if (teamAssignments[key].Contains(player.SteamId.Value.ToString()))
			{
				return key;
			}
		}
		return PlayerTeam.None;
	}

	protected override void OnConfigLoaded()
	{
		base.OnConfigLoaded();
		teamAssignments = Config.teamAssignments;
	}

	protected override void OnPlayerJoined(Player player)
	{
		base.OnPlayerJoined(player);
		PlayerTeam assignedPlayerTeam = GetAssignedPlayerTeam(player);
		switch (assignedPlayerTeam)
		{
		case PlayerTeam.Blue:
		case PlayerTeam.Red:
		{
			PlayerTeam? team = assignedPlayerTeam;
			player.Server_SetGameState(null, team);
			if (CanPlayerEnterPhase(player, PlayerPhase.PositionSelect))
			{
				player.Server_SetGameState(PlayerPhase.PositionSelect);
			}
			break;
		}
		case PlayerTeam.None:
		case PlayerTeam.Spectator:
		{
			PlayerTeam? team = PlayerTeam.Spectator;
			player.Server_SetGameState(null, team);
			if (CanPlayerEnterPhase(player, PlayerPhase.Spectate))
			{
				player.Server_SetGameState(PlayerPhase.Spectate);
			}
			break;
		}
		}
	}

	protected override void OnMatchStarted()
	{
		base.OnMatchStarted();
		teamAssignments = new Dictionary<PlayerTeam, string[]>
		{
			{
				PlayerTeam.Blue,
				matchData.HomeSteamIds
			},
			{
				PlayerTeam.Red,
				matchData.AwaySteamIds
			}
		};
	}
}
