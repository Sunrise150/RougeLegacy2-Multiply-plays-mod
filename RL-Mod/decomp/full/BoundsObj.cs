using System.Collections;
using UnityEngine;

public class BoundsObj : MonoBehaviour
{
	[SerializeField]
	private bool m_drawBounds;

	private bool m_boundsSet;

	public float Top
	{
		get
		{
			if (m_boundsSet)
			{
				return Bounds.max.y;
			}
			return base.transform.position.y;
		}
	}

	public float Bottom
	{
		get
		{
			if (m_boundsSet)
			{
				return Bounds.center.y - Bounds.extents.y;
			}
			return base.transform.position.y;
		}
	}

	public float Left
	{
		get
		{
			if (m_boundsSet)
			{
				return Bounds.center.x - Bounds.extents.x;
			}
			return base.transform.position.x;
		}
	}

	public float Right
	{
		get
		{
			if (m_boundsSet)
			{
				return Bounds.center.x + Bounds.extents.x;
			}
			return base.transform.position.x;
		}
	}

	public Vector3 Center
	{
		get
		{
			if (m_boundsSet)
			{
				return Bounds.center;
			}
			return base.transform.position;
		}
	}

	public Bounds Bounds { get; private set; }

	private IEnumerator Start()
	{
		GameObject rootObj = this.GetRoot();
		IHitboxController hbController = rootObj.GetComponentInChildren<IHitboxController>();
		yield return new WaitUntil(() => hbController == null || hbController.IsInitialized);
		if (!m_boundsSet)
		{
			Collider2D componentInChildren = rootObj.GetComponentInChildren<Collider2D>();
			if (componentInChildren != null)
			{
				Bounds = componentInChildren.bounds;
				m_boundsSet = true;
			}
		}
		if (!m_boundsSet)
		{
			Renderer componentInChildren2 = rootObj.GetComponentInChildren<Renderer>();
			if (componentInChildren2 != null)
			{
				m_boundsSet = true;
				Bounds = componentInChildren2.bounds;
			}
		}
	}
}
