using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.UIElements;

public class UIDirectConnect : UIView
{
	private VisualElement directConnect;

	private IconButton closeIconButton;

	private Button connectButton;

	private TextField ipAddressTextField;

	private IntegerField portIntegerField;

	private string ipAddress = string.Empty;

	private int port = 30609;

	public void Initialize(VisualElement rootVisualElement)
	{
		View = rootVisualElement.Query<VisualElement>("DirectConnectView");
		directConnect = View.Query<VisualElement>("DirectConnect");
		closeIconButton = directConnect.Query<TemplateContainer>("CloseIconButtonContainer").First().Query<IconButton>();
		closeIconButton.clicked += OnClickClose;
		connectButton = directConnect.Query<Button>("ConnectButton");
		connectButton.clicked += OnClickConnect;
		ipAddressTextField = directConnect.Query<VisualElement>("IpAddressTextFieldInput").First().Query<TextField>();
		ipAddressTextField.RegisterValueChangedCallback(OnIpAddressChanged);
		ipAddressTextField.value = ipAddress;
		portIntegerField = directConnect.Query<VisualElement>("PortIntegerFieldInput").First().Query<IntegerField>();
		portIntegerField.RegisterValueChangedCallback(OnPortChanged);
		portIntegerField.value = port;
		RefreshConnectButton();
	}

	private void RefreshConnectButton()
	{
		connectButton.SetEnabled(!string.IsNullOrWhiteSpace(ipAddress));
	}

	private void OnClickClose()
	{
		EventManager.TriggerEvent("Event_OnDirectConnectClickClose");
	}

	private void OnClickConnect()
	{
		if (!string.IsNullOrWhiteSpace(ipAddress))
		{
			EventManager.TriggerEvent("Event_OnDirectConnectClickConnect", new Dictionary<string, object>
			{
				{
					"ipAddress",
					ipAddress.Trim()
				},
				{ "port", port }
			});
		}
	}

	private void OnIpAddressChanged(ChangeEvent<string> changeEvent)
	{
		ipAddress = changeEvent.newValue;
		RefreshConnectButton();
	}

	private void OnPortChanged(ChangeEvent<int> changeEvent)
	{
		port = Mathf.Clamp(changeEvent.newValue, 1, 65535);
		portIntegerField.SetValueWithoutNotify(port);
	}
}
