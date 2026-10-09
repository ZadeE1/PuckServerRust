using System.Collections.Generic;
using UnityEngine;

public class BaseCameraController : MonoBehaviour
{
	private BaseCamera baseCamera;

	public virtual void Awake()
	{
		baseCamera = GetComponent<BaseCamera>();
		EventManager.AddEventListener("Event_OnFovChanged", Event_OnFovChanged);
	}

	public virtual void Start()
	{
		baseCamera.SetFieldOfView(SettingsManager.Fov);
	}

	public virtual void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_OnFovChanged", Event_OnFovChanged);
	}

	private void Event_OnFovChanged(Dictionary<string, object> message)
	{
		float fieldOfView = (float)message["value"];
		baseCamera.SetFieldOfView(fieldOfView);
	}
}
