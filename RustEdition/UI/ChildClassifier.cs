using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

namespace UI;

[UxmlElement]
public class ChildClassifier : VisualElement
{
	[Serializable]
	[CompilerGenerated]
	public new class UxmlSerializedData : VisualElement.UxmlSerializedData
	{
		public override object CreateInstance()
		{
			return new ChildClassifier();
		}
	}

	private VisualElement firstChild;

	private VisualElement lastChild;

	public ChildClassifier()
	{
		RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
		RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
		RegisterCallback<ChildAddedEvent>(OnChildAdded);
		RegisterCallback<BeforeChildRemovedEvent>(OnBeforeChildRemoved);
		RegisterCallback<HierarchyChangedEvent>(OnHierarchyChanged);
	}

	private List<VisualElement> GetChildren()
	{
		return Children().ToList();
	}

	private List<VisualElement> GetVisibleChildren()
	{
		return Children().ToList().FindAll((VisualElement child) => child.resolvedStyle.display == DisplayStyle.Flex);
	}

	private void SetFirstChild(VisualElement child)
	{
		if (firstChild != null)
		{
			DisposeChild(firstChild);
		}
		firstChild = child;
		firstChild.EnableInClassList("firstChild", enable: true);
	}

	private void SetLastChild(VisualElement child)
	{
		if (lastChild != null)
		{
			DisposeChild(lastChild);
		}
		lastChild = child;
		lastChild.EnableInClassList("lastChild", enable: true);
	}

	private void SetChildClasses(List<VisualElement> children)
	{
		if (children.Count != 0)
		{
			if (children.Count == 1)
			{
				SetLastChild(children[0]);
				return;
			}
			SetFirstChild(children[0]);
			SetLastChild(children[children.Count - 1]);
		}
	}

	private void DisposeChild(VisualElement child)
	{
		child.EnableInClassList("firstChild", enable: false);
		child.EnableInClassList("lastChild", enable: false);
	}

	private void OnAttachToPanel(AttachToPanelEvent e)
	{
		GetChildren().ForEach((VisualElement child) =>
		{
			child.RegisterCallback<RenderingToggledEvent>(OnChildRenderingToggled);
		});
		List<VisualElement> visibleChildren = GetVisibleChildren();
		SetChildClasses(visibleChildren);
	}

	private void OnDetachFromPanel(DetachFromPanelEvent e)
	{
		GetChildren().ForEach((VisualElement child) =>
		{
			child.UnregisterCallback<RenderingToggledEvent>(OnChildRenderingToggled);
		});
	}

	private void OnChildAdded(ChildAddedEvent e)
	{
		int index = e.index;
		e.child.RegisterCallback<RenderingToggledEvent>(OnChildRenderingToggled);
		List<VisualElement> visibleChildren = GetVisibleChildren();
		if (index == 0 || index == visibleChildren.Count - 1)
		{
			SetChildClasses(visibleChildren);
		}
	}

	private void OnBeforeChildRemoved(BeforeChildRemovedEvent e)
	{
		int index = e.index;
		VisualElement child = e.child;
		child.UnregisterCallback<RenderingToggledEvent>(OnChildRenderingToggled);
		List<VisualElement> visibleChildren = GetVisibleChildren();
		if (index == 0 || index == visibleChildren.Count - 1)
		{
			List<VisualElement> childClasses = GetVisibleChildren().FindAll((VisualElement c) => c != child);
			SetChildClasses(childClasses);
		}
	}

	private void OnHierarchyChanged(HierarchyChangedEvent e)
	{
		List<VisualElement> visibleChildren = GetVisibleChildren();
		SetChildClasses(visibleChildren);
	}

	private void OnChildRenderingToggled(RenderingToggledEvent e)
	{
		List<VisualElement> visibleChildren = GetVisibleChildren();
		SetChildClasses(visibleChildren);
	}
}
