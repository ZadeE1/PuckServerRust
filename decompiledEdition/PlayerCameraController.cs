public class PlayerCameraController : BaseCameraController
{
	private PlayerCamera playerCamera;

	public override void Awake()
	{
		base.Awake();
		playerCamera = GetComponent<PlayerCamera>();
	}
}
