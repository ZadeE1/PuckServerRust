using System;

public struct ServerState
{
	public AuthenticationPhase AuthenticationPhase;

	public ServerData ServerData;

	public ServerMatchData MatchData;

	public bool Equals(ServerState other)
	{
		if (AuthenticationPhase == other.AuthenticationPhase && ServerData == other.ServerData)
		{
			return MatchData == other.MatchData;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is ServerState other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(AuthenticationPhase, ServerData, MatchData);
	}

	public override string ToString()
	{
		return $"AuthenticationPhase: {AuthenticationPhase}, ServerData: {ServerData}, MatchData: {MatchData}";
	}
}
