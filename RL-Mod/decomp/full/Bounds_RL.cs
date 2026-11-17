using UnityEngine;

public class Bounds_RL : MonoBehaviour
{
	[SerializeField]
	private Collider2D[] m_boundsArray;

	[SerializeField]
	private Rigidbody2D m_rigidbody;

	private int m_activeBoundsIndex;

	public Collider2D Collider => m_boundsArray[m_activeBoundsIndex];

	public Bounds Bounds => Collider.bounds;

	public Rigidbody2D Rigidbody => m_rigidbody;

	private void Awake()
	{
		Collider2D[] boundsArray = m_boundsArray;
		for (int i = 0; i < boundsArray.Length; i++)
		{
			boundsArray[i].gameObject.layer = 2;
		}
	}

	public void ChangeBoundsIndex(int index)
	{
		m_activeBoundsIndex = index;
	}
}
