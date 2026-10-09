using UnityEngine;

public struct ReplayPuckSpawned
{
	public ulong NetworkObjectId;

	public Vector3 Position;

	public Quaternion Rotation;

	public ReplayTrack<ReplayPoseSample> PoseTrack;
}
