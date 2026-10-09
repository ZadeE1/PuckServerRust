using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;

public class ChatManager : NetworkBehaviourSingleton<ChatManager>
{
	private static readonly Logger Logger = new Logger("ChatManager");

	[Header("Settings")]
	[SerializeField]
	private int maxChatMessageLength = 128;

	[SerializeField]
	private int maxChatMessages = 100;

	[SerializeField]
	private SerializedDictionary<QuickChatCategory, QuickChat[]> quickChats = new SerializedDictionary<QuickChatCategory, QuickChat[]>();

	private List<ChatMessage> chatMessages = new List<ChatMessage>();

	private bool isQuickChatEnabled;

	private QuickChatCategory? quickChatCategory;

	private Tween quickChatTimeoutTween;

	public void AddChatMessage(ChatMessage chatMessage)
	{
		if (chatMessages.Count >= maxChatMessages)
		{
			RemoveChatMessage(chatMessages[0]);
		}
		chatMessages.Add(chatMessage);
		EventManager.TriggerEvent("Event_OnChatMessageAdded", new Dictionary<string, object> { { "chatMessage", chatMessage } });
	}

	public void RemoveChatMessage(ChatMessage chatMessage)
	{
		chatMessages.Remove(chatMessage);
		EventManager.TriggerEvent("Event_OnChatMessageRemoved", new Dictionary<string, object> { { "chatMessage", chatMessage } });
	}

	public void ClearChatMessages()
	{
		chatMessages.Clear();
		EventManager.TriggerEvent("Event_OnChatMessagesCleared");
	}

	public string ParseChatMessageContent(string content)
	{
		content = content.Trim();
		if (content.Length > maxChatMessageLength)
		{
			content = content.Substring(0, maxChatMessageLength);
		}
		return content;
	}

	public void SetQuickChatEnabled(bool isEnabled, QuickChatCategory? category = null)
	{
		isQuickChatEnabled = isEnabled;
		quickChatCategory = category;
		if (isQuickChatEnabled && quickChatCategory.HasValue && quickChats.ContainsKey(quickChatCategory.Value))
		{
			EventManager.TriggerEvent("Event_OnQuickChatEnabled", new Dictionary<string, object>
			{
				{ "category", quickChatCategory.Value },
				{
					"quickChats",
					quickChats[quickChatCategory.Value]
				}
			});
		}
		else
		{
			EventManager.TriggerEvent("Event_OnQuickChatDisabled");
		}
	}

	public void Client_SendChatMessage(string content, bool isQuickChat, bool isTeamChat)
	{
		content = ParseChatMessageContent(content);
		if (!string.IsNullOrEmpty(content))
		{
			Client_SendChatMessageRpc(content, isQuickChat, isTeamChat);
		}
	}

	public void Client_QuickChatAction(int index)
	{
		if (isQuickChatEnabled)
		{
			if (quickChatCategory.HasValue && quickChats.ContainsKey(quickChatCategory.Value) && index >= 0 && index < quickChats[quickChatCategory.Value].Length)
			{
				QuickChat quickChat = quickChats[quickChatCategory.Value][index];
				SetQuickChatEnabled(isEnabled: false);
				Client_SendChatMessageRpc(quickChat.Content, isQuickChat: true, quickChat.IsTeamChat);
			}
		}
		else if (Enum.IsDefined(typeof(QuickChatCategory), index))
		{
			SetQuickChatEnabled(isEnabled: true, (QuickChatCategory)index);
			quickChatTimeoutTween?.Kill();
			quickChatTimeoutTween = DOVirtual.DelayedCall(5f, () =>
			{
				SetQuickChatEnabled(isEnabled: false);
			});
		}
	}

	[Rpc(SendTo.Server, DeferLocal = true)]
	private void Client_SendChatMessageRpc(string content, bool isQuickChat, bool isTeamChat, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcParams rpcParams2 = rpcParams;
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(3638797367u, rpcParams2, attributeParams, SendTo.Server, RpcDelivery.Reliable);
			bool value = content != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(content);
			}
			bufferWriter.WriteValueSafe(in isQuickChat, default(FastBufferWriter.ForPrimitives));
			bufferWriter.WriteValueSafe(in isTeamChat, default(FastBufferWriter.ForPrimitives));
			__endSendRpc(ref bufferWriter, 3638797367u, rpcParams, attributeParams, SendTo.Server, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			return;
		}
		__rpc_exec_stage = __RpcExecStage.Send;
		content = ParseChatMessageContent(content.ToString());
		if (string.IsNullOrEmpty(content.ToString()))
		{
			return;
		}
		ulong senderClientId = rpcParams.Receive.SenderClientId;
		Player component = NetworkManager.Singleton.ConnectedClients[senderClientId].PlayerObject.GetComponent<Player>();
		if (!component)
		{
			return;
		}
		if (!component.IsChatAvailable)
		{
			Server_SendChatMessage("Chat timeout", "#e74c3c", senderClientId);
			return;
		}
		component.Server_ConsumeChatTicket();
		Logger.Info($"Received chat message from player {component.Username.Value} ({senderClientId}): {content}");
		if (content[0] == '/')
		{
			string[] array = content.ToString().Split(" ");
			string value2 = array[0].ToLower();
			string[] value3 = array.Skip(1).ToArray();
			EventManager.TriggerEvent("Event_Server_OnChatCommand", new Dictionary<string, object>
			{
				{ "clientId", senderClientId },
				{ "command", value2 },
				{ "args", value3 }
			});
		}
		else if (component.IsMuted.Value)
		{
			Server_SendChatMessage("Chat disabled", "#e74c3c", senderClientId);
		}
		else
		{
			ChatMessage value4 = new ChatMessage
			{
				SteamID = component.SteamId.Value,
				Username = component.Username.Value,
				Team = component.Team,
				Content = content,
				Timestamp = Utils.GetTimestamp(),
				IsQuickChat = isQuickChat,
				IsTeamChat = isTeamChat,
				IsSystem = false
			};
			EventManager.TriggerEvent("Event_Server_OnChatMessageReceived", new Dictionary<string, object> { { "chatMessage", value4 } });
		}
	}

	public void Server_SendChatMessage(ChatMessage chatMessage, params ulong[] clientIds)
	{
		Server_SendChatMessageRpc(chatMessage, RpcTarget.Group(clientIds, RpcTargetUse.Temp));
	}

	public void Server_SendChatMessage(string content, string color, params ulong[] clientIds)
	{
		ChatMessage chatMessage = new ChatMessage
		{
			SteamID = null,
			Username = null,
			Team = null,
			Content = StringUtils.WrapInColor(content, color),
			Timestamp = Utils.GetTimestamp(),
			IsQuickChat = false,
			IsTeamChat = false,
			IsSystem = true
		};
		Server_SendChatMessage(chatMessage, clientIds);
	}

	public void Server_BroadcastChatMessage(ChatMessage chatMessage)
	{
		Server_SendChatMessageRpc(chatMessage, RpcTarget.Everyone);
	}

	public void Server_BroadcastChatMessage(string content, string color = null)
	{
		ChatMessage chatMessage = new ChatMessage
		{
			SteamID = null,
			Username = null,
			Team = null,
			Content = ((color == null) ? content : StringUtils.WrapInColor(content, color)),
			Timestamp = Utils.GetTimestamp(),
			IsQuickChat = false,
			IsTeamChat = false,
			IsSystem = true
		};
		Server_BroadcastChatMessage(chatMessage);
	}

	[Rpc(SendTo.SpecifiedInParams, InvokePermission = RpcInvokePermission.Server, DeferLocal = true)]
	private void Server_SendChatMessageRpc(ChatMessage chatMessage, RpcParams rpcParams = default(RpcParams))
	{
		NetworkManager networkManager = NetworkManager;
		if ((object)networkManager == null || !networkManager.IsListening)
		{
			Debug.LogError("Rpc methods can only be invoked after starting the NetworkManager!");
			return;
		}
		if (__rpc_exec_stage != __RpcExecStage.Execute)
		{
			RpcAttribute.RpcAttributeParams attributeParams = new RpcAttribute.RpcAttributeParams
			{
				InvokePermission = RpcInvokePermission.Server,
				DeferLocal = true
			};
			FastBufferWriter bufferWriter = __beginSendRpc(846499610u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
			bool value = chatMessage != null;
			bufferWriter.WriteValueSafe(in value, default(FastBufferWriter.ForPrimitives));
			if (value)
			{
				bufferWriter.WriteValueSafe(in chatMessage, default(FastBufferWriter.ForNetworkSerializable));
			}
			__endSendRpc(ref bufferWriter, 846499610u, rpcParams, attributeParams, SendTo.SpecifiedInParams, RpcDelivery.Reliable);
		}
		if (__rpc_exec_stage == __RpcExecStage.Execute)
		{
			__rpc_exec_stage = __RpcExecStage.Send;
			AddChatMessage(chatMessage);
		}
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		__registerRpc(3638797367u, __rpc_handler_3638797367, "Client_SendChatMessageRpc", RpcInvokePermission.Everyone);
		__registerRpc(846499610u, __rpc_handler_846499610, "Server_SendChatMessageRpc", RpcInvokePermission.Server);
		base.__initializeRpcs();
	}

	private static void __rpc_handler_3638797367(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			string s = null;
			if (value)
			{
				reader.ReadValueSafe(out s, false);
			}
			reader.ReadValueSafe(out bool value2, default(FastBufferWriter.ForPrimitives));
			reader.ReadValueSafe(out bool value3, default(FastBufferWriter.ForPrimitives));
			RpcParams ext = rpcParams.Ext;
			((ChatManager)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((ChatManager)target).Client_SendChatMessageRpc(s, value2, value3, ext);
			((ChatManager)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	private static void __rpc_handler_846499610(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
	{
		NetworkManager networkManager = target.NetworkManager;
		if ((object)networkManager != null && networkManager.IsListening)
		{
			reader.ReadValueSafe(out bool value, default(FastBufferWriter.ForPrimitives));
			ChatMessage value2 = null;
			if (value)
			{
				reader.ReadValueSafe(out value2, default(FastBufferWriter.ForNetworkSerializable));
			}
			RpcParams ext = rpcParams.Ext;
			((ChatManager)target).__rpc_exec_stage = __RpcExecStage.Execute;
			((ChatManager)target).Server_SendChatMessageRpc(value2, ext);
			((ChatManager)target).__rpc_exec_stage = __RpcExecStage.Send;
		}
	}

	protected override string __getTypeName()
	{
		return "ChatManager";
	}
}
