using System;

public class SteamWorkshopItemState
{
	public string Path;

	public SteamWorkshopItemDetails Details;

	public SteamWorkshopItemPhase Phase;

	public bool Equals(SteamWorkshopItemState other)
	{
		if (Path == other.Path && Details == other.Details)
		{
			return Phase == other.Phase;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is SteamWorkshopItemState other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Path, Details, Phase);
	}

	public override string ToString()
	{
		return $"Path={Path}, Details={Details}, Phase={Phase}";
	}
}
