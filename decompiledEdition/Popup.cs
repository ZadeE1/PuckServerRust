using System.Collections.Generic;
using UI;
using UnityEngine.UIElements;

public class Popup
{
	public TemplateContainer TemplateContainer;

	public VisualElement VisualElement;

	public string Name;

	public string Title;

	public BasePopupContent Content;

	public bool ShowOkButton;

	public bool ShowCloseButton;

	public object Data;

	private VisualElement header;

	private VisualElement content;

	private VisualElement footer;

	private Label titleLabel;

	private Button okButton;

	private IconButton closeIconButton;

	public Popup(VisualElement visualElement, string name, string title, BasePopupContent content, bool showOkButton, bool showCloseButton, object data = null)
	{
		VisualElement = visualElement;
		Name = name;
		Title = title;
		Content = content;
		ShowOkButton = showOkButton;
		ShowCloseButton = showCloseButton;
		Data = data;
	}

	public void Initialize()
	{
		header = VisualElement.Query<VisualElement>("Header");
		content = VisualElement.Query<VisualElement>("Content");
		footer = VisualElement.Query<VisualElement>("Footer");
		titleLabel = header.Query<Label>();
		titleLabel.text = Title;
		closeIconButton = header.Query<VisualElement>("CloseIconButtonContainer").First().Query<IconButton>();
		closeIconButton.clicked += OnClickClose;
		okButton = footer.Query<Button>("OkButton");
		okButton.clicked += OnClickOk;
		if (!ShowOkButton)
		{
			okButton.style.display = DisplayStyle.None;
		}
		if (!ShowCloseButton)
		{
			closeIconButton.style.display = DisplayStyle.None;
		}
		Content.Initialize();
		content.Add(Content.VisualElement);
	}

	public void Dispose()
	{
		closeIconButton.clicked -= OnClickClose;
		okButton.clicked -= OnClickOk;
		content.Remove(Content.VisualElement);
		Content.Dispose();
	}

	private void OnClickClose()
	{
		EventManager.TriggerEvent("Event_OnPopupClickClose", new Dictionary<string, object> { { "popup", this } });
	}

	private void OnClickOk()
	{
		EventManager.TriggerEvent("Event_OnPopupClickOk", new Dictionary<string, object> { { "popup", this } });
	}
}
