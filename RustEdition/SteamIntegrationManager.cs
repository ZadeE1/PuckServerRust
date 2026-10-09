using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Steamworks;
using UnityEngine;

public static class SteamIntegrationManager
{
	private static readonly Logger Logger = new Logger("SteamIntegrationManager");

	private static List<string> joinedLobbyIds = new List<string>();

	private static Tween ticketTween;

	private static HAuthTicket pendingTicketHandle = HAuthTicket.Invalid;

	private static Callback<GetTicketForWebApiResponse_t> GetTicketForWebApiCallback;

	private static Callback<MicroTxnAuthorizationResponse_t> MicroTxnAuthorizationResponse;

	private static Callback<GameRichPresenceJoinRequested_t> GameRichPresenceJoinRequested;

	private static Callback<NewUrlLaunchParameters_t> NewUrlLaunchParameters;

	private static Callback<LobbyCreated_t> LobbyCreatedCallback;

	private static Callback<LobbyEnter_t> LobbyEnterCallback;

	private static Callback<LobbyChatUpdate_t> LobbyChatUpdateCallback;

	private static Callback<GameLobbyJoinRequested_t> GameLobbyJoinRequestedCallback;

	private static Callback<PersonaStateChange_t> PersonaStateChangeCallback;

	public static bool IsOverlayEnabled
	{
		get
		{
			if (SteamManager.IsInitialized)
			{
				return SteamUtils.IsOverlayEnabled();
			}
			return false;
		}
	}

	public static void Initialize()
	{
		RegisterCallbacks();
		SteamIntegrationManagerController.Initialize();
	}

	public static void Dispose()
	{
		SteamIntegrationManagerController.Dispose();
		StopTicketForWebApi();
		UnregisterCallbacks();
		joinedLobbyIds.Clear();
	}

	private static void RegisterCallbacks()
	{
		if (SteamManager.IsInitialized)
		{
			GetTicketForWebApiCallback = Callback<GetTicketForWebApiResponse_t>.Create(OnGetTicketForWebApiResponse);
			MicroTxnAuthorizationResponse = Callback<MicroTxnAuthorizationResponse_t>.Create(OnMicroTxnAuthorizationResponse);
			GameRichPresenceJoinRequested = Callback<GameRichPresenceJoinRequested_t>.Create(OnGameRichPresenceJoinRequested);
			NewUrlLaunchParameters = Callback<NewUrlLaunchParameters_t>.Create(OnNewUrlLaunchParameters);
			LobbyCreatedCallback = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
			LobbyEnterCallback = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
			LobbyChatUpdateCallback = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
			GameLobbyJoinRequestedCallback = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
			PersonaStateChangeCallback = Callback<PersonaStateChange_t>.Create(OnPersonaStateChange);
		}
	}

	private static void UnregisterCallbacks()
	{
		if (SteamManager.IsInitialized)
		{
			GetTicketForWebApiCallback.Unregister();
			MicroTxnAuthorizationResponse.Unregister();
			GameRichPresenceJoinRequested.Unregister();
			NewUrlLaunchParameters.Unregister();
			LobbyCreatedCallback.Unregister();
			LobbyEnterCallback.Unregister();
			LobbyChatUpdateCallback.Unregister();
			GameLobbyJoinRequestedCallback.Unregister();
			PersonaStateChangeCallback.Unregister();
		}
	}

	public static void SetRichPresenceMainMenu()
	{
		if (SteamManager.IsInitialized)
		{
			SteamFriends.ClearRichPresence();
			SteamFriends.SetRichPresence("steam_display", "#Status_MainMenu");
			SteamFriends.SetRichPresence("status", "In the changing room");
		}
	}

	public static void SetRichPresenceSpectating(Server server, int playerCount)
	{
		if (SteamManager.IsInitialized)
		{
			SteamFriends.SetRichPresence("steam_player_group", $"{server.IpAddress}:{server.Port}");
			SteamFriends.SetRichPresence("steam_player_group_size", $"{playerCount}");
			SteamFriends.SetRichPresence("steam_display", "#Status_Spectating");
			SteamFriends.SetRichPresence("status", "Spectating");
			SteamFriends.SetRichPresence("connect", $"+ipAddress {server.IpAddress} +port {server.Port}");
		}
	}

	public static void SetRichPresencePlaying(Server server, int playerCount)
	{
		if (SteamManager.IsInitialized)
		{
			SteamFriends.SetRichPresence("steam_player_group", $"{server.IpAddress}:{server.Port}");
			SteamFriends.SetRichPresence("steam_player_group_size", $"{playerCount}");
			SteamFriends.SetRichPresence("steam_display", "#Status_Playing");
			SteamFriends.SetRichPresence("status", "Playing");
			SteamFriends.SetRichPresence("connect", $"+ipAddress {server.IpAddress} +port {server.Port}");
		}
	}

	public static void UpdateRichPresenceScore(bool show, int period, int blueScore, int redScore)
	{
		if (SteamManager.IsInitialized)
		{
			string pchValue = (show ? $" | P{period} {blueScore} - {redScore}" : " ");
			SteamFriends.SetRichPresence("score", pchValue);
		}
	}

	public static void UpdateRichPresenceRole(PlayerRole role)
	{
		if (SteamManager.IsInitialized)
		{
			string pchValue = role.ToString().Replace("Attacker", "Skater");
			SteamFriends.SetRichPresence("role", pchValue);
		}
	}

	public static void UpdateRichPresenceTeam(PlayerTeam team)
	{
		if (SteamManager.IsInitialized)
		{
			string pchValue = team.ToString().Replace("Blue", "Team Blue").Replace("Red", "Team Red");
			SteamFriends.SetRichPresence("team", pchValue);
		}
	}

	public static void UpdateRichPresencePhase(GamePhase phase)
	{
		if (SteamManager.IsInitialized)
		{
			string pchValue = ((phase != GamePhase.Warmup) ? "Playing" : "Warming up");
			SteamFriends.SetRichPresence("phase", pchValue);
		}
	}

	public static void CreateLobby()
	{
		if (SteamManager.IsInitialized)
		{
			Logger.Info("Creating lobby");
			SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 128);
		}
	}

	public static void JoinLobby(string lobbyId)
	{
		if (SteamManager.IsInitialized && !IsInLobby(lobbyId))
		{
			Logger.Info("Joining lobby " + lobbyId);
			SteamMatchmaking.JoinLobby(new CSteamID(ulong.Parse(lobbyId)));
		}
	}

	public static void LeaveLobby(string lobbyId)
	{
		if (SteamManager.IsInitialized && IsInLobby(lobbyId))
		{
			Logger.Info("Leaving lobby " + lobbyId);
			SteamMatchmaking.LeaveLobby(new CSteamID(ulong.Parse(lobbyId)));
			joinedLobbyIds.Remove(lobbyId);
			EventManager.TriggerEvent("Event_OnLobbyLeft", new Dictionary<string, object> { { "lobbyId", lobbyId } });
		}
	}

	public static void LeaveAllLobbies()
	{
		if (!SteamManager.IsInitialized)
		{
			return;
		}
		Logger.Info("Leaving all lobbies");
		foreach (string item in joinedLobbyIds.ToList())
		{
			LeaveLobby(item);
		}
	}

	public static bool IsInLobby(string lobbyId)
	{
		return joinedLobbyIds.Contains(lobbyId);
	}

	public static string GetSteamId()
	{
		if (!SteamManager.IsInitialized)
		{
			return null;
		}
		return SteamUser.GetSteamID().ToString();
	}

	public static void GetTicketForWebApi()
	{
		if (SteamManager.IsInitialized)
		{
			RequestTicketForWebApi();
		}
	}

	public static void StopTicketForWebApi()
	{
		ticketTween?.Kill();
		ticketTween = null;
		CancelPendingTicket();
	}

	private static void RequestTicketForWebApi()
	{
		ticketTween?.Kill();
		ticketTween = null;
		if (!SteamUser.BLoggedOn())
		{
			Logger.Warning("Steam client is not logged on to Steam servers; cannot fetch web API auth ticket");
			EventManager.TriggerEvent("Event_OnSteamAuthenticationStalled");
			ScheduleTicketRetry();
			return;
		}
		CancelPendingTicket();
		Logger.Info("Requesting web API auth ticket");
		EventManager.TriggerEvent("Event_OnSteamAuthenticationStarted");
		pendingTicketHandle = SteamUser.GetAuthTicketForWebApi("*");
		ticketTween = DOVirtual.DelayedCall(10f, () =>
		{
			Logger.Warning($"Web API auth ticket request did not respond within {10f}s; retrying");
			EventManager.TriggerEvent("Event_OnSteamAuthenticationStalled");
			RequestTicketForWebApi();
		});
	}

	private static void ScheduleTicketRetry()
	{
		ticketTween?.Kill();
		ticketTween = DOVirtual.DelayedCall(3f, RequestTicketForWebApi);
	}

	private static void CancelPendingTicket()
	{
		if (!(pendingTicketHandle == HAuthTicket.Invalid))
		{
			SteamUser.CancelAuthTicket(pendingTicketHandle);
			pendingTicketHandle = HAuthTicket.Invalid;
		}
	}

	public static void GetLaunchCommandLine()
	{
		if (SteamManager.IsInitialized)
		{
			SteamApps.GetLaunchCommandLine(out var pszCommandLine, 256);
			string[] array = pszCommandLine.Split(" ");
			if (array.Length != 0)
			{
				Logger.Info($"GotLaunchCommandLine: {pszCommandLine} ({array.Length})");
				EventManager.TriggerEvent("Event_OnGotLaunchCommandLine", new Dictionary<string, object> { { "args", array } });
			}
		}
	}

	public static Texture2D GetAvatar(string steamId, AvatarSize size)
	{
		if (!SteamManager.IsInitialized)
		{
			return null;
		}
		CSteamID steamIDFriend = new CSteamID(ulong.Parse(steamId));
		int iImage = size switch
		{
			AvatarSize.Small => SteamFriends.GetSmallFriendAvatar(steamIDFriend), 
			AvatarSize.Medium => SteamFriends.GetMediumFriendAvatar(steamIDFriend), 
			AvatarSize.Large => SteamFriends.GetLargeFriendAvatar(steamIDFriend), 
			_ => SteamFriends.GetMediumFriendAvatar(steamIDFriend), 
		};
		SteamUtils.GetImageSize(iImage, out var pnWidth, out var pnHeight);
		byte[] array = new byte[pnWidth * pnHeight * 4];
		bool imageRGBA = SteamUtils.GetImageRGBA(iImage, array, array.Length);
		byte[] array2 = new byte[array.Length];
		int num = (int)(pnWidth * 4);
		for (int i = 0; i < pnHeight; i++)
		{
			Buffer.BlockCopy(array, i * num, array2, ((int)(pnHeight - 1) - i) * num, num);
		}
		if (imageRGBA)
		{
			Texture2D texture2D = new Texture2D((int)pnWidth, (int)pnHeight, TextureFormat.RGBA32, mipChain: false);
			texture2D.LoadRawTextureData(array2);
			texture2D.Apply();
			return texture2D;
		}
		return null;
	}

	public static string GetUsername(string steamId)
	{
		if (!SteamManager.IsInitialized)
		{
			return null;
		}
		CSteamID cSteamID = new CSteamID(ulong.Parse(steamId));
		if (cSteamID == SteamUser.GetSteamID())
		{
			return SteamFriends.GetPersonaName();
		}
		return SteamFriends.GetFriendPersonaName(cSteamID);
	}

	public static string[] GetFriendSteamIds(bool includeOffline = false)
	{
		if (!SteamManager.IsInitialized)
		{
			return new string[0];
		}
		int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
		List<string> list = new List<string>();
		for (int i = 0; i < friendCount; i++)
		{
			string text = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate).ToString();
			if (IsFriendOnline(text) || includeOffline)
			{
				list.Add(text);
			}
		}
		return list.ToArray();
	}

	public static string GetLobbyOwnerSteamId(string lobbyId)
	{
		if (!SteamManager.IsInitialized)
		{
			return null;
		}
		return SteamMatchmaking.GetLobbyOwner(new CSteamID(ulong.Parse(lobbyId))).ToString();
	}

	public static string[] GetLobbyMemberSteamIds(string lobbyId)
	{
		if (!SteamManager.IsInitialized)
		{
			return new string[0];
		}
		CSteamID steamIDLobby = new CSteamID(ulong.Parse(lobbyId));
		int numLobbyMembers = SteamMatchmaking.GetNumLobbyMembers(steamIDLobby);
		List<string> list = new List<string>();
		for (int i = 0; i < numLobbyMembers; i++)
		{
			string item = SteamMatchmaking.GetLobbyMemberByIndex(steamIDLobby, i).ToString();
			list.Add(item);
		}
		return list.ToArray();
	}

	public static bool IsFriend(string steamId)
	{
		if (!SteamManager.IsInitialized)
		{
			return false;
		}
		return SteamFriends.GetFriendRelationship(new CSteamID(ulong.Parse(steamId))) == EFriendRelationship.k_EFriendRelationshipFriend;
	}

	public static bool IsFriendOnline(string steamId)
	{
		if (!SteamManager.IsInitialized)
		{
			return false;
		}
		return SteamFriends.GetFriendPersonaState(new CSteamID(ulong.Parse(steamId))) != EPersonaState.k_EPersonaStateOffline;
	}

	public static void InviteToLobby(string lobbyId, string invitedSteamId)
	{
		if (SteamManager.IsInitialized)
		{
			CSteamID steamIDLobby = new CSteamID(ulong.Parse(lobbyId));
			CSteamID steamIDInvitee = new CSteamID(ulong.Parse(invitedSteamId));
			SteamMatchmaking.InviteUserToLobby(steamIDLobby, steamIDInvitee);
		}
	}

	private static void OnGetTicketForWebApiResponse(GetTicketForWebApiResponse_t response)
	{
		ticketTween?.Kill();
		ticketTween = null;
		pendingTicketHandle = HAuthTicket.Invalid;
		if (response.m_eResult != EResult.k_EResultOK)
		{
			Logger.Warning($"Web API auth ticket request failed ({response.m_eResult}); retrying");
			EventManager.TriggerEvent("Event_OnSteamAuthenticationStalled");
			ScheduleTicketRetry();
		}
		else
		{
			byte[] rgubTicket = response.m_rgubTicket;
			string value = BitConverter.ToString(rgubTicket, 0, rgubTicket.Length).Replace("-", string.Empty);
			EventManager.TriggerEvent("Event_OnGetTicketForWebApiResponse", new Dictionary<string, object> { { "ticket", value } });
		}
	}

	private static void OnMicroTxnAuthorizationResponse(MicroTxnAuthorizationResponse_t response)
	{
		EventManager.TriggerEvent("Event_OnMicroTxnAuthorizationResponse", new Dictionary<string, object>
		{
			{ "orderId", response.m_ulOrderID },
			{
				"authorized",
				Convert.ToBoolean(response.m_bAuthorized)
			}
		});
	}

	private static void OnGameRichPresenceJoinRequested(GameRichPresenceJoinRequested_t response)
	{
		EventManager.TriggerEvent("Event_OnGameRichPresenceJoinRequested", new Dictionary<string, object> { 
		{
			"args",
			response.m_rgchConnect.Split(" ")
		} });
	}

	private static void OnNewUrlLaunchParameters(NewUrlLaunchParameters_t response)
	{
		GetLaunchCommandLine();
	}

	private static void OnLobbyCreated(LobbyCreated_t result)
	{
		if (result.m_eResult == EResult.k_EResultOK)
		{
			string text = result.m_ulSteamIDLobby.ToString();
			Logger.Info("Lobby " + text + " created");
			if (!joinedLobbyIds.Contains(text))
			{
				joinedLobbyIds.Add(text);
			}
			EventManager.TriggerEvent("Event_OnLobbyCreated", new Dictionary<string, object> { { "lobbyId", text } });
		}
	}

	private static void OnLobbyEntered(LobbyEnter_t result)
	{
		string text = result.m_ulSteamIDLobby.ToString();
		Logger.Info("Lobby " + text + " entered");
		if (!joinedLobbyIds.Contains(text))
		{
			joinedLobbyIds.Add(text);
		}
		EventManager.TriggerEvent("Event_OnLobbyEntered", new Dictionary<string, object>
		{
			{ "lobbyId", text },
			{
				"ownerSteamId",
				GetLobbyOwnerSteamId(text)
			},
			{
				"memberSteamIds",
				GetLobbyMemberSteamIds(text)
			}
		});
	}

	private static void OnLobbyChatUpdate(LobbyChatUpdate_t result)
	{
		string text = result.m_ulSteamIDLobby.ToString();
		Logger.Info("Lobby " + text + " updated");
		EventManager.TriggerEvent("Event_OnLobbyChatUpdate", new Dictionary<string, object>
		{
			{ "lobbyId", text },
			{
				"ownerSteamId",
				GetLobbyOwnerSteamId(text)
			},
			{
				"memberSteamIds",
				GetLobbyMemberSteamIds(text)
			}
		});
	}

	private static void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t result)
	{
		CSteamID steamIDLobby = result.m_steamIDLobby;
		string text = steamIDLobby.ToString();
		Logger.Info("Lobby " + text + " join requested");
		EventManager.TriggerEvent("Event_OnGameLobbyJoinRequested", new Dictionary<string, object> { { "lobbyId", text } });
	}

	private static void OnPersonaStateChange(PersonaStateChange_t result)
	{
		string value = result.m_ulSteamID.ToString();
		if ((result.m_nChangeFlags & (EPersonaChange.k_EPersonaChangeName | EPersonaChange.k_EPersonaChangeStatus | EPersonaChange.k_EPersonaChangeAvatar | EPersonaChange.k_EPersonaChangeRelationshipChanged | EPersonaChange.k_EPersonaChangeNickname)) != 0)
		{
			EventManager.TriggerEvent("Event_OnPersonaStateChange", new Dictionary<string, object> { { "steamId", value } });
		}
	}
}
