public struct SynchronizedObjectPlayerDiagnosticsData
{
	public const int PLAYER_COLUMN_WIDTH = 24;

	public const int COLUMN_WIDTH = 10;

	public ulong ClientId;

	public string Username;

	public int DefaultCount;

	public int CulledCount;

	public int NoOriginCount;

	public int[] BandCounts;

	public string Describe()
	{
		string text = DescribePlayer() + Column(DefaultCount);
		for (int i = 0; i < BandCounts.Length; i++)
		{
			text += Column(BandCounts[i]);
		}
		return text + Column(CulledCount) + Column(NoOriginCount);
	}

	public static string Column(string text)
	{
		return text.PadLeft(10);
	}

	private static string Column(int value)
	{
		return Column(value.ToString());
	}

	private string DescribePlayer()
	{
		string text = $"{Username} ({ClientId})";
		if (text.Length >= 24)
		{
			text = text.Substring(0, 23);
		}
		return text.PadRight(24);
	}
}
