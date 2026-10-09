using System.Collections.Generic;

public class GameResult
{
	public PlayerTeam winningTeam { get; set; }

	public int blueScore { get; set; }

	public int redScore { get; set; }

	public bool forfeit { get; set; }

	public Dictionary<string, PlayerResult> playerResults { get; set; } = new Dictionary<string, PlayerResult>();
}
