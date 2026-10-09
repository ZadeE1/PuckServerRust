using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MeshGhoster : MonoBehaviour
{
	private static readonly Logger Logger = new Logger("MeshGhoster");

	private static readonly int GhostFadeId = Shader.PropertyToID("_GhostFade");

	private static readonly int CameraFadeId = Shader.PropertyToID("_CameraFade");

	private static readonly int FadeNearId = Shader.PropertyToID("_FadeNear");

	private static readonly int FadeFarId = Shader.PropertyToID("_FadeFar");

	private static readonly int NearAlphaId = Shader.PropertyToID("_NearAlpha");

	private static readonly int FarAlphaId = Shader.PropertyToID("_FarAlpha");

	private static readonly int BaseMapId = Shader.PropertyToID("_BaseMap");

	private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

	private static readonly int BumpMapId = Shader.PropertyToID("_BumpMap");

	private static readonly int BaseOpacityId = Shader.PropertyToID("_BaseOpacity");

	private static readonly int SurfaceId = Shader.PropertyToID("_Surface");

	private const string DepthShaderName = "Custom/Depth";

	[Header("References")]
	[SerializeField]
	private Material ghostMaterial;

	private Renderer[] ghostableRenderers;

	private TMP_Text[] ghostableTexts;

	private Material[][] preGhostMaterials;

	private Material[][] ghostInstances;

	private float[] preGhostTextAlphas;

	private float ghostFadeNear;

	private float ghostFadeFar;

	private float ghostNearAlpha;

	private float ghostFarAlpha;

	private float ghostCameraFade;

	private float strength;

	private Camera ghostCamera;

	private Camera GhostCamera
	{
		get
		{
			if (ghostCamera == null || !ghostCamera.isActiveAndEnabled)
			{
				ghostCamera = Camera.main;
			}
			return ghostCamera;
		}
	}

	public void SetTransparency(float strength)
	{
		strength = Mathf.Clamp01(strength);
		if (this.strength == strength)
		{
			return;
		}
		if (strength > 0f && ghostMaterial == null)
		{
			Logger.Warning("'" + name + "' has no ghost material assigned — can't render the crease ghost.");
			return;
		}
		bool flag = this.strength > 0f;
		this.strength = strength;
		if (strength > 0f && !flag)
		{
			ApplyGhostMaterials();
		}
		if (strength <= 0f)
		{
			RestoreMaterials();
		}
		else
		{
			ApplyGhostStrength();
		}
	}

	private void Update()
	{
		if (ghostInstances != null)
		{
			ApplyGhostTextAlpha();
		}
	}

	private void ApplyGhostMaterials()
	{
		if (ghostableRenderers == null)
		{
			ghostableRenderers = CollectGhostableRenderers();
			ghostableTexts = GetComponentsInChildren<TMP_Text>(includeInactive: true);
		}
		Shader shader = ghostMaterial.shader;
		ghostFadeNear = ghostMaterial.GetFloat(FadeNearId);
		ghostFadeFar = ghostMaterial.GetFloat(FadeFarId);
		ghostNearAlpha = ghostMaterial.GetFloat(NearAlphaId);
		ghostFarAlpha = ghostMaterial.GetFloat(FarAlphaId);
		ghostCameraFade = ghostMaterial.GetFloat(CameraFadeId);
		CaptureTextAlphas();
		preGhostMaterials = new Material[ghostableRenderers.Length][];
		ghostInstances = new Material[ghostableRenderers.Length][];
		for (int i = 0; i < ghostableRenderers.Length; i++)
		{
			Renderer renderer = ghostableRenderers[i];
			if (!(renderer == null))
			{
				Material[] sharedMaterials = renderer.sharedMaterials;
				preGhostMaterials[i] = sharedMaterials;
				Material[] array = new Material[sharedMaterials.Length];
				for (int j = 0; j < array.Length; j++)
				{
					array[j] = BuildGhostInstance(sharedMaterials[j], shader);
				}
				renderer.sharedMaterials = array;
				ghostInstances[i] = array;
			}
		}
	}

	private Renderer[] CollectGhostableRenderers()
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>(includeInactive: true);
		List<Renderer> list = new List<Renderer>(componentsInChildren.Length);
		Renderer[] array = componentsInChildren;
		foreach (Renderer renderer in array)
		{
			Shader shader = ((renderer != null && renderer.sharedMaterial != null) ? renderer.sharedMaterial.shader : null);
			if (!(shader != null) || !shader.name.StartsWith("TextMeshPro"))
			{
				list.Add(renderer);
			}
		}
		return list.ToArray();
	}

	private Material BuildGhostInstance(Material original, Shader ghostShader)
	{
		if (original == null)
		{
			return new Material(ghostShader)
			{
				hideFlags = HideFlags.HideAndDontSave
			};
		}
		if (original.shader != null && original.shader.name == "Custom/Depth")
		{
			return original;
		}
		Color color = original.color;
		Texture mainTexture = original.mainTexture;
		Vector2 mainTextureScale = original.mainTextureScale;
		Vector2 mainTextureOffset = original.mainTextureOffset;
		Texture texture = (original.HasProperty(BumpMapId) ? original.GetTexture(BumpMapId) : null);
		Material material = new Material(original)
		{
			hideFlags = HideFlags.HideAndDontSave
		};
		material.shader = ghostShader;
		material.SetColor(BaseColorId, color);
		material.SetTexture(BaseMapId, mainTexture);
		material.SetTextureScale(BaseMapId, mainTextureScale);
		material.SetTextureOffset(BaseMapId, mainTextureOffset);
		if (texture != null)
		{
			material.SetTexture(BumpMapId, texture);
		}
		bool flag = (original.HasProperty(SurfaceId) && original.GetFloat(SurfaceId) >= 0.5f) || original.renderQueue >= 2501;
		material.SetFloat(BaseOpacityId, flag ? Mathf.Clamp01(color.a) : 1f);
		if (flag)
		{
			material.SetShaderPassEnabled("SRPDefaultUnlit", enabled: false);
		}
		material.SetFloat(FadeNearId, ghostFadeNear);
		material.SetFloat(FadeFarId, ghostFadeFar);
		material.SetFloat(NearAlphaId, ghostNearAlpha);
		material.SetFloat(FarAlphaId, ghostFarAlpha);
		material.SetFloat(CameraFadeId, ghostCameraFade);
		material.SetFloat(GhostFadeId, strength);
		return material;
	}

	private void ApplyGhostStrength()
	{
		if (ghostInstances == null)
		{
			return;
		}
		Material[][] array = ghostInstances;
		foreach (Material[] array2 in array)
		{
			if (array2 == null)
			{
				continue;
			}
			Material[] array3 = array2;
			foreach (Material material in array3)
			{
				if (material != null)
				{
					material.SetFloat(GhostFadeId, strength);
				}
			}
		}
	}

	private void ApplyGhostTextAlpha()
	{
		if (ghostableTexts == null)
		{
			return;
		}
		Camera camera = GhostCamera;
		Vector3 cameraPosition = ((camera != null) ? camera.transform.position : Vector3.zero);
		TMP_Text[] array = ghostableTexts;
		foreach (TMP_Text tMP_Text in array)
		{
			if (!((Object)(object)tMP_Text == null))
			{
				tMP_Text.alpha = ((camera != null) ? GhostAlphaAt(tMP_Text.transform.position, cameraPosition) : Mathf.Lerp(1f, ghostFarAlpha, strength));
			}
		}
	}

	private float GhostAlphaAt(Vector3 worldPosition, Vector3 cameraPosition)
	{
		float value = Vector3.Distance(worldPosition, cameraPosition);
		float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(ghostFadeNear, ghostFadeFar, value));
		float b = Mathf.Lerp(ghostNearAlpha, ghostFarAlpha, t);
		float b2 = Mathf.Lerp(ghostFarAlpha, b, ghostCameraFade);
		return Mathf.Lerp(1f, b2, strength);
	}

	private void CaptureTextAlphas()
	{
		if (ghostableTexts != null)
		{
			preGhostTextAlphas = new float[ghostableTexts.Length];
			for (int i = 0; i < ghostableTexts.Length; i++)
			{
				preGhostTextAlphas[i] = (((Object)(object)ghostableTexts[i] != null) ? ghostableTexts[i].alpha : 1f);
			}
		}
	}

	private void RestoreMaterials()
	{
		strength = 0f;
		if (ghostInstances == null)
		{
			return;
		}
		for (int i = 0; i < ghostableRenderers.Length; i++)
		{
			Renderer renderer = ghostableRenderers[i];
			if (renderer != null && preGhostMaterials[i] != null)
			{
				renderer.sharedMaterials = preGhostMaterials[i];
			}
			if (ghostInstances[i] == null)
			{
				continue;
			}
			for (int j = 0; j < ghostInstances[i].Length; j++)
			{
				Material material = ghostInstances[i][j];
				if (material != null && material != preGhostMaterials[i][j])
				{
					Object.Destroy(material);
				}
			}
		}
		if (ghostableTexts != null && preGhostTextAlphas != null)
		{
			for (int k = 0; k < ghostableTexts.Length; k++)
			{
				if ((Object)(object)ghostableTexts[k] != null)
				{
					ghostableTexts[k].alpha = preGhostTextAlphas[k];
				}
			}
		}
		preGhostMaterials = null;
		ghostInstances = null;
		preGhostTextAlphas = null;
	}
}
