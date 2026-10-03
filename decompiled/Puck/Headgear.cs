using System;
using UnityEngine;

[Serializable]
public class Headgear
{
	public int ID;

	public GameObject GameObject;

	public GameObject FlagGameObject;

	public HeadgearRole Role;

	public MeshRendererTexturer FlagMeshRendererTexturer
	{
		get
		{
			if (!FlagGameObject)
			{
				return null;
			}
			return FlagGameObject.GetComponent<MeshRendererTexturer>();
		}
	}

	public bool IsForRole(PlayerRole role)
	{
		return role switch
		{
			PlayerRole.Attacker => (Role & HeadgearRole.Attacker) != 0, 
			PlayerRole.Goalie => (Role & HeadgearRole.Goalie) != 0, 
			_ => (Role & HeadgearRole.Any) != 0, 
		};
	}
}
