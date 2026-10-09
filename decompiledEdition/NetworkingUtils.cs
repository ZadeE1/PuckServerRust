using Unity.Netcode;
using UnityEngine;

public static class NetworkingUtils
{
	public static Player GetPlayerFromNetworkObjectReference(NetworkObjectReference reference)
	{
		if (reference.TryGet(out var networkObject))
		{
			return networkObject.GetComponent<Player>();
		}
		return null;
	}

	public static PlayerPosition GetPlayerPositionFromNetworkObjectReference(NetworkObjectReference reference)
	{
		if (reference.TryGet(out var networkObject))
		{
			return networkObject.GetComponent<PlayerPosition>();
		}
		return null;
	}

	public static Puck GetPuckFromNetworkObjectReference(NetworkObjectReference reference)
	{
		if (reference.TryGet(out var networkObject))
		{
			return networkObject.GetComponent<Puck>();
		}
		return null;
	}

	public static byte CompressFloatToByte(float value, float minValue, float maxValue)
	{
		int num = -128;
		int num2 = 127;
		float t = Mathf.InverseLerp(minValue, maxValue, value);
		return (byte)(sbyte)Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(num, num2, t)), num, num2);
	}

	public static short CompressFloatToShort(float value, float minValue, float maxValue)
	{
		int num = -32768;
		int num2 = 32767;
		float t = Mathf.InverseLerp(minValue, maxValue, value);
		return (short)Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(num, num2, t)), num, num2);
	}

	public static float DecompressByteToFloat(byte compressedValue, float minValue, float maxValue)
	{
		int num = 127;
		float t = Mathf.InverseLerp(-128f, num, (sbyte)compressedValue);
		return Mathf.Lerp(minValue, maxValue, t);
	}

	public static float DecompressShortToFloat(short compressedValue, float minValue, float maxValue)
	{
		int num = 32767;
		float t = Mathf.InverseLerp(-32768f, num, compressedValue);
		return Mathf.Lerp(minValue, maxValue, t);
	}

	public static void StepTime(double deltaTime, ref double localTimeline, double localTimescale)
	{
		localTimeline += deltaTime * localTimescale;
	}
}
