using System;
using System.Collections.Generic;
using System.Text.Json;
using SocketIOClient;
using SocketIOClient.Common.Messages;

public class InMessage
{
	private static readonly Logger Logger = new Logger("WebsocketManager");

	public readonly string MessageName;

	public readonly IEventContext EventContext;

	public readonly IDataMessage DataMessage;

	public InMessage(string messageName, IEventContext eventContext = null, IDataMessage dataMessage = null)
	{
		MessageName = messageName;
		EventContext = eventContext;
		DataMessage = dataMessage;
	}

	public T GetData<T>()
	{
		if (EventContext != null)
		{
			return EventContext.GetValue<T>(0);
		}
		if (DataMessage != null)
		{
			return DataMessage.GetValue<T>(0);
		}
		return default;
	}

	public void Respond(Dictionary<string, object> data)
	{
		try
		{
			if (EventContext == null)
			{
				throw new Exception("Cannot send response to message without event context");
			}
			string text = JsonSerializer.Serialize(data, WebSocketManager.JsonOptions);
			Logger.Info("WebSocket sending response to message " + MessageName + " (" + text + ")");
			EventContext.SendAckDataAsync(new object[1] { data });
		}
		catch (Exception ex)
		{
			Logger.Error("WebSocket failed to send response to message " + MessageName + ": " + ex.Message);
		}
	}

	public override string ToString()
	{
		if (EventContext != null)
		{
			return EventContext.RawText;
		}
		if (DataMessage != null)
		{
			return DataMessage.RawText;
		}
		return null;
	}
}
