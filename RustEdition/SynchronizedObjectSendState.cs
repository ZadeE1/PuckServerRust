public struct SynchronizedObjectSendState
{
	public SynchronizedObjectLodSelection LodSelection;

	public SynchronizedObjectData LastSentData;

	public bool HasLastSentData;

	public bool IsAwake;

	public uint LastFullSendTick;
}
