using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI;

[UxmlElement]
public class Spinner : VisualElement
{
	[Serializable]
	[CompilerGenerated]
	public new class UxmlSerializedData : VisualElement.UxmlSerializedData
	{
		[SerializeField]
		private float speed;

		[SerializeField]
		private SpinnerDirection direction;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags speed_UxmlAttributeFlags;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags direction_UxmlAttributeFlags;

		[RegisterUxmlCache]
		[Conditional("UNITY_EDITOR")]
		public new static void Register()
		{
			UxmlDescriptionCache.RegisterType(typeof(UxmlSerializedData), new UxmlAttributeNames[2]
			{
				new UxmlAttributeNames("speed", "speed", null),
				new UxmlAttributeNames("direction", "direction", null)
			});
		}

		public override object CreateInstance()
		{
			return new Spinner();
		}

		public override void Deserialize(object obj)
		{
			base.Deserialize(obj);
			Spinner spinner = (Spinner)obj;
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(speed_UxmlAttributeFlags))
			{
				spinner.speed = speed;
			}
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(direction_UxmlAttributeFlags))
			{
				spinner.direction = direction;
			}
		}
	}

	private IVisualElementScheduledItem scheduledItem;

	[UxmlAttribute]
	public float speed { get; set; }

	[UxmlAttribute]
	public SpinnerDirection direction { get; set; }

	public Spinner()
	{
		RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
		RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
	}

	private void OnAttachToPanel(AttachToPanelEvent e)
	{
		scheduledItem = schedule.Execute(OnScheduleUpdate).Every(16L);
	}

	private void OnDetachFromPanel(DetachFromPanelEvent e)
	{
		scheduledItem.Pause();
	}

	private void OnScheduleUpdate()
	{
		style.rotate = new Rotate(style.rotate.value.angle.value + speed * (float)((direction == SpinnerDirection.Clockwise) ? 1 : (-1)));
	}
}
