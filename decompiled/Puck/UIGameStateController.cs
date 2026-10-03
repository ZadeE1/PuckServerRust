using System.Collections.Generic;

public class UIGameStateController : UIViewController<UIGameState>
{
	private UIGameState uiGameState;

	private int RegulationPeriods
	{
		get
		{
			if (!(NetworkBehaviourSingleton<GameModeManager>.Instance != null) || NetworkBehaviourSingleton<GameModeManager>.Instance.ClientConfig.Value.MaxPeriods <= 0)
			{
				return 3;
			}
			return NetworkBehaviourSingleton<GameModeManager>.Instance.ClientConfig.Value.MaxPeriods;
		}
	}

	public override void Awake()
	{
		base.Awake();
		uiGameState = GetComponent<UIGameState>();
		EventManager.AddEventListener("Event_Everyone_OnGameStateChanged", Event_Everyone_OnGameStateChanged);
		EventManager.AddEventListener("Event_Everyone_OnGameModeClientConfigChanged", Event_Everyone_OnGameModeClientConfigChanged);
		EventManager.AddEventListener("Event_OnShowGameUserInterfaceChanged", Event_OnShowGameUserInterfaceChanged);
	}

	public override void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnGameStateChanged", Event_Everyone_OnGameStateChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnGameModeClientConfigChanged", Event_Everyone_OnGameModeClientConfigChanged);
		EventManager.RemoveEventListener("Event_OnShowGameUserInterfaceChanged", Event_OnShowGameUserInterfaceChanged);
		base.OnDestroy();
	}

	private void SetPhase(GameState gameState)
	{
		uiGameState.SetPhase(Utils.GetHumanizedGamePhase(gameState.Phase, gameState.Period, gameState.IsOvertime, RegulationPeriods));
	}

	private void Event_Everyone_OnGameStateChanged(Dictionary<string, object> message)
	{
		GameState phase = (GameState)message["newGameState"];
		SetPhase(phase);
		uiGameState.SetTick(phase.Tick);
		uiGameState.SetScore(PlayerTeam.Blue, phase.BlueScore);
		uiGameState.SetScore(PlayerTeam.Red, phase.RedScore);
	}

	private void Event_Everyone_OnGameModeClientConfigChanged(Dictionary<string, object> message)
	{
		if (!(NetworkBehaviourSingleton<GameManager>.Instance == null) && NetworkBehaviourSingleton<GameManager>.Instance.GameState != null)
		{
			SetPhase(NetworkBehaviourSingleton<GameManager>.Instance.GameState.Value);
		}
	}

	private void Event_OnShowGameUserInterfaceChanged(Dictionary<string, object> message)
	{
		if (GlobalStateManager.UIState.Phase != UIPhase.LockerRoom)
		{
			if ((bool)message["value"])
			{
				uiGameState.Show();
			}
			else
			{
				uiGameState.Hide();
			}
		}
	}
}
