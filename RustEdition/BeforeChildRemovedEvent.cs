using UnityEngine.UIElements;

public class BeforeChildRemovedEvent : EventBase<BeforeChildRemovedEvent>
{
	public int index;

	public VisualElement child;
}
