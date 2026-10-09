public class SpectatorCameraController : BaseCameraController
{
	private SpectatorCamera spectatorCamera;

	public override void Awake()
	{
		base.Awake();
		spectatorCamera = GetComponent<SpectatorCamera>();
	}
}
