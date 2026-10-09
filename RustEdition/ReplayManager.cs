using UnityEngine;

public class ReplayManager : MonoBehaviourSingleton<ReplayManager>
{
	private static readonly Logger Logger = new Logger("ReplayManager");

	public ReplayRecorder ReplayRecorder;

	public ReplayPlayer ReplayPlayer;

	public override void Awake()
	{
		base.Awake();
		ReplayRecorder = GetComponent<ReplayRecorder>();
		ReplayPlayer = GetComponent<ReplayPlayer>();
	}

	public void Server_StartRecording()
	{
		ReplayRecorder.Server_StartRecording();
	}

	public void Server_StopRecording()
	{
		ReplayRecorder.Server_StopRecording();
	}

	public void Server_StartReplaying(float secondsToReplay)
	{
		ReplayRecording replayRecording = ReplayRecorder.Server_TakeRecording();
		if (replayRecording.TickCount != 0)
		{
			int num = ReplayRecorder.Server_GetTicks(secondsToReplay);
			ReplayPlayer.Server_StartReplay(replayRecording, replayRecording.LastTick - num);
		}
	}

	public void Server_StopReplaying()
	{
		ReplayPlayer.Server_StopReplay();
	}

	public void Server_ScheduleSlowMotionAtWorldTime(float worldTime, float scale, float rampInSeconds, float holdSeconds, float rampOutSeconds, float audioPitchFloor = 1f, float cameraFovPunch = 0f)
	{
		if (ReplayRecorder.IsRecording)
		{
			int num = ReplayRecorder.Server_GetTicks(Time.time - worldTime);
			int tick = ReplayRecorder.Tick - num;
			ReplayRecorder.Server_AddReplayEventAtTick(tick, "SlowMotion", new ReplaySlowMotion
			{
				Scale = scale,
				RampInSeconds = rampInSeconds,
				HoldSeconds = holdSeconds,
				RampOutSeconds = rampOutSeconds,
				AudioPitchFloor = audioPitchFloor,
				CameraFovPunch = cameraFovPunch
			});
		}
	}

	public void Server_ScheduleCellyCam(ulong ownerClientId, float delaySeconds = 0f)
	{
		if (ReplayRecorder.IsRecording)
		{
			int num = ReplayRecorder.Server_GetTicks(delaySeconds);
			ReplayRecorder.Server_AddReplayEventAtTick(ReplayRecorder.Tick + num, "CellyCam", new ReplayCellyCam
			{
				OwnerClientId = ownerClientId
			});
		}
	}
}
