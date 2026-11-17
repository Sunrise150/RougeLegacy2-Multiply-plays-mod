using System;
using System.Collections;
using UnityEngine;

public class TallSpikesHitboxExtender : MonoBehaviour
{
	private static Vector2[] m_pointArrayHelper_STATIC = new Vector2[4];

	[SerializeField]
	private Vector2 m_platformDeltaSize;

	[SerializeField]
	private Vector2 m_hazardDeltaSize;

	[SerializeField]
	private float m_platformYOffset;

	[SerializeField]
	private float m_hazardYOffset;

	private IEnumerator Start()
	{
		IHitboxController hbController = GetComponentInChildren<IHitboxController>();
		while (!hbController.IsInitialized)
		{
			yield return null;
		}
		Ferr2DT_PathTerrain component = GetComponent<Ferr2DT_PathTerrain>();
		if (!(component != null))
		{
			yield break;
		}
		if (component.PathData.Count == 2)
		{
			float angle = CDGHelper.WrapAngleDegrees(CDGHelper.AngleBetweenPts(component.PathData.End, component.PathData.Start), convertToHalfRadius: true);
			Vector2 inDirection = component.PathData.End - component.PathData.Start;
			inDirection = Vector2.Perpendicular(inDirection).normalized;
			PolygonCollider2D polygonCollider2D = hbController.GetCollider(HitboxType.Platform) as PolygonCollider2D;
			BoxCollider2D boxCollider2D = hbController.GetCollider(HitboxType.Platform) as BoxCollider2D;
			if ((bool)polygonCollider2D && polygonCollider2D.GetTotalPointCount() == 4)
			{
				ChangePolyColliderSize(polygonCollider2D, m_platformDeltaSize, angle, inDirection * (0f - m_platformYOffset));
			}
			else if ((bool)boxCollider2D)
			{
				ChangeBoxColliderSize(boxCollider2D, m_platformDeltaSize, m_platformYOffset, angle);
			}
			else
			{
				Debug.Log("<color=yellow>Could not find valid Platform collider for TallSpike: " + base.name + "</color>");
			}
			polygonCollider2D = hbController.GetCollider(HitboxType.Terrain) as PolygonCollider2D;
			boxCollider2D = hbController.GetCollider(HitboxType.Terrain) as BoxCollider2D;
			if ((bool)polygonCollider2D && polygonCollider2D.GetTotalPointCount() == 4)
			{
				ChangePolyColliderSize(polygonCollider2D, m_hazardDeltaSize, angle, inDirection * (0f - m_hazardYOffset));
			}
			else if ((bool)boxCollider2D)
			{
				ChangeBoxColliderSize(boxCollider2D, m_hazardDeltaSize, m_hazardYOffset, angle);
			}
			else
			{
				Debug.Log("<color=yellow>Could not find valid Terrain collider for TallSpike: " + base.name + "</color>");
			}
		}
		else
		{
			Debug.Log("<color=yellow>TallSpike: " + base.name + " does not contain a Ferr2D component.</color>");
		}
	}

	private void ChangePolyColliderSize(PolygonCollider2D polyCollider, Vector2 deltaSize, float angle, Vector2 offsetVector)
	{
		Vector2 theOrigin = polyCollider.bounds.center - polyCollider.transform.position;
		Array.Clear(m_pointArrayHelper_STATIC, 0, m_pointArrayHelper_STATIC.Length);
		float num = deltaSize.x * 0.5f;
		float num2 = deltaSize.y * 0.5f;
		for (int i = 0; i < polyCollider.GetTotalPointCount(); i++)
		{
			Vector2 thePoint = CDGHelper.RotatedPoint(polyCollider.points[i], theOrigin, 0f - angle);
			if (i == 0 || i == 3)
			{
				thePoint.x += num;
			}
			else
			{
				thePoint.x -= num;
			}
			if (i == 0 || i == 1)
			{
				thePoint.y += num2;
			}
			else
			{
				thePoint.y -= num2;
			}
			thePoint = CDGHelper.RotatedPoint(thePoint, theOrigin, angle);
			thePoint += offsetVector;
			m_pointArrayHelper_STATIC[i] = thePoint;
		}
		polyCollider.SetPath(0, m_pointArrayHelper_STATIC);
	}

	private void ChangeBoxColliderSize(BoxCollider2D boxCollider, Vector2 deltaSize, float yOffset, float angle)
	{
		if (Mathf.Approximately(angle, 90f) || Mathf.Approximately(angle, -90f))
		{
			Vector2 size = boxCollider.size;
			size.y += deltaSize.x;
			size.x += deltaSize.y;
			boxCollider.size = size;
			if (Mathf.Approximately(angle, -90f))
			{
				Vector2 offset = boxCollider.offset;
				offset.x += yOffset;
				boxCollider.offset = offset;
			}
			else
			{
				Vector2 offset2 = boxCollider.offset;
				offset2.x -= yOffset;
				boxCollider.offset = offset2;
			}
		}
		else
		{
			Vector2 size2 = boxCollider.size;
			size2.x += deltaSize.x;
			size2.y += deltaSize.y;
			boxCollider.size = size2;
			if (Mathf.Approximately(angle, 0f))
			{
				Vector2 offset3 = boxCollider.offset;
				offset3.y += yOffset;
				boxCollider.offset = offset3;
			}
			else
			{
				Vector2 offset4 = boxCollider.offset;
				offset4.y -= yOffset;
				boxCollider.offset = offset4;
			}
		}
	}
}
