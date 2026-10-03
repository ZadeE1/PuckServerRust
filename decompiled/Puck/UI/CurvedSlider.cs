using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI;

[UxmlElement]
public class CurvedSlider : Slider
{
	[Serializable]
	[CompilerGenerated]
	public new class UxmlSerializedData : Slider.UxmlSerializedData
	{
		[SerializeField]
		private int KneeValue;

		[SerializeField]
		private float KneePosition;

		[SerializeField]
		private int Ceiling;

		[SerializeField]
		private string ValueLabel;

		[SerializeField]
		private string CeilingLabel;

		[SerializeField]
		private string Unit;

		[SerializeField]
		private bool TrackGradient;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags KneeValue_UxmlAttributeFlags;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags KneePosition_UxmlAttributeFlags;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags Ceiling_UxmlAttributeFlags;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags ValueLabel_UxmlAttributeFlags;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags CeilingLabel_UxmlAttributeFlags;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags Unit_UxmlAttributeFlags;

		[SerializeField]
		[UxmlIgnore]
		[HideInInspector]
		private UxmlAttributeFlags TrackGradient_UxmlAttributeFlags;

		[RegisterUxmlCache]
		[Conditional("UNITY_EDITOR")]
		public new static void Register()
		{
			UxmlDescriptionCache.RegisterType(typeof(UxmlSerializedData), new UxmlAttributeNames[7]
			{
				new UxmlAttributeNames("KneeValue", "knee-value", null),
				new UxmlAttributeNames("KneePosition", "knee-position", null),
				new UxmlAttributeNames("Ceiling", "ceiling", null),
				new UxmlAttributeNames("ValueLabel", "value-label", null),
				new UxmlAttributeNames("CeilingLabel", "ceiling-label", null),
				new UxmlAttributeNames("Unit", "unit", null),
				new UxmlAttributeNames("TrackGradient", "track-gradient", null)
			});
		}

		public override object CreateInstance()
		{
			return new CurvedSlider();
		}

		public override void Deserialize(object obj)
		{
			base.Deserialize(obj);
			CurvedSlider curvedSlider = (CurvedSlider)obj;
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(KneeValue_UxmlAttributeFlags))
			{
				curvedSlider.KneeValue = KneeValue;
			}
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(KneePosition_UxmlAttributeFlags))
			{
				curvedSlider.KneePosition = KneePosition;
			}
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(Ceiling_UxmlAttributeFlags))
			{
				curvedSlider.Ceiling = Ceiling;
			}
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(ValueLabel_UxmlAttributeFlags))
			{
				curvedSlider.ValueLabel = ValueLabel;
			}
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(CeilingLabel_UxmlAttributeFlags))
			{
				curvedSlider.CeilingLabel = CeilingLabel;
			}
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(Unit_UxmlAttributeFlags))
			{
				curvedSlider.Unit = Unit;
			}
			if (UnityEngine.UIElements.UxmlSerializedData.ShouldWriteAttributeValue(TrackGradient_UxmlAttributeFlags))
			{
				curvedSlider.TrackGradient = TrackGradient;
			}
		}
	}

	private readonly TextField inputField;

	private Texture2D gradientTexture;

	private int kneeValue = 150;

	private float kneePosition = 0.65f;

	private int ceiling = 100;

	private string valueLabel = "";

	private string ceilingLabel = "";

	private string unit = "";

	private bool trackGradient;

	public Action<int> MappedValueChanged;

	[UxmlAttribute]
	public int KneeValue
	{
		get
		{
			return kneeValue;
		}
		set
		{
			kneeValue = value;
			UpdateDisplay();
		}
	}

	[UxmlAttribute]
	public float KneePosition
	{
		get
		{
			return kneePosition;
		}
		set
		{
			kneePosition = Mathf.Clamp(value, 0.01f, 0.99f);
			UpdateDisplay();
		}
	}

	[UxmlAttribute]
	public int Ceiling
	{
		get
		{
			return ceiling;
		}
		set
		{
			ceiling = value;
			UpdateDisplay();
		}
	}

	[UxmlAttribute]
	public string ValueLabel
	{
		get
		{
			return valueLabel;
		}
		set
		{
			valueLabel = value;
			UpdateDisplay();
		}
	}

	[UxmlAttribute]
	public string CeilingLabel
	{
		get
		{
			return ceilingLabel;
		}
		set
		{
			ceilingLabel = value;
			UpdateDisplay();
		}
	}

	[UxmlAttribute]
	public string Unit
	{
		get
		{
			return unit;
		}
		set
		{
			unit = value;
			UpdateDisplay();
		}
	}

	[UxmlAttribute]
	public bool TrackGradient
	{
		get
		{
			return trackGradient;
		}
		set
		{
			trackGradient = value;
			ApplyTrackGradient();
		}
	}

	public int MappedValue
	{
		get
		{
			return SliderToValue(value);
		}
		set
		{
			this.value = ValueToSlider(value);
		}
	}

	public CurvedSlider()
	{
		lowValue = 0f;
		highValue = 1f;
		showInputField = false;
		inputField = new TextField
		{
			isDelayed = true
		};
		inputField.RegisterValueChangedCallback(OnInputFieldChanged);
		this.Q(null, "unity-base-field__input").Add(inputField);
		this.RegisterValueChangedCallback(OnValueChanged);
	}

	public void SetMappedValueWithoutNotify(int mappedValue)
	{
		SetValueWithoutNotify(ValueToSlider(mappedValue));
		UpdateDisplay();
	}

	private int SliderToValue(float sliderValue)
	{
		if (sliderValue >= 1f)
		{
			if (!string.IsNullOrEmpty(ceilingLabel))
			{
				return int.MaxValue;
			}
			return ceiling;
		}
		if (sliderValue <= kneePosition)
		{
			return Mathf.RoundToInt((float)kneeValue * (sliderValue / kneePosition));
		}
		float num = (sliderValue - kneePosition) / (1f - kneePosition);
		return Mathf.RoundToInt((float)kneeValue + (float)(ceiling - kneeValue) * num);
	}

	private float ValueToSlider(int mappedValue)
	{
		if (mappedValue >= ceiling)
		{
			return 1f;
		}
		if (mappedValue <= kneeValue)
		{
			return kneePosition * ((float)mappedValue / (float)kneeValue);
		}
		float num = (float)(mappedValue - kneeValue) / (float)(ceiling - kneeValue);
		return kneePosition + (1f - kneePosition) * num;
	}

	private void ApplyTrackGradient()
	{
		VisualElement visualElement = this.Q(null, "unity-base-slider__drag-container");
		if (visualElement == null)
		{
			return;
		}
		if (!trackGradient)
		{
			visualElement.style.backgroundImage = StyleKeyword.Null;
			return;
		}
		if ((object)gradientTexture == null)
		{
			gradientTexture = BuildGradientTexture();
		}
		visualElement.style.backgroundImage = Background.FromTexture2D(gradientTexture);
	}

	private static Texture2D BuildGradientTexture()
	{
		Color a = new Color(0.33f, 0.5f, 0.33f);
		Color color = new Color(0.55f, 0.5f, 0.28f);
		Color b = new Color(0.55f, 0.3f, 0.3f);
		Texture2D texture2D = new Texture2D(256, 1, TextureFormat.RGBA32, mipChain: false)
		{
			name = "CurvedSliderTrackGradient",
			wrapMode = TextureWrapMode.Clamp,
			filterMode = FilterMode.Bilinear,
			hideFlags = HideFlags.HideAndDontSave
		};
		for (int i = 0; i < 256; i++)
		{
			float num = (float)i / 255f;
			Color color2 = ((num < 0.5f) ? Color.Lerp(a, color, num * 2f) : Color.Lerp(color, b, (num - 0.5f) * 2f));
			texture2D.SetPixel(i, 0, color2);
		}
		texture2D.Apply();
		return texture2D;
	}

	private void UpdateDisplay()
	{
		label = valueLabel;
		int num = SliderToValue(value);
		string valueWithoutNotify = ((num == int.MaxValue) ? ceilingLabel : $"{num}{unit}");
		inputField.SetValueWithoutNotify(valueWithoutNotify);
	}

	private void OnValueChanged(ChangeEvent<float> changeEvent)
	{
		UpdateDisplay();
		MappedValueChanged?.Invoke(SliderToValue(changeEvent.newValue));
	}

	private void OnInputFieldChanged(ChangeEvent<string> changeEvent)
	{
		string text = changeEvent.newValue.Trim();
		int result;
		if (text.Length == 0 || (!string.IsNullOrEmpty(ceilingLabel) && string.Equals(text, ceilingLabel, StringComparison.OrdinalIgnoreCase)))
		{
			MappedValue = ceiling;
		}
		else if (int.TryParse(new string(text.Where(char.IsDigit).ToArray()), out result))
		{
			MappedValue = Mathf.Clamp(result, 0, ceiling);
		}
		UpdateDisplay();
	}
}
