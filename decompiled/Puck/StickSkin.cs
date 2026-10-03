using System;
using UnityEngine;

[Serializable]
public class StickSkin
{
	public int ID;

	public StickSkinTeam Team;

	public Material Material;

	public bool IsForTeam(PlayerTeam team)
	{
		return team switch
		{
			PlayerTeam.Blue => (Team & StickSkinTeam.Blue) != 0, 
			PlayerTeam.Red => (Team & StickSkinTeam.Red) != 0, 
			_ => (Team & StickSkinTeam.Any) != 0, 
		};
	}
}
