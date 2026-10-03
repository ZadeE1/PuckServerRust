using System;

public struct PlayerState
{
	public AuthenticationPhase AuthenticationPhase;

	public PlayerData PlayerData;

	public PlayerPartyData PartyData;

	public PlayerGroupData GroupData;

	public PlayerMatchData MatchData;

	public PlayerStatistics PlayerStatistics;

	public string Key;

	public bool Equals(PlayerState other)
	{
		if (AuthenticationPhase == other.AuthenticationPhase && PlayerData == other.PlayerData && PartyData == other.PartyData && GroupData == other.GroupData && MatchData == other.MatchData && PlayerStatistics == other.PlayerStatistics)
		{
			return Key == other.Key;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is PlayerState other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(AuthenticationPhase, PlayerData, PartyData, GroupData, MatchData, PlayerStatistics, Key);
	}

	public override string ToString()
	{
		return $"AuthenticationPhase: {AuthenticationPhase}, PlayerData: {PlayerData}, PartyData: {PartyData}, GroupData: {GroupData}, MatchData: {MatchData}, PlayerStatistics: {PlayerStatistics}, Key: {Key}";
	}
}
