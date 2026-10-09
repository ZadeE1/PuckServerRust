using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI;

[UxmlElement]
public class Link : VisualElement
{
	[Serializable]
	[CompilerGenerated]
	public new class UxmlSerializedData : VisualElement.UxmlSerializedData
	{
		[SerializeField]
		private string Text;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags Text_UxmlAttributeFlags;

		[RegisterUxmlCache]
		[Conditional("UNITY_EDITOR")]
		public new static void Register()
		{
			UxmlDescriptionCache.RegisterType(typeof(UxmlSerializedData), new UxmlAttributeNames[1]
			{
				new UxmlAttributeNames("Text", "text", null)
			});
		}

		public override object CreateInstance()
		{
			return new Link();
		}

		public override void Deserialize(object obj)
		{
			base.Deserialize(obj);
			Link link = (Link)obj;
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(Text_UxmlAttributeFlags))
			{
				link.Text = Text;
			}
		}
	}

	private string text;

	public Action Ready;

	public Action Clicked;

	private Label textLabel;

	[UxmlAttribute]
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

	public Link()
	{
		RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
		RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
	}

	private void Update()
	{
		if (textLabel != null)
		{
			textLabel.text = "<a><u>" + Text + "</u></a>";
		}
	}

	private void OnAttachToPanel(AttachToPanelEvent e)
	{
		textLabel = this.Query<Label>("TextLabel");
		if (textLabel != null)
		{
			textLabel.RegisterCallback<ClickEvent>(OnTextLabelClicked);
		}
		Update();
		Ready?.Invoke();
	}

	private void OnDetachFromPanel(DetachFromPanelEvent e)
	{
		if (textLabel != null)
		{
			textLabel.UnregisterCallback<ClickEvent>(OnTextLabelClicked);
		}
	}

	private void OnTextLabelClicked(ClickEvent e)
	{
		Clicked?.Invoke();
	}
}
