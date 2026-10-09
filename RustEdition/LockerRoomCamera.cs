public class LockerRoomCamera : BaseCamera
{
	private SmoothPositioner smoothPositioner;

	public override void Awake()
	{
		base.Awake();
		smoothPositioner = GetComponent<SmoothPositioner>();
	}

	public void SetPosition(string positionName)
	{
		smoothPositioner.SetPosition(positionName);
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected override string __getTypeName()
	{
		return "LockerRoomCamera";
	}
}
