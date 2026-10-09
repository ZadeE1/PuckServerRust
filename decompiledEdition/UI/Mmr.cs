using System;
using System.Runtime.CompilerServices;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI;

[UxmlElement]
public class Mmr : VisualElement
{
	[Serializable]
	[CompilerGenerated]
	public new class UxmlSerializedData : VisualElement.UxmlSerializedData
	{
		public override object CreateInstance()
		{
			return new Mmr();
		}
	}

	private int? currentValue;

	private int? targetValue;

	private Tween transitionTween;

	private Label valueLabel;

	private Label remainderLabel;

	public int? CurrentValue
	{
		get
		{
			return currentValue;
		}
		set
		{
			if (currentValue != value)
			{
				currentValue = value;
				Update();
			}
		}
	}

	public int? TargetValue
	{
		get
		{
			return targetValue;
		}
		set
		{
			if (targetValue != value)
			{
				targetValue = value;
				StartCurrentValueTransition();
			}
		}
	}

	public Mmr()
	{
		RegisterCallback<AttachToPanelEvent>(OnAttachToPanel, TrickleDown.TrickleDown);
	}

	private void OnAttachToPanel(AttachToPanelEvent e)
	{
		valueLabel = this.Query<Label>("ValueLabel");
		remainderLabel = this.Query<Label>("RemainderLabel");
		Update();
	}

	private static Color HexColor(string hex)
	{
		ColorUtility.TryParseHtmlString(hex, out var color);
		return color;
	}

	private void StartCurrentValueTransition()
	{
		if (!TargetValue.HasValue)
		{
			return;
		}
		if (!CurrentValue.HasValue)
		{
			CurrentValue = TargetValue;
			return;
		}
		transitionTween?.Kill();
		transitionTween = DOVirtual.Int(CurrentValue.Value, TargetValue.Value, 3f, (int value) =>
		{
			CurrentValue = value;
		}).SetEase(Ease.OutCubic);
	}

	private void Update()
	{
		if (!CurrentValue.HasValue)
		{
			base.style.display = DisplayStyle.None;
			return;
		}
		base.style.display = DisplayStyle.Flex;
		IStyle style = base.style;
		int? num = CurrentValue;
		if (!num.HasValue)
		{
			goto IL_00d8;
		}
		int valueOrDefault = num.GetValueOrDefault();
		Color color;
		if (valueOrDefault < 1500)
		{
			if (valueOrDefault < 1000)
			{
				color = HexColor("#444751");
			}
			else
			{
				color = ((valueOrDefault >= 1250) ? HexColor("#112061") : HexColor("#2b425a"));
			}
		}
		else if (valueOrDefault < 2000)
		{
			color = ((valueOrDefault >= 1750) ? HexColor("#5a005e") : HexColor("#481a60"));
		}
		else
		{
			if (valueOrDefault >= 2250)
			{
				goto IL_00d8;
			}
			color = HexColor("#610809");
		}
		goto IL_00e3;
		IL_00e3:
		style.unityBackgroundImageTintColor = color;
		IStyle style2;
		if (valueLabel != null)
		{
			valueLabel.text = CurrentValue.ToString();
			style2 = valueLabel.style;
			num = CurrentValue;
			if (!num.HasValue)
			{
				goto IL_01c2;
			}
			valueOrDefault = num.GetValueOrDefault();
			if (valueOrDefault < 1500)
			{
				if (valueOrDefault < 1000)
				{
					color = HexColor("#cbd2e2");
				}
				else
				{
					color = ((valueOrDefault >= 1250) ? HexColor("#4364ff") : HexColor("#81b6eb"));
				}
			}
			else if (valueOrDefault < 2000)
			{
				color = ((valueOrDefault >= 1750) ? HexColor("#ea02fb") : HexColor("#c16bff"));
			}
			else
			{
				if (valueOrDefault >= 2250)
				{
					goto IL_01c2;
				}
				color = HexColor("#f93034");
			}
			goto IL_01cd;
		}
		goto IL_01d8;
		IL_01d8:
		if (remainderLabel != null)
		{
			bool flag = TargetValue > CurrentValue;
			bool flag2 = CurrentValue != TargetValue;
			remainderLabel.style.display = ((!flag2) ? DisplayStyle.None : DisplayStyle.Flex);
			remainderLabel.text = (flag ? $"+{TargetValue - CurrentValue}" : $"{TargetValue - CurrentValue}");
		}
		return;
		IL_01c2:
		color = HexColor("#f5dc0c");
		goto IL_01cd;
		IL_00d8:
		color = HexColor("#625000");
		goto IL_00e3;
		IL_01cd:
		style2.color = color;
		goto IL_01d8;
	}
}
