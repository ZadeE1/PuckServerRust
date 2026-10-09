using UnityEngine;

public struct ReplayStickSpawned
{
	public ulong OwnerClientId;

	public Vector3 Position;

	public Quaternion Rotation;

	public ReplayTrack<ReplayPoseSample> PoseTrack;
}
