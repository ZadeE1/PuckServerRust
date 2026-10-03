using System;
using UnityEngine;

[Serializable]
public class Jersey
{
	public int ID;

	public JerseyTeam Team;

	public Texture Texture;

	public bool IsForTeam(PlayerTeam team)
	{
		return team switch
		{
			PlayerTeam.Blue => (Team & JerseyTeam.Blue) != 0, 
			PlayerTeam.Red => (Team & JerseyTeam.Red) != 0, 
			_ => (Team & JerseyTeam.Any) != 0, 
		};
	}
}
