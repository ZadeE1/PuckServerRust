using System.Collections.Generic;
using UnityEngine;

public class LockerRoomStickController : MonoBehaviour
{
	private LockerRoomStick lockerRoomStick;

	private void Awake()
	{
		lockerRoomStick = GetComponent<LockerRoomStick>();
		EventManager.AddEventListener("Event_OnTeamChanged", Event_OnTeamChanged);
		EventManager.AddEventListener("Event_OnRoleChanged", Event_OnRoleChanged);
		EventManager.AddEventListener("Event_OnAppearanceClickItem", Event_OnAppearanceClickItem);
		EventManager.AddEventListener("Event_OnAppearanceHide", Event_OnAppearanceHide);
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
		EventManager.RemoveEventListener("Event_OnAppearanceHide", Event_OnAppearanceHide);
	}

	private void ApplySettings()
	{
		int stickSkinID = SettingsManager.GetStickSkinID(SettingsManager.Team, SettingsManager.Role);
		int stickShaftTapeID = SettingsManager.GetStickShaftTapeID(SettingsManager.Team, SettingsManager.Role);
		int stickBladeTapeID = SettingsManager.GetStickBladeTapeID(SettingsManager.Team, SettingsManager.Role);
		lockerRoomStick.ShowRoleStick(SettingsManager.Role);
		lockerRoomStick.SetSkinID(stickSkinID, SettingsManager.Team, SettingsManager.Role);
		lockerRoomStick.SetShaftTapeID(stickShaftTapeID, SettingsManager.Role);
		lockerRoomStick.SetBladeTapeID(stickBladeTapeID, SettingsManager.Role);
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
		case AppearanceSubcategory.StickSkins:
			lockerRoomStick.SetSkinID(item.id, team, role);
			break;
		case AppearanceSubcategory.StickShaftTapes:
			lockerRoomStick.SetShaftTapeID(item.id, role);
			break;
		case AppearanceSubcategory.StickBladeTapes:
			lockerRoomStick.SetBladeTapeID(item.id, role);
			break;
		}
	}

	private void Event_OnAppearanceHide(Dictionary<string, object> message)
	{
		ApplySettings();
	}
}
