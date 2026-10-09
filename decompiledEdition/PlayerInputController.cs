using System.Collections.Generic;
using Unity.Netcode;

public class PlayerInputController : NetworkBehaviour
{
	private PlayerInput playerInput;

	private void Awake()
	{
		playerInput = GetComponent<PlayerInput>();
	}

	private void Start()
	{
		playerInput.InitialLookAngle = SettingsManager.CameraAngle;
	}

	public override void OnNetworkSpawn()
	{
		EventManager.AddEventListener("Event_Everyone_OnPlayerBodySpawned", Event_Everyone_OnPlayerBodySpawned);
		EventManager.AddEventListener("Event_Everyone_OnPlayerHandednessChanged", Event_Everyone_OnPlayerHandednessChanged);
		EventManager.AddEventListener("Event_Everyone_OnServerChanged", Event_Everyone_OnServerChanged);
		EventManager.AddEventListener("Event_Server_OnClientSceneSynchronizeComplete", Event_Server_OnClientSceneSynchronizeComplete);
		EventManager.AddEventListener("Event_OnCameraAngleChanged", Event_OnCameraAngleChanged);
		base.OnNetworkSpawn();
	}

	public override void OnNetworkDespawn()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerBodySpawned", Event_Everyone_OnPlayerBodySpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerHandednessChanged", Event_Everyone_OnPlayerHandednessChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnServerChanged", Event_Everyone_OnServerChanged);
		EventManager.RemoveEventListener("Event_Server_OnClientSceneSynchronizeComplete", Event_Server_OnClientSceneSynchronizeComplete);
		EventManager.RemoveEventListener("Event_OnCameraAngleChanged", Event_OnCameraAngleChanged);
		base.OnNetworkDespawn();
	}

	private void Event_Everyone_OnPlayerBodySpawned(Dictionary<string, object> message)
	{
		PlayerBody playerBody = (PlayerBody)message["playerBody"];
		if (playerBody.Player.IsLocalPlayer)
		{
			playerInput.ResetInputs(playerBody.Player.Handedness.Value);
		}
	}

	private void Event_Everyone_OnPlayerHandednessChanged(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		if (player.IsLocalPlayer)
		{
			playerInput.ResetInputs(player.Handedness.Value);
		}
	}

	private void Event_Everyone_OnServerChanged(Dictionary<string, object> message)
	{
		playerInput.TickRate = NetworkBehaviourSingleton<ServerManager>.Instance.Server.Value.TickRate;
	}

	private void Event_Server_OnClientSceneSynchronizeComplete(Dictionary<string, object> message)
	{
		ulong num = (ulong)message["clientId"];
		if (num != 0L)
		{
			playerInput.Server_ForceSynchronizeClientId(num);
		}
	}

	private void Event_OnCameraAngleChanged(Dictionary<string, object> message)
	{
		float initialLookAngle = (float)message["value"];
		playerInput.InitialLookAngle = initialLookAngle;
	}

	protected override void __initializeVariables()
	{
		base.__initializeVariables();
	}

	protected override void __initializeRpcs()
	{
		base.__initializeRpcs();
	}

	protected override string __getTypeName()
	{
		return "PlayerInputController";
	}
}
