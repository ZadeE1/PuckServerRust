using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRendererTexturer))]
[ExecuteInEditMode]
public class PlayerGroin : MonoBehaviour
{
	private static readonly Logger Logger = new Logger("PlayerGroin");

	[Header("References")]
	[SerializeField]
	private List<Jersey> jerseys = new List<Jersey>();

	private MeshRendererTexturer meshRendererTexturer;

	private void Awake()
	{
		meshRendererTexturer = GetComponent<MeshRendererTexturer>();
	}

	public void SetJerseyID(int jerseyID, PlayerTeam team)
	{
		Jersey jersey = jerseys.Find((Jersey j) => j.ID == jerseyID && j.IsForTeam(team));
		if (jersey == null)
		{
			Logger.Warning($"Tried to set invalid jerseyID {jerseyID}");
		}
		else
		{
			meshRendererTexturer.SetTexture(jersey.Texture);
		}
	}
}
