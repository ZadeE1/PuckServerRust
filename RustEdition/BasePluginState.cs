using System;

public class BasePluginState
{
	public string Path;

	public bool IsReady;

	public bool IsEnabled;

	public bool Equals(BasePluginState other)
	{
		if (Path == other.Path && IsReady == other.IsReady)
		{
			return IsEnabled == other.IsEnabled;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is BasePluginState other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Path, IsReady, IsEnabled);
	}

	public override string ToString()
	{
		return $"Path={Path}, IsReady={IsReady}, IsEnabled={IsEnabled}";
	}
}
