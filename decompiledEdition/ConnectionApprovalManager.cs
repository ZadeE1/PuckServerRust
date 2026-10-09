using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(ServerManager))]
[RequireComponent(typeof(TimeoutManager))]
[RequireComponent(typeof(BanManager))]
public class ConnectionApprovalManager : MonoBehaviourSingleton<ConnectionApprovalManager>
{
	private static readonly Logger Logger = new Logger("ConnectionApprovalManager");

	[HideInInspector]
	public ServerManager ServerManager;

	[HideInInspector]
	public TimeoutManager TimeoutManager;

	[HideInInspector]
	public BanManager BanManager;

	[HideInInspector]
	public WhitelistManager WhitelistManager;

	private Dictionary<ulong, ConnectionApproval> clientIdConnectionApprovalMap = new Dictionary<ulong, ConnectionApproval>();

	private bool bufferConnectionApprovals = true;

	private List<(NetworkManager.ConnectionApprovalRequest, NetworkManager.ConnectionApprovalResponse)> bufferedConnectionApprovals = new List<(NetworkManager.ConnectionApprovalRequest, NetworkManager.ConnectionApprovalResponse)>();

	public override void Awake()
	{
		base.Awake();
		ServerManager = GetComponent<ServerManager>();
		TimeoutManager = GetComponent<TimeoutManager>();
		BanManager = GetComponent<BanManager>();
		WhitelistManager = GetComponent<WhitelistManager>();
	}

	private void Start()
	{
		bufferConnectionApprovals = true;
		NetworkManager.Singleton.ConnectionApprovalCallback = ConnectionApprovalCallback;
	}

	public void Dispose()
	{
		clientIdConnectionApprovalMap.Clear();
		bufferConnectionApprovals = true;
		bufferedConnectionApprovals.Clear();
	}

	private void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
	{
		if (request.ClientNetworkId != 0 && !ServerReadinessManager.IsReady)
		{
			Logger.Info($"Rejected connection for client {request.ClientNetworkId}: server is starting up");
			response.Reason = GetRejectionReason(ConnectionRejectionCode.ServerStarting);
			response.Approved = false;
			response.Pending = false;
		}
		else if (bufferConnectionApprovals)
		{
			bufferedConnectionApprovals.Add((request, response));
		}
		else
		{
			HandleConnectionApproval(request, response);
		}
	}

	private void HandleConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
	{
		ulong clientNetworkId = request.ClientNetworkId;
		ConnectionData connectionData = null;
		try
		{
			connectionData = JsonSerializer.Deserialize<ConnectionData>(Encoding.UTF8.GetString(request.Payload));
		}
		catch (Exception ex)
		{
			Logger.Error($"Error deserializing connection data for client {clientNetworkId}: {ex.Message}");
		}
		AddConnectionApproval(clientNetworkId, request, response, connectionData);
	}

	private void AddConnectionApproval(ulong clientId, NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response, ConnectionData connectionData)
	{
		if (!clientIdConnectionApprovalMap.ContainsKey(clientId))
		{
			ConnectionApproval connectionApproval = new ConnectionApproval
			{
				Request = request,
				Response = response,
				ConnectionData = connectionData,
				IpAddress = ((clientId == 0L) ? "127.0.0.1" : ServerManager.UnityTransport.GetEndpoint(clientId).Address)
			};
			clientIdConnectionApprovalMap.Add(clientId, connectionApproval);
			OnConnectionApprovalStarted(clientId, connectionApproval);
		}
	}

	public void ConsumeBufferedConnectionApprovals(bool stopBuffering = true)
	{
		Logger.Info($"Consuming {bufferedConnectionApprovals.Count} buffered connection approvals");
		bufferConnectionApprovals = !stopBuffering;
		foreach (var (connectionApprovalRequest, connectionApprovalResponse) in bufferedConnectionApprovals.ToList())
		{
			HandleConnectionApproval(connectionApprovalRequest, connectionApprovalResponse);
			bufferedConnectionApprovals.Remove((connectionApprovalRequest, connectionApprovalResponse));
		}
	}

	public void RemoveConnectionApproval(ulong clientId)
	{
		if (clientIdConnectionApprovalMap.ContainsKey(clientId))
		{
			clientIdConnectionApprovalMap.Remove(clientId);
		}
	}

	public ConnectionApproval GetConnectionApprovalByClientId(ulong clientId)
	{
		if (!clientIdConnectionApprovalMap.ContainsKey(clientId))
		{
			return null;
		}
		return clientIdConnectionApprovalMap[clientId];
	}

	public ConnectionApproval GetConnectionApprovalBySteamId(string steamId)
	{
		return clientIdConnectionApprovalMap.Values.FirstOrDefault((ConnectionApproval approval) => approval.ConnectionData.SteamId == steamId);
	}

	public void ApproveConnection(ulong clientId, PlayerData playerData)
	{
		if (clientIdConnectionApprovalMap.ContainsKey(clientId))
		{
			ConnectionApproval connectionApproval = clientIdConnectionApprovalMap[clientId];
			connectionApproval.Approve(playerData);
			Logger.Info($"Approved connection for client {clientId}");
			EventManager.TriggerEvent("Event_Server_OnConnectionApproved", new Dictionary<string, object>
			{
				{ "clientId", clientId },
				{ "connectionApproval", connectionApproval }
			});
		}
	}

	public string GetRejectionReason(ConnectionRejectionCode code, string message = null)
	{
		ConnectionRejection connectionRejection = new ConnectionRejection
		{
			code = code,
			message = message,
			data = null
		};
		if (code == ConnectionRejectionCode.MissingMods)
		{
			connectionRejection.data = new ConnectionRejectionData
			{
				clientRequiredModIds = ServerManager.ServerConfig.ClientRequiredModIds
			};
		}
		return JsonSerializer.Serialize(connectionRejection, new JsonSerializerOptions
		{
			WriteIndented = true
		});
	}

	public void RejectConnection(ulong clientId, ConnectionRejectionCode code, string message = null)
	{
		if (clientIdConnectionApprovalMap.ContainsKey(clientId))
		{
			ConnectionApproval connectionApproval = clientIdConnectionApprovalMap[clientId];
			connectionApproval.Reject(GetRejectionReason(code, message));
			Logger.Info($"Rejected connection for client {clientId}: {Utils.GetConnectionRejectionMessage(code, message)}");
			EventManager.TriggerEvent("Event_Server_OnConnectionRejected", new Dictionary<string, object>
			{
				{ "clientId", clientId },
				{ "connectionApproval", connectionApproval },
				{ "rejectionCode", code }
			});
			RemoveConnectionApproval(clientId);
		}
	}

	public ConnectionRejectionCode? GetConnectionRejectionCode(ConnectionApproval connectionApproval)
	{
		int num = NetworkManager.Singleton.ConnectedClientsList.Count((NetworkClient c) => c.ClientId != connectionApproval.ClientID);
		bool flag = !string.IsNullOrEmpty(ServerManager.ServerConfig.password);
		bool useWhitelist = ServerManager.ServerConfig.useWhitelist;
		if (connectionApproval.ConnectionData == null || connectionApproval.ConnectionData.SteamId == null || connectionApproval.ConnectionData.EnabledModIds == null)
		{
			return ConnectionRejectionCode.Unknown;
		}
		bool flag2 = ServerManager.AdminManager.IsSteamIdAdmin(connectionApproval.ConnectionData.SteamId);
		bool flag3 = num >= ServerManager.ServerConfig.maxPlayers;
		bool flag4 = TimeoutManager.IsSteamIdTimedOut(connectionApproval.ConnectionData.SteamId);
		bool flag5 = BanManager.IsSteamIdBanned(connectionApproval.ConnectionData.SteamId);
		bool flag6 = BanManager.IsIpAddressBanned(connectionApproval.IpAddress);
		bool flag7 = WhitelistManager.IsSteamIdWhitelisted(connectionApproval.ConnectionData.SteamId);
		bool flag8 = string.IsNullOrEmpty(connectionApproval.ConnectionData.Password) & flag;
		bool flag9 = connectionApproval.ConnectionData.Password == ServerManager.ServerConfig.password || !flag;
		bool flag10 = ServerManager.ServerConfig.ClientRequiredModIds.Any((string modId) => !Enumerable.Contains(connectionApproval.ConnectionData.EnabledModIds, modId));
		if (flag3 && !flag2)
		{
			return ConnectionRejectionCode.ServerFull;
		}
		if (flag4)
		{
			return ConnectionRejectionCode.TimedOut;
		}
		if (flag5 | flag6)
		{
			return ConnectionRejectionCode.Banned;
		}
		if (useWhitelist && !flag7)
		{
			return ConnectionRejectionCode.NotWhitelisted;
		}
		if (flag8)
		{
			return ConnectionRejectionCode.MissingPassword;
		}
		if (!flag9)
		{
			return ConnectionRejectionCode.InvalidPassword;
		}
		if (flag10)
		{
			return ConnectionRejectionCode.MissingMods;
		}
		return null;
	}

	private void OnConnectionApprovalStarted(ulong clientId, ConnectionApproval connectionApproval)
	{
		Logger.Info($"Started connection approval for client {clientId}");
		ConnectionRejectionCode? connectionRejectionCode = GetConnectionRejectionCode(connectionApproval);
		if (!connectionRejectionCode.HasValue)
		{
			connectionApproval.Halt();
			WebSocketManager.Emit("serverConnectionApprovalRequest", new Dictionary<string, object>
			{
				{
					"steamId",
					connectionApproval.ConnectionData.SteamId
				},
				{
					"key",
					connectionApproval.ConnectionData.Key
				}
			}, "serverConnectionApprovalResponse");
		}
		else
		{
			RejectConnection(clientId, connectionRejectionCode.Value);
		}
	}
}
