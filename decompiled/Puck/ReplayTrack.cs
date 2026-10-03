using System.Collections.Generic;
using UnityEngine;

public class ReplayTrack<TSample> where TSample : struct
{
	private const int CHUNK_SAMPLES = 1024;

	private List<TSample[]> chunks = new List<TSample[]>();

	private int retainedChunks;

	private int startTick;

	private int count;

	private int EndTick => startTick + count - 1;

	public ReplayTrack(int retainedTicks)
	{
		retainedChunks = GetChunksAlwaysCoveringAtLeast(retainedTicks);
	}

	private static int GetChunksAlwaysCoveringAtLeast(int retainedTicks)
	{
		if (retainedTicks <= 0)
		{
			return 0;
		}
		return Mathf.CeilToInt((float)retainedTicks / 1024f) + 1;
	}

	public void Append(int tick, in TSample sample)
	{
		if (count == 0)
		{
			startTick = tick;
			Store(0, in sample);
			count = 1;
		}
		else if (tick > EndTick)
		{
			RepeatLastSampleUntil(tick);
			Store(count, in sample);
			count++;
			DropExpiredChunks();
		}
	}

	public bool TryGetAtOrBefore(int tick, out TSample sample)
	{
		if (count == 0 || tick < startTick)
		{
			sample = default;
			return false;
		}
		sample = Read(Mathf.Min(tick - startTick, count - 1));
		return true;
	}

	private void RepeatLastSampleUntil(int tick)
	{
		if (EndTick < tick - 1)
		{
			TSample sample = Read(count - 1);
			while (EndTick < tick - 1)
			{
				Store(count, in sample);
				count++;
			}
		}
	}

	private void Store(int index, in TSample sample)
	{
		while (index >= chunks.Count * 1024)
		{
			chunks.Add(new TSample[1024]);
		}
		chunks[index / 1024][index % 1024] = sample;
	}

	private TSample Read(int index)
	{
		return chunks[index / 1024][index % 1024];
	}

	private void DropExpiredChunks()
	{
		if (retainedChunks > 0)
		{
			while (chunks.Count > retainedChunks)
			{
				chunks.RemoveAt(0);
				startTick += 1024;
				count -= 1024;
			}
		}
	}
}
