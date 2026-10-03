using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIScoreboard : UIView
{
	[Header("References")]
	public VisualTreeAsset playerAsset;

	private VisualElement scoreboard;

	private VisualElement header;

	private VisualElement players;

	private Label nameLabel;

	private Label playersLabel;

	private Dictionary<Player, VisualElement> playerVisualElementMap = new Dictionary<Player, VisualElement>();

	public void Initialize(VisualElement rootVisualElement)
	{
		View = rootVisualElement.Query<VisualElement>("ScoreboardView");
		scoreboard = View.Query<VisualElement>("Scoreboard");
		header = scoreboard.Query<VisualElement>("Header");
		players = scoreboard.Query<VisualElement>("Players");
		nameLabel = header.Query<Label>("NameLabel");
		playersLabel = header.Query<Label>("PlayersLabel");
		players.Clear();
	}

	public void AddPlayer(Player player)
	{
		if (!playerVisualElementMap.ContainsKey(player))
		{
			VisualElement visualElement = playerAsset.Instantiate();
			visualElement.userData = player;
			players.Add(visualElement);
			((Button)visualElement.Query<Button>()).RegisterCallback<ClickEvent, Player>(OnPlayerClicked, player);
			playerVisualElementMap.Add(player, visualElement);
			StylePlayer(player);
		}
	}

	public void RemovePlayer(Player player)
	{
		if (playerVisualElementMap.ContainsKey(player))
		{
			((Button)playerVisualElementMap[player].Query<Button>()).UnregisterCallback<ClickEvent, Player>(OnPlayerClicked);
			players.Remove(playerVisualElementMap[player]);
			playerVisualElementMap.Remove(player);
		}
	}

	public void StylePlayer(Player player)
	{
		if (playerVisualElementMap.ContainsKey(player))
		{
			VisualElement visualElement = playerVisualElementMap[player].Query<VisualElement>("Player");
			UIUtils.SetTeamClass(visualElement, player.Team);
			visualElement.EnableInClassList("patreon", player.PatreonLevel.Value > 0);
			visualElement.EnableInClassList("moderator", player.AdminLevel.Value == 1);
			visualElement.EnableInClassList("admin", player.AdminLevel.Value == 2);
			visualElement.EnableInClassList("developer", player.AdminLevel.Value == 3);
			Label label = visualElement.Query<Label>("PositionLabel");
			Label label2 = visualElement.Query<Label>("UsernameLabel");
			Label label3 = visualElement.Query<Label>("GoalsLabel");
			Label label4 = visualElement.Query<Label>("AssistsLabel");
			Label label5 = visualElement.Query<Label>("PointsLabel");
			Label label6 = visualElement.Query<Label>("PingLabel");
			bool flag = player.Team != PlayerTeam.Blue && player.Team != PlayerTeam.Red;
			label.text = (player.PlayerPosition ? player.PlayerPosition.Name.ToString() : string.Empty);
			label2.text = $"#{player.Number.Value} {player.Username.Value}";
			label3.text = (flag ? string.Empty : player.Goals.Value.ToString());
			label4.text = (flag ? string.Empty : player.Assists.Value.ToString());
			label5.text = (flag ? string.Empty : (player.Goals.Value + player.Assists.Value).ToString());
			label6.text = $"{player.Ping.Value}ms";
			SortPlayers();
		}
	}

	public void UpdatePlayerPing(Player player)
	{
		if (playerVisualElementMap.TryGetValue(player, out var value))
		{
			((Label)value.Query<Label>("PingLabel")).text = $"{player.Ping.Value}ms";
		}
	}

	public void StyleServer(Server server, int playerCount)
	{
		nameLabel.text = server.Name.Value;
		playersLabel.text = $"{playerCount}/{server.MaxPlayers}";
	}

	public void SortPlayers()
	{
		players.hierarchy.Sort((VisualElement a, VisualElement b) =>
		{
			Player player = (Player)a.userData;
			Player player2 = (Player)b.userData;
			int num = GetTeamOrder(player.Team);
			int num2 = GetTeamOrder(player2.Team);
			int num3 = player.Goals.Value + player.Assists.Value;
			int num4 = player2.Goals.Value + player2.Assists.Value;
			if (num != num2)
			{
				return num.CompareTo(num2);
			}
			return (num3 != num4) ? num4.CompareTo(num3) : player.Username.Value.CompareTo(player2.Username.Value);
		});
		static int GetTeamOrder(PlayerTeam team)
		{
			return team switch
			{
				PlayerTeam.Blue => 0, 
				PlayerTeam.Red => 1, 
				_ => 2, 
			};
		}
	}

	private void OnPlayerClicked(ClickEvent clickEvent, Player player)
	{
		EventManager.TriggerEvent("Event_OnScoreboardClickPlayer", new Dictionary<string, object> { { "player", player } });
	}
}
