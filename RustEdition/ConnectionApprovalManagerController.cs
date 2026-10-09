using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ConnectionApprovalManagerController : MonoBehaviour
{
	private ConnectionApprovalManager connectionApprovalManager;

	private void Awake()
	{
		connectionApprovalManager = GetComponent<ConnectionApprovalManager>();
		EventManager.AddEventListener("Event_Everyone_OnClientConnected", Event_Everyone_OnClientConnected);
		EventManager.AddEventListener("Event_Everyone_OnClientDisconnected", Event_Everyone_OnClientDisconnected);
		EventManager.AddEventListener("Event_Server_OnServerStopped", Event_Server_OnServerStopped);
		EventManager.AddEventListener("Event_Server_OnLoadSceneEventCompleted", Event_Server_OnLoadSceneEventCompleted);
		EventManager.AddEventListener("Event_Server_OnConnectionApproved", Event_Server_OnConnectionApproved);
		WebSocketManager.AddMessageListener("serverConnectionApprovalResponse", WebSocket_Event_OnServerConnectionApprovalResponse);
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnClientConnected", Event_Everyone_OnClientConnected);
		EventManager.RemoveEventListener("Event_Everyone_OnClientDisconnected", Event_Everyone_OnClientDisconnected);
		EventManager.RemoveEventListener("Event_Server_OnServerStopped", Event_Server_OnServerStopped);
		EventManager.RemoveEventListener("Event_Server_OnLoadSceneEventCompleted", Event_Server_OnLoadSceneEventCompleted);
		EventManager.RemoveEventListener("Event_Server_OnConnectionApproved", Event_Server_OnConnectionApproved);
		WebSocketManager.RemoveMessageListener("serverConnectionApprovalResponse", WebSocket_Event_OnServerConnectionApprovalResponse);
	}

	private void Event_Everyone_OnClientConnected(Dictionary<string, object> message)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			ulong num = (ulong)message["clientId"];
			ConnectionApproval connectionApprovalByClientId = connectionApprovalManager.GetConnectionApprovalByClientId(num);
			if (connectionApprovalByClientId != null && connectionApprovalByClientId.IsApproved && !connectionApprovalByClientId.IsHost)
			{
				EventManager.TriggerEvent("Event_Server_OnApprovedClientConnected", new Dictionary<string, object>
				{
					{ "clientId", num },
					{ "connectionApproval", connectionApprovalByClientId }
				});
			}
		}
	}

	private void Event_Everyone_OnClientDisconnected(Dictionary<string, object> message)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			ulong num = (ulong)message["clientId"];
			ConnectionApproval connectionApprovalByClientId = connectionApprovalManager.GetConnectionApprovalByClientId(num);
			if (connectionApprovalByClientId != null && connectionApprovalByClientId.IsApproved)
			{
				EventManager.TriggerEvent("Event_Server_OnApprovedClientDisconnected", new Dictionary<string, object>
				{
					{ "clientId", num },
					{ "connectionApproval", connectionApprovalByClientId }
				});
				connectionApprovalManager.RemoveConnectionApproval(num);
			}
		}
	}

	private void Event_Server_OnServerStopped(Dictionary<string, object> message)
	{
		connectionApprovalManager.Dispose();
	}

	private void Event_Server_OnLoadSceneEventCompleted(Dictionary<string, object> message)
	{
		if ((bool)message["isInitialScene"])
		{
			connectionApprovalManager.ConsumeBufferedConnectionApprovals();
		}
	}

	private void Event_Server_OnConnectionApproved(Dictionary<string, object> message)
	{
		ulong num = (ulong)message["clientId"];
		ConnectionApproval connectionApproval = (ConnectionApproval)message["connectionApproval"];
		if (connectionApproval.IsHost)
		{
			EventManager.TriggerEvent("Event_Server_OnApprovedClientConnected", new Dictionary<string, object>
			{
				{ "clientId", num },
				{ "connectionApproval", connectionApproval }
			});
		}
	}

	private void WebSocket_Event_OnServerConnectionApprovalResponse(Dictionary<string, object> message)
	{
		OutMessage outMessage = (OutMessage)message["outMessage"];
		ServerConnectionApprovalResponse data = ((InMessage)message["inMessage"]).GetData<ServerConnectionApprovalResponse>();
		string steamId = (string)outMessage.Data["steamId"];
		ConnectionApproval connectionApprovalBySteamId = connectionApprovalManager.GetConnectionApprovalBySteamId(steamId);
		if (connectionApprovalBySteamId == null)
		{
			return;
		}
		ulong clientID = connectionApprovalBySteamId.ClientID;
		if (data.success)
		{
			ConnectionRejectionCode? connectionRejectionCode = connectionApprovalManager.GetConnectionRejectionCode(connectionApprovalBySteamId);
			bool flag = !connectionRejectionCode.HasValue;
			if (flag && BackendUtils.GetActivePlayerDataBan(data.data.playerData) != null)
			{
				connectionRejectionCode = ConnectionRejectionCode.Banned;
				flag = false;
			}
			if (flag)
			{
				connectionApprovalManager.ApproveConnection(clientID, data.data.playerData);
			}
			else
			{
				connectionApprovalManager.RejectConnection(clientID, connectionRejectionCode.Value);
			}
		}
		else
		{
			connectionApprovalManager.RejectConnection(clientID, ConnectionRejectionCode.Unknown, data.errorData.message);
		}
	}
}
