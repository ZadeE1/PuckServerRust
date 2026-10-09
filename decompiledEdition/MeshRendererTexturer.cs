using UnityEngine;

[ExecuteInEditMode]
public class MeshRendererTexturer : MonoBehaviour
{
	[Header("References")]
	[SerializeField]
	private MeshRenderer meshRenderer;

	[SerializeField]
	private string texturePropertyName = "_BaseMap";

	[SerializeField]
	private int materialIndex;

	private MaterialPropertyBlock propertyBlock;

	private void Awake()
	{
		if (!meshRenderer)
		{
			meshRenderer = GetComponent<MeshRenderer>();
		}
	}

	public void SetTexture(Texture texture)
	{
		if ((bool)meshRenderer)
		{
			if (propertyBlock == null)
			{
				propertyBlock = new MaterialPropertyBlock();
			}
			meshRenderer.GetPropertyBlock(propertyBlock, materialIndex);
			if (texture != null)
			{
				propertyBlock.SetTexture(texturePropertyName, texture);
			}
			else
			{
				propertyBlock.Clear();
			}
			meshRenderer.SetPropertyBlock(propertyBlock, materialIndex);
		}
	}
}
