using System.Collections.Generic;
using Unity.Netcode;

public class CreaseProtection
{
	private class Crease
	{
		public PlayerTeam DefendingTeam;

		public PlayerBody Goalie;

		public readonly HashSet<PlayerBody> Enemies = new HashSet<PlayerBody>();

		public PlayerBody PhasedGoalie;

		public readonly HashSet<PlayerBody> Filtered = new HashSet<PlayerBody>();
	}

	private static readonly Logger Logger = new Logger("CreaseProtection");

	private static readonly HashSet<string> CreaseZoneIds = new HashSet<string> { "Crease" };

	private readonly Dictionary<Zone, Crease> creases = new Dictionary<Zone, Crease>();

	private readonly List<PlayerBody> restoreScratch = new List<PlayerBody>();

	private static bool IsServer
	{
		get
		{
			if (NetworkManager.Singleton != null)
			{
				return NetworkManager.Singleton.IsServer;
			}
			return false;
		}
	}

	public CreaseProtection()
	{
		EventManager.AddEventListener("Event_Everyone_OnPlayerBodyDespawned", OnPlayerBodyDespawned);
	}

	public void Dispose()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerBodyDespawned", OnPlayerBodyDespawned);
		foreach (Crease value in creases.Values)
		{
			value.Goalie = null;
			value.Enemies.Clear();
			Reconcile(value);
		}
		creases.Clear();
	}

	public void OnPlayerEnterZone(Zone zone, PlayerBody playerBody)
	{
		if (!IsServer || !CreaseZoneIds.Contains(zone.ZoneId))
		{
			return;
		}
		Crease orCreateCrease = GetOrCreateCrease(zone);
		if (orCreateCrease != null)
		{
			if (IsDefendingGoalie(playerBody, orCreateCrease.DefendingTeam))
			{
				orCreateCrease.Goalie = playerBody;
			}
			else if (IsEnemySkater(playerBody, orCreateCrease.DefendingTeam))
			{
				orCreateCrease.Enemies.Add(playerBody);
			}
			Reconcile(orCreateCrease);
		}
	}

	public void OnPlayerExitZone(Zone zone, PlayerBody playerBody)
	{
		if (IsServer && creases.TryGetValue(zone, out var value))
		{
			if (value.Goalie == playerBody)
			{
				value.Goalie = null;
			}
			else
			{
				value.Enemies.Remove(playerBody);
			}
			Reconcile(value);
		}
	}

	private void OnPlayerBodyDespawned(Dictionary<string, object> message)
	{
		if (!IsServer)
		{
			return;
		}
		PlayerBody playerBody = (PlayerBody)message["playerBody"];
		foreach (Crease value in creases.Values)
		{
			bool flag = value.Enemies.Remove(playerBody);
			if (value.Goalie == playerBody)
			{
				value.Goalie = null;
				flag = true;
			}
			if (flag)
			{
				Reconcile(value);
			}
		}
	}

	private void Reconcile(Crease crease)
	{
		restoreScratch.Clear();
		foreach (PlayerBody item in crease.Filtered)
		{
			if (crease.PhasedGoalie != crease.Goalie || crease.Goalie == null || !crease.Enemies.Contains(item))
			{
				restoreScratch.Add(item);
			}
		}
		foreach (PlayerBody item2 in restoreScratch)
		{
			if (crease.PhasedGoalie != null)
			{
				crease.PhasedGoalie.SetCollisionFilteredWith(item2, filtered: false);
			}
			crease.Filtered.Remove(item2);
		}
		crease.PhasedGoalie = crease.Goalie;
		if (crease.Goalie == null)
		{
			return;
		}
		foreach (PlayerBody enemy in crease.Enemies)
		{
			if (crease.Filtered.Add(enemy))
			{
				crease.Goalie.SetCollisionFilteredWith(enemy, filtered: true);
			}
		}
	}

	private Crease GetOrCreateCrease(Zone zone)
	{
		if (creases.TryGetValue(zone, out var value))
		{
			return value;
		}
		Goal componentInParent = zone.GetComponentInParent<Goal>();
		if (componentInParent == null)
		{
			Logger.Warning("Crease zone '" + zone.name + "' is not under a Goal — can't resolve a defending team.");
			return null;
		}
		value = new Crease
		{
			DefendingTeam = componentInParent.Team
		};
		creases[zone] = value;
		return value;
	}

	private static bool IsDefendingGoalie(PlayerBody playerBody, PlayerTeam defendingTeam)
	{
		if (playerBody != null && playerBody.Player != null && playerBody.Player.Role == PlayerRole.Goalie)
		{
			return playerBody.Player.Team == defendingTeam;
		}
		return false;
	}

	private static bool IsEnemySkater(PlayerBody playerBody, PlayerTeam defendingTeam)
	{
		if (playerBody == null || playerBody.Player == null)
		{
			return false;
		}
		PlayerTeam team = playerBody.Player.Team;
		if (playerBody.Player.Role != PlayerRole.Goalie && (team == PlayerTeam.Blue || team == PlayerTeam.Red))
		{
			return team != defendingTeam;
		}
		return false;
	}
}
