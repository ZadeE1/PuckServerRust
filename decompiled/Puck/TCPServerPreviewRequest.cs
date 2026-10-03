public class TCPServerPreviewRequest : TCPServerMessage
{
	public TCPServerPreviewRequest()
	{
		type = TCPServerMessageType.PreviewRequest;
	}
}
