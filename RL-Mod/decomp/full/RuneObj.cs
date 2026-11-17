using System;
using UnityEngine;

[Serializable]
public class RuneObj
{
	public RuneType RuneType;

	public int UpgradeLevel = -3;

	public int EquippedLevel;

	public int UpgradeBlueprintsFound;

	public FoundState FoundState
	{
		get
		{
			if (RuneData.Disabled)
			{
				return FoundState.NotFound;
			}
			if (UpgradeLevel < -3)
			{
				return FoundState.NotFound;
			}
			if (UpgradeLevel > 0)
			{
				return FoundState.Purchased;
			}
			return (FoundState)UpgradeLevel;
		}
	}

	public int ClampedUpgradeLevel => Mathf.Clamp(UpgradeLevel, 0, MaxLevel);

	public bool IsMaxEquippedLevel => EquippedLevel == MaxLevel;

	public bool IsMaxUpgradeLevel => ClampedUpgradeLevel == MaxLevel;

	public bool HasMaxBlueprints => UpgradeBlueprintsFound >= MaxLevel;

	public float CurrentStatModTotal_1
	{
		get
		{
			if (RuneData.Disabled)
			{
				return 0f;
			}
			if (EquippedLevel <= 0)
			{
				return 0f;
			}
			return RuneData.StatMod01 + (float)(EquippedLevel - 1) * RuneData.ScalingStatMod01;
		}
	}

	public float StatModTotal_2
	{
		get
		{
			if (RuneData.Disabled)
			{
				return 0f;
			}
			if (EquippedLevel <= 0)
			{
				return 0f;
			}
			return RuneData.StatMod02 + (float)(EquippedLevel - 1) * RuneData.ScalingStatMod02;
		}
	}

	public float StatModTotal_3
	{
		get
		{
			if (RuneData.Disabled)
			{
				return 0f;
			}
			if (EquippedLevel <= 0)
			{
				return 0f;
			}
			return RuneData.StatMod03 + (float)(EquippedLevel - 1) * RuneData.ScalingStatMod03;
		}
	}

	public int CurrentWeight
	{
		get
		{
			if (RuneData.Disabled)
			{
				return 0;
			}
			if (EquippedLevel <= 0)
			{
				return 0;
			}
			return RuneData.BaseWeight + RuneData.ScalingWeight * (EquippedLevel - 1);
		}
	}

	public int GoldCostToUpgrade
	{
		get
		{
			if (RuneData.Disabled)
			{
				return 0;
			}
			int goldCost = RuneData.GoldCost;
			int clampedUpgradeLevel = ClampedUpgradeLevel;
			clampedUpgradeLevel = Mathf.Clamp(clampedUpgradeLevel, 0, Economy_EV.RUNE_LEVEL_GOLD_MOD.Length - 1);
			goldCost *= Economy_EV.RUNE_LEVEL_GOLD_MOD[clampedUpgradeLevel];
			if (TraitManager.IsTraitActive(TraitType.LowerStorePrice))
			{
				goldCost = (int)((float)goldCost * 0.85f);
			}
			return goldCost;
		}
	}

	public int OreCostToUpgrade
	{
		get
		{
			if (RuneData.Disabled)
			{
				return 0;
			}
			int blackStoneCost = RuneData.BlackStoneCost;
			int clampedUpgradeLevel = ClampedUpgradeLevel;
			clampedUpgradeLevel = Mathf.Clamp(clampedUpgradeLevel, 0, Economy_EV.RUNE_LEVEL_ORE_MOD.Length - 1);
			return blackStoneCost * Economy_EV.RUNE_LEVEL_ORE_MOD[clampedUpgradeLevel];
		}
	}

	public int MaxLevel
	{
		get
		{
			if (!RuneData)
			{
				return 0;
			}
			int num = RuneData.MaximumLevel;
			SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType.MaxRuneDrops);
			if (!soulShopObj.IsNativeNull())
			{
				num += soulShopObj.CurrentEquippedLevel;
			}
			return num;
		}
	}

	public RuneData RuneData => RuneLibrary.GetRuneData(RuneType);

	public float GetStatModTotal_1AtLevel(int level)
	{
		if (RuneData.Disabled)
		{
			return 0f;
		}
		if (level <= 0)
		{
			return 0f;
		}
		level = Mathf.Clamp(level, 0, MaxLevel);
		return RuneData.StatMod01 + (float)(level - 1) * RuneData.ScalingStatMod01;
	}

	public int GetWeightAtLevel(int level)
	{
		if (RuneData.Disabled)
		{
			return 0;
		}
		if (level <= 0)
		{
			return 0;
		}
		level = Mathf.Clamp(level, 0, MaxLevel);
		return RuneData.BaseWeight + RuneData.ScalingWeight * (level - 1);
	}

	public RuneObj(RuneType runeType)
	{
		RuneType = runeType;
	}
}
