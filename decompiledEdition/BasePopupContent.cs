using System;
using UnityEngine.UIElements;

public class BasePopupContent
{
	public Action Initialized;

	public Action Disposed;

	private VisualTreeAsset asset;

	public VisualElement VisualElement { get; set; }

	public BasePopupContent(VisualTreeAsset asset)
	{
		this.asset = asset;
	}

	public virtual void Initialize()
	{
		VisualElement = asset.Instantiate();
		Initialized?.Invoke();
	}

	public virtual void Dispose()
	{
		Disposed?.Invoke();
	}

	internal virtual void Update()
	{
	}
}
