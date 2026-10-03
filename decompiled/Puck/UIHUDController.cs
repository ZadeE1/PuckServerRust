using System.Collections.Generic;

public class UIHUDController : UIViewController<UIHUD>
{
	private UIHUD uiHud;

	private PlayerBody localPlayerBody;

	private PlayerTeam localTeam;

	private PlayerVoiceRecorder localVoiceRecorder;

	public override void Awake()
	{
		base.Awake();
		uiHud = GetComponent<UIHUD>();
		EventManager.AddEventListener("Event_Everyone_OnPlayerBodySpawned", Event_Everyone_OnPlayerBodySpawned);
		EventManager.AddEventListener("Event_Everyone_OnPlayerBodyDespawned", Event_Everyone_OnPlayerBodyDespawned);
		EventManager.AddEventListener("Event_Everyone_OnPlayerVoiceStarted", Event_Everyone_OnPlayerVoiceStarted);
		EventManager.AddEventListener("Event_Everyone_OnPlayerVoiceStopped", Event_Everyone_OnPlayerVoiceStopped);
		EventManager.AddEventListener("Event_OnUnitsChanged", Event_OnUnitsChanged);
		EventManager.AddEventListener("Event_Everyone_OnPlayerGameStateChanged", Event_Everyone_OnPlayerGameStateChanged);
		EventManager.AddEventListener("Event_Everyone_OnPlayerPositionChanged", Event_Everyone_OnPlayerPositionChanged);
		EventManager.AddEventListener("Event_OnShowTeamColorBarChanged", Event_OnShowTeamColorBarChanged);
		EventManager.AddEventListener("Event_OnShowGameUserInterfaceChanged", Event_OnShowGameUserInterfaceChanged);
	}

	public override void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerBodySpawned", Event_Everyone_OnPlayerBodySpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerBodyDespawned", Event_Everyone_OnPlayerBodyDespawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerVoiceStarted", Event_Everyone_OnPlayerVoiceStarted);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerVoiceStopped", Event_Everyone_OnPlayerVoiceStopped);
		EventManager.RemoveEventListener("Event_OnUnitsChanged", Event_OnUnitsChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerGameStateChanged", Event_Everyone_OnPlayerGameStateChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerPositionChanged", Event_Everyone_OnPlayerPositionChanged);
		EventManager.RemoveEventListener("Event_OnShowTeamColorBarChanged", Event_OnShowTeamColorBarChanged);
		EventManager.RemoveEventListener("Event_OnShowGameUserInterfaceChanged", Event_OnShowGameUserInterfaceChanged);
		UnsubscribeFromLocalPlayerBody();
		UnsubscribeFromLocalVoiceRecorder();
		base.OnDestroy();
	}

	private void SubscribeToLocalPlayerBody(PlayerBody playerBody)
	{
		UnsubscribeFromLocalPlayerBody();
		localPlayerBody = playerBody;
		localPlayerBody.Stamina.OnRawValueChanged += OnLocalPlayerBodyStaminaChanged;
		localPlayerBody.Speed.OnRawValueChanged += OnLocalPlayerBodySpeedChanged;
	}

	private void UnsubscribeFromLocalPlayerBody()
	{
		if ((bool)localPlayerBody)
		{
			localPlayerBody.Stamina.OnRawValueChanged -= OnLocalPlayerBodyStaminaChanged;
			localPlayerBody.Speed.OnRawValueChanged -= OnLocalPlayerBodySpeedChanged;
			localPlayerBody = null;
		}
	}

	private void UpdatePosition(PlayerPosition playerPosition)
	{
		uiHud.SetPosition(playerPosition ? playerPosition.Name : string.Empty);
	}

	private void RefreshTeamColorBar()
	{
		bool flag = SettingsManager.ShowTeamColorBar && SettingsManager.ShowGameUserInterface;
		uiHud.SetTeam(flag ? localTeam : PlayerTeam.None);
	}

	private void Event_Everyone_OnPlayerBodySpawned(Dictionary<string, object> message)
	{
		PlayerBody playerBody = (PlayerBody)message["playerBody"];
		if (playerBody.Player.IsLocalPlayer)
		{
			SubscribeToLocalPlayerBody(playerBody);
			uiHud.Show();
			uiHud.SetStamina(playerBody.Stamina.Value);
			localTeam = playerBody.Player.Team;
			RefreshTeamColorBar();
			UpdatePosition(playerBody.Player.PlayerPosition);
		}
	}

	private void Event_Everyone_OnPlayerBodyDespawned(Dictionary<string, object> message)
	{
		if (!((PlayerBody)message["playerBody"] != localPlayerBody))
		{
			UnsubscribeFromLocalPlayerBody();
		}
	}

	private void SubscribeToLocalVoiceRecorder(PlayerVoiceRecorder voiceRecorder)
	{
		UnsubscribeFromLocalVoiceRecorder();
		localVoiceRecorder = voiceRecorder;
		localVoiceRecorder.OnTransmitLevelChanged += OnLocalVoiceTransmitLevelChanged;
	}

	private void UnsubscribeFromLocalVoiceRecorder()
	{
		if (!(localVoiceRecorder == null))
		{
			localVoiceRecorder.OnTransmitLevelChanged -= OnLocalVoiceTransmitLevelChanged;
			localVoiceRecorder = null;
		}
	}

	private void OnLocalPlayerBodyStaminaChanged(float oldStamina, float newStamina)
	{
		uiHud.SetStamina(newStamina);
	}

	private void OnLocalVoiceTransmitLevelChanged(float level)
	{
		uiHud.SetVoiceLevel(level);
	}

	private void Event_Everyone_OnPlayerVoiceStarted(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		if (player.IsLocalPlayer)
		{
			SubscribeToLocalVoiceRecorder(player.GetComponent<PlayerVoiceRecorder>());
			uiHud.SetVoiceLevel(0f);
			uiHud.SetVoiceTransmitting(value: true);
		}
	}

	private void Event_Everyone_OnPlayerVoiceStopped(Dictionary<string, object> message)
	{
		if (((Player)message["player"]).IsLocalPlayer)
		{
			UnsubscribeFromLocalVoiceRecorder();
			uiHud.SetVoiceTransmitting(value: false);
		}
	}

	private void OnLocalPlayerBodySpeedChanged(float oldSpeed, float newSpeed)
	{
		uiHud.SetSpeed(newSpeed);
	}

	private void Event_OnUnitsChanged(Dictionary<string, object> message)
	{
		switch ((Units)message["value"])
		{
		case Units.Metric:
			uiHud.SetUnits("KPH");
			break;
		case Units.Imperial:
			uiHud.SetUnits("MPH");
			break;
		}
	}

	private void Event_Everyone_OnPlayerGameStateChanged(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		if (player.IsLocalPlayer)
		{
			localTeam = player.Team;
			RefreshTeamColorBar();
		}
	}

	private void Event_Everyone_OnPlayerPositionChanged(Dictionary<string, object> message)
	{
		if (((Player)message["player"]).IsLocalPlayer)
		{
			UpdatePosition((PlayerPosition)message["newPlayerPosition"]);
		}
	}

	private void Event_OnShowTeamColorBarChanged(Dictionary<string, object> message)
	{
		RefreshTeamColorBar();
	}

	private void Event_OnShowGameUserInterfaceChanged(Dictionary<string, object> message)
	{
		RefreshTeamColorBar();
	}
}
