public struct SynchronizedObjectServerDiagnosticsData
{
	public float[] BandMinDistances;

	public SynchronizedObjectPlayerDiagnosticsData[] Players;

	public string DescribeHeader()
	{
		string text = "player".PadRight(24) + SynchronizedObjectPlayerDiagnosticsData.Column("default");
		for (int i = 0; i < BandMinDistances.Length; i++)
		{
			text += SynchronizedObjectPlayerDiagnosticsData.Column($"{BandMinDistances[i]}m");
		}
		return text + SynchronizedObjectPlayerDiagnosticsData.Column("culled") + SynchronizedObjectPlayerDiagnosticsData.Column("noOrigin");
	}
}
