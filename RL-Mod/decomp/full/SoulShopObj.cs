using System;
using UnityEngine;

[Serializable]
public class SoulShopObj
{
	private int m_equippedLevel;

	private int m_ownedLevel;

	private bool m_wasViewed;

	public int CurrentOwnedLevel => m_ownedLevel;

	public int CurrentEquippedLevel => m_equippedLevel;

	public int MaxLevel
	{
		get
		{
			int overloadMaxLevel = SoulShopData.OverloadMaxLevel;
			if (overloadMaxLevel > 0)
			{
				if (SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType.UnlockOverload).CurrentEquippedLevel > 0)
				{
					return overloadMaxLevel;
				}
				return SoulShopData.MaxLevel;
			}
			return SoulShopData.MaxLevel;
		}
	}

	public int InitialCost => SoulShopData.BaseCost;

	public bool WasViewed
	{
		get
		{
			return m_wasViewed;
		}
		set
		{
			m_wasViewed = value;
		}
	}

	public float CurrentStatGain => GetStatGainAtLevel(CurrentEquippedLevel);

	public SoulShopData SoulShopData => SoulShopLibrary.GetSoulShopData(SoulShopType);

	public SoulShopType SoulShopType { get; private set; }

	public int CurrentCost(bool applyMaxLevelScalingCap)
	{
		return GetCostAtLevel(CurrentOwnedLevel + 1, applyMaxLevelScalingCap);
	}

	public int GetCostAtLevel(int level, bool applyMaxLevelScalingCap)
	{
		level = (applyMaxLevelScalingCap ? Mathf.Clamp(level, 0, SoulShopData.MaxLevelScalingCap + 1) : Mathf.Clamp(level, 0, int.MaxValue));
		return InitialCost + (level - 1) * SoulShopData.ScalingCost;
	}

	public float GetStatGainAtLevel(int level)
	{
		level = Mathf.Clamp(level, 0, int.MaxValue);
		if (level <= 0)
		{
			return 0f;
		}
		SoulShopData soulShopData = SoulShopData;
		return soulShopData.FirstLevelStatGain + (float)(level - 1) * soulShopData.AdditionalLevelStatGain;
	}

	public void SetEquippedLevel(int value, bool additive, bool broadcast = true)
	{
		_ = m_equippedLevel;
		if (additive)
		{
			m_equippedLevel += value;
		}
		else
		{
			m_equippedLevel = value;
		}
		m_equippedLevel = Mathf.Clamp(m_equippedLevel, 0, CurrentOwnedLevel);
	}

	public void SetOwnedLevel(int value, bool additive, bool clampToMaxLevel, bool broadcast = true)
	{
		_ = m_ownedLevel;
		if (additive)
		{
			m_ownedLevel += value;
		}
		else
		{
			m_ownedLevel = value;
		}
		if (clampToMaxLevel)
		{
			m_ownedLevel = Mathf.Clamp(m_ownedLevel, 0, MaxLevel);
		}
	}

	public SoulShopObj(SoulShopType soulShopType)
	{
		SoulShopType = soulShopType;
	}
}
