using System;
using System.Linq;

public struct ReconnectionState
{
	public ReconnectionPhase Phase = ReconnectionPhase.None;

	public string Password = null;

	public string[] ClientRequiredModIds = new string[0];

	public string[] PendingReadinessModIds = new string[0];

	public string[] PendingEnablingModIds = new string[0];

	public string[] PendingModIds => PendingReadinessModIds.Union(PendingEnablingModIds).ToArray();

	public ReconnectionState()
	{
	}

	public bool IsPendingModId(string modId)
	{
		if (!Enumerable.Contains(PendingReadinessModIds, modId))
		{
			return Enumerable.Contains(PendingEnablingModIds, modId);
		}
		return true;
	}

	public bool Equals(ReconnectionState other)
	{
		if (Phase == other.Phase && Password == other.Password && Enumerable.SequenceEqual(ClientRequiredModIds, other.ClientRequiredModIds) && Enumerable.SequenceEqual(PendingReadinessModIds, other.PendingReadinessModIds))
		{
			return Enumerable.SequenceEqual(PendingEnablingModIds, other.PendingEnablingModIds);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is ReconnectionState other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Phase, Password, ClientRequiredModIds, PendingReadinessModIds, PendingEnablingModIds);
	}

	public override string ToString()
	{
		return string.Format("Phase: {0}, Password: {1}, ClientRequiredModIds: [{2}], PendingReadinessModIds: [{3}], PendingEnablingModIds: [{4}]", Phase, Password ?? "null", string.Join(", ", ClientRequiredModIds), string.Join(", ", PendingReadinessModIds), string.Join(", ", PendingEnablingModIds));
	}
}
