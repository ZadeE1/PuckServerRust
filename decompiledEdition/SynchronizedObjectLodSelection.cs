public struct SynchronizedObjectLodSelection
{
	public const int NO_BAND_INDEX = -1;

	public static readonly SynchronizedObjectLodSelection Default = new SynchronizedObjectLodSelection
	{
		BandIndex = -1,
		TickRateDivisor = 1
	};

	public int BandIndex;

	public SynchronizedObjectLodSource Source;

	public byte TickRateDivisor;

	public bool UseHighPrecisionRotation;

	public bool IsCulled => Source == SynchronizedObjectLodSource.Culled;
}
