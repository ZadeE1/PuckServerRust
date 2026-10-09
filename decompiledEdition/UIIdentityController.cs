using System.Collections.Generic;

public class UIIdentityController : UIViewController<UIIdentity>
{
	private UIIdentity uiIdentity;

	public override void Awake()
	{
		base.Awake();
		uiIdentity = GetComponent<UIIdentity>();
		EventManager.AddEventListener("Event_OnPlayerDataChanged", Event_OnPlayerDataChanged);
	}

	public override void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_OnPlayerDataChanged", Event_OnPlayerDataChanged);
		base.OnDestroy();
	}

	private void Event_OnPlayerDataChanged(Dictionary<string, object> message)
	{
		PlayerData playerData = (PlayerData)message["newPlayerData"];
		if (playerData != null)
		{
			uiIdentity.SetIdentity(playerData.username, playerData.number);
		}
	}
}
