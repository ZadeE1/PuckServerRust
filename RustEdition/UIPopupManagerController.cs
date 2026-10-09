using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Humanizer;

internal class UIPopupManagerController : UIViewController<UIPopupManager>
{
	private UIPopupManager uiPopupManager;

	public override void Awake()
	{
		base.Awake();
		uiPopupManager = GetComponent<UIPopupManager>();
		EventManager.AddEventListener("Event_OnReconnectionStateChanged", Event_OnReconnectionStateChanged);
		EventManager.AddEventListener("Event_OnIdentityClickConfirm", Event_OnIdentityClickConfirm);
		EventManager.AddEventListener("Event_OnMainMenuClickExitGame", Event_OnMainMenuClickExitGame);
		EventManager.AddEventListener("Event_OnPauseMenuClickExitGame", Event_OnPauseMenuClickExitGame);
		EventManager.AddEventListener("Event_OnPlayerBanned", Event_OnPlayerBanned);
		EventManager.AddEventListener("Event_OnPlayerMuted", Event_OnPlayerMuted);
		EventManager.AddEventListener("Event_OnPlayerCooldown", Event_OnPlayerCooldown);
		EventManager.AddEventListener("Event_OnSettingsClickResetToDefault", Event_OnSettingsClickResetToDefault);
		EventManager.AddEventListener("Event_OnPopupClickOk", Event_OnPopupClickOk);
		EventManager.AddEventListener("Event_OnPopupClickClose", Event_OnPopupClickClose);
		EventManager.AddEventListener("Event_OnKeyBindRebindStart", Event_OnKeyBindRebindStart);
		EventManager.AddEventListener("Event_OnKeyBindRebindComplete", Event_OnKeyBindRebindComplete);
		EventManager.AddEventListener("Event_OnKeyBindRebindCancel", Event_OnKeyBindRebindCancel);
	}

	public override void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_OnReconnectionStateChanged", Event_OnReconnectionStateChanged);
		EventManager.RemoveEventListener("Event_OnIdentityClickConfirm", Event_OnIdentityClickConfirm);
		EventManager.RemoveEventListener("Event_OnMainMenuClickExitGame", Event_OnMainMenuClickExitGame);
		EventManager.RemoveEventListener("Event_OnPauseMenuClickExitGame", Event_OnPauseMenuClickExitGame);
		EventManager.RemoveEventListener("Event_OnPlayerBanned", Event_OnPlayerBanned);
		EventManager.RemoveEventListener("Event_OnPlayerMuted", Event_OnPlayerMuted);
		EventManager.RemoveEventListener("Event_OnPlayerCooldown", Event_OnPlayerCooldown);
		EventManager.RemoveEventListener("Event_OnSettingsClickResetToDefault", Event_OnSettingsClickResetToDefault);
		EventManager.RemoveEventListener("Event_OnPopupClickOk", Event_OnPopupClickOk);
		EventManager.RemoveEventListener("Event_OnPopupClickClose", Event_OnPopupClickClose);
		EventManager.RemoveEventListener("Event_OnKeyBindRebindStart", Event_OnKeyBindRebindStart);
		EventManager.RemoveEventListener("Event_OnKeyBindRebindComplete", Event_OnKeyBindRebindComplete);
		EventManager.RemoveEventListener("Event_OnKeyBindRebindCancel", Event_OnKeyBindRebindCancel);
		base.OnDestroy();
	}

	private void Event_OnReconnectionStateChanged(Dictionary<string, object> message)
	{
		ReconnectionState reconnectionState = (ReconnectionState)message["oldReconnectionState"];
		ReconnectionState reconnectionState2 = (ReconnectionState)message["newReconnectionState"];
		switch (reconnectionState2.Phase)
		{
		case ReconnectionPhase.None:
			if (reconnectionState.Phase == ReconnectionPhase.AwaitingPassword)
			{
				uiPopupManager.HidePopup("missingPassword");
			}
			else if (reconnectionState.Phase == ReconnectionPhase.AwaitingMods)
			{
				uiPopupManager.HidePopup("missingMods");
				uiPopupManager.HidePopup("downloadingMods");
			}
			break;
		case ReconnectionPhase.AwaitingPassword:
			if (reconnectionState2.Password == null)
			{
				PopupMissingPasswordContent content3 = uiPopupManager.CreateMissingPasswordContent();
				uiPopupManager.ShowPopup("missingPassword", "PASSWORD REQUIRED", content3, showOkButton: true, showCloseButton: true);
			}
			break;
		case ReconnectionPhase.AwaitingMods:
		{
			bool flag = !Enumerable.SequenceEqual(reconnectionState.ClientRequiredModIds, reconnectionState2.ClientRequiredModIds);
			bool flag2 = !Enumerable.SequenceEqual(reconnectionState.PendingEnablingModIds, reconnectionState2.PendingEnablingModIds);
			bool flag3 = !Enumerable.SequenceEqual(reconnectionState.PendingReadinessModIds, reconnectionState2.PendingReadinessModIds);
			if (flag && reconnectionState2.PendingModIds.Length == 0)
			{
				string text = "This server requires the following mods to be installed and enabled in order to join:";
				string text2 = "\ud83d\udccc Safety Notice<br>";
				text2 += "This server requires mods with executable code (.dll files) that run directly on your computer. Steam does not audit mod code, and neither do we. Only proceed if you trust this server's host and its required mods.";
				PopupMissingModsPopupContent content = uiPopupManager.CreateMissingModsContent(text, text2, reconnectionState2.ClientRequiredModIds);
				uiPopupManager.ShowPopup("missingMods", "MODS REQUIRED", content, showOkButton: true, showCloseButton: true);
			}
			else if ((flag3 | flag2) && reconnectionState2.PendingModIds.Length != 0)
			{
				string text3 = $"<align=center>Downloading & installing {reconnectionState2.PendingModIds.Length} mods...";
				PopupNotificationContent content2 = uiPopupManager.CreateNotificationContent(text3);
				uiPopupManager.ShowPopup("downloadingMods", "MODS REQUIRED", content2, showOkButton: false, showCloseButton: false);
			}
			break;
		}
		}
	}

	private void Event_OnIdentityClickConfirm(Dictionary<string, object> message)
	{
		string value = (string)message["username"];
		int num = (int)message["number"];
		PopupNotificationContent content = uiPopupManager.CreateNotificationContent("<align=center>Identity can be changed once every 24 hours.<br>Are you sure you want to continue?");
		uiPopupManager.ShowPopup("identity", "IDENTITY", content, showOkButton: true, showCloseButton: true, new Dictionary<string, object>
		{
			{ "username", value },
			{ "number", num }
		});
	}

	private void Event_OnMainMenuClickExitGame(Dictionary<string, object> message)
	{
		PopupNotificationContent content = uiPopupManager.CreateNotificationContent("<align=center>Are you sure you want to exit the game?");
		uiPopupManager.ShowPopup("mainMenuExitGame", "EXIT GAME", content, showOkButton: true, showCloseButton: true);
	}

	private void Event_OnPauseMenuClickExitGame(Dictionary<string, object> message)
	{
		PopupNotificationContent content = uiPopupManager.CreateNotificationContent("<align=center>Are you sure you want to exit the game?");
		uiPopupManager.ShowPopup("pauseMenuExitGame", "EXIT GAME", content, showOkButton: true, showCloseButton: true);
	}

	private void Event_OnPlayerBanned(Dictionary<string, object> message)
	{
		string text = (string)message["reason"];
		double num = (double)message["expiresAt"];
		DateTime utcNow = DateTime.UtcNow;
		TimeSpan timeSpan = DateTimeOffset.FromUnixTimeMilliseconds((long)num).DateTime.Subtract(utcNow);
		string text2 = "<align=center>Your account has been banned<br>Banned for " + timeSpan.Humanize(2, CultureInfo.InvariantCulture);
		if (!string.IsNullOrEmpty(text))
		{
			text2 = text2 + "<br><br><align=left>" + text;
		}
		PopupNotificationContent content = uiPopupManager.CreateNotificationContent(text2);
		uiPopupManager.ShowPopup("banned", "BANNED", content, showOkButton: true, showCloseButton: true);
	}

	private void Event_OnPlayerMuted(Dictionary<string, object> message)
	{
		string text = (string)message["reason"];
		double num = (double)message["expiresAt"];
		DateTime utcNow = DateTime.UtcNow;
		TimeSpan timeSpan = DateTimeOffset.FromUnixTimeMilliseconds((long)num).DateTime.Subtract(utcNow);
		string text2 = "<align=center>Your account has been muted<br>Muted for " + timeSpan.Humanize(2, CultureInfo.InvariantCulture);
		if (!string.IsNullOrEmpty(text))
		{
			text2 = text2 + "<br><br><align=left>" + text;
		}
		PopupNotificationContent content = uiPopupManager.CreateNotificationContent(text2);
		uiPopupManager.ShowPopup("muted", "MUTED", content, showOkButton: true, showCloseButton: true);
	}

	private void Event_OnPlayerCooldown(Dictionary<string, object> message)
	{
		double num = (double)message["expiresAt"];
		DateTime utcNow = DateTime.UtcNow;
		TimeSpan timeSpan = DateTimeOffset.FromUnixTimeMilliseconds((long)num).DateTime.Subtract(utcNow);
		string text = "<align=center>Your account has received a matchmaking cooldown<br>Expires in " + timeSpan.Humanize(2, CultureInfo.InvariantCulture);
		PopupNotificationContent content = uiPopupManager.CreateNotificationContent(text);
		uiPopupManager.ShowPopup("cooldown", "COOLDOWN", content, showOkButton: true, showCloseButton: true);
	}

	private void Event_OnSettingsClickResetToDefault(Dictionary<string, object> message)
	{
		PopupNotificationContent content = uiPopupManager.CreateNotificationContent("<align=center>This will reset all settings to their default values, including key binds. Are you sure you want to continue?");
		uiPopupManager.ShowPopup("settingsResetToDefault", "RESET SETTINGS", content, showOkButton: true, showCloseButton: true);
	}

	private void Event_OnPopupClickOk(Dictionary<string, object> message)
	{
		Popup popup = (Popup)message["popup"];
		uiPopupManager.HidePopup(popup.Name);
	}

	private void Event_OnPopupClickClose(Dictionary<string, object> message)
	{
		Popup popup = (Popup)message["popup"];
		uiPopupManager.HidePopup(popup.Name);
	}

	private void Event_OnKeyBindRebindStart(Dictionary<string, object> message)
	{
		if ((bool)message["isComposite"])
		{
			PopupNotificationContent content = uiPopupManager.CreateNotificationContent("<align=center>Press a <b>key</b> or combination of <b>modifier + key</b> to rebind");
			uiPopupManager.ShowPopup("keyBindRebind", "KEY REBIND", content, showOkButton: false, showCloseButton: false);
		}
		else
		{
			PopupNotificationContent content2 = uiPopupManager.CreateNotificationContent("<align=center>Press a <b>key</b> to rebind");
			uiPopupManager.ShowPopup("keyBindRebind", "KEY REBIND", content2, showOkButton: false, showCloseButton: false);
		}
	}

	private void Event_OnKeyBindRebindComplete(Dictionary<string, object> message)
	{
		uiPopupManager.HidePopup("keyBindRebind");
	}

	private void Event_OnKeyBindRebindCancel(Dictionary<string, object> message)
	{
		uiPopupManager.HidePopup("keyBindRebind");
	}
}
