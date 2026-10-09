using System.Linq;

public static class BackendUtils
{
	public static PlayerBan GetActivePlayerDataBan(PlayerData playerData)
	{
		return playerData?.bans.FirstOrDefault((PlayerBan ban) => Utils.GetTimestamp() <= ban.expiresAt);
	}

	public static PlayerMute GetActivePlayerDataMute(PlayerData playerData)
	{
		return playerData?.mutes.FirstOrDefault((PlayerMute mute) => Utils.GetTimestamp() <= mute.expiresAt);
	}

	public static PlayerCooldown GetActivePlayerDataCooldown(PlayerData playerData)
	{
		return playerData?.cooldowns.FirstOrDefault((PlayerCooldown cooldown) => Utils.GetTimestamp() <= cooldown.expiresAt);
	}

	public static bool IsConnectedToMatchEndPoint()
	{
		EndPoint endPoint = GlobalStateManager.ConnectionState.Connection?.EndPoint;
		EndPoint endPoint2 = BackendManager.PlayerState.MatchData?.endPoint;
		if (endPoint != null)
		{
			return endPoint == endPoint2;
		}
		return false;
	}
}
