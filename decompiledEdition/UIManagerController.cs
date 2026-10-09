using System.Collections.Generic;
using UnityEngine;

public class UIManagerController : MonoBehaviour
{
	private UIManager uiManager;

	public void Awake()
	{
		uiManager = GetComponent<UIManager>();
		EventManager.AddEventListener("Event_OnUserInterfaceScaleChanged", Event_OnUserInterfaceScaleChanged);
		EventManager.AddEventListener("Event_OnUIStateChanged", Event_OnUIStateChanged);
		EventManager.AddEventListener("Event_OnMainMenuClickPlay", Event_OnMainMenuClickPlay);
		EventManager.AddEventListener("Event_OnMainMenuClickPlayer", Event_OnMainMenuClickPlayer);
		EventManager.AddEventListener("Event_OnMainMenuClickSettings", Event_OnMainMenuClickSettings);
		EventManager.AddEventListener("Event_OnMainMenuClickMods", Event_OnMainMenuClickMods);
		EventManager.AddEventListener("Event_OnPlayerMenuClickBack", Event_OnPlayerMenuClickBack);
		EventManager.AddEventListener("Event_OnPlayerMenuClickIdentity", Event_OnPlayerMenuClickIdentity);
		EventManager.AddEventListener("Event_OnPlayerMenuClickAppearance", Event_OnPlayerMenuClickAppearance);
		EventManager.AddEventListener("Event_OnIdentityClickClose", Event_OnIdentityClickClose);
		EventManager.AddEventListener("Event_OnAppearanceClickClose", Event_OnAppearanceClickClose);
		EventManager.AddEventListener("Event_OnPauseMenuClickReturnToGame", Event_OnPauseMenuClickReturnToGame);
		EventManager.AddEventListener("Event_OnPauseMenuClickSettings", Event_OnPauseMenuClickSettings);
		EventManager.AddEventListener("Event_OnPauseMenuClickSelectTeam", Event_OnPauseMenuClickSelectTeam);
		EventManager.AddEventListener("Event_OnPauseMenuClickSelectPosition", Event_OnPauseMenuClickSelectPosition);
		EventManager.AddEventListener("Event_OnPauseMenuClickServerBrowser", Event_OnPauseMenuClickServerBrowser);
		EventManager.AddEventListener("Event_OnServerBrowserClickClose", Event_OnServerBrowserClickClose);
		EventManager.AddEventListener("Event_OnServerBrowserClickEndPoint", Event_OnServerBrowserClickEndPoint);
		EventManager.AddEventListener("Event_OnServerBrowserClickNewServer", Event_OnServerBrowserClickNewServer);
		EventManager.AddEventListener("Event_OnServerBrowserClickDirectConnect", Event_OnServerBrowserClickDirectConnect);
		EventManager.AddEventListener("Event_OnDirectConnectClickClose", Event_OnDirectConnectClickClose);
		EventManager.AddEventListener("Event_OnDirectConnectClickConnect", Event_OnDirectConnectClickConnect);
		EventManager.AddEventListener("Event_OnSettingsClickClose", Event_OnSettingsClickClose);
		EventManager.AddEventListener("Event_OnNewServerClickStart", Event_OnNewServerClickStart);
		EventManager.AddEventListener("Event_OnNewServerClickClose", Event_OnNewServerClickClose);
		EventManager.AddEventListener("Event_OnModsClickClose", Event_OnModsClickClose);
		EventManager.AddEventListener("Event_OnFooterClickInvite", Event_OnFooterClickInvite);
		EventManager.AddEventListener("Event_OnFriendsClickClose", Event_OnFriendsClickClose);
		EventManager.AddEventListener("Event_OnPlayClickServerBrowser", Event_OnPlayClickServerBrowser);
		EventManager.AddEventListener("Event_OnPlayClickClose", Event_OnPlayClickClose);
		EventManager.AddEventListener("Event_OnChatMessageAdded", Event_OnChatMessageAdded);
		EventManager.AddEventListener("Event_OnMatchJoinTimeoutTickerStarted", Event_OnMatchJoinTimeoutTickerStarted);
		EventManager.AddEventListener("Event_OnMatchJoinTimeoutTickerTick", Event_OnMatchJoinTimeoutTickerTick);
		EventManager.AddEventListener("Event_Everyone_OnPlayerSpawned", Event_Everyone_OnPlayerSpawned);
		EventManager.AddEventListener("Event_Everyone_OnPlayerGameStateChanged", Event_Everyone_OnPlayerGameStateChanged);
	}

	private void Start()
	{
		uiManager.SetUIScale(SettingsManager.UserInterfaceScale);
		uiManager.ShowPhaseViews(GlobalStateManager.UIState.Phase);
	}

	private void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_OnUserInterfaceScaleChanged", Event_OnUserInterfaceScaleChanged);
		EventManager.RemoveEventListener("Event_OnUIStateChanged", Event_OnUIStateChanged);
		EventManager.RemoveEventListener("Event_OnMainMenuClickPlay", Event_OnMainMenuClickPlay);
		EventManager.RemoveEventListener("Event_OnMainMenuClickPlayer", Event_OnMainMenuClickPlayer);
		EventManager.RemoveEventListener("Event_OnMainMenuClickSettings", Event_OnMainMenuClickSettings);
		EventManager.RemoveEventListener("Event_OnMainMenuClickMods", Event_OnMainMenuClickMods);
		EventManager.RemoveEventListener("Event_OnPlayerMenuClickBack", Event_OnPlayerMenuClickBack);
		EventManager.RemoveEventListener("Event_OnPlayerMenuClickIdentity", Event_OnPlayerMenuClickIdentity);
		EventManager.RemoveEventListener("Event_OnPlayerMenuClickAppearance", Event_OnPlayerMenuClickAppearance);
		EventManager.RemoveEventListener("Event_OnIdentityClickClose", Event_OnIdentityClickClose);
		EventManager.RemoveEventListener("Event_OnAppearanceClickClose", Event_OnAppearanceClickClose);
		EventManager.RemoveEventListener("Event_OnPauseMenuClickReturnToGame", Event_OnPauseMenuClickReturnToGame);
		EventManager.RemoveEventListener("Event_OnPauseMenuClickSettings", Event_OnPauseMenuClickSettings);
		EventManager.RemoveEventListener("Event_OnPauseMenuClickSelectTeam", Event_OnPauseMenuClickSelectTeam);
		EventManager.RemoveEventListener("Event_OnPauseMenuClickSelectPosition", Event_OnPauseMenuClickSelectPosition);
		EventManager.RemoveEventListener("Event_OnPauseMenuClickServerBrowser", Event_OnPauseMenuClickServerBrowser);
		EventManager.RemoveEventListener("Event_OnServerBrowserClickClose", Event_OnServerBrowserClickClose);
		EventManager.RemoveEventListener("Event_OnServerBrowserClickEndPoint", Event_OnServerBrowserClickEndPoint);
		EventManager.RemoveEventListener("Event_OnServerBrowserClickNewServer", Event_OnServerBrowserClickNewServer);
		EventManager.RemoveEventListener("Event_OnServerBrowserClickDirectConnect", Event_OnServerBrowserClickDirectConnect);
		EventManager.RemoveEventListener("Event_OnDirectConnectClickClose", Event_OnDirectConnectClickClose);
		EventManager.RemoveEventListener("Event_OnDirectConnectClickConnect", Event_OnDirectConnectClickConnect);
		EventManager.RemoveEventListener("Event_OnSettingsClickClose", Event_OnSettingsClickClose);
		EventManager.RemoveEventListener("Event_OnNewServerClickStart", Event_OnNewServerClickStart);
		EventManager.RemoveEventListener("Event_OnNewServerClickClose", Event_OnNewServerClickClose);
		EventManager.RemoveEventListener("Event_OnModsClickClose", Event_OnModsClickClose);
		EventManager.RemoveEventListener("Event_OnFooterClickInvite", Event_OnFooterClickInvite);
		EventManager.RemoveEventListener("Event_OnFriendsClickClose", Event_OnFriendsClickClose);
		EventManager.RemoveEventListener("Event_OnPlayClickServerBrowser", Event_OnPlayClickServerBrowser);
		EventManager.RemoveEventListener("Event_OnPlayClickClose", Event_OnPlayClickClose);
		EventManager.RemoveEventListener("Event_OnChatMessageAdded", Event_OnChatMessageAdded);
		EventManager.RemoveEventListener("Event_OnMatchJoinTimeoutTickerStarted", Event_OnMatchJoinTimeoutTickerStarted);
		EventManager.RemoveEventListener("Event_OnMatchJoinTimeoutTickerTick", Event_OnMatchJoinTimeoutTickerTick);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerSpawned", Event_Everyone_OnPlayerSpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerGameStateChanged", Event_Everyone_OnPlayerGameStateChanged);
	}

	private void HandlePlayerGameState(Player player)
	{
		PlayerGameState value = player.GameState.Value;
		uiManager.ShowPhaseViews(GlobalStateManager.UIState.Phase);
		switch (value.Phase)
		{
		case PlayerPhase.TeamSelect:
			uiManager.TeamSelect.Show();
			break;
		case PlayerPhase.PositionSelect:
			uiManager.PositionSelect.Show();
			break;
		case PlayerPhase.Play:
			uiManager.Hud.Show();
			uiManager.Minimap.Show();
			break;
		case PlayerPhase.Replay:
		case PlayerPhase.Spectate:
			uiManager.Minimap.Show();
			break;
		}
	}

	private void Event_OnUserInterfaceScaleChanged(Dictionary<string, object> message)
	{
		float uIScale = (float)message["value"];
		uiManager.SetUIScale(uIScale);
	}

	private void Event_OnUIStateChanged(Dictionary<string, object> message)
	{
		UIState uIState = (UIState)message["oldUIState"];
		UIState uIState2 = (UIState)message["newUIState"];
		if (uIState.Phase != uIState2.Phase)
		{
			uiManager.ShowPhaseViews(uIState2.Phase);
		}
	}

	private void Event_OnMainMenuClickPlay(Dictionary<string, object> message)
	{
		uiManager.Play.Show();
		uiManager.MainMenu.Hide();
	}

	private void Event_OnMainMenuClickPlayer(Dictionary<string, object> message)
	{
		uiManager.PlayerMenu.Show();
		uiManager.MainMenu.Hide();
	}

	private void Event_OnMainMenuClickSettings(Dictionary<string, object> message)
	{
		uiManager.Settings.Show();
		uiManager.MainMenu.Hide();
	}

	private void Event_OnMainMenuClickMods(Dictionary<string, object> message)
	{
		uiManager.Mods.Show();
		uiManager.MainMenu.Hide();
	}

	private void Event_OnPlayerMenuClickBack(Dictionary<string, object> message)
	{
		uiManager.MainMenu.Show();
		uiManager.PlayerMenu.Hide();
	}

	private void Event_OnPlayerMenuClickIdentity(Dictionary<string, object> message)
	{
		uiManager.Identity.Show();
		uiManager.PlayerMenu.Hide();
	}

	private void Event_OnPlayerMenuClickAppearance(Dictionary<string, object> message)
	{
		uiManager.Appearance.Show();
		uiManager.PlayerMenu.Hide();
	}

	private void Event_OnIdentityClickClose(Dictionary<string, object> message)
	{
		uiManager.PlayerMenu.Show();
		uiManager.Identity.Hide();
	}

	private void Event_OnAppearanceClickClose(Dictionary<string, object> message)
	{
		uiManager.PlayerMenu.Show();
		uiManager.Appearance.Hide();
	}

	private void Event_OnPauseMenuClickReturnToGame(Dictionary<string, object> message)
	{
		uiManager.PauseMenu.Hide();
	}

	private void Event_OnPauseMenuClickSettings(Dictionary<string, object> message)
	{
		uiManager.Settings.Show();
		uiManager.PauseMenu.Hide();
	}

	private void Event_OnPauseMenuClickSelectTeam(Dictionary<string, object> message)
	{
		uiManager.PauseMenu.Hide();
	}

	private void Event_OnPauseMenuClickSelectPosition(Dictionary<string, object> message)
	{
		uiManager.PauseMenu.Hide();
	}

	private void Event_OnPauseMenuClickServerBrowser(Dictionary<string, object> message)
	{
		uiManager.ServerBrowser.Show();
		uiManager.PauseMenu.Hide();
	}

	private void Event_OnServerBrowserClickClose(Dictionary<string, object> message)
	{
		switch (GlobalStateManager.UIState.Phase)
		{
		case UIPhase.LockerRoom:
			uiManager.Play.Show();
			break;
		case UIPhase.Playing:
			uiManager.PauseMenu.Show();
			break;
		}
		uiManager.ServerBrowser.Hide();
	}

	private void Event_OnServerBrowserClickEndPoint(Dictionary<string, object> message)
	{
	}

	private void Event_OnServerBrowserClickNewServer(Dictionary<string, object> message)
	{
		uiManager.NewServer.Show();
		uiManager.ServerBrowser.Hide();
	}

	private void Event_OnServerBrowserClickDirectConnect(Dictionary<string, object> message)
	{
		uiManager.DirectConnect.Show();
		uiManager.ServerBrowser.Hide();
	}

	private void Event_OnDirectConnectClickClose(Dictionary<string, object> message)
	{
		uiManager.ServerBrowser.Show();
		uiManager.DirectConnect.Hide();
	}

	private void Event_OnDirectConnectClickConnect(Dictionary<string, object> message)
	{
	}

	private void Event_OnSettingsClickClose(Dictionary<string, object> message)
	{
		switch (GlobalStateManager.UIState.Phase)
		{
		case UIPhase.LockerRoom:
			uiManager.MainMenu.Show();
			break;
		case UIPhase.Playing:
			uiManager.PauseMenu.Show();
			break;
		}
		uiManager.Settings.Hide();
	}

	private void Event_OnNewServerClickStart(Dictionary<string, object> message)
	{
		uiManager.ServerBrowser.Show();
		uiManager.NewServer.Hide();
	}

	private void Event_OnNewServerClickClose(Dictionary<string, object> message)
	{
		uiManager.ServerBrowser.Show();
		uiManager.NewServer.Hide();
	}

	private void Event_OnModsClickClose(Dictionary<string, object> message)
	{
		uiManager.MainMenu.Show();
		uiManager.Mods.Hide();
	}

	private void Event_OnFooterClickInvite(Dictionary<string, object> message)
	{
		uiManager.Friends.Show();
	}

	private void Event_OnFriendsClickClose(Dictionary<string, object> message)
	{
		uiManager.Friends.Hide();
	}

	private void Event_OnPlayClickServerBrowser(Dictionary<string, object> message)
	{
		uiManager.ServerBrowser.Show();
		uiManager.Play.Hide();
	}

	private void Event_OnPlayClickClose(Dictionary<string, object> message)
	{
		uiManager.MainMenu.Show();
		uiManager.Play.Hide();
	}

	private void Event_OnChatMessageAdded(Dictionary<string, object> message)
	{
		uiManager.PlayNotificationSound();
	}

	private void Event_OnMatchJoinTimeoutTickerStarted(Dictionary<string, object> message)
	{
		if (!BackendUtils.IsConnectedToMatchEndPoint())
		{
			uiManager.PlayWhooshSound();
		}
	}

	private void Event_OnMatchJoinTimeoutTickerTick(Dictionary<string, object> message)
	{
		if (!BackendUtils.IsConnectedToMatchEndPoint())
		{
			uiManager.PlayTickSound();
		}
	}

	private void Event_Everyone_OnPlayerSpawned(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		if (player.IsLocalPlayer)
		{
			HandlePlayerGameState(player);
		}
	}

	private void Event_Everyone_OnPlayerGameStateChanged(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		PlayerGameState playerGameState = (PlayerGameState)message["oldGameState"];
		PlayerGameState playerGameState2 = (PlayerGameState)message["newGameState"];
		if (player.IsLocalPlayer && playerGameState.Phase != playerGameState2.Phase)
		{
			HandlePlayerGameState(player);
		}
	}
}
