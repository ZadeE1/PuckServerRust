using System.Collections.Generic;

public static class GlobalStateManager
{
	private static readonly Logger Logger = new Logger("GlobalStateManager");

	private static UIState uiState = new UIState();

	private static ConnectionState connectionState = new ConnectionState();

	private static ReconnectionState reconnectionState = new ReconnectionState();

	public static UIState UIState
	{
		get
		{
			return uiState;
		}
		set
		{
			if (!uiState.Equals(value))
			{
				UIState oldUIState = uiState;
				uiState = value;
				OnUIStateChanged(oldUIState, uiState);
			}
		}
	}

	public static ConnectionState ConnectionState
	{
		get
		{
			return connectionState;
		}
		set
		{
			if (!connectionState.Equals(value))
			{
				ConnectionState oldConnectionState = connectionState;
				connectionState = value;
				OnConnectionStateChanged(oldConnectionState, connectionState);
			}
		}
	}

	public static ReconnectionState ReconnectionState
	{
		get
		{
			return reconnectionState;
		}
		set
		{
			if (!reconnectionState.Equals(value))
			{
				ReconnectionState oldReconnectionState = reconnectionState;
				reconnectionState = value;
				OnReconnectionStateChanged(oldReconnectionState, reconnectionState);
			}
		}
	}

	public static void Initialize()
	{
		GlobalStateManagerController.Initialize();
	}

	public static void Dispose()
	{
		GlobalStateManagerController.Dispose();
	}

	public static void SetUIState(Dictionary<string, object> updates)
	{
		UIState uIState = new UIState();
		uIState.Phase = (updates.ContainsKey("phase") ? ((UIPhase)updates["phase"]) : UIState.Phase);
		uIState.IsMouseRequired = (updates.ContainsKey("isMouseRequired") ? ((bool)updates["isMouseRequired"]) : UIState.IsMouseRequired);
		uIState.IsMouseOverUI = (updates.ContainsKey("isMouseOverUI") ? ((bool)updates["isMouseOverUI"]) : UIState.IsMouseOverUI);
		uIState.InteractingViews = (updates.ContainsKey("interactingViews") ? ((List<UIView>)updates["interactingViews"]) : UIState.InteractingViews);
		UIState = uIState;
	}

	public static void ClearUIState()
	{
		UIState = new UIState();
	}

	public static void SetConnectionState(Dictionary<string, object> updates)
	{
		ConnectionState connectionState = new ConnectionState();
		connectionState.Connection = (updates.ContainsKey("connection") ? ((Connection)updates["connection"]) : ConnectionState.Connection);
		connectionState.LastConnection = (updates.ContainsKey("lastConnection") ? ((Connection)updates["lastConnection"]) : ConnectionState.LastConnection);
		connectionState.ConnectionRejection = (updates.ContainsKey("connectionRejection") ? ((ConnectionRejection)updates["connectionRejection"]) : ConnectionState.ConnectionRejection);
		connectionState.Disconnection = (updates.ContainsKey("disconnection") ? ((Disconnection)updates["disconnection"]) : ConnectionState.Disconnection);
		connectionState.PendingConnection = (updates.ContainsKey("pendingConnection") ? ((Connection)updates["pendingConnection"]) : ConnectionState.PendingConnection);
		connectionState.Phase = (updates.ContainsKey("phase") ? ((ConnectionPhase)updates["phase"]) : ConnectionState.Phase);
		ConnectionState = connectionState;
	}

	public static void ClearConnectionState()
	{
		ConnectionState = new ConnectionState();
	}

	public static void SetReconnectionState(Dictionary<string, object> updates)
	{
		ReconnectionState reconnectionState = new ReconnectionState();
		reconnectionState.Phase = (updates.ContainsKey("phase") ? ((ReconnectionPhase)updates["phase"]) : ReconnectionState.Phase);
		reconnectionState.Password = (updates.ContainsKey("password") ? ((string)updates["password"]) : ReconnectionState.Password);
		reconnectionState.ClientRequiredModIds = (updates.ContainsKey("clientRequiredModIds") ? ((string[])updates["clientRequiredModIds"]) : ReconnectionState.ClientRequiredModIds);
		reconnectionState.PendingReadinessModIds = (updates.ContainsKey("pendingReadinessModIds") ? ((string[])updates["pendingReadinessModIds"]) : ReconnectionState.PendingReadinessModIds);
		reconnectionState.PendingEnablingModIds = (updates.ContainsKey("pendingEnablingModIds") ? ((string[])updates["pendingEnablingModIds"]) : ReconnectionState.PendingEnablingModIds);
		ReconnectionState = reconnectionState;
	}

	public static void ClearReconnectionState()
	{
		ReconnectionState = new ReconnectionState();
	}

	private static void OnUIStateChanged(UIState oldUIState, UIState newUIState)
	{
		EventManager.TriggerEvent("Event_OnUIStateChanged", new Dictionary<string, object>
		{
			{ "oldUIState", oldUIState },
			{ "newUIState", newUIState }
		});
	}

	private static void OnConnectionStateChanged(ConnectionState oldConnectionState, ConnectionState newConnectionState)
	{
		EventManager.TriggerEvent("Event_OnConnectionStateChanged", new Dictionary<string, object>
		{
			{ "oldConnectionState", oldConnectionState },
			{ "newConnectionState", newConnectionState }
		});
	}

	private static void OnReconnectionStateChanged(ReconnectionState oldReconnectionState, ReconnectionState newReconnectionState)
	{
		Logger.Info($"Reconnection state changed \nFrom:{{{oldReconnectionState}}} \nTo:{{{newReconnectionState}}}");
		EventManager.TriggerEvent("Event_OnReconnectionStateChanged", new Dictionary<string, object>
		{
			{ "oldReconnectionState", oldReconnectionState },
			{ "newReconnectionState", newReconnectionState }
		});
	}
}
