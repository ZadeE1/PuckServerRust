using System.Collections.Generic;
using UnityEngine;

public class LockerRoomPlayerController : MonoBehaviour
{
	private LockerRoomPlayer lockerRoomPlayer;

	private void Awake()
	{
		lockerRoomPlayer = GetComponent<LockerRoomPlayer>();
		EventManager.AddEventListener("Event_OnTeamChanged", Event_OnTeamChanged);
		EventManager.AddEventListener("Event_OnRoleChanged", Event_OnRoleChanged);
		EventManager.AddEventListener("Event_OnAppearanceClickItem", Event_OnAppearanceClickItem);
		EventManager.AddEventListener("Event_OnAppearanceShow", Event_OnAppearanceShow);
		EventManager.AddEventListener("Event_OnAppearanceHide", Event_OnAppearanceHide);
		EventManager.AddEventListener("Event_OnIdentityShow", Event_OnIdentityShow);
		EventManager.AddEventListener("Event_OnIdentityHide", Event_OnIdentityHide);
		EventManager.AddEventListener("Event_OnPlayerDataChanged", Event_OnPlayerDataChanged);
	}

	private void Start()
	{
		ApplySettings();
	}

	private void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_OnTeamChanged", Event_OnTeamChanged);
		EventManager.RemoveEventListener("Event_OnRoleChanged", Event_OnRoleChanged);
		EventManager.RemoveEventListener("Event_OnAppearanceClickItem", Event_OnAppearanceClickItem);
		EventManager.RemoveEventListener("Event_OnAppearanceShow", Event_OnAppearanceShow);
		EventManager.RemoveEventListener("Event_OnAppearanceHide", Event_OnAppearanceHide);
		EventManager.RemoveEventListener("Event_OnIdentityShow", Event_OnIdentityShow);
		EventManager.RemoveEventListener("Event_OnIdentityHide", Event_OnIdentityHide);
		EventManager.RemoveEventListener("Event_OnPlayerDataChanged", Event_OnPlayerDataChanged);
	}

	private void ApplySettings()
	{
		int flagID = SettingsManager.FlagID;
		int headgearID = SettingsManager.GetHeadgearID(SettingsManager.Team, SettingsManager.Role);
		int mustacheID = SettingsManager.MustacheID;
		int beardID = SettingsManager.BeardID;
		int jerseyID = SettingsManager.GetJerseyID(SettingsManager.Team, SettingsManager.Role);
		if (BackendManager.PlayerState.PlayerData != null)
		{
			lockerRoomPlayer.SetUsername(BackendManager.PlayerState.PlayerData.username);
			lockerRoomPlayer.SetNumber(BackendManager.PlayerState.PlayerData.number.ToString());
		}
		lockerRoomPlayer.SetLegsPadsActive(SettingsManager.Role == PlayerRole.Goalie);
		lockerRoomPlayer.SetFlagID(flagID);
		lockerRoomPlayer.SetHeadgearID(headgearID, SettingsManager.Role);
		lockerRoomPlayer.SetMustacheID(mustacheID);
		lockerRoomPlayer.SetBeardID(beardID);
		lockerRoomPlayer.SetJerseyID(jerseyID, SettingsManager.Team);
	}

	private void Event_OnTeamChanged(Dictionary<string, object> message)
	{
		ApplySettings();
	}

	private void Event_OnRoleChanged(Dictionary<string, object> message)
	{
		ApplySettings();
	}

	private void Event_OnAppearanceClickItem(Dictionary<string, object> message)
	{
		Item item = message["item"] as Item;
		_ = (AppearanceCategory)message["category"];
		AppearanceSubcategory appearanceSubcategory = (AppearanceSubcategory)message["subcategory"];
		PlayerTeam team = (PlayerTeam)message["team"];
		PlayerRole role = (PlayerRole)message["role"];
		switch (appearanceSubcategory)
		{
		case AppearanceSubcategory.Flags:
			lockerRoomPlayer.SetFlagID(item.id);
			break;
		case AppearanceSubcategory.Headgear:
			lockerRoomPlayer.SetHeadgearID(item.id, role);
			break;
		case AppearanceSubcategory.Mustaches:
			lockerRoomPlayer.SetMustacheID(item.id);
			break;
		case AppearanceSubcategory.Beards:
			lockerRoomPlayer.SetBeardID(item.id);
			break;
		case AppearanceSubcategory.Jerseys:
			lockerRoomPlayer.SetJerseyID(item.id, team);
			break;
		}
	}

	private void Event_OnAppearanceShow(Dictionary<string, object> message)
	{
		lockerRoomPlayer.AllowRotation = true;
		lockerRoomPlayer.SetRotationFromPreset("front");
	}

	private void Event_OnAppearanceHide(Dictionary<string, object> message)
	{
		ApplySettings();
		lockerRoomPlayer.AllowRotation = false;
		lockerRoomPlayer.SetRotationFromPreset("front");
	}

	private void Event_OnIdentityShow(Dictionary<string, object> message)
	{
		lockerRoomPlayer.AllowRotation = true;
		lockerRoomPlayer.SetRotationFromPreset("back");
	}

	private void Event_OnIdentityHide(Dictionary<string, object> message)
	{
		lockerRoomPlayer.AllowRotation = false;
		lockerRoomPlayer.SetRotationFromPreset("front");
	}

	private void Event_OnPlayerDataChanged(Dictionary<string, object> message)
	{
		if ((PlayerData)message["newPlayerData"] != null)
		{
			ApplySettings();
		}
	}
}
