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
}
