using UnityEngine;

public class PlayerPositionController : MonoBehaviour
{
	private PlayerPosition playerPosition;

	private void Awake()
	{
		playerPosition = GetComponent<PlayerPosition>();
	}

	public void Start()
	{
	}

	private void OnDestroy()
	{
	}
}
