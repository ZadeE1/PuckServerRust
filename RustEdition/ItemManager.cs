using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using UnityEngine;

public static class ItemManager
{
	private static readonly Logger Logger;

	[CompilerGenerated]
	private static List<Item> Items__BackingField;

	public static List<Item> Items
	{
		[CompilerGenerated]
		get
		{
			return Items__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			Items__BackingField = value;
		}
	}

	static ItemManager()
	{
		Logger = new Logger("ItemManager");
		Items__BackingField = new List<Item>();
		LoadItems();
	}

	public static void Initialize()
	{
		ItemManagerController.Initialize();
	}

	public static void Dispose()
	{
		ItemManagerController.Dispose();
	}

	public static Item GetItemById(int id)
	{
		return Items.Find((Item item) => item.id == id);
	}

	public static bool IsItemOwned(int itemId, PlayerData playerData)
	{
		Item itemById = GetItemById(itemId);
		if (itemById == null)
		{
			return true;
		}
		if (itemById.price == 0)
		{
			return true;
		}
		if (playerData?.items != null)
		{
			return playerData.items.Any((PlayerItem ownedItem) => ownedItem.itemId == itemId);
		}
		return false;
	}

	public static int ValidateEquippedItem(int itemId, int defaultItemId, Func<Item, bool> isValidForSlot, PlayerData playerData)
	{
		if (itemId == -1)
		{
			if (defaultItemId == -1)
			{
				return -1;
			}
		}
		else
		{
			Item itemById = GetItemById(itemId);
			if (itemById != null && isValidForSlot(itemById) && IsItemOwned(itemId, playerData))
			{
				return itemId;
			}
		}
		Logger.Warning($"Player {playerData?.steamId} equipped invalid or unowned item {itemId}; resetting to default {defaultItemId}");
		return defaultItemId;
	}

	public static List<Item> GetItemsByCategories(string[] categories)
	{
		return Items.FindAll((Item item) => Array.Exists(item.categories, (string itemCategory) => Array.IndexOf(categories, itemCategory) >= 0));
	}

	private static void LoadItems()
	{
		try
		{
			Items = JsonSerializer.Deserialize<List<Item>>(Resources.Load<TextAsset>("items").text);
			Logger.Info($"Loaded {Items.Count} items");
		}
		catch (Exception ex)
		{
			Logger.Error("Error loading items asset: " + ex.Message);
		}
	}
}
