using System;
using Unity.Netcode;

public struct PlayerGameState : INetworkSerializable, IEquatable<PlayerGameState>
{
	public PlayerPhase Phase;

	public PlayerTeam Team;

	public PlayerRole Role;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		if (serializer.IsReader)
		{
			FastBufferReader fastBufferReader = serializer.GetFastBufferReader();
			fastBufferReader.ReadValueSafe(out Phase, default(FastBufferWriter.ForEnums));
			fastBufferReader.ReadValueSafe(out Team, default(FastBufferWriter.ForEnums));
			fastBufferReader.ReadValueSafe(out Role, default(FastBufferWriter.ForEnums));
		}
		else
		{
			FastBufferWriter fastBufferWriter = serializer.GetFastBufferWriter();
			fastBufferWriter.WriteValueSafe(in Phase, default(FastBufferWriter.ForEnums));
			fastBufferWriter.WriteValueSafe(in Team, default(FastBufferWriter.ForEnums));
			fastBufferWriter.WriteValueSafe(in Role, default(FastBufferWriter.ForEnums));
		}
	}

	public bool Equals(PlayerGameState other)
	{
		if (Phase == other.Phase && Team == other.Team)
		{
			return Role == other.Role;
		}
		return false;
	}

	public override string ToString()
	{
		return $"Phase: {Phase}, Team: {Team}, Role: {Role}";
	}
}
