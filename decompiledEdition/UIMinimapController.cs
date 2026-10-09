using System.Collections.Generic;
using UnityEngine;

internal class UIMinimapController : UIViewController<UIMinimap>
{
	private UIMinimap uiMinimap;

	public override void Awake()
	{
		base.Awake();
		uiMinimap = GetComponent<UIMinimap>();
		EventManager.AddEventListener("Event_Everyone_OnLevelSpawned", Event_Everyone_OnLevelSpawned);
		EventManager.AddEventListener("Event_Everyone_OnPlayerBodySpawned", Event_Everyone_OnPlayerBodySpawned);
		EventManager.AddEventListener("Event_Everyone_OnPlayerSpawned", Event_Everyone_OnPlayerSpawned);
		EventManager.AddEventListener("Event_Everyone_OnPlayerGameStateChanged", Event_Everyone_OnPlayerGameStateChanged);
		EventManager.AddEventListener("Event_Everyone_OnPlayerNumberChanged", Event_Everyone_OnPlayerNumberChanged);
		EventManager.AddEventListener("Event_Everyone_OnPlayerBodyDespawned", Event_Everyone_OnPlayerBodyDespawned);
		EventManager.AddEventListener("Event_Everyone_OnPuckSpawned", Event_Everyone_OnPuckSpawned);
		EventManager.AddEventListener("Event_Everyone_OnPuckDespawned", Event_Everyone_OnPuckDespawned);
		EventManager.AddEventListener("Event_Everyone_OnStickSpawned", Event_Everyone_OnStickSpawned);
		EventManager.AddEventListener("Event_Everyone_OnStickDespawned", Event_Everyone_OnStickDespawned);
		EventManager.AddEventListener("Event_OnShowMinimapChanged", Event_OnShowMinimapChanged);
		EventManager.AddEventListener("Event_OnMinimapOpacityChanged", Event_OnMinimapOpacityChanged);
		EventManager.AddEventListener("Event_OnShowGameUserInterfaceChanged", Event_OnShowGameUserInterfaceChanged);
		EventManager.AddEventListener("Event_OnMinimapHorizontalPositionChanged", Event_OnMinimapHorizontalPositionChanged);
		EventManager.AddEventListener("Event_OnMinimapVerticalPositionChanged", Event_OnMinimapVerticalPositionChanged);
		EventManager.AddEventListener("Event_OnMinimapBackgroundOpacityChanged", Event_OnMinimapBackgroundOpacityChanged);
		EventManager.AddEventListener("Event_OnMinimapScaleChanged", Event_OnMinimapScaleChanged);
	}

	private void Start()
	{
		uiMinimap.SetOpacity(SettingsManager.MinimapOpacity);
		uiMinimap.SetBackgroundOpacity(SettingsManager.MinimapBackgroundOpacity);
		uiMinimap.SetPosition(new Vector2(SettingsManager.MinimapHorizontalPosition, SettingsManager.MinimapVerticalPosition));
		uiMinimap.SetScale(SettingsManager.MinimapScale);
	}

	public override void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnLevelSpawned", Event_Everyone_OnLevelSpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerBodySpawned", Event_Everyone_OnPlayerBodySpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerSpawned", Event_Everyone_OnPlayerSpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerGameStateChanged", Event_Everyone_OnPlayerGameStateChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerNumberChanged", Event_Everyone_OnPlayerNumberChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerBodyDespawned", Event_Everyone_OnPlayerBodyDespawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPuckSpawned", Event_Everyone_OnPuckSpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPuckDespawned", Event_Everyone_OnPuckDespawned);
		EventManager.RemoveEventListener("Event_Everyone_OnStickSpawned", Event_Everyone_OnStickSpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnStickDespawned", Event_Everyone_OnStickDespawned);
		EventManager.RemoveEventListener("Event_OnShowMinimapChanged", Event_OnShowMinimapChanged);
		EventManager.RemoveEventListener("Event_OnMinimapOpacityChanged", Event_OnMinimapOpacityChanged);
		EventManager.RemoveEventListener("Event_OnShowGameUserInterfaceChanged", Event_OnShowGameUserInterfaceChanged);
		EventManager.RemoveEventListener("Event_OnMinimapHorizontalPositionChanged", Event_OnMinimapHorizontalPositionChanged);
		EventManager.RemoveEventListener("Event_OnMinimapVerticalPositionChanged", Event_OnMinimapVerticalPositionChanged);
		EventManager.RemoveEventListener("Event_OnMinimapBackgroundOpacityChanged", Event_OnMinimapBackgroundOpacityChanged);
		EventManager.RemoveEventListener("Event_OnMinimapScaleChanged", Event_OnMinimapScaleChanged);
		base.OnDestroy();
	}

	private void HandlePlayerGameState(Player player)
	{
		PlayerGameState value = player.GameState.Value;
		uiMinimap.Team = value.Team;
	}

	private void Event_Everyone_OnLevelSpawned(Dictionary<string, object> message)
	{
		Level level = (Level)message["level"];
		uiMinimap.Bounds = level.Bounds;
	}

	private void Event_Everyone_OnPlayerBodySpawned(Dictionary<string, object> message)
	{
		PlayerBody playerBody = (PlayerBody)message["playerBody"];
		uiMinimap.AddPlayerBody(playerBody);
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
		if (player.IsLocalPlayer)
		{
			HandlePlayerGameState(player);
		}
		uiMinimap.StylePlayer(player.PlayerBody);
	}

	private void Event_Everyone_OnPlayerNumberChanged(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		uiMinimap.StylePlayer(player.PlayerBody);
	}

	private void Event_Everyone_OnPlayerBodyDespawned(Dictionary<string, object> message)
	{
		PlayerBody playerBody = (PlayerBody)message["playerBody"];
		uiMinimap.RemovePlayerBody(playerBody);
	}

	private void Event_Everyone_OnPuckSpawned(Dictionary<string, object> message)
	{
		Puck puck = (Puck)message["puck"];
		uiMinimap.AddPuck(puck);
	}

	private void Event_Everyone_OnPuckDespawned(Dictionary<string, object> message)
	{
		Puck puck = (Puck)message["puck"];
		uiMinimap.RemovePuck(puck);
	}

	private void Event_Everyone_OnStickSpawned(Dictionary<string, object> message)
	{
		Stick stick = (Stick)message["stick"];
		uiMinimap.AddStick(stick);
	}

	private void Event_Everyone_OnStickDespawned(Dictionary<string, object> message)
	{
		Stick stick = (Stick)message["stick"];
		uiMinimap.RemoveStick(stick);
	}

	private void Event_OnMinimapOpacityChanged(Dictionary<string, object> message)
	{
		float opacity = (float)message["value"];
		uiMinimap.SetOpacity(opacity);
	}

	private void Event_OnShowMinimapChanged(Dictionary<string, object> message)
	{
		if (GlobalStateManager.UIState.Phase != UIPhase.LockerRoom)
		{
			if ((bool)message["value"])
			{
				uiMinimap.Show();
			}
			else
			{
				uiMinimap.Hide();
			}
		}
	}

	private void Event_OnShowGameUserInterfaceChanged(Dictionary<string, object> message)
	{
		if (GlobalStateManager.UIState.Phase != UIPhase.LockerRoom)
		{
			if ((bool)message["value"])
			{
				uiMinimap.Show();
			}
			else
			{
				uiMinimap.Hide();
			}
		}
	}

	private void Event_OnMinimapHorizontalPositionChanged(Dictionary<string, object> message)
	{
		float x = (float)message["value"];
		uiMinimap.SetPosition(new Vector2(x, uiMinimap.Position.y));
	}

	private void Event_OnMinimapVerticalPositionChanged(Dictionary<string, object> message)
	{
		float y = (float)message["value"];
		uiMinimap.SetPosition(new Vector2(uiMinimap.Position.x, y));
	}

	private void Event_OnMinimapBackgroundOpacityChanged(Dictionary<string, object> message)
	{
		float backgroundOpacity = (float)message["value"];
		uiMinimap.SetBackgroundOpacity(backgroundOpacity);
	}

	private void Event_OnMinimapScaleChanged(Dictionary<string, object> message)
	{
		float scale = (float)message["value"];
		uiMinimap.SetScale(scale);
	}
}
