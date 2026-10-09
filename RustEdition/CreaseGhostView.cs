using System.Collections.Generic;
using UnityEngine;

public class CreaseGhostView : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField]
	private float fullGhostDistance = 0.5f;

	[SerializeField]
	private float noGhostDistance = 2.5f;

	[SerializeField]
	private float fadeSpeed = 6f;

	private Zone creaseZone;

	private readonly HashSet<PlayerBody> enemiesInCrease = new HashSet<PlayerBody>();

	private readonly Dictionary<PlayerBody, float> strengthByEnemy = new Dictionary<PlayerBody, float>();

	private readonly List<PlayerBody> scratch = new List<PlayerBody>();

	public Zone CreaseZone => creaseZone;

	public void EnterCrease(Zone zone)
	{
		creaseZone = zone;
	}

	public void ExitCrease()
	{
		creaseZone = null;
		enemiesInCrease.Clear();
	}

	public void AddEnemy(PlayerBody enemy)
	{
		enemiesInCrease.Add(enemy);
	}

	public void RemoveEnemy(PlayerBody enemy)
	{
		enemiesInCrease.Remove(enemy);
	}

	public void ForgetEnemy(PlayerBody enemy)
	{
		enemiesInCrease.Remove(enemy);
		strengthByEnemy.Remove(enemy);
	}

	public void ResetAll()
	{
		foreach (PlayerBody key in strengthByEnemy.Keys)
		{
			if (key != null && key.PlayerMesh != null)
			{
				key.PlayerMesh.SetTransparency(0f);
			}
		}
		creaseZone = null;
		enemiesInCrease.Clear();
		strengthByEnemy.Clear();
	}

	private void Update()
	{
		bool flag = creaseZone != null;
		if (flag)
		{
			foreach (PlayerBody item in enemiesInCrease)
			{
				if (item != null && !strengthByEnemy.ContainsKey(item))
				{
					strengthByEnemy[item] = 0f;
				}
			}
		}
		if (strengthByEnemy.Count == 0)
		{
			return;
		}
		scratch.Clear();
		scratch.AddRange(strengthByEnemy.Keys);
		foreach (PlayerBody item2 in scratch)
		{
			float num = ((flag && item2 != null && enemiesInCrease.Contains(item2)) ? TargetStrength(item2) : 0f);
			float num2 = Mathf.MoveTowards(strengthByEnemy[item2], num, fadeSpeed * Time.deltaTime);
			if (item2 != null && item2.PlayerMesh != null)
			{
				item2.PlayerMesh.SetTransparency(num2);
			}
			if (num2 <= 0f && num <= 0f)
			{
				strengthByEnemy.Remove(item2);
			}
			else
			{
				strengthByEnemy[item2] = num2;
			}
		}
	}

	private float TargetStrength(PlayerBody enemy)
	{
		Vector3 vector = enemy.transform.position - creaseZone.Origin;
		vector.y = 0f;
		return Mathf.Clamp01(Mathf.InverseLerp(noGhostDistance, fullGhostDistance, vector.magnitude));
	}
}
