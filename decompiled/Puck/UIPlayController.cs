using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Humanizer;

public class UIPlayController : UIViewController<UIPlay>
{
	private UIPlay uiPlay;

	public override void Awake()
	{
		base.Awake();
		uiPlay = GetComponent<UIPlay>();
		EventManager.AddEventListener("Event_OnPlayerDataChanged", Event_OnPlayerDataChanged);
		EventManager.AddEventListener("Event_OnPlayerPartyDataChanged", Event_OnPlayerPartyDataChanged);
		EventManager.AddEventListener("Event_OnPlayerGroupDataChanged", Event_OnPlayerGroupDataChanged);
		EventManager.AddEventListener("Event_OnPlayerMatchDataChanged", Event_OnPlayerMatchDataChanged);
		EventManager.AddEventListener("Event_OnPlayerStatisticsChanged", Event_OnPlayerStatisticsChanged);
	}

	private void Start()
	{
		UpdateMatchmakingPoolAvailability();
	}

	public override void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_OnPlayerDataChanged", Event_OnPlayerDataChanged);
		EventManager.RemoveEventListener("Event_OnPlayerPartyDataChanged", Event_OnPlayerPartyDataChanged);
		EventManager.RemoveEventListener("Event_OnPlayerGroupDataChanged", Event_OnPlayerGroupDataChanged);
		EventManager.RemoveEventListener("Event_OnPlayerMatchDataChanged", Event_OnPlayerMatchDataChanged);
		EventManager.RemoveEventListener("Event_OnPlayerStatisticsChanged", Event_OnPlayerStatisticsChanged);
		base.OnDestroy();
	}

	private void UpdateMatchmakingPoolAvailability()
	{
		PlayerData playerData = BackendManager.PlayerState.PlayerData;
		PlayerGroupData groupData = BackendManager.PlayerState.GroupData;
		PlayerMatchData matchData = BackendManager.PlayerState.MatchData;
		PlayerPartyData partyData = BackendManager.PlayerState.PartyData;
		bool isMatchmakingAvailable = playerData != null && (partyData == null || (partyData != null && partyData.ownerSteamId == playerData.steamId)) && groupData == null && matchData == null;
		uiPlay.SetPoolsAvailable(isMatchmakingAvailable, DoesPartyFitPool(3), DoesPartyFitPool(5));
	}

	private bool DoesPartyFitPool(int teamSize)
	{
		return (BackendManager.PlayerState.PartyData?.memberSteamIds.Length ?? 1) <= teamSize;
	}

	private void Event_OnPlayerStatisticsChanged(Dictionary<string, object> message)
	{
		PlayerStatistics playerStatistics = (PlayerStatistics)message["newPlayerStatistics"];
		uiPlay.SetThreeVsThreePoolDescription(GetPoolDescription(playerStatistics, "3v3"));
		uiPlay.SetFiveVsFivePoolDescription(GetPoolDescription(playerStatistics, "5v5"));
		uiPlay.SetStatistics(playerStatistics.playerManager.playerCount);
	}

	private string GetPoolDescription(PlayerStatistics playerStatistics, string poolId)
	{
		PoolStatistics poolStatistics = playerStatistics.matchmakingManager.pools.FirstOrDefault((PoolStatistics pool) => pool.id == poolId);
		if (poolStatistics == null)
		{
			return string.Empty;
		}
		return $"IN QUEUE: {poolStatistics.groupPlayerCount}<br>EST. MATCHING TIME: {GetEstimatedMatchingTime(poolStatistics)}";
	}

	private string GetEstimatedMatchingTime(PoolStatistics poolStatistics)
	{
		if (!poolStatistics.averageGroupLifetime.HasValue)
		{
			return "NO DATA";
		}
		return TimeSpan.FromMilliseconds(poolStatistics.averageGroupLifetime.Value).Humanize(2, CultureInfo.InvariantCulture, TimeUnit.Week, TimeUnit.Second);
	}

	private void Event_OnPlayerDataChanged(Dictionary<string, object> message)
	{
		UpdateMatchmakingPoolAvailability();
	}

	private void Event_OnPlayerPartyDataChanged(Dictionary<string, object> message)
	{
		UpdateMatchmakingPoolAvailability();
	}

	private void Event_OnPlayerGroupDataChanged(Dictionary<string, object> message)
	{
		UpdateMatchmakingPoolAvailability();
	}

	private void Event_OnPlayerMatchDataChanged(Dictionary<string, object> message)
	{
		UpdateMatchmakingPoolAvailability();
	}
}
