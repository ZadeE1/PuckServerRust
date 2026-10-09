using UnityEngine;
using UnityEngine.Playables;

public class CrowdMember : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private PlayerMesh playerMesh;

	[SerializeField]
	private Animator animator;

	[HideInInspector]
	public Transform LookTarget;

	private bool animationRequested;

	private double lastUpdateTime;

	private int[] headgearOptions = new int[4] { -1, 537, 538, 539 };

	private int[] teamJerseyOptions = new int[7] { 2048, 2049, 2050, 2051, 2052, 2053, 2054 };

	private int[] spectatorJerseyOptions = new int[5] { 2118, 2119, 2120, 2121, 2122 };

	private int[] mustacheOptions = new int[7] { -1, 1024, 1025, 1026, 1027, 1029, 1030 };

	private int[] beardOptions = new int[6] { -1, 1536, 1537, 1538, 1539, 1540 };

	private const float teamJerseyChance = 0.45f;

	private void Awake()
	{
		animator = GetComponent<Animator>();
		animator.playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
	}

	public void PlayAnimation(string animationName)
	{
		StopAnimations();
		animator.SetBool(animationName, value: true);
		animationRequested = true;
	}

	public void StopAnimations()
	{
		animator.SetBool("Seated", value: false);
		animator.SetBool("Cheering", value: false);
		animator.SetBool("Standing", value: false);
		animationRequested = true;
	}

	public void UpdateAnimation()
	{
		if ((bool)playerMesh)
		{
			AnimatorStateInfo currentAnimatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
			bool num = currentAnimatorStateInfo.loop || currentAnimatorStateInfo.normalizedTime < 1f || animator.IsInTransition(0) || animationRequested;
			double num2 = Time.timeAsDouble - lastUpdateTime;
			if (num)
			{
				animator.Update((float)num2);
			}
			else
			{
				playerMesh.LookAt(LookTarget ? LookTarget.position : Vector3.zero, (float)num2, rotateTorso: false);
			}
			if (animationRequested)
			{
				animationRequested = false;
			}
			lastUpdateTime = Time.timeAsDouble;
		}
	}

	public void RandomizeAppearance()
	{
		playerMesh.SetUsername(null);
		playerMesh.SetNumber(null);
		playerMesh.SetLegsPadsActive(isActive: false);
		int headgearID = headgearOptions[Random.Range(0, headgearOptions.Length)];
		playerMesh.SetHeadgearID(headgearID, PlayerRole.None);
		RandomizeJersey();
		int mustacheID = mustacheOptions[Random.Range(0, mustacheOptions.Length)];
		playerMesh.SetMustacheID(mustacheID);
		int beardID = beardOptions[Random.Range(0, beardOptions.Length)];
		playerMesh.SetBeardID(beardID);
	}

	private void RandomizeJersey()
	{
		int jerseyID = spectatorJerseyOptions[Random.Range(0, spectatorJerseyOptions.Length)];
		playerMesh.PlayerGroin.SetJerseyID(jerseyID, PlayerTeam.None);
		if (Random.value < 0.45f)
		{
			PlayerTeam team = ((Random.value < 0.5f) ? PlayerTeam.Blue : PlayerTeam.Red);
			int jerseyID2 = teamJerseyOptions[Random.Range(0, teamJerseyOptions.Length)];
			playerMesh.PlayerTorso.SetJerseyID(jerseyID2, team);
		}
		else
		{
			int jerseyID3 = spectatorJerseyOptions[Random.Range(0, spectatorJerseyOptions.Length)];
			playerMesh.PlayerTorso.SetJerseyID(jerseyID3, PlayerTeam.None);
		}
	}
}
