using System;
using Unity.Collections;
using Unity.Netcode;

public class ChatMessage : INetworkSerializable, IEquatable<ChatMessage>
{
	public FixedString32Bytes? SteamID;

	public FixedString32Bytes? Username;

	public PlayerTeam? Team;

	public FixedString512Bytes Content;

	public double Timestamp;

	public bool IsQuickChat;

	public bool IsTeamChat;

	public bool IsSystem;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		bool value = SteamID.HasValue;
		serializer.SerializeValue(ref value, default(FastBufferWriter.ForPrimitives));
		if (value)
		{
			FixedString32Bytes value2 = SteamID.GetValueOrDefault();
			serializer.SerializeValue(ref value2, default(FastBufferWriter.ForFixedStrings));
			SteamID = value2;
		}
		bool value3 = Username.HasValue;
		serializer.SerializeValue(ref value3, default(FastBufferWriter.ForPrimitives));
		if (value3)
		{
			FixedString32Bytes value4 = Username.GetValueOrDefault();
			serializer.SerializeValue(ref value4, default(FastBufferWriter.ForFixedStrings));
			Username = value4;
		}
		bool value5 = Team.HasValue;
		serializer.SerializeValue(ref value5, default(FastBufferWriter.ForPrimitives));
		if (value5)
		{
			PlayerTeam value6 = Team.GetValueOrDefault();
			serializer.SerializeValue(ref value6, default(FastBufferWriter.ForEnums));
			Team = value6;
		}
		serializer.SerializeValue(ref Content, default(FastBufferWriter.ForFixedStrings));
		serializer.SerializeValue(ref Timestamp, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref IsQuickChat, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref IsTeamChat, default(FastBufferWriter.ForPrimitives));
		serializer.SerializeValue(ref IsSystem, default(FastBufferWriter.ForPrimitives));
	}

	public bool Equals(ChatMessage other)
	{
		FixedString32Bytes? steamID = SteamID;
		FixedString32Bytes? steamID2 = other.SteamID;
		if (steamID.HasValue == steamID2.HasValue && (!steamID.HasValue || steamID.GetValueOrDefault() == steamID2.GetValueOrDefault()))
		{
			steamID2 = Username;
			steamID = other.Username;
			if (steamID2.HasValue == steamID.HasValue && (!steamID2.HasValue || steamID2.GetValueOrDefault() == steamID.GetValueOrDefault()) && Content == other.Content && Timestamp == other.Timestamp && IsQuickChat == other.IsQuickChat && IsTeamChat == other.IsTeamChat)
			{
				return IsSystem == other.IsSystem;
			}
		}
		return false;
	}
}
