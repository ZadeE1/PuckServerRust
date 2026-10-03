using System.Collections.Generic;

public class StandardGameModeConfig : BaseGameModeConfig
{
	public Dictionary<GamePhase, int> phaseDurationMap { get; set; } = new Dictionary<GamePhase, int>
	{
		{
			GamePhase.None,
			0
		},
		{
			GamePhase.Warmup,
			60
		},
		{
			GamePhase.PreGame,
			10
		},
		{
			GamePhase.FaceOff,
			4
		},
		{
			GamePhase.Play,
			300
		},
		{
			GamePhase.BlueScore,
			4
		},
		{
			GamePhase.RedScore,
			4
		},
		{
			GamePhase.Replay,
			9
		},
		{
			GamePhase.Intermission,
			10
		},
		{
			GamePhase.GameOver,
			30
		},
		{
			GamePhase.PostGame,
			10
		}
	};

	public float spawnDelay { get; set; } = 10f;

	public int maxPeriods { get; set; } = 3;

	public bool goalSlowMotion { get; set; } = true;

	public bool goalieCreaseProtection { get; set; } = true;
}
