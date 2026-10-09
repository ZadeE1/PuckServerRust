using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public static class BotManager
{
	public enum BotBehavior
	{
		Chase,
		Rookie,
		Destroy
	}

	public static BotBehavior DefaultBehavior = BotBehavior.Chase;

	public const ulong BotClientIdBase = 100000uL;

	private static ulong nextBotClientId = 100000uL;

	private static readonly string[] BotNames = new string[33]
	{
		"Sauce", "Dangles", "Wheels", "Mitts", "Biscuit", "Tendy", "Pylon", "Duster", "Grinder", "Sniper",
		"Celly", "Flow", "Chirp", "Hoser", "Clapper", "Stretch", "Bardown", "Chiclets", "Lettuce", "Twig",
		"Bucket", "Snipes", "Apple", "Ferda", "Wheelhouse", "Breakaway", "Deke", "Saucer", "Slapshot", "Crease",
		"Chel", "Gongshow", "Beauty"
	};

	public static BotBrain CreateBrain(BotBehavior behavior)
	{
		return behavior switch
		{
			BotBehavior.Rookie => (BotBrain)new RookieBotBrain(), 
			BotBehavior.Destroy => new DestroyBotBrain(), 
			_ => new ChaseBotBrain(), 
		};
	}

	public static void Server_SetBotBehavior(BotBehavior behavior)
	{
		DefaultBehavior = behavior;
		BotInputDriver[] array = Object.FindObjectsByType<BotInputDriver>(FindObjectsSortMode.None);
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetBrain(CreateBrain(behavior));
		}
	}

	public static int Server_SpawnBots(int count, PlayerTeam team = PlayerTeam.None)
	{
		PlayerManager instance = MonoBehaviourSingleton<PlayerManager>.Instance;
		if (!instance)
		{
			return 0;
		}
		List<PlayerPosition> list = (from position in Object.FindObjectsByType<PlayerPosition>(FindObjectsSortMode.None)
			where !position.IsClaimed && (team == PlayerTeam.None || position.Team == team)
			orderby (position.Role == PlayerRole.Goalie) ? 1 : 0
			select position).ToList();
		int num = 0;
		foreach (PlayerPosition item in list)
		{
			if (num >= count)
			{
				break;
			}
			ulong num2 = nextBotClientId++;
			int num3 = (int)(num2 - 100000 + 1);
			instance.Server_SpawnPlayer(num2, new PlayerGameState
			{
				Phase = PlayerPhase.TeamSelect,
				Team = PlayerTeam.None,
				Role = PlayerRole.None
			}, CreateRandomCustomizationState(), PlayerHandedness.Right, $"BOT{num2}", GetRandomBotUsername(instance, num3), num3 % 100, 0, 0);
			Player playerByClientId = instance.GetPlayerByClientId(num2);
			if ((bool)playerByClientId)
			{
				playerByClientId.gameObject.AddComponent<BotInputDriver>();
				PlayerTeam? team2 = item.Team;
				playerByClientId.Server_SetGameState(null, team2);
				item.Server_Claim(playerByClientId);
				num++;
			}
		}
		return num;
	}

	private static string GetRandomBotUsername(PlayerManager playerManager, int botNumber)
	{
		int num = Random.Range(0, BotNames.Length);
		for (int i = 0; i < BotNames.Length; i++)
		{
			string text = "Bot" + BotNames[(num + i) % BotNames.Length];
			if (!playerManager.GetPlayerByUsername(text))
			{
				return text;
			}
		}
		return $"Bot{botNumber}";
	}

	private static int GetRandomItemId(string category, PlayerRole role, float noneChance = 0f)
	{
		if (noneChance > 0f && Random.value < noneChance)
		{
			return -1;
		}
		List<Item> list = ItemManager.GetItemsByCategories(new string[1] { category }).FindAll((Item item) => !item.IsUnlisted && ((role != PlayerRole.Goalie) ? item.IsAttackerItem : item.IsGoalieItem));
		if (list.Count == 0)
		{
			return -1;
		}
		return list[Random.Range(0, list.Count)].id;
	}

	private static PlayerCustomizationState CreateRandomCustomizationState()
	{
		return new PlayerCustomizationState
		{
			FlagID = GetRandomItemId("flag", PlayerRole.Attacker),
			HeadgearIDBlueAttacker = GetRandomItemId("headgear", PlayerRole.Attacker),
			HeadgearIDRedAttacker = GetRandomItemId("headgear", PlayerRole.Attacker),
			HeadgearIDBlueGoalie = GetRandomItemId("headgear", PlayerRole.Goalie),
			HeadgearIDRedGoalie = GetRandomItemId("headgear", PlayerRole.Goalie),
			MustacheID = GetRandomItemId("mustache", PlayerRole.Attacker, 0.5f),
			BeardID = GetRandomItemId("beard", PlayerRole.Attacker, 0.5f),
			JerseyIDBlueAttacker = GetRandomItemId("jersey", PlayerRole.Attacker),
			JerseyIDRedAttacker = GetRandomItemId("jersey", PlayerRole.Attacker),
			JerseyIDBlueGoalie = GetRandomItemId("jersey", PlayerRole.Goalie),
			JerseyIDRedGoalie = GetRandomItemId("jersey", PlayerRole.Goalie),
			StickSkinIDBlueAttacker = GetRandomItemId("stickSkin", PlayerRole.Attacker),
			StickSkinIDRedAttacker = GetRandomItemId("stickSkin", PlayerRole.Attacker),
			StickSkinIDBlueGoalie = GetRandomItemId("stickSkin", PlayerRole.Goalie),
			StickSkinIDRedGoalie = GetRandomItemId("stickSkin", PlayerRole.Goalie),
			StickShaftTapeIDBlueAttacker = GetRandomItemId("stickShaftTape", PlayerRole.Attacker, 0.3f),
			StickShaftTapeIDRedAttacker = GetRandomItemId("stickShaftTape", PlayerRole.Attacker, 0.3f),
			StickShaftTapeIDBlueGoalie = GetRandomItemId("stickShaftTape", PlayerRole.Goalie, 0.3f),
			StickShaftTapeIDRedGoalie = GetRandomItemId("stickShaftTape", PlayerRole.Goalie, 0.3f),
			StickBladeTapeIDBlueAttacker = GetRandomItemId("stickBladeTape", PlayerRole.Attacker, 0.3f),
			StickBladeTapeIDRedAttacker = GetRandomItemId("stickBladeTape", PlayerRole.Attacker, 0.3f),
			StickBladeTapeIDBlueGoalie = GetRandomItemId("stickBladeTape", PlayerRole.Goalie, 0.3f),
			StickBladeTapeIDRedGoalie = GetRandomItemId("stickBladeTape", PlayerRole.Goalie, 0.3f)
		};
	}

	public static int Server_DespawnBots()
	{
		PlayerManager instance = MonoBehaviourSingleton<PlayerManager>.Instance;
		if (!instance)
		{
			return 0;
		}
		List<Player> list = (from botPlayer in instance.GetPlayers()
			where botPlayer.OwnerClientId >= 100000
			select botPlayer).ToList();
		foreach (Player item in list)
		{
			ulong ownerClientId = item.OwnerClientId;
			if (item.PlayerPosition != null)
			{
				item.PlayerPosition.Server_Unclaim();
			}
			if (item.IsCharacterSpawned)
			{
				item.Server_DespawnCharacter();
			}
			item.NetworkObject.Despawn();
			if (NetworkManager.Singleton.ConnectedClients.ContainsKey(ownerClientId))
			{
				NetworkManager.Singleton.DisconnectClient(ownerClientId);
			}
		}
		return list.Count;
	}
}
