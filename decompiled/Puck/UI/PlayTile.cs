using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI;

[UxmlElement]
public class PlayTile : VisualElement
{
	[Serializable]
	[CompilerGenerated]
	public new class UxmlSerializedData : VisualElement.UxmlSerializedData
	{
		[SerializeField]
		private string Text;

		[SerializeField]
		private string Subtitle;

		[SerializeField]
		private string Description;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags Text_UxmlAttributeFlags;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags Subtitle_UxmlAttributeFlags;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags Description_UxmlAttributeFlags;

		[RegisterUxmlCache]
		[Conditional("UNITY_EDITOR")]
		public new static void Register()
		{
			UxmlDescriptionCache.RegisterType(typeof(UxmlSerializedData), new UxmlAttributeNames[3]
			{
				new UxmlAttributeNames("Text", "text", null),
				new UxmlAttributeNames("Subtitle", "subtitle", null),
				new UxmlAttributeNames("Description", "description", null)
			});
		}

		public override object CreateInstance()
		{
			return new PlayTile();
		}

		public override void Deserialize(object obj)
		{
			base.Deserialize(obj);
			PlayTile playTile = (PlayTile)obj;
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(Text_UxmlAttributeFlags))
			{
				playTile.Text = Text;
			}
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(Subtitle_UxmlAttributeFlags))
			{
				playTile.Subtitle = Subtitle;
			}
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(Description_UxmlAttributeFlags))
			{
				playTile.Description = Description;
			}
		}
	}

	private string text;

	private string subtitle;

	private string description;

	private Label textLabel;

	private Label subtitleLabel;

	private Label descriptionLabel;

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

	[UxmlAttribute]
	public string Subtitle
	{
		get
		{
			return subtitle;
		}
		set
		{
			if (!(subtitle == value))
			{
				subtitle = value;
				Update();
			}
		}
	}

	[UxmlAttribute]
	public string Description
	{
		get
		{
			return description;
		}
		set
		{
			if (!(description == value))
			{
				description = value;
				Update();
			}
		}
	}

	public PlayTile()
	{
		RegisterCallback<AttachToPanelEvent>(OnAttachToPanel, TrickleDown.TrickleDown);
	}

	private void OnAttachToPanel(AttachToPanelEvent e)
	{
		textLabel = this.Query<Label>("TextLabel");
		subtitleLabel = this.Query<Label>("SubtitleLabel");
		descriptionLabel = this.Query<Label>("DescriptionLabel");
		Update();
	}

	private void Update()
	{
		if (textLabel != null)
		{
			textLabel.text = Text;
		}
		if (subtitleLabel != null)
		{
			subtitleLabel.text = Subtitle;
		}
		if (descriptionLabel != null)
		{
			descriptionLabel.text = Description;
		}
	}
}
