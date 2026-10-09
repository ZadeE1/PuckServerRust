using Unity.Netcode;

public struct SynchronizedObjectTickHeader : INetworkSerializable
{
	public const byte CHANGE_MASK_TIME_SCALE = 1;

	public const byte CHANGE_MASK_TICK_INTERVAL = 2;

	public const byte CHANGE_MASK_ALL_COMPONENTS = 3;

	public byte ChangeMask;

	public ushort SequenceNumber;

	public double ServerTime;

	public float TimeScale;

	public float TickInterval;

	public bool HasTimeScale => (ChangeMask & 1) != 0;

	public bool HasTickInterval => (ChangeMask & 2) != 0;

	public SynchronizedObjectTickHeader(ushort sequenceNumber, double serverTime, float timeScale, float tickInterval)
	{
		ChangeMask = 3;
		SequenceNumber = sequenceNumber;
		ServerTime = serverTime;
		TimeScale = timeScale;
		TickInterval = tickInterval;
	}

	public byte GetChangeMask(SynchronizedObjectTickHeader other)
	{
		byte b = 0;
		if (TimeScale != other.TimeScale)
		{
			b |= 1;
		}
		if (TickInterval != other.TickInterval)
		{
			b |= 2;
		}
		return b;
	}

	public SynchronizedObjectTickHeader WithComponentMask(byte componentMask)
	{
		SynchronizedObjectTickHeader result = this;
		result.ChangeMask = (byte)(componentMask & 3);
		return result;
	}

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		if (serializer.IsReader)
		{
			Read(serializer.GetFastBufferReader());
		}
		else
		{
			Write(serializer.GetFastBufferWriter());
		}
	}

	private void Read(FastBufferReader reader)
	{
		reader.ReadValueSafe(out ChangeMask, default(FastBufferWriter.ForPrimitives));
		reader.ReadValueSafe(out SequenceNumber, default(FastBufferWriter.ForPrimitives));
		reader.ReadValueSafe(out ServerTime, default(FastBufferWriter.ForPrimitives));
		if (HasTimeScale)
		{
			reader.ReadValueSafe(out TimeScale, default(FastBufferWriter.ForPrimitives));
		}
		if (HasTickInterval)
		{
			reader.ReadValueSafe(out TickInterval, default(FastBufferWriter.ForPrimitives));
		}
	}

	private void Write(FastBufferWriter writer)
	{
		writer.WriteValueSafe(in ChangeMask, default(FastBufferWriter.ForPrimitives));
		writer.WriteValueSafe(in SequenceNumber, default(FastBufferWriter.ForPrimitives));
		writer.WriteValueSafe(in ServerTime, default(FastBufferWriter.ForPrimitives));
		if (HasTimeScale)
		{
			writer.WriteValueSafe(in TimeScale, default(FastBufferWriter.ForPrimitives));
		}
		if (HasTickInterval)
		{
			writer.WriteValueSafe(in TickInterval, default(FastBufferWriter.ForPrimitives));
		}
	}
}
