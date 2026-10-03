using System.Collections.Generic;

public class SynchronizedObjectSendBuffers
{
	private List<SynchronizedObjectData> deltas = new List<SynchronizedObjectData>();

	private List<SynchronizedObjectData> reliables = new List<SynchronizedObjectData>();

	private Dictionary<int, SynchronizedObjectData[]> resultsByLength = new Dictionary<int, SynchronizedObjectData[]>();

	public void AddDelta(SynchronizedObjectData data)
	{
		deltas.Add(data);
	}

	public void AddReliable(SynchronizedObjectData data)
	{
		reliables.Add(data);
	}

	public SynchronizedObjectData[] TakeDeltas()
	{
		return Take(deltas);
	}

	public SynchronizedObjectData[] TakeReliables()
	{
		return Take(reliables);
	}

	public void Clear()
	{
		deltas.Clear();
		reliables.Clear();
	}

	public void Reset()
	{
		Clear();
		resultsByLength.Clear();
	}

	private SynchronizedObjectData[] Take(List<SynchronizedObjectData> buffer)
	{
		SynchronizedObjectData[] reusableResult = GetReusableResult(buffer.Count);
		buffer.CopyTo(reusableResult);
		return reusableResult;
	}

	private SynchronizedObjectData[] GetReusableResult(int length)
	{
		if (!resultsByLength.TryGetValue(length, out var value))
		{
			value = new SynchronizedObjectData[length];
			resultsByLength.Add(length, value);
		}
		return value;
	}
}
