using System.Collections.Generic;
using Rooms;
using UnityEngine;

namespace RLWorldCreation;

public class RoomRequirementsController
{
	protected RoomTypeEntry m_lastEntryRemovedFromBacklog;

	protected List<RoomTypeEntry> m_backlog = new List<RoomTypeEntry>();

	protected List<RoomTypeEntry> m_specialRoomPool = new List<RoomTypeEntry>();

	private BiomeController m_biomeController;

	private int m_interval = -1;

	private int m_requiredEasyRoomCount = -1;

	private int m_easyRoomsPlaced;

	public RoomRequirementsController(BiomeController biomeController)
	{
		CreateSpecialRoomPool(biomeController);
		int num = biomeController.TargetRoomCountsByRoomType[RoomType.Standard];
		int num2 = biomeController.TargetRoomCountsByRoomType[RoomType.Fairy];
		int num3 = biomeController.TargetRoomCountsByRoomType[RoomType.Trap];
		int num4 = biomeController.TargetRoomCountsByRoomType[RoomType.Bonus];
		int num5 = biomeController.TargetRoomCountsByRoomType[RoomType.Mandatory];
		int num6 = num + num2 + num3 + num4 + num5;
		int count = m_specialRoomPool.Count;
		if (count > 0)
		{
			m_interval = num6 / count;
			if (num6 % count != 0)
			{
				m_interval++;
			}
			if (m_interval == 0)
			{
				m_interval = 1;
			}
		}
		if (BiomeCreation_EV.FORCE_EASY_ROOMS_AT_BIOME_START.ContainsKey(biomeController.Biome))
		{
			m_requiredEasyRoomCount = BiomeCreation_EV.FORCE_EASY_ROOMS_AT_BIOME_START[biomeController.Biome];
		}
		m_biomeController = biomeController;
	}

	public void AddToBacklog(RoomTypeEntry entry)
	{
		if (m_lastEntryRemovedFromBacklog == entry)
		{
			m_backlog.Insert(0, entry);
		}
		else
		{
			m_backlog.Add(entry);
		}
	}

	private void CreateSpecialRoomPool(BiomeController biomeController)
	{
		int num = biomeController.TargetRoomCountsByRoomType[RoomType.Bonus];
		int num2 = biomeController.TargetRoomCountsByRoomType[RoomType.Fairy];
		int num3 = biomeController.TargetRoomCountsByRoomType[RoomType.Trap];
		int num4 = biomeController.TargetRoomCountsByRoomType[RoomType.Mandatory];
		int num5 = (num + num2 + num3 + num4) / 2;
		List<RoomTypeEntry> list = new List<RoomTypeEntry>();
		for (int i = 0; i < num; i++)
		{
			list.Add(new RoomTypeEntry(RoomType.Bonus));
		}
		for (int j = 0; j < num2; j++)
		{
			list.Add(new RoomTypeEntry(RoomType.Fairy));
		}
		for (int k = 0; k < num3; k++)
		{
			list.Add(new RoomTypeEntry(RoomType.Trap));
		}
		MandatoryRoomEntry[] mandatoryRooms = RoomLibrary.GetSetCollection(biomeController.Biome).MandatoryRooms;
		List<RoomTypeEntry> list2 = new List<RoomTypeEntry>();
		for (int l = 0; l < mandatoryRooms.Length; l++)
		{
			bool flag = false;
			if (mandatoryRooms[l].ReplacementCriteria != 0)
			{
				flag = ConditionFlag_RL.IsConditionFulfilled(mandatoryRooms[l].ReplacementCriteria);
			}
			RoomMetaData roomMetaData = mandatoryRooms[l].RoomMetaData;
			if (flag && mandatoryRooms[l].ReplacementRoom != null)
			{
				roomMetaData = mandatoryRooms[l].ReplacementRoom;
			}
			RoomTypeEntry item = new RoomTypeEntry(mandatoryRooms[l].RoomType, RoomSide.Any, roomMetaData, mustBeEasy: false);
			if (mandatoryRooms[l].RoomMetaData.SpecialRoomType == SpecialRoomType.BossEntrance || mandatoryRooms[l].RoomMetaData.SpecialRoomType == SpecialRoomType.Heirloom || mandatoryRooms[l].RoomMetaData.SpecialRoomType == SpecialRoomType.HeirloomEntrance)
			{
				list2.Add(item);
			}
			else
			{
				list.Add(item);
			}
		}
		RoomUtility.ShuffleList(list);
		if (list.Count > num5)
		{
			for (int num6 = list.Count - 1; num6 >= num5; num6--)
			{
				list2.Add(list[num6]);
				list.RemoveAt(num6);
			}
		}
		RoomUtility.ShuffleList(list2);
		m_specialRoomPool.AddRange(list);
		m_specialRoomPool.AddRange(list2);
		if (!BiomeCreation_EV.REQUEST_EASY_FAIRY_ROOMS_IN_BIOME.ContainsKey(biomeController.Biome))
		{
			return;
		}
		int num7 = BiomeCreation_EV.REQUEST_EASY_FAIRY_ROOMS_IN_BIOME[biomeController.Biome];
		int num8 = 0;
		for (int m = 0; m < m_specialRoomPool.Count; m++)
		{
			if (m_specialRoomPool[m].RoomType == RoomType.Fairy)
			{
				m_specialRoomPool[m].MustBeEasy = true;
				num8++;
			}
			if (num8 == num7)
			{
				break;
			}
		}
	}

	public RoomTypeEntry GetRequirements(int roomNumber)
	{
		RoomTypeEntry roomTypeEntry = null;
		if (m_backlog.Count > 0)
		{
			roomTypeEntry = m_backlog[0];
			m_backlog.RemoveAt(0);
			m_lastEntryRemovedFromBacklog = roomTypeEntry;
		}
		else
		{
			if (m_specialRoomPool.Count <= 0 || ((m_interval <= 0 || roomNumber % m_interval != 0) && m_biomeController.GridPointManager.StandardRoomCount != m_biomeController.TargetRoomCountsByRoomType[RoomType.Standard]))
			{
				bool mustBeEasy = false;
				if (m_easyRoomsPlaced < m_requiredEasyRoomCount)
				{
					mustBeEasy = true;
					m_easyRoomsPlaced++;
				}
				return new RoomTypeEntry(RoomType.Standard, RoomSide.Any, null, mustBeEasy);
			}
			roomTypeEntry = m_specialRoomPool[0];
			m_specialRoomPool.RemoveAt(0);
		}
		RoomLibrary.GetSetCollection(m_biomeController.Biome).SetMaxRoomSizeOverride(Vector2Int.zero);
		if (roomTypeEntry.RoomMetaData != null)
		{
			Vector2Int maxRoomSize = RoomLibrary.GetSetCollection(m_biomeController.Biome).MaxRoomSize;
			if (maxRoomSize.x < roomTypeEntry.RoomMetaData.Size.x || maxRoomSize.y < roomTypeEntry.RoomMetaData.Size.y)
			{
				int x = maxRoomSize.x;
				if (x < roomTypeEntry.RoomMetaData.Size.x)
				{
					x = roomTypeEntry.RoomMetaData.Size.x;
				}
				int y = maxRoomSize.y;
				if (y < roomTypeEntry.RoomMetaData.Size.y)
				{
					y = roomTypeEntry.RoomMetaData.Size.y;
				}
				Vector2Int maxRoomSizeOverride = new Vector2Int(x, y);
				RoomLibrary.GetSetCollection(m_biomeController.Biome).SetMaxRoomSizeOverride(maxRoomSizeOverride);
			}
		}
		return roomTypeEntry;
	}
}
