using System;

public struct TransactionState
{
	public TransactionPhase Phase;

	public bool Equals(TransactionState other)
	{
		return Phase == other.Phase;
	}

	public override bool Equals(object obj)
	{
		if (obj is TransactionState other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Phase);
	}

	public override string ToString()
	{
		return $"Phase: {Phase}";
	}
}
