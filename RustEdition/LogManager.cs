using System;
using System.IO;
using System.Text;
using UnityEngine;

public static class LogManager
{
	private class LogHandler : ILogHandler
	{
		private ILogHandler baseLogHandler;

		public LogHandler(ILogHandler blh)
		{
			baseLogHandler = blh;
		}

		private static string GetColor(LogType type)
		{
			if (Application.isEditor)
			{
				return type switch
				{
					LogType.Warning => "<color=yellow>", 
					LogType.Error => "<color=red>", 
					LogType.Assert => "<color=magenta>", 
					LogType.Exception => "<color=red>", 
					_ => "<color=grey>", 
				};
			}
			if (!TerminalUtils.SupportsAnsiColor)
			{
				return "";
			}
			return type switch
			{
				LogType.Warning => "\u001b[33m", 
				LogType.Error => "\u001b[31m", 
				LogType.Assert => "\u001b[35m", 
				LogType.Exception => "\u001b[91m", 
				_ => "\u001b[90m", 
			};
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

		[HideInCallstack]
		public void LogFormat(LogType type, UnityEngine.Object context, string format, params object[] args)
		{
			string text = FormatPrefix(type);
			WriteToLogFile(text, (args.Length != 0) ? string.Format(format, args) : format);
			baseLogHandler.LogFormat(type, context, GetColor(type) + text + GetReset() + " " + format, args);
		}

		public void LogException(Exception exception, UnityEngine.Object context)
		{
			WriteToLogFile(FormatPrefix(LogType.Exception), exception.ToString());
			baseLogHandler.LogException(exception, context);
		}
	}

	public const string DefaultLogFilePath = "./Logs/Puck.log";

	private static readonly Logger Logger = new Logger("LogManager");

	private static string logFileRawPath = Utils.GetCommandLineArgument("--logPath") ?? "./Logs/Puck.log";

	private static string logFilePath = Path.GetFullPath(logFileRawPath);

	private static string logDirectoryPath = Path.GetDirectoryName(logFilePath);

	private static StreamWriter streamWriter;

	private static readonly object streamWriterLock = new object();

	public static void Initialize()
	{
		TerminalUtils.TryEnableUtf8Output();
		Debug.unityLogger.logHandler = new LogHandler(Debug.unityLogger.logHandler);
		if (!Directory.Exists(logDirectoryPath))
		{
			Directory.CreateDirectory(logDirectoryPath);
		}
		try
		{
			streamWriter = new StreamWriter(logFilePath, append: false, Encoding.UTF8);
			streamWriter.AutoFlush = true;
		}
		catch (Exception ex)
		{
			Logger.Error("Failed to initialize StreamWriter: " + ex.Message);
			streamWriter = null;
		}
	}

	public static void Dispose()
	{
		lock (streamWriterLock)
		{
			if (streamWriter != null)
			{
				streamWriter.Flush();
				streamWriter.Close();
				streamWriter = null;
			}
		}
	}

	private static string ParseLogType(LogType type)
	{
		return type switch
		{
			LogType.Error => "ERROR", 
			LogType.Warning => "WARNING", 
			LogType.Log => "INFO", 
			LogType.Assert => "ASSERT", 
			LogType.Exception => "EXCEPTION", 
			_ => "UNKNOWN", 
		};
	}

	private static string FormatPrefix(LogType type)
	{
		return $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [{ParseLogType(type)}]";
	}

	private static void WriteToLogFile(string prefix, string message)
	{
		string value = TerminalUtils.StripColors(prefix + " " + message);
		lock (streamWriterLock)
		{
			if (streamWriter != null)
			{
				streamWriter.WriteLine(value);
			}
		}
	}
}
