using System;
using System.Collections.Generic;
using UnityEngine;

public class ShrinkCollider : MonoBehaviour
{
	private static List<Vector2> m_colliderPointsHelper_STATIC = new List<Vector2>();

	[SerializeField]
	private float m_shrinkAmount;

	[SerializeField]
	private bool m_shrinkByPercent;

	private void Start()
	{
		Shrink();
	}

	public void Shrink()
	{
		bool flag = false;
		Ferr2DT_PathTerrain component = GetComponent<Ferr2DT_PathTerrain>();
		if ((bool)component)
		{
			component.RecreateCollider();
		}
		PolygonCollider2D component2 = GetComponent<PolygonCollider2D>();
		if ((bool)component2)
		{
			if (component2.pathCount > 1)
			{
				Debug.Log("ShrinkPolygonCollider - Could not shrink polygon collider.  Collider had more than one path. Please convert to simple polygon first.");
				return;
			}
			Vector2 offset = component2.offset;
			Vector2[] points = component2.points;
			m_colliderPointsHelper_STATIC.Clear();
			m_colliderPointsHelper_STATIC.AddRange(points);
			for (int i = 0; i < points.Length; i++)
			{
				int index = ((i <= 0) ? (points.Length - 1) : (i - 1));
				Vector2 inverseNormalVector = GetInverseNormalVector(points, component2, index);
				Vector2 inverseNormalVector2 = GetInverseNormalVector(points, component2, i);
				Vector2 value = points[i];
				Vector2 normalized = (inverseNormalVector2 + inverseNormalVector).normalized;
				value -= offset;
				float num = Math.Abs(value.x);
				float num2 = Math.Abs(value.y);
				float value2 = ((!m_shrinkByPercent) ? m_shrinkAmount : (num * m_shrinkAmount));
				value2 = Mathf.Clamp(value2, 0f, num);
				value2 *= normalized.x;
				float value3 = ((!m_shrinkByPercent) ? m_shrinkAmount : (num2 * m_shrinkAmount));
				value3 = Mathf.Clamp(value3, 0f, num2);
				value3 *= normalized.y;
				value += offset;
				value.x += value2;
				value.y += value3;
				m_colliderPointsHelper_STATIC[i] = value;
			}
			component2.SetPath(0, m_colliderPointsHelper_STATIC);
			flag = true;
		}
		else
		{
			BoxCollider2D component3 = GetComponent<BoxCollider2D>();
			if (component3 != null)
			{
				Vector2 size = component3.size;
				float num3 = ((!m_shrinkByPercent) ? m_shrinkAmount : (size.x * m_shrinkAmount));
				float num4 = ((!m_shrinkByPercent) ? m_shrinkAmount : (size.y * m_shrinkAmount));
				size.x -= num3;
				size.y -= num4;
				component3.size = size;
				flag = true;
			}
		}
		if (!flag)
		{
			Debug.Log("<color=red>TEDDY: NO COLLIDER FOUND ON HIDDEN WALL. THIS MEANS ALL EDGES ARE SET TO 1 ON THE FERR2D. FIX THIS!!! DO NOT IGNORE THIS MESSAGE!!!</color>");
		}
	}

	private Vector2 GetInverseNormalVector(Vector2[] vertices, PolygonCollider2D collider, int index)
	{
		Vector2 vector = vertices[index];
		index++;
		if (index >= vertices.Length)
		{
			index = 0;
		}
		Vector2 vector2 = vertices[index];
		Vector2 vector3 = (vector2 + vector) / 2f;
		Vector2 normalized = Vector2.Perpendicular(vector2 - vector).normalized;
		if (collider.OverlapPoint((Vector2)collider.transform.position + vector3 + normalized))
		{
			return normalized;
		}
		return -normalized;
	}
}
