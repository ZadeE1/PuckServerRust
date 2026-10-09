using System;
using DG.Tweening;
using UI;
using UnityEngine.UIElements;

public class Overlay
{
	public VisualElement VisualElement;

	public string Identifier;

	public bool RequiresSpinner;

	public bool FadeIn;

	public bool FadeOut;

	public float FadeTime;

	public bool AutoHide;

	public float HideTimeout;

	public Action Showing;

	public Action Shown;

	public Action Hiding;

	public Action Hidden;

	private Tween fadeInTween;

	private Tween fadeOutTween;

	private Tween autoHideTween;

	private Spinner spinner;

	public Overlay(VisualElement visualElement, string identifier, bool requiresSpinner = false)
	{
		VisualElement = visualElement;
		Identifier = identifier;
		VisualElement.style.display = DisplayStyle.None;
		VisualElement.style.opacity = 0f;
		spinner = VisualElement.Query<Spinner>();
		spinner.style.display = ((!requiresSpinner) ? DisplayStyle.None : DisplayStyle.Flex);
	}

	public void Show()
	{
		fadeOutTween?.Kill();
		if (fadeInTween != null)
		{
			fadeInTween.Kill();
		}
		if (autoHideTween != null)
		{
			autoHideTween.Kill();
		}
		Showing?.Invoke();
		if (FadeIn)
		{
			fadeInTween = DOVirtual.Float(VisualElement.style.opacity.value, 1f, FadeTime, (float value) =>
			{
				VisualElement.style.opacity = value;
			}).OnStart(() =>
			{
				VisualElement.style.display = DisplayStyle.Flex;
				Shown?.Invoke();
			});
			if (AutoHide)
			{
				autoHideTween = DOVirtual.DelayedCall(FadeTime + HideTimeout, () =>
				{
					Hide();
				});
			}
			return;
		}
		VisualElement.style.display = DisplayStyle.Flex;
		VisualElement.style.opacity = 1f;
		Shown?.Invoke();
		if (AutoHide)
		{
			autoHideTween = DOVirtual.DelayedCall(HideTimeout, () =>
			{
				Hide();
			});
		}
	}

	public void Hide()
	{
		fadeInTween?.Kill();
		if (fadeOutTween != null)
		{
			fadeOutTween.Kill();
		}
		if (autoHideTween != null)
		{
			autoHideTween.Kill();
		}
		Hiding?.Invoke();
		if (FadeOut)
		{
			fadeOutTween = DOVirtual.Float(VisualElement.style.opacity.value, 0f, FadeTime, (float value) =>
			{
				VisualElement.style.opacity = value;
			}).OnComplete(() =>
			{
				VisualElement.style.display = DisplayStyle.None;
				Hidden?.Invoke();
			});
		}
		else
		{
			VisualElement.style.display = DisplayStyle.None;
			VisualElement.style.opacity = 0f;
			Hidden?.Invoke();
		}
	}

	public void Dispose()
	{
		fadeInTween?.Kill();
		fadeOutTween?.Kill();
		autoHideTween?.Kill();
	}
}
