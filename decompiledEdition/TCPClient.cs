using System;
using System.Text;
using System.Threading.Tasks;
using SuperSimpleTcp;

public class TCPClient
{
	private static readonly Logger Logger = new Logger("TCPClient");

	public SimpleTcpClient Client;

	public EndPoint EndPoint;

	public bool IsConnecting { get; private set; }

	public bool IsConnected => Client.IsConnected;

	public event Action OnConnected;

	public event Action OnConnectionFailed;

	public event Action OnDisconnected;

	public event Action<string> OnMessageReceived;

	public event Action<string> OnMessageSent;

	public TCPClient(EndPoint endPoint, int connectTimeoutMs = 1000, int readTimeoutMs = 1000)
	{
		EndPoint = endPoint;
		Client = new SimpleTcpClient(EndPoint.ipAddress, EndPoint.port);
		Client.Settings.NoDelay = true;
		Client.Settings.UseAsyncDataReceivedEvents = false;
		Client.Settings.ConnectTimeoutMs = connectTimeoutMs;
		Client.Settings.ReadTimeoutMs = readTimeoutMs;
		Client.Events.Connected += (object sender, ConnectionEventArgs args) =>
		{
			OnConnected?.Invoke();
		};
		Client.Events.Disconnected += (object sender, ConnectionEventArgs args) =>
		{
			OnDisconnected?.Invoke();
		};
		Client.Events.DataReceived += (object sender, DataReceivedEventArgs args) =>
		{
			OnDataReceived(sender, args);
		};
	}

	public void Connect()
	{
		try
		{
			if (!IsConnecting)
			{
				IsConnecting = true;
				Client.Connect();
				IsConnecting = false;
			}
		}
		catch (TimeoutException)
		{
			IsConnecting = false;
			Logger.Error($"Connection to server {EndPoint} timed out");
			OnConnectionFailed?.Invoke();
		}
		catch (Exception ex2)
		{
			IsConnecting = false;
			Logger.Error($"Connection to server {EndPoint} failed: {ex2.Message}");
			OnConnectionFailed?.Invoke();
		}
	}

	public void ConnectAsync()
	{
		Task.Run(() =>
		{
			Connect();
		});
	}

	public void Disconnect()
	{
		try
		{
			Client.Disconnect();
		}
		catch (Exception ex)
		{
			Logger.Error("Error disconnecting: " + ex.Message);
		}
	}

	public void DisconnectAsync()
	{
		Task.Run(() =>
		{
			Disconnect();
		});
	}

	public void SendMessage(string message)
	{
		try
		{
			byte[] bytes = Encoding.UTF8.GetBytes(message);
			Client.Send(bytes);
			OnMessageSent?.Invoke(message);
		}
		catch (Exception ex)
		{
			Logger.Error($"Error sending message to server {EndPoint}: {ex.Message}");
		}
	}

	public void SendMessageAsync(string message)
	{
		Task.Run(() =>
		{
			SendMessage(message);
		});
	}

	private void OnDataReceived(object sender, DataReceivedEventArgs args)
	{
		try
		{
			string obj = Encoding.UTF8.GetString(args.Data);
			OnMessageReceived?.Invoke(obj);
		}
		catch (Exception ex)
		{
			Logger.Error($"Error deserializing message from server {EndPoint}: {ex.Message}");
		}
	}
}
