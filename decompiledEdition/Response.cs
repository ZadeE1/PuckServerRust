public class Response<TSuccessData, TErrorData>
{
	public bool success { get; set; }

	public TSuccessData data { get; set; }

	public TErrorData errorData { get; set; }
}
