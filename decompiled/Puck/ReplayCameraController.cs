using System.Collections.Generic;

public class ReplayCameraController : BaseCameraController
{
	private ReplayCamera replayCamera;

	public override void Awake()
	{
		base.Awake();
		replayCamera = GetComponent<ReplayCamera>();
		EventManager.AddEventListener("Event_Everyone_OnPuckSpawned", Event_Everyone_OnPuckSpawned);
		EventManager.AddEventListener("Event_Everyone_OnReplayCellyCam", Event_Everyone_OnReplayCellyCam);
	}

	public override void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnPuckSpawned", Event_Everyone_OnPuckSpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnReplayCellyCam", Event_Everyone_OnReplayCellyCam);
		base.OnDestroy();
	}

	private void Event_Everyone_OnPuckSpawned(Dictionary<string, object> message)
	{
		Puck puck = (Puck)message["puck"];
		replayCamera.IsCellyCam = false;
		replayCamera.Target = puck.transform;
	}

	private void Event_Everyone_OnReplayCellyCam(Dictionary<string, object> message)
	{
		ulong clientId = (ulong)message["scorerClientId"];
		Player replayPlayerByClientId = MonoBehaviourSingleton<PlayerManager>.Instance.GetReplayPlayerByClientId(clientId);
		if ((bool)replayPlayerByClientId && (bool)replayPlayerByClientId.PlayerBody)
		{
			replayCamera.IsCellyCam = true;
			replayCamera.Target = replayPlayerByClientId.PlayerBody.transform;
		}
	}
}
