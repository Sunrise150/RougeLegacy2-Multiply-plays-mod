using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomEnemyManager : MonoBehaviour, IRoomConsumer, ILevelConsumer, ISetSpawnType
{
	private int m_level = -1;

	private BaseRoom m_room;

	private static List<EnemySpawnController> m_enemySpawnerListHelper_STATIC = new List<EnemySpawnController>();

	public EnemySpawnController[] EnemySpawnControllers
	{
		get
		{
			if (!Room.IsNativeNull())
			{
				return Room.SpawnControllerManager.EnemySpawnControllers;
			}
			return null;
		}
	}

	public BaseRoom Room => m_room;

	public bool HasRequiredReferences
	{
		get
		{
			if (Room != null)
			{
				return Level != -1;
			}
			return false;
		}
	}

	public bool IsInitialised { get; private set; }

	public int Level => m_level;

	public void SetRoom(BaseRoom room)
	{
		if (!(room is MergeRoom))
		{
			m_room = room;
			if (room is Room && HasRequiredReferences)
			{
				Initialise();
			}
		}
	}

	public void SetLevel(int value)
	{
		m_level = value;
		if (HasRequiredReferences)
		{
			Initialise();
		}
	}

	private void Initialise()
	{
		if (Room.IsNativeNull())
		{
			throw new ArgumentNullException("Room");
		}
		if (!IsInitialised)
		{
			if (GameUtility.IsInLevelEditor)
			{
				SetSpawnType();
			}
			IsInitialised = true;
		}
	}

	public void SetSpawnType()
	{
		Room room = Room as Room;
		if ((bool)room)
		{
			BiomeType biomeType = room.BiomeType;
			if (biomeType != BiomeType.Any && biomeType != 0)
			{
				foreach (IGrouping<int, EnemySpawnController> item in from entry in EnemySpawnControllers
					group entry by entry.ID)
				{
					List<EnemySpawnController> enemySpawnerList = GetEnemySpawnerList(item, commanderOnly: false, flyingOnly: false);
					SetupEnemySpawnControllers(enemySpawnerList, biomeType, commanderOnly: false, flyingOnly: false, Room.RoomType);
					List<EnemySpawnController> enemySpawnerList2 = GetEnemySpawnerList(item, commanderOnly: false, flyingOnly: true);
					SetupEnemySpawnControllers(enemySpawnerList2, biomeType, commanderOnly: false, flyingOnly: true, Room.RoomType);
					List<EnemySpawnController> enemySpawnerList3 = GetEnemySpawnerList(item, commanderOnly: true, flyingOnly: false);
					SetupEnemySpawnControllers(enemySpawnerList3, biomeType, commanderOnly: true, flyingOnly: false, Room.RoomType);
					List<EnemySpawnController> enemySpawnerList4 = GetEnemySpawnerList(item, commanderOnly: true, flyingOnly: true);
					SetupEnemySpawnControllers(enemySpawnerList4, biomeType, commanderOnly: true, flyingOnly: true, Room.RoomType);
				}
				return;
			}
			Debug.LogFormat("<color=red>({0}) Room Biome ({1}) is invalid</color>", this, biomeType);
			return;
		}
		throw new InvalidCastException("{0}: Can't cast Room as Room", Time.frameCount);
	}

	private List<EnemySpawnController> GetEnemySpawnerList(IGrouping<int, EnemySpawnController> enemySpawnerGroup, bool commanderOnly, bool flyingOnly)
	{
		m_enemySpawnerListHelper_STATIC.Clear();
		foreach (EnemySpawnController item in enemySpawnerGroup)
		{
			if ((!flyingOnly || item.ForceFlying) && (!commanderOnly || item.ForceCommander))
			{
				m_enemySpawnerListHelper_STATIC.Add(item);
			}
		}
		return m_enemySpawnerListHelper_STATIC;
	}

	private void SetupEnemySpawnControllers(List<EnemySpawnController> enemySpawnControllers, BiomeType biome, bool commanderOnly, bool flyingOnly, RoomType roomType)
	{
		if (enemySpawnControllers.Count <= 0)
		{
			return;
		}
		EnemyTypeAndRank randomEnemyTypeAndRank = GetRandomEnemyTypeAndRank(biome, flyingOnly, commanderOnly, roomType);
		for (int i = 0; i < enemySpawnControllers.Count; i++)
		{
			bool num = BurdenManager.GetBurdenLevel(BurdenType.EnemyEvolve) > 0;
			bool flag = false;
			if (num && randomEnemyTypeAndRank.Rank < EnemyRank.Expert && roomType != RoomType.Fairy)
			{
				float burdenStatGain = BurdenManager.GetBurdenStatGain(BurdenType.EnemyEvolve);
				if (RNGManager.GetRandomNumber(RngID.Enemy_RoomSeed, "(RoomEnemyManager) GetRandomEnemyTypeAndRank.CanEvolve", 0f, 1f) < burdenStatGain)
				{
					flag = true;
				}
			}
			if (flag)
			{
				enemySpawnControllers[i].SetEnemy(randomEnemyTypeAndRank.Type, randomEnemyTypeAndRank.Rank + 1, Level);
			}
			else
			{
				enemySpawnControllers[i].SetEnemy(randomEnemyTypeAndRank.Type, randomEnemyTypeAndRank.Rank, Level);
			}
		}
	}

	private EnemyTypeAndRank GetRandomEnemyTypeAndRank(BiomeType biome, bool flyingOnly, bool commanderOnly, RoomType roomType)
	{
		EnemyTypeAndRank result = new EnemyTypeAndRank(EnemyType.None, EnemyRank.None);
		List<EnemyTypeAndRank> allEnemiesInBiome = EnemyUtility.GetAllEnemiesInBiome(biome, flyingOnly);
		for (int i = 0; i < allEnemiesInBiome.Count; i++)
		{
			if (allEnemiesInBiome[i].Type == EnemyType.BouncySpike && (roomType == RoomType.Fairy || allEnemiesInBiome[i].Rank == EnemyRank.Expert))
			{
				allEnemiesInBiome.RemoveAt(i);
				i--;
			}
		}
		if (allEnemiesInBiome.Count > 0)
		{
			int randomNumber = RNGManager.GetRandomNumber(RngID.Enemy_RoomSeed, $"(RoomEnemyManager) GetRandomEnemyTypeAndRank", 0, allEnemiesInBiome.Count);
			result = allEnemiesInBiome[randomNumber];
		}
		else
		{
			Debug.LogFormat("<color=red>| {0} | No Enemy in Enemy Object Pool matches criteria: Biome = ({1}), Is Flying = ({2})</color>", this, biome, flyingOnly);
		}
		if (commanderOnly && result.Type != EnemyType.BouncySpike)
		{
			return new EnemyTypeAndRank(result.Type, EnemyRank.Expert);
		}
		return result;
	}
}
