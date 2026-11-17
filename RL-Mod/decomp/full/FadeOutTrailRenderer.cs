using UnityEngine;

public class FadeOutTrailRenderer : MonoBehaviour
{
	[SerializeField]
	private bool m_resetAlphaOnEnable;

	private MaterialPropertyBlock m_matBlock;

	private TrailRenderer m_trailRenderer;

	private void Awake()
	{
		m_matBlock = new MaterialPropertyBlock();
		m_trailRenderer = GetComponentInChildren<TrailRenderer>();
	}

	public void OnEnable()
	{
		if ((bool)m_trailRenderer && m_resetAlphaOnEnable)
		{
			m_trailRenderer.GetPropertyBlock(m_matBlock);
			m_matBlock.SetFloat(ShaderID_RL._Opacity, 1f);
			m_trailRenderer.SetPropertyBlock(m_matBlock);
		}
	}

	public void SetOpacity(float opacity)
	{
		if ((bool)m_trailRenderer)
		{
			m_trailRenderer.GetPropertyBlock(m_matBlock);
			m_matBlock.SetFloat(ShaderID_RL._Opacity, opacity);
			m_trailRenderer.SetPropertyBlock(m_matBlock);
		}
	}
}
