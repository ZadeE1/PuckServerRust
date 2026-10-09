using Steamworks;
using UnityEngine;

public static class VoiceUtils
{
	public static float ComputeLevel(byte[] voiceData, uint bytesRead, uint sampleRate, byte[] decompressBuffer)
	{
		if (sampleRate == 0 || decompressBuffer == null)
		{
			return 0f;
		}
		SteamUser.DecompressVoice(voiceData, bytesRead, decompressBuffer, (uint)decompressBuffer.Length, out var nBytesWritten, sampleRate);
		double num = 0.0;
		int num2 = 0;
		for (int i = 0; i + 1 < nBytesWritten; i += 2)
		{
			float num3 = (float)(short)(decompressBuffer[i] | (decompressBuffer[i + 1] << 8)) / 32768f;
			num += (double)num3 * (double)num3;
			num2++;
		}
		if (num2 == 0)
		{
			return 0f;
		}
		return Mathf.Clamp01(Mathf.Sqrt((float)(num / (double)num2)) * 14f);
	}
}
