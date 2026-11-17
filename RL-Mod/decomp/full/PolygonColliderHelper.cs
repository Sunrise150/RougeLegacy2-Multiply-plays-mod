using System.Collections.Generic;
using UnityEngine;

public class PolygonColliderHelper : MonoBehaviour
{
	[SerializeField]
	private CompositeCollider2D m_compositeCollider;

	[SerializeField]
	private PolygonCollider2D[] m_debugPolygonColliders;

	private static List<PolygonCollider2D> m_colliders;

	private static PolygonColliderHelper m_instance;

	public static CompositeCollider2D CompositeCollider
	{
		get
		{
			return Instance.m_compositeCollider;
		}
		set
		{
			Instance.m_compositeCollider = value;
		}
	}

	private static PolygonColliderHelper Instance
	{
		get
		{
			return m_instance;
		}
		set
		{
			m_instance = value;
		}
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		m_instance = null;
		if (m_colliders != null)
		{
			for (int num = m_colliders.Count - 1; num >= 0; num--)
			{
				Object.Destroy(m_colliders[num]);
			}
			m_colliders.Clear();
			m_colliders = null;
		}
	}

	public static void UpdateCompositeCollider(PolygonCollider2D[] collider2Ds)
	{
		if (m_colliders == null)
		{
			m_colliders = new List<PolygonCollider2D>();
		}
		if (m_colliders.Count < collider2Ds.Length)
		{
			int num = collider2Ds.Length - m_colliders.Count;
			for (int i = 0; i < num; i++)
			{
				GameObject obj = new GameObject("Collider");
				obj.transform.SetParent(Instance.transform);
				obj.transform.localPosition = Vector3.zero;
				obj.AddComponent<PolygonCollider2D>();
				PolygonCollider2D component = obj.GetComponent<PolygonCollider2D>();
				component.usedByComposite = true;
				component.isTrigger = true;
				m_colliders.Add(component);
				obj.gameObject.SetActive(value: false);
			}
		}
		for (int j = 0; j < collider2Ds.Length; j++)
		{
			m_colliders[j].transform.localPosition = collider2Ds[j].transform.localPosition;
			m_colliders[j].gameObject.SetActive(value: true);
			m_colliders[j].SetPath(0, collider2Ds[j].points);
		}
		Instance.m_compositeCollider.GenerateGeometry();
		for (int k = 0; k < m_colliders.Count; k++)
		{
			m_colliders[k].gameObject.SetActive(value: false);
		}
	}
}
