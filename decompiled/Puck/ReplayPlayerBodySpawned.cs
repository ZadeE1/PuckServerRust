using Unity.Collections;
using UnityEngine;

public struct ReplayPlayerBodySpawned
{
	public ulong OwnerClientId;

	public Vector3 Position;

	public Quaternion Rotation;

	public PlayerGameState GameState;

	public PlayerCustomizationState CustomizationState;

	public FixedString32Bytes Username;

	public int Number;

	public ReplayTrack<ReplayPoseSample> PoseTrack;

	public ReplayTrack<ReplayPlayerBodyStateSample> StateTrack;

	public ReplayTrack<ReplayPlayerInputSample> InputTrack;
}
