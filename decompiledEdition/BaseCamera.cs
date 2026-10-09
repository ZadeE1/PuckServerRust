using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class BaseCamera : NetworkBehaviour
{
	[Header("Settings")]
	public CameraType Type;

	[HideInInspector]
	public Camera UnityCamera;

	[HideInInspector]
	public AudioListener AudioListener;

	[HideInInspector]
	public bool IsEnabled;

	public virtual void Awake()
	{
		UnityCamera = GetComponent<Camera>();
		AudioListener = GetComponent<AudioListener>();
		UnityCamera.enabled = IsEnabled;
		AudioListener.enabled = IsEnabled;
	}

	public virtual void Start()
	{
		EventManager.TriggerEvent("Event_OnBaseCameraStarted", new Dictionary<string, object> { { "baseCamera", this } });
	}

	public override void OnDestroy()
	{
		Disable();
		EventManager.TriggerEvent("Event_OnBaseCameraDestroyed", new Dictionary<string, object> { { "baseCamera", this } });
		base.OnDestroy();
	}

	public virtual bool Enable()
	{
		if (IsEnabled)
		{
			return false;
		}
		IsEnabled = true;
		UnityCamera.enabled = IsEnabled;
		AudioListener.enabled = IsEnabled;
		EventManager.TriggerEvent("Event_OnBaseCameraEnabled", new Dictionary<string, object> { { "baseCamera", this } });
		return true;
	}

	public virtual bool Disable()
	{
		if (!IsEnabled)
		{
			return false;
		}
		IsEnabled = false;
		UnityCamera.enabled = IsEnabled;
		AudioListener.enabled = IsEnabled;
		EventManager.TriggerEvent("Event_OnBaseCameraDisabled", new Dictionary<string, object> { { "baseCamera", this } });
		return true;
	}

	public virtual void SetFieldOfView(float fieldOfView)
	{
		UnityCamera.fieldOfView = fieldOfView;
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
		return "BaseCamera";
	}
}
