using System;
using System.Collections.Generic;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SocketIOClient;
using SocketIOClient.Common;
using SocketIOClient.Common.Messages;
using SocketIOClient.Protocol.WebSocket;
using UnityEngine;

public static class WebSocketManager
{
	private static readonly Logger Logger = new Logger("WebsocketManager");

	public static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
	{
		Converters = { (JsonConverter)new JsonStringEnumConverter() }
	};

	private static Dictionary<string, Action<Dictionary<string, object>>> events = new Dictionary<string, Action<Dictionary<string, object>>>();

	private static SocketIO socket = null;

	private static CancellationTokenSource cancellationTokenSource = null;

	private static string url = null;

	private static bool isIntentionalDisconnect = false;

	private static long lastServerPingTicks = 0L;

	private static CancellationTokenSource watchdogCancellationTokenSource = null;

	private static readonly System.Random reconnectRandom = new System.Random();

	private static bool forcePolling = false;

	private const string BackendUrlAssetName = "backend_url";

	public static bool IsConnected
	{
		get
		{
			if (socket != null)
			{
				return socket.Connected;
			}
			return false;
		}
	}

	public static bool IsReconnecting
	{
		get
		{
			if (IsConnectionInProgress)
			{
				return !IsConnected;
			}
			return false;
		}
	}

	public static bool IsConnectionInProgress => cancellationTokenSource != null;

	public static string BackendUrl
	{
		get
		{
			TextAsset textAsset = Resources.Load<TextAsset>("backend_url");
			if (textAsset == null)
			{
				Logger.Error("Error loading backend url asset, falling back to ws://localhost:8080");
				return "ws://localhost:8080";
			}
			return textAsset.text.Trim();
		}
	}

	public static void Initialize()
	{
		forcePolling = bool.TryParse(Utils.GetCommandLineArgument("--polling"), out var result) && result;
		WebSocketManagerController.Initialize();
	}

	public static void Dispose()
	{
		WebSocketManagerController.Dispose();
	}

	public static async Task Connect(string targetUrl)
	{
		url = targetUrl;
		isIntentionalDisconnect = false;
		if (IsConnectionInProgress)
		{
			return;
		}
		Logger.Info("WebSocket connection started");
		TriggerMessage("connecting", new Dictionary<string, object> { { "url", url } });
		cancellationTokenSource = new CancellationTokenSource();
		CancellationToken token = cancellationTokenSource.Token;
		int attempt = 0;
		while (!token.IsCancellationRequested && !isIntentionalDisconnect)
		{
			try
			{
				CreateSocket(url);
				await socket.ConnectAsync(token);
			}
			catch (OperationCanceledException)
			{
				Logger.Warning("WebSocket connection cancelled");
				DisposeSocket();
			}
			catch (Exception exception)
			{
				attempt++;
				int reconnectDelay = GetReconnectDelay(attempt);
				Logger.Error($"WebSocket connection attempt {attempt} failed ({DescribeException(exception)}); retrying in {reconnectDelay}ms");
				DisposeSocket();
				TriggerMessage("reconnecting", new Dictionary<string, object>
				{
					{ "attempt", attempt },
					{ "delay", reconnectDelay }
				});
				try
				{
					await Task.Delay(reconnectDelay, token);
				}
				catch (OperationCanceledException)
				{
					goto end_IL_0122;
				}
				continue;
				end_IL_0122:;
			}
			break;
		}
		cancellationTokenSource = null;
	}

	public static async Task CancelConnection()
	{
		if (!IsConnectionInProgress)
		{
			return;
		}
		if (!cancellationTokenSource.IsCancellationRequested)
		{
			cancellationTokenSource.Cancel();
		}
		await Task.Run(async () =>
		{
			while (IsConnectionInProgress)
			{
				await Task.Yield();
			}
		});
	}

	public static async Task Disconnect()
	{
		Logger.Info("WebSocket disconnection started");
		isIntentionalDisconnect = true;
		StopWatchdog();
		if (IsConnectionInProgress)
		{
			await CancelConnection();
		}
		if (socket != null && socket.Connected)
		{
			await socket.DisconnectAsync();
		}
		DisposeSocket();
	}

	private static int GetReconnectDelay(int attempt)
	{
		double num = Math.Min(500.0 * Math.Pow(2.0, attempt - 1), 30000.0);
		return (int)(num / 2.0 + reconnectRandom.NextDouble() * (num / 2.0));
	}

	private static void StartWatchdog()
	{
		StopWatchdog();
		Interlocked.Exchange(ref lastServerPingTicks, DateTime.UtcNow.Ticks);
		watchdogCancellationTokenSource = new CancellationTokenSource();
		RunWatchdog(watchdogCancellationTokenSource.Token);
	}

	private static void StopWatchdog()
	{
		if (watchdogCancellationTokenSource != null)
		{
			watchdogCancellationTokenSource.Cancel();
			watchdogCancellationTokenSource = null;
		}
	}

	private static async Task RunWatchdog(CancellationToken token)
	{
		try
		{
			while (!token.IsCancellationRequested)
			{
				await Task.Delay(1000, token);
				if (IsConnected)
				{
					long ticks = Interlocked.Read(ref lastServerPingTicks);
					double totalMilliseconds = (DateTime.UtcNow - new DateTime(ticks, DateTimeKind.Utc)).TotalMilliseconds;
					if (totalMilliseconds >= 8000.0)
					{
						Logger.Warning($"WebSocket received no server ping for {totalMilliseconds:F0}ms " + $"(timeout {8000}ms) — connection is dead, reconnecting");
						ForceReconnect("ping timeout");
						break;
					}
				}
			}
		}
		catch (OperationCanceledException)
		{
		}
	}

	private static void ForceReconnect(string reason)
	{
		StopWatchdog();
		DisposeSocket();
		TriggerMessage("disconnected", new Dictionary<string, object> { { "reason", reason } });
		if (!isIntentionalDisconnect)
		{
			Connect(url);
		}
	}

	public static void CreateSocket(string url)
	{
		if (socket != null)
		{
			DisposeSocket();
		}
		SocketIOOptions socketIOOptions = new SocketIOOptions
		{
			ConnectionTimeout = TimeSpan.FromMilliseconds(5000.0),
			Transport = ((!forcePolling) ? TransportProtocol.WebSocket : TransportProtocol.Polling),
			AutoUpgrade = false,
			Reconnection = false
		};
		Logger.Info($"Creating WebSocket (url: {url}, transport: {socketIOOptions.Transport})");
		socket = new SocketIO(new Uri(url), socketIOOptions, (IServiceCollection services) =>
		{
			services.AddSystemTextJson(JsonOptions);
			if (!url.StartsWith("wss://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
			{
				services.AddSingleton(new WebSocketOptions
				{
					RemoteCertificateValidationCallback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => true
				});
			}
		});
		socket.OnConnected += OnConnected;
		socket.OnDisconnected += OnDisconnected;
		socket.OnError += OnError;
		socket.OnPing += OnPing;
		socket.OnAny(OnAny);
	}

	public static void DisposeSocket()
	{
		if (socket != null)
		{
			Logger.Info("Disposing WebSocket");
			socket.OnConnected -= OnConnected;
			socket.OnDisconnected -= OnDisconnected;
			socket.OnError -= OnError;
			socket.OnPing -= OnPing;
			socket.OffAny(OnAny);
			socket = null;
		}
	}

	public static void Emit(string messageName, Dictionary<string, object> data = null, string responseMessageName = null)
	{
		OutMessage outMessage = new OutMessage(messageName, data, responseMessageName);
		if (outMessage.IsRequestMessage)
		{
			Logger.Info($"WebSocket sending request message {outMessage.MessageName} ({outMessage})");
			Func<IDataMessage, Task> ack = (IDataMessage dataMessage) => OnCallback(outMessage, dataMessage);
			socket.EmitAsync(outMessage.MessageName, (outMessage.Data == null) ? Array.Empty<object>() : new object[1] { outMessage.Data }, ack);
		}
		else
		{
			Logger.Info($"WebSocket sending message {outMessage.MessageName} ({outMessage})");
			socket.EmitAsync(outMessage.MessageName, (outMessage.Data == null) ? Array.Empty<object>() : new object[1] { outMessage.Data });
		}
		TriggerMessage("emit", new Dictionary<string, object> { { "messageName", outMessage.MessageName } });
	}

	private static void OnConnected(object sender, EventArgs args)
	{
		Logger.Info("WebSocket connected");
		StartWatchdog();
		TriggerMessage("connected", new Dictionary<string, object> { { "socket", socket } });
	}

	private static void OnDisconnected(object sender, string reason)
	{
		Logger.Info("WebSocket disconnected (" + reason + ")");
		StopWatchdog();
		TriggerMessage("disconnected", new Dictionary<string, object> { { "reason", reason } });
		if (!isIntentionalDisconnect)
		{
			Connect(url);
		}
	}

	private static void OnError(object sender, string error)
	{
		Logger.Error("WebSocket error: " + error);
	}

	private static void OnPing(object sender, EventArgs args)
	{
		Interlocked.Exchange(ref lastServerPingTicks, DateTime.UtcNow.Ticks);
	}

	private static string DescribeException(Exception exception)
	{
		string text = "";
		for (Exception ex = exception; ex != null; ex = ex.InnerException)
		{
			text = text + ((text.Length > 0) ? " ---> " : "") + ex.GetType().Name + ": " + ex.Message;
		}
		return text;
	}

	private static Task OnAny(string messageName, IEventContext eventContext)
	{
		InMessage inMessage = new InMessage(messageName, eventContext);
		Logger.Info($"WebSocket received message {messageName} ({inMessage})");
		TriggerMessage(messageName, new Dictionary<string, object> { { "inMessage", inMessage } });
		return Task.CompletedTask;
	}

	private static Task OnCallback(OutMessage outMessage, IDataMessage dataMessage)
	{
		InMessage inMessage = new InMessage(outMessage.ResponseMessageName, null, dataMessage);
		Logger.Info($"WebSocket received response to message {outMessage.MessageName} -> {inMessage.MessageName} ({inMessage})");
		TriggerMessage(inMessage.MessageName, new Dictionary<string, object>
		{
			{ "outMessage", outMessage },
			{ "inMessage", inMessage }
		});
		return Task.CompletedTask;
	}

	public static void AddMessageListener(string messageName, Action<Dictionary<string, object>> listener)
	{
		if (events.ContainsKey(messageName))
		{
			Dictionary<string, Action<Dictionary<string, object>>> dictionary = events;
			dictionary[messageName] = (Action<Dictionary<string, object>>)Delegate.Combine(dictionary[messageName], listener);
		}
		else
		{
			Action<Dictionary<string, object>> a = null;
			a = (Action<Dictionary<string, object>>)Delegate.Combine(a, listener);
			events.Add(messageName, a);
		}
	}

	public static void RemoveMessageListener(string messageName, Action<Dictionary<string, object>> listener)
	{
		if (events.ContainsKey(messageName))
		{
			Dictionary<string, Action<Dictionary<string, object>>> dictionary = events;
			dictionary[messageName] = (Action<Dictionary<string, object>>)Delegate.Remove(dictionary[messageName], listener);
		}
	}

	public static void TriggerMessage(string messageName, Dictionary<string, object> message = null)
	{
		MonoBehaviourSingleton<ThreadManager>.Instance.Enqueue(() =>
		{
			if (events.ContainsKey(messageName))
			{
				events[messageName]?.Invoke(message);
			}
		});
	}
}
