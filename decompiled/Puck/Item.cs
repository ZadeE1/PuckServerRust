using System.Linq;
using System.Text.Json.Serialization;

public class Item
{
	public int id { get; set; }

	public string name { get; set; }

	public string description { get; set; }

	public string[] categories { get; set; } = new string[0];

	public int price { get; set; }

	public string code { get; set; }

	[JsonIgnore]
	public bool IsFlag => categories.Contains("flag");

	[JsonIgnore]
	public bool IsHeadgear => categories.Contains("headgear");

	[JsonIgnore]
	public bool IsMustache => categories.Contains("mustache");

	[JsonIgnore]
	public bool IsBeard => categories.Contains("beard");

	[JsonIgnore]
	public bool IsJersey => categories.Contains("jersey");

	[JsonIgnore]
	public bool IsStickSkin => categories.Contains("stickSkin");

	[JsonIgnore]
	public bool IsStickShaftTape => categories.Contains("stickShaftTape");

	[JsonIgnore]
	public bool IsStickBladeTape => categories.Contains("stickBladeTape");

	[JsonIgnore]
	public bool HasRolePostfix
	{
		get
		{
			if (!categories.Contains("attacker"))
			{
				return categories.Contains("goalie");
			}
			return true;
		}
	}

	[JsonIgnore]
	public bool IsAttackerItem
	{
		get
		{
			if (HasRolePostfix)
			{
				return categories.Contains("attacker");
			}
			return true;
		}
	}

	[JsonIgnore]
	public bool IsGoalieItem
	{
		get
		{
			if (HasRolePostfix)
			{
				return categories.Contains("goalie");
			}
			return true;
		}
	}

	[JsonIgnore]
	public bool IsPurchased
	{
		get
		{
			if (BackendManager.PlayerState.PlayerData != null)
			{
				return BackendManager.PlayerState.PlayerData.items.Any((PlayerItem item) => item.itemId == id);
			}
			return false;
		}
	}

	[JsonIgnore]
	public bool IsOwned
	{
		get
		{
			if (price != 0)
			{
				return IsPurchased;
			}
			return true;
		}
	}

	[JsonIgnore]
	public bool IsUnlisted => categories.Contains("unlisted");

	[JsonIgnore]
	public string EditorDisplayName => $"{name} ({id})";
}
