using System.Collections.Generic;
using UnityEngine;

public class RoomCriteriaLookupCacheKey
{
	public class EqualityComparer : EqualityComparer<RoomCriteriaLookupCacheKey>
	{
		public override bool Equals(RoomCriteriaLookupCacheKey x, RoomCriteriaLookupCacheKey y)
		{
			bool result = false;
			if (x.Biome == y.Biome && x.RoomSide == y.RoomSide && x.RoomType == y.RoomType && x.SpecialRoomType == y.SpecialRoomType && x.MustBeEasy == y.MustBeEasy)
			{
				bool flag = true;
				for (int i = 0; i < x.RoomSizeToDoorLocationTable.Length; i++)
				{
					if (x.RoomSizeToDoorLocationTable[i] != y.RoomSizeToDoorLocationTable[i])
					{
						flag = false;
						break;
					}
				}
				result = flag;
			}
			return result;
		}

		public override int GetHashCode(RoomCriteriaLookupCacheKey key)
		{
			int num = 13;
			num = (num * 397) ^ (int)key.Biome;
			num = (num * 397) ^ (int)key.RoomType;
			num = (num * 397) ^ (int)key.SpecialRoomType;
			for (int i = 0; i < key.RoomSizeToDoorLocationTable.Length; i++)
			{
				num = (num * 397) ^ key.RoomSizeToDoorLocationTable[i];
			}
			return (num * 397) ^ (key.MustBeEasy ? 1 : 0);
		}
	}

	public BiomeType Biome { get; private set; }

	public RoomSide RoomSide { get; private set; }

	public int[] RoomSizeToDoorLocationTable { get; set; }

	public RoomType RoomType { get; private set; }

	public SpecialRoomType SpecialRoomType { get; private set; }

	public bool MustBeEasy { get; private set; }

	public RoomCriteriaLookupCacheKey(BiomeType biome, RoomSide side, RoomType roomType, Dictionary<Vector2Int, List<DoorLocation>> roomSizeToDoorLocations, SpecialRoomType specialRoomType, bool mustBeEasy)
	{
		Initialise(biome, side, roomType, roomSizeToDoorLocations, specialRoomType, mustBeEasy);
	}

	public void Initialise(BiomeType biome, RoomSide side, RoomType roomType, Dictionary<Vector2Int, List<DoorLocation>> roomSizeToDoorLocations, SpecialRoomType specialRoomType, bool mustBeEasy)
	{
		Biome = biome;
		RoomSide = side;
		RoomType = roomType;
		SpecialRoomType = specialRoomType;
		MustBeEasy = mustBeEasy;
		RoomSizeToDoorLocationTable = new int[8];
		for (int i = 0; i < RoomSizeToDoorLocationTable.Length; i++)
		{
			int num = 0;
			Vector2Int key = new Vector2Int(i / 3 + 1, i % 3 + 1);
			if (roomSizeToDoorLocations.TryGetValue(key, out var value))
			{
				num = RoomUtility.GetDoorLocationMask(side, value);
			}
			RoomSizeToDoorLocationTable[i] = num;
		}
	}

	public override bool Equals(object otherObject)
	{
		RoomCriteriaLookupCacheKey roomCriteriaLookupCacheKey = (RoomCriteriaLookupCacheKey)otherObject;
		bool result = false;
		if (Biome == roomCriteriaLookupCacheKey.Biome && RoomSide == roomCriteriaLookupCacheKey.RoomSide && RoomType == roomCriteriaLookupCacheKey.RoomType && SpecialRoomType == roomCriteriaLookupCacheKey.SpecialRoomType && MustBeEasy == roomCriteriaLookupCacheKey.MustBeEasy)
		{
			bool flag = true;
			for (int i = 0; i < RoomSizeToDoorLocationTable.Length; i++)
			{
				if (RoomSizeToDoorLocationTable[i] != roomCriteriaLookupCacheKey.RoomSizeToDoorLocationTable[i])
				{
					flag = false;
					break;
				}
			}
			result = flag;
		}
		return result;
	}

	public override int GetHashCode()
	{
		int num = 13;
		num = (num * 397) ^ (int)Biome;
		num = (num * 397) ^ (int)RoomSide;
		num = (num * 397) ^ (int)RoomType;
		num = (num * 397) ^ (int)SpecialRoomType;
		for (int i = 0; i < RoomSizeToDoorLocationTable.Length; i++)
		{
			num = (num * 397) ^ RoomSizeToDoorLocationTable[i];
		}
		return (num * 397) ^ (MustBeEasy ? 1 : 0);
	}

	public override string ToString()
	{
		return $"RoomCriteriaLookupCacheKey - ({Biome}, {RoomSide}, {RoomSizeToDoorLocationTable.Length}, {RoomType}, {SpecialRoomType}, {MustBeEasy})";
	}
}
