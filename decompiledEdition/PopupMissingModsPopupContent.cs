using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UI;
using UnityEngine.UIElements;

public class PopupMissingModsPopupContent : BasePopupContent
{
	private static readonly Logger Logger = new Logger("PopupMissingModsPopupContent");

	private string text;

	private string notice;

	private VisualTreeAsset modPreviewAsset;

	private Label textLabel;

	private Label noticeLabel;

	private VisualElement missingModsList;

	private SteamWorkshopItem[] steamWorkshopItems;

	private Dictionary<SteamWorkshopItem, TemplateContainer> steamWorkshopItemTemplateContainerMap = new Dictionary<SteamWorkshopItem, TemplateContainer>();

	public string Text
	{
		get
		{
			return text;
		}
		set
		{
			if (!(text == value))
			{
				text = value;
				Update();
			}
		}
	}

	public string Notice
	{
		get
		{
			return notice;
		}
		set
		{
			if (!(notice == value))
			{
				notice = value;
				Update();
			}
		}
	}

	public PopupMissingModsPopupContent(VisualTreeAsset asset, VisualTreeAsset modPreviewAsset, string text, string notice, string[] missingModIds)
		: base(asset)
	{
		this.modPreviewAsset = modPreviewAsset;
		this.text = text;
		this.notice = notice;
		steamWorkshopItems = missingModIds.Select((string modId) => new SteamWorkshopItem(modId)).ToArray();
	}

	public override void Initialize()
	{
		base.Initialize();
		textLabel = VisualElement.Query<Label>("TextLabel");
		noticeLabel = VisualElement.Query<Label>("NoticeLabel");
		missingModsList = VisualElement.Query<VisualElement>("MissingModsList");
		SteamWorkshopItem[] array = steamWorkshopItems;
		foreach (SteamWorkshopItem steamWorkshopItem in array)
		{
			AddModPreview(steamWorkshopItem);
			SteamWorkshopItem steamWorkshopItem2 = steamWorkshopItem;
			steamWorkshopItem2.StateChanged = (Action<SteamWorkshopItemState, SteamWorkshopItemState>)Delegate.Combine(steamWorkshopItem2.StateChanged, (Action<SteamWorkshopItemState, SteamWorkshopItemState>)((SteamWorkshopItemState oldState, SteamWorkshopItemState newState) =>
			{
				OnSteamWorkshopItemStateChanged(steamWorkshopItem.Id, oldState, newState);
			}));
			SteamWorkshopItem steamWorkshopItem3 = steamWorkshopItem;
			steamWorkshopItem3.DetailsStateChanged = (Action<SteamWorkshopItemDetailsState, SteamWorkshopItemDetailsState>)Delegate.Combine(steamWorkshopItem3.DetailsStateChanged, (Action<SteamWorkshopItemDetailsState, SteamWorkshopItemDetailsState>)((SteamWorkshopItemDetailsState oldDetailsState, SteamWorkshopItemDetailsState newDetailsState) =>
			{
				OnSteamWorkshopItemDetailsStateChanged(steamWorkshopItem.Id, oldDetailsState, newDetailsState);
			}));
			steamWorkshopItem.Initialize();
		}
		Update();
	}

	internal override void Update()
	{
		base.Update();
		if (textLabel != null)
		{
			textLabel.text = Text;
		}
		if (noticeLabel != null)
		{
			noticeLabel.text = Notice;
		}
	}

	public override void Dispose()
	{
		base.Dispose();
		SteamWorkshopItem[] array = steamWorkshopItems;
		foreach (SteamWorkshopItem steamWorkshopItem in array)
		{
			steamWorkshopItem.Dispose();
			RemoveMod(steamWorkshopItem);
		}
	}

	private void AddModPreview(SteamWorkshopItem steamWorkshopItem)
	{
		TemplateContainer templateContainer = modPreviewAsset.Instantiate();
		ModPreview uiModPreview = templateContainer.Query<ModPreview>();
		ModPreview modPreview = uiModPreview;
		modPreview.Ready = (Action)Delegate.Combine(modPreview.Ready, (Action)(() =>
		{
			Link link = uiModPreview.Link;
			link.Clicked = (Action)Delegate.Combine(link.Clicked, (Action)(() =>
			{
				OnModLinkClicked(steamWorkshopItem);
			}));
			UpdateMod(steamWorkshopItem);
		}));
		steamWorkshopItemTemplateContainerMap.Add(steamWorkshopItem, templateContainer);
		missingModsList.Add(templateContainer);
	}

	private void UpdateMod(SteamWorkshopItem steamWorkshopItem)
	{
		if (steamWorkshopItemTemplateContainerMap.ContainsKey(steamWorkshopItem))
		{
			ModPreview modPreview = steamWorkshopItemTemplateContainerMap[steamWorkshopItem].Query<ModPreview>();
			modPreview.IsStatisticsVisible = steamWorkshopItem.Details != null;
			modPreview.Subscriptions = steamWorkshopItem.Details?.Subscriptions ?? 0;
			modPreview.Upvotes = steamWorkshopItem.Details?.Upvotes ?? 0;
			modPreview.Downvotes = steamWorkshopItem.Details?.Downvotes ?? 0;
			modPreview.Link.Text = steamWorkshopItem.Details?.Title ?? steamWorkshopItem.Id.ToString();
		}
	}

	private void RemoveMod(SteamWorkshopItem steamWorkshopItem)
	{
		if (steamWorkshopItemTemplateContainerMap.ContainsKey(steamWorkshopItem))
		{
			TemplateContainer element = steamWorkshopItemTemplateContainerMap[steamWorkshopItem];
			missingModsList.Remove(element);
			steamWorkshopItemTemplateContainerMap.Remove(steamWorkshopItem);
		}
	}

	private void OnSteamWorkshopItemStateChanged(string id, SteamWorkshopItemState oldState, SteamWorkshopItemState newState)
	{
		SteamWorkshopItem steamWorkshopItem = steamWorkshopItems.FirstOrDefault((SteamWorkshopItem item) => item.Id == id);
		if (steamWorkshopItem != null)
		{
			UpdateMod(steamWorkshopItem);
		}
	}

	private void OnSteamWorkshopItemDetailsStateChanged(string id, SteamWorkshopItemDetailsState oldDetailsState, SteamWorkshopItemDetailsState newDetailsState)
	{
		SteamWorkshopItem steamWorkshopItem = steamWorkshopItems.FirstOrDefault((SteamWorkshopItem item) => item.Id == id);
		if (steamWorkshopItem != null)
		{
			UpdateMod(steamWorkshopItem);
		}
	}

	private void OnSteamWorkshopItemPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		SteamWorkshopItem steamWorkshopItem = (SteamWorkshopItem)sender;
		UpdateMod(steamWorkshopItem);
	}

	private void OnModLinkClicked(SteamWorkshopItem steamWorkshopItem)
	{
		EventManager.TriggerEvent("Event_OnModPreviewLinkClicked", new Dictionary<string, object> { { "id", steamWorkshopItem.Id } });
	}
}
