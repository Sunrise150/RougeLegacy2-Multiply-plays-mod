using UnityEngine;

public class ClearTrailRenderer : MonoBehaviour
{
	private TrailRenderer m_trailRenderer;

	private void Awake()
	{
		m_trailRenderer = GetComponent<TrailRenderer>();
	}

	private void OnDisable()
	{
		if ((bool)m_trailRenderer)
		{
			m_trailRenderer.Clear();
		}
	}
}
