using UnityEngine;

public class LockerRoomStick : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private StickMesh attackerStickMesh;

	[SerializeField]
	private StickMesh goalieStickMesh;

	public void SetSkinID(int skinID, PlayerTeam team, PlayerRole role)
	{
		((role == PlayerRole.Goalie) ? goalieStickMesh : attackerStickMesh).SetSkinID(skinID, team);
	}

	public void SetShaftTapeID(int shaftTapeID, PlayerRole role)
	{
		((role == PlayerRole.Goalie) ? goalieStickMesh : attackerStickMesh).SetShaftTapeID(shaftTapeID);
	}

	public void SetBladeTapeID(int bladeTapeID, PlayerRole role)
	{
		((role == PlayerRole.Goalie) ? goalieStickMesh : attackerStickMesh).SetBladeTapeID(bladeTapeID);
	}

	public void ShowRoleStick(PlayerRole role)
	{
		attackerStickMesh.gameObject.SetActive(role == PlayerRole.Attacker);
		goalieStickMesh.gameObject.SetActive(role == PlayerRole.Goalie);
	}
}
