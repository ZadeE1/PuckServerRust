using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI;

[UxmlElement]
public class Mod : VisualElement
{
	[Serializable]
	[CompilerGenerated]
	public new class UxmlSerializedData : VisualElement.UxmlSerializedData
	{
		[SerializeField]
		private string Description;

		[SerializeField]
		private StyleBackground PreviewTexture;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags Description_UxmlAttributeFlags;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags PreviewTexture_UxmlAttributeFlags;

		[RegisterUxmlCache]
		[Conditional("UNITY_EDITOR")]
		public new static void Register()
		{
			UxmlDescriptionCache.RegisterType(typeof(UxmlSerializedData), new UxmlAttributeNames[2]
			{
				new UxmlAttributeNames("Description", "description", null),
				new UxmlAttributeNames("PreviewTexture", "preview-texture", null)
			});
		}

		public override object CreateInstance()
		{
			return new Mod();
		}

		public override void Deserialize(object obj)
		{
			base.Deserialize(obj);
			Mod mod = (Mod)obj;
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(Description_UxmlAttributeFlags))
			{
				mod.Description = Description;
			}
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(PreviewTexture_UxmlAttributeFlags))
			{
				mod.PreviewTexture = PreviewTexture;
			}
		}
	}

	private string description;

	private StyleBackground previewTexture;

	private bool hasFiles = true;

	public Action Ready;

	private VisualElement previewVisualElement;

	private Label descriptionLabel;

	private Label statusLabel;

	private Toggle toggle;

	private ModPreview modPreview;

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

	[UxmlAttribute]
	public StyleBackground PreviewTexture
	{
		get
		{
			return previewTexture;
		}
		set
		{
			if (!(previewTexture == value))
			{
				previewTexture = value;
				Update();
			}
		}
	}

	public bool HasFiles
	{
		get
		{
			return hasFiles;
		}
		set
		{
			if (hasFiles != value)
			{
				hasFiles = value;
				Refresh();
			}
		}
	}

	public Toggle Toggle => toggle;

	public ModPreview ModPreview => modPreview;

	public Mod()
	{
		RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
		RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
	}

	private void OnAttachToPanel(AttachToPanelEvent e)
	{
		descriptionLabel = this.Query<Label>("DescriptionLabel");
		previewVisualElement = this.Query<VisualElement>("Preview");
		statusLabel = this.Query<Label>("Status");
		toggle = this.Query<Toggle>();
		toggle.RegisterValueChangedCallback(OnToggleValueChanged);
		Refresh();
		modPreview = this.Query<TemplateContainer>("ModPreviewContainer").First().Query<ModPreview>();
		Update();
		Utils.WhenAllActions(() =>
		{
			Ready?.Invoke();
		}, (Action a) =>
		{
			ModPreview modPreview = this.modPreview;
			modPreview.Ready = (Action)Delegate.Combine(modPreview.Ready, a);
		});
	}

	private void OnDetachFromPanel(DetachFromPanelEvent e)
	{
		if (toggle != null)
		{
			toggle.UnregisterValueChangedCallback(OnToggleValueChanged);
		}
	}

	private void Update()
	{
		if (descriptionLabel != null)
		{
			descriptionLabel.text = Description;
		}
		if (previewVisualElement != null)
		{
			previewVisualElement.style.backgroundImage = PreviewTexture;
		}
	}

	public void Refresh()
	{
		if (toggle != null)
		{
			bool value = toggle.value;
			bool enable = !hasFiles || (toggle.enabledSelf && !value);
			toggle.label = (value ? "ENABLED" : "DISABLED");
			toggle.EnableInClassList("enabled", value);
			EnableInClassList("disabled", enable);
			if (statusLabel != null)
			{
				statusLabel.style.display = (hasFiles ? DisplayStyle.None : DisplayStyle.Flex);
			}
		}
	}

	private void OnToggleValueChanged(ChangeEvent<bool> e)
	{
		Refresh();
	}
}
