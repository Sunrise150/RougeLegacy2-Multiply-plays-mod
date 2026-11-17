using UnityEngine;

[ExecuteInEditMode]
public class AngryOnHit_Effect : MonoBehaviour
{
	[Range(0f, 1f)]
	public float Amount;

	[SerializeField]
	private Color m_fullAngryColor;

	private Material m_material;

	private float m_previousAmount;

	private void OnEnable()
	{
		if (m_material == null)
		{
			m_material = new Material(Shader.Find("Ferr/2D Terrain/Unlit/Tinted Textured Vertex Color"));
		}
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (Amount == 0f && m_previousAmount == 0f)
		{
			base.enabled = false;
		}
		if (Amount != m_previousAmount)
		{
			m_material.SetColor("_Color", Color.Lerp(Color.white, m_fullAngryColor, Amount));
		}
		m_previousAmount = Amount;
		Graphics.Blit(source, destination, m_material);
	}
}
