using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIUsernames : UIView
{
	private static readonly Logger Logger = new Logger("UIUsernames");

	[Header("Settings")]
	[SerializeField]
	private float yOffset = 2.5f;

	[Header("References")]
	[SerializeField]
	private VisualTreeAsset playerUsernameAsset;

	[HideInInspector]
	public float FadeThreshold = 0.5f;

	[HideInInspector]
	public Bounds Bounds;

	private Dictionary<PlayerBody, VisualElement> playerBodyVisualElementMap = new Dictionary<PlayerBody, VisualElement>();

	private VisualElement usernames;

	[HideInInspector]
	public float MaximumDistance => Mathf.Max(Bounds.size.x, Bounds.size.z);

	[HideInInspector]
	public float FadeRange => MaximumDistance / 4f;

	public void Initialize(VisualElement rootVisualElement)
	{
		RootVisualElement = rootVisualElement;
		View = rootVisualElement.Query<VisualElement>("UsernamesView");
		usernames = View.Query<VisualElement>("Usernames");
		usernames.Clear();
	}

	private void Update()
	{
		if (ApplicationManager.IsDedicatedGameServer)
		{
			return;
		}
		foreach (KeyValuePair<PlayerBody, VisualElement> item in playerBodyVisualElementMap)
		{
			PlayerBody key = item.Key;
			VisualElement value = item.Value;
			if (!(key == null))
			{
				UsernameWorldToScreen(value, key);
			}
		}
	}

	public override bool Show()
	{
		if (!SettingsManager.ShowPlayerUsernames)
		{
			return false;
		}
		return base.Show();
	}

	public void AddPlayerBody(PlayerBody playerBody)
	{
		TemplateContainer templateContainer = playerUsernameAsset.Instantiate();
		playerBodyVisualElementMap.Add(playerBody, templateContainer);
		StyleUsername(playerBody);
		usernames.Add(templateContainer);
	}

	public void RemovePlayerBody(PlayerBody playerBody)
	{
		if (playerBodyVisualElementMap.ContainsKey(playerBody))
		{
			VisualElement element = playerBodyVisualElementMap[playerBody];
			playerBodyVisualElementMap.Remove(playerBody);
			usernames.Remove(element);
		}
	}

	public void StyleUsername(PlayerBody playerBody)
	{
		if (playerBodyVisualElementMap.ContainsKey(playerBody))
		{
			((Label)playerBodyVisualElementMap[playerBody].Query<Label>("UsernameLabel")).text = $"#{playerBody.Player.Number.Value} {playerBody.Player.Username.Value}";
		}
	}

	private void UsernameWorldToScreen(VisualElement playerVisualElement, PlayerBody playerBody)
	{
		if (!(Camera.main == null))
		{
			Vector3 position = Camera.main.transform.position;
			Vector3 position2 = playerBody.transform.position;
			float value = Vector3.Distance(position, position2);
			Vector3 vector = Camera.main.WorldToScreenPoint(position2 + Vector3.up * yOffset);
			vector.y = (float)Screen.height - vector.y;
			RuntimePanelUtils.ScreenToPanel(RootVisualElement.panel, vector);
			Vector2 vector2 = RuntimePanelUtils.ScreenToPanel(RootVisualElement.panel, vector);
			if (vector.z < 0f)
			{
				playerVisualElement.style.display = DisplayStyle.None;
				return;
			}
			float value2 = Utils.Map(value, MaximumDistance * FadeThreshold, MaximumDistance * FadeThreshold + FadeRange, 1f, 0f);
			value2 = Mathf.Clamp01(value2);
			playerVisualElement.style.display = DisplayStyle.Flex;
			playerVisualElement.style.left = vector2.x;
			playerVisualElement.style.top = vector2.y;
			playerVisualElement.style.opacity = new StyleFloat(value2);
		}
	}
}
