using UnityEngine;

public class Colorblindness_Effect : MonoBehaviour
{
	private enum ColorblindType
	{
		Protanopia,
		Deuteranopia,
		Tritanopia
	}

	[SerializeField]
	private ColorblindType m_colorblindType;

	private Material m_material;

	private void Awake()
	{
		m_material = new Material(Shader.Find("RL2/Colorblindness"));
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		m_material.SetInt("_CBType", (int)m_colorblindType);
		Graphics.Blit(source, destination, m_material);
	}
}
