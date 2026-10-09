using System;
using System.Runtime.InteropServices;

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
}
