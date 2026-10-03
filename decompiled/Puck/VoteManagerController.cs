using UnityEngine;

public class VoteManagerController : MonoBehaviour
{
	private static readonly Logger Logger = new Logger("VoteManagerController");

	private VoteManager voteManager;

	private void Awake()
	{
		voteManager = GetComponent<VoteManager>();
	}

	private void Start()
	{
	}

	private void OnDestroy()
	{
	}
}
