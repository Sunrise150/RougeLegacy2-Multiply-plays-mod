using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ModeSaveData : IVersionUpdateable
{
	public int REVISION_NUMBER = 2;

	public int FILE_NUMBER;

	public ModSetting CustomModSetting;

	public int SoulSpent;

	public int MiscSoulCollected;

	public int SoulSwapResourcesSpent;

	public ClassType SoulShopClassChosen;

	public AbilityType SoulShopSpellChosen;

	public bool HasBronzeSisyphusTrophy;

	public bool HasSilverSisyphusTrophy;

	public bool HasGoldSisyphusTrophy;

	public Dictionary<ChallengeType, ChallengeObj> ChallengeDict;

	public Dictionary<GameModeType, HighestNGBossBeatenEntry> HighestNGBossBeatenDict;

	public Dictionary<SoulShopType, SoulShopObj> SoulShopTable;

	public int[] HighestNGBlackChestOpenedArray;

	public Dictionary<GameModeType, Dictionary<EnemyTypeAndRank, int>> EnemiesDefeatedDict;

	public Dictionary<GameModeType, Dictionary<EnemyTypeAndRank, int>> TimesDefeatedByEnemies;

	public Dictionary<AchievementType, bool> AchievementUnlockedDict;

	public bool IsInitialized { get; private set; }

	public ModeSaveData()
	{
		Initialize();
	}

	public void Initialize()
	{
		InitializeChallengeDict();
		InitializeHighestNGBossBeatenDict();
		InitializeSoulShopTable();
		InitializeHighestNGBlackChestsOpenedArray();
		InitializeEnemiesDefeatedDict();
		InitializeAchievementUnlockedDict();
		IsInitialized = true;
	}

	private void InitializeChallengeDict()
	{
		if (ChallengeDict == null)
		{
			ChallengeDict = new Dictionary<ChallengeType, ChallengeObj>();
		}
		ChallengeType[] typeArray = ChallengeType_RL.TypeArray;
		foreach (ChallengeType challengeType in typeArray)
		{
			if (challengeType != 0 && !ChallengeDict.ContainsKey(challengeType))
			{
				ChallengeDict.Add(challengeType, new ChallengeObj(challengeType));
			}
		}
		SetStartingModeSaveData();
	}

	private void InitializeHighestNGBossBeatenDict()
	{
		if (HighestNGBossBeatenDict == null)
		{
			HighestNGBossBeatenDict = new Dictionary<GameModeType, HighestNGBossBeatenEntry>();
		}
		GameModeType[] typeArray = GameModeType_RL.TypeArray;
		foreach (GameModeType key in typeArray)
		{
			if (!HighestNGBossBeatenDict.ContainsKey(key))
			{
				HighestNGBossBeatenDict.Add(key, new HighestNGBossBeatenEntry());
			}
		}
	}

	private void InitializeSoulShopTable()
	{
		if (SoulShopTable == null)
		{
			SoulShopTable = new Dictionary<SoulShopType, SoulShopObj>();
		}
		SoulShopType[] typeArray = SoulShopType_RL.TypeArray;
		foreach (SoulShopType soulShopType in typeArray)
		{
			if (soulShopType != 0 && !SoulShopTable.ContainsKey(soulShopType))
			{
				SoulShopTable.Add(soulShopType, new SoulShopObj(soulShopType));
			}
		}
	}

	private void InitializeHighestNGBlackChestsOpenedArray()
	{
		if (HighestNGBlackChestOpenedArray == null)
		{
			HighestNGBlackChestOpenedArray = new int[5];
			for (int i = 0; i < HighestNGBlackChestOpenedArray.Length; i++)
			{
				HighestNGBlackChestOpenedArray[i] = -1;
			}
		}
	}

	private void InitializeEnemiesDefeatedDict()
	{
		if (EnemiesDefeatedDict == null)
		{
			EnemiesDefeatedDict = new Dictionary<GameModeType, Dictionary<EnemyTypeAndRank, int>>();
		}
		if (TimesDefeatedByEnemies == null)
		{
			TimesDefeatedByEnemies = new Dictionary<GameModeType, Dictionary<EnemyTypeAndRank, int>>();
		}
		GameModeType[] typeArray = GameModeType_RL.TypeArray;
		foreach (GameModeType gameModeType in typeArray)
		{
			if (gameModeType != 0)
			{
				continue;
			}
			if (!EnemiesDefeatedDict.ContainsKey(gameModeType))
			{
				EnemiesDefeatedDict.Add(gameModeType, new Dictionary<EnemyTypeAndRank, int>());
			}
			if (!TimesDefeatedByEnemies.ContainsKey(gameModeType))
			{
				TimesDefeatedByEnemies.Add(gameModeType, new Dictionary<EnemyTypeAndRank, int>());
			}
			Dictionary<EnemyTypeAndRank, int> dictionary = EnemiesDefeatedDict[gameModeType];
			Dictionary<EnemyTypeAndRank, int> dictionary2 = TimesDefeatedByEnemies[gameModeType];
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
						EnemyTypeAndRank key = new EnemyTypeAndRank(enemyType, enemyRank);
						if (!dictionary.ContainsKey(key))
						{
							dictionary.Add(key, 0);
						}
						if (!dictionary2.ContainsKey(key))
						{
							dictionary2.Add(key, 0);
						}
					}
				}
			}
		}
	}

	private void InitializeAchievementUnlockedDict()
	{
		if (AchievementUnlockedDict == null)
		{
			AchievementUnlockedDict = new Dictionary<AchievementType, bool>();
		}
		AchievementType[] typeArray = AchievementType_RL.TypeArray;
		foreach (AchievementType achievementType in typeArray)
		{
			if (achievementType != 0 && !AchievementUnlockedDict.ContainsKey(achievementType))
			{
				AchievementUnlockedDict.Add(achievementType, value: false);
			}
		}
	}

	public int GetHighestNGBossBeaten(GameModeType gameMode, BossID bossID)
	{
		if (HighestNGBossBeatenDict.TryGetValue(gameMode, out var value))
		{
			return value.GetHighestNGBossBeaten(bossID);
		}
		throw new KeyNotFoundException("Cannot GET HighestBossBeaten value: " + gameMode.ToString() + " not found in Mode Save Data.");
	}

	public void SetHighestNGBossBeaten(GameModeType gameMode, BossID bossID, int highestBossBeaten, bool forceOverride)
	{
		if (HighestNGBossBeatenDict.TryGetValue(gameMode, out var value))
		{
			value.SetHighestNGBossBeaten(bossID, highestBossBeaten, forceOverride);
			return;
		}
		throw new KeyNotFoundException("Cannot SET HighestBossBeaten value: " + gameMode.ToString() + " not found in Mode Save Data.");
	}

	public int GetHighestNGBlackChestOpened(EquipmentCategoryType categoryType)
	{
		int num = (int)(categoryType - 1);
		if (num >= 0 && num < HighestNGBlackChestOpenedArray.Length)
		{
			return HighestNGBlackChestOpenedArray[num];
		}
		throw new KeyNotFoundException("Cannot GET GetHighestNGBlackChestOpened value: " + categoryType.ToString() + " not found in Mode Save Data.");
	}

	public void SetHighestNGBlackChestOpened(EquipmentCategoryType categoryType, int value, bool additive, bool overrideEvenIfLower)
	{
		int num = 0;
		int num2 = (int)(categoryType - 1);
		if (num2 >= 0 && num2 < HighestNGBlackChestOpenedArray.Length)
		{
			num = HighestNGBlackChestOpenedArray[num2];
			int num3 = (additive ? (num + value) : value);
			if (overrideEvenIfLower || num3 > num)
			{
				HighestNGBlackChestOpenedArray[num2] = num3;
			}
			return;
		}
		throw new KeyNotFoundException("Cannot SET GetHighestNGBlackChestOpened value: " + categoryType.ToString() + " not found in Mode Save Data.");
	}

	public SoulShopObj GetSoulShopObj(SoulShopType soulShopType)
	{
		if (SoulShopTable.TryGetValue(soulShopType, out var value))
		{
			return value;
		}
		Debug.Log("Cannot GET SoulShop Type: " + soulShopType.ToString() + " because entry not found in Mode Save Data.");
		return null;
	}

	public int GetTotalSoulShopObjOwnedLevel()
	{
		int num = 0;
		foreach (KeyValuePair<SoulShopType, SoulShopObj> item in SoulShopTable)
		{
			SoulShopObj value = item.Value;
			if (!value.SoulShopData.Disabled)
			{
				num += value.CurrentOwnedLevel;
			}
		}
		return num;
	}

	public void SetEnemiesDefeated(GameModeType gameMode, EnemyType enemyType, EnemyRank enemyRank, int value, bool additive)
	{
		if (EnemiesDefeatedDict.TryGetValue(gameMode, out var value2))
		{
			EnemyTypeAndRank key = new EnemyTypeAndRank(enemyType, enemyRank);
			if (value2.ContainsKey(key))
			{
				int num = value2[key];
				int value3 = (additive ? (num + value) : value);
				value2[key] = value3;
				return;
			}
			throw new KeyNotFoundException("Cannot SET Enemies Defeated for EnemyTypeAndRank: " + key.ToString() + " not found in Mode Save Data.");
		}
		throw new KeyNotFoundException("Cannot SET Enemies Defeated for GameMode: " + gameMode.ToString() + " not found in Mode Save Data.");
	}

	public int GetEnemiesDefeated(GameModeType gameMode, EnemyType enemyType, EnemyRank enemyRank)
	{
		if (EnemiesDefeatedDict.TryGetValue(gameMode, out var value))
		{
			EnemyTypeAndRank key = new EnemyTypeAndRank(enemyType, enemyRank);
			if (value.TryGetValue(key, out var value2))
			{
				return value2;
			}
			throw new KeyNotFoundException("Cannot GET Enemies Defeated for EnemyTypeAndRank: " + key.ToString() + " not found in Mode Save Data.");
		}
		throw new KeyNotFoundException("Cannot GET Enemies Defeated for GameMode: " + gameMode.ToString() + " not found in Mode Save Data.");
	}

	public void SetTimesDefeatedByEnemy(GameModeType gameMode, EnemyType enemyType, EnemyRank enemyRank, int value, bool additive)
	{
		if (TimesDefeatedByEnemies.TryGetValue(gameMode, out var value2))
		{
			EnemyTypeAndRank key = new EnemyTypeAndRank(enemyType, enemyRank);
			if (value2.ContainsKey(key))
			{
				int num = value2[key];
				int value3 = (additive ? (num + value) : value);
				value2[key] = value3;
				return;
			}
			throw new KeyNotFoundException("Cannot SET Times Defeated By Enemy for EnemyTypeAndRank: " + key.ToString() + " not found in Mode Save Data.");
		}
		throw new KeyNotFoundException("Cannot SET Times Defeated By Enemy for GameMode: " + gameMode.ToString() + " not found in Mode Save Data.");
	}

	public int GetTimesDefeatedByEnemy(GameModeType gameMode, EnemyType enemyType, EnemyRank enemyRank)
	{
		if (TimesDefeatedByEnemies.TryGetValue(gameMode, out var value))
		{
			EnemyTypeAndRank key = new EnemyTypeAndRank(enemyType, enemyRank);
			if (value.TryGetValue(key, out var value2))
			{
				return value2;
			}
			throw new KeyNotFoundException("Cannot GET Times Defeated By Enemy for EnemyTypeAndRank: " + key.ToString() + " not found in Mode Save Data.");
		}
		throw new KeyNotFoundException("Cannot GET Times Defeated By Enemy for GameMode: " + gameMode.ToString() + " not found in Mode Save Data.");
	}

	public bool GetAchievementUnlocked(AchievementType achievementType)
	{
		if (AchievementUnlockedDict.TryGetValue(achievementType, out var value))
		{
			return value;
		}
		return false;
	}

	public bool SetAchievementUnlocked(AchievementType achievementType, bool unlocked, bool forceOverride = false)
	{
		if (AchievementUnlockedDict.ContainsKey(achievementType))
		{
			if (AchievementUnlockedDict[achievementType])
			{
				return false;
			}
			if (unlocked || forceOverride)
			{
				AchievementUnlockedDict[achievementType] = unlocked;
			}
			return true;
		}
		return false;
	}

	private void SetStartingModeSaveData()
	{
		if (ChallengeDict[ChallengeType.Tutorial].FoundLevel <= -3)
		{
			ChallengeDict[ChallengeType.Tutorial].FoundLevel = -2;
		}
	}

	public void UpdateVersion()
	{
		Initialize();
		if (REVISION_NUMBER <= 0)
		{
			if (ChallengeDict.TryGetValue(ChallengeType.Tutorial, out var value) && value.EquippedLevel > 0)
			{
				value.EquippedLevel = 0;
			}
			foreach (ChallengeType item in ChallengeManager.GetAllChallengesWithFoundState(FoundState.FoundAndViewed))
			{
				if (ChallengeManager.GetChallengeTrophyRank(item) == ChallengeTrophyRank.Gold)
				{
					ChallengeObj challenge = ChallengeManager.GetChallenge(item);
					ChallengeManager.SetUpgradeBlueprintsFound(item, challenge.MaxLevel, additive: false);
				}
			}
		}
		if (REVISION_NUMBER <= 1)
		{
			ResetSoulShopEntries();
		}
		REVISION_NUMBER = 2;
	}

	public static ModSetting GetModSetting(GameModeType modeType)
	{
		switch (modeType)
		{
		default:
		{
			ModSetting result = default(ModSetting);
			result.WorldRandomizes = true;
			return result;
		}
		case GameModeType.StaticCastle:
		{
			ModSetting result = default(ModSetting);
			result.WorldRandomizes = false;
			return result;
		}
		case GameModeType.Custom:
			return SaveManager.ModeSaveData.CustomModSetting;
		}
	}

	public void ResetSoulShopEntries()
	{
		foreach (KeyValuePair<SoulShopType, SoulShopObj> item in SoulShopTable)
		{
			SoulShopObj value = item.Value;
			value.SetOwnedLevel(0, additive: false, clampToMaxLevel: false);
			value.SetEquippedLevel(0, additive: false, broadcast: false);
		}
	}
}
