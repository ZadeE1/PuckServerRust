using System;
using System.Text;
using System.Threading.Tasks;
using SuperSimpleTcp;

public class TCPServer
{
	private static readonly Logger Logger = new Logger("TCPServer");

	public SimpleTcpServer Server;

	public event Action<ushort> OnServerStarted;

	public event Action<Exception> OnServerStartFailed;

	public event Action<ushort> OnServerStopped;

	public event Action<string> OnClientConnected;

	public event Action<string> OnClientDisconnected;

	public event Action<string, string> OnMessageReceived;

	public event Action<string, string> OnMessageSent;

	public TCPServer(ushort port)
	{
		Server = new SimpleTcpServer("0.0.0.0", port);
		Server.Settings.IdleClientTimeoutMs = 1000;
		Server.Settings.NoDelay = true;
		Server.Settings.UseAsyncDataReceivedEvents = false;
		Server.Events.ClientConnected += (object sender, ConnectionEventArgs args) =>
		{
			OnClientConnected?.Invoke(args.IpPort);
		};
		Server.Events.ClientDisconnected += (object sender, ConnectionEventArgs args) =>
		{
			OnClientDisconnected?.Invoke(args.IpPort);
		};
		Server.Events.DataReceived += (object sender, DataReceivedEventArgs args) =>
		{
			OnDataReceived(sender, args);
		};
	}

	public void Start()
	{
		try
		{
			if (!Server.IsListening)
			{
				Server.Start();
				Logger.Info(string.Format("Started TCP listener on {0}:{1}", "0.0.0.0", Server.Port));
				OnServerStarted?.Invoke((ushort)Server.Port);
			}
		}
		catch (Exception ex)
		{
			Logger.Error($"Server start failed on port {Server.Port}: {ex.Message}");
			OnServerStartFailed?.Invoke(ex);
		}
	}

	public void StartAsync()
	{
		Task.Run(() =>
		{
			Start();
		});
	}

	public void Stop()
	{
		Server.Stop();
		Logger.Info($"Server stopped on port {Server.Port}");
		OnServerStopped?.Invoke((ushort)Server.Port);
	}

	public void StopAsync()
	{
		Task.Run(() =>
		{
			Stop();
		});
	}

	public void SendMessage(string ipPort, string message)
	{
		try
		{
			byte[] bytes = Encoding.UTF8.GetBytes(message);
			Server.Send(ipPort, bytes);
			OnMessageSent?.Invoke(ipPort, message);
		}
		catch (Exception ex)
		{
			Logger.Error("Error sending message to client " + ipPort + ": " + ex.Message);
		}
	}

	public void SendMessageAsync(string ipPort, string message)
	{
		Task.Run(() =>
		{
			SendMessage(ipPort, message);
		});
	}

	public void DisconnectClient(string ipPort)
	{
		try
		{
			Server.DisconnectClient(ipPort);
		}
		catch (Exception ex)
		{
			Logger.Error("Error disconnecting client " + ipPort + ": " + ex.Message);
		}
	}

	private void OnDataReceived(object sender, DataReceivedEventArgs args)
	{
		try
		{
			string arg = Encoding.UTF8.GetString(args.Data);
			OnMessageReceived?.Invoke(args.IpPort, arg);
		}
		catch (Exception ex)
		{
			Logger.Error("Error deserializing message from client " + args.IpPort + ": " + ex.Message);
		}
	}
}
