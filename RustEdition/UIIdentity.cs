using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.UIElements;

public class UIIdentity : UIView
{
	private VisualElement identity;

	private TextField usernameTextField;

	private IntegerField numberIntegerField;

	private IconButton closeIconButton;

	private Button confirmButton;

	private string username;

	private int number;

	public void Initialize(VisualElement rootVisualElement)
	{
		View = rootVisualElement.Query<VisualElement>("IdentityView");
		identity = View.Query<VisualElement>("Identity");
		closeIconButton = identity.Query<TemplateContainer>("CloseIconButtonContainer").First().Query<IconButton>();
		closeIconButton.clicked += OnClickClose;
		confirmButton = identity.Query<Button>("ConfirmButton");
		confirmButton.clicked += OnClickConfirm;
		usernameTextField = identity.Query<VisualElement>("UsernameTextField").First().Query<TextField>();
		usernameTextField.RegisterValueChangedCallback(OnNameChanged);
		usernameTextField.RegisterCallback<FocusOutEvent>(OnNameFocusOut);
		numberIntegerField = identity.Query<VisualElement>("NumberIntegerField").First().Query<IntegerField>();
		numberIntegerField.RegisterValueChangedCallback(OnNumberChanged);
		numberIntegerField.RegisterCallback<FocusOutEvent>(OnNumberFocusOut);
	}

	public override bool Show()
	{
		bool flag = base.Show();
		if (flag)
		{
			EventManager.TriggerEvent("Event_OnIdentityShow");
		}
		return flag;
	}

	public override bool Hide()
	{
		bool flag = base.Hide();
		if (flag)
		{
			EventManager.TriggerEvent("Event_OnIdentityHide");
		}
		return flag;
	}

	public void SetIdentity(string username, int number)
	{
		this.username = username;
		usernameTextField.value = this.username;
		this.number = number;
		numberIntegerField.value = this.number;
	}

	private void OnNameChanged(ChangeEvent<string> changeEvent)
	{
		username = changeEvent.newValue;
		if (!string.IsNullOrEmpty(username))
		{
			username = StringUtils.FilterStringNotLetters(username);
			usernameTextField.value = username;
		}
	}

	private void OnNameFocusOut(FocusOutEvent focusOutEvent)
	{
		username = usernameTextField.value;
		if (!string.IsNullOrEmpty(username))
		{
			username = StringUtils.FilterStringNotLetters(username);
			username = StringUtils.FilterStringProfanity(username);
			usernameTextField.value = username;
		}
	}

	private void OnNumberChanged(ChangeEvent<int> changeEvent)
	{
		number = changeEvent.newValue;
	}

	private void OnNumberFocusOut(FocusOutEvent focusOutEvent)
	{
		number = Mathf.Clamp(numberIntegerField.value, 1, 99);
		numberIntegerField.value = number;
	}

	private void OnClickClose()
	{
		EventManager.TriggerEvent("Event_OnIdentityClickClose");
	}

	private void OnClickConfirm()
	{
		EventManager.TriggerEvent("Event_OnIdentityClickConfirm", new Dictionary<string, object>
		{
			{ "username", username },
			{ "number", number }
		});
	}
}
