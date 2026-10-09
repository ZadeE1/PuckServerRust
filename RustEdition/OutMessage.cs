using System.Collections.Generic;
using System.Text.Json;

public class OutMessage
{
	private static readonly Logger Logger = new Logger("WebsocketManager");

	public readonly string MessageName;

	public readonly Dictionary<string, object> Data;

	public readonly string ResponseMessageName;

	public bool IsRequestMessage => ResponseMessageName != null;

	public OutMessage(string messageName, Dictionary<string, object> data = null, string responseMessageName = null)
	{
		MessageName = messageName;
		Data = data;
		ResponseMessageName = responseMessageName;
	}

	public override string ToString()
	{
		try
		{
			return JsonSerializer.Serialize(Data, WebSocketManager.JsonOptions);
		}
		catch
		{
			return null;
		}
	}
}
