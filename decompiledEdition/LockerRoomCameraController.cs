using System.Collections.Generic;

public class LockerRoomCameraController : BaseCameraController
{
	private LockerRoomCamera lockerRoomCamera;

	public override void Awake()
	{
		base.Awake();
		lockerRoomCamera = GetComponent<LockerRoomCamera>();
		EventManager.AddEventListener("Event_OnMainMenuShow", Event_OnMainMenuShow);
		EventManager.AddEventListener("Event_OnPlayerMenuShow", Event_OnPlayerMenuShow);
		EventManager.AddEventListener("Event_OnAppearanceShow", Event_OnAppearanceShow);
		EventManager.AddEventListener("Event_OnAppearanceCategoryChanged", Event_OnAppearanceCategoryChanged);
	}

	public override void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_OnMainMenuShow", Event_OnMainMenuShow);
		EventManager.RemoveEventListener("Event_OnPlayerMenuShow", Event_OnPlayerMenuShow);
		EventManager.RemoveEventListener("Event_OnAppearanceShow", Event_OnAppearanceShow);
		EventManager.RemoveEventListener("Event_OnAppearanceCategoryChanged", Event_OnAppearanceCategoryChanged);
		base.OnDestroy();
	}

	private void SetAppearancePosition(AppearanceCategory category, AppearanceSubcategory subcategory)
	{
		switch (category)
		{
		case AppearanceCategory.Head:
			lockerRoomCamera.SetPosition("headCloseUp");
			break;
		case AppearanceCategory.Stick:
			lockerRoomCamera.SetPosition("stickCloseUp");
			break;
		default:
			lockerRoomCamera.SetPosition("bodyCloseUp");
			break;
		}
	}

	private void Event_OnMainMenuShow(Dictionary<string, object> message)
	{
		lockerRoomCamera.SetPosition("default");
	}

	private void Event_OnPlayerMenuShow(Dictionary<string, object> message)
	{
		lockerRoomCamera.SetPosition("bodyCloseUp");
	}

	private void Event_OnAppearanceShow(Dictionary<string, object> message)
	{
		AppearanceCategory category = (AppearanceCategory)message["category"];
		AppearanceSubcategory subcategory = (AppearanceSubcategory)message["subcategory"];
		SetAppearancePosition(category, subcategory);
	}

	private void Event_OnAppearanceCategoryChanged(Dictionary<string, object> message)
	{
		AppearanceCategory category = (AppearanceCategory)message["category"];
		AppearanceSubcategory subcategory = (AppearanceSubcategory)message["subcategory"];
		SetAppearancePosition(category, subcategory);
	}
}
