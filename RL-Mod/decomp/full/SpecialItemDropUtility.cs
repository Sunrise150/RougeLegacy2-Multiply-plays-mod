using System.Collections.Generic;
using UnityEngine;

public static class SpecialItemDropUtility
{
	private static List<EquipmentObj> m_equipmentListHelper = new List<EquipmentObj>();

	private static List<RuneType> m_runeListHelper = new List<RuneType>();

	private static List<ChallengeType> m_challengeListHelper = new List<ChallengeType>();

	private static List<ChallengeType> m_challengeGoldListHelper = new List<ChallengeType>();

	public static IBlueprintDrop GetBlueprintDrop(int chestLevel, int chestRarityLevel)
	{
		EquipmentObj randomEquipment = GetRandomEquipment(chestLevel, chestRarityLevel);
		if (randomEquipment == null)
		{
			Debug.Log("<color=yellow>Could not find an available blueprint drop - SpecialItemDropUtility</color>");
			return null;
		}
		return new BlueprintDrop(randomEquipment.CategoryType, randomEquipment.EquipmentType);
	}

	public static IBlueprintDrop GetFinalBlueprintDrop(EquipmentCategoryType categoryType)
	{
		EquipmentObj randomFinalEquipment = GetRandomFinalEquipment(categoryType);
		if (randomFinalEquipment == null)
		{
			Debug.Log("<color=yellow>Could not find an available final blueprint drop - SpecialItemDropUtility</color>");
			return null;
		}
		return new BlueprintDrop(randomFinalEquipment.CategoryType, randomFinalEquipment.EquipmentType);
	}

	public static IRuneDrop GetRuneDrop(int chestLevel)
	{
		RuneType randomRune = GetRandomRune(chestLevel);
		if (randomRune == RuneType.None)
		{
			Debug.Log("<color=yellow>Could not find an available rune drop - SpecialItemDropUtility</color>");
			return null;
		}
		return new RuneDrop(randomRune);
	}

	public static IChallengeDrop GetChallengeDrop()
	{
		ChallengeType randomChallenge = GetRandomChallenge();
		if (randomChallenge == ChallengeType.None)
		{
			Debug.Log("<color=yellow>Could not find an available challenge drop - SpecialItemDropUtility</color>");
			return null;
		}
		return new ChallengeDrop(randomChallenge);
	}

	public static IRelicDrop GetRandomRelicDrop()
	{
		RelicType[] typeArray = RelicType_RL.TypeArray;
		return new RelicDrop(typeArray[Random.Range(1, typeArray.Length)], RelicModType.None);
	}

	public static IWeaponDrop GetWeaponDrop()
	{
		return new WeaponDrop(AbilityType.AxeWeapon);
	}

	private static EquipmentObj GetRandomEquipment(int chestLevel, int chestRarityLevel)
	{
		m_equipmentListHelper.Clear();
		float minEquipmentLevelScale = EquipmentManager.GetMinEquipmentLevelScale();
		EquipmentCategoryType[] categoryTypeArray = EquipmentType_RL.CategoryTypeArray;
		foreach (EquipmentCategoryType equipmentCategoryType in categoryTypeArray)
		{
			if (equipmentCategoryType == EquipmentCategoryType.None)
			{
				continue;
			}
			EquipmentType[] typeArray = EquipmentType_RL.TypeArray;
			foreach (EquipmentType equipmentType in typeArray)
			{
				if (equipmentType == EquipmentType.None)
				{
					continue;
				}
				EquipmentData equipmentData = EquipmentLibrary.GetEquipmentData(equipmentCategoryType, equipmentType);
				EquipmentObj equipment = EquipmentManager.GetEquipment(equipmentCategoryType, equipmentType);
				if (equipment.IsNativeNull() || !equipmentData || equipmentData.Disabled)
				{
					continue;
				}
				if (equipment.FoundState == FoundState.NotFound)
				{
					bool flag = (float)chestLevel >= (float)equipmentData.ChestLevelRequirement + minEquipmentLevelScale;
					bool flag2 = chestRarityLevel >= equipmentData.ChestRarityRequirement;
					if (!equipment.HasMaxBlueprints && flag && flag2)
					{
						m_equipmentListHelper.Add(equipment);
					}
				}
				else
				{
					bool flag3 = (float)chestLevel >= (float)(equipmentData.ChestLevelRequirement + (EquipmentManager.GetUpgradeBlueprintsFound(equipment.CategoryType, equipment.EquipmentType, ignoreInfinitePurchasePower: true) + 1) * equipmentData.ScalingItemLevel) + minEquipmentLevelScale;
					bool flag4 = chestRarityLevel >= equipmentData.ChestRarityRequirement;
					if (!equipment.HasMaxBlueprints && flag3 && flag4)
					{
						m_equipmentListHelper.Add(equipment);
					}
				}
			}
		}
		if (m_equipmentListHelper != null && m_equipmentListHelper.Count > 0)
		{
			int index = Random.Range(0, m_equipmentListHelper.Count);
			return m_equipmentListHelper[index];
		}
		return null;
	}

	private static EquipmentObj GetRandomFinalEquipment(EquipmentCategoryType categoryType)
	{
		m_equipmentListHelper.Clear();
		int newGamePlusLevel = SaveManager.PlayerSaveData.NewGamePlusLevel;
		EquipmentObj equipment = EquipmentManager.GetEquipment(categoryType, EquipmentType.GEAR_REVIVE);
		EquipmentObj equipment2 = EquipmentManager.GetEquipment(categoryType, EquipmentType.GEAR_FINAL_BOSS);
		m_equipmentListHelper.Add(equipment);
		m_equipmentListHelper.Add(equipment2);
		for (int i = 0; i < m_equipmentListHelper.Count; i++)
		{
			EquipmentObj equipmentObj = m_equipmentListHelper[i];
			EquipmentData equipmentData = EquipmentLibrary.GetEquipmentData(equipmentObj.CategoryType, equipmentObj.EquipmentType);
			bool flag = false;
			if (equipmentObj.IsNativeNull())
			{
				flag = true;
			}
			else if (!equipmentData || ((bool)equipmentData && equipmentData.Disabled))
			{
				flag = true;
			}
			else if (equipmentObj.FoundState != FoundState.NotFound)
			{
				bool num = equipmentObj.UpgradeBlueprintsFound <= newGamePlusLevel;
				bool flag2 = !equipmentObj.HasMaxBlueprints;
				if (!num || !flag2)
				{
					flag = true;
				}
			}
			if (flag)
			{
				m_equipmentListHelper.RemoveAt(i);
				i--;
			}
		}
		if (m_equipmentListHelper != null && m_equipmentListHelper.Count > 0)
		{
			int index = Random.Range(0, m_equipmentListHelper.Count);
			return m_equipmentListHelper[index];
		}
		return null;
	}

	private static RuneType GetRandomRune(int chestLevel)
	{
		m_runeListHelper.Clear();
		float minRuneLevelScale = RuneManager.GetMinRuneLevelScale();
		RuneType[] typeArray = RuneType_RL.TypeArray;
		foreach (RuneType runeType in typeArray)
		{
			if (runeType == RuneType.None)
			{
				continue;
			}
			RuneData runeData = RuneLibrary.GetRuneData(runeType);
			RuneObj rune = RuneManager.GetRune(runeType);
			if (!runeData || rune.IsNativeNull() || runeData.Disabled)
			{
				continue;
			}
			if (rune.FoundState == FoundState.NotFound)
			{
				bool flag = (float)chestLevel >= (float)runeData.BaseItemLevel + minRuneLevelScale;
				bool flag2 = HasHeirloom(runeType);
				if (rune != null && !rune.HasMaxBlueprints && flag && flag2)
				{
					m_runeListHelper.Add(runeType);
				}
			}
			else
			{
				bool flag3 = (float)chestLevel >= (float)(runeData.BaseItemLevel + (RuneManager.GetUpgradeBlueprintsFound(runeType, ignoreInfinitePurchasePower: true) + 1) * runeData.ScalingItemLevel) + minRuneLevelScale;
				bool flag4 = HasHeirloom(runeType);
				if (rune != null && !rune.HasMaxBlueprints && flag3 && flag4)
				{
					m_runeListHelper.Add(runeType);
				}
			}
		}
		if (m_runeListHelper.Count > 0)
		{
			int index = Random.Range(0, m_runeListHelper.Count);
			return m_runeListHelper[index];
		}
		return RuneType.None;
	}

	private static ChallengeType GetRandomChallenge()
	{
		m_challengeListHelper.Clear();
		m_challengeGoldListHelper.Clear();
		ChallengeType[] typeArray = ChallengeType_RL.TypeArray;
		foreach (ChallengeType challengeType in typeArray)
		{
			if (challengeType == ChallengeType.None)
			{
				continue;
			}
			ChallengeData challengeData = ChallengeLibrary.GetChallengeData(challengeType);
			ChallengeObj challenge = ChallengeManager.GetChallenge(challengeType);
			if ((bool)challengeData && !challenge.IsNativeNull() && !challengeData.Disabled && challenge.FoundState != FoundState.NotFound && !challenge.HasMaxBlueprints)
			{
				if (ChallengeManager.GetChallengeTrophyRank(challengeType) != ChallengeTrophyRank.Gold)
				{
					m_challengeListHelper.Add(challengeType);
				}
				else
				{
					m_challengeGoldListHelper.Add(challengeType);
				}
			}
		}
		if (m_challengeListHelper.Count <= 0)
		{
			m_challengeListHelper.AddRange(m_challengeGoldListHelper);
		}
		if (m_challengeListHelper.Count > 0)
		{
			int index = Random.Range(0, m_challengeListHelper.Count);
			return m_challengeListHelper[index];
		}
		return ChallengeType.None;
	}

	private static bool HasHeirloom(RuneType runeType)
	{
		return runeType switch
		{
			RuneType.DoubleJump => SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockDoubleJump) > 0, 
			RuneType.Dash => SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockAirDash) > 0, 
			_ => true, 
		};
	}
}
