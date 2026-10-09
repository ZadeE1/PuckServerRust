using System.Collections.Generic;

public class SynchronizedObjectServerDiagnostics
{
	private SynchronizedObjectLodCounts lodCounts = new SynchronizedObjectLodCounts();

	private List<SynchronizedObjectPlayerDiagnosticsData> playersData = new List<SynchronizedObjectPlayerDiagnosticsData>();

	private SynchronizedObjectBandSettings[] lodBands;

	private float reportInterval;

	private float reportTimer;

	private bool isReportDue;

	private bool IsEnabled => reportInterval > 0f;

	public void Configure(float reportInterval, SynchronizedObjectBandSettings[] lodBands)
	{
		this.reportInterval = reportInterval;
		this.lodBands = lodBands;
		lodCounts.Configure(lodBands?.Length ?? 0);
	}

	public void Step(float deltaTime)
	{
		isReportDue = false;
		if (IsEnabled)
		{
			reportTimer += deltaTime;
			if (!(reportTimer < reportInterval))
			{
				reportTimer = 0f;
				isReportDue = true;
				playersData.Clear();
			}
		}
	}

	public SynchronizedObjectLodCounts BeginPlayer()
	{
		if (!isReportDue)
		{
			return null;
		}
		lodCounts.Clear();
		return lodCounts;
	}

	public void EndPlayer(SynchronizedPlayerState playerState)
	{
		if (isReportDue)
		{
			playersData.Add(BuildPlayerData(playerState.Player));
		}
	}

	public bool TryGetData(out SynchronizedObjectServerDiagnosticsData data)
	{
		data = default;
		if (!isReportDue || playersData.Count == 0)
		{
			return false;
		}
		data = new SynchronizedObjectServerDiagnosticsData
		{
			BandMinDistances = TakeBandMinDistances(),
			Players = playersData.ToArray()
		};
		return true;
	}

	public void Reset()
	{
		reportTimer = 0f;
		isReportDue = false;
		playersData.Clear();
	}

	private float[] TakeBandMinDistances()
	{
		float[] array = new float[(lodBands != null) ? lodBands.Length : 0];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = lodBands[i].MinDistance;
		}
		return array;
	}

	private SynchronizedObjectPlayerDiagnosticsData BuildPlayerData(Player player)
	{
		return new SynchronizedObjectPlayerDiagnosticsData
		{
			ClientId = (player ? player.OwnerClientId : 0),
			Username = (player ? player.Username.Value.ToString() : "unknown"),
			DefaultCount = lodCounts.DefaultCount,
			CulledCount = lodCounts.CulledCount,
			NoOriginCount = lodCounts.NoOriginCount,
			BandCounts = lodCounts.TakeBandCounts()
		};
	}
}
