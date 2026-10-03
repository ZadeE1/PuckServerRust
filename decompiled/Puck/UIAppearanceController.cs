using System.Collections.Generic;

public class UIAppearanceController : UIViewController<UIAppearance>
{
	private UIAppearance uiAppearance;

	public override void Awake()
	{
		base.Awake();
		uiAppearance = GetComponent<UIAppearance>();
		EventManager.AddEventListener("Event_OnTeamChanged", Event_OnTeamChanged);
		EventManager.AddEventListener("Event_OnRoleChanged", Event_OnRoleChanged);
		EventManager.AddEventListener("Event_OnApplyForBothTeamsChanged", Event_OnApplyForBothTeamsChanged);
		EventManager.AddEventListener("Event_OnFlagIDChanged", Event_OnFlagIDChanged);
		EventManager.AddEventListener("Event_OnHeadgearIDChanged", Event_OnHeadgearIDChanged);
		EventManager.AddEventListener("Event_OnMustacheIDChanged", Event_OnMustacheIDChanged);
		EventManager.AddEventListener("Event_OnBeardIDChanged", Event_OnBeardIDChanged);
		EventManager.AddEventListener("Event_OnJerseyIDChanged", Event_OnJerseyIDChanged);
		EventManager.AddEventListener("Event_OnStickSkinIDChanged", Event_OnStickSkinIDChanged);
		EventManager.AddEventListener("Event_OnStickShaftTapeIDChanged", Event_OnStickShaftTapeIDChanged);
		EventManager.AddEventListener("Event_OnStickBladeTapeIDChanged", Event_OnStickBladeTapeIDChanged);
		EventManager.AddEventListener("Event_OnPlayerDataChanged", Event_OnPlayerDataChanged);
		EventManager.AddEventListener("Event_OnAppearanceHide", Event_OnAppearanceHide);
	}

	private void Start()
	{
		uiAppearance.SetTeam(SettingsManager.Team);
		uiAppearance.SetRole(SettingsManager.Role);
		uiAppearance.SetApplyForBothTeams(SettingsManager.ApplyForBothTeams);
		uiAppearance.SetFlagID(SettingsManager.FlagID);
		uiAppearance.SetHeadgearID(PlayerTeam.Blue, PlayerRole.Attacker, SettingsManager.HeadgearIDBlueAttacker);
		uiAppearance.SetHeadgearID(PlayerTeam.Blue, PlayerRole.Goalie, SettingsManager.HeadgearIDBlueGoalie);
		uiAppearance.SetHeadgearID(PlayerTeam.Red, PlayerRole.Attacker, SettingsManager.HeadgearIDRedAttacker);
		uiAppearance.SetHeadgearID(PlayerTeam.Red, PlayerRole.Goalie, SettingsManager.HeadgearIDRedGoalie);
		uiAppearance.SetMustacheID(SettingsManager.MustacheID);
		uiAppearance.SetBeardID(SettingsManager.BeardID);
		uiAppearance.SetJerseyID(PlayerTeam.Blue, PlayerRole.Attacker, SettingsManager.JerseyIDBlueAttacker);
		uiAppearance.SetJerseyID(PlayerTeam.Blue, PlayerRole.Goalie, SettingsManager.JerseyIDBlueGoalie);
		uiAppearance.SetJerseyID(PlayerTeam.Red, PlayerRole.Attacker, SettingsManager.JerseyIDRedAttacker);
		uiAppearance.SetJerseyID(PlayerTeam.Red, PlayerRole.Goalie, SettingsManager.JerseyIDRedGoalie);
		uiAppearance.SetStickSkinID(PlayerTeam.Blue, PlayerRole.Attacker, SettingsManager.StickSkinIDBlueAttacker);
		uiAppearance.SetStickSkinID(PlayerTeam.Blue, PlayerRole.Goalie, SettingsManager.StickSkinIDBlueGoalie);
		uiAppearance.SetStickSkinID(PlayerTeam.Red, PlayerRole.Attacker, SettingsManager.StickSkinIDRedAttacker);
		uiAppearance.SetStickSkinID(PlayerTeam.Red, PlayerRole.Goalie, SettingsManager.StickSkinIDRedGoalie);
		uiAppearance.SetStickShaftTapeID(PlayerTeam.Blue, PlayerRole.Attacker, SettingsManager.StickShaftTapeIDBlueAttacker);
		uiAppearance.SetStickShaftTapeID(PlayerTeam.Blue, PlayerRole.Goalie, SettingsManager.StickShaftTapeIDBlueGoalie);
		uiAppearance.SetStickShaftTapeID(PlayerTeam.Red, PlayerRole.Attacker, SettingsManager.StickShaftTapeIDRedAttacker);
		uiAppearance.SetStickShaftTapeID(PlayerTeam.Red, PlayerRole.Goalie, SettingsManager.StickShaftTapeIDRedGoalie);
		uiAppearance.SetStickBladeTapeID(PlayerTeam.Blue, PlayerRole.Attacker, SettingsManager.StickBladeTapeIDBlueAttacker);
		uiAppearance.SetStickBladeTapeID(PlayerTeam.Blue, PlayerRole.Goalie, SettingsManager.StickBladeTapeIDBlueGoalie);
		uiAppearance.SetStickBladeTapeID(PlayerTeam.Red, PlayerRole.Attacker, SettingsManager.StickBladeTapeIDRedAttacker);
		uiAppearance.SetStickBladeTapeID(PlayerTeam.Red, PlayerRole.Goalie, SettingsManager.StickBladeTapeIDRedGoalie);
	}

	public override void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_OnTeamChanged", Event_OnTeamChanged);
		EventManager.RemoveEventListener("Event_OnRoleChanged", Event_OnRoleChanged);
		EventManager.RemoveEventListener("Event_OnApplyForBothTeamsChanged", Event_OnApplyForBothTeamsChanged);
		EventManager.RemoveEventListener("Event_OnFlagIDChanged", Event_OnFlagIDChanged);
		EventManager.RemoveEventListener("Event_OnHeadgearIDChanged", Event_OnHeadgearIDChanged);
		EventManager.RemoveEventListener("Event_OnMustacheIDChanged", Event_OnMustacheIDChanged);
		EventManager.RemoveEventListener("Event_OnBeardIDChanged", Event_OnBeardIDChanged);
		EventManager.RemoveEventListener("Event_OnJerseyIDChanged", Event_OnJerseyIDChanged);
		EventManager.RemoveEventListener("Event_OnStickSkinIDChanged", Event_OnStickSkinIDChanged);
		EventManager.RemoveEventListener("Event_OnStickShaftTapeIDChanged", Event_OnStickShaftTapeIDChanged);
		EventManager.RemoveEventListener("Event_OnStickBladeTapeIDChanged", Event_OnStickBladeTapeIDChanged);
		EventManager.RemoveEventListener("Event_OnPlayerDataChanged", Event_OnPlayerDataChanged);
		EventManager.RemoveEventListener("Event_OnAppearanceHide", Event_OnAppearanceHide);
		base.OnDestroy();
	}

	private void Event_OnTeamChanged(Dictionary<string, object> message)
	{
		PlayerTeam team = (PlayerTeam)message["value"];
		uiAppearance.SetTeam(team);
	}

	private void Event_OnRoleChanged(Dictionary<string, object> message)
	{
		PlayerRole role = (PlayerRole)message["value"];
		uiAppearance.SetRole(role);
	}

	private void Event_OnApplyForBothTeamsChanged(Dictionary<string, object> message)
	{
		bool applyForBothTeams = (bool)message["value"];
		uiAppearance.SetApplyForBothTeams(applyForBothTeams);
	}

	private void Event_OnFlagIDChanged(Dictionary<string, object> message)
	{
		int flagID = (int)message["value"];
		uiAppearance.SetFlagID(flagID);
	}

	private void Event_OnHeadgearIDChanged(Dictionary<string, object> message)
	{
		PlayerTeam team = (PlayerTeam)message["team"];
		PlayerRole role = (PlayerRole)message["role"];
		int value = (int)message["value"];
		uiAppearance.SetHeadgearID(team, role, value);
	}

	private void Event_OnMustacheIDChanged(Dictionary<string, object> message)
	{
		int mustacheID = (int)message["value"];
		uiAppearance.SetMustacheID(mustacheID);
	}

	private void Event_OnBeardIDChanged(Dictionary<string, object> message)
	{
		int beardID = (int)message["value"];
		uiAppearance.SetBeardID(beardID);
	}

	private void Event_OnJerseyIDChanged(Dictionary<string, object> message)
	{
		PlayerTeam team = (PlayerTeam)message["team"];
		PlayerRole role = (PlayerRole)message["role"];
		int value = (int)message["value"];
		uiAppearance.SetJerseyID(team, role, value);
	}

	private void Event_OnStickSkinIDChanged(Dictionary<string, object> message)
	{
		PlayerTeam team = (PlayerTeam)message["team"];
		PlayerRole role = (PlayerRole)message["role"];
		int value = (int)message["value"];
		uiAppearance.SetStickSkinID(team, role, value);
	}

	private void Event_OnStickShaftTapeIDChanged(Dictionary<string, object> message)
	{
		PlayerTeam team = (PlayerTeam)message["team"];
		PlayerRole role = (PlayerRole)message["role"];
		int value = (int)message["value"];
		uiAppearance.SetStickShaftTapeID(team, role, value);
	}

	private void Event_OnStickBladeTapeIDChanged(Dictionary<string, object> message)
	{
		PlayerTeam team = (PlayerTeam)message["team"];
		PlayerRole role = (PlayerRole)message["role"];
		int value = (int)message["value"];
		uiAppearance.SetStickBladeTapeID(team, role, value);
	}

	private void Event_OnPlayerDataChanged(Dictionary<string, object> message)
	{
		if ((PlayerData)message["newPlayerData"] != null)
		{
			uiAppearance.StyleRadioButtonGroups();
		}
	}

	private void Event_OnAppearanceHide(Dictionary<string, object> message)
	{
		uiAppearance.UpdateRadioButtons();
	}
}
