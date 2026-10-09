using AYellowpaper.SerializedCollections;
using UnityEngine;

public class LockerRoomPlayer : MonoBehaviour
{
	private static readonly Logger Logger = new Logger("LockerRoomPlayer");

	[Header("Settings")]
	[SerializeField]
	private float rotationSpeed = 10f;

	[SerializeField]
	private float rotationSmoothing = 0.1f;

	[SerializeField]
	private SerializedDictionary<string, Vector3> rotationPresets = new SerializedDictionary<string, Vector3>();

	[SerializeField]
	private string defaultRotationPreset = "front";

	[Header("References")]
	[SerializeField]
	private PlayerMesh playerMesh;

	[HideInInspector]
	public bool AllowRotation;

	[HideInInspector]
	public bool IsRotating;

	private Vector2 lastPointerPosition = Vector2.zero;

	private Vector3 initialRotation;

	private Vector3 targetRotation;

	private Vector3 lookAtPosition;

	private void Awake()
	{
		initialRotation = transform.rotation.eulerAngles;
		targetRotation = initialRotation;
	}

	private void Start()
	{
		SetRotationFromPreset(defaultRotationPreset);
	}

	private void Update()
	{
		Vector2 vector = InputManager.PointAction.ReadValue<Vector2>();
		if (AllowRotation)
		{
			if (InputManager.ClickAction.WasPressedThisFrame() && !GlobalStateManager.UIState.IsMouseOverUI)
			{
				IsRotating = true;
				lastPointerPosition = vector;
			}
			else if (InputManager.ClickAction.WasReleasedThisFrame())
			{
				IsRotating = false;
			}
			if (IsRotating)
			{
				Vector2 vector2 = vector - lastPointerPosition;
				lastPointerPosition = vector;
				if (IsRotating)
				{
					targetRotation.y += vector2.x * rotationSpeed * Time.deltaTime;
				}
			}
		}
		Quaternion b = Quaternion.Euler(targetRotation);
		transform.rotation = Quaternion.Slerp(transform.rotation, b, Time.deltaTime / rotationSmoothing);
		BaseCamera activeCamera = CameraManager.GetActiveCamera();
		if ((bool)activeCamera)
		{
			Plane plane = new Plane(activeCamera.transform.forward, activeCamera.transform.position + activeCamera.transform.forward);
			Ray ray = activeCamera.UnityCamera.ScreenPointToRay(vector);
			if (plane.Raycast(ray, out var enter))
			{
				Vector3 point = ray.GetPoint(enter);
				Vector3 vector3 = activeCamera.transform.InverseTransformPoint(point);
				vector3.x *= Vector3.Dot(transform.forward, activeCamera.transform.forward);
				vector3.y += 0.5f;
				vector3.z *= 2f;
				Vector3 vector4 = transform.position + transform.right * vector3.x + transform.up * vector3.y + transform.forward * vector3.z;
				lookAtPosition = vector4;
			}
		}
		playerMesh.LookAt(lookAtPosition, Time.deltaTime);
	}

	public void SetRotationFromPreset(string name)
	{
		if (!rotationPresets.ContainsKey(name))
		{
			Logger.Error("Rotation preset " + name + " does not exist");
		}
		else
		{
			targetRotation = rotationPresets[name];
		}
	}

	public void SetUsername(string username)
	{
		playerMesh.SetUsername(username);
	}

	public void SetNumber(string number)
	{
		playerMesh.SetNumber(number);
	}

	public void SetLegsPadsActive(bool isActive)
	{
		playerMesh.SetLegsPadsActive(isActive);
	}

	public void SetFlagID(int flagID)
	{
		playerMesh.SetFlagID(flagID);
	}

	public void SetHeadgearID(int headgearID, PlayerRole role)
	{
		playerMesh.SetHeadgearID(headgearID, role);
	}

	public void SetMustacheID(int mustacheID)
	{
		playerMesh.SetMustacheID(mustacheID);
	}

	public void SetBeardID(int beardID)
	{
		playerMesh.SetBeardID(beardID);
	}

	public void SetJerseyID(int jerseyID, PlayerTeam team)
	{
		playerMesh.SetJerseyID(jerseyID, team);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.black;
		Gizmos.DrawSphere(lookAtPosition, 0.05f);
	}
}
