using System;
using UnityEngine;

public readonly struct Logger
{
	private readonly string prefix;

	private static string GetColor(LogType type)
	{
		if (Application.isEditor)
		{
			return "<color=white>";
		}
		if (!TerminalUtils.SupportsAnsiColor)
		{
			return "";
		}
		return "\u001b[37m";
	}

	private static string GetReset()
	{
		if (Application.isEditor)
		{
			return "</color>";
		}
		if (!TerminalUtils.SupportsAnsiColor)
		{
			return "";
		}
		return "\u001b[0m";
	}

	public Logger(string tag)
	{
		string color = GetColor(LogType.Log);
		string reset = GetReset();
		prefix = color + "[" + tag + "]" + reset + " ";
	}

	[HideInCallstack]
	public void Info(string message)
	{
		Debug.Log(prefix + message);
	}

	[HideInCallstack]
	public void Warning(string message)
	{
		Debug.LogWarning(prefix + message);
	}

	[HideInCallstack]
	public void Error(string message)
	{
		Debug.LogError(prefix + message);
	}

	[HideInCallstack]
	public void Exception(Exception exception)
	{
		Debug.LogException(exception);
	}
}
