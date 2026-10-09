using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIToastManager : UIView
{
	[Header("References")]
	[SerializeField]
	public VisualTreeAsset toastAsset;

	private Dictionary<string, Toast> nameToastMap = new Dictionary<string, Toast>();

	private VisualElement toasts;

	public void Initialize(VisualElement rootVisualElement)
	{
		View = rootVisualElement.Query<VisualElement>("ToastsView");
		toasts = View.Query<VisualElement>("Toasts");
		toasts.Clear();
	}

	public void ShowToast(string name, string content, float hideDelay = 3f)
	{
		if (nameToastMap.ContainsKey(name))
		{
			HideToast(name);
		}
		VisualElement visualElement = toastAsset.Instantiate();
		Toast toast = new Toast(this, visualElement, name, content.ToUpper(), hideDelay);
		toasts.Add(toast.VisualElement);
		nameToastMap.Add(name, toast);
	}

	public void HideToast(string name)
	{
		if (nameToastMap.ContainsKey(name))
		{
			toasts.Remove(nameToastMap[name].VisualElement);
			nameToastMap[name].Dispose();
			nameToastMap.Remove(name);
		}
	}
}
