internal interface IGameMode
{
	bool IsInitialized { get; set; }

	bool Initialize(Level level, ServerManager serverManager, GameManager gameManager, PlayerManager playerManager, PuckManager puckManager, ChatManager chatManager, ReplayManager replayManager, VoteManager voteManager);

	bool Dispose();
}
