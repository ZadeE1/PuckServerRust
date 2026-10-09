using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI;

[UxmlElement]
public class IconButton : Button
{
	[Serializable]
	[CompilerGenerated]
	public new class UxmlSerializedData : Button.UxmlSerializedData
	{
		[SerializeField]
		private StyleBackground Texture;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags Texture_UxmlAttributeFlags;

		[RegisterUxmlCache]
		[Conditional("UNITY_EDITOR")]
		public new static void Register()
		{
			UxmlDescriptionCache.RegisterType(typeof(UxmlSerializedData), new UxmlAttributeNames[1]
			{
				new UxmlAttributeNames("Texture", "texture", null)
			});
		}

		public override object CreateInstance()
		{
			return new IconButton();
		}

		public override void Deserialize(object obj)
		{
			base.Deserialize(obj);
			IconButton iconButton = (IconButton)obj;
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(Texture_UxmlAttributeFlags))
			{
				iconButton.Texture = Texture;
			}
		}
	}

	private StyleBackground texture;

	private Icon icon;

	[UxmlAttribute]
	public StyleBackground Texture
	{
		get
		{
			return texture;
		}
		set
		{
			if (!(texture == value))
			{
				texture = value;
				Update();
			}
		}
	}

	public IconButton()
	{
		RegisterCallback<AttachToPanelEvent>(OnAttachToPanel, TrickleDown.TrickleDown);
	}

	private void OnAttachToPanel(AttachToPanelEvent e)
	{
		icon = this.Query<Icon>();
		Update();
	}

	private void Update()
	{
		if (icon != null)
		{
			icon.Texture = Texture;
		}
	}
}
