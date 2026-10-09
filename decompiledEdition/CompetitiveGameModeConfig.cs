using System.Collections.Generic;

public class CompetitiveGameModeConfig : StandardGameModeConfig
{
	public Dictionary<PlayerTeam, string[]> teamAssignments { get; set; } = new Dictionary<PlayerTeam, string[]>();
}
