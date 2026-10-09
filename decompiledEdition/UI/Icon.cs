using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI;

[UxmlElement]
public class Icon : VisualElement
{
	[Serializable]
	[CompilerGenerated]
	public new class UxmlSerializedData : VisualElement.UxmlSerializedData
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
			return new Icon();
		}

		public override void Deserialize(object obj)
		{
			base.Deserialize(obj);
			Icon icon = (Icon)obj;
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(Texture_UxmlAttributeFlags))
			{
				icon.Texture = Texture;
			}
		}
	}

	private StyleBackground texture;

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

	public Icon()
	{
		RegisterCallback<AttachToPanelEvent>(OnAttachToPanel, TrickleDown.TrickleDown);
	}

	private void OnAttachToPanel(AttachToPanelEvent e)
	{
		Update();
	}

	private void Update()
	{
		style.backgroundImage = Texture;
	}
}
