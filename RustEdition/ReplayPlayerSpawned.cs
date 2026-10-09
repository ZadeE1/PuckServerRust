using Unity.Collections;

public struct ReplayPlayerSpawned
{
	public ulong OwnerClientId;

	public PlayerGameState GameState;

	public PlayerCustomizationState CustomizationState;

	public PlayerHandedness Handedness;

	public FixedString32Bytes SteamId;

	public FixedString32Bytes Username;

	public int Number;

	public int PatreonLevel;

	public int AdminLevel;

	public bool IsMuted;
}
