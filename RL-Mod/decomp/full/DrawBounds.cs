using UnityEngine;

public class DrawBounds : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer m_spriteRenderer;

	private void OnDrawGizmosSelected()
	{
		if (m_spriteRenderer != null)
		{
			Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
			Gizmos.DrawCube(m_spriteRenderer.bounds.center, m_spriteRenderer.bounds.size);
		}
	}
}
