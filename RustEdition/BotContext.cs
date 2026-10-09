using UnityEngine;

public struct BotContext
{
	public Puck TargetPuck;

	public bool IsClosestTeammateToPuck;

	public Vector3 HomePosition;

	public bool HasHomePosition;

	public float Seed;

	public PlayerBody TargetPlayerBody;
}
