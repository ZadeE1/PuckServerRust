using System.Collections.Generic;
using UnityEngine;

public class StickMesh : MonoBehaviour
{
	private static readonly Logger Logger = new Logger("StickMesh");

	[Header("Settings")]
	[SerializeField]
	private List<StickSkin> skins = new List<StickSkin>();

	[SerializeField]
	private List<StickTape> shaftTapes = new List<StickTape>();

	[SerializeField]
	private List<StickTape> bladeTapes = new List<StickTape>();

	[Header("References")]
	[SerializeField]
	private MeshRenderer stickMeshRenderer;

	[SerializeField]
	private GameObject shaftTapeGameObject;

	[SerializeField]
	private MeshRenderer shaftTapeMeshRenderer;

	[SerializeField]
	private GameObject bladeTapeGameObject;

	[SerializeField]
	private MeshRenderer bladeTapeMeshRenderer;

	[Space(20f)]
	[SerializeField]
	private Collider shaftCollider;

	[SerializeField]
	private Collider bladeCollider;

	[HideInInspector]
	public Collider ShaftCollider => shaftCollider;

	[HideInInspector]
	public Collider BladeCollider => bladeCollider;

	private void OnDestroy()
	{
		Object.Destroy(stickMeshRenderer.material);
		Object.Destroy(shaftTapeMeshRenderer.material);
		Object.Destroy(bladeTapeMeshRenderer.material);
	}

	public void SetSkinID(int skinID, PlayerTeam team)
	{
		StickSkin stickSkin = skins.Find((StickSkin s) => s.ID == skinID && s.IsForTeam(team));
		if (stickSkin == null)
		{
			Logger.Warning($"Tried to set invalid skinID {skinID}");
		}
		else
		{
			Utils.SwapMaterial(stickMeshRenderer, stickSkin.Material);
		}
	}

	public void SetShaftTapeID(int shaftTapeID)
	{
		if (shaftTapeID == -1)
		{
			shaftTapeGameObject.SetActive(value: false);
			return;
		}
		StickTape stickTape = shaftTapes.Find((StickTape t) => t.ID == shaftTapeID && t.Material != null);
		if (stickTape == null)
		{
			Logger.Warning($"Tried to set invalid shaftTapeID {shaftTapeID}");
			return;
		}
		shaftTapeGameObject.SetActive(value: true);
		Utils.SwapMaterial(shaftTapeMeshRenderer, stickTape.Material);
	}

	public void SetBladeTapeID(int bladeTapeID)
	{
		if (bladeTapeID == -1)
		{
			bladeTapeGameObject.SetActive(value: false);
			return;
		}
		StickTape stickTape = bladeTapes.Find((StickTape t) => t.ID == bladeTapeID && t.Material != null);
		if (stickTape == null)
		{
			Logger.Warning($"Tried to set invalid bladeTapeID {bladeTapeID}");
			return;
		}
		bladeTapeGameObject.SetActive(value: true);
		Utils.SwapMaterial(bladeTapeMeshRenderer, stickTape.Material);
	}
}
