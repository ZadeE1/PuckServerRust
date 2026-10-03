using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI;

[UxmlElement]
public class KeyBindField : VisualElement
{
	[Serializable]
	[CompilerGenerated]
	public new class UxmlSerializedData : VisualElement.UxmlSerializedData
	{
		[SerializeField]
		private KeyBindInteractionType InteractionType;

		[SerializeField]
		private string Label;

		[SerializeField]
		private string Path;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags InteractionType_UxmlAttributeFlags;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags Label_UxmlAttributeFlags;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags Path_UxmlAttributeFlags;

		[RegisterUxmlCache]
		[Conditional("UNITY_EDITOR")]
		public new static void Register()
		{
			UxmlDescriptionCache.RegisterType(typeof(UxmlSerializedData), new UxmlAttributeNames[3]
			{
				new UxmlAttributeNames("InteractionType", "interaction-type", null),
				new UxmlAttributeNames("Label", "label", null),
				new UxmlAttributeNames("Path", "path", null)
			});
		}

		public override object CreateInstance()
		{
			return new KeyBindField();
		}

		public override void Deserialize(object obj)
		{
			base.Deserialize(obj);
			KeyBindField keyBindField = (KeyBindField)obj;
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(InteractionType_UxmlAttributeFlags))
			{
				keyBindField.InteractionType = InteractionType;
			}
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(Label_UxmlAttributeFlags))
			{
				keyBindField.Label = Label;
			}
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(Path_UxmlAttributeFlags))
			{
				keyBindField.Path = Path;
			}
		}
	}

	private KeyBindInteractionType interactionType;

	private string label;

	private string path;

	private KeyBindInteraction interaction;

	public Action Click;

	public Action<KeyBindInteraction> InteractionChange;

	private Label nameLabel;

	private TextField pathTextField;

	private DropdownField interactionDropdownField;

	[UxmlAttribute]
	public KeyBindInteractionType InteractionType
	{
		get
		{
			return interactionType;
		}
		set
		{
			if (interactionType != value)
			{
				interactionType = value;
				OnInteractionTypeChanged();
			}
		}
	}

	[UxmlAttribute]
	public string Label
	{
		get
		{
			return label;
		}
		set
		{
			if (!(label == value))
			{
				label = value;
				OnLabelChanged();
			}
		}
	}

	[UxmlAttribute]
	public string Path
	{
		get
		{
			return path;
		}
		set
		{
			if (!(path == value))
			{
				path = value;
				OnPathChanged();
			}
		}
	}

	public KeyBindInteraction Interaction
	{
		get
		{
			return interaction;
		}
		set
		{
			if (interaction != value)
			{
				interaction = value;
				OnInteractionChanged();
			}
		}
	}

	public KeyBindField()
	{
		RegisterCallback<AttachToPanelEvent>(OnAttachToPanel, TrickleDown.TrickleDown);
	}

	private void OnAttachToPanel(AttachToPanelEvent e)
	{
		nameLabel = this.Query<Label>("NameLabel").First();
		pathTextField = this.Query<TextField>("PathTextField").First();
		pathTextField.RegisterCallback<ClickEvent>(OnPathTextFieldClicked);
		interactionDropdownField = this.Query<DropdownField>("InteractionDropdownField").First();
		interactionDropdownField.RegisterValueChangedCallback(OnInteractionDropdownFieldValueChanged);
		OnLabelChanged();
		OnPathChanged();
		OnInteractionTypeChanged();
	}

	private void OnInteractionTypeChanged()
	{
		if (interactionDropdownField != null)
		{
			switch (InteractionType)
			{
			case KeyBindInteractionType.Press:
				interactionDropdownField.choices = new List<string> { "PRESS", "RELEASE", "DOUBLE PRESS", "HOLD" };
				break;
			case KeyBindInteractionType.Hold:
				interactionDropdownField.choices = new List<string> { "CONTINUOUS", "TOGGLE" };
				break;
			}
			OnInteractionChanged();
		}
	}

	private void OnLabelChanged()
	{
		if (nameLabel != null)
		{
			nameLabel.text = label;
		}
	}

	private void OnPathChanged()
	{
		if (pathTextField != null)
		{
			pathTextField.value = path;
		}
	}

	private void OnInteractionChanged()
	{
		if (interactionDropdownField != null)
		{
			string valueWithoutNotify = interaction switch
			{
				KeyBindInteraction.Release => "RELEASE", 
				KeyBindInteraction.DoublePress => "DOUBLE PRESS", 
				KeyBindInteraction.Hold => "HOLD", 
				KeyBindInteraction.Toggle => "TOGGLE", 
				_ => (InteractionType == KeyBindInteractionType.Press) ? "PRESS" : "CONTINUOUS", 
			};
			interactionDropdownField.SetValueWithoutNotify(valueWithoutNotify);
		}
	}

	private void OnPathTextFieldClicked(ClickEvent clickEvent)
	{
		Click?.Invoke();
	}

	private void OnInteractionDropdownFieldValueChanged(ChangeEvent<string> changeEvent)
	{
		switch (changeEvent.newValue)
		{
		case "RELEASE":
			interaction = KeyBindInteraction.Release;
			break;
		case "DOUBLE PRESS":
			interaction = KeyBindInteraction.DoublePress;
			break;
		case "HOLD":
			interaction = KeyBindInteraction.Hold;
			break;
		case "TOGGLE":
			interaction = KeyBindInteraction.Toggle;
			break;
		default:
			interaction = ((InteractionType != KeyBindInteractionType.Press) ? KeyBindInteraction.Continuous : KeyBindInteraction.Press);
			break;
		}
		InteractionChange?.Invoke(interaction);
	}
}
