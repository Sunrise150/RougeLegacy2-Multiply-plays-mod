using System;
using System.Collections.Generic;
using Rooms;
using UnityEngine;

public class RoomSetCollection
{
	private static List<RoomSetEntry> m_roomList = new List<RoomSetEntry>(100);

	private static Dictionary<RoomCriteriaLookupCacheKey, RoomSetEntry[]> m_roomCriteriaTable = null;

	private static Dictionary<RoomCriteriaLookupCacheKey, int> m_rngWeightSum = null;

	private Vector2Int m_maxRoomSizeOverride = Vector2Int.zero;

	private Vector2Int m_maxRoomSize;

	public BiomeType Biome { get; }

	public MandatoryRoomEntry[] MandatoryRooms { get; set; }

	public Vector2Int MaxRoomSize
	{
		get
		{
			if (m_maxRoomSizeOverride == Vector2Int.zero)
			{
				return m_maxRoomSize;
			}
			return m_maxRoomSizeOverride;
		}
		private set
		{
			m_maxRoomSize = value;
		}
	}

	public HashSet<RoomSetEntry> RoomSet { get; set; }

	public Dictionary<RoomType, HashSet<RoomSetEntry>> RoomTypeSets { get; set; }

	public RoomMetaData TransitionRoom { get; set; }

	public RoomSetCollection(BiomeType biome, RoomPool roomPool)
	{
		Biome = biome;
		RoomSet = new HashSet<RoomSetEntry>();
		RoomTypeSets = new Dictionary<RoomType, HashSet<RoomSetEntry>>();
		MandatoryRooms = roomPool.GetMandatoryRoomEntries(biome);
		TransitionRoom = roomPool.GetTransitionRoom(biome);
		RoomType[] roomTypeArray = RoomType_RL.RoomTypeArray;
		foreach (RoomType roomType in roomTypeArray)
		{
			RoomTypeSets.Add(roomType, new HashSet<RoomSetEntry>());
			CompiledScene_ScriptableObject[] compiledScenes = roomPool.GetCompiledScenes(biome, roomType);
			foreach (CompiledScene_ScriptableObject compiledScene_ScriptableObject in compiledScenes)
			{
				if (compiledScene_ScriptableObject.RoomMetaData != null && compiledScene_ScriptableObject.RoomMetaData.Count > 0)
				{
					try
					{
						AddRoomsAndMirrorToSet(RoomTypeSets[roomType], compiledScene_ScriptableObject.RoomMetaData, compiledScene_ScriptableObject.name);
						AddRoomsAndMirrorToSet(RoomSet, compiledScene_ScriptableObject.RoomMetaData, compiledScene_ScriptableObject.name);
					}
					catch (Exception ex)
					{
						if (ex is UnassignedReferenceException)
						{
							Debug.LogFormat("<color=red>| RoomLibrary | Room GameObject List Field has not been assigned to on Compiled Scene SO ({0}). This may mean the compilation of Level Editor Scene ({1}) failed. Try recompiling the Scene.</color>", compiledScene_ScriptableObject.name, compiledScene_ScriptableObject.SceneID);
						}
						throw;
					}
				}
				else
				{
					Debug.LogFormat("<color=red>| RoomLibrary | Compiled Scene SO (<b>{0}</b>)'s Room GameObject List is empty</color>", compiledScene_ScriptableObject.name);
				}
			}
		}
		foreach (RoomSetEntry item in RoomSet)
		{
			if (item.RoomMetaData.Size.x > MaxRoomSize.x)
			{
				MaxRoomSize = new Vector2Int(item.RoomMetaData.Size.x, MaxRoomSize.y);
			}
			if (item.RoomMetaData.Size.y > MaxRoomSize.y)
			{
				MaxRoomSize = new Vector2Int(MaxRoomSize.x, item.RoomMetaData.Size.y);
			}
		}
	}

	public void SetMaxRoomSizeOverride(Vector2Int maxRoomSize)
	{
		m_maxRoomSizeOverride = maxRoomSize;
	}

	private void AddRoomsAndMirrorToSet(HashSet<RoomSetEntry> set, IEnumerable<RoomMetaData> roomMetaData, string compiledSceneName = "")
	{
		if (set == null || roomMetaData == null)
		{
			return;
		}
		foreach (RoomMetaData roomMetaDatum in roomMetaData)
		{
			if (roomMetaDatum != null)
			{
				set.Add(new RoomSetEntry(roomMetaDatum));
				if (roomMetaDatum.CanFlip)
				{
					bool flag = true;
					if (flag)
					{
						_ = roomMetaDatum.Size == Vector2Int.one;
					}
					if (flag)
					{
						_ = roomMetaDatum.Size.x;
						_ = 1;
					}
					if (flag)
					{
						set.Add(new RoomSetEntry(roomMetaDatum, isMirrored: true));
					}
				}
			}
			else
			{
				Debug.LogFormat("<color=red>| RoomSetCollection | <b>{0}</b> Null room found in {1}.</color>", Biome, compiledSceneName);
			}
		}
	}

	public int GetRNGWeightSum(RoomCriteriaLookupCacheKey criteria)
	{
		return m_rngWeightSum[criteria];
	}

	public RoomSetEntry[] GetRoomsMatchingCriteria(RoomCriteriaLookupCacheKey criteria, List<RoomSetEntry> standardRoomPoolOverride)
	{
		if (m_roomCriteriaTable == null)
		{
			m_roomCriteriaTable = new Dictionary<RoomCriteriaLookupCacheKey, RoomSetEntry[]>(new RoomCriteriaLookupCacheKey.EqualityComparer());
		}
		if (m_rngWeightSum == null)
		{
			m_rngWeightSum = new Dictionary<RoomCriteriaLookupCacheKey, int>();
		}
		if (m_roomCriteriaTable.TryGetValue(criteria, out var value))
		{
			return value;
		}
		m_roomList.Clear();
		HashSet<RoomSetEntry> hashSet = ((standardRoomPoolOverride != null) ? new HashSet<RoomSetEntry>(standardRoomPoolOverride) : RoomLibrary.GetSetCollection(criteria.Biome).RoomTypeSets[criteria.RoomType]);
		foreach (RoomSetEntry item2 in hashSet)
		{
			Vector2Int size = item2.RoomMetaData.Size;
			int num = (size.x - 1) * 3 + (size.y - 1);
			if (criteria.RoomSizeToDoorLocationTable[num] != 0 && (criteria.RoomType != RoomType.Bonus || (item2.RoomMetaData.IsSpecialRoom && (item2.RoomMetaData.SpecialRoomType == criteria.SpecialRoomType || item2.RoomMetaData.SpecialRoomType == SpecialRoomType.IncludeInAll))) && (!criteria.MustBeEasy || item2.RoomMetaData.IsEasy) && (item2.DoorMask & criteria.RoomSizeToDoorLocationTable[num]) != 0)
			{
				int weight = RoomWeightManager.GetWeight(item2.RoomMetaData.ID, item2.IsMirrored);
				RoomSetEntry item = new RoomSetEntry(item2, weight);
				m_roomList.Add(item);
			}
		}
		value = m_roomList.ToArray();
		m_rngWeightSum.Add(criteria, SetRoomWeights(value));
		m_roomCriteriaTable.Add(criteria, value);
		return value;
	}

	private static int SetRoomWeights(RoomSetEntry[] rooms)
	{
		int num = 0;
		for (int i = 0; i < rooms.Length; i++)
		{
			int num2 = num + rooms[i].Weight;
			rooms[i] = new RoomSetEntry(rooms[i], num2);
			num = num2;
		}
		return num;
	}
}
