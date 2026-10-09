using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIPositionSelect : UIView
{
	[Header("Settings")]
	[SerializeField]
	private int updateRate = 30;

	[Header("References")]
	[SerializeField]
	private VisualTreeAsset positionAsset;

	private PlayerTeam team;

	private Dictionary<PlayerPosition, VisualElement> playerPositionVisualElementMap = new Dictionary<PlayerPosition, VisualElement>();

	private float updateAccumulator;

	private VisualElement positions;

	public PlayerTeam Team
	{
		get
		{
			return team;
		}
		set
		{
			if (team != value)
			{
				PlayerTeam oldTeam = team;
				team = value;
				OnTeamChanged(oldTeam, team);
			}
		}
	}

	public void Initialize(VisualElement rootVisualElement)
	{
		RootVisualElement = rootVisualElement;
		View = rootVisualElement.Query<VisualElement>("PositionsView");
		positions = View.Query<VisualElement>("Positions");
	}

	private void Update()
	{
		if (ApplicationManager.IsDedicatedGameServer)
		{
			return;
		}
		updateAccumulator += Time.deltaTime;
		if (updateAccumulator < 1f / (float)updateRate)
		{
			return;
		}
		updateAccumulator = 0f;
		foreach (KeyValuePair<PlayerPosition, VisualElement> item in playerPositionVisualElementMap)
		{
			PlayerPosition key = item.Key;
			VisualElement value = item.Value;
			if (!(key == null))
			{
				PositionWorldToScreen(value, key);
			}
		}
	}

	public void AddPosition(PlayerPosition playerPosition)
	{
		if (!playerPositionVisualElementMap.ContainsKey(playerPosition))
		{
			VisualElement visualElement = positionAsset.Instantiate();
			((Button)visualElement.Query<Button>()).RegisterCallback((ClickEvent e) =>
			{
				OnPositionClicked(playerPosition);
			});
			positions.Add(visualElement);
			playerPositionVisualElementMap.Add(playerPosition, visualElement);
			StylePosition(playerPosition);
		}
	}

	public void StylePosition(PlayerPosition playerPosition)
	{
		if (playerPositionVisualElementMap.ContainsKey(playerPosition))
		{
			VisualElement visualElement = playerPositionVisualElementMap[playerPosition];
			VisualElement visualElement2 = visualElement.Query<VisualElement>("Position");
			Button button = visualElement.Query<Button>();
			Label label = visualElement.Query<Label>("UsernameLabel");
			UIUtils.SetTeamClass(visualElement2, playerPosition.Team);
			visualElement2.EnableInClassList("claimed", playerPosition.IsClaimed);
			button.text = playerPosition.Name.ToString();
			if (playerPosition.IsClaimed)
			{
				label.text = playerPosition.ClaimedByPlayer.Username.Value.ToString();
			}
			else
			{
				label.text = null;
			}
			visualElement.style.display = ((Team != playerPosition.Team) ? DisplayStyle.None : DisplayStyle.Flex);
		}
	}

	public void RemovePosition(PlayerPosition playerPosition)
	{
		if (playerPositionVisualElementMap.ContainsKey(playerPosition))
		{
			positions.Remove(playerPositionVisualElementMap[playerPosition]);
			playerPositionVisualElementMap.Remove(playerPosition);
		}
	}

	private void PositionWorldToScreen(VisualElement positionVisualElement, PlayerPosition playerPosition)
	{
		if (!(Camera.main == null))
		{
			Vector3 vector = Camera.main.WorldToScreenPoint(playerPosition.transform.position);
			vector.y = (float)Screen.height - vector.y;
			RuntimePanelUtils.ScreenToPanel(RootVisualElement.panel, vector);
			Vector2 vector2 = RuntimePanelUtils.ScreenToPanel(RootVisualElement.panel, vector);
			if (vector.z < 0f)
			{
				positionVisualElement.style.visibility = Visibility.Hidden;
				return;
			}
			positionVisualElement.style.visibility = Visibility.Visible;
			positionVisualElement.style.left = vector2.x;
			positionVisualElement.style.top = vector2.y;
		}
	}

	private void OnPositionClicked(PlayerPosition playerPosition)
	{
		EventManager.TriggerEvent("Event_OnPositionSelectClickPosition", new Dictionary<string, object> { { "playerPosition", playerPosition } });
	}

	private void OnTeamChanged(PlayerTeam oldTeam, PlayerTeam newTeam)
	{
		foreach (PlayerPosition key in playerPositionVisualElementMap.Keys)
		{
			StylePosition(key);
		}
	}
}
