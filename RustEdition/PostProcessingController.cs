using System.Collections.Generic;
using UnityEngine;

public class PostProcessingController : MonoBehaviour
{
	private PostProcessing postProcessing;

	private void Awake()
	{
		postProcessing = GetComponent<PostProcessing>();
		EventManager.AddEventListener("Event_OnShowPuckSilhouetteChanged", Event_OnShowPuckSilhouetteChanged);
		EventManager.AddEventListener("Event_OnShowPuckOutlineChanged", Event_OnShowPuckOutlineChanged);
		EventManager.AddEventListener("Event_OnQualityChanged", Event_OnQualityChanged);
		EventManager.AddEventListener("Event_OnMotionBlurChanged", Event_OnMotionBlurChanged);
	}

	private void Start()
	{
		postProcessing.SetPuckSilhouette(SettingsManager.ShowPuckSilhouette);
		postProcessing.SetPuckOutline(SettingsManager.ShowPuckOutline);
		postProcessing.SetQuality(SettingsManager.Quality);
		postProcessing.SetMotionBlur(SettingsManager.MotionBlur);
	}

	private void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_OnShowPuckSilhouetteChanged", Event_OnShowPuckSilhouetteChanged);
		EventManager.RemoveEventListener("Event_OnShowPuckOutlineChanged", Event_OnShowPuckOutlineChanged);
		EventManager.RemoveEventListener("Event_OnQualityChanged", Event_OnQualityChanged);
		EventManager.RemoveEventListener("Event_OnMotionBlurChanged", Event_OnMotionBlurChanged);
	}

	private void Event_OnShowPuckSilhouetteChanged(Dictionary<string, object> message)
	{
		bool puckSilhouette = (bool)message["value"];
		postProcessing.SetPuckSilhouette(puckSilhouette);
	}

	private void Event_OnShowPuckOutlineChanged(Dictionary<string, object> message)
	{
		bool puckOutline = (bool)message["value"];
		postProcessing.SetPuckOutline(puckOutline);
	}

	private void Event_OnQualityChanged(Dictionary<string, object> message)
	{
		ApplicationQuality quality = (ApplicationQuality)message["value"];
		postProcessing.SetQuality(quality);
	}

	private void Event_OnMotionBlurChanged(Dictionary<string, object> message)
	{
		bool motionBlur = (bool)message["value"];
		postProcessing.SetMotionBlur(motionBlur);
	}
}
