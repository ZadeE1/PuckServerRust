public class PlayerData
{
	public string steamId { get; set; }

	public string username { get; set; }

	public int number { get; set; }

	public double? usernameChangedAt { get; set; }

	public int patreonLevel { get; set; }

	public int mmr { get; set; }

	public int adminLevel { get; set; }

	public PlayerItem[] items { get; set; }

	public PlayerMute[] mutes { get; set; }

	public PlayerBan[] bans { get; set; }

	public PlayerCooldown[] cooldowns { get; set; }
}
