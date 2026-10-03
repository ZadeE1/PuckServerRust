using System.Collections.Generic;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
	private Player player;

	private Tween pingTween;

	private void Awake()
	{
		player = GetComponent<Player>();
	}

	public override void OnNetworkSpawn()
	{
		InputManager.PositionSelectAction.performed += OnPositionSelectActionPerformed;
		EventManager.AddEventListener("Event_OnTeamSelectClickTeam", Event_OnTeamSelectClickTeam);
		EventManager.AddEventListener("Event_OnPositionSelectClickPosition", Event_OnPositionSelectClickPosition);
		EventManager.AddEventListener("Event_OnPauseMenuClickSelectTeam", Event_OnPauseMenuClickSelectTeam);
		EventManager.AddEventListener("Event_OnPauseMenuClickSelectPosition", Event_OnPauseMenuClickSelectPosition);
		EventManager.AddEventListener("Event_OnPauseMenuClickForfeit", Event_OnPauseMenuClickForfeit);
		EventManager.AddEventListener("Event_OnHandednessChanged", Event_OnHandednessChanged);
		EventManager.AddEventListener("Event_Everyone_OnPlayerPositionClaimedByPlayerChanged", Event_Everyone_OnPlayerPositionClaimedByPlayerChanged);
		if (NetworkManager.Singleton.IsServer)
		{
			pingTween = DOVirtual.DelayedCall(1f, () =>
			{
				player.Server_UpdatePing();
			}).SetLoops(-1);
		}
		base.OnNetworkSpawn();
	}

	public override void OnNetworkDespawn()
	{
		InputManager.PositionSelectAction.performed -= OnPositionSelectActionPerformed;
		EventManager.RemoveEventListener("Event_OnTeamSelectClickTeam", Event_OnTeamSelectClickTeam);
		EventManager.RemoveEventListener("Event_OnPositionSelectClickPosition", Event_OnPositionSelectClickPosition);
		EventManager.RemoveEventListener("Event_OnPauseMenuClickSelectTeam", Event_OnPauseMenuClickSelectTeam);
		EventManager.RemoveEventListener("Event_OnPauseMenuClickSelectPosition", Event_OnPauseMenuClickSelectPosition);
		EventManager.RemoveEventListener("Event_OnPauseMenuClickForfeit", Event_OnPauseMenuClickForfeit);
		EventManager.RemoveEventListener("Event_OnHandednessChanged", Event_OnHandednessChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerPositionClaimedByPlayerChanged", Event_Everyone_OnPlayerPositionClaimedByPlayerChanged);
		if (NetworkManager.Singleton.IsServer)
		{
			pingTween?.Kill();
		}
		base.OnNetworkDespawn();
	}

	private void OnPositionSelectActionPerformed(InputAction.CallbackContext context)
	{
		if (GlobalStateManager.UIState.Phase == UIPhase.Playing && !GlobalStateManager.UIState.IsInteracting && player.IsLocalPlayer)
		{
			player.Client_RequestPositionSelectRpc();
		}
	}

	private void Event_OnTeamSelectClickTeam(Dictionary<string, object> message)
	{
		PlayerTeam team = (PlayerTeam)message["team"];
		if (player.IsLocalPlayer)
		{
			player.Client_RequestTeamRpc(team);
		}
	}

	private void Event_OnPositionSelectClickPosition(Dictionary<string, object> message)
	{
		PlayerPosition playerPosition = (PlayerPosition)message["playerPosition"];
		if (player.IsLocalPlayer)
		{
			NetworkObjectReference playerPositionReference = new NetworkObjectReference(playerPosition.NetworkObject);
			player.Client_RequestClaimPositionRpc(playerPositionReference);
		}
	}

	private void Event_OnPauseMenuClickSelectTeam(Dictionary<string, object> message)
	{
		if (player.IsLocalPlayer)
		{
			player.Client_RequestTeamSelectRpc();
		}
	}

	private void Event_OnPauseMenuClickSelectPosition(Dictionary<string, object> message)
	{
		if (player.IsLocalPlayer)
		{
			player.Client_RequestPositionSelectRpc();
		}
	}

	private void Event_OnPauseMenuClickForfeit(Dictionary<string, object> message)
	{
		if (player.IsLocalPlayer)
		{
			player.Client_RequestForfeitRpc();
		}
	}

	private void Event_OnHandednessChanged(Dictionary<string, object> message)
	{
		PlayerHandedness handedness = (PlayerHandedness)message["value"];
		if (player.IsLocalPlayer)
		{
			player.Client_RequestHandednessRpc(handedness);
		}
	}

	private void Event_Everyone_OnPlayerPositionClaimedByPlayerChanged(Dictionary<string, object> message)
	{
		PlayerPosition playerPosition = (PlayerPosition)message["playerPosition"];
		Player player = (Player)message["oldClaimedByPlayer"];
		Player player2 = (Player)message["newClaimedByPlayer"];
		if (NetworkManager.Singleton.IsServer)
		{
			if (player2 == this.player)
			{
				this.player.PlayerPositionReference.Value = new NetworkObjectReference(playerPosition.NetworkObject);
			}
			else if (player == this.player && playerPosition == this.player.PlayerPosition)
			{
				this.player.PlayerPositionReference.Value = default;
			}
		}
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected override string __getTypeName()
	{
		return "PlayerController";
	}
}
