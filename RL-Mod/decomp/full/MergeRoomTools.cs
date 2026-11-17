using System;
using System.Collections.Generic;
using System.Linq;
using ClipperLibFerr;
using Ferr;
using UnityEngine;

public static class MergeRoomTools
{
	private static int m_mergeRoomNumber = 0;

	private const int DIRECTION_OVERRIDE_DEFAULT_VALUE = 100;

	private static List<GridPoint> m_gridPointsHelper = new List<GridPoint>();

	private static List<Vector2Int> m_gridPointsToFillHelper = new List<Vector2Int>();

	private static List<Ferr2DT_PathTerrain> m_decoTerrainHelper = new List<Ferr2DT_PathTerrain>();

	private static List<Vector2> m_pointsList = new List<Vector2>();

	private static List<Ferr2DT_PathTerrain> m_mergeTerrainHelper = new List<Ferr2DT_PathTerrain>();

	private static List<Ferr2DT_PathTerrain> m_normalTerrainHelper = new List<Ferr2DT_PathTerrain>();

	private static List<Ferr2DT_PathTerrain> m_invertedTerrainHelper = new List<Ferr2DT_PathTerrain>();

	public static GameObject Merge(List<Room> roomsToMerge)
	{
		if (roomsToMerge == null)
		{
			throw new ArgumentNullException("roomsToMerge");
		}
		if (roomsToMerge.Count == 0 || roomsToMerge.Count == 1)
		{
			throw new ArgumentException("roomsToMerge", "List should contain at least 2 Rooms");
		}
		if (roomsToMerge.Any((Room room) => (object)room == null))
		{
			throw new ArgumentException("roomsToMerge", "Rooms must be of Type Room");
		}
		MergeRoom mergeRoom = CreateMergeRoom();
		mergeRoom.AddRooms(roomsToMerge);
		return mergeRoom.gameObject;
	}

	private static List<Vector2Int> GetGridPointCoordinatesToFill(MergeRoom mergeRoom)
	{
		m_gridPointsHelper.Clear();
		GridPointManager[] standaloneGridPointManagers = mergeRoom.StandaloneGridPointManagers;
		for (int k = 0; k < standaloneGridPointManagers.Length; k++)
		{
			GridPoint[,] gridPoints = standaloneGridPointManagers[k].GridPoints;
			foreach (GridPoint item in gridPoints)
			{
				m_gridPointsHelper.Add(item);
			}
		}
		int num = m_gridPointsHelper.Min((GridPoint g) => g.GridCoordinates.x);
		int num2 = m_gridPointsHelper.Min((GridPoint g) => g.GridCoordinates.y);
		int num3 = m_gridPointsHelper.Max((GridPoint g) => g.GridCoordinates.x) + 1;
		int num4 = m_gridPointsHelper.Max((GridPoint g) => g.GridCoordinates.y) + 1;
		m_gridPointsToFillHelper.Clear();
		for (int i = num; i < num3; i++)
		{
			int j;
			for (j = num2; j < num4; j++)
			{
				if (!m_gridPointsHelper.Any((GridPoint g) => g.GridCoordinates == new Vector2Int(i, j)))
				{
					m_gridPointsToFillHelper.Add(new Vector2Int(i, j));
				}
			}
		}
		return m_gridPointsToFillHelper;
	}

	public static List<Ferr2DT_PathTerrain> MergeTerrain(BaseRoom room)
	{
		BiomeType terrainBiomeType = ArtUtility.GetTerrainBiomeType(room);
		BiomeArtData biomeArtData = room.BiomeArtDataOverride;
		if (!biomeArtData)
		{
			biomeArtData = BiomeArtDataLibrary.GetArtData(terrainBiomeType);
		}
		m_decoTerrainHelper.Clear();
		if (room is Room)
		{
			m_decoTerrainHelper.AddRange(MergeTerrain(room.gameObject.transform, terrainBiomeType, biomeArtData, room.Bounds, room.TerrainManager.Platforms));
		}
		else
		{
			MergeRoom mergeRoom = room as MergeRoom;
			int num = 0;
			foreach (KeyValuePair<Transform, List<Ferr2DT_PathTerrain>> item in mergeRoom.StandaloneRoomToTerrainTable)
			{
				Bounds bounds = mergeRoom.StandaloneRoomBounds[num];
				m_decoTerrainHelper.AddRange(MergeTerrain(mergeRoom.gameObject.transform, terrainBiomeType, biomeArtData, bounds, item.Value));
				num++;
			}
			List<Vector2Int> gridPointCoordinatesToFill = GetGridPointCoordinatesToFill(mergeRoom);
			CreateFillDecoTerrain(m_decoTerrainHelper, mergeRoom, gridPointCoordinatesToFill);
			m_decoTerrainHelper = MergeDecoTerrain(mergeRoom, m_decoTerrainHelper);
		}
		foreach (Ferr2DT_PathTerrain item2 in m_decoTerrainHelper)
		{
			BiomeCreatorTools.SetOuterEdges(item2, room.Bounds);
			item2.BuildMeshOnly();
		}
		return m_decoTerrainHelper;
	}

	private static void CreateFillDecoTerrain(List<Ferr2DT_PathTerrain> decoTerrain, MergeRoom mergeRoom, List<Vector2Int> gridPointCoordsToFill)
	{
		BiomeArtData artData = BiomeArtDataLibrary.GetArtData(mergeRoom.AppearanceBiomeType);
		foreach (Vector2Int item in gridPointCoordsToFill)
		{
			Bounds gridPointBounds = GridController.GetGridPointBounds(item);
			m_pointsList.Clear();
			m_pointsList.Add(new Vector2(gridPointBounds.min.x, gridPointBounds.min.y));
			m_pointsList.Add(new Vector2(gridPointBounds.max.x, gridPointBounds.min.y));
			m_pointsList.Add(new Vector2(gridPointBounds.max.x, gridPointBounds.max.y));
			m_pointsList.Add(new Vector2(gridPointBounds.min.x, gridPointBounds.max.y));
			decoTerrain.Add(CreateDecorativeTerrain(mergeRoom.gameObject.transform, artData, gridPointBounds, m_pointsList, isInvertedTerrain: false));
		}
	}

	private static List<Ferr2DT_PathTerrain> MergeDecoTerrain(MergeRoom mergeRoom, List<Ferr2DT_PathTerrain> terrainToMerge)
	{
		BiomeArtData artData = BiomeArtDataLibrary.GetArtData(mergeRoom.AppearanceBiomeType);
		List<Ferr2DT_PathTerrain> list = new List<Ferr2DT_PathTerrain>();
		if (terrainToMerge.Count > 0)
		{
			List<List<Vector2>> union = GetUnion(GetTerrainPoints(terrainToMerge));
			for (int i = 0; i < union.Count; i++)
			{
				Ferr2DT_PathTerrain item = CreateDecorativeTerrain(mergeRoom.gameObject.transform, artData, mergeRoom.Bounds, union[i], isInvertedTerrain: false);
				list.Add(item);
			}
		}
		return list;
	}

	private static List<Ferr2DT_PathTerrain> MergeTerrain(Transform parent, BiomeType biome, BiomeArtData biomeArtData, Bounds bounds, IEnumerable<Ferr2DT_PathTerrain> terrainToMerge)
	{
		m_mergeTerrainHelper.Clear();
		m_normalTerrainHelper.Clear();
		m_invertedTerrainHelper.Clear();
		foreach (Ferr2DT_PathTerrain item in terrainToMerge)
		{
			ISpawnController component = item.GetComponent<ISpawnController>();
			if (component == null || component.ShouldSpawn)
			{
				if (item.DisableMerging)
				{
					m_mergeTerrainHelper.Add(item);
				}
				else if (item.FillMode == Ferr2D_SectionMode.Normal)
				{
					m_normalTerrainHelper.Add(item);
				}
				else if (item.FillMode == Ferr2D_SectionMode.Invert)
				{
					m_invertedTerrainHelper.Add(item);
				}
			}
		}
		SetDontMergeTerrainMaterial(biome, m_mergeTerrainHelper);
		m_mergeTerrainHelper.Clear();
		if (m_normalTerrainHelper.Count > 0)
		{
			m_mergeTerrainHelper.AddRange(DoMerge(parent, biomeArtData, bounds, m_normalTerrainHelper));
		}
		if (m_invertedTerrainHelper.Count > 0)
		{
			m_mergeTerrainHelper.AddRange(DoMerge(parent, biomeArtData, bounds, m_invertedTerrainHelper));
		}
		return m_mergeTerrainHelper;
	}

	private static void SetDontMergeTerrainMaterial(BiomeType biome, List<Ferr2DT_PathTerrain> dontMergeTerrain)
	{
		Ferr2DT_Material ferr2DT_Material = null;
		ferr2DT_Material = ((biome != BiomeType.Study) ? RoomPrefabLibrary.DefaultDontMergeFerr2DMaterial : RoomPrefabLibrary.StudyDontMergeFerr2DMaterial);
		BiomeArtData artData = BiomeArtDataLibrary.GetArtData(biome);
		foreach (Ferr2DT_PathTerrain item in dontMergeTerrain)
		{
			Ferr2DT_Material material = ferr2DT_Material;
			if (item.UseBiomeMaterial && artData != null && artData.Ferr2DBiomeArtData != null && artData.Ferr2DBiomeArtData.TerrainMaster != null && artData.Ferr2DBiomeArtData.TerrainMaster.Ferr2DSettings.Material != null)
			{
				material = BiomeArtDataLibrary.GetArtData(biome).Ferr2DBiomeArtData.TerrainMaster.Ferr2DSettings.Material;
			}
			if (!item.OverrideDisabledMergeMaterial)
			{
				item.SetMaterial(material);
			}
			item.BuildMeshOnly();
		}
	}

	private static List<Ferr2DT_PathTerrain> DoMerge(Transform parent, BiomeArtData biomeArtData, Bounds bounds, List<Ferr2DT_PathTerrain> terrainToMerge)
	{
		bool isInvertedTerrain = terrainToMerge[0].FillMode == Ferr2D_SectionMode.Invert;
		List<List<Vector2>> union = GetUnion(GetTerrainPoints(terrainToMerge));
		Dictionary<Line_RL, Ferr2D_PointData> directionOverrides = GetDirectionOverrides(terrainToMerge);
		ApplyMissingEdgesToUnionResult(union, directionOverrides);
		List<Ferr2DT_PathTerrain> list = new List<Ferr2DT_PathTerrain>();
		for (int i = 0; i < union.Count; i++)
		{
			Ferr2DT_PathTerrain item = CreateDecorativeTerrain(parent, biomeArtData, bounds, union[i], isInvertedTerrain, directionOverrides);
			list.Add(item);
		}
		return list;
	}

	private static void ApplyMissingEdgesToUnionResult(List<List<Vector2>> unionResult, Dictionary<Line_RL, Ferr2D_PointData> edgeOverrides)
	{
		if (edgeOverrides == null)
		{
			return;
		}
		foreach (List<Vector2> item in unionResult)
		{
			for (int i = 0; i < item.Count; i++)
			{
				int index = ((i - 1 < 0) ? (item.Count - 1) : (i - 1));
				int index2 = ((i + 1 < item.Count) ? (i + 1) : 0);
				Vector2 vector = item[i];
				Vector2 vector2 = item[index];
				Vector2 vector3 = item[index2];
				foreach (KeyValuePair<Line_RL, Ferr2D_PointData> edgeOverride in edgeOverrides)
				{
					Line_RL key = edgeOverride.Key;
					if (key.PointB == vector && key.PointA != vector2)
					{
						float num = CDGHelper.AngleBetweenPts(key.PointB, key.PointA);
						float num2 = CDGHelper.AngleBetweenPts(vector, vector2);
						if (num == num2)
						{
							item.Insert(i, key.PointA);
							i++;
						}
					}
					else if (key.PointA == vector && key.PointB != vector3)
					{
						float num3 = CDGHelper.AngleBetweenPts(key.PointA, key.PointB);
						float num4 = CDGHelper.AngleBetweenPts(vector, vector3);
						if (num3 == num4)
						{
							item.Insert(i + 1, key.PointB);
						}
					}
				}
			}
		}
	}

	private static Dictionary<Line_RL, Ferr2D_PointData> GetDirectionOverrides(List<Ferr2DT_PathTerrain> terrainToMerge)
	{
		Dictionary<Line_RL, Ferr2D_PointData> dictionary = new Dictionary<Line_RL, Ferr2D_PointData>();
		for (int i = 0; i < terrainToMerge.Count; i++)
		{
			Ferr2DT_PathTerrain ferr2DT_PathTerrain = terrainToMerge[i];
			List<Vector2> points = ferr2DT_PathTerrain.PathData.GetPoints(0);
			for (int j = 0; j < points.Count; j++)
			{
				Ferr2D_PointData data = ferr2DT_PathTerrain.PathData.GetData(j);
				int num = j + 1;
				if (j == points.Count - 1)
				{
					num = 0;
				}
				ferr2DT_PathTerrain.PathData.GetData(num);
				if (data.directionOverride != 100)
				{
					Vector2 pointInWorldSpaceCoordinates = GetPointInWorldSpaceCoordinates(points[j], ferr2DT_PathTerrain.transform);
					Vector2 pointInWorldSpaceCoordinates2 = GetPointInWorldSpaceCoordinates(points[num], ferr2DT_PathTerrain.transform);
					Line_RL key = new Line_RL(pointInWorldSpaceCoordinates, pointInWorldSpaceCoordinates2);
					if (!dictionary.ContainsKey(key))
					{
						dictionary.Add(key, data);
					}
				}
			}
		}
		return dictionary;
	}

	private static List<List<Vector2>> GetTerrainPoints(IEnumerable<Ferr2DT_PathTerrain> terrainToMerge)
	{
		List<List<Vector2>> list = new List<List<Vector2>>();
		foreach (Ferr2DT_PathTerrain item in terrainToMerge)
		{
			Vector3 localPosition = item.transform.localPosition;
			Vector3 localPosition2 = new Vector3(Mathf.RoundToInt(localPosition.x), Mathf.RoundToInt(localPosition.y), localPosition.z);
			item.transform.localPosition = localPosition2;
			item.GetComponent<Renderer>().enabled = false;
			list.Add(GetPoints(item));
		}
		return list;
	}

	private static List<Vector2> GetPoints(Ferr2DT_PathTerrain terrain)
	{
		List<Vector2> points = terrain.PathData.GetPoints(0);
		List<Vector2> list = new List<Vector2>();
		for (int i = 0; i < points.Count; i++)
		{
			list.Add(GetPointInWorldSpaceCoordinates(points[i], terrain.transform));
		}
		return list;
	}

	private static Vector2 GetPointInWorldSpaceCoordinates(Vector2 localCoords, Transform transform)
	{
		int num = Mathf.RoundToInt(localCoords.x);
		int num2 = Mathf.RoundToInt(localCoords.y);
		return transform.TransformPoint(new Vector2(num, num2));
	}

	private static Ferr2DT_PathTerrain CreateDecorativeTerrain(Transform parent, BiomeArtData biomeArtData, Bounds bounds, List<Vector2> terrainPoints, bool isInvertedTerrain, Dictionary<Line_RL, Ferr2D_PointData> edgeOverrides = null)
	{
		Ferr2DT_PathTerrain ferr2DT_PathTerrain = UnityEngine.Object.Instantiate(RoomPrefabLibrary.DecoTerrainPrefab);
		ferr2DT_PathTerrain.ClearPoints();
		GameObject gameObject = ferr2DT_PathTerrain.gameObject;
		gameObject.transform.SetParent(parent);
		gameObject.transform.localPosition = Vector3.zero;
		BaseRoom component = parent.gameObject.GetComponent<BaseRoom>();
		ferr2DT_PathTerrain.PathData.Closed = true;
		ferr2DT_PathTerrain.pixelsPerUnit = biomeArtData.Ferr2DBiomeArtData.TerrainMaster.Ferr2DSettings.PixelsPerUnit;
		ferr2DT_PathTerrain.ColliderMode = Ferr2D_ColliderMode.None;
		Ferr2DT_Material material = biomeArtData.Ferr2DBiomeArtData.TerrainMaster.Ferr2DSettings.Material;
		ferr2DT_PathTerrain.SetMaterial(material);
		if (isInvertedTerrain)
		{
			ferr2DT_PathTerrain.FillMode = Ferr2D_SectionMode.Invert;
			ferr2DT_PathTerrain.EdgeMode = Ferr2D_SectionMode.Invert;
		}
		bool flag = component != null && !component.Equals(null);
		bool flag2 = flag && BiomeCreation_EV.BIOME_TAGS.ContainsKey(component.AppearanceBiomeType) && BiomeCreation_EV.BIOME_TAGS[component.AppearanceBiomeType].Contains(BiomeTag.Rounded_Terrain);
		float x = component.Bounds.min.x;
		float x2 = component.Bounds.max.x;
		float y = component.Bounds.max.y;
		float y2 = component.Bounds.min.y;
		for (int i = 0; i < terrainPoints.Count; i++)
		{
			Vector2 vector = terrainPoints[i];
			Vector2 aPt = gameObject.transform.InverseTransformPoint(vector);
			ferr2DT_PathTerrain.AddPoint(aPt);
			int index = i + 1;
			if (i == terrainPoints.Count - 1)
			{
				index = 0;
			}
			Vector2 pointB = terrainPoints[index];
			Line_RL key = new Line_RL(vector, pointB);
			if (edgeOverrides != null && edgeOverrides.ContainsKey(key))
			{
				ferr2DT_PathTerrain.PathData.SetData(i, edgeOverrides[key]);
			}
		}
		if (flag && flag2)
		{
			for (int j = 0; j < terrainPoints.Count; j++)
			{
				Vector2 vector2 = terrainPoints[j];
				int aRawIndex = ((j + 1 < terrainPoints.Count) ? (j + 1) : 0);
				ferr2DT_PathTerrain.PathData.GetSegmentNormal(j);
				ferr2DT_PathTerrain.PathData.GetSegmentNormal(aRawIndex);
				if (false || vector2.x - x < 0.1f || x2 - vector2.x < 0.1f || y - vector2.y < 0.1f || vector2.y - y2 < 0.1f)
				{
					ferr2DT_PathTerrain.PathData.GetControls(aRawIndex).type = PointType.Sharp;
				}
				else
				{
					ferr2DT_PathTerrain.PathData.GetControls(aRawIndex).type = PointType.CircleCorner;
				}
			}
		}
		return ferr2DT_PathTerrain;
	}

	public static List<List<Vector2>> GetUnion(List<List<Vector2>> points, bool applyOffsetCheck = true)
	{
		if (!applyOffsetCheck)
		{
			Clipper clipper = new Clipper();
			clipper.AddPath(points[0], PolyType.ptSubject, closed: true);
			for (int i = 1; i < points.Count; i++)
			{
				clipper.AddPath(points[i], PolyType.ptClip, closed: true);
			}
			List<List<Vector2>> list = new List<List<Vector2>>();
			clipper.Execute(ClipType.ctUnion, list, PolyFillType.pftNonZero);
			return list;
		}
		List<List<IntPoint>> list2 = new List<List<IntPoint>>();
		for (int j = 0; j < points.Count; j++)
		{
			list2.Add(new List<IntPoint>());
			for (int k = 0; k < points[j].Count; k++)
			{
				IntPoint item = new IntPoint((int)(points[j][k].x * 1000f), (int)(points[j][k].y * 1000f));
				list2[j].Add(item);
			}
		}
		Clipper clipper2 = new Clipper();
		clipper2.AddPath(list2[0], PolyType.ptSubject, Closed: true);
		for (int l = 1; l < list2.Count; l++)
		{
			clipper2.AddPath(list2[l], PolyType.ptClip, Closed: true);
		}
		List<List<IntPoint>> solution = new List<List<IntPoint>>();
		clipper2.Execute(ClipType.ctUnion, solution, PolyFillType.pftNonZero);
		ClipperOffset clipperOffset = new ClipperOffset();
		clipperOffset.AddPaths(solution, JoinType.jtSquare, EndType.etClosedPolygon);
		clipperOffset.Execute(ref solution, 0.00019999999494757503);
		List<List<Vector2>> list3 = new List<List<Vector2>>();
		for (int m = 0; m < solution.Count; m++)
		{
			list3.Add(new List<Vector2>());
			for (int n = 0; n < solution[m].Count; n++)
			{
				Vector2 item2 = new Vector2((float)solution[m][n].X / 1000f, (float)solution[m][n].Y / 1000f);
				list3[m].Add(item2);
			}
		}
		return list3;
	}

	private static MergeRoom CreateMergeRoom()
	{
		m_mergeRoomNumber++;
		GameObject gameObject = new GameObject("Merge Room " + m_mergeRoomNumber);
		MergeRoom result = gameObject.AddComponent<MergeRoom>();
		gameObject.AddComponent<CameraZoomController>();
		return result;
	}

	public static void ResetMergeRoomCounter()
	{
		m_mergeRoomNumber = 0;
	}

	public static void SetMergeRoomCollider(MergeRoom mergeRoom)
	{
		mergeRoom.gameObject.AddComponent<PolygonCollider2D>();
		PolygonCollider2D component = mergeRoom.gameObject.GetComponent<PolygonCollider2D>();
		component.isTrigger = true;
		Vector2[] array = new Vector2[4]
		{
			mergeRoom.gameObject.transform.InverseTransformPoint(mergeRoom.Bounds.min),
			default(Vector2),
			default(Vector2),
			default(Vector2)
		};
		Vector2 vector = new Vector2(mergeRoom.Bounds.max.x, mergeRoom.Bounds.min.y);
		array[1] = mergeRoom.gameObject.transform.InverseTransformPoint(vector);
		array[2] = mergeRoom.gameObject.transform.InverseTransformPoint(mergeRoom.Bounds.max);
		Vector2 vector2 = new Vector2(mergeRoom.Bounds.min.x, mergeRoom.Bounds.max.y);
		array[3] = mergeRoom.gameObject.transform.InverseTransformPoint(vector2);
		component.pathCount = 1;
		component.SetPath(0, array);
	}

	public static void ConvertCompositeColliderToPolygonCollider(CompositeCollider2D compositeCollider)
	{
		PolygonCollider2D polygonCollider2D = compositeCollider.gameObject.AddComponent<PolygonCollider2D>();
		polygonCollider2D.isTrigger = true;
		polygonCollider2D.pathCount = compositeCollider.pathCount;
		for (int i = 0; i < compositeCollider.pathCount; i++)
		{
			int pathPointCount = compositeCollider.GetPathPointCount(i);
			Vector2[] array = new Vector2[pathPointCount];
			List<Vector2> list = new List<Vector2>();
			compositeCollider.GetPath(i, array);
			float num = float.MaxValue;
			float num2 = float.MinValue;
			float num3 = float.MaxValue;
			float num4 = float.MinValue;
			for (int j = 0; j < pathPointCount; j++)
			{
				Vector2 vector = array[j];
				if (vector.x < num)
				{
					num = vector.x;
				}
				if (vector.x > num2)
				{
					num2 = vector.x;
				}
				if (vector.y < num3)
				{
					num3 = vector.y;
				}
				if (vector.y > num4)
				{
					num4 = vector.y;
				}
			}
			list.Add(new Vector2(num, num4));
			list.Add(new Vector2(num2, num4));
			list.Add(new Vector2(num2, num3));
			list.Add(new Vector2(num, num3));
			polygonCollider2D.SetPath(i, list.ToArray());
		}
		for (int k = 0; k < compositeCollider.transform.childCount; k++)
		{
			Collider2D component = compositeCollider.transform.GetChild(k).GetComponent<Collider2D>();
			if (component != null && component.usedByComposite)
			{
				component.enabled = false;
			}
		}
		if (Application.isPlaying)
		{
			UnityEngine.Object.Destroy(compositeCollider);
		}
	}

	public static MergeRoom MergeRooms(BiomeController biomeController, List<Room> connectedRooms)
	{
		GameObject gameObject = Merge(connectedRooms);
		gameObject.transform.SetParent(biomeController.StandardRoomStorageLocation);
		MergeRoom component = gameObject.GetComponent<MergeRoom>();
		SetMergeRoomCollider(component);
		biomeController.MergeRoomCreatedByWorldBuilder(component);
		return component;
	}

	public static void MergeConnectedGridPointManagers(BiomeController biomeController, BiomeType biome)
	{
		List<GridPointManager> list = biomeController.GridPointManager.GridPointManagers.Where((GridPointManager room) => room.RoomMetaData.CanMerge && room.Biome == biome).ToList();
		if (biome == BiomeType.Tower || biome == BiomeType.TowerExterior)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (list[num].RoomType == RoomType.BossEntrance)
				{
					list.RemoveAt(num);
				}
			}
		}
		while (list.Count > 0)
		{
			List<GridPointManager> list2 = new List<GridPointManager>();
			GetConnectedRooms_V2(list2, list.First());
			foreach (GridPointManager item in list2)
			{
				list.Remove(item);
			}
			if (list2.Count > 1)
			{
				MergeGridPointManagers(biomeController, list2);
			}
		}
	}

	public static void MergeGridPointManagers(BiomeController biomeController, List<GridPointManager> connectedRooms)
	{
		foreach (GridPointManager connectedRoom in connectedRooms)
		{
			connectedRoom.SetMergeWithRooms(connectedRooms);
		}
	}

	private static void GetConnectedRooms(List<Room> connectedRooms, Room room)
	{
		connectedRooms.Add(room);
		if (room.ConnectedRooms == null || room.ConnectedRooms.Count <= 0)
		{
			return;
		}
		foreach (BaseRoom connectedRoom in room.ConnectedRooms)
		{
			if ((connectedRoom as Room).RoomType != RoomType.BossEntrance && (connectedRoom as Room).AppearanceBiomeType == room.AppearanceBiomeType && (connectedRoom as Room).CanMerge && !connectedRooms.Contains(connectedRoom))
			{
				GetConnectedRooms(connectedRooms, connectedRoom as Room);
			}
		}
	}

	private static void GetConnectedRooms_V2(List<GridPointManager> connectedRooms, GridPointManager room)
	{
		connectedRooms.Add(room);
		List<GridPointManager> list = new List<GridPointManager>();
		foreach (DoorLocation doorLocation in room.DoorLocations)
		{
			GridPointManager connectedRoom = room.GetConnectedRoom(doorLocation);
			if (connectedRoom != null && !list.Contains(connectedRoom) && ((room.Biome != BiomeType.Tower && room.Biome != BiomeType.TowerExterior) || connectedRoom.RoomType != RoomType.BossEntrance))
			{
				list.Add(connectedRoom);
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		foreach (GridPointManager item in list)
		{
			if (item.Biome == room.Biome && item.RoomMetaData.CanMerge && !connectedRooms.Contains(item))
			{
				GetConnectedRooms_V2(connectedRooms, item);
			}
		}
	}
}
