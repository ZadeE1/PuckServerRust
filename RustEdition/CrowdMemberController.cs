using UnityEngine;

public class CrowdMemberController : MonoBehaviour
{
	private CrowdMember crowdMember;

	private void Awake()
	{
		crowdMember = GetComponent<CrowdMember>();
	}

	private void OnDestroy()
	{
	}
}
