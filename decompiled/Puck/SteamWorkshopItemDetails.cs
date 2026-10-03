using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SteamWorkshopItemDetails
{
	private static readonly Logger Logger = new Logger("SteamWorkshopItemDetails");

	private SteamWorkshopItemDetailsState state;

	public Action<SteamWorkshopItemDetailsState, SteamWorkshopItemDetailsState> StateChanged;

	public SteamWorkshopItemDetailsState State
	{
		get
		{
			return state;
		}
		set
		{
			if (!state.Equals(value))
			{
				SteamWorkshopItemDetailsState oldState = state;
				state = value;
				OnStateChanged(oldState, state);
			}
		}
	}

	public string Title => State.Title;

	public string Description => State.Description;

	public string PreviewUrl => State.PreviewUrl;

	public int Subscriptions => State.Subscriptions;

	public int Upvotes => State.Upvotes;

	public int Downvotes => State.Downvotes;

	public string Metadata => State.Metadata;

	public Texture2D PreviewTexture => State.PreviewTexture;

	public SteamWorkshopItemDetails(string title, string description, string previewUrl, int subscriptions, int upvotes, int downvotes, string metadata)
	{
		state = new SteamWorkshopItemDetailsState
		{
			Title = title,
			Description = description,
			PreviewUrl = previewUrl,
			Subscriptions = subscriptions,
			Upvotes = upvotes,
			Downvotes = downvotes,
			Metadata = metadata
		};
	}

	public void Initialize()
	{
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			MonoBehaviourSingleton<ThreadManager>.Instance.Enqueue(DownloadPreviewTexture());
		}
	}

	public void Dispose()
	{
		StateChanged = null;
	}

	public void SetState(Dictionary<string, object> updates)
	{
		SteamWorkshopItemDetailsState steamWorkshopItemDetailsState = new SteamWorkshopItemDetailsState
		{
			Title = (updates.ContainsKey("title") ? ((string)updates["title"]) : State.Title),
			Description = (updates.ContainsKey("description") ? ((string)updates["description"]) : State.Description),
			PreviewUrl = (updates.ContainsKey("previewUrl") ? ((string)updates["previewUrl"]) : State.PreviewUrl),
			Subscriptions = (updates.ContainsKey("subscriptions") ? ((int)updates["subscriptions"]) : State.Subscriptions),
			Upvotes = (updates.ContainsKey("upvotes") ? ((int)updates["upvotes"]) : State.Upvotes),
			Downvotes = (updates.ContainsKey("downvotes") ? ((int)updates["downvotes"]) : State.Downvotes),
			Metadata = (updates.ContainsKey("metadata") ? ((string)updates["metadata"]) : State.Metadata),
			PreviewTexture = (updates.ContainsKey("previewTexture") ? ((Texture2D)updates["previewTexture"]) : State.PreviewTexture)
		};
		State = steamWorkshopItemDetailsState;
	}

	private IEnumerator DownloadPreviewTexture()
	{
		Logger.Info("Downloading preview texture from " + State.PreviewUrl);
		using UnityWebRequest request = UnityWebRequestTexture.GetTexture(State.PreviewUrl);
		yield return request.SendWebRequest();
		if (request.result != UnityWebRequest.Result.Success)
		{
			Logger.Error("Failed to download preview texture from " + State.PreviewUrl + ": " + request.error);
			yield break;
		}
		SetState(new Dictionary<string, object> { 
		{
			"previewTexture",
			DownloadHandlerTexture.GetContent(request)
		} });
	}

	private void OnStateChanged(SteamWorkshopItemDetailsState oldState, SteamWorkshopItemDetailsState newState)
	{
		StateChanged?.Invoke(oldState, newState);
	}
}
