using Unity.Netcode;
using UnityEngine;

public class ReplayRecorder : MonoBehaviour
{
	private static readonly Logger Logger = new Logger("ReplayRecorder");

	[Header("Settings")]
	[Tooltip("Seconds of motion always available to play back. Older samples are dropped as recording continues, so memory stays flat however long a period runs. Must exceed the Replay phase duration, or a replay will show objects frozen at their spawn pose. Zero keeps the entire recording, which is what writing a whole match to a file will need.")]
	[SerializeField]
	private float retainedSeconds = 20f;

	[HideInInspector]
	public bool IsRecording;

	private float tickInterval;

	private ReplayRecording recording = new ReplayRecording();

	private ReplayBindings bindings = new ReplayBindings();

	[HideInInspector]
	public int Tick => recording.TickCount;

	private void Awake()
	{
		PhysicsManager.OnAfterSimulate += Server_OnAfterSimulate;
	}

	private void OnDestroy()
	{
		PhysicsManager.OnAfterSimulate -= Server_OnAfterSimulate;
	}

	private void Server_OnAfterSimulate(float deltaTime)
	{
		if (!(NetworkManager.Singleton == null) && NetworkManager.Singleton.IsServer)
		{
			tickInterval = deltaTime;
			if (IsRecording)
			{
				Server_Tick();
				recording.TickCount++;
			}
		}
	}

	public void Server_StartRecording()
	{
		if (NetworkManager.Singleton.IsServer && !IsRecording)
		{
			recording = new ReplayRecording();
			bindings.Clear();
			IsRecording = true;
			Logger.Info("Replay recording started");
			Server_SeedFromWorld();
		}
	}

	public void Server_StopRecording()
	{
		if (NetworkManager.Singleton.IsServer && IsRecording)
		{
			Logger.Info("Replay recording stopped");
			IsRecording = false;
		}
	}

	public ReplayRecording Server_TakeRecording()
	{
		ReplayRecording result = recording;
		recording = new ReplayRecording();
		bindings.Clear();
		if (IsRecording)
		{
			Server_SeedFromWorld();
		}
		return result;
	}

	public int Server_GetTicks(float seconds)
	{
		if (tickInterval <= 0f)
		{
			return 0;
		}
		return Mathf.RoundToInt(seconds / tickInterval);
	}

	private int Server_GetRetainedTicks()
	{
		if (retainedSeconds <= 0f)
		{
			return 0;
		}
		return Server_GetTicks(retainedSeconds);
	}

	private void Server_SeedFromWorld()
	{
		foreach (Player player in MonoBehaviourSingleton<PlayerManager>.Instance.GetPlayers())
		{
			Server_AddPlayerSpawnedEvent(player);
			if ((bool)player.PlayerBody)
			{
				Server_AddPlayerBodySpawnedEvent(player.PlayerBody);
			}
			if ((bool)player.Stick)
			{
				Server_AddStickSpawnedEvent(player.Stick);
			}
		}
		foreach (Puck puck in MonoBehaviourSingleton<PuckManager>.Instance.GetPucks())
		{
			Server_AddPuckSpawnedEvent(puck);
		}
	}

	private void Server_Tick()
	{
		foreach (ReplayPoseBinding poseBinding in bindings.PoseBindings)
		{
			if ((bool)poseBinding.SynchronizedObject)
			{
				poseBinding.Track.Append(Tick, Server_GetPoseSample(poseBinding.SynchronizedObject));
			}
		}
		foreach (ReplayPlayerBodyBinding playerBodyBinding in bindings.PlayerBodyBindings)
		{
			if ((bool)playerBodyBinding.PlayerBody && (bool)playerBodyBinding.PlayerBody.Player)
			{
				playerBodyBinding.StateTrack.Append(Tick, Server_GetPlayerBodyStateSample(playerBodyBinding.PlayerBody));
				playerBodyBinding.InputTrack.Append(Tick, Server_GetPlayerInputSample(playerBodyBinding.PlayerBody.Player));
			}
		}
	}

	private static ReplayPoseSample Server_GetPoseSample(SynchronizedObject synchronizedObject)
	{
		return new ReplayPoseSample
		{
			Position = synchronizedObject.Rigidbody.position,
			Rotation = synchronizedObject.Rigidbody.rotation,
			LinearVelocity = synchronizedObject.GetLinearVelocityAtPosition(),
			AngularVelocity = synchronizedObject.Rigidbody.angularVelocity
		};
	}

	private static ReplayPlayerBodyStateSample Server_GetPlayerBodyStateSample(PlayerBody playerBody)
	{
		return new ReplayPlayerBodyStateSample
		{
			Stamina = playerBody.Stamina.Value,
			Speed = playerBody.Speed.Value,
			IsSprinting = playerBody.IsSprinting.Value,
			IsSliding = playerBody.IsSliding.Value,
			IsStopping = playerBody.IsStopping.Value,
			IsExtendedLeft = playerBody.IsExtendedLeft.Value,
			IsExtendedRight = playerBody.IsExtendedRight.Value
		};
	}

	private static ReplayPlayerInputSample Server_GetPlayerInputSample(Player player)
	{
		return new ReplayPlayerInputSample
		{
			LookAngleInput = player.PlayerInput.LookAngleInput.ServerValue,
			BladeAngleInput = player.PlayerInput.BladeAngleInput.ServerValue,
			TrackInput = player.PlayerInput.TrackInput.ServerValue,
			LookInput = player.PlayerInput.LookInput.ServerValue
		};
	}

	public void Server_AddReplayEvent(string eventName, object eventData)
	{
		Server_AddReplayEventAtTick(Tick, eventName, eventData);
	}

	public void Server_AddReplayEventAtTick(int tick, string eventName, object eventData)
	{
		if (NetworkManager.Singleton.IsServer && IsRecording && tick >= 0)
		{
			recording.AddEvent(tick, eventName, eventData);
		}
	}

	public void Server_AddPlayerSpawnedEvent(Player player)
	{
		if (NetworkManager.Singleton.IsServer && IsRecording)
		{
			Server_AddReplayEvent("PlayerSpawned", new ReplayPlayerSpawned
			{
				OwnerClientId = player.OwnerClientId,
				GameState = player.GameState.Value,
				CustomizationState = player.CustomizationState.Value,
				Handedness = player.Handedness.Value,
				SteamId = player.SteamId.Value,
				Username = player.Username.Value,
				Number = player.Number.Value,
				PatreonLevel = player.PatreonLevel.Value,
				AdminLevel = player.AdminLevel.Value,
				IsMuted = player.IsMuted.Value
			});
		}
	}

	public void Server_AddPlayerDespawnedEvent(Player player)
	{
		if (NetworkManager.Singleton.IsServer && IsRecording)
		{
			if ((bool)player.PlayerBody)
			{
				bindings.RemovePlayerBody(player.PlayerBody);
			}
			if ((bool)player.Stick)
			{
				bindings.RemovePose(player.Stick.SynchronizedObject);
			}
			Server_AddReplayEvent("PlayerDespawned", new ReplayPlayerDespawned
			{
				OwnerClientId = player.OwnerClientId
			});
		}
	}

	public void Server_AddPlayerBodySpawnedEvent(PlayerBody playerBody)
	{
		if (NetworkManager.Singleton.IsServer && IsRecording)
		{
			int retainedTicks = Server_GetRetainedTicks();
			ReplayTrack<ReplayPoseSample> replayTrack = new ReplayTrack<ReplayPoseSample>(retainedTicks);
			ReplayTrack<ReplayPlayerBodyStateSample> stateTrack = new ReplayTrack<ReplayPlayerBodyStateSample>(retainedTicks);
			ReplayTrack<ReplayPlayerInputSample> inputTrack = new ReplayTrack<ReplayPlayerInputSample>(retainedTicks);
			bindings.AddPose(playerBody.SynchronizedObject, replayTrack);
			bindings.AddPlayerBody(playerBody, stateTrack, inputTrack);
			Server_AddReplayEvent("PlayerBodySpawned", new ReplayPlayerBodySpawned
			{
				OwnerClientId = playerBody.OwnerClientId,
				Position = playerBody.transform.position,
				Rotation = playerBody.transform.rotation,
				GameState = playerBody.Player.GameState.Value,
				CustomizationState = playerBody.Player.CustomizationState.Value,
				Username = playerBody.Player.Username.Value,
				Number = playerBody.Player.Number.Value,
				PoseTrack = replayTrack,
				StateTrack = stateTrack,
				InputTrack = inputTrack
			});
		}
	}

	public void Server_AddPlayerBodyDespawnedEvent(PlayerBody playerBody)
	{
		if (NetworkManager.Singleton.IsServer && IsRecording)
		{
			bindings.RemovePlayerBody(playerBody);
			Server_AddReplayEvent("PlayerBodyDespawned", new ReplayPlayerBodyDespawned
			{
				OwnerClientId = playerBody.OwnerClientId
			});
		}
	}

	public void Server_AddStickSpawnedEvent(Stick stick)
	{
		if (NetworkManager.Singleton.IsServer && IsRecording)
		{
			ReplayTrack<ReplayPoseSample> replayTrack = new ReplayTrack<ReplayPoseSample>(Server_GetRetainedTicks());
			bindings.AddPose(stick.SynchronizedObject, replayTrack);
			Server_AddReplayEvent("StickSpawned", new ReplayStickSpawned
			{
				OwnerClientId = stick.OwnerClientId,
				Position = stick.transform.position,
				Rotation = stick.transform.rotation,
				PoseTrack = replayTrack
			});
		}
	}

	public void Server_AddStickDespawnedEvent(Stick stick)
	{
		if (NetworkManager.Singleton.IsServer && IsRecording)
		{
			bindings.RemovePose(stick.SynchronizedObject);
			Server_AddReplayEvent("StickDespawned", new ReplayStickDespawned
			{
				OwnerClientId = stick.OwnerClientId
			});
		}
	}

	public void Server_AddPuckSpawnedEvent(Puck puck)
	{
		if (NetworkManager.Singleton.IsServer && IsRecording)
		{
			ReplayTrack<ReplayPoseSample> replayTrack = new ReplayTrack<ReplayPoseSample>(Server_GetRetainedTicks());
			bindings.AddPose(puck.SynchronizedObject, replayTrack);
			Server_AddReplayEvent("PuckSpawned", new ReplayPuckSpawned
			{
				NetworkObjectId = puck.NetworkObjectId,
				Position = puck.transform.position,
				Rotation = puck.transform.rotation,
				PoseTrack = replayTrack
			});
		}
	}

	public void Server_AddPuckDespawnedEvent(Puck puck)
	{
		if (NetworkManager.Singleton.IsServer && IsRecording)
		{
			bindings.RemovePose(puck.SynchronizedObject);
			Server_AddReplayEvent("PuckDespawned", new ReplayPuckDespawned
			{
				NetworkObjectId = puck.NetworkObjectId
			});
		}
	}
}
