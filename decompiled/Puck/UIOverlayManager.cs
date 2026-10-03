using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

public class UIOverlayManager : UIView
{
	[Header("References")]
	public VisualTreeAsset overlayAsset;

	private VisualElement overlays;

	private VisualElement flashElement;

	private Sequence flashSequence;

	private Texture2D vignetteTexture;

	private Dictionary<string, Overlay> identifierOverlaysMap = new Dictionary<string, Overlay>();

	public void Initialize(VisualElement rootVisualElement)
	{
		View = rootVisualElement.Query<VisualElement>("OverlaysView");
		overlays = View.Query<VisualElement>("Overlays");
		overlays.Clear();
	}

	private void OnDestroy()
	{
		flashSequence?.Kill();
		if (vignetteTexture != null)
		{
			UnityEngine.Object.Destroy(vignetteTexture);
		}
		foreach (Overlay value in identifierOverlaysMap.Values)
		{
			value.Dispose();
		}
	}

	public void FlashScreen(Color color, float peakOpacity, float rampInSeconds, float holdSeconds, float rampOutSeconds)
	{
		if (overlays != null)
		{
			if (vignetteTexture == null)
			{
				vignetteTexture = CreateVignetteTexture();
			}
			if (flashElement == null)
			{
				flashElement = new VisualElement();
				flashElement.style.position = Position.Absolute;
				flashElement.style.left = 0f;
				flashElement.style.right = 0f;
				flashElement.style.top = 0f;
				flashElement.style.bottom = 0f;
				flashElement.style.opacity = 0f;
				flashElement.style.backgroundImage = new StyleBackground(vignetteTexture);
				flashElement.style.unityBackgroundScaleMode = ScaleMode.StretchToFill;
				flashElement.pickingMode = PickingMode.Ignore;
				overlays.Add(flashElement);
			}
			flashElement.BringToFront();
			flashElement.style.unityBackgroundImageTintColor = color;
			flashSequence?.Kill();
			flashSequence = TweenUtils.RampHoldRamp(0f, peakOpacity, rampInSeconds, holdSeconds, rampOutSeconds, Ease.OutQuad, Ease.OutQuad, (float value) =>
			{
				flashElement.style.opacity = value;
			});
			flashSequence.OnKill(() =>
			{
				flashSequence = null;
			});
		}
	}

	private Texture2D CreateVignetteTexture(int size = 256)
	{
		Texture2D texture2D = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: false)
		{
			wrapMode = TextureWrapMode.Clamp
		};
		Color[] array = new Color[size * size];
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				float num = (float)j / (float)(size - 1) * 2f - 1f;
				float num2 = (float)i / (float)(size - 1) * 2f - 1f;
				float num3 = Mathf.Clamp01((Mathf.Sqrt(num * num + num2 * num2) - 0.5f) / 0.70000005f);
				float a = num3 * num3 * (3f - 2f * num3);
				array[i * size + j] = new Color(1f, 1f, 1f, a);
			}
		}
		texture2D.SetPixels(array);
		texture2D.Apply();
		return texture2D;
	}

	public void ShowOverlay(string identifier, bool requiresSpinner = false, bool fadeIn = false, bool fadeOut = false, float fadeTime = 0.25f, bool autoHide = false, float hideTimeout = 0.25f)
	{
		Overlay overlay;
		if (!identifierOverlaysMap.ContainsKey(identifier))
		{
			overlay = new Overlay(overlayAsset.Instantiate(), identifier, requiresSpinner);
			Overlay overlay2 = overlay;
			overlay2.Hidden = (Action)Delegate.Combine(overlay2.Hidden, (Action)(() =>
			{
				overlay.Dispose();
				identifierOverlaysMap.Remove(identifier);
				overlays.Remove(overlay.VisualElement);
			}));
			identifierOverlaysMap.Add(identifier, overlay);
			overlays.Add(overlay.VisualElement);
		}
		else
		{
			overlay = identifierOverlaysMap[identifier];
		}
		overlay.FadeIn = fadeIn;
		overlay.FadeOut = fadeOut;
		overlay.FadeTime = fadeTime;
		overlay.AutoHide = autoHide;
		overlay.HideTimeout = hideTimeout;
		overlay.Show();
	}

	public void HideOverlay(string identifier)
	{
		if (identifierOverlaysMap.ContainsKey(identifier))
		{
			identifierOverlaysMap[identifier].Hide();
		}
	}
}
