using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class PlayerHead : MonoBehaviour
{
	private static readonly Logger Logger = new Logger("PlayerHead");

	[Header("References")]
	[SerializeField]
	private List<Flag> flags = new List<Flag>();

	[SerializeField]
	private List<Headgear> headgear = new List<Headgear>();

	[SerializeField]
	private List<Mustache> mustaches = new List<Mustache>();

	[SerializeField]
	private List<Beard> beards = new List<Beard>();

	public void SetFlagID(int flagID)
	{
		headgear.ForEach((Headgear h) =>
		{
			if (h.FlagGameObject != null)
			{
				h.FlagGameObject.SetActive(value: false);
			}
		});
		if (flagID == -1)
		{
			return;
		}
		Flag flag = flags.FirstOrDefault((Flag f) => f.ID == flagID);
		if (flag == null)
		{
			Logger.Warning($"Tried to set invalid flagID {flagID}");
			return;
		}
		headgear.ForEach((Headgear h) =>
		{
			if (h.FlagGameObject != null)
			{
				h.FlagGameObject.SetActive(value: true);
				if (h.FlagMeshRendererTexturer != null)
				{
					h.FlagMeshRendererTexturer.SetTexture(flag.Texture);
				}
			}
		});
	}

	public void SetHeadgearID(int headgearID, PlayerRole role)
	{
		this.headgear.ForEach((Headgear h) =>
		{
			h.GameObject.SetActive(value: false);
		});
		if (headgearID != -1)
		{
			Headgear headgear = this.headgear.FirstOrDefault((Headgear h) => h.ID == headgearID && h.IsForRole(role));
			if (headgear == null)
			{
				Logger.Warning($"Tried to set invalid headgearID {headgearID} for role {role}");
			}
			else
			{
				headgear.GameObject.SetActive(value: true);
			}
		}
	}

	public void SetMustacheID(int mustacheID)
	{
		mustaches.ForEach((Mustache m) =>
		{
			m.GameObject.SetActive(value: false);
		});
		if (mustacheID != -1)
		{
			Mustache mustache = mustaches.FirstOrDefault((Mustache m) => m.ID == mustacheID);
			if (mustache == null)
			{
				Logger.Warning($"Tried to set invalid mustacheID {mustacheID}");
			}
			else
			{
				mustache.GameObject.SetActive(value: true);
			}
		}
	}

	public void SetBeardID(int beardID)
	{
		beards.ForEach((Beard b) =>
		{
			b.GameObject.SetActive(value: false);
		});
		if (beardID != -1)
		{
			Beard beard = beards.FirstOrDefault((Beard b) => b.ID == beardID);
			if (beard == null)
			{
				Logger.Warning($"Tried to set invalid beardID {beardID}");
			}
			else
			{
				beard.GameObject.SetActive(value: true);
			}
		}
	}
}
