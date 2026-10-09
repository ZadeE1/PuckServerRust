using System;
using System.Collections.Generic;

public class SteamWorkshopItem
{
	public readonly string Id;

	private SteamWorkshopItemState state;

	public Action<SteamWorkshopItemState, SteamWorkshopItemState> StateChanged;

	public Action<SteamWorkshopItemDetailsState, SteamWorkshopItemDetailsState> DetailsStateChanged;

	public SteamWorkshopItemState State
	{
		get
		{
			return state;
		}
		set
		{
			if (!state.Equals(value))
			{
				SteamWorkshopItemState oldState = state;
				state = value;
				OnStateChanged(oldState, state);
			}
		}
	}

	public string Path => State.Path;

	public SteamWorkshopItemDetails Details => State.Details;

	public SteamWorkshopItemPhase Phase => State.Phase;

	public SteamWorkshopItem(string id, string path = null)
	{
		Id = id;
		state = new SteamWorkshopItemState
		{
			Path = path
		};
	}

	public virtual void Initialize()
	{
		EventManager.AddEventListener("Event_OnSteamWorkshopItemDetails", Event_OnSteamWorkshopItemDetails);
		SteamWorkshopManager.GetItemDetails(Id);
	}

	public virtual void Dispose()
	{
		EventManager.RemoveEventListener("Event_OnSteamWorkshopItemDetails", Event_OnSteamWorkshopItemDetails);
		if (State.Details != null)
		{
			State.Details.Dispose();
		}
		StateChanged = null;
		DetailsStateChanged = null;
	}

	public void SetState(Dictionary<string, object> updates)
	{
		SteamWorkshopItemState steamWorkshopItemState = new SteamWorkshopItemState
		{
			Path = (updates.ContainsKey("path") ? ((string)updates["path"]) : State.Path),
			Details = (updates.ContainsKey("details") ? ((SteamWorkshopItemDetails)updates["details"]) : State.Details),
			Phase = (updates.ContainsKey("phase") ? ((SteamWorkshopItemPhase)updates["phase"]) : State.Phase)
		};
		State = steamWorkshopItemState;
	}

	private void Event_OnSteamWorkshopItemDetails(Dictionary<string, object> message)
	{
		string text = (string)message["id"];
		string text2 = (string)message["title"];
		string text3 = (string)message["description"];
		string text4 = (string)message["previewUrl"];
		int num = (int)message["subscriptions"];
		int num2 = (int)message["upvotes"];
		int num3 = (int)message["downvotes"];
		string text5 = (string)message["metadata"];
		if (!(Id != text))
		{
			if (state.Details == null)
			{
				SteamWorkshopItemDetails steamWorkshopItemDetails = new SteamWorkshopItemDetails(text2, text3, text4, num, num2, num3, text5);
				SetState(new Dictionary<string, object> { { "details", steamWorkshopItemDetails } });
				steamWorkshopItemDetails.StateChanged = (Action<SteamWorkshopItemDetailsState, SteamWorkshopItemDetailsState>)Delegate.Combine(steamWorkshopItemDetails.StateChanged, new Action<SteamWorkshopItemDetailsState, SteamWorkshopItemDetailsState>(OnDetailsStateChanged));
				steamWorkshopItemDetails.Initialize();
			}
			else
			{
				Details.SetState(new Dictionary<string, object>
				{
					{ "title", text2 },
					{ "description", text3 },
					{ "previewUrl", text4 },
					{ "subscriptions", num },
					{ "upvotes", num2 },
					{ "downvotes", num3 },
					{ "metadata", text5 }
				});
			}
		}
	}

	private void OnStateChanged(SteamWorkshopItemState oldState, SteamWorkshopItemState newState)
	{
		StateChanged?.Invoke(oldState, newState);
	}

	private void OnDetailsStateChanged(SteamWorkshopItemDetailsState oldDetailsState, SteamWorkshopItemDetailsState newDetailsState)
	{
		DetailsStateChanged?.Invoke(oldDetailsState, newDetailsState);
	}
}
