using UnityEngine.UIElements;

public class PopupMissingPasswordContent : BasePopupContent
{
	private string password = string.Empty;

	private TextField textField;

	public string Password
	{
		get
		{
			return password;
		}
		set
		{
			if (!(password == value))
			{
				password = value;
				Update();
			}
		}
	}

	public PopupMissingPasswordContent(VisualTreeAsset asset)
		: base(asset)
	{
	}

	public override void Initialize()
	{
		base.Initialize();
		textField = VisualElement.Query<VisualElement>("PasswordTextField").First().Query<TextField>();
		textField.value = Password;
		textField.RegisterCallback<ChangeEvent<string>>(OnPasswordChanged);
		Update();
	}

	public override void Dispose()
	{
		base.Dispose();
		textField.UnregisterCallback<ChangeEvent<string>>(OnPasswordChanged);
	}

	internal override void Update()
	{
		base.Update();
		textField.value = Password;
	}

	private void OnPasswordChanged(ChangeEvent<string> changeEvent)
	{
		Password = changeEvent.newValue;
	}
}
