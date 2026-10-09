using System;
using Unity.Netcode;

public struct GameModeClientConfig : INetworkSerializable, IEquatable<GameModeClientConfig>
{
	public bool GoalieCreaseProtection;

	public int MaxPeriods;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref GoalieCreaseProtection, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref MaxPeriods, default(FastBufferWriter.ForPrimitives));
	}

	public bool Equals(GameModeClientConfig other)
	{
		if (GoalieCreaseProtection == other.GoalieCreaseProtection)
		{
			return MaxPeriods == other.MaxPeriods;
		}
		return false;
	}
}
