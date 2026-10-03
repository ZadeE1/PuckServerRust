using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;

public static class TerminalUtils
{
	public static readonly bool SupportsAnsiColor = DetectAnsiSupport();

	private static readonly Regex AnsiColorRegex = new Regex("\\x1b\\[[0-9;]*m", RegexOptions.Compiled);

	private static readonly Regex RichTextColorRegex = new Regex("</?color(=[^>]*)?>", RegexOptions.Compiled);

	private static readonly Logger Log = new Logger("TerminalUtils");

	private const int STD_OUTPUT_HANDLE = -11;

	private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 4u;

	private const uint CP_UTF8 = 65001u;

	public static void TryEnableUtf8Output()
	{
		if (!IsWindowsPlatform())
		{
			return;
		}
		try
		{
			uint consoleOutputCP = GetConsoleOutputCP();
			if (consoleOutputCP != 65001)
			{
				if (SetConsoleOutputCP(65001u))
				{
					Log.Info($"Set console output code page to UTF-8 (was {consoleOutputCP})");
				}
				else
				{
					Log.Warning($"SetConsoleOutputCP(UTF-8) failed (code page stays {consoleOutputCP})");
				}
			}
		}
		catch (Exception ex)
		{
			Log.Warning("Failed to set UTF-8 console output: " + ex.GetType().Name + ": " + ex.Message);
		}
	}

	private static bool IsWindowsPlatform()
	{
		RuntimePlatform platform = Application.platform;
		if (platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor || platform == RuntimePlatform.WindowsServer)
		{
			return true;
		}
		return false;
	}

	public static string StripColors(string input)
	{
		if (string.IsNullOrEmpty(input))
		{
			return input;
		}
		return RichTextColorRegex.Replace(AnsiColorRegex.Replace(input, string.Empty), string.Empty);
	}

	private static bool DetectAnsiSupport()
	{
		try
		{
			string value = Utils.GetCommandLineArgument("--logColor") ?? Environment.GetEnvironmentVariable("PUCK_LOG_COLOR");
			if (!string.IsNullOrEmpty(value))
			{
				return !IsFalsey(value);
			}
			if (Environment.GetEnvironmentVariable("NO_COLOR") != null)
			{
				return false;
			}
			if (IsPipeBasedColorTerminal())
			{
				TryEnableWindowsVirtualTerminal();
				return true;
			}
			if (Console.IsOutputRedirected)
			{
				return false;
			}
			RuntimePlatform platform = Application.platform;
			if (platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor || platform == RuntimePlatform.WindowsServer)
			{
				return TryEnableWindowsVirtualTerminal();
			}
			string environmentVariable = Environment.GetEnvironmentVariable("TERM");
			return !string.IsNullOrEmpty(environmentVariable) && environmentVariable != "dumb";
		}
		catch
		{
			return false;
		}
	}

	private static bool IsPipeBasedColorTerminal()
	{
		if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MSYSTEM")))
		{
			return true;
		}
		if (string.Equals(Environment.GetEnvironmentVariable("ConEmuANSI"), "ON", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return false;
	}

	private static bool IsFalsey(string value)
	{
		string text = value.Trim().ToLowerInvariant();
		if (!(text == "0"))
		{
			return text == "false";
		}
		return true;
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern uint GetConsoleOutputCP();

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool SetConsoleOutputCP(uint wCodePageID);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr GetStdHandle(int nStdHandle);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

	private static bool TryEnableWindowsVirtualTerminal()
	{
		IntPtr stdHandle = GetStdHandle(-11);
		if (stdHandle == IntPtr.Zero || stdHandle == new IntPtr(-1))
		{
			return false;
		}
		if (!GetConsoleMode(stdHandle, out var lpMode))
		{
			return false;
		}
		if ((lpMode & 4) != 0)
		{
			return true;
		}
		return SetConsoleMode(stdHandle, lpMode | 4);
	}
}
