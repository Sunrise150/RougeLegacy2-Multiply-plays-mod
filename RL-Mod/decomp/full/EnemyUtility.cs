using System.Collections.Generic;
using UnityEngine;

public static class EnemyUtility
{
	private static bool DISPLAY_IN_CONSOLE_ON_INITIALISE = false;

	private static Dictionary<BiomeType, List<EnemyTypeAndRank>> m_enemiesInBiomeTable = null;

	private static List<Renderer> m_boundsHelper_STATIC = new List<Renderer>();

	private static List<EnemyTypeAndRank> m_enemiesInBiomeHelper = new List<EnemyTypeAndRank>();

	private static void InitialiseEnemiesInBiomeTable()
	{
		m_enemiesInBiomeTable = new Dictionary<BiomeType, List<EnemyTypeAndRank>>();
		BiomeType[] typeArray = BiomeType_RL.TypeArray;
		foreach (BiomeType biomeType in typeArray)
		{
			if (!IsIncludedBiome(biomeType))
			{
				continue;
			}
			List<EnemyTypeAndRank> list = new List<EnemyTypeAndRank>();
			EnemyType[] typeArray2 = EnemyTypes_RL.TypeArray;
			foreach (EnemyType enemyType in typeArray2)
			{
				if (enemyType == EnemyType.None || enemyType == EnemyType.Any)
				{
					continue;
				}
				EnemyRank[] rankArray = EnemyTypes_RL.RankArray;
				foreach (EnemyRank enemyRank in rankArray)
				{
					if (enemyRank != EnemyRank.None && enemyRank != EnemyRank.Any)
					{
						EnemyData enemyData = EnemyClassLibrary.GetEnemyData(enemyType, enemyRank);
						if ((bool)enemyData && enemyData.GetSpawnInBiome(biomeType))
						{
							list.Add(new EnemyTypeAndRank(enemyType, enemyRank));
						}
					}
				}
			}
			m_enemiesInBiomeTable.Add(biomeType, list);
		}
		if (!DISPLAY_IN_CONSOLE_ON_INITIALISE)
		{
			return;
		}
		string text = "Enemies in Biome Table:\n";
		foreach (KeyValuePair<BiomeType, List<EnemyTypeAndRank>> item in m_enemiesInBiomeTable)
		{
			text = text + item.Key.ToString() + ":\n";
			foreach (EnemyTypeAndRank item2 in item.Value)
			{
				text += $"{item2.Type}, {item2.Rank}\n";
			}
		}
		Debug.LogFormat(text);
	}

	public static bool IsFlyingEnemy(EnemyType enemyType, EnemyRank enemyRank)
	{
		EnemyData enemyData = EnemyClassLibrary.GetEnemyData(enemyType, enemyRank);
		if ((bool)enemyData)
		{
			return enemyData.IsFlying;
		}
		return false;
	}

	private static bool IsIncludedBiome(BiomeType biome)
	{
		if (biome != BiomeType.Any && biome != 0 && biome != BiomeType.Spawn)
		{
			return biome != BiomeType.Special;
		}
		return false;
	}

	public static Bounds GetBounds(GameObject gameObject)
	{
		m_boundsHelper_STATIC.Clear();
		Transform transform = gameObject.transform.FindDeep("Visuals");
		Bounds result = new Bounds(gameObject.transform.position, Vector3.zero);
		if ((bool)transform)
		{
			transform.GetComponentsInChildren(includeInactive: true, m_boundsHelper_STATIC);
			foreach (Renderer item in m_boundsHelper_STATIC)
			{
				if (!(item is ParticleSystemRenderer) && (!(item.bounds.center == Vector3.zero) || !(item.bounds.size == Vector3.zero)))
				{
					result.Encapsulate(item.bounds);
				}
			}
		}
		else
		{
			Debug.LogFormat("<color=purple>| {0} | Couldn't find child called \"Visuals\" in GameObject ({1})</color>", "EnemyUtility", gameObject.name);
		}
		return result;
	}

	public static float GetEnemyScale(EnemyController enemyController)
	{
		EnemyData enemyData = EnemyClassLibrary.GetEnemyData(enemyController.EnemyType, enemyController.EnemyRank);
		if ((bool)enemyData)
		{
			return enemyData.Scale;
		}
		return 0f;
	}

	public static List<EnemyTypeAndRank> GetAllEnemiesInBiome(BiomeType biome, bool onlyFlyingEnemies = false)
	{
		if (m_enemiesInBiomeTable == null)
		{
			InitialiseEnemiesInBiomeTable();
		}
		if (biome == BiomeType.TowerExterior)
		{
			biome = BiomeType.Tower;
		}
		if (m_enemiesInBiomeTable.ContainsKey(biome))
		{
			List<EnemyTypeAndRank> collection = m_enemiesInBiomeTable[biome];
			m_enemiesInBiomeHelper.Clear();
			m_enemiesInBiomeHelper.AddRange(collection);
			if (biome == BiomeType.Castle && BurdenManager.IsBurdenActive(BurdenType.CastleBiomeUp))
			{
				m_enemiesInBiomeHelper.Remove(new EnemyTypeAndRank(EnemyType.SwordKnight, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Remove(new EnemyTypeAndRank(EnemyType.SpearKnight, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Remove(new EnemyTypeAndRank(EnemyType.ElementalFire, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.SwordKnight, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.SpearKnight, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.ElementalFire, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.AxeKnight, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.PaintingEnemy, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.FlyingHunter, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.Sniper, EnemyRank.Basic));
			}
			if (biome == BiomeType.Stone && BurdenManager.IsBurdenActive(BurdenType.BridgeBiomeUp))
			{
				m_enemiesInBiomeHelper.Remove(new EnemyTypeAndRank(EnemyType.ElementalBounce, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Remove(new EnemyTypeAndRank(EnemyType.Skeleton, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Remove(new EnemyTypeAndRank(EnemyType.BlobFish, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Remove(new EnemyTypeAndRank(EnemyType.Eyeball, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.ElementalBounce, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.Skeleton, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.BlobFish, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.Eyeball, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.FlyingSkull, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.Fireball, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.Zombie, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.Slug, EnemyRank.Advanced));
			}
			if (biome == BiomeType.Forest && BurdenManager.IsBurdenActive(BurdenType.ForestBiomeUp))
			{
				m_enemiesInBiomeHelper.Remove(new EnemyTypeAndRank(EnemyType.BouncySpike, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Remove(new EnemyTypeAndRank(EnemyType.Wolf, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Remove(new EnemyTypeAndRank(EnemyType.ElementalIce, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Remove(new EnemyTypeAndRank(EnemyType.Starburst, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Remove(new EnemyTypeAndRank(EnemyType.Wisp, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Remove(new EnemyTypeAndRank(EnemyType.Zombie, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Remove(new EnemyTypeAndRank(EnemyType.FlyingBurst, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.BouncySpike, EnemyRank.Expert));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.Wolf, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.ElementalIce, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.Starburst, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.Wisp, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.Zombie, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.FlyingBurst, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.Plant, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.TopShotHazard, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.Volcano, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.AxeKnight, EnemyRank.Basic));
			}
			if (biome == BiomeType.Study && BurdenManager.IsBurdenActive(BurdenType.StudyBiomeUp))
			{
				m_enemiesInBiomeHelper.Remove(new EnemyTypeAndRank(EnemyType.MimicChest, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Remove(new EnemyTypeAndRank(EnemyType.PaintingEnemy, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Remove(new EnemyTypeAndRank(EnemyType.Slug, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Remove(new EnemyTypeAndRank(EnemyType.Sniper, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.MimicChest, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.PaintingEnemy, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.Slug, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.Sniper, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.ElementalDash, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.ElementalIce, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.ElementalBounce, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.ElementalCurse, EnemyRank.Advanced));
			}
			if (biome == BiomeType.Tower && BurdenManager.IsBurdenActive(BurdenType.TowerBiomeUp))
			{
				m_enemiesInBiomeHelper.Remove(new EnemyTypeAndRank(EnemyType.FlyingHunter, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Remove(new EnemyTypeAndRank(EnemyType.FlyingSkull, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Remove(new EnemyTypeAndRank(EnemyType.Fireball, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Remove(new EnemyTypeAndRank(EnemyType.ElementalCurse, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.FlyingHunter, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.FlyingSkull, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.Fireball, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.ElementalCurse, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.SkeletonArcher, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.Wisp, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.Plant, EnemyRank.Advanced));
			}
			if (biome == BiomeType.Cave && BurdenManager.IsBurdenActive(BurdenType.CaveBiomeUp))
			{
				m_enemiesInBiomeHelper.Remove(new EnemyTypeAndRank(EnemyType.AxeKnight, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Remove(new EnemyTypeAndRank(EnemyType.Plant, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Remove(new EnemyTypeAndRank(EnemyType.TopShotHazard, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Remove(new EnemyTypeAndRank(EnemyType.ElementalDash, EnemyRank.Basic));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.AxeKnight, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.Plant, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.TopShotHazard, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.ElementalDash, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.FlyingSkull, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.MimicChest, EnemyRank.Advanced));
				m_enemiesInBiomeHelper.Add(new EnemyTypeAndRank(EnemyType.Wolf, EnemyRank.Advanced));
			}
			if (onlyFlyingEnemies)
			{
				for (int i = 0; i < m_enemiesInBiomeHelper.Count; i++)
				{
					if (!IsFlyingEnemy(m_enemiesInBiomeHelper[i].Type, m_enemiesInBiomeHelper[i].Rank))
					{
						m_enemiesInBiomeHelper.RemoveAt(i);
						i--;
					}
				}
			}
			return m_enemiesInBiomeHelper;
		}
		Debug.LogFormat("<color=red>(EnemyUtility) No Entry in Table for Biome ({0})</color>", biome);
		return null;
	}
}
