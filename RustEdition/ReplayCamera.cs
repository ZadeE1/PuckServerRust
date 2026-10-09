using UnityEngine;

public class ReplayCamera : BaseCamera
{
	[Header("Settings")]
	[SerializeField]
	private float followSpeed = 10f;

	[SerializeField]
	private float followDistance = 5f;

	[SerializeField]
	private float followHeight = 5f;

	[SerializeField]
	private float rotationSpeed = 10f;

	[Header("Celly Cam")]
	[SerializeField]
	private float cellyCamFollowSpeed = 2f;

	[SerializeField]
	private float cellyCamRotationSpeed = 2f;

	[HideInInspector]
	public Transform Target;

	[HideInInspector]
	public Vector3 CenterPoint = Vector3.zero;

	[HideInInspector]
	public bool IsCellyCam;

	private void Update()
	{
		if ((bool)Target)
		{
			float deltaTime = Time.deltaTime;
			float num = (IsCellyCam ? cellyCamFollowSpeed : followSpeed);
			float num2 = (IsCellyCam ? cellyCamRotationSpeed : rotationSpeed);
			Vector3 normalized = (CenterPoint - Target.position).normalized;
			Vector3 b = Target.position + normalized * followDistance;
			b.y = followHeight;
			transform.position = Vector3.Lerp(transform.position, b, deltaTime * num);
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Target.position - transform.position), deltaTime * num2);
		}
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
		return "ReplayCamera";
	}
}
