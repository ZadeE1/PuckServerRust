using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CreaseGhostView))]
public class CreaseGhostViewController : MonoBehaviour
{
	private static readonly HashSet<string> CreaseZoneIds = new HashSet<string> { "Crease" };

	private PlayerBody playerBody;

	private CreaseGhostView creaseGhostView;

	private bool IsLocalGoalie
	{
		get
		{
			if (playerBody != null && playerBody.IsOwner && playerBody.Player != null)
			{
				return playerBody.Player.Role == PlayerRole.Goalie;
			}
			return false;
		}
	}

	private bool IsCreaseProtectionEnabled
	{
		get
		{
			if (NetworkBehaviourSingleton<GameModeManager>.Instance != null)
			{
				return NetworkBehaviourSingleton<GameModeManager>.Instance.ClientConfig.Value.GoalieCreaseProtection;
			}
			return false;
		}
	}

	private void Awake()
	{
		playerBody = GetComponent<PlayerBody>();
		creaseGhostView = GetComponent<CreaseGhostView>();
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			EventManager.AddEventListener("Event_Everyone_OnPlayerEnterZone", Event_Everyone_OnPlayerEnterZone);
			EventManager.AddEventListener("Event_Everyone_OnPlayerExitZone", Event_Everyone_OnPlayerExitZone);
			EventManager.AddEventListener("Event_Everyone_OnPlayerBodyDespawned", Event_Everyone_OnPlayerBodyDespawned);
		}
	}

	private void OnDestroy()
	{
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			EventManager.RemoveEventListener("Event_Everyone_OnPlayerEnterZone", Event_Everyone_OnPlayerEnterZone);
			EventManager.RemoveEventListener("Event_Everyone_OnPlayerExitZone", Event_Everyone_OnPlayerExitZone);
			EventManager.RemoveEventListener("Event_Everyone_OnPlayerBodyDespawned", Event_Everyone_OnPlayerBodyDespawned);
			creaseGhostView.ResetAll();
		}
	}

	private void Event_Everyone_OnPlayerEnterZone(Dictionary<string, object> message)
	{
		Zone zone = (Zone)message["zone"];
		if (!CreaseZoneIds.Contains(zone.ZoneId))
		{
			return;
		}
		PlayerBody playerBody = (PlayerBody)message["playerBody"];
		if (!IsLocalGoalie || !IsCreaseProtectionEnabled)
		{
			return;
		}
		if (playerBody == this.playerBody)
		{
			Goal componentInParent = zone.GetComponentInParent<Goal>();
			if (componentInParent == null || componentInParent.Team != this.playerBody.Player.Team)
			{
				return;
			}
			creaseGhostView.EnterCrease(zone);
			{
				foreach (PlayerBody occupant in zone.Occupants)
				{
					if (IsEnemy(occupant))
					{
						creaseGhostView.AddEnemy(occupant);
					}
				}
				return;
			}
		}
		if (zone == creaseGhostView.CreaseZone && IsEnemy(playerBody))
		{
			creaseGhostView.AddEnemy(playerBody);
		}
	}

	private void Event_Everyone_OnPlayerExitZone(Dictionary<string, object> message)
	{
		if (!((Zone)message["zone"] != creaseGhostView.CreaseZone))
		{
			PlayerBody playerBody = (PlayerBody)message["playerBody"];
			if (playerBody == this.playerBody)
			{
				creaseGhostView.ExitCrease();
			}
			else
			{
				creaseGhostView.RemoveEnemy(playerBody);
			}
		}
	}

	private void Event_Everyone_OnPlayerBodyDespawned(Dictionary<string, object> message)
	{
		PlayerBody enemy = (PlayerBody)message["playerBody"];
		creaseGhostView.ForgetEnemy(enemy);
	}

	private bool IsEnemy(PlayerBody body)
	{
		if (body == null || body.Player == null || playerBody.Player == null)
		{
			return false;
		}
		PlayerTeam team = body.Player.Team;
		if (body.Player.Role != PlayerRole.Goalie && (team == PlayerTeam.Blue || team == PlayerTeam.Red))
		{
			return team != playerBody.Player.Team;
		}
		return false;
	}
}
