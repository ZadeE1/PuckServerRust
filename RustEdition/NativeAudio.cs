using System;
using System.Runtime.InteropServices;
using UnityEngine;

// ponytail: thin P/Invoke shim to the Rust audio_curve plugin. All Unity and
// NetworkVar calls stay in managed code; native only does curve math + gating.
// Missing plugin = silent managed fallback, never a crash.
public static class NativeAudio
{
	[DllImport("audio_curve", CallingConvention = CallingConvention.Cdecl)]
	private static extern void audio_curve_init([In] float[] vol, int volKeys, [In] float[] pitch, int pitchKeys);

	[DllImport("audio_curve", CallingConvention = CallingConvention.Cdecl)]
	private static extern int audio_wind_update(float speed, float maxSpeed, out float vol, out float pitch);

	[DllImport("audio_curve", CallingConvention = CallingConvention.Cdecl)]
	private static extern int physics_puck_radius(int grounded, float predictedSpeed, float radius, float fixedDt, out float newRadius);

	private static bool useNative;

	public static void Init(AnimationCurve vol, AnimationCurve pitch)
	{
		try
		{
			audio_curve_init(Flatten(vol), KeyCount(vol), Flatten(pitch), KeyCount(pitch));
			useNative = true;
		}
		catch (DllNotFoundException)
		{
			useNative = false;
		}
		catch (EntryPointNotFoundException)
		{
			useNative = false;
		}
	}

	// 1 = changed, apply outputs; 0 = gated, skip; -1 = use managed path.
	public static int EvaluateWind(float speed, float maxSpeed, out float vol, out float pitch)
	{
		vol = 0f;
		pitch = 0f;
		if (!useNative)
		{
			return -1;
		}
		try
		{
			return audio_wind_update(speed, maxSpeed, out vol, out pitch);
		}
		catch
		{
			useNative = false;
			return -1;
		}
	}

	// 1 = changed, apply output; 0 = gated, skip; -1 = use managed path.
	public static int EvaluateRadius(bool grounded, float predictedSpeed, float radius, float fixedDt, out float newRadius)
	{
		newRadius = radius;
		if (!useNative)
		{
			return -1;
		}
		try
		{
			return physics_puck_radius(grounded ? 1 : 0, predictedSpeed, radius, fixedDt, out newRadius);
		}
		catch
		{
			useNative = false;
			return -1;
		}
	}

	private static int KeyCount(AnimationCurve curve)
	{
		if (curve == null || curve.keys == null)
		{
			return 0;
		}
		return curve.keys.Length;
	}

	private static float[] Flatten(AnimationCurve curve)
	{
		if (curve == null || curve.keys == null)
		{
			return new float[0];
		}
		Keyframe[] keys = curve.keys;
		float[] flat = new float[keys.Length * 4];
		for (int i = 0; i < keys.Length; i++)
		{
			flat[i * 4] = keys[i].time;
			flat[i * 4 + 1] = keys[i].value;
			flat[i * 4 + 2] = keys[i].inTangent;
			flat[i * 4 + 3] = keys[i].outTangent;
		}
		return flat;
	}
}
