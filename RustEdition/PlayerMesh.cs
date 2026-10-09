using UnityEngine;

public class PlayerMesh : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField]
	private float lookAtSpeed = 10f;

	[Header("References")]
	[SerializeField]
	private Transform groinBone;

	[SerializeField]
	private Transform torsoBone;

	[SerializeField]
	private Transform headBone;

	[SerializeField]
	public PlayerHead PlayerHead;

	[SerializeField]
	public PlayerTorso PlayerTorso;

	[SerializeField]
	public PlayerGroin PlayerGroin;

	[SerializeField]
	public PlayerLegPad PlayerLegPadLeft;

	[SerializeField]
	public PlayerLegPad PlayerLegPadRight;

	private MeshGhoster meshGhoster;

	private float stretch = 1f;

	private Vector3 initialGroinBonePosition;

	private Vector3 initialTorsoBonePosition;

	private Vector3 initialHeadBonePosition;

	public float Stretch
	{
		get
		{
			return stretch;
		}
		set
		{
			if (stretch != value)
			{
				stretch = value;
				OnStretchChanged();
			}
		}
	}

	private void Awake()
	{
		initialGroinBonePosition = groinBone.localPosition;
		initialTorsoBonePosition = torsoBone.localPosition;
		initialHeadBonePosition = headBone.localPosition;
		meshGhoster = GetComponent<MeshGhoster>();
	}

	public void SetTransparency(float strength)
	{
		if (meshGhoster != null)
		{
			meshGhoster.SetTransparency(strength);
		}
	}

	public void LookAt(Vector3 targetPosition, float deltaTime, bool rotateTorso = true, bool rotateHead = true)
	{
		Quaternion quaternion = Utils.GetLocalLookRotation(torsoBone, targetPosition);
		if (rotateTorso & rotateHead)
		{
			quaternion = Utils.GetLocalLookRotation(torsoBone, targetPosition);
			quaternion = Quaternion.Slerp(Quaternion.identity, quaternion, 0.5f);
		}
		else if (rotateTorso)
		{
			quaternion = Utils.GetLocalLookRotation(torsoBone, targetPosition);
		}
		else if (rotateHead)
		{
			quaternion = Utils.GetLocalLookRotation(headBone, targetPosition);
		}
		Vector3 value = Utils.WrapEulerAngles(quaternion.eulerAngles);
		value = Utils.Vector3Clamp(value, new Vector3(-11.25f, -45f, 0f), new Vector3(45f, 45f, 0f));
		if (rotateTorso)
		{
			torsoBone.localRotation = Quaternion.Lerp(torsoBone.localRotation, Quaternion.Euler(value), lookAtSpeed * deltaTime);
		}
		if (rotateHead)
		{
			headBone.localRotation = Quaternion.Lerp(headBone.localRotation, Quaternion.Euler(value), lookAtSpeed * deltaTime);
		}
	}

	public void SetUsername(string username)
	{
		PlayerTorso.SetUsername(username);
	}

	public void SetNumber(string number)
	{
		PlayerTorso.SetNumber(number);
	}

	public void SetLegsPadsActive(bool isActive)
	{
		PlayerLegPadLeft.gameObject.SetActive(isActive);
		PlayerLegPadRight.gameObject.SetActive(isActive);
	}

	public void SetFlagID(int flagID)
	{
		PlayerHead.SetFlagID(flagID);
	}

	public void SetHeadgearID(int headgearID, PlayerRole role)
	{
		PlayerHead.SetHeadgearID(headgearID, role);
	}

	public void SetMustacheID(int mustacheID)
	{
		PlayerHead.SetMustacheID(mustacheID);
	}

	public void SetBeardID(int beardID)
	{
		PlayerHead.SetBeardID(beardID);
	}

	public void SetJerseyID(int jerseyID, PlayerTeam team)
	{
		PlayerTorso.SetJerseyID(jerseyID, team);
		PlayerGroin.SetJerseyID(jerseyID, team);
	}

	private void OnStretchChanged()
	{
		groinBone.localPosition = initialGroinBonePosition * Stretch;
		torsoBone.localPosition = initialTorsoBonePosition * Stretch;
		headBone.localPosition = initialHeadBonePosition * Stretch;
	}
}
