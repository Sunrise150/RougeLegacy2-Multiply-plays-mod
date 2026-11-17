using UnityEngine;

public class MidpointObj : MonoBehaviour, IMidpointObj
{
	[SerializeField]
	private bool m_usePositionOffset;

	[SerializeField]
	private Vector3 m_positionOffset;

	private Renderer m_renderer;

	private Collider2D m_collider;

	private BaseCharacterController m_charController;

	public Vector3 Midpoint
	{
		get
		{
			if (m_usePositionOffset)
			{
				return base.gameObject.transform.position + m_positionOffset;
			}
			if (m_charController != null)
			{
				return m_charController.CollisionBounds.center;
			}
			if (m_collider != null)
			{
				return m_collider.bounds.center;
			}
			if (m_renderer != null)
			{
				return m_renderer.bounds.center;
			}
			return base.transform.position;
		}
	}

	private void Awake()
	{
		GameObject root = this.GetRoot();
		m_charController = root.GetComponentInChildren<BaseCharacterController>();
		if (m_charController == null)
		{
			m_collider = root.GetComponentInChildren<Collider2D>();
			if (m_collider == null)
			{
				m_renderer = root.GetComponentInChildren<Renderer>();
			}
		}
	}
}
