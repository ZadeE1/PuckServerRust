using System.Collections.Generic;
using UnityEngine;

public class PlayerManagerController : MonoBehaviour
{
	private PlayerManager playerManager;

	private void Awake()
	{
		playerManager = GetComponent<PlayerManager>();
		EventManager.AddEventListener("Event_Everyone_OnPlayerSpawned", Event_Everyone_OnPlayerSpawned);
		EventManager.AddEventListener("Event_Everyone_OnPlayerDespawned", Event_Everyone_OnPlayerDespawned);
		EventManager.AddEventListener("Event_Server_OnApprovedClientConnected", Event_Server_OnApprovedClientConnected);
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerSpawned", Event_Everyone_OnPlayerSpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerDespawned", Event_Everyone_OnPlayerDespawned);
		EventManager.RemoveEventListener("Event_Server_OnApprovedClientConnected", Event_Server_OnApprovedClientConnected);
	}

	private void Event_Everyone_OnPlayerSpawned(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		playerManager.AddPlayer(player);
	}

	private void Event_Everyone_OnPlayerDespawned(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		playerManager.RemovePlayer(player);
	}

	private void Event_Server_OnApprovedClientConnected(Dictionary<string, object> message)
	{
		ulong clientId = (ulong)message["clientId"];
		ConnectionApproval connectionApproval = (ConnectionApproval)message["connectionApproval"];
		ConnectionData connectionData = connectionApproval.ConnectionData;
		PlayerData playerData = connectionApproval.PlayerData;
		PlayerGameState gameState = new PlayerGameState
		{
			Phase = PlayerPhase.TeamSelect,
			Team = PlayerTeam.None,
			Role = PlayerRole.None
		};
		PlayerCustomizationState customizationState = new PlayerCustomizationState
		{
			FlagID = ItemManager.ValidateEquippedItem(connectionData.FlagID, -1, (Item item) => item.IsFlag, playerData),
			HeadgearIDBlueAttacker = ItemManager.ValidateEquippedItem(connectionData.HeadgearIDBlueAttacker, 513, (Item item) => item.IsHeadgear && item.IsAttackerItem, playerData),
			HeadgearIDRedAttacker = ItemManager.ValidateEquippedItem(connectionData.HeadgearIDRedAttacker, 513, (Item item) => item.IsHeadgear && item.IsAttackerItem, playerData),
			HeadgearIDBlueGoalie = ItemManager.ValidateEquippedItem(connectionData.HeadgearIDBlueGoalie, 527, (Item item) => item.IsHeadgear && item.IsGoalieItem, playerData),
			HeadgearIDRedGoalie = ItemManager.ValidateEquippedItem(connectionData.HeadgearIDRedGoalie, 527, (Item item) => item.IsHeadgear && item.IsGoalieItem, playerData),
			MustacheID = ItemManager.ValidateEquippedItem(connectionData.MustacheID, -1, (Item item) => item.IsMustache, playerData),
			BeardID = ItemManager.ValidateEquippedItem(connectionData.BeardID, -1, (Item item) => item.IsBeard, playerData),
			JerseyIDBlueAttacker = ItemManager.ValidateEquippedItem(connectionData.JerseyIDBlueAttacker, 2048, (Item item) => item.IsJersey && item.IsAttackerItem, playerData),
			JerseyIDRedAttacker = ItemManager.ValidateEquippedItem(connectionData.JerseyIDRedAttacker, 2048, (Item item) => item.IsJersey && item.IsAttackerItem, playerData),
			JerseyIDBlueGoalie = ItemManager.ValidateEquippedItem(connectionData.JerseyIDBlueGoalie, 2048, (Item item) => item.IsJersey && item.IsGoalieItem, playerData),
			JerseyIDRedGoalie = ItemManager.ValidateEquippedItem(connectionData.JerseyIDRedGoalie, 2048, (Item item) => item.IsJersey && item.IsGoalieItem, playerData),
			StickSkinIDBlueAttacker = ItemManager.ValidateEquippedItem(connectionData.StickSkinIDBlueAttacker, 2621, (Item item) => item.IsStickSkin && item.IsAttackerItem, playerData),
			StickSkinIDRedAttacker = ItemManager.ValidateEquippedItem(connectionData.StickSkinIDRedAttacker, 2621, (Item item) => item.IsStickSkin && item.IsAttackerItem, playerData),
			StickSkinIDBlueGoalie = ItemManager.ValidateEquippedItem(connectionData.StickSkinIDBlueGoalie, 2621, (Item item) => item.IsStickSkin && item.IsGoalieItem, playerData),
			StickSkinIDRedGoalie = ItemManager.ValidateEquippedItem(connectionData.StickSkinIDRedGoalie, 2621, (Item item) => item.IsStickSkin && item.IsGoalieItem, playerData),
			StickShaftTapeIDBlueAttacker = ItemManager.ValidateEquippedItem(connectionData.StickShaftTapeIDBlueAttacker, -1, (Item item) => item.IsStickShaftTape && item.IsAttackerItem, playerData),
			StickShaftTapeIDRedAttacker = ItemManager.ValidateEquippedItem(connectionData.StickShaftTapeIDRedAttacker, -1, (Item item) => item.IsStickShaftTape && item.IsAttackerItem, playerData),
			StickShaftTapeIDBlueGoalie = ItemManager.ValidateEquippedItem(connectionData.StickShaftTapeIDBlueGoalie, -1, (Item item) => item.IsStickShaftTape && item.IsGoalieItem, playerData),
			StickShaftTapeIDRedGoalie = ItemManager.ValidateEquippedItem(connectionData.StickShaftTapeIDRedGoalie, -1, (Item item) => item.IsStickShaftTape && item.IsGoalieItem, playerData),
			StickBladeTapeIDBlueAttacker = ItemManager.ValidateEquippedItem(connectionData.StickBladeTapeIDBlueAttacker, -1, (Item item) => item.IsStickBladeTape && item.IsAttackerItem, playerData),
			StickBladeTapeIDRedAttacker = ItemManager.ValidateEquippedItem(connectionData.StickBladeTapeIDRedAttacker, -1, (Item item) => item.IsStickBladeTape && item.IsAttackerItem, playerData),
			StickBladeTapeIDBlueGoalie = ItemManager.ValidateEquippedItem(connectionData.StickBladeTapeIDBlueGoalie, -1, (Item item) => item.IsStickBladeTape && item.IsGoalieItem, playerData),
			StickBladeTapeIDRedGoalie = ItemManager.ValidateEquippedItem(connectionData.StickBladeTapeIDRedGoalie, -1, (Item item) => item.IsStickBladeTape && item.IsGoalieItem, playerData)
		};
		bool isMuted = BackendUtils.GetActivePlayerDataMute(playerData) != null;
		playerManager.Server_SpawnPlayer(clientId, gameState, customizationState, connectionData.Handedness, playerData.steamId, playerData.username, playerData.number, playerData.patreonLevel, playerData.adminLevel, isMuted);
	}
}
