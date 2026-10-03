using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

public class MatchData
{
	public MatchPlayer[] homePlayers { get; set; }

	public MatchPlayer[] awayPlayers { get; set; }

	public double? startedAt { get; set; }

	public EndPoint endPoint { get; set; }

	[JsonIgnore]
	public Dictionary<PlayerTeam, string[]> TeamAssignments => new Dictionary<PlayerTeam, string[]>
	{
		{
			PlayerTeam.Blue,
			HomeSteamIds
		},
		{
			PlayerTeam.Red,
			AwaySteamIds
		}
	};

	[JsonIgnore]
	public MatchPlayer[] Players => homePlayers.Concat(awayPlayers).ToArray();

	[JsonIgnore]
	public string[] SteamIds => Players.Select((MatchPlayer p) => p.steamId).ToArray();

	[JsonIgnore]
	public string[] HomeSteamIds => homePlayers.Select((MatchPlayer p) => p.steamId).ToArray();

	[JsonIgnore]
	public string[] AwaySteamIds => awayPlayers.Select((MatchPlayer p) => p.steamId).ToArray();

	public MatchPlayer GetMatchPlayerBySteamId(string steamId)
	{
		return Players.FirstOrDefault((MatchPlayer p) => p.steamId == steamId);
	}

	public PlayerTeam? GetTeamBySteamId(string steamId)
	{
		if (HomeSteamIds.Contains(steamId))
		{
			return PlayerTeam.Blue;
		}
		if (AwaySteamIds.Contains(steamId))
		{
			return PlayerTeam.Red;
		}
		return null;
	}

	public string[] GetTeamSteamIds(PlayerTeam team)
	{
		if (!TeamAssignments.ContainsKey(team))
		{
			return new string[0];
		}
		return TeamAssignments[team];
	}
}
