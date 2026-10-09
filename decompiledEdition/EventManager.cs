using System;
using System.Collections.Generic;
using Unity.Netcode;

public static class EventManager
{
	private static readonly Logger Logger = new Logger("EventManager");

	private static Dictionary<string, List<Action<Dictionary<string, object>>>> events = new Dictionary<string, List<Action<Dictionary<string, object>>>>();

	public static void Initialize()
	{
	}

	public static void Dispose()
	{
	}

	public static void AddEventListener(string eventName, Action<Dictionary<string, object>> listener)
	{
		if (!events.TryGetValue(eventName, out var value))
		{
			events.Add(eventName, new List<Action<Dictionary<string, object>>> { listener });
		}
		else
		{
			List<Action<Dictionary<string, object>>> list = new List<Action<Dictionary<string, object>>>(value);
			list.Add(listener);
			events[eventName] = list;
		}
	}

	public static void RemoveEventListener(string eventName, Action<Dictionary<string, object>> listener)
	{
		if (!events.TryGetValue(eventName, out var value))
		{
			Logger.Warning("Tried to remove listener for event " + eventName + ", but no listener was registered for it");
			return;
		}
		List<Action<Dictionary<string, object>>> list = new List<Action<Dictionary<string, object>>>(value);
		list.Remove(listener);
		if (list.Count == 0)
		{
			events.Remove(eventName);
		}
		else
		{
			events[eventName] = list;
		}
	}

	public static bool HasEventListeners(string eventName)
	{
		return events.ContainsKey(eventName);
	}

	public static void TriggerEvent(string eventName, Dictionary<string, object> message = null)
	{
		if (!events.TryGetValue(eventName, out var value))
		{
			return;
		}
		bool flag = eventName.StartsWith("Event_Server_", StringComparison.Ordinal);
		bool flag2 = eventName.StartsWith("Event_Client_", StringComparison.Ordinal);
		bool flag3 = eventName.StartsWith("Event_Everyone_", StringComparison.Ordinal);
		if ((flag2 | flag | flag3) && (!NetworkManager.Singleton || !NetworkManager.Singleton.IsListening))
		{
			Logger.Warning("Triggering network event " + eventName + " without a NetworkManager listening");
		}
		if (message == null)
		{
			message = new Dictionary<string, object> { { "eventName", eventName } };
		}
		else if (!message.ContainsKey("eventName"))
		{
			message.Add("eventName", eventName);
		}
		for (int i = 0; i < value.Count; i++)
		{
			try
			{
				value[i](message);
			}
			catch (Exception arg)
			{
				Logger.Error($"Listener for event {eventName} threw an exception: {arg}");
			}
		}
	}
}
