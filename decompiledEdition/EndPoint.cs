using System;

public class EndPoint : IEquatable<EndPoint>
{
	public string ipAddress { get; set; }

	public ushort port { get; set; }

	public EndPoint(string ipAddress, ushort port)
	{
		this.ipAddress = ipAddress;
		this.port = port;
	}

	public bool Equals(EndPoint other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if (ipAddress == other.ipAddress)
		{
			return port == other.port;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is EndPoint other)
		{
			return Equals(other);
		}
		return false;
	}

	public static bool operator ==(EndPoint a, EndPoint b)
	{
		return a?.Equals(b) ?? ((object)b == null);
	}

	public static bool operator !=(EndPoint a, EndPoint b)
	{
		return !(a == b);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(ipAddress, port);
	}

	public override string ToString()
	{
		return $"{ipAddress}:{port}";
	}
}
