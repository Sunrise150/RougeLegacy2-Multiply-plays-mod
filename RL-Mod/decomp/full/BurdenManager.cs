using UnityEngine;

public class BurdenManager : MonoBehaviour
{
	private static BurdenManager m_instance;

	private static BurdenManager Instance
	{
		get
		{
			return m_instance;
		}
		set
		{
			m_instance = value;
		}
	}

	public static bool IsInitialized { get; private set; }

	private void Awake()
	{
		Instance = this;
		IsInitialized = true;
	}

	private void OnDestroy()
	{
		IsInitialized = false;
	}

	public static BurdenObj GetBurden(BurdenType burdenType)
	{
		if (burdenType == BurdenType.None)
		{
			return null;
		}
		return SaveManager.PlayerSaveData.GetBurden(burdenType);
	}

	public static bool IsBurdenActive(BurdenType burdenType)
	{
		BurdenObj burden = GetBurden(burdenType);
		if (!burden.IsNativeNull())
		{
			return burden.CurrentLevel > 0;
		}
		return false;
	}

	public static void SetBurdenLevel(BurdenType burdenType, int level, bool additive, bool broadcast = true)
	{
		BurdenObj burden = GetBurden(burdenType);
		if (!burden.IsNativeNull())
		{
			burden.SetLevel(level, additive, broadcast);
		}
	}

	public static int GetBurdenLevel(BurdenType burdenType)
	{
		BurdenObj burden = GetBurden(burdenType);
		if (!burden.IsNativeNull() && IsBurdenUnlocked(burdenType))
		{
			return burden.CurrentLevel;
		}
		return 0;
	}

	public static int GetTotalBurdenLevel()
	{
		int num = 0;
		BurdenType[] typeArray = BurdenType_RL.TypeArray;
		foreach (BurdenType burdenType in typeArray)
		{
			if (burdenType != 0)
			{
				num += GetBurdenLevel(burdenType);
			}
		}
		return num;
	}

	public static int GetTotalBurdenMaxLevel()
	{
		int num = 0;
		BurdenType[] typeArray = BurdenType_RL.TypeArray;
		foreach (BurdenType burdenType in typeArray)
		{
			if (burdenType != 0)
			{
				num += GetBurden(burdenType).MaxLevel;
			}
		}
		return num;
	}

	public static float GetBurdenStatGain(BurdenType burdenType)
	{
		BurdenObj burden = GetBurden(burdenType);
		if (!burden.IsNativeNull() && IsBurdenUnlocked(burdenType))
		{
			return burden.CurrentStatGain;
		}
		return 0f;
	}

	public static int GetBurdenWeight(BurdenType burdenType)
	{
		BurdenObj burden = GetBurden(burdenType);
		if (!burden.IsNativeNull() && IsBurdenUnlocked(burdenType))
		{
			return burden.CurrentBurdenWeight;
		}
		return 0;
	}

	public static int GetTotalBurdenWeight()
	{
		int num = 0;
		BurdenType[] typeArray = BurdenType_RL.TypeArray;
		foreach (BurdenType burdenType in typeArray)
		{
			if (burdenType != 0)
			{
				num += GetBurdenWeight(burdenType);
			}
		}
		return num;
	}

	public static FoundState GetFoundState(BurdenType burdenType)
	{
		BurdenObj burden = GetBurden(burdenType);
		if (!burden.IsNativeNull())
		{
			return burden.FoundState;
		}
		return FoundState.NotFound;
	}

	public static void SetFoundState(BurdenType burdenType, FoundState foundState, bool overrideValues)
	{
		BurdenObj burden = GetBurden(burdenType);
		if (!burden.IsNativeNull() && (overrideValues || foundState > burden.FoundState))
		{
			burden.FoundState = foundState;
		}
	}

	public static bool IsBurdenUnlocked(BurdenType burdenType)
	{
		BurdenObj burden = GetBurden(burdenType);
		switch (burdenType)
		{
		case BurdenType.CastleBossUp:
			return SaveManager.PlayerSaveData.HighestNGPlusBeaten >= 0;
		case BurdenType.BridgeBossUp:
			return SaveManager.PlayerSaveData.HighestNGPlusBeaten >= 1;
		case BurdenType.ForestBossUp:
			return SaveManager.PlayerSaveData.HighestNGPlusBeaten >= 2;
		case BurdenType.StudyBossUp:
			return SaveManager.PlayerSaveData.HighestNGPlusBeaten >= 3;
		case BurdenType.TowerBossUp:
			return SaveManager.PlayerSaveData.HighestNGPlusBeaten >= 4;
		case BurdenType.CaveBossUp:
			return SaveManager.PlayerSaveData.HighestNGPlusBeaten >= 5;
		case BurdenType.FinalBossUp:
			return SaveManager.PlayerSaveData.HighestNGPlusBeaten >= 6;
		case BurdenType.CastleBiomeUp:
			return SaveManager.PlayerSaveData.HighestNGPlusBeaten >= 1;
		case BurdenType.BridgeBiomeUp:
			return SaveManager.PlayerSaveData.HighestNGPlusBeaten >= 2;
		case BurdenType.ForestBiomeUp:
			return SaveManager.PlayerSaveData.HighestNGPlusBeaten >= 3;
		case BurdenType.StudyBiomeUp:
			return SaveManager.PlayerSaveData.HighestNGPlusBeaten >= 4;
		case BurdenType.TowerBiomeUp:
			return SaveManager.PlayerSaveData.HighestNGPlusBeaten >= 5;
		case BurdenType.CaveBiomeUp:
			return SaveManager.PlayerSaveData.HighestNGPlusBeaten >= 6;
		default:
			if (!burden.IsNativeNull() && !burden.BurdenData.Disabled)
			{
				return true;
			}
			return false;
		}
	}

	public static int BurdenRequiredForNG(int ngPlusLevel)
	{
		int num = NewGamePlus_EV.GetBurdensRequiredForNG(ngPlusLevel);
		int num2 = 30;
		if (SaveManager.PlayerSaveData.EnableHouseRules)
		{
			num = Mathf.FloorToInt((float)num * SaveManager.PlayerSaveData.Assist_BurdenRequirementsMod);
			num2 = Mathf.FloorToInt((float)num2 * SaveManager.PlayerSaveData.Assist_BurdenRequirementsMod);
		}
		int max = Mathf.Min(GetTotalBurdenMaxLevel(), num2);
		num = Mathf.Clamp(num, 0, max);
		int totalBurdenWeight = GetTotalBurdenWeight();
		return num - totalBurdenWeight;
	}

	public static bool CanEnterNewGamePlus(int ngPlusLevel)
	{
		return BurdenRequiredForNG(ngPlusLevel) <= 0;
	}
}
