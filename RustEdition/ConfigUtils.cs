using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class ConfigUtils
{
	private static readonly Logger Logger = new Logger("ConfigUtils");

	public static T LoadConfigFromFile<T>(string filePath, bool createIfNotExists = true) where T : class, new()
	{
		if (string.IsNullOrEmpty(filePath))
		{
			return null;
		}
		if (!File.Exists(filePath) & createIfNotExists)
		{
			SaveConfigToFile(filePath, new T());
		}
		if (!TryReadFile(filePath, out var contents))
		{
			return null;
		}
		return LoadConfigFromSerializedString<T>(contents, filePath);
	}

	public static T LoadConfigFromSerializedString<T>(string serializedString, string source = "serialized string") where T : class, new()
	{
		if (string.IsNullOrEmpty(serializedString))
		{
			return null;
		}
		if (!TryDeserialize<T>(serializedString, source, out var result))
		{
			return null;
		}
		return result;
	}

	public static bool TryLoadFromFile<T>(string filePath, out T result)
	{
		result = default;
		if (!TryReadFile(filePath, out var contents))
		{
			return false;
		}
		return TryDeserialize<T>(contents, filePath, out result);
	}

	public static bool TryDeserialize<T>(string serializedString, string source, out T result)
	{
		result = default;
		try
		{
			result = JsonSerializer.Deserialize<T>(serializedString);
			return true;
		}
		catch (JsonException exception)
		{
			Logger.Error("Malformed JSON in " + source + ": " + DescribeJsonError(exception));
			return false;
		}
		catch (Exception ex)
		{
			Logger.Error("Error loading config from " + source + ": " + ex.Message);
			return false;
		}
	}

	private static bool TryReadFile(string filePath, out string contents)
	{
		contents = null;
		try
		{
			contents = File.ReadAllText(filePath);
			return true;
		}
		catch (Exception ex)
		{
			Logger.Error("Could not read config file " + filePath + ": " + ex.Message);
			return false;
		}
	}

	private static string DescribeJsonError(JsonException exception)
	{
		string text = "";
		if (exception.LineNumber.HasValue)
		{
			text = $"line {exception.LineNumber + 1}";
			if (exception.BytePositionInLine.HasValue)
			{
				text += $", position {exception.BytePositionInLine + 1}";
			}
		}
		if (!string.IsNullOrEmpty(exception.Path) && exception.Path != "$")
		{
			text += (string.IsNullOrEmpty(text) ? ("at " + exception.Path) : (" (at " + exception.Path + ")"));
		}
		if (!string.IsNullOrEmpty(text))
		{
			return text + ": " + exception.Message;
		}
		return exception.Message;
	}

	public static void SaveConfigToFile<T>(string filePath, T config)
	{
		try
		{
			JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
			{
				WriteIndented = true
			};
			jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
			string text = JsonSerializer.Serialize(config, jsonSerializerOptions);
			Logger.Info("Serialized config " + typeof(T).Name + ": " + text);
			File.WriteAllText(filePath, text);
		}
		catch (Exception ex)
		{
			Logger.Error("Error saving config to " + filePath + ": " + ex.Message);
		}
	}
}
