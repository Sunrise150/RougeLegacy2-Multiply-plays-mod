using System;
using UnityEngine;

[Serializable]
public class SkillTreeObj
{
	public SkillTreeType SkillTreeType;

	public int Level;

	public int ClampedLevel => Mathf.Clamp(Level, 0, MaxLevel);

	public float FirstLevelStatGain => SkillTreeData.FirstLevelStatGain;

	public float AdditionalLevelStatGain => SkillTreeData.AdditionalLevelStatGain;

	public float CurrentStatGain => GetStatGainAtLevel(Level);

	public int GoldCost => SkillTreeData.BaseCost + SkillTreeData.Appreciation * Level;

	public int GoldCostWithLevelAppreciation
	{
		get
		{
			int num = Mathf.Clamp(SkillTreeManager.GetTotalSkillObjLevel() - 18, 0, int.MaxValue);
			return (int)((double)GoldCost + Math.Floor((float)num * 14f / 5f) * 5.0);
		}
	}

	public bool IsLevelLocked
	{
		get
		{
			if (SkillTreeManager.IsInitialized)
			{
				return UnlockLevel > SkillTreeManager.GetTotalSkillObjLevel();
			}
			return false;
		}
	}

	public int UnlockLevel => SkillTreeData.SkillUnlockLevel;

	public bool IsLocked => SkillTreeData.SkillUnlockState == SkillUnlockState.Locked;

	public bool IsSoulLocked => MaxLevel <= 0;

	public int MaxLevel
	{
		get
		{
			int num = 0;
			if (!string.IsNullOrEmpty(SkillTreeData.SoulShopTag) && Enum.TryParse<SoulShopType>(SkillTreeData.SoulShopTag, out var result) && Enum.IsDefined(typeof(SoulShopType), result))
			{
				SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(result);
				num = soulShopObj.CurrentEquippedLevel * SkillTreeData.AdditiveSoulShopLevels;
			}
			int num2 = SkillTreeData.MaxLevel + num;
			if (SkillTreeData.OverloadLevelCap > 0)
			{
				return Mathf.Min(SkillTreeData.OverloadLevelCap, num2);
			}
			return num2;
		}
	}

	public SkillTreeData SkillTreeData => SkillTreeLibrary.GetSkillTreeData(SkillTreeType);

	public float GetStatGainAtLevel(int level)
	{
		if (level <= 0)
		{
			return 0f;
		}
		level = Mathf.Clamp(level, 0, MaxLevel);
		return FirstLevelStatGain + AdditionalLevelStatGain * (float)(level - 1);
	}

	public int GetGoldCostWithAppreciationWhenAddingLevels(int levelsToAdd)
	{
		int level = Level;
		int num = 0;
		for (int i = 0; i < levelsToAdd; i++)
		{
			num += GoldCostWithLevelAppreciation;
			Level++;
			if (Level >= MaxLevel)
			{
				break;
			}
		}
		Level = level;
		return num;
	}

	public int GetNumLevelsPurchaseableWithGold(int levelsToAdd, int goldAmount)
	{
		int level = Level;
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < levelsToAdd; i++)
		{
			num += GoldCostWithLevelAppreciation;
			if (num <= goldAmount)
			{
				num2++;
			}
			Level++;
			if (Level >= MaxLevel)
			{
				break;
			}
		}
		Level = level;
		return num2;
	}

	public SkillTreeObj(SkillTreeType skillTreeType)
	{
		SkillTreeType = skillTreeType;
	}
}
