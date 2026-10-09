using Unity.Netcode;
using UnityEngine;

public struct SynchronizedObjectData : INetworkSerializable
{
	public const ushort CHANGE_MASK_POSITION_X = 1;

	public const ushort CHANGE_MASK_POSITION_Y = 2;

	public const ushort CHANGE_MASK_POSITION_Z = 4;

	public const ushort CHANGE_MASK_ROTATION = 8;

	public const ushort CHANGE_MASK_LINEAR_VELOCITY_X = 16;

	public const ushort CHANGE_MASK_LINEAR_VELOCITY_Y = 32;

	public const ushort CHANGE_MASK_LINEAR_VELOCITY_Z = 64;

	public const ushort CHANGE_MASK_ANGULAR_VELOCITY_X = 128;

	public const ushort CHANGE_MASK_ANGULAR_VELOCITY_Y = 256;

	public const ushort CHANGE_MASK_ANGULAR_VELOCITY_Z = 512;

	public const ushort CHANGE_MASK_HIGH_PRECISION_ROTATION = 1024;

	public const ushort CHANGE_MASK_TICK_RATE_DIVISOR = 2048;

	public const ushort CHANGE_MASK_ASLEEP = 4096;

	public const ushort CHANGE_MASK_POSITION = 7;

	public const ushort CHANGE_MASK_LINEAR_VELOCITY = 112;

	public const ushort CHANGE_MASK_ANGULAR_VELOCITY = 896;

	public const ushort CHANGE_MASK_MOTION_COMPONENTS = 1023;

	public const ushort CHANGE_MASK_ALL_COMPONENTS = 3071;

	private const float POSITION_CHANGE_THRESHOLD = 0.002f;

	private const float ROTATION_CHANGE_THRESHOLD = 0.05f;

	private const float LINEAR_VELOCITY_CHANGE_THRESHOLD = 0.05f;

	private const float ANGULAR_VELOCITY_CHANGE_THRESHOLD = 0.1f;

	private const float POSITION_MIN_X = -25f;

	private const float POSITION_MAX_X = 25f;

	private const float POSITION_MIN_Y = -50f;

	private const float POSITION_MAX_Y = 50f;

	private const float POSITION_MIN_Z = -50f;

	private const float POSITION_MAX_Z = 50f;

	private const float ROTATION_MIN = -1f;

	private const float ROTATION_MAX = 1f;

	private const float LINEAR_VELOCITY_MIN = -100f;

	private const float LINEAR_VELOCITY_MAX = 100f;

	private const float ANGULAR_VELOCITY_MIN = -100f;

	private const float ANGULAR_VELOCITY_MAX = 100f;

	public ushort NetworkObjectId;

	public ushort ChangeMask;

	public byte TickRateDivisor;

	public short X;

	public short Y;

	public short Z;

	public uint CompressedRotation;

	public short Rx;

	public short Ry;

	public short Rz;

	public short Rw;

	public short Vx;

	public short Vy;

	public short Vz;

	public short Ax;

	public short Ay;

	public short Az;

	public bool HasAllComponents => (ChangeMask & 0xBFF) == 3071;

	public bool IsAsleep => (ChangeMask & 0x1000) != 0;

	public Quaternion Rotation => GetRotation((ChangeMask & 0x400) != 0);

	public Vector3 LinearVelocity => new Vector3(NetworkingUtils.DecompressShortToFloat(Vx, -100f, 100f), NetworkingUtils.DecompressShortToFloat(Vy, -100f, 100f), NetworkingUtils.DecompressShortToFloat(Vz, -100f, 100f));

	public Vector3 AngularVelocity => new Vector3(NetworkingUtils.DecompressShortToFloat(Ax, -100f, 100f), NetworkingUtils.DecompressShortToFloat(Ay, -100f, 100f), NetworkingUtils.DecompressShortToFloat(Az, -100f, 100f));

	public Vector3 GetPosition()
	{
		return new Vector3(NetworkingUtils.DecompressShortToFloat(X, -25f, 25f), NetworkingUtils.DecompressShortToFloat(Y, -50f, 50f), NetworkingUtils.DecompressShortToFloat(Z, -50f, 50f));
	}

	public Quaternion GetRotation(bool useHighPrecisionRotation)
	{
		if (useHighPrecisionRotation)
		{
			return Quaternion.Normalize(new Quaternion(NetworkingUtils.DecompressShortToFloat(Rx, -1f, 1f), NetworkingUtils.DecompressShortToFloat(Ry, -1f, 1f), NetworkingUtils.DecompressShortToFloat(Rz, -1f, 1f), NetworkingUtils.DecompressShortToFloat(Rw, -1f, 1f)));
		}
		Quaternion quaternion = Quaternion.identity;
		QuaternionCompressor.DecompressQuaternion(ref quaternion, CompressedRotation);
		return quaternion;
	}

	public SynchronizedObjectData(ulong networkObjectId, Vector3 position, Quaternion rotation, Vector3 linearVelocity, Vector3 angularVelocity)
	{
		long syncPerfStart = SyncPerf.Enter();
		NetworkObjectId = (ushort)networkObjectId;
		ChangeMask = 4095;
		X = NetworkingUtils.CompressFloatToShort(position.x, -25f, 25f);
		Y = NetworkingUtils.CompressFloatToShort(position.y, -50f, 50f);
		Z = NetworkingUtils.CompressFloatToShort(position.z, -50f, 50f);
		Rx = NetworkingUtils.CompressFloatToShort(rotation.x, -1f, 1f);
		Ry = NetworkingUtils.CompressFloatToShort(rotation.y, -1f, 1f);
		Rz = NetworkingUtils.CompressFloatToShort(rotation.z, -1f, 1f);
		Rw = NetworkingUtils.CompressFloatToShort(rotation.w, -1f, 1f);
		CompressedRotation = QuaternionCompressor.CompressQuaternion(ref rotation);
		Vx = NetworkingUtils.CompressFloatToShort(linearVelocity.x, -100f, 100f);
		Vy = NetworkingUtils.CompressFloatToShort(linearVelocity.y, -100f, 100f);
		Vz = NetworkingUtils.CompressFloatToShort(linearVelocity.z, -100f, 100f);
		Ax = NetworkingUtils.CompressFloatToShort(angularVelocity.x, -100f, 100f);
		Ay = NetworkingUtils.CompressFloatToShort(angularVelocity.y, -100f, 100f);
		Az = NetworkingUtils.CompressFloatToShort(angularVelocity.z, -100f, 100f);
		TickRateDivisor = 1;
		SyncPerf.Exit(SyncPerf.DataCtor, syncPerfStart);
	}

	public SynchronizedObjectData WithTickRateDivisor(byte tickRateDivisor)
	{
		long syncPerfStart = SyncPerf.Enter();
		SynchronizedObjectData result = this;
		result.TickRateDivisor = tickRateDivisor;
		SyncPerf.Exit(SyncPerf.DataWithTickRateDivisor, syncPerfStart);
		return result;
	}

	public ushort GetChangeMask(SynchronizedObjectData other, bool useHighPrecisionRotation)
	{
		long syncPerfStart = SyncPerf.Enter();
		ushort num = GetAxisChangeMask(GetPosition(), other.GetPosition(), 0.002f, 1, 2, 4);
		if (HasRotationChanged(other, useHighPrecisionRotation) && GetAngleDegrees(GetRotation(useHighPrecisionRotation), other.GetRotation(useHighPrecisionRotation)) > 0.05f)
		{
			num |= 8;
		}
		num |= GetAxisChangeMask(LinearVelocity, other.LinearVelocity, 0.05f, 16, 32, 64);
		num |= GetAxisChangeMask(AngularVelocity, other.AngularVelocity, 0.1f, 128, 256, 512);
		if (TickRateDivisor != other.TickRateDivisor)
		{
			num |= 0x800;
		}
		SyncPerf.Exit(SyncPerf.DataGetChangeMask, syncPerfStart);
		return num;
	}

	public SynchronizedObjectData WithAsleep()
	{
		long syncPerfStart = SyncPerf.Enter();
		SynchronizedObjectData result = this;
		result.ChangeMask |= 4096;
		SyncPerf.Exit(SyncPerf.DataWithAsleep, syncPerfStart);
		return result;
	}

	public SynchronizedObjectData WithComponentMask(ushort componentMask, bool useHighPrecisionRotation)
	{
		long syncPerfStart = SyncPerf.Enter();
		SynchronizedObjectData result = this;
		result.ChangeMask = (ushort)(componentMask & 0xBFF);
		if (useHighPrecisionRotation && (result.ChangeMask & 8) != 0)
		{
			result.ChangeMask |= 1024;
		}
		SyncPerf.Exit(SyncPerf.DataWithComponentMask, syncPerfStart);
		return result;
	}

	public SynchronizedObjectData Merge(SynchronizedObjectData changes)
	{
		long syncPerfStart = SyncPerf.Enter();
		SynchronizedObjectData result = this;
		if ((changes.ChangeMask & 1) != 0)
		{
			result.X = changes.X;
		}
		if ((changes.ChangeMask & 2) != 0)
		{
			result.Y = changes.Y;
		}
		if ((changes.ChangeMask & 4) != 0)
		{
			result.Z = changes.Z;
		}
		if ((changes.ChangeMask & 8) != 0)
		{
			result.CompressedRotation = changes.CompressedRotation;
			result.Rx = changes.Rx;
			result.Ry = changes.Ry;
			result.Rz = changes.Rz;
			result.Rw = changes.Rw;
		}
		if ((changes.ChangeMask & 0x10) != 0)
		{
			result.Vx = changes.Vx;
		}
		if ((changes.ChangeMask & 0x20) != 0)
		{
			result.Vy = changes.Vy;
		}
		if ((changes.ChangeMask & 0x40) != 0)
		{
			result.Vz = changes.Vz;
		}
		if ((changes.ChangeMask & 0x80) != 0)
		{
			result.Ax = changes.Ax;
		}
		if ((changes.ChangeMask & 0x100) != 0)
		{
			result.Ay = changes.Ay;
		}
		if ((changes.ChangeMask & 0x200) != 0)
		{
			result.Az = changes.Az;
		}
		if ((changes.ChangeMask & 0x800) != 0)
		{
			result.TickRateDivisor = changes.TickRateDivisor;
		}
		ushort num = (((changes.ChangeMask & 8) != 0) ? changes.ChangeMask : ChangeMask);
		result.ChangeMask = (ushort)(0xBFF | (num & 0x400));
		SyncPerf.Exit(SyncPerf.DataMerge, syncPerfStart);
		return result;
	}

	private bool HasRotationChanged(SynchronizedObjectData other, bool useHighPrecisionRotation)
	{
		if (useHighPrecisionRotation)
		{
			if (Rx == other.Rx && Ry == other.Ry && Rz == other.Rz)
			{
				return Rw != other.Rw;
			}
			return true;
		}
		return CompressedRotation != other.CompressedRotation;
	}

	private static ushort GetAxisChangeMask(Vector3 value, Vector3 otherValue, float changeThreshold, ushort xAxisMask, ushort yAxisMask, ushort zAxisMask)
	{
		ushort num = 0;
		if (Mathf.Abs(value.x - otherValue.x) > changeThreshold)
		{
			num |= xAxisMask;
		}
		if (Mathf.Abs(value.y - otherValue.y) > changeThreshold)
		{
			num |= yAxisMask;
		}
		if (Mathf.Abs(value.z - otherValue.z) > changeThreshold)
		{
			num |= zAxisMask;
		}
		return num;
	}

	private static float GetAngleDegrees(Quaternion a, Quaternion b)
	{
		if (Quaternion.Dot(a, b) < 0f)
		{
			b = new Quaternion(0f - b.x, 0f - b.y, 0f - b.z, 0f - b.w);
		}
		float num = a.x - b.x;
		float num2 = a.y - b.y;
		float num3 = a.z - b.z;
		float num4 = a.w - b.w;
		float b2 = 0.5f * Mathf.Sqrt(num * num + num2 * num2 + num3 * num3 + num4 * num4);
		return 4f * Mathf.Asin(Mathf.Min(1f, b2)) * 57.29578f;
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
		reader.ReadValueSafe(out NetworkObjectId, default(FastBufferWriter.ForPrimitives));
		reader.ReadValueSafe(out ChangeMask, default(FastBufferWriter.ForPrimitives));
		if ((ChangeMask & 1) != 0)
		{
			reader.ReadValueSafe(out X, default(FastBufferWriter.ForPrimitives));
		}
		if ((ChangeMask & 2) != 0)
		{
			reader.ReadValueSafe(out Y, default(FastBufferWriter.ForPrimitives));
		}
		if ((ChangeMask & 4) != 0)
		{
			reader.ReadValueSafe(out Z, default(FastBufferWriter.ForPrimitives));
		}
		if ((ChangeMask & 8) != 0)
		{
			if ((ChangeMask & 0x400) != 0)
			{
				reader.ReadValueSafe(out Rx, default(FastBufferWriter.ForPrimitives));
				reader.ReadValueSafe(out Ry, default(FastBufferWriter.ForPrimitives));
				reader.ReadValueSafe(out Rz, default(FastBufferWriter.ForPrimitives));
				reader.ReadValueSafe(out Rw, default(FastBufferWriter.ForPrimitives));
			}
			else
			{
				reader.ReadValueSafe(out CompressedRotation, default(FastBufferWriter.ForPrimitives));
			}
		}
		if ((ChangeMask & 0x10) != 0)
		{
			reader.ReadValueSafe(out Vx, default(FastBufferWriter.ForPrimitives));
		}
		if ((ChangeMask & 0x20) != 0)
		{
			reader.ReadValueSafe(out Vy, default(FastBufferWriter.ForPrimitives));
		}
		if ((ChangeMask & 0x40) != 0)
		{
			reader.ReadValueSafe(out Vz, default(FastBufferWriter.ForPrimitives));
		}
		if ((ChangeMask & 0x80) != 0)
		{
			reader.ReadValueSafe(out Ax, default(FastBufferWriter.ForPrimitives));
		}
		if ((ChangeMask & 0x100) != 0)
		{
			reader.ReadValueSafe(out Ay, default(FastBufferWriter.ForPrimitives));
		}
		if ((ChangeMask & 0x200) != 0)
		{
			reader.ReadValueSafe(out Az, default(FastBufferWriter.ForPrimitives));
		}
		if ((ChangeMask & 0x800) != 0)
		{
			reader.ReadValueSafe(out TickRateDivisor, default(FastBufferWriter.ForPrimitives));
		}
	}

	private void Write(FastBufferWriter writer)
	{
		writer.WriteValueSafe(in NetworkObjectId, default(FastBufferWriter.ForPrimitives));
		writer.WriteValueSafe(in ChangeMask, default(FastBufferWriter.ForPrimitives));
		if ((ChangeMask & 1) != 0)
		{
			writer.WriteValueSafe(in X, default(FastBufferWriter.ForPrimitives));
		}
		if ((ChangeMask & 2) != 0)
		{
			writer.WriteValueSafe(in Y, default(FastBufferWriter.ForPrimitives));
		}
		if ((ChangeMask & 4) != 0)
		{
			writer.WriteValueSafe(in Z, default(FastBufferWriter.ForPrimitives));
		}
		if ((ChangeMask & 8) != 0)
		{
			if ((ChangeMask & 0x400) != 0)
			{
				writer.WriteValueSafe(in Rx, default(FastBufferWriter.ForPrimitives));
				writer.WriteValueSafe(in Ry, default(FastBufferWriter.ForPrimitives));
				writer.WriteValueSafe(in Rz, default(FastBufferWriter.ForPrimitives));
				writer.WriteValueSafe(in Rw, default(FastBufferWriter.ForPrimitives));
			}
			else
			{
				writer.WriteValueSafe(in CompressedRotation, default(FastBufferWriter.ForPrimitives));
			}
		}
		if ((ChangeMask & 0x10) != 0)
		{
			writer.WriteValueSafe(in Vx, default(FastBufferWriter.ForPrimitives));
		}
		if ((ChangeMask & 0x20) != 0)
		{
			writer.WriteValueSafe(in Vy, default(FastBufferWriter.ForPrimitives));
		}
		if ((ChangeMask & 0x40) != 0)
		{
			writer.WriteValueSafe(in Vz, default(FastBufferWriter.ForPrimitives));
		}
		if ((ChangeMask & 0x80) != 0)
		{
			writer.WriteValueSafe(in Ax, default(FastBufferWriter.ForPrimitives));
		}
		if ((ChangeMask & 0x100) != 0)
		{
			writer.WriteValueSafe(in Ay, default(FastBufferWriter.ForPrimitives));
		}
		if ((ChangeMask & 0x200) != 0)
		{
			writer.WriteValueSafe(in Az, default(FastBufferWriter.ForPrimitives));
		}
		if ((ChangeMask & 0x800) != 0)
		{
			writer.WriteValueSafe(in TickRateDivisor, default(FastBufferWriter.ForPrimitives));
		}
	}
}
