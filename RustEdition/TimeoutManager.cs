using System.Collections.Generic;
using DG.Tweening;

public class TimeoutManager : MonoBehaviourSingleton<TimeoutManager>
{
	private Dictionary<string, Tween> steamIdTweenMap = new Dictionary<string, Tween>();

	public void Dispose()
	{
		foreach (Tween value in steamIdTweenMap.Values)
		{
			value.Kill();
		}
		steamIdTweenMap.Clear();
	}

	public void AddSteamIdTimeout(string steamId, float timeout)
	{
		if (!steamIdTweenMap.ContainsKey(steamId))
		{
			steamIdTweenMap[steamId] = DOVirtual.DelayedCall(timeout, () =>
			{
				RemoveSteamIdTimeout(steamId);
			});
		}
	}

	public void RemoveSteamIdTimeout(string steamId)
	{
		if (steamIdTweenMap.ContainsKey(steamId))
		{
			steamIdTweenMap[steamId].Kill();
			steamIdTweenMap.Remove(steamId);
		}
	}

	public bool IsSteamIdTimedOut(string steamId)
	{
		return steamIdTweenMap.ContainsKey(steamId);
	}
}
