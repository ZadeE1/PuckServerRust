using UnityEngine.UIElements;

public class PopupNotificationContent : BasePopupContent
{
	private string text;

	private Label textLabel;

	public string Text
	{
		get
		{
			return text;
		}
		set
		{
			if (!(text == value))
			{
				text = value;
				Update();
			}
		}
	}

	public PopupNotificationContent(VisualTreeAsset asset, string text)
		: base(asset)
	{
		this.text = text;
	}

	public override void Initialize()
	{
		base.Initialize();
		textLabel = VisualElement.Query<Label>("TextLabel");
		Update();
	}

	internal override void Update()
	{
		base.Update();
		if (textLabel != null)
		{
			textLabel.text = Text;
		}
	}
}
