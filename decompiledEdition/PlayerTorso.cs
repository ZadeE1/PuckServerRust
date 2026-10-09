using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(MeshRendererTexturer))]
[ExecuteInEditMode]
public class PlayerTorso : MonoBehaviour
{
	private static readonly Logger Logger = new Logger("PlayerTorso");

	[Header("Settings")]
	[SerializeField]
	private List<Jersey> jerseys = new List<Jersey>();

	[Header("References")]
	[SerializeField]
	private TMP_Text usernameText;

	[SerializeField]
	private TMP_Text numberText;

	private MeshRendererTexturer meshRendererTexturer;

	private void Awake()
	{
		meshRendererTexturer = GetComponent<MeshRendererTexturer>();
		if (ApplicationManager.IsDedicatedGameServer)
		{
			((Component)(object)usernameText).gameObject.SetActive(value: false);
			((Component)(object)numberText).gameObject.SetActive(value: false);
		}
	}

	public void SetUsername(string username)
	{
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			if (string.IsNullOrEmpty(username))
			{
				((Component)(object)usernameText).gameObject.SetActive(value: false);
				return;
			}
			usernameText.text = username;
			((Component)(object)usernameText).gameObject.SetActive(value: true);
		}
	}

	public void SetNumber(string number)
	{
		if (!ApplicationManager.IsDedicatedGameServer)
		{
			if (string.IsNullOrEmpty(number))
			{
				((Component)(object)numberText).gameObject.SetActive(value: false);
				return;
			}
			numberText.text = number;
			((Component)(object)numberText).gameObject.SetActive(value: true);
		}
	}

	public void SetJerseyID(int jerseyID, PlayerTeam team)
	{
		Jersey jersey = jerseys.Find((Jersey j) => j.ID == jerseyID && j.IsForTeam(team));
		if (jersey == null)
		{
			Logger.Warning($"Tried to set invalid jerseyID {jerseyID} for team {team}");
		}
		else
		{
			meshRendererTexturer.SetTexture(jersey.Texture);
		}
	}
}
