using System;
using UnityEngine;

[Serializable]
public class EquipmentObj
{
	public EquipmentCategoryType CategoryType;

	public EquipmentType EquipmentType;

	public int UpgradeLevel = -3;

	public int UpgradeBlueprintsFound;

	public FoundState FoundState
	{
		get
		{
			if (EquipmentData.Disabled)
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

	public bool IsMaxUpgradeLevel => ClampedUpgradeLevel == MaxLevel;

	public bool HasMaxBlueprints
	{
		get
		{
			if (MaxLevel == 0)
			{
				return FoundState > FoundState.NotFound;
			}
			return UpgradeBlueprintsFound >= MaxLevel;
		}
	}

	public int MaxLevel
	{
		get
		{
			if (EquipmentData == null)
			{
				return 0;
			}
			int num = EquipmentData.MaximumLevel;
			SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType.MaxEquipmentDrops);
			if (!soulShopObj.IsNativeNull())
			{
				num += soulShopObj.CurrentEquippedLevel;
			}
			return Mathf.Clamp(num, 0, Economy_EV.EQUIPMENT_LEVEL_GOLD_MOD.Length - 1);
		}
	}

	public int GoldCostToUpgrade
	{
		get
		{
			if (EquipmentData.Disabled)
			{
				return 0;
			}
			int goldCost = EquipmentData.GoldCost;
			int num = UpgradeLevel;
			if (num >= 0)
			{
				num++;
			}
			num = Mathf.Clamp(num, 0, Economy_EV.EQUIPMENT_LEVEL_GOLD_MOD.Length - 1);
			goldCost *= Economy_EV.EQUIPMENT_LEVEL_GOLD_MOD[num];
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
			if (EquipmentData.Disabled)
			{
				return 0;
			}
			int oreCost = EquipmentData.OreCost;
			int num = UpgradeLevel;
			if (num >= 0)
			{
				num++;
			}
			num = Mathf.Clamp(num, 0, Economy_EV.EQUIPMENT_LEVEL_ORE_MOD.Length - 1);
			return oreCost * Economy_EV.EQUIPMENT_LEVEL_ORE_MOD[num];
		}
	}

	public int EquipmentSetLevel
	{
		get
		{
			if (EquipmentData.Disabled)
			{
				return 0;
			}
			return EquipmentData.BaseEquipmentSetLevel + EquipmentData.ScalingEquipmentSetLevel * ClampedUpgradeLevel;
		}
	}

	public EquipmentData EquipmentData => EquipmentLibrary.GetEquipmentData(CategoryType, EquipmentType);

	public float GetStatValueAtLevel(EquipmentStatType statType, int level)
	{
		if (EquipmentData.Disabled)
		{
			return 0f;
		}
		if (level < 0)
		{
			return 0f;
		}
		level = Mathf.Clamp(level, 0, MaxLevel);
		return statType switch
		{
			EquipmentStatType.Weight => EquipmentData.BaseWeight + EquipmentData.ScalingWeight * level, 
			EquipmentStatType.Cooldown => 0f, 
			EquipmentStatType.Strength => EquipmentData.BaseStrengthDamage + EquipmentData.ScalingStrengthDamage * level, 
			EquipmentStatType.Vitality => EquipmentData.BaseHealth + EquipmentData.ScalingHealth * level, 
			EquipmentStatType.Magic => EquipmentData.BaseMagicDamage + EquipmentData.ScalingMagicDamage * level, 
			EquipmentStatType.Armor => EquipmentData.BaseArmor + EquipmentData.ScalingArmor * level, 
			EquipmentStatType.Dexterity_Add => EquipmentData.BaseStrengthCritChance + EquipmentData.ScalingStrengthCritChance * (float)level, 
			EquipmentStatType.CritDamage => EquipmentData.BaseStrengthCritDamage + EquipmentData.ScalingStrengthCritDamage * (float)level, 
			EquipmentStatType.Focus_Add => EquipmentData.BaseMagicCritChance + EquipmentData.ScalingMagicCritChance * (float)level, 
			EquipmentStatType.MagicCritDamage => EquipmentData.BaseMagicCritDamage + EquipmentData.ScalingMagicCritDamage * (float)level, 
			EquipmentStatType.Unity => EquipmentData.BaseEquipmentSetLevel + EquipmentData.ScalingEquipmentSetLevel * level, 
			_ => 0f, 
		};
	}

	public float GetCurrentStatValue(EquipmentStatType statType)
	{
		if (EquipmentData.Disabled)
		{
			return 0f;
		}
		return GetStatValueAtLevel(statType, ClampedUpgradeLevel);
	}

	public EquipmentObj(EquipmentCategoryType category, EquipmentType equipType)
	{
		CategoryType = category;
		EquipmentType = equipType;
	}
}
