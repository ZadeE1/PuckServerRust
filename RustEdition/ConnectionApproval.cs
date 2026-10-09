using Unity.Netcode;

public class ConnectionApproval
{
	public NetworkManager.ConnectionApprovalRequest Request;

	public NetworkManager.ConnectionApprovalResponse Response;

	public ConnectionData ConnectionData;

	public PlayerData PlayerData;

	public string IpAddress;

	public bool IsApproved;

	public bool IsInProgress;

	public ulong ClientID => Request.ClientNetworkId;

	public bool IsHost => ClientID == 0;

	public void Halt()
	{
		IsInProgress = true;
		Response.Pending = IsInProgress;
	}

	public void Approve(PlayerData playerData)
	{
		PlayerData = playerData;
		IsApproved = true;
		IsInProgress = false;
		Response.Approved = IsApproved;
		Response.Pending = IsInProgress;
	}

	public void Reject(string reason)
	{
		IsApproved = false;
		IsInProgress = false;
		Response.Reason = reason;
		Response.Approved = IsApproved;
		Response.Pending = IsInProgress;
	}
}
