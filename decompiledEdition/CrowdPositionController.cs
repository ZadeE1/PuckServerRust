using UnityEngine;

public class CrowdPositionController : MonoBehaviour
{
	private CrowdPosition crowdPosition;

	private void Awake()
	{
		crowdPosition = GetComponent<CrowdPosition>();
	}

	public void Start()
	{
	}

	private void OnDestroy()
	{
	}
}
