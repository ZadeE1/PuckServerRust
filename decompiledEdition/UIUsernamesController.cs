using System.Collections.Generic;

public class UIUsernamesController : UIViewController<UIUsernames>
{
	private static readonly Logger Logger = new Logger("UIUsernamesController");

	private UIUsernames uiUsernames;

	public override void Awake()
	{
		base.Awake();
		uiUsernames = GetComponent<UIUsernames>();
		EventManager.AddEventListener("Event_Everyone_OnLevelSpawned", Event_Everyone_OnLevelSpawned);
		EventManager.AddEventListener("Event_Everyone_OnPlayerBodySpawned", Event_Everyone_OnPlayerBodySpawned);
		EventManager.AddEventListener("Event_Everyone_OnPlayerBodyDespawned", Event_Everyone_OnPlayerBodyDespawned);
		EventManager.AddEventListener("Event_Everyone_OnPlayerUsernameChanged", Event_Everyone_OnPlayerUsernameChanged);
		EventManager.AddEventListener("Event_Everyone_OnPlayerNumberChanged", Event_Everyone_OnPlayerNumberChanged);
		EventManager.AddEventListener("Event_OnShowPlayerUsernamesChanged", Event_OnShowPlayerUsernamesChanged);
		EventManager.AddEventListener("Event_OnPlayerUsernamesFadeThresholdChanged", Event_OnPlayerUsernamesFadeThresholdChanged);
	}

	private void Start()
	{
		uiUsernames.FadeThreshold = SettingsManager.PlayerUsernamesFadeThreshold;
	}

	public override void OnDestroy()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnLevelSpawned", Event_Everyone_OnLevelSpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerBodySpawned", Event_Everyone_OnPlayerBodySpawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerBodyDespawned", Event_Everyone_OnPlayerBodyDespawned);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerUsernameChanged", Event_Everyone_OnPlayerUsernameChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerNumberChanged", Event_Everyone_OnPlayerNumberChanged);
		EventManager.RemoveEventListener("Event_OnShowPlayerUsernamesChanged", Event_OnShowPlayerUsernamesChanged);
		EventManager.RemoveEventListener("Event_OnPlayerUsernamesFadeThresholdChanged", Event_OnPlayerUsernamesFadeThresholdChanged);
		base.OnDestroy();
	}

	private void Event_Everyone_OnLevelSpawned(Dictionary<string, object> message)
	{
		Level level = (Level)message["level"];
		uiUsernames.Bounds = level.Bounds;
	}

	private void Event_Everyone_OnPlayerBodySpawned(Dictionary<string, object> message)
	{
		PlayerBody playerBody = (PlayerBody)message["playerBody"];
		uiUsernames.AddPlayerBody(playerBody);
	}

	private void Event_Everyone_OnPlayerBodyDespawned(Dictionary<string, object> message)
	{
		PlayerBody playerBody = (PlayerBody)message["playerBody"];
		uiUsernames.RemovePlayerBody(playerBody);
	}

	private void Event_Everyone_OnPlayerUsernameChanged(Dictionary<string, object> message)
	{
		PlayerBody playerBody = ((Player)message["player"]).PlayerBody;
		if ((bool)playerBody)
		{
			uiUsernames.StyleUsername(playerBody);
		}
	}

	private void Event_Everyone_OnPlayerNumberChanged(Dictionary<string, object> message)
	{
		PlayerBody playerBody = ((Player)message["player"]).PlayerBody;
		if ((bool)playerBody)
		{
			uiUsernames.StyleUsername(playerBody);
		}
	}

	private void Event_OnShowPlayerUsernamesChanged(Dictionary<string, object> message)
	{
		if ((bool)message["value"])
		{
			uiUsernames.Show();
		}
		else
		{
			uiUsernames.Hide();
		}
	}

	private void Event_OnPlayerUsernamesFadeThresholdChanged(Dictionary<string, object> message)
	{
		float fadeThreshold = (float)message["value"];
		uiUsernames.FadeThreshold = fadeThreshold;
	}
}
