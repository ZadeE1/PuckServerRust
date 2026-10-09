using UnityEngine.UIElements;

public class UIPlayerMenu : UIView
{
	private VisualElement playerMenu;

	private Button identityButton;

	private Button appearanceButton;

	private Button backButton;

	public void Initialize(VisualElement rootVisualElement)
	{
		View = rootVisualElement.Query<VisualElement>("PlayerMenuView");
		playerMenu = View.Query<VisualElement>("PlayerMenu");
		identityButton = playerMenu.Query<Button>("IdentityButton");
		identityButton.clicked += OnClickIdentity;
		appearanceButton = playerMenu.Query<Button>("AppearanceButton");
		appearanceButton.clicked += OnClickAppearance;
		backButton = playerMenu.Query<Button>("BackButton");
		backButton.clicked += OnClickBack;
	}

	public override bool Show()
	{
		bool flag = base.Show();
		if (flag)
		{
			EventManager.TriggerEvent("Event_OnPlayerMenuShow");
		}
		return flag;
	}

	public override bool Hide()
	{
		bool flag = base.Hide();
		if (flag)
		{
			EventManager.TriggerEvent("Event_OnPlayerMenuHide");
		}
		return flag;
	}

	private void OnClickIdentity()
	{
		EventManager.TriggerEvent("Event_OnPlayerMenuClickIdentity");
	}

	private void OnClickAppearance()
	{
		EventManager.TriggerEvent("Event_OnPlayerMenuClickAppearance");
	}

	private void OnClickBack()
	{
		EventManager.TriggerEvent("Event_OnPlayerMenuClickBack");
	}
}
