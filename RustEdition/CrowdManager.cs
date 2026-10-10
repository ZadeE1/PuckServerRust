using System.Collections.Generic;
using UnityEngine;

public class CrowdManager : MonoBehaviourSingleton<CrowdManager>
{
	[Header("Settings")]
	[SerializeField]
	private float crowdDensity = 0.25f;

	[SerializeField]
	private int crowdUpdatesPerFrame = 8;

	[Header("Prefabs")]
	[SerializeField]
	private CrowdMember crowdMemberPrefab;

	private Dictionary<CrowdPosition, CrowdMember> crowdPositionMemberMap = new Dictionary<CrowdPosition, CrowdMember>();

	private List<CrowdMember> crowdMembers = new List<CrowdMember>();

	private Transform currentLookTarget;

	private string currentAnimation = "Seated";

	private int updateBatch;

	private float headlessAccumulator;

	private void Update()
	{
		if (ApplicationManager.IsDedicatedGameServer)
		{
			// ponytail: nothing renders headless; step crowd animation at 30Hz
			// instead of per-frame. Deltas stay real-time so motion is equivalent.
			headlessAccumulator += Time.deltaTime;
			if (headlessAccumulator < 1f / 30f)
			{
				return;
			}
			headlessAccumulator -= 1f / 30f;
		}
		int count = crowdMembers.Count;
		if (count != 0)
		{
			for (int i = 0; i < crowdUpdatesPerFrame; i++)
			{
				int index = (updateBatch + i) % count;
				crowdMembers[index].UpdateAnimation();
			}
			updateBatch = (updateBatch + crowdUpdatesPerFrame) % count;
		}
	}

	public void RegisterCrowdPosition(CrowdPosition position)
	{
		if (!crowdPositionMemberMap.ContainsKey(position) && !(Random.value > crowdDensity))
		{
			CrowdMember crowdMember = Object.Instantiate(crowdMemberPrefab, position.transform.position, position.transform.rotation, transform);
			crowdPositionMemberMap[position] = crowdMember;
			crowdMembers.Add(crowdMember);
			crowdMember.RandomizeAppearance();
			crowdMember.PlayAnimation(currentAnimation);
			crowdMember.LookTarget = currentLookTarget;
			if (ApplicationManager.IsDedicatedGameServer)
			{
				// ponytail: crowd sits outside the play area and only celebrates
				// (animations, kept). Its colliders cost solver pairs for nothing.
				foreach (Collider collider in crowdMember.GetComponentsInChildren<Collider>(true))
				{
					collider.enabled = false;
				}
			}
		}
	}

	public void UnregisterCrowdPosition(CrowdPosition position)
	{
		if (crowdPositionMemberMap.ContainsKey(position))
		{
			CrowdMember crowdMember = crowdPositionMemberMap[position];
			Object.Destroy(crowdMember.gameObject);
			crowdPositionMemberMap.Remove(position);
			crowdMembers.Remove(crowdMember);
		}
	}

	public void SetCrowdLookTarget(Transform lookTarget)
	{
		currentLookTarget = lookTarget;
		foreach (CrowdMember value in crowdPositionMemberMap.Values)
		{
			value.LookTarget = currentLookTarget;
		}
	}

	public void SetCrowdAnimation(string animationName)
	{
		currentAnimation = animationName;
		foreach (CrowdMember value in crowdPositionMemberMap.Values)
		{
			value.PlayAnimation(currentAnimation);
		}
	}
}
