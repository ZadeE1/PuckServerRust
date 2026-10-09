using System.Collections.Generic;
using Unity.Netcode;

internal class UIOverlayManagerController : UIViewController<UIOverlayManager>
{
	private UIOverlayManager uiOverlay;

	public override void Awake()
	{
		base.Awake();
		uiOverlay = GetComponent<UIOverlayManager>();
		EventManager.AddEventListener("Event_Everyone_OnClientConnected", Event_Everyone_OnClientConnected);
		EventManager.AddEventListener("Event_Everyone_OnGoalScored", Event_Everyone_OnGoalScored);
		EventManager.AddEventListener("Event_OnBaseCameraEnabled", Event_OnBaseCameraEnabled);
		EventManager.AddEventListener("Event_OnBaseCameraDisabled", Event_OnBaseCameraDisabled);
		EventManager.AddEventListener("Event_OnClientStarted", Event_OnClientStarted);
		EventManager.AddEventListener("Event_OnClientStopped", Event_OnClientStopped);
		EventManager.AddEventListener("Event_OnPopupShow", Event_OnPopupShow);
		EventManager.AddEventListener("Event_OnPopupHide", Event_OnPopupHide);
		WebSocketManager.AddMessageListener("playerData", WebSocket_Event_OnPlayerData);
	}

	private void Start()
	{
		uiOverlay.ShowOverlay("loading", requiresSpinner: true, fadeIn: false, fadeOut: true);
	}

	public override void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnClientConnected", Event_Everyone_OnClientConnected);
		EventManager.RemoveEventListener("Event_Everyone_OnGoalScored", Event_Everyone_OnGoalScored);
		EventManager.RemoveEventListener("Event_OnBaseCameraEnabled", Event_OnBaseCameraEnabled);
		EventManager.RemoveEventListener("Event_OnBaseCameraDisabled", Event_OnBaseCameraDisabled);
		EventManager.RemoveEventListener("Event_OnClientStarted", Event_OnClientStarted);
		EventManager.RemoveEventListener("Event_OnClientStopped", Event_OnClientStopped);
		EventManager.RemoveEventListener("Event_OnPopupShow", Event_OnPopupShow);
		EventManager.RemoveEventListener("Event_OnPopupHide", Event_OnPopupHide);
		WebSocketManager.RemoveMessageListener("playerData", WebSocket_Event_OnPlayerData);
		base.OnDestroy();
	}

	private void Event_Everyone_OnClientConnected(Dictionary<string, object> message)
	{
		ulong num = (ulong)message["clientId"];
		if (NetworkManager.Singleton.LocalClientId == num)
		{
			uiOverlay.HideOverlay("connecting");
		}
	}

	private void Event_Everyone_OnGoalScored(Dictionary<string, object> message)
	{
		PlayerTeam team = (PlayerTeam)message["byTeam"];
		uiOverlay.FlashScreen(Utils.GetTeamColor(team), 0.4f, 0.08f, 0.28f, 0.42f);
	}

	private void Event_OnBaseCameraEnabled(Dictionary<string, object> message)
	{
		uiOverlay.HideOverlay("camera");
	}

	private void Event_OnBaseCameraDisabled(Dictionary<string, object> message)
	{
		uiOverlay.ShowOverlay("camera", requiresSpinner: false, fadeIn: false, fadeOut: true);
	}

	private void Event_OnClientStarted(Dictionary<string, object> message)
	{
		uiOverlay.ShowOverlay("connecting", requiresSpinner: true, fadeIn: false, fadeOut: true);
	}

	private void Event_OnClientStopped(Dictionary<string, object> message)
	{
		uiOverlay.HideOverlay("connecting");
	}

	private void Event_OnPopupShow(Dictionary<string, object> message)
	{
		switch ((string)message["name"])
		{
		case "missingPassword":
			uiOverlay.ShowOverlay("missingPassword", requiresSpinner: false, fadeIn: false, fadeOut: true);
			break;
		case "missingMods":
			uiOverlay.ShowOverlay("missingMods", requiresSpinner: false, fadeIn: false, fadeOut: true);
			break;
		case "downloadingMods":
			uiOverlay.ShowOverlay("downloadingMods", requiresSpinner: true, fadeIn: false, fadeOut: true);
			break;
		}
	}

	private void Event_OnPopupHide(Dictionary<string, object> message)
	{
		switch ((string)message["name"])
		{
		case "missingPassword":
			uiOverlay.HideOverlay("missingPassword");
			break;
		case "missingMods":
			uiOverlay.HideOverlay("missingMods");
			break;
		case "downloadingMods":
			uiOverlay.HideOverlay("downloadingMods");
			break;
		}
	}

	private void WebSocket_Event_OnPlayerData(Dictionary<string, object> message)
	{
		uiOverlay.HideOverlay("loading");
	}
}
