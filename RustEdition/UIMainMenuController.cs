using System.Collections.Generic;

public class UIMainMenuController : UIViewController<UIMainMenu>
{
	private UIMainMenu uiMainMenu;

	public override void Awake()
	{
		base.Awake();
		uiMainMenu = GetComponent<UIMainMenu>();
		EventManager.AddEventListener("Event_OnDebugChanged", Event_OnDebugChanged);
	}

	private void Start()
	{
		if (SettingsManager.Debug != DebugMode.Off)
		{
			uiMainMenu.ShowDebug();
		}
		else
		{
			uiMainMenu.HideDebug();
		}
	}

	public override void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_OnDebugChanged", Event_OnDebugChanged);
		base.OnDestroy();
	}

	private void Event_OnDebugChanged(Dictionary<string, object> message)
	{
		if ((DebugMode)message["value"] != DebugMode.Off)
		{
			uiMainMenu.ShowDebug();
		}
		else
		{
			uiMainMenu.HideDebug();
		}
	}
}
