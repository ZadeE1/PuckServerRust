public struct ApplicationDiagnostics
{
	private const int COLUMN_WIDTH = 20;

	public float WindowTime;

	public int FrameCount;

	public double AverageFrameTime;

	public double P95FrameTime;

	public double MaxFrameTime;

	public float FramesPerSecond
	{
		get
		{
			if (!(WindowTime > 0f))
			{
				return 0f;
			}
			return (float)FrameCount / WindowTime;
		}
	}

	public static string DescribeHeader()
	{
		return Column("frames/s") + Column("frame time ms") + Column("p95 ms") + Column("max ms");
	}

	public string Describe()
	{
		return Column(FramesPerSecond) + Column(GetMilliseconds(AverageFrameTime)) + Column(GetMilliseconds(P95FrameTime)) + Column(GetMilliseconds(MaxFrameTime));
	}

	private static string Column(string text)
	{
		return text.PadLeft(20);
	}

	private static string Column(float value)
	{
		return Column(value.ToString("F1"));
	}

	private static float GetMilliseconds(double seconds)
	{
		return (float)(seconds * 1000.0);
	}
}
