using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ReplayPlayer : MonoBehaviour
{
	private static readonly Logger Logger = new Logger("ReplayPlayer");

	[HideInInspector]
	public bool IsReplaying;

	private ReplayRecording recording;

	private int tick;

	private int eventIndex;

	private ReplayBindings bindings = new ReplayBindings();

	private Dictionary<ulong, Player> playersByRecordedClientId = new Dictionary<ulong, Player>();

	private Dictionary<ulong, Puck> pucksByRecordedNetworkObjectId = new Dictionary<ulong, Puck>();

	private Dictionary<ulong, ReplayPlayerSpawned> seekPlayers = new Dictionary<ulong, ReplayPlayerSpawned>();

	private Dictionary<ulong, ReplayPlayerBodySpawned> seekPlayerBodies = new Dictionary<ulong, ReplayPlayerBodySpawned>();

	private Dictionary<ulong, ReplayStickSpawned> seekSticks = new Dictionary<ulong, ReplayStickSpawned>();

	private Dictionary<ulong, ReplayPuckSpawned> seekPucks = new Dictionary<ulong, ReplayPuckSpawned>();

	private void Awake()
	{
		PhysicsManager.OnBeforeSimulate += Server_OnBeforeSimulate;
	}

	private void OnDestroy()
	{
		PhysicsManager.OnBeforeSimulate -= Server_OnBeforeSimulate;
	}

	private void Server_OnBeforeSimulate(float deltaTime)
	{
		if (!(NetworkManager.Singleton == null) && NetworkManager.Singleton.IsServer && IsReplaying)
		{
			if (tick >= recording.LastTick)
			{
				Server_StopReplay();
				return;
			}
			tick++;
			Server_ApplyEvents();
			Server_ApplyStateSamples();
			Server_DrivePoses();
		}
	}

	public void Server_StartReplay(ReplayRecording replayRecording, int fromTick)
	{
		if (NetworkManager.Singleton.IsServer && !IsReplaying && replayRecording.TickCount != 0)
		{
			recording = replayRecording;
			tick = Mathf.Clamp(fromTick, 0, recording.LastTick);
			eventIndex = recording.GetIndexAtOrAfterTick(tick);
			IsReplaying = true;
			Server_SpawnWorldAtTick();
			Server_ApplyEvents();
			Server_ApplyStateSamples();
			Server_DrivePoses();
		}
	}

	public void Server_StopReplay()
	{
		if (NetworkManager.Singleton.IsServer && IsReplaying)
		{
			IsReplaying = false;
			if (NetworkBehaviourSingleton<GameManager>.Instance != null)
			{
				NetworkBehaviourSingleton<GameManager>.Instance.Server_StopSlowMotion();
			}
			Server_Dispose();
		}
	}

	private void Server_ApplyEvents()
	{
		while (eventIndex < recording.Events.Count && recording.Events[eventIndex].Tick <= tick)
		{
			ReplayDiscreteEvent replayDiscreteEvent = recording.Events[eventIndex];
			eventIndex++;
			Server_ReplayEvent(replayDiscreteEvent.Name, replayDiscreteEvent.Data);
		}
	}

	private void Server_ApplyStateSamples()
	{
		foreach (ReplayPlayerBodyBinding playerBodyBinding in bindings.PlayerBodyBindings)
		{
			if ((bool)playerBodyBinding.PlayerBody && (bool)playerBodyBinding.PlayerBody.Player)
			{
				if (playerBodyBinding.StateTrack.TryGetAtOrBefore(tick, out var sample))
				{
					Server_ApplyPlayerBodyState(playerBodyBinding.PlayerBody, in sample);
				}
				if (playerBodyBinding.InputTrack.TryGetAtOrBefore(tick, out var sample2))
				{
					Server_ApplyPlayerInput(playerBodyBinding.PlayerBody.Player, in sample2);
				}
			}
		}
	}

	private void Server_DrivePoses()
	{
		foreach (ReplayPoseBinding poseBinding in bindings.PoseBindings)
		{
			if ((bool)poseBinding.SynchronizedObject && poseBinding.Track.TryGetAtOrBefore(tick, out var sample))
			{
				poseBinding.SynchronizedObject.SetDrivenPose(Server_GetPose(in sample));
			}
		}
	}

	private static SynchronizedObjectPose Server_GetPose(in ReplayPoseSample poseSample)
	{
		return new SynchronizedObjectPose
		{
			Position = poseSample.Position,
			Rotation = poseSample.Rotation,
			LinearVelocity = poseSample.LinearVelocity,
			AngularVelocity = poseSample.AngularVelocity
		};
	}

	private static void Server_ApplyPlayerBodyState(PlayerBody playerBody, in ReplayPlayerBodyStateSample stateSample)
	{
		playerBody.Stamina.Value = stateSample.Stamina;
		playerBody.Speed.Value = stateSample.Speed;
		playerBody.IsSprinting.Value = stateSample.IsSprinting;
		playerBody.IsSliding.Value = stateSample.IsSliding;
		playerBody.IsStopping.Value = stateSample.IsStopping;
		playerBody.IsExtendedLeft.Value = stateSample.IsExtendedLeft;
		playerBody.IsExtendedRight.Value = stateSample.IsExtendedRight;
	}

	private static void Server_ApplyPlayerInput(Player player, in ReplayPlayerInputSample inputSample)
	{
		player.PlayerInput.LookAngleInput.ClientValue = inputSample.LookAngleInput;
		player.PlayerInput.BladeAngleInput.ClientValue = inputSample.BladeAngleInput;
		player.PlayerInput.TrackInput.ClientValue = inputSample.TrackInput;
		player.PlayerInput.LookInput.ClientValue = inputSample.LookInput;
	}

	private void Server_SpawnWorldAtTick()
	{
		Server_CollectSurvivors();
		Server_SpawnSurvivingPlayers();
		Server_SpawnSurvivingPlayerBodies();
		Server_SpawnSurvivingSticks();
		Server_SpawnSurvivingPucks();
		seekPlayers.Clear();
		seekPlayerBodies.Clear();
		seekSticks.Clear();
		seekPucks.Clear();
	}

	private void Server_CollectSurvivors()
	{
		for (int i = 0; i < eventIndex; i++)
		{
			ReplayDiscreteEvent replayDiscreteEvent = recording.Events[i];
			Server_CollectSurvivorEvent(replayDiscreteEvent.Name, replayDiscreteEvent.Data);
		}
	}

	private void Server_CollectSurvivorEvent(string eventName, object eventData)
	{
		switch (eventName)
		{
		case "PlayerSpawned":
		{
			ReplayPlayerSpawned value4 = (ReplayPlayerSpawned)eventData;
			seekPlayers[value4.OwnerClientId] = value4;
			break;
		}
		case "PlayerDespawned":
		{
			ReplayPlayerDespawned replayPlayerDespawned = (ReplayPlayerDespawned)eventData;
			seekPlayers.Remove(replayPlayerDespawned.OwnerClientId);
			seekPlayerBodies.Remove(replayPlayerDespawned.OwnerClientId);
			seekSticks.Remove(replayPlayerDespawned.OwnerClientId);
			break;
		}
		case "PlayerBodySpawned":
		{
			ReplayPlayerBodySpawned value3 = (ReplayPlayerBodySpawned)eventData;
			seekPlayerBodies[value3.OwnerClientId] = value3;
			break;
		}
		case "PlayerBodyDespawned":
		{
			ReplayPlayerBodyDespawned replayPlayerBodyDespawned = (ReplayPlayerBodyDespawned)eventData;
			seekPlayerBodies.Remove(replayPlayerBodyDespawned.OwnerClientId);
			break;
		}
		case "StickSpawned":
		{
			ReplayStickSpawned value2 = (ReplayStickSpawned)eventData;
			seekSticks[value2.OwnerClientId] = value2;
			break;
		}
		case "StickDespawned":
		{
			ReplayStickDespawned replayStickDespawned = (ReplayStickDespawned)eventData;
			seekSticks.Remove(replayStickDespawned.OwnerClientId);
			break;
		}
		case "PuckSpawned":
		{
			ReplayPuckSpawned value = (ReplayPuckSpawned)eventData;
			seekPucks[value.NetworkObjectId] = value;
			break;
		}
		case "PuckDespawned":
		{
			ReplayPuckDespawned replayPuckDespawned = (ReplayPuckDespawned)eventData;
			seekPucks.Remove(replayPuckDespawned.NetworkObjectId);
			break;
		}
		}
	}

	private void Server_SpawnSurvivingPlayers()
	{
		foreach (KeyValuePair<ulong, ReplayPlayerSpawned> seekPlayer in seekPlayers)
		{
			Server_ReplayPlayerSpawned(seekPlayer.Value);
		}
	}

	private void Server_SpawnSurvivingPlayerBodies()
	{
		foreach (KeyValuePair<ulong, ReplayPlayerBodySpawned> seekPlayerBody in seekPlayerBodies)
		{
			ReplayPlayerBodySpawned playerBodySpawned = seekPlayerBody.Value;
			if (Server_TryGetPoseSampleAtTick(playerBodySpawned.PoseTrack, out var poseSample))
			{
				playerBodySpawned.Position = poseSample.Position;
				playerBodySpawned.Rotation = poseSample.Rotation;
			}
			Server_ReplayPlayerBodySpawned(in playerBodySpawned);
		}
	}

	private void Server_SpawnSurvivingSticks()
	{
		foreach (KeyValuePair<ulong, ReplayStickSpawned> seekStick in seekSticks)
		{
			ReplayStickSpawned stickSpawned = seekStick.Value;
			if (Server_TryGetPoseSampleAtTick(stickSpawned.PoseTrack, out var poseSample))
			{
				stickSpawned.Position = poseSample.Position;
				stickSpawned.Rotation = poseSample.Rotation;
			}
			Server_ReplayStickSpawned(in stickSpawned);
		}
	}

	private void Server_SpawnSurvivingPucks()
	{
		foreach (KeyValuePair<ulong, ReplayPuckSpawned> seekPuck in seekPucks)
		{
			ReplayPuckSpawned puckSpawned = seekPuck.Value;
			if (Server_TryGetPoseSampleAtTick(puckSpawned.PoseTrack, out var poseSample))
			{
				puckSpawned.Position = poseSample.Position;
				puckSpawned.Rotation = poseSample.Rotation;
			}
			Server_ReplayPuckSpawned(in puckSpawned);
		}
	}

	private bool Server_TryGetPoseSampleAtTick(ReplayTrack<ReplayPoseSample> poseTrack, out ReplayPoseSample poseSample)
	{
		poseSample = default;
		return poseTrack?.TryGetAtOrBefore(tick, out poseSample) ?? false;
	}

	private void Server_ReplayEvent(string eventName, object eventData)
	{
		switch (eventName)
		{
		case "PlayerSpawned":
			Server_ReplayPlayerSpawned((ReplayPlayerSpawned)eventData);
			break;
		case "PlayerDespawned":
			Server_ReplayPlayerDespawned((ReplayPlayerDespawned)eventData);
			break;
		case "PlayerBodySpawned":
			Server_ReplayPlayerBodySpawned((ReplayPlayerBodySpawned)eventData);
			break;
		case "PlayerBodyDespawned":
			Server_ReplayPlayerBodyDespawned((ReplayPlayerBodyDespawned)eventData);
			break;
		case "StickSpawned":
			Server_ReplayStickSpawned((ReplayStickSpawned)eventData);
			break;
		case "StickDespawned":
			Server_ReplayStickDespawned((ReplayStickDespawned)eventData);
			break;
		case "PuckSpawned":
			Server_ReplayPuckSpawned((ReplayPuckSpawned)eventData);
			break;
		case "PuckDespawned":
			Server_ReplayPuckDespawned((ReplayPuckDespawned)eventData);
			break;
		case "SlowMotion":
			Server_ReplaySlowMotion((ReplaySlowMotion)eventData);
			break;
		case "CellyCam":
			Server_ReplayCellyCam((ReplayCellyCam)eventData);
			break;
		}
	}

	private void Server_ReplayPlayerSpawned(in ReplayPlayerSpawned playerSpawned)
	{
		MonoBehaviourSingleton<PlayerManager>.Instance.Server_SpawnPlayer(playerSpawned.OwnerClientId, playerSpawned.GameState, playerSpawned.CustomizationState, playerSpawned.Handedness, playerSpawned.SteamId.ToString(), playerSpawned.Username.ToString(), playerSpawned.Number, playerSpawned.PatreonLevel, playerSpawned.AdminLevel, playerSpawned.IsMuted, isReplay: true);
		Player replayPlayerByClientId = MonoBehaviourSingleton<PlayerManager>.Instance.GetReplayPlayerByClientId(playerSpawned.OwnerClientId);
		if ((bool)replayPlayerByClientId)
		{
			playersByRecordedClientId[playerSpawned.OwnerClientId] = replayPlayerByClientId;
		}
	}

	private void Server_ReplayPlayerDespawned(in ReplayPlayerDespawned playerDespawned)
	{
		if (Server_TryGetReplayPlayer(playerDespawned.OwnerClientId, out var player))
		{
			playersByRecordedClientId.Remove(playerDespawned.OwnerClientId);
			if ((bool)player.PlayerBody)
			{
				bindings.RemovePlayerBody(player.PlayerBody);
			}
			if ((bool)player.Stick)
			{
				bindings.RemovePose(player.Stick.SynchronizedObject);
			}
			player.NetworkObject.Despawn();
			Logger.Info($"Despawned replay player {player.OwnerClientId}");
		}
	}

	private void Server_ReplayPlayerBodySpawned(in ReplayPlayerBodySpawned playerBodySpawned)
	{
		if (Server_TryGetReplayPlayer(playerBodySpawned.OwnerClientId, out var player))
		{
			player.GameState.Value = playerBodySpawned.GameState;
			player.CustomizationState.Value = playerBodySpawned.CustomizationState;
			player.Username.Value = playerBodySpawned.Username;
			player.Number.Value = playerBodySpawned.Number;
			player.Server_SpawnPlayerBody(playerBodySpawned.Position, playerBodySpawned.Rotation, player.Role);
			if ((bool)player.PlayerBody)
			{
				Server_BindDrivenPose(player.PlayerBody.SynchronizedObject, playerBodySpawned.PoseTrack);
				bindings.AddPlayerBody(player.PlayerBody, playerBodySpawned.StateTrack, playerBodySpawned.InputTrack);
			}
		}
	}

	private void Server_ReplayPlayerBodyDespawned(in ReplayPlayerBodyDespawned playerBodyDespawned)
	{
		if (Server_TryGetReplayPlayer(playerBodyDespawned.OwnerClientId, out var player) && (bool)player.PlayerBody)
		{
			bindings.RemovePlayerBody(player.PlayerBody);
			player.Server_DespawnPlayerBody();
		}
	}

	private void Server_ReplayStickSpawned(in ReplayStickSpawned stickSpawned)
	{
		if (Server_TryGetReplayPlayer(stickSpawned.OwnerClientId, out var player))
		{
			player.Server_SpawnStick(stickSpawned.Position, stickSpawned.Rotation, player.Role);
			if ((bool)player.Stick)
			{
				Server_BindDrivenPose(player.Stick.SynchronizedObject, stickSpawned.PoseTrack);
			}
		}
	}

	private void Server_ReplayStickDespawned(in ReplayStickDespawned stickDespawned)
	{
		if (Server_TryGetReplayPlayer(stickDespawned.OwnerClientId, out var player) && (bool)player.Stick)
		{
			bindings.RemovePose(player.Stick.SynchronizedObject);
			player.Server_DespawnStick();
		}
	}

	private void Server_ReplayPuckSpawned(in ReplayPuckSpawned puckSpawned)
	{
		Puck puck = MonoBehaviourSingleton<PuckManager>.Instance.Server_SpawnPuck(puckSpawned.Position, puckSpawned.Rotation, isReplay: true);
		if ((bool)puck)
		{
			pucksByRecordedNetworkObjectId[puckSpawned.NetworkObjectId] = puck;
			Server_BindDrivenPose(puck.SynchronizedObject, puckSpawned.PoseTrack);
		}
	}

	private void Server_ReplayPuckDespawned(in ReplayPuckDespawned puckDespawned)
	{
		if (pucksByRecordedNetworkObjectId.TryGetValue(puckDespawned.NetworkObjectId, out var value) && (bool)value)
		{
			pucksByRecordedNetworkObjectId.Remove(puckDespawned.NetworkObjectId);
			bindings.RemovePose(value.SynchronizedObject);
			MonoBehaviourSingleton<PuckManager>.Instance.Server_DespawnPuck(value);
		}
	}

	private static void Server_ReplaySlowMotion(in ReplaySlowMotion slowMotion)
	{
		if (!(NetworkBehaviourSingleton<GameManager>.Instance == null))
		{
			NetworkBehaviourSingleton<GameManager>.Instance.Server_StartSlowMotion(slowMotion.Scale, slowMotion.RampInSeconds, slowMotion.HoldSeconds, slowMotion.RampOutSeconds, slowMotion.AudioPitchFloor, slowMotion.CameraFovPunch);
		}
	}

	private static void Server_ReplayCellyCam(in ReplayCellyCam cellyCam)
	{
		if (!(NetworkBehaviourSingleton<GameManager>.Instance == null))
		{
			NetworkBehaviourSingleton<GameManager>.Instance.Server_NotifyReplayCellyCamRpc(cellyCam.OwnerClientId);
		}
	}

	private void Server_BindDrivenPose(SynchronizedObject synchronizedObject, ReplayTrack<ReplayPoseSample> poseTrack)
	{
		bindings.AddPose(synchronizedObject, poseTrack);
		synchronizedObject.DriveExternally(SynchronizedObjectDriveCadence.PerTick);
	}

	private bool Server_TryGetReplayPlayer(ulong recordedClientId, out Player player)
	{
		if (!playersByRecordedClientId.TryGetValue(recordedClientId, out player) || !player)
		{
			player = null;
			return false;
		}
		return true;
	}

	private void Server_Dispose()
	{
		Server_DespawnReplayObjects();
		recording = null;
		tick = 0;
		eventIndex = 0;
		bindings.Clear();
		playersByRecordedClientId.Clear();
		pucksByRecordedNetworkObjectId.Clear();
	}

	private void Server_DespawnReplayObjects()
	{
		foreach (Player replayPlayer in MonoBehaviourSingleton<PlayerManager>.Instance.GetReplayPlayers())
		{
			if ((bool)replayPlayer.PlayerBody)
			{
				replayPlayer.Server_DespawnPlayerBody();
			}
			if ((bool)replayPlayer.Stick)
			{
				replayPlayer.Server_DespawnStick();
			}
			replayPlayer.NetworkObject.Despawn();
			Logger.Info($"Despawned replay player {replayPlayer.OwnerClientId}");
		}
		foreach (Puck replayPuck in MonoBehaviourSingleton<PuckManager>.Instance.GetReplayPucks())
		{
			MonoBehaviourSingleton<PuckManager>.Instance.Server_DespawnPuck(replayPuck);
			Logger.Info($"Despawned replay puck {replayPuck.OwnerClientId}");
		}
	}
}
