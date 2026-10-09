using System;
using DG.Tweening;
using UnityEngine;

public static class SaveManager
{
	private static Tween saveDebounceTween;

	public static void Initialize()
	{
	}

	public static void Dispose()
	{
		saveDebounceTween?.Kill();
	}

	public static void SetBool(string key, bool value)
	{
		PlayerPrefs.SetInt(key, value ? 1 : 0);
		Save();
	}

	public static bool GetBool(string key, bool defaultValue)
	{
		int defaultValue2 = (defaultValue ? 1 : 0);
		return PlayerPrefs.GetInt(key, defaultValue2) == 1;
	}

	public static void SetInt(string key, int value)
	{
		PlayerPrefs.SetInt(key, value);
		Save();
	}

	public static int GetInt(string key, int defaultValue)
	{
		return PlayerPrefs.GetInt(key, defaultValue);
	}

	public static void SetFloat(string key, float value)
	{
		PlayerPrefs.SetFloat(key, value);
		Save();
	}

	public static float GetFloat(string key, float defaultValue)
	{
		return PlayerPrefs.GetFloat(key, defaultValue);
	}

	public static void SetString(string key, string value)
	{
		PlayerPrefs.SetString(key, value);
		Save();
	}

	public static string GetString(string key, string defaultValue)
	{
		return PlayerPrefs.GetString(key, defaultValue);
	}

	public static void SetEnum<T>(string key, T value) where T : Enum
	{
		PlayerPrefs.SetInt(key, Convert.ToInt32(value));
		Save();
	}

	public static T GetEnum<T>(string key, T defaultValue) where T : Enum
	{
		int defaultValue2 = Convert.ToInt32(defaultValue);
		int value = PlayerPrefs.GetInt(key, defaultValue2);
		return (T)Enum.ToObject(typeof(T), value);
	}

	private static void Save()
	{
		saveDebounceTween?.Kill();
		saveDebounceTween = DOVirtual.DelayedCall(0f, () =>
		{
			PlayerPrefs.Save();
		});
	}
}
