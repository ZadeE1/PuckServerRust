using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public static class BackendManagerController
{
	private static readonly Logger Logger = new Logger("BackendManagerController");

	private const float ServerUnlistedCheckIntervalSeconds = 15f;

	private const float ServerUnlistedAlertDelaySeconds = 30f;

	private const float ServerUnlistedReAlertIntervalSeconds = 300f;

	private static bool serverHasAuthenticated;

	private static Tween serverUnlistedWatchdogTween;

	private static double serverUnlistedSinceTime;

	private static double serverLastUnlistedAlertTime;

	public static void Initialize()
	{
		EventManager.AddEventListener("Event_OnGetTicketForWebApiResponse", Event_OnGetTicketForWebApiResponse);
		EventManager.AddEventListener("Event_OnFooterClickCreateParty", Event_OnFooterClickCreateParty);
		EventManager.AddEventListener("Event_OnFooterClickLeaveParty", Event_OnFooterClickLeaveParty);
		EventManager.AddEventListener("Event_OnFooterClickDisbandParty", Event_OnFooterClickDisbandParty);
		EventManager.AddEventListener("Event_OnLobbyCreated", Event_OnLobbyCreated);
		EventManager.AddEventListener("Event_OnLobbyEntered", Event_OnLobbyEntered);
		EventManager.AddEventListener("Event_OnMatchmakingMatchingClickStartMatchmaking", Event_OnMatchmakingMatchingClickStartMatchmaking);
		EventManager.AddEventListener("Event_OnMatchmakingMatchingClickClose", Event_OnMatchmakingMatchingClickClose);
		EventManager.AddEventListener("Event_OnPopupClickOk", Event_OnPopupClickOk);
		EventManager.AddEventListener("Event_OnAppearanceClickPurchaseItem", Event_OnAppearanceClickPurchaseItem);
		EventManager.AddEventListener("Event_OnMicroTxnAuthorizationResponse", Event_OnMicroTxnAuthorizationResponse);
		EventManager.AddEventListener("Event_OnNewServerClickStart", Event_OnNewServerClickStart);
		EventManager.AddEventListener("Event_OnServerStateChanged", Event_OnServerStateChanged);
		WebSocketManager.AddMessageListener("disconnected", WebSocket_Event_OnDisconnected);
		WebSocketManager.AddMessageListener("playerAuthenticateResponse", WebSocket_Event_OnPlayerAuthenticateResponse);
		WebSocketManager.AddMessageListener("playerData", WebSocket_Event_OnPlayerData);
		WebSocketManager.AddMessageListener("playerPartyData", WebSocket_Event_OnPlayerPartyData);
		WebSocketManager.AddMessageListener("playerGroupData", WebSocket_Event_OnPlayerGroupData);
		WebSocketManager.AddMessageListener("playerMatchData", WebSocket_Event_OnPlayerMatchData);
		WebSocketManager.AddMessageListener("playerStatistics", WebSocket_Event_OnPlayerStatistics);
		WebSocketManager.AddMessageListener("playerKey", WebSocket_Event_OnPlayerKey);
		WebSocketManager.AddMessageListener("playerProbeRttRequest", WebSocket_Event_OnPlayerProbeRttRequest);
		WebSocketManager.AddMessageListener("playerMaxRttRequest", WebSocket_Event_OnPlayerMaxRttRequest);
		WebSocketManager.AddMessageListener("playerStartTransactionResponse", WebSocket_Event_OnPlayerStartTransactionResponse);
		WebSocketManager.AddMessageListener("playerFinalizeTransactionResponse", WebSocket_Event_OnPlayerFinalizeTransactionResponse);
		WebSocketManager.AddMessageListener("serverAuthenticateResponse", WebSocket_Event_OnServerAuthenticateResponse);
		WebSocketManager.AddMessageListener("serverUnauthenticateResponse", WebSocket_Event_OnServerUnauthenticateResponse);
		WebSocketManager.AddMessageListener("serverData", WebSocket_Event_OnServerData);
		WebSocketManager.AddMessageListener("serverMatchData", WebSocket_Event_OnServerMatchData);
	}

	public static void Dispose()
	{
		EventManager.RemoveEventListener("Event_OnGetTicketForWebApiResponse", Event_OnGetTicketForWebApiResponse);
		EventManager.RemoveEventListener("Event_OnFooterClickCreateParty", Event_OnFooterClickCreateParty);
		EventManager.RemoveEventListener("Event_OnFooterClickLeaveParty", Event_OnFooterClickLeaveParty);
		EventManager.RemoveEventListener("Event_OnFooterClickDisbandParty", Event_OnFooterClickDisbandParty);
		EventManager.RemoveEventListener("Event_OnLobbyCreated", Event_OnLobbyCreated);
		EventManager.RemoveEventListener("Event_OnLobbyEntered", Event_OnLobbyEntered);
		EventManager.RemoveEventListener("Event_OnMatchmakingMatchingClickStartMatchmaking", Event_OnMatchmakingMatchingClickStartMatchmaking);
		EventManager.RemoveEventListener("Event_OnMatchmakingMatchingClickClose", Event_OnMatchmakingMatchingClickClose);
		EventManager.RemoveEventListener("Event_OnPopupClickOk", Event_OnPopupClickOk);
		EventManager.RemoveEventListener("Event_OnAppearanceClickPurchaseItem", Event_OnAppearanceClickPurchaseItem);
		EventManager.RemoveEventListener("Event_OnMicroTxnAuthorizationResponse", Event_OnMicroTxnAuthorizationResponse);
		EventManager.RemoveEventListener("Event_OnNewServerClickStart", Event_OnNewServerClickStart);
		EventManager.RemoveEventListener("Event_OnServerStateChanged", Event_OnServerStateChanged);
		WebSocketManager.RemoveMessageListener("disconnected", WebSocket_Event_OnDisconnected);
		WebSocketManager.RemoveMessageListener("playerAuthenticateResponse", WebSocket_Event_OnPlayerAuthenticateResponse);
		WebSocketManager.RemoveMessageListener("playerData", WebSocket_Event_OnPlayerData);
		WebSocketManager.RemoveMessageListener("playerPartyData", WebSocket_Event_OnPlayerPartyData);
		WebSocketManager.RemoveMessageListener("playerGroupData", WebSocket_Event_OnPlayerGroupData);
		WebSocketManager.RemoveMessageListener("playerMatchData", WebSocket_Event_OnPlayerMatchData);
		WebSocketManager.RemoveMessageListener("playerStatistics", WebSocket_Event_OnPlayerStatistics);
		WebSocketManager.RemoveMessageListener("playerKey", WebSocket_Event_OnPlayerKey);
		WebSocketManager.RemoveMessageListener("playerProbeRttRequest", WebSocket_Event_OnPlayerProbeRttRequest);
		WebSocketManager.RemoveMessageListener("playerMaxRttRequest", WebSocket_Event_OnPlayerMaxRttRequest);
		WebSocketManager.RemoveMessageListener("playerStartTransactionResponse", WebSocket_Event_OnPlayerStartTransactionResponse);
		WebSocketManager.RemoveMessageListener("playerFinalizeTransactionResponse", WebSocket_Event_OnPlayerFinalizeTransactionResponse);
		WebSocketManager.RemoveMessageListener("serverAuthenticateResponse", WebSocket_Event_OnServerAuthenticateResponse);
		WebSocketManager.RemoveMessageListener("serverUnauthenticateResponse", WebSocket_Event_OnServerUnauthenticateResponse);
		WebSocketManager.RemoveMessageListener("serverData", WebSocket_Event_OnServerData);
		WebSocketManager.RemoveMessageListener("serverMatchData", WebSocket_Event_OnServerMatchData);
	}

	private static int? PingProbe(Probe probe, int connectTimeout, int responseTimeout)
	{
		EndPoint endPoint = new EndPoint(probe.host, probe.port);
		TCPClient tcpClient = new TCPClient(endPoint, connectTimeout);
		double pingTimestamp = 0.0;
		int? rtt = null;
		ManualResetEventSlim responseEvent = new ManualResetEventSlim(initialState: false);
		tcpClient.OnConnected += () =>
		{
			tcpClient.SendMessage("ping");
		};
		tcpClient.OnMessageSent += (string message) =>
		{
			pingTimestamp = Utils.GetTimestamp();
		};
		tcpClient.OnMessageReceived += (string message) =>
		{
			rtt = (int)(Utils.GetTimestamp() - pingTimestamp);
			responseEvent.Set();
		};
		tcpClient.Connect();
		if (tcpClient.IsConnected)
		{
			responseEvent.Wait(responseTimeout);
			tcpClient.Disconnect();
		}
		return rtt;
	}

	private static void Event_OnGetTicketForWebApiResponse(Dictionary<string, object> message)
	{
		string value = (string)message["ticket"];
		BackendManager.SetPlayerState(new Dictionary<string, object> { 
		{
			"authenticationPhase",
			AuthenticationPhase.Authenticating
		} });
		WebSocketManager.Emit("playerAuthenticateRequest", new Dictionary<string, object> { { "ticket", value } }, "playerAuthenticateResponse");
	}

	private static void Event_OnFooterClickCreateParty(Dictionary<string, object> message)
	{
		WebSocketManager.Emit("playerCreatePartyRequest", null, "playerCreatePartyResponse");
	}

	private static void Event_OnFooterClickLeaveParty(Dictionary<string, object> message)
	{
		WebSocketManager.Emit("playerLeavePartyRequest", null, "playerLeavePartyResponse");
	}

	private static void Event_OnFooterClickDisbandParty(Dictionary<string, object> message)
	{
		WebSocketManager.Emit("playerDisbandPartyRequest", null, "playerDisbandPartyResponse");
	}

	private static void Event_OnLobbyCreated(Dictionary<string, object> message)
	{
		string value = (string)message["lobbyId"];
		WebSocketManager.Emit("playerUpdatePartyRequest", new Dictionary<string, object> { { "steamLobbyId", value } }, "playerUpdatePartyResponse");
	}

	private static void Event_OnLobbyEntered(Dictionary<string, object> message)
	{
		string text = (string)message["lobbyId"];
		string text2 = (string)message["ownerSteamId"];
		Logger.Info("Entered lobby " + text + " owned by " + text2 + " (player's Steam ID: " + BackendManager.PlayerState.PlayerData.steamId + ")");
		if (text2 != BackendManager.PlayerState.PlayerData.steamId)
		{
			WebSocketManager.Emit("playerJoinPartyRequest", new Dictionary<string, object> { { "steamLobbyId", text } }, "playerJoinPartyResponse");
		}
	}

	private static void Event_OnMatchmakingMatchingClickStartMatchmaking(Dictionary<string, object> message)
	{
		string[] value = (string[])message["poolIds"];
		WebSocketManager.Emit("playerStartMatchmakingRequest", new Dictionary<string, object>
		{
			{ "poolIds", value },
			{
				"maxRtt",
				SettingsManager.MaxMatchmakingPing
			}
		}, "playerStartMatchmakingResponse");
	}

	private static void Event_OnMatchmakingMatchingClickClose(Dictionary<string, object> message)
	{
		WebSocketManager.Emit("playerStopMatchmakingRequest", null, "playerStopMatchmakingResponse");
	}

	private static void Event_OnPopupClickOk(Dictionary<string, object> message)
	{
		Popup popup = (Popup)message["popup"];
		if (popup.Name == "identity")
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)popup.Data;
			string value = (string)dictionary["username"];
			int num = (int)dictionary["number"];
			WebSocketManager.Emit("playerSetIdentityRequest", new Dictionary<string, object>
			{
				{ "username", value },
				{ "number", num }
			}, "playerSetIdentityResponse");
		}
	}

	private static void Event_OnAppearanceClickPurchaseItem(Dictionary<string, object> message)
	{
		Item item = (Item)message["item"];
		BackendManager.SetTransactionState(new Dictionary<string, object> { 
		{
			"phase",
			TransactionPhase.Starting
		} });
		if (!SteamIntegrationManager.IsOverlayEnabled)
		{
			BackendManager.SetTransactionState(new Dictionary<string, object> { 
			{
				"phase",
				TransactionPhase.None
			} });
		}
		else
		{
			WebSocketManager.Emit("playerStartTransactionRequest", new Dictionary<string, object> { { "itemId", item.id } }, "playerStartTransactionResponse");
		}
	}

	private static void Event_OnMicroTxnAuthorizationResponse(Dictionary<string, object> message)
	{
		if ((bool)message["authorized"])
		{
			WebSocketManager.Emit("playerFinalizeTransactionRequest", null, "playerFinalizeTransactionResponse");
			return;
		}
		WebSocketManager.Emit("playerCancelTransaction");
		BackendManager.SetTransactionState(new Dictionary<string, object> { 
		{
			"phase",
			TransactionPhase.None
		} });
	}

	private static void Event_OnNewServerClickStart(Dictionary<string, object> message)
	{
		if (!((string)message["type"] != "dedicated"))
		{
			string value = (string)message["name"];
			int num = (int)message["maxPlayers"];
			string value2 = (string)message["password"];
			string value3 = (string)message["probeId"];
			WebSocketManager.Emit("playerDeployServerRequest", new Dictionary<string, object>
			{
				{ "name", value },
				{ "maxPlayers", num },
				{ "password", value2 },
				{ "probeId", value3 }
			}, "playerDeployServerResponse");
		}
	}

	private static void WebSocket_Event_OnDisconnected(Dictionary<string, object> message)
	{
		BackendManager.SetPlayerState(new Dictionary<string, object>
		{
			{ "steamId", null },
			{ "playerData", null },
			{ "partyData", null },
			{ "key", null },
			{
				"authenticationPhase",
				AuthenticationPhase.None
			}
		});
		BackendManager.SetServerState(new Dictionary<string, object>
		{
			{ "serverData", null },
			{ "matchData", null },
			{
				"authenticationPhase",
				AuthenticationPhase.None
			}
		});
	}

	private static void WebSocket_Event_OnPlayerAuthenticateResponse(Dictionary<string, object> message)
	{
		PlayerAuthenticateResponse data = ((InMessage)message["inMessage"]).GetData<PlayerAuthenticateResponse>();
		BackendManager.SetPlayerState(new Dictionary<string, object> { 
		{
			"authenticationPhase",
			data.success ? AuthenticationPhase.Authenticated : AuthenticationPhase.None
		} });
	}

	private static void WebSocket_Event_OnPlayerData(Dictionary<string, object> message)
	{
		InMessage inMessage = (InMessage)message["inMessage"];
		BackendManager.SetPlayerState(new Dictionary<string, object> { 
		{
			"playerData",
			inMessage.GetData<PlayerDataMessage>().player
		} });
	}

	private static void WebSocket_Event_OnPlayerPartyData(Dictionary<string, object> message)
	{
		InMessage inMessage = (InMessage)message["inMessage"];
		BackendManager.SetPlayerState(new Dictionary<string, object> { 
		{
			"partyData",
			inMessage.GetData<PlayerPartyDataMessage>().party
		} });
	}

	private static void WebSocket_Event_OnPlayerGroupData(Dictionary<string, object> message)
	{
		InMessage inMessage = (InMessage)message["inMessage"];
		BackendManager.SetPlayerState(new Dictionary<string, object> { 
		{
			"groupData",
			inMessage.GetData<PlayerGroupDataMessage>().group
		} });
	}

	private static void WebSocket_Event_OnPlayerMatchData(Dictionary<string, object> message)
	{
		InMessage inMessage = (InMessage)message["inMessage"];
		BackendManager.SetPlayerState(new Dictionary<string, object> { 
		{
			"matchData",
			inMessage.GetData<PlayerMatchDataMessage>().match
		} });
	}

	private static void WebSocket_Event_OnPlayerStatistics(Dictionary<string, object> message)
	{
		InMessage inMessage = (InMessage)message["inMessage"];
		BackendManager.SetPlayerState(new Dictionary<string, object> { 
		{
			"playerStatistics",
			inMessage.GetData<PlayerStatisticsMessage>().statistics
		} });
	}

	private static void WebSocket_Event_OnPlayerKey(Dictionary<string, object> message)
	{
		InMessage inMessage = (InMessage)message["inMessage"];
		BackendManager.SetPlayerState(new Dictionary<string, object> { 
		{
			"key",
			inMessage.GetData<PlayerKeyMessage>().key
		} });
	}

	private static void WebSocket_Event_OnPlayerProbeRttRequest(Dictionary<string, object> message)
	{
		InMessage inMessage = (InMessage)message["inMessage"];
		Probe[] probes = inMessage.GetData<PlayerProbeRttRequestMessage>().probes;
		Task.Run(async () =>
		{
			SemaphoreSlim semaphore = new SemaphoreSlim(4);
			Dictionary<string, int> value = (await Task.WhenAll(probes.Select(async (Probe probe) =>
			{
				await semaphore.WaitAsync();
				int? item = await Task.Run(() => PingProbe(probe, 1000, 1000));
				semaphore.Release();
				return (id: probe.id, rtt: item);
			}))).Where(((string id, int? rtt) r) => r.rtt.HasValue).ToDictionary(((string id, int? rtt) r) => r.id, ((string id, int? rtt) r) => r.rtt.Value);
			inMessage.Respond(new Dictionary<string, object> { { "probeIdRttMap", value } });
		});
	}

	private static void WebSocket_Event_OnPlayerMaxRttRequest(Dictionary<string, object> message)
	{
		((InMessage)message["inMessage"]).Respond(new Dictionary<string, object> { 
		{
			"maxRtt",
			SettingsManager.MaxMatchmakingPing
		} });
	}

	private static void WebSocket_Event_OnPlayerStartTransactionResponse(Dictionary<string, object> message)
	{
		if (((InMessage)message["inMessage"]).GetData<PlayerStartTransactionResponse>().success)
		{
			BackendManager.SetTransactionState(new Dictionary<string, object> { 
			{
				"phase",
				TransactionPhase.Started
			} });
		}
		else
		{
			BackendManager.SetTransactionState(new Dictionary<string, object> { 
			{
				"phase",
				TransactionPhase.None
			} });
		}
	}

	private static void WebSocket_Event_OnPlayerFinalizeTransactionResponse(Dictionary<string, object> message)
	{
		((InMessage)message["inMessage"]).GetData<PlayerFinalizeTransactionResponse>();
		BackendManager.SetTransactionState(new Dictionary<string, object> { 
		{
			"phase",
			TransactionPhase.None
		} });
	}

	private static void WebSocket_Event_OnServerAuthenticateResponse(Dictionary<string, object> message)
	{
		ServerAuthenticateResponse data = ((InMessage)message["inMessage"]).GetData<ServerAuthenticateResponse>();
		BackendManager.SetServerState(new Dictionary<string, object> { 
		{
			"authenticationPhase",
			data.success ? AuthenticationPhase.Authenticated : AuthenticationPhase.None
		} });
	}

	private static void WebSocket_Event_OnServerUnauthenticateResponse(Dictionary<string, object> message)
	{
		serverHasAuthenticated = false;
		StopServerUnlistedWatchdog();
		BackendManager.SetServerState(new Dictionary<string, object> { 
		{
			"authenticationPhase",
			AuthenticationPhase.None
		} });
	}

	private static void Event_OnServerStateChanged(Dictionary<string, object> message)
	{
		ServerState serverState = (ServerState)message["oldServerState"];
		ServerState serverState2 = (ServerState)message["newServerState"];
		if (serverState.AuthenticationPhase == serverState2.AuthenticationPhase)
		{
			return;
		}
		if (serverState2.AuthenticationPhase == AuthenticationPhase.Authenticated)
		{
			if (serverUnlistedWatchdogTween != null)
			{
				int num = (int)(Time.realtimeSinceStartupAsDouble - serverUnlistedSinceTime);
				Logger.Info($"Server re-listed on the public server browser after ~{num}s unlisted.");
			}
			serverHasAuthenticated = true;
			StopServerUnlistedWatchdog();
		}
		else if (serverHasAuthenticated)
		{
			StartServerUnlistedWatchdog();
		}
	}

	private static void StartServerUnlistedWatchdog()
	{
		if (serverUnlistedWatchdogTween == null)
		{
			serverUnlistedSinceTime = Time.realtimeSinceStartupAsDouble;
			serverLastUnlistedAlertTime = 0.0;
			serverUnlistedWatchdogTween = DOVirtual.DelayedCall(15f, CheckServerUnlisted).SetLoops(-1);
		}
	}

	private static void StopServerUnlistedWatchdog()
	{
		serverUnlistedWatchdogTween?.Kill();
		serverUnlistedWatchdogTween = null;
		serverLastUnlistedAlertTime = 0.0;
	}

	private static void CheckServerUnlisted()
	{
		double realtimeSinceStartupAsDouble = Time.realtimeSinceStartupAsDouble;
		double num = realtimeSinceStartupAsDouble - serverUnlistedSinceTime;
		if (!(num < 30.0) && (!(serverLastUnlistedAlertTime > 0.0) || !(realtimeSinceStartupAsDouble - serverLastUnlistedAlertTime < 300.0)))
		{
			serverLastUnlistedAlertTime = realtimeSinceStartupAsDouble;
			Logger.Error($"Server has been unlisted from the public server browser for ~{(int)num}s " + $"(AuthenticationPhase={BackendManager.ServerState.AuthenticationPhase}, websocket " + (WebSocketManager.IsConnected ? "connected" : "disconnected") + "). Players cannot find it until it re-authenticates with the backend.");
		}
	}

	private static void WebSocket_Event_OnServerData(Dictionary<string, object> message)
	{
		InMessage inMessage = (InMessage)message["inMessage"];
		BackendManager.SetServerState(new Dictionary<string, object> { 
		{
			"serverData",
			inMessage.GetData<ServerDataMessage>().server
		} });
	}

	private static void WebSocket_Event_OnServerMatchData(Dictionary<string, object> message)
	{
		InMessage inMessage = (InMessage)message["inMessage"];
		BackendManager.SetServerState(new Dictionary<string, object> { 
		{
			"matchData",
			inMessage.GetData<ServerMatchDataMessage>().match
		} });
	}
}
