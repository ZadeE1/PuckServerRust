public class TCPServerPreviewResponse : TCPServerMessage
{
	public string name { get; set; }

	public int players { get; set; }

	public int maxPlayers { get; set; }

	public bool isPasswordProtected { get; set; }

	public string[] clientRequiredModIds { get; set; }

	public TCPServerPreviewResponse()
	{
		type = TCPServerMessageType.PreviewResponse;
	}
}
