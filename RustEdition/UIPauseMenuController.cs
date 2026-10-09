using System.Collections.Generic;
using DG.Tweening;

public class UIPauseMenuController : UIViewController<UIPauseMenu>
{
	private const float forfeitVotedResetDelay = 5f;

	private UIPauseMenu uiPauseMenu;

	private Player localPlayer;

	private GamePhase phase;

	private bool forfeitVoted;

	private Tween forfeitVotedResetTween;

	public override void Awake()
	{
		base.Awake();
		uiPauseMenu = GetComponent<UIPauseMenu>();
		EventManager.AddEventListener("Event_Everyone_OnPlayerSpawned", Event_Everyone_OnPlayerSpawned);
		EventManager.AddEventListener("Event_Everyone_OnGameStateChanged", Event_Everyone_OnGameStateChanged);
		EventManager.AddEventListener("Event_Everyone_OnPlayerGameStateChanged", Event_Everyone_OnPlayerGameStateChanged);
		EventManager.AddEventListener("Event_Everyone_OnServerChanged", Event_Everyone_OnServerChanged);
		EventManager.AddEventListener("Event_OnPauseMenuClickForfeit", Event_OnPauseMenuClickForfeit);
	}

	public override void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerSpawned", Event_Everyone_OnPlayerSpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnGameStateChanged", Event_Everyone_OnGameStateChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerGameStateChanged", Event_Everyone_OnPlayerGameStateChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnServerChanged", Event_Everyone_OnServerChanged);
		EventManager.RemoveEventListener("Event_OnPauseMenuClickForfeit", Event_OnPauseMenuClickForfeit);
		forfeitVotedResetTween?.Kill();
		base.OnDestroy();
	}

	private void Event_Everyone_OnPlayerSpawned(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		if (player.IsLocalPlayer)
		{
			localPlayer = player;
			RefreshButtons();
		}
	}

	private void Event_Everyone_OnGameStateChanged(Dictionary<string, object> message)
	{
		phase = ((GameState)message["newGameState"]).Phase;
		RefreshButtons();
	}

	private void Event_Everyone_OnPlayerGameStateChanged(Dictionary<string, object> message)
	{
		if (!((Player)message["player"] != localPlayer))
		{
			RefreshButtons();
		}
	}

	private void Event_Everyone_OnServerChanged(Dictionary<string, object> message)
	{
		RefreshButtons();
	}

	private void Event_OnPauseMenuClickForfeit(Dictionary<string, object> message)
	{
		forfeitVoted = true;
		uiPauseMenu.SetForfeitVoted(value: true);
		RefreshButtons();
		forfeitVotedResetTween?.Kill();
		forfeitVotedResetTween = DOVirtual.DelayedCall(5f, () =>
		{
			forfeitVoted = false;
			uiPauseMenu.SetForfeitVoted(value: false);
			RefreshButtons();
		});
	}

	private void RefreshButtons()
	{
		bool flag = localPlayer != null && (localPlayer.Team == PlayerTeam.Blue || localPlayer.Team == PlayerTeam.Red);
		int num;
		if (NetworkBehaviourSingleton<ServerManager>.Instance != null)
		{
			Server value = NetworkBehaviourSingleton<ServerManager>.Instance.Server.Value;
			num = ((value.GameMode == "competitive") ? 1 : 0);
		}
		else
		{
			num = 0;
		}
		bool flag2 = (byte)num != 0;
		bool voiceEnabled = NetworkBehaviourSingleton<ServerManager>.Instance != null && NetworkBehaviourSingleton<ServerManager>.Instance.Server.Value.UseVoip;
		int tickRate = ((NetworkBehaviourSingleton<ServerManager>.Instance != null) ? NetworkBehaviourSingleton<ServerManager>.Instance.Server.Value.TickRate : 0);
		uiPauseMenu.SetSelectPositionEnabled(flag);
		uiPauseMenu.SetForfeitEnabled((Utils.IsGameInProgress(phase) & flag) && !forfeitVoted);
		uiPauseMenu.SetSelectTeamEnabled(!flag2);
		uiPauseMenu.SetVoiceEnabled(voiceEnabled);
		uiPauseMenu.SetTickRate(tickRate);
	}
}
