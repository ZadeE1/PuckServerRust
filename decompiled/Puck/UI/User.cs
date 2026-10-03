using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI;

[UxmlElement]
public class User : VisualElement
{
	[Serializable]
	[CompilerGenerated]
	public new class UxmlSerializedData : VisualElement.UxmlSerializedData
	{
		[SerializeField]
		private Texture2D AvatarTexture;

		[SerializeField]
		private string Username;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags AvatarTexture_UxmlAttributeFlags;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags Username_UxmlAttributeFlags;

		[RegisterUxmlCache]
		[Conditional("UNITY_EDITOR")]
		public new static void Register()
		{
			UxmlDescriptionCache.RegisterType(typeof(UxmlSerializedData), new UxmlAttributeNames[2]
			{
				new UxmlAttributeNames("Username", "username", null),
				new UxmlAttributeNames("AvatarTexture", "avatar-texture", null)
			});
		}

		public override object CreateInstance()
		{
			return new User();
		}

		public override void Deserialize(object obj)
		{
			base.Deserialize(obj);
			User user = (User)obj;
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(Username_UxmlAttributeFlags))
			{
				user.Username = Username;
			}
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(AvatarTexture_UxmlAttributeFlags))
			{
				user.AvatarTexture = AvatarTexture;
			}
		}
	}

	private string username;

	private Texture2D avatarTexture;

	private Icon avatarIcon;

	private Label usernameLabel;

	[UxmlAttribute]
	public string Username
	{
		get
		{
			return username;
		}
		set
		{
			if (!(username == value))
			{
				username = value;
				Update();
			}
		}
	}

	[UxmlAttribute]
	public Texture2D AvatarTexture
	{
		get
		{
			return avatarTexture;
		}
		set
		{
			if (!(avatarTexture == value))
			{
				avatarTexture = value;
				Update();
			}
		}
	}

	public User()
	{
		RegisterCallback<AttachToPanelEvent>(OnAttachToPanel, TrickleDown.TrickleDown);
	}

	private void OnAttachToPanel(AttachToPanelEvent e)
	{
		avatarIcon = this.Query<TemplateContainer>("AvatarIconContainer").First().Query<Icon>();
		usernameLabel = this.Query<Label>("UsernameLabel");
		Update();
	}

	private void Update()
	{
		if (usernameLabel != null)
		{
			usernameLabel.text = Username;
		}
		if (avatarIcon != null)
		{
			avatarIcon.Texture = AvatarTexture;
		}
	}
}
