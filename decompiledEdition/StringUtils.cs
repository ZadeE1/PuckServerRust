using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using UnityEngine;

public static class StringUtils
{
	private static readonly Logger Logger;

	private static Regex profanityRegex;

	static StringUtils()
	{
		Logger = new Logger("StringUtils");
		LoadProfanityWords();
	}

	private static void LoadProfanityWords()
	{
		try
		{
			string[] array = JsonSerializer.Deserialize<string[]>(Resources.Load<TextAsset>("profanity_words").text);
			string[] value = (from w in array
				where !string.IsNullOrWhiteSpace(w)
				orderby w.Length descending
				select Regex.Escape(w)).ToArray();
			profanityRegex = new Regex("(?<![a-zA-Z])(" + string.Join("|", value) + ")(?![a-zA-Z])", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			Logger.Info($"Loaded {array.Length} profanity words and compiled regex");
		}
		catch (Exception ex)
		{
			Logger.Error("Error loading profanity words asset: " + ex.Message);
		}
	}

	public static string FilterStringNotLetters(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}
		StringBuilder stringBuilder = new StringBuilder(text.Length);
		foreach (char c in text)
		{
			if (char.IsLetter(c))
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	public static string FilterStringSpecialCharacters(string text, string[] characterWhitelist = null, string[] characterBlacklist = null)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}
		StringBuilder stringBuilder = new StringBuilder(text.Length);
		TextElementEnumerator textElementEnumerator = StringInfo.GetTextElementEnumerator(text);
		while (textElementEnumerator.MoveNext())
		{
			string textElement = textElementEnumerator.GetTextElement();
			if (characterBlacklist == null || !Enumerable.Contains(characterBlacklist, textElement))
			{
				UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(textElement, 0);
				if (textElement.All(char.IsLetterOrDigit) || textElement.All(char.IsWhiteSpace) || unicodeCategory == UnicodeCategory.ConnectorPunctuation || unicodeCategory == UnicodeCategory.DashPunctuation || unicodeCategory == UnicodeCategory.OpenPunctuation || unicodeCategory == UnicodeCategory.ClosePunctuation || unicodeCategory == UnicodeCategory.InitialQuotePunctuation || unicodeCategory == UnicodeCategory.FinalQuotePunctuation || unicodeCategory == UnicodeCategory.OtherPunctuation || unicodeCategory == UnicodeCategory.MathSymbol || unicodeCategory == UnicodeCategory.CurrencySymbol || (characterWhitelist != null && Enumerable.Contains(characterWhitelist, textElement)))
				{
					stringBuilder.Append(textElement);
				}
			}
		}
		return stringBuilder.ToString();
	}

	public static string FilterStringProfanity(string text, bool replaceWithStars = false)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}
		if (profanityRegex == null)
		{
			return text;
		}
		string text2 = ((!replaceWithStars) ? profanityRegex.Replace(text, string.Empty) : profanityRegex.Replace(text, (Match match) => new string('*', match.Length)));
		if (!replaceWithStars)
		{
			text2 = Regex.Replace(text2, "\\s+", " ").Trim();
		}
		return text2;
	}

	public static string FilterStringRichText(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}
		string[] richTextTags = new string[37]
		{
			"a", "align", "allcaps", "alpha", "b", "br", "color", "cspace", "font", "font-weight",
			"gradient", "i", "indent", "line-height", "line-indent", "link", "lowercase", "margin", "margin-left", "margin-right",
			"mark", "mspace", "nobr", "noparse", "pos", "s", "size", "smallcaps", "space", "sprite",
			"style", "sub", "sup", "u", "uppercase", "voffset", "width"
		};
		string pattern = "</?(#[0-9a-fA-F]*|\\w+(?:-\\w+)?)(?:[\\s=][^>]*)?>";
		MatchEvaluator evaluator = (Match match) =>
		{
			string text4 = match.Groups[1].Value.ToLower();
			if (text4.StartsWith("#"))
			{
				return string.Empty;
			}
			return Enumerable.Contains(richTextTags, text4) ? string.Empty : match.Value;
		};
		string text2 = text;
		string text3;
		do
		{
			text3 = text2;
			text2 = Regex.Replace(text3, pattern, evaluator, RegexOptions.IgnoreCase);
		}
		while (text2 != text3);
		return text2;
	}

	public static string WrapInColor(string text, string color)
	{
		if (string.IsNullOrEmpty(color))
		{
			return text;
		}
		return "<color=" + color + ">" + text + "</color>";
	}

	public static List<string> ChunkLinesByByteBudget(IReadOnlyList<string> lines, int maxBytes)
	{
		List<string> list = new List<string>();
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		foreach (string line in lines)
		{
			int byteCount = Encoding.UTF8.GetByteCount(line);
			int num2 = ((stringBuilder.Length > 0) ? 1 : 0);
			if (stringBuilder.Length > 0 && num + num2 + byteCount > maxBytes)
			{
				list.Add(stringBuilder.ToString());
				stringBuilder.Clear();
				num = 0;
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append('\n');
				num++;
			}
			stringBuilder.Append(line);
			num += byteCount;
		}
		if (stringBuilder.Length > 0)
		{
			list.Add(stringBuilder.ToString());
		}
		return list;
	}

	public static string WrapInTeamColor(string text, PlayerTeam team)
	{
		string text2 = team switch
		{
			PlayerTeam.Blue => "#3b82f6", 
			PlayerTeam.Red => "#d13333", 
			_ => "#404040", 
		};
		if (text2 == null)
		{
			return text;
		}
		return "<color=" + text2 + ">" + text + "</color>";
	}
}
