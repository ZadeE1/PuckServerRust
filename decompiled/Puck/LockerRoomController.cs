using UnityEngine;

public class LockerRoomController : MonoBehaviour
{
	private LockerRoom lockerRoom;

	private void Awake()
	{
		lockerRoom = GetComponent<LockerRoom>();
	}

	private void OnDestroy()
	{
	}
}
