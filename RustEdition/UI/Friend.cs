using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI;

[UxmlElement]
public class Friend : VisualElement
{
	[Serializable]
	[CompilerGenerated]
	public new class UxmlSerializedData : VisualElement.UxmlSerializedData
	{
		[SerializeField]
		private Texture2D Texture;

		[SerializeField]
		private string Username;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags Texture_UxmlAttributeFlags;

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
				new UxmlAttributeNames("Texture", "texture", null)
			});
		}

		public override object CreateInstance()
		{
			return new Friend();
		}

		public override void Deserialize(object obj)
		{
			base.Deserialize(obj);
			Friend friend = (Friend)obj;
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(Username_UxmlAttributeFlags))
			{
				friend.Username = Username;
			}
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(Texture_UxmlAttributeFlags))
			{
				friend.Texture = Texture;
			}
		}
	}

	private string username;

	private Texture2D texture;

	public Action InviteButtonClicked;

	private IconButton inviteIconButton;

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
	public Texture2D Texture
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

	public User User { get; private set; }

	public Friend()
	{
		RegisterCallback<AttachToPanelEvent>(OnAttachToPanel, TrickleDown.TrickleDown);
	}

	private void OnAttachToPanel(AttachToPanelEvent e)
	{
		User = this.Query<User>("User");
		inviteIconButton = this.Query<TemplateContainer>("InviteIconButtonContainer").First().Query<IconButton>();
		inviteIconButton.clicked += OnClickInviteButton;
		Update();
	}

	private void OnClickInviteButton()
	{
		InviteButtonClicked?.Invoke();
	}

	private void Update()
	{
		if (User != null)
		{
			User.AvatarTexture = Texture;
			User.Username = Username;
		}
	}
}
