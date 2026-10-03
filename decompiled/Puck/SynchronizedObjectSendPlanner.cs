public class SynchronizedObjectSendPlanner
{
	private SynchronizedObjectLodSelector lodSelector = new SynchronizedObjectLodSelector();

	private uint tickCount;

	private int tickRate = 1;

	public void Configure(SynchronizedObjectBandSettings[] lodBands, SynchronizedObjectBandSettings culling, byte noOriginTickRateDivisor, float lodHysteresis, bool useHighPrecisionRotation)
	{
		lodSelector.Configure(lodBands, culling, noOriginTickRateDivisor, lodHysteresis, useHighPrecisionRotation);
	}

	public void Plan(SynchronizedObjectSnapshot snapshot, SynchronizedPlayerState playerState, uint tickCount, int tickRate, SynchronizedObjectSendBuffers buffers, SynchronizedObjectLodCounts lodCounts)
	{
		this.tickCount = tickCount;
		this.tickRate = tickRate;
		buffers.Clear();
		lodSelector.BeginPlayer(playerState.Player);
		for (int i = 0; i < snapshot.Count; i++)
		{
			PlanObject(snapshot, i, playerState, buffers, lodCounts);
		}
	}

	private void PlanObject(SynchronizedObjectSnapshot snapshot, int index, SynchronizedPlayerState playerState, SynchronizedObjectSendBuffers buffers, SynchronizedObjectLodCounts lodCounts)
	{
		SynchronizedObjectData data = snapshot.GetData(index);
		ulong networkObjectId = data.NetworkObjectId;
		SynchronizedObjectSendState sendState = playerState.GetSendState(networkObjectId);
		sendState.LodSelection = lodSelector.Select(snapshot.GetPosition(index), sendState.LodSelection, snapshot.GetUsesLod(index), snapshot.GetUsesCulling(index));
		lodCounts?.Add(in sendState.LodSelection);
		PlanSend(data, ref sendState, buffers);
		playerState.SetSendState(networkObjectId, sendState);
	}

	private void PlanSend(SynchronizedObjectData synchronizedObjectData, ref SynchronizedObjectSendState sendState, SynchronizedObjectSendBuffers buffers)
	{
		SynchronizedObjectLodSelection lodSelection = sendState.LodSelection;
		if (tickCount % lodSelection.TickRateDivisor != 0)
		{
			return;
		}
		synchronizedObjectData = synchronizedObjectData.WithTickRateDivisor(lodSelection.TickRateDivisor);
		bool flag = IsFullSendDue(in sendState);
		ushort num = (ushort)(flag ? 3071 : synchronizedObjectData.GetChangeMask(sendState.LastSentData, lodSelection.UseHighPrecisionRotation));
		if ((num & 0x3FF) == 0)
		{
			if (sendState.IsAwake)
			{
				sendState.IsAwake = false;
				sendState.LastSentData = synchronizedObjectData;
				sendState.HasLastSentData = true;
				buffers.AddReliable(synchronizedObjectData.WithAsleep());
			}
			return;
		}
		SynchronizedObjectData synchronizedObjectData2 = synchronizedObjectData.WithComponentMask(num, lodSelection.UseHighPrecisionRotation);
		if (flag)
		{
			sendState.LastFullSendTick = GetFullSendTick(in sendState, synchronizedObjectData.NetworkObjectId);
		}
		sendState.LastSentData = (sendState.HasLastSentData ? sendState.LastSentData.Merge(synchronizedObjectData2) : synchronizedObjectData2);
		sendState.HasLastSentData = true;
		sendState.IsAwake = true;
		if (flag)
		{
			buffers.AddReliable(synchronizedObjectData2);
		}
		else
		{
			buffers.AddDelta(synchronizedObjectData2);
		}
	}

	private bool IsFullSendDue(in SynchronizedObjectSendState sendState)
	{
		if (!sendState.HasLastSentData)
		{
			return true;
		}
		if (!sendState.IsAwake)
		{
			return false;
		}
		return tickCount - sendState.LastFullSendTick >= (uint)tickRate;
	}

	private uint GetFullSendTick(in SynchronizedObjectSendState sendState, ushort networkObjectId)
	{
		if (sendState.HasLastSentData)
		{
			return tickCount;
		}
		return tickCount - (uint)(networkObjectId % tickRate);
	}
}
