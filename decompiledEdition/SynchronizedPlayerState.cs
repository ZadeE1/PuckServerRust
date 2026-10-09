using System.Collections.Generic;
using Unity.Netcode;

public class SynchronizedPlayerState
{
	public Player Player;

	public BaseRpcTarget RpcTarget;

	public ushort SendSequenceNumber;

	public SynchronizedObjectTickHeader LastSentTickHeader;

	public bool HasSentTickHeader;

	private Dictionary<ulong, SynchronizedObjectSendState> sendStates = new Dictionary<ulong, SynchronizedObjectSendState>();

	public SynchronizedObjectSendState GetSendState(ulong networkObjectId)
	{
		if (sendStates.TryGetValue(networkObjectId, out var value))
		{
			return value;
		}
		return new SynchronizedObjectSendState
		{
			LodSelection = SynchronizedObjectLodSelection.Default
		};
	}

	public void SetSendState(ulong networkObjectId, SynchronizedObjectSendState sendState)
	{
		sendStates[networkObjectId] = sendState;
	}

	public void Forget(ulong networkObjectId)
	{
		sendStates.Remove(networkObjectId);
	}

	public void Dispose()
	{
		RpcTarget?.Dispose();
		RpcTarget = null;
	}
}
