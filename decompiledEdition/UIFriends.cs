using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.UIElements;

public class UIFriends : UIView
{
	[Header("References")]
	public VisualTreeAsset friendAsset;

	private VisualElement friends;

	private VisualElement friendsList;

	private IconButton closeIconButton;

	private Dictionary<string, TemplateContainer> friendsMap = new Dictionary<string, TemplateContainer>();

	public void Initialize(VisualElement rootVisualElement)
	{
		View = rootVisualElement.Query<VisualElement>("FriendsView");
		friends = View.Query<VisualElement>("Friends");
		friendsList = friends.Query<VisualElement>("FriendsList");
		closeIconButton = friends.Query<TemplateContainer>("CloseIconButtonContainer").First().Query<IconButton>();
		closeIconButton.clicked += OnCloseIconButtonClicked;
		friendsList.Clear();
		friendsMap.Clear();
	}

	public void AddFriend(string steamId, string username, Texture2D avatar)
	{
		if (!friendsMap.ContainsKey(steamId))
		{
			TemplateContainer templateContainer = CreateFriend(steamId, username, avatar);
			friendsList.Add(templateContainer);
			friendsMap.Add(steamId, templateContainer);
			SortFriends();
		}
	}

	public void UpdateFriend(string steamId, string username, Texture2D texture)
	{
		if (friendsMap.ContainsKey(steamId))
		{
			Friend friend = friendsMap[steamId].Query<Friend>("Friend");
			friend.Texture = texture;
			friend.Username = username;
			friend.InviteButtonClicked = () =>
			{
				OnFriendInviteButtonClicked(steamId);
			};
			SortFriends();
		}
	}

	public void RemoveFriend(string steamId)
	{
		if (friendsMap.ContainsKey(steamId))
		{
			TemplateContainer element = friendsMap[steamId];
			friendsList.Remove(element);
			friendsMap.Remove(steamId);
		}
	}

	public bool IsFriendListed(string steamId)
	{
		return friendsMap.ContainsKey(steamId);
	}

	private TemplateContainer CreateFriend(string steamId, string username, Texture2D texture)
	{
		TemplateContainer templateContainer = friendAsset.Instantiate();
		Friend friend = templateContainer.Query<Friend>("Friend");
		friend.Texture = texture;
		friend.Username = username;
		friend.InviteButtonClicked = () =>
		{
			OnFriendInviteButtonClicked(steamId);
		};
		return templateContainer;
	}

	private void SortFriends()
	{
		friendsList.Sort((VisualElement a, VisualElement b) =>
		{
			Friend friend = a.Query<Friend>("Friend").First();
			return string.Compare(strB: b.Query<Friend>("Friend").First().Username, strA: friend.Username);
		});
	}

	private void OnCloseIconButtonClicked()
	{
		EventManager.TriggerEvent("Event_OnFriendsClickClose");
	}

	private void OnFriendInviteButtonClicked(string steamId)
	{
		EventManager.TriggerEvent("Event_OnFriendInviteButtonClicked", new Dictionary<string, object> { { "steamId", steamId } });
	}
}
