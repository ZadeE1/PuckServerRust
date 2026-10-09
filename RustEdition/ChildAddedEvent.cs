using UnityEngine.UIElements;

public class ChildAddedEvent : EventBase<ChildAddedEvent>
{
	public int index;

	public VisualElement child;
}
