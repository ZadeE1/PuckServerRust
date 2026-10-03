using System.Collections.Generic;

public class ReplayBindings
{
	private List<ReplayPoseBinding> poseBindings = new List<ReplayPoseBinding>();

	private List<ReplayPlayerBodyBinding> playerBodyBindings = new List<ReplayPlayerBodyBinding>();

	public List<ReplayPoseBinding> PoseBindings => poseBindings;

	public List<ReplayPlayerBodyBinding> PlayerBodyBindings => playerBodyBindings;

	public void AddPose(SynchronizedObject synchronizedObject, ReplayTrack<ReplayPoseSample> track)
	{
		poseBindings.Add(new ReplayPoseBinding
		{
			SynchronizedObject = synchronizedObject,
			Track = track
		});
	}

	public void AddPlayerBody(PlayerBody playerBody, ReplayTrack<ReplayPlayerBodyStateSample> stateTrack, ReplayTrack<ReplayPlayerInputSample> inputTrack)
	{
		playerBodyBindings.Add(new ReplayPlayerBodyBinding
		{
			PlayerBody = playerBody,
			StateTrack = stateTrack,
			InputTrack = inputTrack
		});
	}

	public void RemovePose(SynchronizedObject synchronizedObject)
	{
		for (int i = 0; i < poseBindings.Count; i++)
		{
			if (!(poseBindings[i].SynchronizedObject != synchronizedObject))
			{
				poseBindings.RemoveAt(i);
				break;
			}
		}
	}

	public void RemovePlayerBody(PlayerBody playerBody)
	{
		RemovePose(playerBody.SynchronizedObject);
		for (int i = 0; i < playerBodyBindings.Count; i++)
		{
			if (!(playerBodyBindings[i].PlayerBody != playerBody))
			{
				playerBodyBindings.RemoveAt(i);
				break;
			}
		}
	}

	public void Clear()
	{
		poseBindings.Clear();
		playerBodyBindings.Clear();
	}
}
