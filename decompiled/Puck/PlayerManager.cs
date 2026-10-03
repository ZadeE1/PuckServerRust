using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : MonoBehaviourSingleton<PlayerManager>
{
	private static readonly Logger Logger = new Logger("PlayerManager");

	[Header("Prefabs")]
	[SerializeField]
	private Player playerPrefab;

	private List<Player> players = new List<Player>();

	private List<Player> nonReplayPlayers = new List<Player>();

	private Player localPlayer;

	public void AddPlayer(Player player)
	{
		players.Add(player);
		if (!player.IsReplay.Value)
		{
			nonReplayPlayers.Add(player);
		}
		EventManager.TriggerEvent("Event_Everyone_OnPlayerAdded", new Dictionary<string, object> { { "player", player } });
	}

	public void RemovePlayer(Player player)
	{
		players.Remove(player);
		nonReplayPlayers.Remove(player);
		if (localPlayer == player)
		{
			localPlayer = null;
		}
		EventManager.TriggerEvent("Event_Everyone_OnPlayerRemoved", new Dictionary<string, object> { { "player", player } });
	}

	public List<Player> GetPlayers(bool includeReplay = false)
	{
		RemoveStalePlayers();
		if (includeReplay)
		{
			return players;
		}
		return nonReplayPlayers;
	}

	private void RemoveStalePlayers()
	{
		for (int i = 0; i < players.Count; i++)
		{
			Player player = players[i];
			if (!player || !player.NetworkObject.IsSpawned)
			{
				RebuildPlayerLists();
				break;
			}
		}
	}

	private void RebuildPlayerLists()
	{
		List<Player> list = new List<Player>(players.Count);
		List<Player> list2 = new List<Player>(nonReplayPlayers.Count);
		foreach (Player player in players)
		{
			if ((bool)player && player.NetworkObject.IsSpawned)
			{
				list.Add(player);
				if (!player.IsReplay.Value)
				{
					list2.Add(player);
				}
			}
		}
		players = list;
		nonReplayPlayers = list2;
	}

	public List<Player> GetPlayersByPhase(PlayerPhase phase, bool includeReplay = false)
	{
		return (from player in GetPlayers(includeReplay)
			where player.Phase == phase
			select player).ToList();
	}

	public List<Player> GetPlayersByPhases(PlayerPhase[] phases, bool includeReplay = false)
	{
		return (from player in GetPlayers(includeReplay)
			where phases.Contains(player.Phase)
			select player).ToList();
	}

	public List<Player> GetPlayersByTeam(PlayerTeam team, bool includeReplay = false)
	{
		return (from player in GetPlayers(includeReplay)
			where player.Team == team
			select player).ToList();
	}

	public List<Player> GetPlayersByTeams(PlayerTeam[] team, bool includeReplay = false)
	{
		return (from player in GetPlayers(includeReplay)
			where team.Contains(player.Team)
			select player).ToList();
	}

	public Player GetPlayerByClientId(ulong clientId)
	{
		return GetPlayers().Find((Player player) => player.OwnerClientId == clientId);
	}

	public Player GetPlayerByUsername(FixedString32Bytes username, bool caseSensitive = false)
	{
		return GetPlayers().Find((Player player) => (caseSensitive ? player.Username.Value.ToString() : player.Username.Value.ToString().ToLower()) == (caseSensitive ? username.ToString() : username.ToString().ToLower()));
	}

	public Player GetPlayerByNumber(int number)
	{
		return GetPlayers().Find((Player player) => player.Number.Value == number);
	}

	public Player GetPlayerByNeedle(string needle, bool caseSensitive = true)
	{
		Player player = GetPlayerByUsername(needle, caseSensitive);
		if (!player && int.TryParse(needle, out var result))
		{
			player = GetPlayerByNumber(result);
		}
		if (!player && Utils.IsValidSteamId64(needle))
		{
			player = GetPlayerBySteamId(needle);
		}
		return player;
	}

	public Player GetPlayerBySteamId(FixedString32Bytes steamId)
	{
		return GetPlayers().Find((Player player) => player.SteamId.Value == steamId);
	}

	public List<Player> GetReplayPlayers()
	{
		return (from player in GetPlayers(includeReplay: true)
			where player.IsReplay.Value
			select player).ToList();
	}

	public Player GetReplayPlayerByClientId(ulong clientId)
	{
		return GetReplayPlayers().Find((Player player) => player.OwnerClientId == clientId + 1337);
	}

	public Player GetLocalPlayer()
	{
		if ((bool)localPlayer && localPlayer.NetworkObject.IsSpawned)
		{
			return localPlayer;
		}
		localPlayer = null;
		foreach (Player player in GetPlayers())
		{
			if (player.IsLocalPlayer)
			{
				localPlayer = player;
				break;
			}
		}
		return localPlayer;
	}

	public List<Player> GetSpawnedPlayers(bool includeReplay = false)
	{
		return GetPlayers(includeReplay).FindAll((Player player) => player.IsCharacterSpawned);
	}

	public List<Player> GetSpawnedPlayersByTeam(PlayerTeam team, bool includeReplay = false)
	{
		return (from player in GetSpawnedPlayers(includeReplay)
			where player.Team == team
			select player).ToList();
	}

	public void Server_SpawnPlayer(ulong clientId, PlayerGameState gameState, PlayerCustomizationState customizationState, PlayerHandedness handedness, string steamID, string username, int number, int patreonLevel, int adminLevel, bool isMuted = false, bool isReplay = false)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			Player player = Object.Instantiate(playerPrefab);
			FixedString32Bytes steamID2 = steamID;
			FixedString32Bytes username2 = username;
			long ping = 0L;
			bool isMuted2 = isMuted;
			bool isReplay2 = isReplay;
			player.InitializeNetworkVariables(gameState, customizationState, handedness, steamID2, username2, number, patreonLevel, adminLevel, 0, 0, (ulong)ping, default, isMuted2, isReplay2);
			if (isReplay)
			{
				player.NetworkObject.SpawnWithOwnership(1337 + clientId);
				Logger.Info($"Spawned replay player ({clientId})");
			}
			else
			{
				player.NetworkObject.SpawnAsPlayerObject(clientId);
				Logger.Info($"Spawned player ({clientId})");
			}
		}
	}
}
