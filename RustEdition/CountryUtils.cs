using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.Json;
using UnityEngine;

public static class CountryUtils
{
	private static readonly Logger Logger;

	[CompilerGenerated]
	private static List<Country> Countries__BackingField;

	public static List<Country> Countries
	{
		[CompilerGenerated]
		get
		{
			return Countries__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Countries__BackingField = value;
		}
	}

	static CountryUtils()
	{
		Logger = new Logger("CountryUtils");
		Countries__BackingField = new List<Country>();
		LoadCountries();
	}

	public static Country GetCountryByCode(string code)
	{
		return Countries.Find((Country country) => country.code.Equals(code, StringComparison.OrdinalIgnoreCase));
	}

	public static Country GetCountryByName(string name)
	{
		return Countries.Find((Country country) => country.name.Equals(name, StringComparison.OrdinalIgnoreCase));
	}

	private static void LoadCountries()
	{
		try
		{
			Countries = JsonSerializer.Deserialize<List<Country>>(Resources.Load<TextAsset>("countries").text);
			Logger.Info($"Loaded {Countries.Count} countries");
		}
		catch (Exception ex)
		{
			Logger.Error("Error loading countries asset: " + ex.Message);
		}
	}
}
