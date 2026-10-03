using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Level : NetworkBehaviour
{
	private static readonly Logger Logger = new Logger("Level");

	[Header("References")]
	[SerializeField]
	private GameObject boundsGameObject;

	[SerializeField]
	private Light blueGoalLight;

	[SerializeField]
	private Light redGoalLight;

	[SerializeField]
	private SynchronizedAudio blueGoalSound;

	[SerializeField]
	private SynchronizedAudio redGoalSound;

	[SerializeField]
	private List<SynchronizedAudio> cheerSounds = new List<SynchronizedAudio>();

	[SerializeField]
	private SynchronizedAudio hornSound;

	[HideInInspector]
	public Bounds Bounds;

	private void Awake()
	{
		MeshRenderer component = boundsGameObject.GetComponent<MeshRenderer>();
		if (component == null)
		{
			Logger.Warning("boundsGameObject does not have a MeshRenderer component");
		}
		else
		{
			Bounds = component.bounds;
		}
	}

	protected override void OnNetworkPostSpawn()
	{
		EventManager.TriggerEvent("Event_Everyone_OnLevelSpawned", new Dictionary<string, object> { { "level", this } });
		base.OnNetworkPostSpawn();
	}

	public override void OnNetworkDespawn()
	{
		EventManager.TriggerEvent("Event_Everyone_OnLevelDespawned", new Dictionary<string, object> { { "level", this } });
		base.OnNetworkDespawn();
	}

	public void SetBlueGoalLightEnabled(bool isEnabled)
	{
		blueGoalLight.enabled = isEnabled;
	}

	public void SetRedGoalLightEnabled(bool isEnabled)
	{
		redGoalLight.enabled = isEnabled;
	}

	public void Server_PlayBlueGoalSound()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			blueGoalSound.Server_Play();
		}
	}

	public void Server_PlayRedGoalSound()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			redGoalSound.Server_Play();
		}
	}

	public void Server_PlayerCheerSound(float duration)
	{
		if (NetworkManager.Singleton.IsServer)
		{
			cheerSounds.ForEach((SynchronizedAudio cheerSound) =>
			{
				cheerSound.Server_Play(-1f, -1f, isOneShot: false, -1, 0f, randomClip: false, randomTime: false, fadeIn: true, 3f, fadeOut: true, 3f, duration);
			});
		}
	}

	public void Server_PlayHornSound()
	{
		if (NetworkManager.Singleton.IsServer)
		{
			hornSound.Server_Play();
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
		return "Level";
	}
}
