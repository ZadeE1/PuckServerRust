using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;

public abstract class BaseGameMode<TConfig> : IGameMode where TConfig : BaseGameModeConfig, new()
{
	public Level Level;

	public ServerManager ServerManager;

	public GameManager GameManager;

	public PlayerManager PlayerManager;

	public PuckManager PuckManager;

	public ChatManager ChatManager;

	public ReplayManager ReplayManager;

	public VoteManager VoteManager;

	private string defaultConfigFilePath;

	private string configFilePathCliArgument;

	private string configCliArgument;

	private string configEnvVariable;

	protected Logger Logger => new Logger(Utils.GetReadableTypeName(GetType()));

	public bool IsInitialized { get; set; }

	public TConfig Config { get; private set; }

	public BaseGameMode(string defaultConfigFilePath, string configFilePathCliArgument = null, string configCliArgument = null, string configEnvVariable = null)
	{
		this.defaultConfigFilePath = defaultConfigFilePath;
		this.configFilePathCliArgument = configFilePathCliArgument;
		this.configCliArgument = configCliArgument;
		this.configEnvVariable = configEnvVariable;
	}

	public virtual bool Initialize(Level level, ServerManager serverManager, GameManager gameManager, PlayerManager playerManager, PuckManager puckManager, ChatManager chatManager, ReplayManager replayManager, VoteManager voteManager)
	{
		if (IsInitialized)
		{
			return false;
		}
		IsInitialized = true;
		Level = level;
		ServerManager = serverManager;
		GameManager = gameManager;
		PlayerManager = playerManager;
		PuckManager = puckManager;
		ChatManager = chatManager;
		ReplayManager = replayManager;
		VoteManager = voteManager;
		LoadConfig(defaultConfigFilePath, configFilePathCliArgument, configCliArgument, configEnvVariable);
		SubscribeEvents();
		return true;
	}

	public virtual bool Dispose()
	{
		if (!IsInitialized)
		{
			return false;
		}
		IsInitialized = false;
		Level = null;
		GameManager = null;
		PlayerManager = null;
		PuckManager = null;
		ChatManager = null;
		ReplayManager = null;
		VoteManager = null;
		UnsubscribeEvents();
		return true;
	}

	protected virtual void SubscribeEvents()
	{
		EventManager.AddEventListener("Event_Everyone_OnGameStateChanged", Event_Everyone_OnGameStateChanged);
		EventManager.AddEventListener("Event_Everyone_OnPlayerAdded", Event_Everyone_OnPlayerAdded);
		EventManager.AddEventListener("Event_Everyone_OnPlayerRemoved", Event_Everyone_OnPlayerRemoved);
		EventManager.AddEventListener("Event_Everyone_OnPlayerGameStateChanged", Event_Everyone_OnPlayerGameStateChanged);
		EventManager.AddEventListener("Event_Everyone_OnPlayerPositionChanged", Event_Everyone_OnPlayerPositionChanged);
		EventManager.AddEventListener("Event_Everyone_OnGoalScored", Event_Everyone_OnGoalScored);
		EventManager.AddEventListener("Event_Everyone_OnPlayerEnterZone", Event_Everyone_OnPlayerEnterZone);
		EventManager.AddEventListener("Event_Everyone_OnPlayerExitZone", Event_Everyone_OnPlayerExitZone);
		EventManager.AddEventListener("Event_Server_OnPhaseExpired", Event_Server_OnPhaseExpired);
		EventManager.AddEventListener("Event_Server_OnPuckEnterGoal", Event_Server_OnPuckEnterGoal);
		EventManager.AddEventListener("Event_Server_OnPlayerRequestTeamSelect", Event_Server_OnPlayerRequestTeamSelect);
		EventManager.AddEventListener("Event_Server_OnPlayerRequestTeam", Event_Server_OnPlayerRequestTeam);
		EventManager.AddEventListener("Event_Server_OnPlayerRequestPositionSelect", Event_Server_OnPlayerRequestPositionSelect);
		EventManager.AddEventListener("Event_Server_OnPlayerRequestPosition", Event_Server_OnPlayerRequestPosition);
		EventManager.AddEventListener("Event_Server_OnPlayerRequestHandedness", Event_Server_OnPlayerRequestHandedness);
		EventManager.AddEventListener("Event_Server_OnPlayerRequestForfeit", Event_Server_OnPlayerRequestForfeit);
		EventManager.AddEventListener("Event_Server_OnVoteAdded", Event_Server_OnVoteAdded);
		EventManager.AddEventListener("Event_Server_OnVoteProgressed", Event_Server_OnVoteProgressed);
		EventManager.AddEventListener("Event_Server_OnVoteRemoved", Event_Server_OnVoteRemoved);
		EventManager.AddEventListener("Event_Server_OnChatCommand", Event_Server_OnChatCommand);
	}

	protected virtual void UnsubscribeEvents()
	{
		EventManager.RemoveEventListener("Event_Everyone_OnGameStateChanged", Event_Everyone_OnGameStateChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerAdded", Event_Everyone_OnPlayerAdded);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerRemoved", Event_Everyone_OnPlayerRemoved);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerGameStateChanged", Event_Everyone_OnPlayerGameStateChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerPositionChanged", Event_Everyone_OnPlayerPositionChanged);
		EventManager.RemoveEventListener("Event_Everyone_OnGoalScored", Event_Everyone_OnGoalScored);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerEnterZone", Event_Everyone_OnPlayerEnterZone);
		EventManager.RemoveEventListener("Event_Everyone_OnPlayerExitZone", Event_Everyone_OnPlayerExitZone);
		EventManager.RemoveEventListener("Event_Server_OnPhaseExpired", Event_Server_OnPhaseExpired);
		EventManager.RemoveEventListener("Event_Server_OnPuckEnterGoal", Event_Server_OnPuckEnterGoal);
		EventManager.RemoveEventListener("Event_Server_OnPlayerRequestTeamSelect", Event_Server_OnPlayerRequestTeamSelect);
		EventManager.RemoveEventListener("Event_Server_OnPlayerRequestTeam", Event_Server_OnPlayerRequestTeam);
		EventManager.RemoveEventListener("Event_Server_OnPlayerRequestPositionSelect", Event_Server_OnPlayerRequestPositionSelect);
		EventManager.RemoveEventListener("Event_Server_OnPlayerRequestPosition", Event_Server_OnPlayerRequestPosition);
		EventManager.RemoveEventListener("Event_Server_OnPlayerRequestHandedness", Event_Server_OnPlayerRequestHandedness);
		EventManager.RemoveEventListener("Event_Server_OnPlayerRequestForfeit", Event_Server_OnPlayerRequestForfeit);
		EventManager.RemoveEventListener("Event_Server_OnVoteAdded", Event_Server_OnVoteAdded);
		EventManager.RemoveEventListener("Event_Server_OnVoteProgressed", Event_Server_OnVoteProgressed);
		EventManager.RemoveEventListener("Event_Server_OnVoteRemoved", Event_Server_OnVoteRemoved);
		EventManager.RemoveEventListener("Event_Server_OnChatCommand", Event_Server_OnChatCommand);
	}

	private void LoadConfig(string defaultFilePath, string filePathCliArgument = null, string cliArgument = null, string envVariable = null)
	{
		string environmentVariable = Environment.GetEnvironmentVariable(envVariable);
		string commandLineArgument = Utils.GetCommandLineArgument(cliArgument);
		if (!string.IsNullOrEmpty(commandLineArgument))
		{
			Logger.Info("Deserializing config from CLI argument (" + cliArgument + ")");
			Config = ConfigUtils.LoadConfigFromSerializedString<TConfig>(commandLineArgument);
		}
		else if (!string.IsNullOrEmpty(environmentVariable))
		{
			Logger.Info("Deserializing config from environment variable (" + envVariable + ")");
			Config = ConfigUtils.LoadConfigFromSerializedString<TConfig>(environmentVariable);
		}
		else
		{
			string text = Utils.GetCommandLineArgument(filePathCliArgument) ?? defaultFilePath;
			Logger.Info("Deserializing config from file (" + text + ")");
			Config = ConfigUtils.LoadConfigFromFile<TConfig>(text);
		}
		if (Config == null)
		{
			Logger.Error("Game mode config is invalid (see the error above); falling back to default " + typeof(TConfig).Name + ".");
			Config = new TConfig();
		}
		OnConfigLoaded();
	}

	protected virtual void ScoreGoal(PlayerTeam byTeam, Player goalPlayer, Player assistPlayer, Player secondAssistPlayer, Puck puck)
	{
		NetworkObjectReference goalPlayerNetworkObjectReference = new NetworkObjectReference(goalPlayer?.NetworkObject);
		NetworkObjectReference assistPlayerNetworkObjectReference = new NetworkObjectReference(assistPlayer?.NetworkObject);
		NetworkObjectReference secondAssistPlayerNetworkObjectReference = new NetworkObjectReference(secondAssistPlayer?.NetworkObject);
		NetworkObjectReference puckNetworkObjectReference = new NetworkObjectReference(puck?.NetworkObject);
		GameManager.Server_NotifyGoalScoredRpc(byTeam, goalPlayerNetworkObjectReference, assistPlayerNetworkObjectReference, secondAssistPlayerNetworkObjectReference, puckNetworkObjectReference);
	}

	protected virtual void OnConfigLoaded()
	{
	}

	protected virtual void OnGameStateChanged(GameState oldGameState, GameState newGameState)
	{
		if (oldGameState.Phase != newGameState.Phase)
		{
			OnGamePhaseEnded(oldGameState.Phase);
			OnGamePhaseStarted(newGameState.Phase);
		}
	}

	protected virtual void OnGamePhaseTimedOut(GamePhase gamePhase)
	{
		switch (gamePhase)
		{
		case GamePhase.Warmup:
			OnWarmupTimedOut();
			break;
		case GamePhase.PreGame:
			OnPreGameTimedOut();
			break;
		case GamePhase.FaceOff:
			OnFaceOffTimedOut();
			break;
		case GamePhase.BlueScore:
			OnBlueScoreTimedOut();
			break;
		case GamePhase.RedScore:
			OnRedScoreTimedOut();
			break;
		case GamePhase.Replay:
			OnReplayTimedOut();
			break;
		case GamePhase.Intermission:
			OnIntermissionTimedOut();
			break;
		case GamePhase.Play:
			OnPlayTimedOut();
			break;
		case GamePhase.GameOver:
			OnGameOverTimedOut();
			break;
		case GamePhase.PostGame:
			OnPostGameTimedOut();
			break;
		}
	}

	protected virtual void OnWarmupTimedOut()
	{
	}

	protected virtual void OnPreGameTimedOut()
	{
	}

	protected virtual void OnFaceOffTimedOut()
	{
	}

	protected virtual void OnBlueScoreTimedOut()
	{
	}

	protected virtual void OnRedScoreTimedOut()
	{
	}

	protected virtual void OnReplayTimedOut()
	{
	}

	protected virtual void OnIntermissionTimedOut()
	{
	}

	protected virtual void OnPlayTimedOut()
	{
	}

	protected virtual void OnGameOverTimedOut()
	{
	}

	protected virtual void OnPostGameTimedOut()
	{
	}

	protected virtual void OnGamePhaseStarted(GamePhase gamePhase)
	{
		switch (gamePhase)
		{
		case GamePhase.Warmup:
			OnWarmupStarted();
			break;
		case GamePhase.PreGame:
			OnPreGameStarted();
			break;
		case GamePhase.FaceOff:
			OnFaceOffStarted();
			break;
		case GamePhase.BlueScore:
			OnBlueScoreStarted();
			break;
		case GamePhase.RedScore:
			OnRedScoreStarted();
			break;
		case GamePhase.Replay:
			OnReplayStarted();
			break;
		case GamePhase.Intermission:
			OnIntermissionStarted();
			break;
		case GamePhase.Play:
			OnPlayStarted();
			break;
		case GamePhase.GameOver:
			OnGameOverStarted();
			break;
		case GamePhase.PostGame:
			OnPostGameStarted();
			break;
		}
	}

	protected virtual void OnWarmupStarted()
	{
	}

	protected virtual void OnPreGameStarted()
	{
	}

	protected virtual void OnFaceOffStarted()
	{
	}

	protected virtual void OnBlueScoreStarted()
	{
	}

	protected virtual void OnRedScoreStarted()
	{
	}

	protected virtual void OnReplayStarted()
	{
	}

	protected virtual void OnIntermissionStarted()
	{
	}

	protected virtual void OnPlayStarted()
	{
	}

	protected virtual void OnGameOverStarted()
	{
	}

	protected virtual void OnPostGameStarted()
	{
	}

	protected virtual void OnGamePhaseEnded(GamePhase gamePhase)
	{
		switch (gamePhase)
		{
		case GamePhase.Warmup:
			OnWarmupEnded();
			break;
		case GamePhase.PreGame:
			OnPreGameEnded();
			break;
		case GamePhase.FaceOff:
			OnFaceOffEnded();
			break;
		case GamePhase.BlueScore:
			OnBlueScoreEnded();
			break;
		case GamePhase.RedScore:
			OnRedScoreEnded();
			break;
		case GamePhase.Replay:
			OnReplayEnded();
			break;
		case GamePhase.Intermission:
			OnIntermissionEnded();
			break;
		case GamePhase.Play:
			OnPlayEnded();
			break;
		case GamePhase.GameOver:
			OnGameOverEnded();
			break;
		case GamePhase.PostGame:
			OnPostGameEnded();
			break;
		}
	}

	protected virtual void OnWarmupEnded()
	{
	}

	protected virtual void OnPreGameEnded()
	{
	}

	protected virtual void OnFaceOffEnded()
	{
	}

	protected virtual void OnBlueScoreEnded()
	{
	}

	protected virtual void OnRedScoreEnded()
	{
	}

	protected virtual void OnReplayEnded()
	{
	}

	protected virtual void OnIntermissionEnded()
	{
	}

	protected virtual void OnPlayEnded()
	{
	}

	protected virtual void OnGameOverEnded()
	{
	}

	protected virtual void OnPostGameEnded()
	{
	}

	protected virtual void OnPlayerJoined(Player player)
	{
		ChatManager.Server_BroadcastChatMessage($"{player.Username.Value} has joined the server", "#b8b8b8");
	}

	protected virtual void OnPlayerLeft(Player player)
	{
		ChatManager.Server_BroadcastChatMessage($"{player.Username.Value} has left the server", "#b8b8b8");
	}

	protected virtual void OnPlayerGameStateChanged(Player player, PlayerGameState oldGameState, PlayerGameState newGameState)
	{
		if (oldGameState.Phase != newGameState.Phase)
		{
			OnPlayerPhaseChanged(player, oldGameState.Phase, newGameState.Phase);
		}
		if (oldGameState.Team != newGameState.Team)
		{
			OnPlayerTeamChanged(player, oldGameState.Team, newGameState.Team);
		}
		if (oldGameState.Role != newGameState.Role)
		{
			OnPlayerRoleChanged(player, oldGameState.Role, newGameState.Role);
		}
	}

	protected virtual void OnPlayerPhaseChanged(Player player, PlayerPhase oldPlayerPhase, PlayerPhase newPlayerPhase)
	{
	}

	protected virtual void OnPlayerTeamChanged(Player player, PlayerTeam oldPlayerTeam, PlayerTeam newPlayerTeam)
	{
		if (newPlayerTeam != PlayerTeam.None && newPlayerTeam != PlayerTeam.Spectator)
		{
			string arg = StringUtils.WrapInTeamColor(newPlayerTeam.ToString(), newPlayerTeam);
			ChatManager.Server_BroadcastChatMessage($"{player.Username.Value} joined team {arg}", "#b8b8b8");
		}
	}

	protected virtual void OnPlayerRoleChanged(Player player, PlayerRole oldPlayerRole, PlayerRole newPlayerRole)
	{
	}

	protected virtual void OnPlayerPositionChanged(Player player, PlayerPosition oldPlayerPosition, PlayerPosition newPlayerPosition)
	{
	}

	protected virtual void OnGoalScored(PlayerTeam byTeam, Player goalPlayer, Player assistPlayer, Player secondAssistPlayer, Puck puck)
	{
	}

	protected virtual void OnPuckEnterGoal(PlayerTeam team, Puck puck)
	{
	}

	protected virtual void OnPlayerEnterZone(Zone zone, PlayerBody playerBody)
	{
	}

	protected virtual void OnPlayerExitZone(Zone zone, PlayerBody playerBody)
	{
	}

	protected virtual void OnPlayerRequestTeamSelect(Player player)
	{
	}

	protected virtual void OnPlayerRequestTeam(Player player, PlayerTeam team)
	{
	}

	protected virtual void OnPlayerRequestPositionSelect(Player player)
	{
	}

	protected virtual void OnPlayerRequestPosition(Player player, PlayerPosition position)
	{
	}

	protected virtual void OnPlayerRequestHandedness(Player player, PlayerHandedness handedness)
	{
		player.Handedness.Value = handedness;
	}

	protected virtual void OnPlayerRequestForfeit(Player player)
	{
	}

	protected virtual void OnVoteAdded(Vote vote)
	{
		Player playerBySteamId = PlayerManager.GetPlayerBySteamId(vote.SteamId);
		List<Player> playersByTeams = PlayerManager.GetPlayersByTeams(vote.Teams);
		if ((bool)playerBySteamId && playersByTeams.Count > 0)
		{
			string text = StringUtils.WrapInTeamColor(playerBySteamId.Username.Value.ToString(), playerBySteamId.Team);
			ChatManager.Server_SendChatMessage(vote.Title + " vote started by " + text + " (" + vote.Description + ")", "#e67e22", playersByTeams.ConvertAll((Player p) => p.OwnerClientId).ToArray());
		}
	}

	protected virtual void OnVoteProgressed(Vote vote, string steamId, bool inFavour)
	{
		if (!inFavour)
		{
			return;
		}
		List<Player> playersByTeams = PlayerManager.GetPlayersByTeams(vote.Teams);
		if (playersByTeams.Count > 0)
		{
			ChatManager.Server_SendChatMessage($"{vote.Title} vote progressed {vote.InFavourVotes}/{vote.RequiredVotes}", "#e67e22", playersByTeams.ConvertAll((Player p) => p.OwnerClientId).ToArray());
		}
	}

	protected virtual void OnVoteRemoved(Vote vote)
	{
		Player playerBySteamId = PlayerManager.GetPlayerBySteamId(vote.SteamId);
		List<Player> playersByTeams = PlayerManager.GetPlayersByTeams(vote.Teams);
		if ((bool)playerBySteamId && playersByTeams.Count > 0)
		{
			string text = (vote.Passed ? "passed" : "failed");
			ChatManager.Server_SendChatMessage(vote.Title + " vote " + text, "#e67e22", playersByTeams.ConvertAll((Player p) => p.OwnerClientId).ToArray());
		}
	}

	protected virtual List<ChatCommandInfo> GetChatCommands()
	{
		return new List<ChatCommandInfo>
		{
			new ChatCommandInfo("/help", "List the commands available to you")
		};
	}

	protected virtual void OnChatCommand(Player player, string command, string[] args)
	{
		if (command == "/help")
		{
			SendChatCommandHelp(player);
			return;
		}
		ChatManager.Server_SendChatMessage("Unknown command", "#e74c3c", player.OwnerClientId);
	}

	private void SendChatCommandHelp(Player player)
	{
		int num = player.AdminLevel.Value;
		if (num < 1 && ServerManager.AdminManager.IsSteamIdAdmin(player.SteamId.Value.ToString()))
		{
			num = 1;
		}
		List<string> list = new List<string> { "Available commands:" };
		foreach (ChatCommandInfo chatCommand in GetChatCommands())
		{
			if (chatCommand.RequiredAdminLevel <= num)
			{
				list.Add(chatCommand.Usage + " - " + chatCommand.Description);
			}
		}
		int byteCount = Encoding.UTF8.GetByteCount(StringUtils.WrapInColor(string.Empty, "#b8b8b8"));
		int maxBytes = 509 - byteCount;
		foreach (string item in StringUtils.ChunkLinesByByteBudget(list, maxBytes))
		{
			ChatManager.Server_SendChatMessage(item, "#b8b8b8", player.OwnerClientId);
		}
	}

	private void Event_Everyone_OnGameStateChanged(Dictionary<string, object> message)
	{
		GameState oldGameState = (GameState)message["oldGameState"];
		GameState newGameState = (GameState)message["newGameState"];
		OnGameStateChanged(oldGameState, newGameState);
	}

	private void Event_Everyone_OnPlayerAdded(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		if (!player.IsReplay.Value)
		{
			OnPlayerJoined(player);
		}
	}

	private void Event_Everyone_OnPlayerRemoved(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		if (!player.IsReplay.Value)
		{
			OnPlayerLeft(player);
		}
	}

	private void Event_Everyone_OnPlayerGameStateChanged(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		PlayerGameState oldGameState = (PlayerGameState)message["oldGameState"];
		PlayerGameState newGameState = (PlayerGameState)message["newGameState"];
		if (!player.IsReplay.Value)
		{
			OnPlayerGameStateChanged(player, oldGameState, newGameState);
		}
	}

	private void Event_Everyone_OnPlayerPositionChanged(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		PlayerPosition oldPlayerPosition = (PlayerPosition)message["oldPlayerPosition"];
		PlayerPosition newPlayerPosition = (PlayerPosition)message["newPlayerPosition"];
		if (!player.IsReplay.Value)
		{
			OnPlayerPositionChanged(player, oldPlayerPosition, newPlayerPosition);
		}
	}

	private void Event_Everyone_OnGoalScored(Dictionary<string, object> message)
	{
		PlayerTeam byTeam = (PlayerTeam)message["byTeam"];
		Player goalPlayer = (Player)message["goalPlayer"];
		Player assistPlayer = (Player)message["assistPlayer"];
		Player secondAssistPlayer = (Player)message["secondAssistPlayer"];
		Puck puck = (Puck)message["puck"];
		OnGoalScored(byTeam, goalPlayer, assistPlayer, secondAssistPlayer, puck);
	}

	private void Event_Server_OnPhaseExpired(Dictionary<string, object> message)
	{
		GamePhase gamePhase = (GamePhase)message["phase"];
		OnGamePhaseTimedOut(gamePhase);
	}

	private void Event_Server_OnPuckEnterGoal(Dictionary<string, object> message)
	{
		PlayerTeam team = (PlayerTeam)message["team"];
		Puck puck = (Puck)message["puck"];
		OnPuckEnterGoal(team, puck);
	}

	private void Event_Everyone_OnPlayerEnterZone(Dictionary<string, object> message)
	{
		Zone zone = (Zone)message["zone"];
		PlayerBody playerBody = (PlayerBody)message["playerBody"];
		OnPlayerEnterZone(zone, playerBody);
	}

	private void Event_Everyone_OnPlayerExitZone(Dictionary<string, object> message)
	{
		Zone zone = (Zone)message["zone"];
		PlayerBody playerBody = (PlayerBody)message["playerBody"];
		OnPlayerExitZone(zone, playerBody);
	}

	private void Event_Server_OnPlayerRequestTeamSelect(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		if (!player.IsReplay.Value)
		{
			OnPlayerRequestTeamSelect(player);
		}
	}

	private void Event_Server_OnPlayerRequestTeam(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		PlayerTeam team = (PlayerTeam)message["team"];
		if (!player.IsReplay.Value)
		{
			OnPlayerRequestTeam(player, team);
		}
	}

	private void Event_Server_OnPlayerRequestPositionSelect(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		if (!player.IsReplay.Value)
		{
			OnPlayerRequestPositionSelect(player);
		}
	}

	private void Event_Server_OnPlayerRequestPosition(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		PlayerPosition position = (PlayerPosition)message["playerPosition"];
		if (!player.IsReplay.Value)
		{
			OnPlayerRequestPosition(player, position);
		}
	}

	private void Event_Server_OnPlayerRequestHandedness(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		PlayerHandedness handedness = (PlayerHandedness)message["handedness"];
		if (!player.IsReplay.Value)
		{
			OnPlayerRequestHandedness(player, handedness);
		}
	}

	private void Event_Server_OnPlayerRequestForfeit(Dictionary<string, object> message)
	{
		Player player = (Player)message["player"];
		if (!player.IsReplay.Value)
		{
			OnPlayerRequestForfeit(player);
		}
	}

	private void Event_Server_OnVoteAdded(Dictionary<string, object> message)
	{
		Vote vote = (Vote)message["vote"];
		OnVoteAdded(vote);
	}

	private void Event_Server_OnVoteProgressed(Dictionary<string, object> message)
	{
		Vote vote = (Vote)message["vote"];
		string steamId = (string)message["steamId"];
		bool inFavour = (bool)message["inFavour"];
		OnVoteProgressed(vote, steamId, inFavour);
	}

	private void Event_Server_OnVoteRemoved(Dictionary<string, object> message)
	{
		Vote vote = (Vote)message["vote"];
		OnVoteRemoved(vote);
	}

	private void Event_Server_OnChatCommand(Dictionary<string, object> message)
	{
		ulong clientId = (ulong)message["clientId"];
		string command = (string)message["command"];
		string[] args = (string[])message["args"];
		Player playerByClientId = PlayerManager.GetPlayerByClientId(clientId);
		if (playerByClientId != null)
		{
			OnChatCommand(playerByClientId, command, args);
		}
	}
}
