public struct NetworkDiagnostics
{
	private const int COLUMN_WIDTH = 20;

	public float WindowTime;

	public double MaxStallTime;

	public ulong SentWireByteCount;

	public ulong ReceivedWireByteCount;

	public ulong SentPacketCount;

	public ulong ReceivedPacketCount;

	public float SentKilobytesPerSecond => GetKilobytesPerSecond(SentWireByteCount);

	public float ReceivedKilobytesPerSecond => GetKilobytesPerSecond(ReceivedWireByteCount);

	public float SentPacketsPerSecond => GetPerSecond(SentPacketCount);

	public float ReceivedPacketsPerSecond => GetPerSecond(ReceivedPacketCount);

	public static string DescribeHeader()
	{
		return Column("sent KB/s") + Column("sent packets/s") + Column("received KB/s") + Column("received packets/s") + Column("max stall ms");
	}

	public string Describe()
	{
		return Column(SentKilobytesPerSecond) + Column(SentPacketsPerSecond) + Column(ReceivedKilobytesPerSecond) + Column(ReceivedPacketsPerSecond) + Column((float)(MaxStallTime * 1000.0));
	}

	private static string Column(string text)
	{
		return text.PadLeft(20);
	}

	private static string Column(float value)
	{
		return Column(value.ToString("F1"));
	}

	private float GetKilobytesPerSecond(ulong byteCount)
	{
		return GetPerSecond(byteCount) / 1024f;
	}

	private float GetPerSecond(ulong count)
	{
		if (!(WindowTime > 0f))
		{
			return 0f;
		}
		return (float)count / WindowTime;
	}
}
