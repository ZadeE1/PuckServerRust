using System;
using Unity.Netcode;

public struct PlayerCustomizationState : INetworkSerializable, IEquatable<PlayerCustomizationState>
{
	public int FlagID;

	public int HeadgearIDBlueAttacker;

	public int HeadgearIDRedAttacker;

	public int HeadgearIDBlueGoalie;

	public int HeadgearIDRedGoalie;

	public int MustacheID;

	public int BeardID;

	public int JerseyIDBlueAttacker;

	public int JerseyIDRedAttacker;

	public int JerseyIDBlueGoalie;

	public int JerseyIDRedGoalie;

	public int StickSkinIDBlueAttacker;

	public int StickSkinIDRedAttacker;

	public int StickSkinIDBlueGoalie;

	public int StickSkinIDRedGoalie;

	public int StickShaftTapeIDBlueAttacker;

	public int StickShaftTapeIDRedAttacker;

	public int StickShaftTapeIDBlueGoalie;

	public int StickShaftTapeIDRedGoalie;

	public int StickBladeTapeIDBlueAttacker;

	public int StickBladeTapeIDRedAttacker;

	public int StickBladeTapeIDBlueGoalie;

	public int StickBladeTapeIDRedGoalie;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		if (serializer.IsReader)
		{
			FastBufferReader fastBufferReader = serializer.GetFastBufferReader();
			fastBufferReader.ReadValueSafe(out FlagID, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out HeadgearIDBlueAttacker, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out HeadgearIDRedAttacker, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out HeadgearIDBlueGoalie, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out HeadgearIDRedGoalie, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out MustacheID, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out BeardID, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out JerseyIDBlueAttacker, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out JerseyIDRedAttacker, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out JerseyIDBlueGoalie, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out JerseyIDRedGoalie, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out StickSkinIDBlueAttacker, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out StickSkinIDRedAttacker, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out StickSkinIDBlueGoalie, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out StickSkinIDRedGoalie, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out StickShaftTapeIDBlueAttacker, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out StickShaftTapeIDRedAttacker, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out StickShaftTapeIDBlueGoalie, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out StickShaftTapeIDRedGoalie, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out StickBladeTapeIDBlueAttacker, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out StickBladeTapeIDRedAttacker, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out StickBladeTapeIDBlueGoalie, default(FastBufferWriter.ForPrimitives));
			fastBufferReader.ReadValueSafe(out StickBladeTapeIDRedGoalie, default(FastBufferWriter.ForPrimitives));
		}
		else
		{
			FastBufferWriter fastBufferWriter = serializer.GetFastBufferWriter();
			fastBufferWriter.WriteValueSafe(in FlagID, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in HeadgearIDBlueAttacker, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in HeadgearIDRedAttacker, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in HeadgearIDBlueGoalie, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in HeadgearIDRedGoalie, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in MustacheID, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in BeardID, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in JerseyIDBlueAttacker, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in JerseyIDRedAttacker, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in JerseyIDBlueGoalie, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in JerseyIDRedGoalie, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in StickSkinIDBlueAttacker, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in StickSkinIDRedAttacker, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in StickSkinIDBlueGoalie, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in StickSkinIDRedGoalie, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in StickShaftTapeIDBlueAttacker, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in StickShaftTapeIDRedAttacker, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in StickShaftTapeIDBlueGoalie, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in StickShaftTapeIDRedGoalie, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in StickBladeTapeIDBlueAttacker, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in StickBladeTapeIDRedAttacker, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in StickBladeTapeIDBlueGoalie, default(FastBufferWriter.ForPrimitives));
			fastBufferWriter.WriteValueSafe(in StickBladeTapeIDRedGoalie, default(FastBufferWriter.ForPrimitives));
		}
	}

	public bool Equals(PlayerCustomizationState other)
	{
		if (FlagID == other.FlagID && HeadgearIDBlueAttacker == other.HeadgearIDBlueAttacker && HeadgearIDRedAttacker == other.HeadgearIDRedAttacker && HeadgearIDBlueGoalie == other.HeadgearIDBlueGoalie && HeadgearIDRedGoalie == other.HeadgearIDRedGoalie && MustacheID == other.MustacheID && BeardID == other.BeardID && JerseyIDBlueAttacker == other.JerseyIDBlueAttacker && JerseyIDRedAttacker == other.JerseyIDRedAttacker && JerseyIDBlueGoalie == other.JerseyIDBlueGoalie && JerseyIDRedGoalie == other.JerseyIDRedGoalie && StickSkinIDBlueAttacker == other.StickSkinIDBlueAttacker && StickSkinIDRedAttacker == other.StickSkinIDRedAttacker && StickSkinIDBlueGoalie == other.StickSkinIDBlueGoalie && StickSkinIDRedGoalie == other.StickSkinIDRedGoalie && StickShaftTapeIDBlueAttacker == other.StickShaftTapeIDBlueAttacker && StickShaftTapeIDRedAttacker == other.StickShaftTapeIDRedAttacker && StickShaftTapeIDBlueGoalie == other.StickShaftTapeIDBlueGoalie && StickShaftTapeIDRedGoalie == other.StickShaftTapeIDRedGoalie && StickBladeTapeIDBlueAttacker == other.StickBladeTapeIDBlueAttacker && StickBladeTapeIDRedAttacker == other.StickBladeTapeIDRedAttacker && StickBladeTapeIDBlueGoalie == other.StickBladeTapeIDBlueGoalie)
		{
			return StickBladeTapeIDRedGoalie == other.StickBladeTapeIDRedGoalie;
		}
		return false;
	}

	public override string ToString()
	{
		return $"FlagID: {FlagID}, HeadgearIDs: [{HeadgearIDBlueAttacker}, {HeadgearIDRedAttacker}, {HeadgearIDBlueGoalie}, {HeadgearIDRedGoalie}], MustacheID: {MustacheID}, BeardID: {BeardID}, JerseyIDs: [{JerseyIDBlueAttacker}, {JerseyIDRedAttacker}, {JerseyIDBlueGoalie}, {JerseyIDRedGoalie}], StickSkinIDs: [{StickSkinIDBlueAttacker}, {StickSkinIDRedAttacker}, {StickSkinIDBlueGoalie}, {StickSkinIDRedGoalie}], StickShaftTapeIDs: [{StickShaftTapeIDBlueAttacker}, {StickShaftTapeIDRedAttacker}, {StickShaftTapeIDBlueGoalie}, {StickShaftTapeIDRedGoalie}], StickBladeTapeIDs: [{StickBladeTapeIDBlueAttacker}, {StickBladeTapeIDRedAttacker}, {StickBladeTapeIDBlueGoalie}, {StickBladeTapeIDRedGoalie}]";
	}
}
