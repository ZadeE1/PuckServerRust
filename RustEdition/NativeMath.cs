using System;
using System.Runtime.InteropServices;
using UnityEngine;

// ponytail: thin P/Invoke shim to the Rust plugin (same audio_curve binary as
// NativeAudio — one plugin, one surface). All state lives in the caller's
// fields (passed by ref), so the managed fallback below stays consistent.
// Missing plugin = silent managed fallback, never a crash.
public static class NativeMath
{
	[DllImport("audio_curve", CallingConvention = CallingConvention.Cdecl)]
	private static extern float pid_update(ref float errorLast, ref float valueLast, ref float integrationStored, ref float derivativeLast, ref int derivativeInitialized, float proportionalGain, float integralGain, float integralSaturation, float derivativeGain, float derivativeSmoothing, float outputMin, float outputMax, int measurement, float deltaTime, float currentValue, float targetValue, int angleMode);

	// Bit-identical mirror is SyncMaskInput in the Rust plugin; field order matters.
	[StructLayout(LayoutKind.Sequential)]
	public struct SyncMaskInput
	{
		public short X;
		public short Y;
		public short Z;
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
		public byte TickRateDivisor;
		public uint CompressedRotation;
	}

	[DllImport("audio_curve", CallingConvention = CallingConvention.Cdecl)]
	private static extern ushort sync_mask(SyncMaskInput a, SyncMaskInput b, int useHighPrecisionRotation);

	// Bit-identical mirror is LodBandFlat in the Rust plugin; field order matters.
	[StructLayout(LayoutKind.Sequential)]
	public struct LodBandFlat
	{
		public float MinDistance;
		public byte TickRateDivisor;
	}

	// Mirrors LodSelectOut in the Rust plugin (band, source, divisor, high-prec).
	[StructLayout(LayoutKind.Sequential)]
	public struct LodSelectOut
	{
		public int BandIndex;
		public int Source;
		public byte TickRateDivisor;
		public byte UseHighPrecisionRotation;
	}

	[DllImport("audio_curve", CallingConvention = CallingConvention.Cdecl)]
	private static extern unsafe void sync_lod_configure(LodBandFlat* bands, int bandCount, float cullingMinDistance, byte cullingTickRateDivisor, byte noOriginTickRateDivisor, float hysteresis, int useHighPrecisionRotation);

	[DllImport("audio_curve", CallingConvention = CallingConvention.Cdecl)]
	private static extern LodSelectOut sync_lod_select(float originX, float originY, float originZ, float viewX, float viewY, float viewZ, float posX, float posY, float posZ, int prevBandIndex, int prevCulled, int usesLod, int usesCulling, int hasOrigin);

	private static bool useNative = true;

	// True when native handled the call (result valid); false = use managed path.
	public static bool TryPidUpdate(ref float errorLast, ref float valueLast, ref float integrationStored, ref float derivativeLast, ref int derivativeInitialized, float proportionalGain, float integralGain, float integralSaturation, float derivativeGain, float derivativeSmoothing, float outputMin, float outputMax, int measurement, float deltaTime, float currentValue, float targetValue, int angleMode, out float result)
	{
		result = 0f;
		if (!useNative)
		{
			return false;
		}
		try
		{
			result = pid_update(ref errorLast, ref valueLast, ref integrationStored, ref derivativeLast, ref derivativeInitialized, proportionalGain, integralGain, integralSaturation, derivativeGain, derivativeSmoothing, outputMin, outputMax, measurement, deltaTime, currentValue, targetValue, angleMode);
			return true;
		}
		catch
		{
			useNative = false;
			return false;
		}
	}

	// True when native handled the call (result valid); false = use managed path.
	// Proven bit-identical over 500k fuzz cases; short-circuits unchanged inputs.
	public static bool TryGetChangeMask(SyncMaskInput a, SyncMaskInput b, bool useHighPrecisionRotation, out ushort result)
	{
		result = 0;
		if (!useNative)
		{
			return false;
		}
		try
		{
			result = sync_mask(a, b, useHighPrecisionRotation ? 1 : 0);
			return true;
		}
		catch
		{
			useNative = false;
			return false;
		}
	}

	// Copies band config to the plugin. Cheap enough to call per Configure
	// (small array, config values rarely change).
	public static unsafe void SyncLodConfig(LodBandFlat[] bands, float cullingMinDistance, byte cullingTickRateDivisor, byte noOriginTickRateDivisor, float hysteresis, bool useHighPrecisionRotation)
	{
		if (!useNative)
		{
			return;
		}
		try
		{
			if (bands == null || bands.Length == 0)
			{
				sync_lod_configure(null, 0, cullingMinDistance, cullingTickRateDivisor, noOriginTickRateDivisor, hysteresis, useHighPrecisionRotation ? 1 : 0);
				return;
			}
			fixed (LodBandFlat* ptr = bands)
			{
				sync_lod_configure(ptr, bands.Length, cullingMinDistance, cullingTickRateDivisor, noOriginTickRateDivisor, hysteresis, useHighPrecisionRotation ? 1 : 0);
			}
		}
		catch
		{
			useNative = false;
		}
	}

	// True when native handled the call (selection valid); false = use managed path.
	// Proven bit-identical over 300k fuzz cases.
	public static bool TryLodSelect(Vector3 origin, Vector3 viewDirection, bool hasOrigin, Vector3 position, int prevBandIndex, bool prevCulled, bool usesLod, bool usesCulling, out LodSelectOut selection)
	{
		selection = default(LodSelectOut);
		if (!useNative)
		{
			return false;
		}
		try
		{
			selection = sync_lod_select(origin.x, origin.y, origin.z, viewDirection.x, viewDirection.y, viewDirection.z, position.x, position.y, position.z, prevBandIndex, prevCulled ? 1 : 0, usesLod ? 1 : 0, usesCulling ? 1 : 0, hasOrigin ? 1 : 0);
			return true;
		}
		catch
		{
			useNative = false;
			return false;
		}
	}
}
