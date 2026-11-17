using UnityEngine;

public class CaveLanternFadeInController : MonoBehaviour
{
	private BoxCollider2D m_collider;

	private bool m_effectEnabled;

	private void Awake()
	{
		m_collider = GetComponent<BoxCollider2D>();
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if ((bool)CaveLanternPostProcessingController.Instance && collision.CompareTag("Player"))
		{
			if (!m_effectEnabled)
			{
				CaveLanternPostProcessingController.EnableCaveLanternEffect();
				m_effectEnabled = true;
			}
			float y = m_collider.bounds.max.y;
			float y2 = m_collider.size.y;
			CaveLanternPostProcessingController.SetDimnessPercent(Mathf.Abs(PlayerManager.GetPlayerController().Midpoint.y - y) / y2);
		}
	}

	private void OnDisable()
	{
		if (m_effectEnabled && (bool)CaveLanternPostProcessingController.Instance)
		{
			CaveLanternPostProcessingController.DisableCaveLanternEffect();
		}
		m_effectEnabled = false;
	}
}
