using System.Collections.Generic;
using Steamworks;
using Unity.Netcode;

public class PlayerVoiceRecorderController : NetworkBehaviour
{
	private PlayerVoiceRecorder playerVoiceRecorder;

	private void Awake()
	{
		playerVoiceRecorder = GetComponent<PlayerVoiceRecorder>();
	}

	private void Start()
	{
		playerVoiceRecorder.IsEnabled = NetworkBehaviourSingleton<ServerManager>.Instance.Server.Value.UseVoip;
	}

	public override void OnNetworkSpawn()
	{
		EventManager.AddEventListener("Event_Everyone_OnPlayerTalkInput", Event_Everyone_OnPlayerTalkInput);
		EventManager.AddEventListener("Event_Everyone_OnServerChanged", Event_Everyone_OnServerChanged);
		base.OnNetworkSpawn();
	}

	public override void OnNetworkDespawn()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerTalkInput", Event_Everyone_OnPlayerTalkInput);
		EventManager.RemoveEventListener("Event_Everyone_OnServerChanged", Event_Everyone_OnServerChanged);
		base.OnNetworkDespawn();
	}

	private void Event_Everyone_OnPlayerTalkInput(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		bool flag = (bool)message["value"];
		if (OwnerClientId == player.OwnerClientId && player.IsLocalPlayer)
		{
			if (flag)
			{
				playerVoiceRecorder.Client_RequestVoiceStartRpc(SteamUser.GetVoiceOptimalSampleRate());
			}
			else
			{
				playerVoiceRecorder.Client_RequestVoiceStopRpc();
			}
		}
	}

	private void Event_Everyone_OnServerChanged(Dictionary<string, object> message)
	{
		playerVoiceRecorder.IsEnabled = NetworkBehaviourSingleton<ServerManager>.Instance.Server.Value.UseVoip;
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
		return "PlayerVoiceRecorderController";
	}
}
