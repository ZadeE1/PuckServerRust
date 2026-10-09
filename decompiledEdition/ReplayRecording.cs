using System.Collections.Generic;

public class ReplayRecording
{
	public int TickCount;

	private List<ReplayDiscreteEvent> events = new List<ReplayDiscreteEvent>();

	public List<ReplayDiscreteEvent> Events => events;

	public int LastTick => TickCount - 1;

	public void AddEvent(int tick, string name, object data)
	{
		events.Insert(GetIndexAfterTick(tick), new ReplayDiscreteEvent
		{
			Tick = tick,
			Name = name,
			Data = data
		});
	}

	public int GetIndexAtOrAfterTick(int tick)
	{
		for (int i = 0; i < events.Count; i++)
		{
			if (events[i].Tick >= tick)
			{
				return i;
			}
		}
		return events.Count;
	}

	private int GetIndexAfterTick(int tick)
	{
		for (int num = events.Count - 1; num >= 0; num--)
		{
			if (events[num].Tick <= tick)
			{
				return num + 1;
			}
		}
		return 0;
	}
}
