using System;
using UnityEngine;

[Serializable]
public class BurdenObj
{
	private int m_currentLevel;

	private FoundState m_foundState = FoundState.FoundButNotViewed;

	public FoundState FoundState
	{
		get
		{
			if (BurdenData.Disabled)
			{
				return FoundState.NotFound;
			}
			if (m_foundState < FoundState.NotFound)
			{
				return FoundState.NotFound;
			}
			return m_foundState;
		}
		set
		{
			m_foundState = value;
		}
	}

	public int CurrentLevel => Mathf.Clamp(m_currentLevel, 0, MaxLevel);

	public int MaxLevel => BurdenData.MaxBurdenLevel;

	public int InitialBurdenWeight => BurdenData.InitialBurdenCost;

	public int CurrentBurdenWeight => GetBurdenWeightAtLevel(CurrentLevel);

	public float CurrentStatGain => GetStatGainAtLevel(CurrentLevel);

	public BurdenData BurdenData => BurdenLibrary.GetBurdenData(BurdenType);

	public BurdenType BurdenType { get; private set; }

	public float GetStatGainAtLevel(int level)
	{
		level = Mathf.Clamp(level, 0, MaxLevel);
		if (level <= 0)
		{
			return 0f;
		}
		return BurdenData.StatsGain * (float)level;
	}

	public int GetBurdenWeightAtLevel(int level)
	{
		level = Mathf.Clamp(level, 0, MaxLevel);
		if (level <= 0)
		{
			return 0;
		}
		return InitialBurdenWeight * level;
	}

	public void ForceLevel(int level)
	{
		m_currentLevel = level;
	}

	public void SetLevel(int value, bool additive, bool broadcast = true)
	{
		_ = m_currentLevel;
		if (additive)
		{
			m_currentLevel += value;
		}
		else
		{
			m_currentLevel = value;
		}
		m_currentLevel = Mathf.Clamp(m_currentLevel, 0, MaxLevel);
	}

	public BurdenObj(BurdenType burdenType)
	{
		BurdenType = burdenType;
	}
}
