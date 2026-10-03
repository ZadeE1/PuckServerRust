using System;
using Unity.Collections;
using Unity.Netcode;

public struct Server : INetworkSerializable, IEquatable<Server>
{
	public FixedString32Bytes IpAddress;

	public ushort Port;

	public FixedString128Bytes Name;

	public int MaxPlayers;

	public int TickRate;

	public bool UseVoip;

	public FixedString64Bytes GameMode;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		if (serializer.IsReader)
		{
			FastBufferReader fastBufferReader = serializer.GetFastBufferReader();
			fastBufferReader.ReadValueSafe(out IpAddress, default(FastBufferWriter.ForFixedStrings));
			fastBufferReader.ReadValueSafe(out Port, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out Name, default(FastBufferWriter.ForFixedStrings));
			fastBufferReader.ReadValueSafe(out MaxPlayers, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out TickRate, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out UseVoip, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out GameMode, default(FastBufferWriter.ForFixedStrings));
		}
		else
		{
			FastBufferWriter fastBufferWriter = serializer.GetFastBufferWriter();
			fastBufferWriter.WriteValueSafe(in IpAddress, default(FastBufferWriter.ForFixedStrings));
			fastBufferWriter.WriteValueSafe(in Port, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in Name, default(FastBufferWriter.ForFixedStrings));
			fastBufferWriter.WriteValueSafe(in MaxPlayers, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in TickRate, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in UseVoip, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in GameMode, default(FastBufferWriter.ForFixedStrings));
		}
	}

	public bool Equals(Server other)
	{
		if (IpAddress == other.IpAddress && Port == other.Port && Name == other.Name && MaxPlayers == other.MaxPlayers && TickRate == other.TickRate && UseVoip == other.UseVoip)
		{
			return GameMode == other.GameMode;
		}
		return false;
	}
}
