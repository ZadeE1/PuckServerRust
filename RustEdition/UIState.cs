using System;
using System.Collections.Generic;
using System.Linq;

public struct UIState
{
	public UIPhase Phase = UIPhase.None;

	public bool IsMouseRequired = false;

	public bool IsMouseOverUI = false;

	public List<UIView> InteractingViews = new List<UIView>();

	public bool IsInteracting
	{
		get
		{
			if (InteractingViews != null)
			{
				return InteractingViews.Count > 0;
			}
			return false;
		}
	}

	public UIState()
	{
	}

	public bool IsViewInteracting<T>() where T : UIView
	{
		if (InteractingViews == null)
		{
			return false;
		}
		foreach (UIView interactingView in InteractingViews)
		{
			if (interactingView is T)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsViewTopmostInteracting<T>() where T : UIView
	{
		if (InteractingViews == null || InteractingViews.Count == 0)
		{
			return false;
		}
		return InteractingViews[InteractingViews.Count - 1] is T;
	}

	public UIView GetTopmostInteractingView()
	{
		if (InteractingViews == null || InteractingViews.Count == 0)
		{
			return null;
		}
		return InteractingViews[InteractingViews.Count - 1];
	}

	public bool Equals(UIState other)
	{
		if (Phase == other.Phase && IsMouseRequired == other.IsMouseRequired && IsMouseOverUI == other.IsMouseOverUI)
		{
			return InteractingViews.SequenceEqual(other.InteractingViews);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is UIState other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Phase, IsMouseRequired, IsMouseOverUI, InteractingViews);
	}

	public override string ToString()
	{
		return $"Phase: {Phase}, IsMouseRequired: {IsMouseRequired}, IsMouseOverUI: {IsMouseOverUI}, IsInteracting: {IsInteracting}";
	}
}
