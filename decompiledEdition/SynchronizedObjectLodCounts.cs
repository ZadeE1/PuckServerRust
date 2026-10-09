public class SynchronizedObjectLodCounts
{
	private int[] bandCounts = new int[0];

	private int defaultCount;

	private int culledCount;

	private int noOriginCount;

	public int DefaultCount => defaultCount;

	public int CulledCount => culledCount;

	public int NoOriginCount => noOriginCount;

	public void Configure(int bandCount)
	{
		if (bandCounts.Length != bandCount)
		{
			bandCounts = new int[bandCount];
		}
	}

	public void Clear()
	{
		for (int i = 0; i < bandCounts.Length; i++)
		{
			bandCounts[i] = 0;
		}
		defaultCount = 0;
		culledCount = 0;
		noOriginCount = 0;
	}

	public void Add(in SynchronizedObjectLodSelection selection)
	{
		switch (selection.Source)
		{
		case SynchronizedObjectLodSource.Band:
			AddBand(selection.BandIndex);
			break;
		case SynchronizedObjectLodSource.Culled:
			culledCount++;
			break;
		case SynchronizedObjectLodSource.NoOrigin:
			noOriginCount++;
			break;
		default:
			defaultCount++;
			break;
		}
	}

	public int[] TakeBandCounts()
	{
		return (int[])bandCounts.Clone();
	}

	private void AddBand(int bandIndex)
	{
		if (bandIndex >= 0 && bandIndex < bandCounts.Length)
		{
			bandCounts[bandIndex]++;
		}
	}
}
