using System;
using Unity.Netcode;

public struct GameState : INetworkSerializable, IEquatable<GameState>
{
	public GamePhase Phase;

	public int Tick;

	public int Period;

	public int BlueScore;

	public int RedScore;

	public bool IsOvertime;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		if (serializer.IsReader)
		{
			FastBufferReader fastBufferReader = serializer.GetFastBufferReader();
			fastBufferReader.ReadValueSafe(out Phase, default(FastBufferWriter.ForEnums));
			fastBufferReader.ReadValueSafe(out Tick, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out Period, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out BlueScore, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out RedScore, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out IsOvertime, default(FastBufferWriter.ForPrimitives));
		}
		else
		{
			FastBufferWriter fastBufferWriter = serializer.GetFastBufferWriter();
			fastBufferWriter.WriteValueSafe(in Phase, default(FastBufferWriter.ForEnums));
			fastBufferWriter.WriteValueSafe(in Tick, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in Period, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in BlueScore, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in RedScore, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in IsOvertime, default(FastBufferWriter.ForPrimitives));
		}
	}

	public bool Equals(GameState other)
	{
		if (Phase.Equals(other.Phase) && Tick == other.Tick && Period == other.Period && BlueScore == other.BlueScore && RedScore == other.RedScore)
		{
			return IsOvertime == other.IsOvertime;
		}
		return false;
	}
}
