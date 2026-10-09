using Linework.SoftOutline;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessing : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private UniversalRenderPipelineAsset renderPipelineAsset;

	[SerializeField]
	private UniversalRendererData universalRendererData;

	[SerializeField]
	private SoftOutlineSettings puckOutlineSettings;

	private Volume volume;

	private void Awake()
	{
		volume = GetComponent<Volume>();
	}

	public void SetPuckSilhouette(bool enabled)
	{
		universalRendererData.rendererFeatures.Find((ScriptableRendererFeature x) => x.name == "Puck Silhouette").SetActive(enabled);
	}

	public void SetPuckOutline(bool enabled)
	{
		puckOutlineSettings.SetActive(enabled);
	}

	public void SetQuality(ApplicationQuality quality)
	{
		switch (quality)
		{
		case ApplicationQuality.Low:
			renderPipelineAsset.msaaSampleCount = 1;
			break;
		case ApplicationQuality.Medium:
			renderPipelineAsset.msaaSampleCount = 2;
			break;
		case ApplicationQuality.High:
			renderPipelineAsset.msaaSampleCount = 4;
			break;
		case ApplicationQuality.Ultra:
			renderPipelineAsset.msaaSampleCount = 8;
			break;
		}
	}

	public void SetMotionBlur(bool enabled)
	{
		if (volume.profile.TryGet<MotionBlur>(out var component))
		{
			component.active = enabled;
		}
	}
}
