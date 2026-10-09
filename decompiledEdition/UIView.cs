using System;
using UnityEngine;
using UnityEngine.UIElements;

public class UIView : MonoBehaviour
{
	[Header("UI Settings")]
	public bool FocusRequiresMouse;

	public bool FocusIsInteractive;

	public bool VisibilityRequiresMouse;

	public bool VisibilityIsInteractive;

	public bool AlwaysVisible;

	private bool isVisible = true;

	private bool isFocused;

	[HideInInspector]
	public Action<UIView> OnVisibility;

	[HideInInspector]
	public Action<UIView> OnFocus;

	[HideInInspector]
	public VisualElement RootVisualElement;

	private VisualElement view;

	public bool IsVisible
	{
		get
		{
			return isVisible;
		}
		set
		{
			if (isVisible != value)
			{
				bool oldIsVisible = isVisible;
				isVisible = value;
				OnIsVisibileChanged(oldIsVisible, isVisible);
			}
		}
	}

	public bool IsFocused
	{
		get
		{
			return isFocused;
		}
		set
		{
			if (isFocused != value)
			{
				bool oldIsFocused = isFocused;
				isFocused = value;
				OnIsFocusedChanged(oldIsFocused, isFocused);
			}
		}
	}

	[HideInInspector]
	public int Order
	{
		get
		{
			if (!(View.parent is TemplateContainer))
			{
				return View.parent.IndexOf(View);
			}
			return View.parent.parent.IndexOf(View.parent);
		}
	}

	public VisualElement View
	{
		get
		{
			return view;
		}
		set
		{
			if (view != value)
			{
				VisualElement oldView = view;
				view = value;
				OnViewChanged(oldView, view);
			}
		}
	}

	public virtual bool Show()
	{
		if (IsVisible)
		{
			return false;
		}
		IsVisible = true;
		return true;
	}

	public virtual bool Hide()
	{
		if (!IsVisible || AlwaysVisible)
		{
			return false;
		}
		IsVisible = false;
		return true;
	}

	public virtual bool Toggle()
	{
		if (IsVisible)
		{
			return Hide();
		}
		return Show();
	}

	private void OnIsVisibileChanged(bool oldIsVisible, bool newIsVisible)
	{
		View.style.display = ((!newIsVisible) ? DisplayStyle.None : DisplayStyle.Flex);
		OnVisibility?.Invoke(this);
	}

	private void OnIsFocusedChanged(bool oldIsFocused, bool newIsFocused)
	{
		OnFocus?.Invoke(this);
	}

	private void OnViewChanged(VisualElement oldView, VisualElement newView)
	{
	}
}
