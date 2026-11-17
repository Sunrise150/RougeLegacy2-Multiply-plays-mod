using UnityEngine;

[ExecuteInEditMode]
public class HeirloomWarp_Effect : MonoBehaviour
{
	public Texture WarpDistortionMap;

	public float WarpCenterX;

	public float WarpCenterY;

	public float DistortionAmount;

	private Material m_material;

	private float prevWarpCenterX;

	private float prevWarpCenterY;

	private float prevDistortionAmount;

	private void OnEnable()
	{
		if (m_material == null)
		{
			m_material = new Material(Shader.Find("RL2/PPS/HeirloomWarp"));
			m_material.SetTexture("_WarpDistortionMap", WarpDistortionMap);
		}
	}

	private void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		if (prevWarpCenterX != WarpCenterX)
		{
			m_material.SetFloat("_WarpCenterX", WarpCenterX);
		}
		prevWarpCenterX = WarpCenterX;
		if (prevWarpCenterY != WarpCenterY)
		{
			m_material.SetFloat("_WarpCenterY", WarpCenterY);
		}
		prevWarpCenterY = WarpCenterY;
		if (prevDistortionAmount != DistortionAmount)
		{
			m_material.SetFloat("_DistortionAmount", DistortionAmount);
		}
		prevDistortionAmount = DistortionAmount;
		Graphics.Blit(src, dst, m_material);
	}
}
