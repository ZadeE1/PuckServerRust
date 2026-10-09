using System.Collections.Generic;
using UnityEngine;

public class PuckElevationIndicatorController : MonoBehaviour
{
	private PuckElevationIndicator puckElevationIndicator;

	private void Awake()
	{
		puckElevationIndicator = GetComponent<PuckElevationIndicator>();
	}

	private void Start()
	{
		EventManager.AddEventListener("Event_OnShowPuckElevationChanged", Event_OnShowPuckElevationChanged);
		puckElevationIndicator.IsVisible = SettingsManager.ShowPuckElevation;
	}

	private void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_OnShowPuckElevationChanged", Event_OnShowPuckElevationChanged);
	}

	private void Event_OnShowPuckElevationChanged(Dictionary<string, object> message)
	{
		bool isVisible = (bool)message["value"];
		puckElevationIndicator.IsVisible = isVisible;
	}
}
