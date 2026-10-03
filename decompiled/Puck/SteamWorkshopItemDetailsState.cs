using System;
using UnityEngine;

public class SteamWorkshopItemDetailsState
{
	public string Title;

	public string Description;

	public string PreviewUrl;

	public int Subscriptions;

	public int Upvotes;

	public int Downvotes;

	public string Metadata;

	public Texture2D PreviewTexture;

	public bool Equals(SteamWorkshopItemDetailsState other)
	{
		if (Title == other.Title && Description == other.Description && PreviewUrl == other.PreviewUrl && Subscriptions == other.Subscriptions && Upvotes == other.Upvotes && Downvotes == other.Downvotes && Metadata == other.Metadata)
		{
			return PreviewTexture == other.PreviewTexture;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is SteamWorkshopItemDetailsState other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Title, Description, PreviewUrl, Subscriptions, Upvotes, Downvotes, Metadata, PreviewTexture);
	}

	public override string ToString()
	{
		return $"Title={Title}, Description={Description}, PreviewUrl={PreviewUrl}, Subscriptions={Subscriptions}, Upvotes={Upvotes}, Downvotes={Downvotes}, Metadata={Metadata}, PreviewTexture={PreviewTexture}";
	}
}
