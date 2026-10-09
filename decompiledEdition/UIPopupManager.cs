using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIPopupManager : UIView
{
	[Header("References")]
	[SerializeField]
	private VisualTreeAsset popupAsset;

	[SerializeField]
	private VisualTreeAsset notificationContentAsset;

	[SerializeField]
	private VisualTreeAsset missingPasswordContentAsset;

	[SerializeField]
	private VisualTreeAsset missingModsContentAsset;

	[SerializeField]
	private VisualTreeAsset modPreviewAsset;

	private VisualElement popups;

	private Dictionary<string, Popup> namePopupMap = new Dictionary<string, Popup>();

	public void Initialize(VisualElement rootVisualElement)
	{
		View = rootVisualElement.Query<VisualElement>("PopupsView");
		popups = View.Query<VisualElement>("Popups");
		popups.Clear();
		UpdateFocus();
		UpdateVisibility();
	}

	public void ShowPopup(string name, string title, BasePopupContent content, bool showOkButton, bool showCloseButton, object data = null)
	{
		if (GetPopupByName(name) == null)
		{
			Popup popup = new Popup(popupAsset.Instantiate(), name, title, content, showOkButton, showCloseButton, data);
			popups.Add(popup.VisualElement);
			namePopupMap.Add(name, popup);
			popup.Initialize();
			popup.VisualElement.BringToFront();
			UpdateFocus();
			UpdateVisibility();
			EventManager.TriggerEvent("Event_OnPopupShow", new Dictionary<string, object> { { "name", name } });
		}
	}

	public void HidePopup(string name)
	{
		Popup popupByName = GetPopupByName(name);
		if (popupByName != null)
		{
			popups.Remove(popupByName.VisualElement);
			namePopupMap.Remove(name);
			popupByName.Dispose();
			UpdateFocus();
			UpdateVisibility();
			EventManager.TriggerEvent("Event_OnPopupHide", new Dictionary<string, object> { { "name", name } });
		}
	}

	public Popup GetPopupByName(string name)
	{
		if (namePopupMap.ContainsKey(name))
		{
			return namePopupMap[name];
		}
		return null;
	}

	private void UpdateFocus()
	{
		IsFocused = namePopupMap.Count > 0;
	}

	private void UpdateVisibility()
	{
		popups.style.display = ((namePopupMap.Count <= 0) ? DisplayStyle.None : DisplayStyle.Flex);
	}

	public PopupNotificationContent CreateNotificationContent(string text)
	{
		return new PopupNotificationContent(notificationContentAsset, text);
	}

	public PopupMissingPasswordContent CreateMissingPasswordContent()
	{
		return new PopupMissingPasswordContent(missingPasswordContentAsset);
	}

	public PopupMissingModsPopupContent CreateMissingModsContent(string text, string notice, string[] missingModIds)
	{
		return new PopupMissingModsPopupContent(missingModsContentAsset, modPreviewAsset, text, notice, missingModIds);
	}
}
