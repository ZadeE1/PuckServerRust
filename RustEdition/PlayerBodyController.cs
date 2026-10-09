using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerBodyController : NetworkBehaviour
{
	private PlayerBody playerBody;

	private void Awake()
	{
		playerBody = GetComponent<PlayerBody>();
	}

	public override void OnNetworkSpawn()
	{
		EventManager.AddEventListener("Event_Everyone_OnPlayerGameStateChanged", Event_Everyone_OnPlayerGameStateChanged);
		EventManager.AddEventListener("Event_Everyone_OnPlayerUsernameChanged", Event_Everyone_OnPlayerUsernameChanged);
		EventManager.AddEventListener("Event_Everyone_OnPlayerNumberChanged", Event_Everyone_OnPlayerNumberChanged);
		EventManager.AddEventListener("Event_Everyone_OnPlayerCustomizationStateChanged", Event_Everyone_OnPlayerCustomizationStateChanged);
		EventManager.AddEventListener("Event_Everyone_OnPlayerVoiceStarted", Event_Everyone_OnPlayerVoiceStarted);
		EventManager.AddEventListener("Event_Everyone_OnPlayerVoiceStopped", Event_Everyone_OnPlayerVoiceStopped);
		EventManager.AddEventListener("Event_Server_OnPlayerJumpInput", Event_Server_OnPlayerJumpInput);
		EventManager.AddEventListener("Event_Server_OnPlayerDashLeftInput", Event_Server_OnPlayerDashLeftInput);
		EventManager.AddEventListener("Event_Server_OnPlayerDashRightInput", Event_Server_OnPlayerDashRightInput);
		EventManager.AddEventListener("Event_Server_OnPlayerTwistLeftInput", Event_Server_OnPlayerTwistLeftInput);
		EventManager.AddEventListener("Event_Server_OnPlayerTwistRightInput", Event_Server_OnPlayerTwistRightInput);
		EventManager.AddEventListener("Event_OnPlayerCameraEnabled", Event_OnPlayerCameraEnabled);
		EventManager.AddEventListener("Event_OnPlayerCameraDisabled", Event_OnPlayerCameraDisabled);
		base.OnNetworkSpawn();
	}

	public override void OnNetworkDespawn()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerGameStateChanged", Event_Everyone_OnPlayerGameStateChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerUsernameChanged", Event_Everyone_OnPlayerUsernameChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerNumberChanged", Event_Everyone_OnPlayerNumberChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerCustomizationStateChanged", Event_Everyone_OnPlayerCustomizationStateChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerVoiceStarted", Event_Everyone_OnPlayerVoiceStarted);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerVoiceStopped", Event_Everyone_OnPlayerVoiceStopped);
		EventManager.RemoveEventListener("Event_Server_OnPlayerJumpInput", Event_Server_OnPlayerJumpInput);
		EventManager.RemoveEventListener("Event_Server_OnPlayerDashLeftInput", Event_Server_OnPlayerDashLeftInput);
		EventManager.RemoveEventListener("Event_Server_OnPlayerDashRightInput", Event_Server_OnPlayerDashRightInput);
		EventManager.RemoveEventListener("Event_Server_OnPlayerTwistLeftInput", Event_Server_OnPlayerTwistLeftInput);
		EventManager.RemoveEventListener("Event_Server_OnPlayerTwistRightInput", Event_Server_OnPlayerTwistRightInput);
		EventManager.RemoveEventListener("Event_OnPlayerCameraEnabled", Event_OnPlayerCameraEnabled);
		EventManager.RemoveEventListener("Event_OnPlayerCameraDisabled", Event_OnPlayerCameraDisabled);
		base.OnNetworkDespawn();
	}

	private void Event_Everyone_OnPlayerGameStateChanged(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		PlayerGameState playerGameState = (PlayerGameState)message["oldGameState"];
		PlayerGameState playerGameState2 = (PlayerGameState)message["newGameState"];
		if (OwnerClientId == player.OwnerClientId && (playerGameState.Team != playerGameState2.Team || playerGameState.Role != playerGameState2.Role))
		{
			playerBody.ApplyCustomizations();
		}
	}

	private void Event_Everyone_OnPlayerUsernameChanged(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		if (OwnerClientId == player.OwnerClientId)
		{
			playerBody.ApplyCustomizations();
		}
	}

	private void Event_Everyone_OnPlayerNumberChanged(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		if (OwnerClientId == player.OwnerClientId)
		{
			playerBody.ApplyCustomizations();
		}
	}

	private void Event_Everyone_OnPlayerCustomizationStateChanged(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		if (OwnerClientId == player.OwnerClientId)
		{
			playerBody.ApplyCustomizations();
		}
	}

	private void Event_Everyone_OnPlayerVoiceStarted(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		AudioClip clip = (AudioClip)message["audioClip"];
		if (OwnerClientId == player.OwnerClientId && !player.IsLocalPlayer)
		{
			playerBody.VoiceAudioSource.clip = clip;
			playerBody.VoiceAudioSource.loop = true;
			playerBody.VoiceAudioSource.Play();
		}
	}

	private void Event_Everyone_OnPlayerVoiceStopped(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		if (OwnerClientId == player.OwnerClientId && !player.IsLocalPlayer)
		{
			playerBody.VoiceAudioSource.Stop();
		}
	}

	private void Event_Server_OnPlayerJumpInput(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		if (OwnerClientId == player.OwnerClientId)
		{
			playerBody.Jump();
		}
	}

	private void Event_Server_OnPlayerTwistLeftInput(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		if (OwnerClientId == player.OwnerClientId)
		{
			playerBody.TwistLeft();
		}
	}

	private void Event_Server_OnPlayerTwistRightInput(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		if (OwnerClientId == player.OwnerClientId)
		{
			playerBody.TwistRight();
		}
	}

	private void Event_Server_OnPlayerDashLeftInput(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		if (OwnerClientId == player.OwnerClientId)
		{
			playerBody.DashLeft();
		}
	}

	private void Event_Server_OnPlayerDashRightInput(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		if (OwnerClientId == player.OwnerClientId)
		{
			playerBody.DashRight();
		}
	}

	private void Event_OnPlayerCameraEnabled(Dictionary<string, object> message)
	{
		PlayerCamera playerCamera = (PlayerCamera)message["playerCamera"];
		if (OwnerClientId == playerCamera.OwnerClientId)
		{
			playerBody.MeshRendererHider.HideMeshRenderers();
		}
	}

	private void Event_OnPlayerCameraDisabled(Dictionary<string, object> message)
	{
		PlayerCamera playerCamera = (PlayerCamera)message["playerCamera"];
		if (OwnerClientId == playerCamera.OwnerClientId)
		{
			playerBody.MeshRendererHider.ShowMeshRenderers();
		}
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
		return "PlayerBodyController";
	}
}
