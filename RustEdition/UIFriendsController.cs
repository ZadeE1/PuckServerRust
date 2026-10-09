using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEngine;

public class UIFriendsController : UIViewController<UIFriends>
{
	private UIFriends uiFriends;

	public override void Awake()
	{
		base.Awake();
		uiFriends = GetComponent<UIFriends>();
		EventManager.AddEventListener("Event_OnSteamConnected", Event_OnSteamConnected);
		EventManager.AddEventListener("Event_OnPersonaStateChange", Event_OnPersonaStateChange);
	}

	public override void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_OnSteamConnected", Event_OnSteamConnected);
		EventManager.RemoveEventListener("Event_OnPersonaStateChange", Event_OnPersonaStateChange);
		base.OnDestroy();
	}

	private void ParseSteamId(string steamId)
	{
		bool flag = uiFriends.IsFriendListed(steamId);
		bool flag2 = SteamIntegrationManager.IsFriend(steamId);
		bool flag3 = SteamIntegrationManager.IsFriendOnline(steamId);
		bool flag4 = !flag & flag2 & flag3;
		bool flag5 = flag & flag2 & flag3;
		bool flag6 = flag && (!flag2 || !flag3);
		if (flag4)
		{
			string username = SteamIntegrationManager.GetUsername(steamId);
			Texture2D avatar = SteamIntegrationManager.GetAvatar(steamId, AvatarSize.Medium);
			uiFriends.AddFriend(steamId, username, avatar);
		}
		else if (flag5)
		{
			string username2 = SteamIntegrationManager.GetUsername(steamId);
			Texture2D avatar2 = SteamIntegrationManager.GetAvatar(steamId, AvatarSize.Medium);
			uiFriends.UpdateFriend(steamId, username2, avatar2);
		}
		else if (flag6)
		{
			uiFriends.RemoveFriend(steamId);
		}
	}

	private void Event_OnSteamConnected(Dictionary<string, object> message)
	{
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			SteamIntegrationManager.GetFriendSteamIds().ForEach((string steamId) =>
			{
				ParseSteamId(steamId);
			});
		}
	}

	private void Event_OnPersonaStateChange(Dictionary<string, object> message)
	{
		string steamId = (string)message["steamId"];
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			ParseSteamId(steamId);
		}
	}
}
