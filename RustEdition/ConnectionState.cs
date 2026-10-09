using System;

public struct ConnectionState
{
	public Connection Connection = null;

	public ConnectionRejection ConnectionRejection = null;

	public Disconnection Disconnection = null;

	public Connection LastConnection = null;

	public Connection PendingConnection = null;

	public ConnectionPhase Phase = ConnectionPhase.Disconnected;

	public ConnectionState()
	{
	}

	public bool Equals(ConnectionState other)
	{
		if (Connection == other.Connection && LastConnection == other.LastConnection && ConnectionRejection == other.ConnectionRejection && Disconnection == other.Disconnection && PendingConnection == other.PendingConnection)
		{
			return Phase == other.Phase;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is ConnectionState other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Connection, LastConnection, ConnectionRejection, Disconnection, PendingConnection, Phase);
	}

	public override string ToString()
	{
		return string.Format("Connection: {0}, LastConnection: {1}, ConnectionRejection: {2}, Disconnection: {3}, PendingConnection: {4}, Phase: {5}", Connection?.ToString() ?? "null", LastConnection?.ToString() ?? "null", ConnectionRejection?.ToString() ?? "null", Disconnection?.ToString() ?? "null", PendingConnection?.ToString() ?? "null", Phase);
	}
}
