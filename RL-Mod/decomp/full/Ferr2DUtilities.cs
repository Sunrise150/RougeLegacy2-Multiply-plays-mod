using System;
using System.Collections.Generic;
using ClipperLibFerr;
using Ferr;
using UnityEngine;

public static class Ferr2DUtilities
{
	public struct CDGFerr2D_EdgeData
	{
		public Ferr2D_PointData pointData;

		public IntPoint leftPoint;

		public IntPoint rightPoint;
	}

	public enum Ferr2DUndoType
	{
		None,
		Destroy,
		DestroyImmediate
	}

	private static List<IntPoint> m_subjIPList = new List<IntPoint>();

	private static List<IntPoint> m_clipIPList = new List<IntPoint>();

	private static List<CDGFerr2D_EdgeData> m_edgeDataList = new List<CDGFerr2D_EdgeData>();

	private static Clipper m_clipper = new Clipper();

	private static ClipperOffset m_clipperOffset = new ClipperOffset();

	private static List<List<IntPoint>> m_ipResultList = new List<List<IntPoint>>();

	private static List<Collider2D> m_daisyChainedColliderList = new List<Collider2D>();

	private static List<Ferr2DT_PathTerrain> m_terrainCollList = new List<Ferr2DT_PathTerrain>();

	private static ContactFilter2D m_colliderFilter = default(ContactFilter2D);

	private static List<GameObject> m_boolNewObjList = new List<GameObject>();

	private static List<GameObject> m_newObjList = new List<GameObject>();

	private static List<Vector2> m_pointList = new List<Vector2>(10);

	private static List<Vector2> m_invertedPointList = new List<Vector2>(10);

	private static List<Ferr2D_PointData> m_pointDataList = new List<Ferr2D_PointData>(10);

	public static void ChangeMaterial(Ferr2DT_PathTerrain obj1, Ferr2DT_PathTerrain obj2)
	{
		obj1.SetMaterial(obj2.TerrainMaterial);
		obj1.Build();
	}

	public static List<GameObject> Boolean(ClipType clipType, Ferr2DT_PathTerrain subject, List<Ferr2DT_PathTerrain> clipList, bool roundToInt = true)
	{
		m_subjIPList.Clear();
		m_clipper.Clear();
		m_edgeDataList.Clear();
		m_newObjList.Clear();
		foreach (List<IntPoint> ipResult in m_ipResultList)
		{
			ipResult.Clear();
		}
		m_ipResultList.Clear();
		List<Vector2> finalPath = subject.PathData.GetFinalPath();
		Vector2 vector = subject.gameObject.transform.position;
		for (int i = 0; i < finalPath.Count; i++)
		{
			m_subjIPList.Add(new IntPoint((finalPath[i].x + vector.x) * 1000f, (finalPath[i].y + vector.y) * 1000f));
		}
		m_clipper.AddPath(m_subjIPList, PolyType.ptSubject, Closed: true);
		foreach (Ferr2DT_PathTerrain clip in clipList)
		{
			if (!clip.DisableMerging)
			{
				m_clipIPList.Clear();
				List<Vector2> finalPath2 = clip.PathData.GetFinalPath();
				Vector2 vector2 = clip.gameObject.transform.position;
				for (int j = 0; j < finalPath2.Count; j++)
				{
					m_clipIPList.Add(new IntPoint((finalPath2[j].x + vector2.x) * 1000f, (finalPath2[j].y + vector2.y) * 1000f));
				}
				m_clipper.AddPath(m_clipIPList, PolyType.ptClip, Closed: true);
			}
		}
		CDGFerr2D_EdgeData item = default(CDGFerr2D_EdgeData);
		if (m_clipper.Execute(clipType, m_ipResultList, PolyFillType.pftNonZero))
		{
			m_clipperOffset.Clear();
			m_clipperOffset.AddPaths(m_ipResultList, JoinType.jtSquare, EndType.etClosedPolygon);
			m_clipperOffset.Execute(ref m_ipResultList, 0.00019999999494757503);
			List<Ferr2D_PointData> data = subject.PathData.GetData();
			List<Vector2> finalPath3 = subject.PathData.GetFinalPath();
			for (int k = 0; k < data.Count; k++)
			{
				Ferr2D_PointData pointData = data[k];
				if (pointData.directionOverride != 100)
				{
					item.pointData = pointData;
					item.rightPoint = new IntPoint((finalPath3[k].x + vector.x) * 1000f, (finalPath3[k].y + vector.y) * 1000f);
					if (k < data.Count - 1)
					{
						item.leftPoint = new IntPoint((finalPath3[k + 1].x + vector.x) * 1000f, (finalPath3[k + 1].y + vector.y) * 1000f);
					}
					else
					{
						item.leftPoint = new IntPoint((finalPath3[0].x + vector.x) * 1000f, (finalPath3[0].y + vector.y) * 1000f);
					}
					m_edgeDataList.Add(item);
				}
			}
			for (int l = 0; l < m_ipResultList.Count; l++)
			{
				List<IntPoint> list = m_ipResultList[l];
				GameObject gameObject = null;
				gameObject = UnityEngine.Object.Instantiate(subject.gameObject);
				if (gameObject.transform.parent != subject.transform.parent)
				{
					gameObject.transform.parent = subject.transform.parent;
					gameObject.transform.position += subject.transform.parent.position;
				}
				m_newObjList.Add(gameObject);
				Ferr2DT_PathTerrain component = gameObject.GetComponent<Ferr2DT_PathTerrain>();
				component.ClearPoints();
				for (int m = 0; m < list.Count; m++)
				{
					IntPoint intPoint = list[m];
					Vector2 aPt = new Vector2((float)intPoint.X / 1000f - vector.x, (float)intPoint.Y / 1000f - vector.y);
					if (roundToInt)
					{
						aPt.x = Mathf.RoundToInt(aPt.x);
						aPt.y = Mathf.RoundToInt(aPt.y);
					}
					component.AddPoint(aPt);
				}
			}
			int num = 0;
			foreach (GameObject newObj in m_newObjList)
			{
				Ferr2DT_PathTerrain component2 = newObj.GetComponent<Ferr2DT_PathTerrain>();
				List<Vector2> finalPath4 = component2.PathData.GetFinalPath();
				for (int n = 0; n < finalPath4.Count; n++)
				{
					Vector2 vector3 = finalPath4[n];
					foreach (CDGFerr2D_EdgeData edgeData in m_edgeDataList)
					{
						Vector2 vector4 = new Vector2((float)edgeData.leftPoint.X / 1000f - vector.x, (float)edgeData.leftPoint.Y / 1000f - vector.y);
						if (roundToInt)
						{
							vector4.x = Mathf.RoundToInt(vector4.x);
							vector4.y = Mathf.RoundToInt(vector4.y);
						}
						if (vector3 == vector4)
						{
							if (n == 0)
							{
								component2.PathData.SetData(component2.PathData.Count - 1, edgeData.pointData);
							}
							else
							{
								component2.PathData.SetData(n - 1, edgeData.pointData);
							}
							continue;
						}
						vector4 = new Vector2((float)edgeData.rightPoint.X / 1000f - vector.x, (float)edgeData.rightPoint.Y / 1000f - vector.y);
						if (roundToInt)
						{
							vector4.x = Mathf.RoundToInt(vector4.x);
							vector4.y = Mathf.RoundToInt(vector4.y);
						}
						if (vector3 == vector4)
						{
							component2.PathData.SetData(n, edgeData.pointData);
						}
					}
				}
				if (num > 0)
				{
					newObj.name = subject.name + " (" + num + ")";
				}
				else
				{
					newObj.name = subject.name;
				}
				num++;
				bool rebuild = !Application.isPlaying;
				RecentreFerr2DTerrain(component2, rebuild);
			}
		}
		else
		{
			Debug.Log("Collision between Ferr2D objects was not found. CDGFerr2D.Boolean: " + clipType.ToString() + " failed.");
		}
		foreach (Ferr2DT_PathTerrain clip2 in clipList)
		{
			if (clip2.DisableMerging)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(clip2.gameObject);
				if (Application.isPlaying)
				{
					gameObject2.transform.parent = subject.transform.parent;
					gameObject2.transform.position = clip2.transform.position;
				}
				else if (gameObject2.transform.parent != subject.transform.parent)
				{
					gameObject2.transform.parent = subject.transform.parent;
					gameObject2.transform.position += subject.transform.parent.position;
				}
				m_newObjList.Add(gameObject2);
			}
		}
		return m_newObjList;
	}

	public static List<GameObject> EasyBoolean(ClipType clipType, List<Ferr2DT_PathTerrain> objsToCollide, Ferr2DUndoType undoType, bool roundToInt = true)
	{
		m_boolNewObjList.Clear();
		int layer = LayerMask.NameToLayer("HELPER_NOTOUCH");
		int layer2 = objsToCollide[0].gameObject.layer;
		foreach (Ferr2DT_PathTerrain item in objsToCollide)
		{
			if (item != null)
			{
				item.gameObject.layer = layer;
			}
		}
		m_colliderFilter.SetLayerMask(LayerMask.GetMask("HELPER_NOTOUCH"));
		m_colliderFilter.useTriggers = true;
		for (int i = 0; i < objsToCollide.Count; i++)
		{
			Ferr2DT_PathTerrain ferr2DT_PathTerrain = objsToCollide[i];
			if (!(ferr2DT_PathTerrain != null))
			{
				continue;
			}
			m_daisyChainedColliderList.Clear();
			m_terrainCollList.Clear();
			Collider2D component = ferr2DT_PathTerrain.GetComponent<Collider2D>();
			List<GameObject> list = null;
			if (component != null)
			{
				DaisyChainOverlapColliderRecursionLoop(component, m_daisyChainedColliderList, m_colliderFilter);
				foreach (Collider2D daisyChainedCollider in m_daisyChainedColliderList)
				{
					if (daisyChainedCollider != null && daisyChainedCollider != component)
					{
						Ferr2DT_PathTerrain component2 = daisyChainedCollider.GetComponent<Ferr2DT_PathTerrain>();
						if (component2 != null)
						{
							m_terrainCollList.Add(component2);
						}
					}
				}
				if (m_terrainCollList.Count > 0)
				{
					list = Boolean(clipType, ferr2DT_PathTerrain, m_terrainCollList, roundToInt);
				}
			}
			else
			{
				Debug.Log("Cannot perform Boolean. Subject: " + ferr2DT_PathTerrain.name + " has no collider.");
			}
			if (list != null && list.Count > 0)
			{
				switch (undoType)
				{
				case Ferr2DUndoType.Destroy:
					UnityEngine.Object.Destroy(ferr2DT_PathTerrain.gameObject);
					foreach (Collider2D daisyChainedCollider2 in m_daisyChainedColliderList)
					{
						if (daisyChainedCollider2 != null)
						{
							UnityEngine.Object.Destroy(daisyChainedCollider2.gameObject);
						}
					}
					break;
				case Ferr2DUndoType.DestroyImmediate:
					UnityEngine.Object.DestroyImmediate(ferr2DT_PathTerrain.gameObject);
					foreach (Collider2D daisyChainedCollider3 in m_daisyChainedColliderList)
					{
						if (daisyChainedCollider3 != null)
						{
							UnityEngine.Object.DestroyImmediate(daisyChainedCollider3.gameObject);
						}
					}
					break;
				}
			}
			if (list != null)
			{
				m_boolNewObjList.AddRange(list);
			}
		}
		foreach (Ferr2DT_PathTerrain item2 in objsToCollide)
		{
			if (item2 != null)
			{
				item2.gameObject.layer = layer2;
			}
		}
		foreach (GameObject boolNewObj in m_boolNewObjList)
		{
			if (boolNewObj != null)
			{
				boolNewObj.layer = layer2;
			}
		}
		return m_boolNewObjList;
	}

	public static List<GameObject> FastEasyBoolean(ClipType clipType, List<Ferr2DT_PathTerrain> clipList, Ferr2DUndoType undoType, bool roundToInt = true)
	{
		if (clipList != null && clipList.Count > 0)
		{
			Ferr2DT_PathTerrain subjectTerrain = clipList[0];
			return FastEasyBoolean(clipType, subjectTerrain, clipList, undoType, roundToInt);
		}
		throw new ArgumentNullException("<color=red>[Ferr2DUtilities] FastEasyBoolean was passed an emtpy clipList</color>");
	}

	public static List<GameObject> FastEasyBoolean(ClipType clipType, Ferr2DT_PathTerrain subjectTerrain, List<Ferr2DT_PathTerrain> clipList, Ferr2DUndoType undoType, bool roundToInt = true)
	{
		if (clipList == null)
		{
			throw new ArgumentNullException("objsToCollide");
		}
		if (clipList.Count == 0)
		{
			return null;
		}
		if (clipList.Contains(subjectTerrain))
		{
			clipList.Remove(subjectTerrain);
		}
		m_boolNewObjList.Clear();
		Ferr2DT_PathTerrain ferr2DT_PathTerrain = subjectTerrain;
		if (ferr2DT_PathTerrain != null)
		{
			Collider2D component = ferr2DT_PathTerrain.GetComponent<Collider2D>();
			List<GameObject> list = null;
			m_terrainCollList.Clear();
			if (component != null)
			{
				m_terrainCollList.AddRange(clipList);
				if (clipType != ClipType.ctDifference)
				{
					list = Boolean(clipType, ferr2DT_PathTerrain, m_terrainCollList, roundToInt);
				}
				else
				{
					List<Ferr2DT_PathTerrain> list2 = new List<Ferr2DT_PathTerrain>();
					list = new List<GameObject>();
					for (int i = 0; i < m_terrainCollList.Count; i++)
					{
						ferr2DT_PathTerrain = m_terrainCollList[i];
						list2.Clear();
						list2.Add(subjectTerrain);
						List<GameObject> collection = Boolean(clipType, ferr2DT_PathTerrain, list2, roundToInt);
						list.AddRange(collection);
					}
				}
				m_terrainCollList.Add(subjectTerrain);
			}
			else
			{
				Debug.Log("Cannot perform Boolean. Subject: " + ferr2DT_PathTerrain.name + " has no collider.");
			}
			if (list != null && list.Count > 0)
			{
				switch (undoType)
				{
				case Ferr2DUndoType.Destroy:
					UnityEngine.Object.Destroy(ferr2DT_PathTerrain.gameObject);
					foreach (Ferr2DT_PathTerrain terrainColl in m_terrainCollList)
					{
						if (terrainColl != null)
						{
							UnityEngine.Object.Destroy(terrainColl.gameObject);
						}
					}
					break;
				case Ferr2DUndoType.DestroyImmediate:
					UnityEngine.Object.DestroyImmediate(ferr2DT_PathTerrain.gameObject);
					foreach (Ferr2DT_PathTerrain terrainColl2 in m_terrainCollList)
					{
						if (terrainColl2 != null)
						{
							UnityEngine.Object.DestroyImmediate(terrainColl2.gameObject);
						}
					}
					break;
				}
			}
			if (list != null)
			{
				m_boolNewObjList.AddRange(list);
			}
		}
		return m_boolNewObjList;
	}

	private static void DaisyChainOverlapColliderRecursionLoop(Collider2D subject, List<Collider2D> listToModify, ContactFilter2D contactFilter)
	{
		Collider2D[] array = new Collider2D[10];
		subject.OverlapCollider(contactFilter, array);
		Collider2D[] array2 = array;
		foreach (Collider2D collider2D in array2)
		{
			if (collider2D != null && !listToModify.Contains(collider2D))
			{
				listToModify.Add(collider2D);
				DaisyChainOverlapColliderRecursionLoop(collider2D, listToModify, contactFilter);
			}
		}
	}

	public static bool RecentreFerr2DTerrain(Ferr2DT_PathTerrain terrain, bool rebuild, bool roundToInt = true)
	{
		if (terrain == null)
		{
			return false;
		}
		Vector2 vector = PathUtil.Average(terrain.PathData.GetPathRaw());
		Vector2 vector2 = vector;
		if (roundToInt)
		{
			vector2.x = Mathf.RoundToInt(vector2.x);
			vector2.y = Mathf.RoundToInt(vector2.y);
		}
		bool flag = vector2 != Vector2.zero;
		if (flag)
		{
			Vector3 vector3 = terrain.transform.position + (Vector3)vector;
			Vector3 vector4 = new Vector3((int)Math.Round(vector3.x), (int)Math.Round(vector3.y), vector3.z);
			vector -= (Vector2)(vector3 - vector4);
			terrain.transform.position = vector4;
			List<Vector2> pathRaw = terrain.PathData.GetPathRaw();
			for (int i = 0; i < pathRaw.Count; i++)
			{
				Vector2 aValue = pathRaw[i] - vector;
				if (roundToInt)
				{
					aValue.x = Mathf.RoundToInt(aValue.x);
					aValue.y = Mathf.RoundToInt(aValue.y);
				}
				terrain.PathData.Set(i, aValue);
			}
		}
		if (rebuild)
		{
			Bounds bounds = default(Bounds);
			if (!Application.isPlaying && !flag)
			{
				Collider2D component = terrain.GetComponent<Collider2D>();
				if (component != null)
				{
					bounds = component.bounds;
				}
			}
			terrain.Build();
			if (!Application.isPlaying && !flag)
			{
				Collider2D component2 = terrain.GetComponent<Collider2D>();
				if (component2 != null && component2.bounds != bounds)
				{
					flag = true;
				}
			}
		}
		return flag;
	}

	public static void Flip(Ferr2DT_PathTerrain terrain, bool flipHorizontally, bool flipVertically, bool recentre = true)
	{
		if (!flipHorizontally && !flipVertically)
		{
			return;
		}
		m_invertedPointList.Clear();
		m_pointDataList.Clear();
		m_pointList.Clear();
		if (recentre)
		{
			RecentreFerr2DTerrain(terrain, rebuild: false);
		}
		m_pointList.AddRange(terrain.PathData.GetFinalPath());
		m_pointDataList.AddRange(terrain.PathData.GetData());
		if (flipHorizontally)
		{
			for (int num = m_pointList.Count - 1; num >= 0; num--)
			{
				m_invertedPointList.Add(new Vector2(m_pointList[num].x * -1f, m_pointList[num].y));
			}
			if (flipVertically)
			{
				m_pointList.Clear();
				m_pointList.AddRange(m_invertedPointList);
				m_invertedPointList.Clear();
				for (int num2 = m_pointList.Count - 1; num2 >= 0; num2--)
				{
					m_invertedPointList.Add(new Vector2(m_pointList[num2].x, m_pointList[num2].y * -1f));
				}
			}
		}
		else if (flipVertically)
		{
			for (int num3 = m_pointList.Count - 1; num3 >= 0; num3--)
			{
				m_invertedPointList.Add(new Vector2(m_pointList[num3].x, m_pointList[num3].y * -1f));
			}
		}
		for (int i = 0; i < m_invertedPointList.Count; i++)
		{
			terrain.PathData.Set(i, m_invertedPointList[i]);
			int num4 = m_invertedPointList.Count - 2 - i;
			if (num4 < 0)
			{
				num4 += m_invertedPointList.Count;
			}
			terrain.PathData.SetData(i, m_pointDataList[num4]);
		}
		terrain.RecreateCollider();
	}
}
