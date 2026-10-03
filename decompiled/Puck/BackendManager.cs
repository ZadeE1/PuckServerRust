using System.Collections.Generic;
using DG.Tweening;

public static class BackendManager
{
	private static PlayerState playerState;

	private static ServerState serverState;

	private static TransactionState transactionState;

	private static Tween matchingTickerTween;

	private static int matchingTick;

	private static Tween matchJoinTimeoutTickerTween;

	private static int joinTimeoutTick;

	public static PlayerState PlayerState
	{
		get
		{
			return playerState;
		}
		set
		{
			if (!playerState.Equals(value))
			{
				PlayerState oldPlayerState = playerState;
				playerState = value;
				OnPlayerStateChanged(oldPlayerState, playerState);
			}
		}
	}

	public static ServerState ServerState
	{
		get
		{
			return serverState;
		}
		set
		{
			if (!serverState.Equals(value))
			{
				ServerState oldServerState = serverState;
				serverState = value;
				OnServerStateChanged(oldServerState, serverState);
			}
		}
	}

	public static TransactionState TransactionState
	{
		get
		{
			return transactionState;
		}
		set
		{
			if (!BackendManager.transactionState.Equals(value))
			{
				TransactionState transactionState = BackendManager.transactionState;
				BackendManager.transactionState = value;
				EventManager.TriggerEvent("Event_OnTransactionStateChanged", new Dictionary<string, object>
				{
					{ "oldTransactionState", transactionState },
					{
						"newTransactionState",
						BackendManager.transactionState
					}
				});
			}
		}
	}

	public static void Initialize()
	{
		BackendManagerController.Initialize();
	}

	public static void Dispose()
	{
		BackendManagerController.Dispose();
	}

	public static void SetPlayerState(Dictionary<string, object> updates)
	{
		PlayerState = new PlayerState
		{
			AuthenticationPhase = (updates.ContainsKey("authenticationPhase") ? ((AuthenticationPhase)updates["authenticationPhase"]) : PlayerState.AuthenticationPhase),
			PlayerData = (updates.ContainsKey("playerData") ? ((PlayerData)updates["playerData"]) : PlayerState.PlayerData),
			PartyData = (updates.ContainsKey("partyData") ? ((PlayerPartyData)updates["partyData"]) : PlayerState.PartyData),
			GroupData = (updates.ContainsKey("groupData") ? ((PlayerGroupData)updates["groupData"]) : PlayerState.GroupData),
			MatchData = (updates.ContainsKey("matchData") ? ((PlayerMatchData)updates["matchData"]) : PlayerState.MatchData),
			PlayerStatistics = (updates.ContainsKey("playerStatistics") ? ((PlayerStatistics)updates["playerStatistics"]) : PlayerState.PlayerStatistics),
			Key = (updates.ContainsKey("key") ? ((string)updates["key"]) : PlayerState.Key)
		};
	}

	public static void SetServerState(Dictionary<string, object> updates)
	{
		ServerState = new ServerState
		{
			AuthenticationPhase = (updates.ContainsKey("authenticationPhase") ? ((AuthenticationPhase)updates["authenticationPhase"]) : ServerState.AuthenticationPhase),
			ServerData = (updates.ContainsKey("serverData") ? ((ServerData)updates["serverData"]) : ServerState.ServerData),
			MatchData = (updates.ContainsKey("matchData") ? ((ServerMatchData)updates["matchData"]) : ServerState.MatchData)
		};
	}

	public static void SetTransactionState(Dictionary<string, object> updates)
	{
		TransactionState = new TransactionState
		{
			Phase = (updates.ContainsKey("phase") ? ((TransactionPhase)updates["phase"]) : TransactionState.Phase)
		};
	}

	private static void OnPlayerStateChanged(PlayerState oldPlayerState, PlayerState newPlayerState)
	{
		EventManager.TriggerEvent("Event_OnPlayerStateChanged", new Dictionary<string, object>
		{
			{ "oldPlayerState", oldPlayerState },
			{ "newPlayerState", newPlayerState }
		});
		if (oldPlayerState.PlayerData != newPlayerState.PlayerData)
		{
			EventManager.TriggerEvent("Event_OnPlayerDataChanged", new Dictionary<string, object>
			{
				{ "oldPlayerData", oldPlayerState.PlayerData },
				{ "newPlayerData", newPlayerState.PlayerData }
			});
			if (newPlayerState.PlayerData != null)
			{
				bool flag = BackendUtils.GetActivePlayerDataBan(oldPlayerState.PlayerData) != null;
				bool flag2 = BackendUtils.GetActivePlayerDataMute(oldPlayerState.PlayerData) != null;
				bool flag3 = BackendUtils.GetActivePlayerDataCooldown(oldPlayerState.PlayerData) != null;
				PlayerBan activePlayerDataBan = BackendUtils.GetActivePlayerDataBan(newPlayerState.PlayerData);
				PlayerMute activePlayerDataMute = BackendUtils.GetActivePlayerDataMute(newPlayerState.PlayerData);
				PlayerCooldown activePlayerDataCooldown = BackendUtils.GetActivePlayerDataCooldown(newPlayerState.PlayerData);
				bool flag4 = activePlayerDataBan != null;
				bool flag5 = activePlayerDataMute != null;
				bool flag6 = activePlayerDataCooldown != null;
				if (flag4 && !flag)
				{
					EventManager.TriggerEvent("Event_OnPlayerBanned", new Dictionary<string, object>
					{
						{ "reason", activePlayerDataBan.reason },
						{ "expiresAt", activePlayerDataBan.expiresAt }
					});
				}
				else if (!flag4 & flag)
				{
					EventManager.TriggerEvent("Event_OnPlayerUnbanned");
				}
				if (flag5 && !flag2)
				{
					EventManager.TriggerEvent("Event_OnPlayerMuted", new Dictionary<string, object>
					{
						{ "reason", activePlayerDataMute.reason },
						{ "expiresAt", activePlayerDataMute.expiresAt }
					});
				}
				else if (!flag5 & flag2)
				{
					EventManager.TriggerEvent("Event_OnPlayerUnmuted");
				}
				if (flag6 && !flag3)
				{
					EventManager.TriggerEvent("Event_OnPlayerCooldown", new Dictionary<string, object> { { "expiresAt", activePlayerDataCooldown.expiresAt } });
				}
				else if (!flag6 & flag3)
				{
					EventManager.TriggerEvent("Event_OnPlayerCooldownExpired");
				}
			}
		}
		if (oldPlayerState.PartyData != newPlayerState.PartyData)
		{
			EventManager.TriggerEvent("Event_OnPlayerPartyDataChanged", new Dictionary<string, object>
			{
				{ "oldPlayerPartyData", oldPlayerState.PartyData },
				{ "newPlayerPartyData", newPlayerState.PartyData }
			});
		}
		if (oldPlayerState.GroupData != newPlayerState.GroupData)
		{
			EventManager.TriggerEvent("Event_OnPlayerGroupDataChanged", new Dictionary<string, object>
			{
				{ "oldPlayerGroupData", oldPlayerState.GroupData },
				{ "newPlayerGroupData", newPlayerState.GroupData }
			});
			bool num = oldPlayerState.GroupData == null && newPlayerState.GroupData != null;
			bool flag7 = oldPlayerState.GroupData != null && newPlayerState.GroupData == null;
			if (num)
			{
				StartMatchingTicker(0);
			}
			else if (flag7)
			{
				StopMatchingTicker();
			}
		}
		if (oldPlayerState.MatchData != newPlayerState.MatchData)
		{
			EventManager.TriggerEvent("Event_OnPlayerMatchDataChanged", new Dictionary<string, object>
			{
				{ "oldPlayerMatchData", oldPlayerState.MatchData },
				{ "newPlayerMatchData", newPlayerState.MatchData }
			});
			PlayerMatchData matchData = oldPlayerState.MatchData;
			bool num2 = (matchData == null || !matchData.JoinTimeoutRemainingSeconds.HasValue) && (newPlayerState.MatchData?.JoinTimeoutRemainingSeconds.HasValue ?? false);
			PlayerMatchData matchData2 = oldPlayerState.MatchData;
			int num3;
			if (matchData2 != null && matchData2.JoinTimeoutRemainingSeconds.HasValue)
			{
				PlayerMatchData matchData3 = newPlayerState.MatchData;
				num3 = ((matchData3 == null || !matchData3.JoinTimeoutRemainingSeconds.HasValue) ? 1 : 0);
			}
			else
			{
				num3 = 0;
			}
			bool flag8 = (byte)num3 != 0;
			if (num2)
			{
				StartMatchJoinTimeoutTicker(newPlayerState.MatchData.JoinTimeoutRemainingSeconds.Value);
			}
			else if (flag8)
			{
				StopMatchJoinTimeoutTicker();
			}
		}
		if (oldPlayerState.PlayerStatistics != newPlayerState.PlayerStatistics)
		{
			EventManager.TriggerEvent("Event_OnPlayerStatisticsChanged", new Dictionary<string, object>
			{
				{ "oldPlayerStatistics", oldPlayerState.PlayerStatistics },
				{ "newPlayerStatistics", newPlayerState.PlayerStatistics }
			});
		}
		if (oldPlayerState.Key != newPlayerState.Key)
		{
			EventManager.TriggerEvent("Event_OnPlayerKeyChanged", new Dictionary<string, object>
			{
				{ "oldKey", oldPlayerState.Key },
				{ "newKey", newPlayerState.Key }
			});
		}
	}

	private static void OnServerStateChanged(ServerState oldServerState, ServerState newServerState)
	{
		EventManager.TriggerEvent("Event_OnServerStateChanged", new Dictionary<string, object>
		{
			{ "oldServerState", oldServerState },
			{ "newServerState", newServerState }
		});
		if (oldServerState.ServerData != newServerState.ServerData)
		{
			EventManager.TriggerEvent("Event_OnServerDataChanged", new Dictionary<string, object>
			{
				{ "oldServerData", oldServerState.ServerData },
				{ "newServerData", newServerState.ServerData }
			});
		}
		if (oldServerState.MatchData != newServerState.MatchData)
		{
			EventManager.TriggerEvent("Event_OnServerMatchDataChanged", new Dictionary<string, object>
			{
				{ "oldServerMatchData", oldServerState.MatchData },
				{ "newServerMatchData", newServerState.MatchData }
			});
		}
	}

	private static void StartMatchingTicker(int startingTick)
	{
		EventManager.TriggerEvent("Event_OnMatchingTickerStarted", new Dictionary<string, object> { { "startingTick", startingTick } });
		matchingTick = startingTick;
		matchingTickerTween?.Kill();
		matchingTickerTween = DOVirtual.DelayedCall(1f, Tick).SetLoops(-1);
		static void Tick()
		{
			matchingTick++;
			EventManager.TriggerEvent("Event_OnMatchingTickerTick", new Dictionary<string, object> { { "tick", matchingTick } });
		}
	}

	private static void StopMatchingTicker()
	{
		matchingTick = 0;
		matchingTickerTween?.Kill();
		matchingTickerTween = null;
		EventManager.TriggerEvent("Event_OnMatchingTickerStopped");
	}

	private static void StartMatchJoinTimeoutTicker(int startingTick)
	{
		EventManager.TriggerEvent("Event_OnMatchJoinTimeoutTickerStarted", new Dictionary<string, object> { { "startingTick", startingTick } });
		joinTimeoutTick = startingTick;
		matchJoinTimeoutTickerTween?.Kill();
		matchJoinTimeoutTickerTween = DOVirtual.DelayedCall(1f, Tick).SetLoops(-1);
		static void Tick()
		{
			if (joinTimeoutTick <= 0)
			{
				StopMatchJoinTimeoutTicker();
			}
			else
			{
				joinTimeoutTick--;
				EventManager.TriggerEvent("Event_OnMatchJoinTimeoutTickerTick", new Dictionary<string, object> { { "tick", joinTimeoutTick } });
			}
		}
	}

	private static void StopMatchJoinTimeoutTicker()
	{
		joinTimeoutTick = 0;
		matchJoinTimeoutTickerTween?.Kill();
		matchJoinTimeoutTickerTween = null;
		EventManager.TriggerEvent("Event_OnMatchJoinTimeoutTickerStopped");
	}
}
