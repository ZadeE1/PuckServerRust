using System.Collections.Generic;
using UnityEngine;

public class UIFooterController : UIViewController<UIFooter>
{
	private UIFooter uiFooter;

	private Texture2D localAvatar;

	public override void Awake()
	{
		base.Awake();
		uiFooter = GetComponent<UIFooter>();
		EventManager.AddEventListener("Event_OnSteamConnected", Event_OnSteamConnected);
		EventManager.AddEventListener("Event_OnPlayerDataChanged", Event_OnPlayerDataChanged);
		EventManager.AddEventListener("Event_OnPlayerPartyDataChanged", Event_OnPlayerPartyDataChanged);
	}

	public override void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_OnSteamConnected", Event_OnSteamConnected);
		EventManager.RemoveEventListener("Event_OnPlayerDataChanged", Event_OnPlayerDataChanged);
		EventManager.RemoveEventListener("Event_OnPlayerPartyDataChanged", Event_OnPlayerPartyDataChanged);
		base.OnDestroy();
	}

	private void Event_OnSteamConnected(Dictionary<string, object> message)
	{
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			uiFooter.SetCreatePartyButtonVisibility(show: true);
			string steamId = SteamIntegrationManager.GetSteamId();
			if (!string.IsNullOrEmpty(steamId))
			{
				localAvatar = SteamIntegrationManager.GetAvatar(steamId, AvatarSize.Medium);
				RefreshLocalUser();
			}
		}
	}

	private void Event_OnPlayerDataChanged(Dictionary<string, object> message)
	{
		_ = (PlayerData)message["oldPlayerData"];
		PlayerData playerData = (PlayerData)message["newPlayerData"];
		if (playerData != null)
		{
			uiFooter.SetMmr(playerData.mmr);
			RefreshLocalUser();
		}
	}

	private void RefreshLocalUser()
	{
		string steamId = SteamIntegrationManager.GetSteamId();
		if (!string.IsNullOrEmpty(steamId))
		{
			PlayerData playerData = BackendManager.PlayerState.PlayerData;
			string username = ((playerData != null && !string.IsNullOrEmpty(playerData.username)) ? playerData.username : SteamIntegrationManager.GetUsername(steamId));
			uiFooter.SetLocalUser(username, localAvatar);
		}
	}

	private void Event_OnPlayerPartyDataChanged(Dictionary<string, object> message)
	{
		_ = (PlayerPartyData)message["oldPlayerPartyData"];
		PlayerPartyData playerPartyData = (PlayerPartyData)message["newPlayerPartyData"];
		if (playerPartyData == null)
		{
			uiFooter.ClearPartyUsers();
			uiFooter.SetCreatePartyButtonVisibility(show: true);
			uiFooter.SetInviteButtonVisibility(show: false);
			uiFooter.SetLeavePartyButtonVisibility(show: false);
			uiFooter.SetDisbandPartyButtonVisibility(show: false);
			return;
		}
		bool flag = BackendManager.PlayerState.PlayerData.steamId == playerPartyData.ownerSteamId;
		uiFooter.ClearPartyUsers();
		string[] memberSteamIds = playerPartyData.memberSteamIds;
		foreach (string steamId in memberSteamIds)
		{
			string username = SteamIntegrationManager.GetUsername(steamId);
			Texture2D avatar = SteamIntegrationManager.GetAvatar(steamId, AvatarSize.Medium);
			uiFooter.AddPartyUser(steamId, username, avatar);
		}
		uiFooter.SetCreatePartyButtonVisibility(show: false);
		uiFooter.SetInviteButtonVisibility(show: true);
		uiFooter.SetLeavePartyButtonVisibility(!flag);
		uiFooter.SetDisbandPartyButtonVisibility(flag);
	}
}
