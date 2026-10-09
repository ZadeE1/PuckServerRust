using UnityEngine;

public class VoiceLevelMeter
{
	private float level;

	private float target;

	private float timeSinceVoice;

	public float Level => level;

	public void Reset()
	{
		level = 0f;
		target = 0f;
		timeSinceVoice = 0f;
	}

	public float Update(float rawLevel, bool hasVoice, float deltaTime)
	{
		if (hasVoice)
		{
			target = rawLevel;
			timeSinceVoice = 0f;
		}
		else
		{
			timeSinceVoice += deltaTime;
			if (timeSinceVoice >= 0.1f)
			{
				target = 0f;
			}
		}
		float t = 1f - Mathf.Exp(-15f * deltaTime);
		level = Mathf.Lerp(level, target, t);
		return level;
	}
}
