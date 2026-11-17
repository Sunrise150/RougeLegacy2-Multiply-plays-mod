using System.Collections.Generic;
using UnityEngine;

public class Souls_EV
{
	public static Dictionary<BossID, Vector2Int> BOSS_SOUL_DROP_TABLE = new Dictionary<BossID, Vector2Int>
	{
		{
			BossID.Castle_Boss,
			new Vector2Int(100, 100)
		},
		{
			BossID.Bridge_Boss,
			new Vector2Int(100, 100)
		},
		{
			BossID.Forest_Boss,
			new Vector2Int(100, 100)
		},
		{
			BossID.Study_Boss,
			new Vector2Int(100, 100)
		},
		{
			BossID.Tower_Boss,
			new Vector2Int(100, 100)
		},
		{
			BossID.Cave_Boss,
			new Vector2Int(100, 100)
		},
		{
			BossID.Garden_Boss,
			new Vector2Int(100, 100)
		},
		{
			BossID.Final_Boss,
			new Vector2Int(100, 100)
		}
	};

	public const int BLACK_CHEST_SOUL_DROP_AMOUNT = 50;

	public const int CHALLENGE_BRONZE_REWARD_MULT = 1;

	public const int CHALLENGE_SILVER_REWARD_MULT = 2;

	public const int CHALLENGE_GOLD_REWARD_MULT = 3;

	public const int ORE_AETHER_EXCHANGE_RATE = 2;

	public const int ORE_AETHER_EXCHANGE_FLOOR = 500;

	public static readonly int[] ORE_AETHER_TO_SOUL_COST_LEVELS = new int[111]
	{
		2500, 5000, 7500, 12500, 17500, 25000, 32500, 42500, 52500, 65000,
		77500, 92500, 107500, 125000, 142500, 162500, 182500, 205000, 227500, 252500,
		277500, 305000, 332500, 362500, 392500, 425000, 457500, 492500, 527500, 565000,
		602500, 642500, 682500, 725000, 767500, 812500, 857500, 905000, 952500, 1002500,
		1052500, 1105000, 1157500, 1212500, 1267500, 1325000, 1382500, 1442500, 1502500, 1565000,
		1627500, 1692500, 1757500, 1825000, 1892500, 1962500, 2032500, 2105000, 2177500, 2252500,
		2327500, 2405000, 2482500, 2562500, 2642500, 2725000, 2807500, 2892500, 2977500, 3065000,
		3152500, 3242500, 3332500, 3425000, 3517500, 3612500, 3707500, 3805000, 3902500, 4002500,
		4102500, 4205000, 4307500, 4412500, 4517500, 4625000, 4732500, 4842500, 4952500, 5000000,
		5000000, 5000000, 5000000, 5000000, 5000000, 5000000, 5000000, 5000000, 5000000, 5000000,
		5000000, 5000000, 5000000, 5000000, 5000000, 5000000, 5000000, 5000000, 5000000, 5000000,
		5000000
	};

	public const int SOULS_PER_LEVEL = 150;

	public const int NPC_DIALOGUE_SOUL_DROP_AMOUNT = 500;

	public static int GetSoulSwapCost(int soulLevel)
	{
		soulLevel = Mathf.Clamp(soulLevel, 0, ORE_AETHER_TO_SOUL_COST_LEVELS.Length - 1);
		return ORE_AETHER_TO_SOUL_COST_LEVELS[soulLevel];
	}

	public static int GetBossSoulsDropAmount(BossID bossID, int bossBeatenLevel)
	{
		int x = BOSS_SOUL_DROP_TABLE[bossID].x;
		int y = BOSS_SOUL_DROP_TABLE[bossID].y;
		if (bossBeatenLevel < 0)
		{
			return 0;
		}
		return x + y * bossBeatenLevel;
	}

	public static int GetBossSoulsCollected(GameModeType gameMode, BossID bossID)
	{
		int highestNGBossBeaten = SaveManager.ModeSaveData.GetHighestNGBossBeaten(gameMode, bossID);
		return GetBossSoulsDropAmount(bossID, highestNGBossBeaten);
	}

	public static int GetTotalBossSoulsCollected(GameModeType gameMode)
	{
		int num = 0;
		BossID[] typeArray = BossID_RL.TypeArray;
		foreach (BossID bossID in typeArray)
		{
			if (bossID != 0)
			{
				num += GetBossSoulsCollected(gameMode, bossID);
			}
		}
		return num;
	}

	public static int GetBlackChestSoulsDropAmount(int ngLevel)
	{
		return (ngLevel + 1) * 50;
	}

	public static int GetBlackChestSoulsCollected(EquipmentCategoryType categoryType)
	{
		int highestNGBlackChestOpened = SaveManager.ModeSaveData.GetHighestNGBlackChestOpened(categoryType);
		if (highestNGBlackChestOpened >= 0)
		{
			return (highestNGBlackChestOpened + 1) * 50;
		}
		return 0;
	}

	public static int GetTotalBlackChestSoulsCollected()
	{
		int num = 0;
		EquipmentCategoryType[] categoryTypeArray = EquipmentType_RL.CategoryTypeArray;
		foreach (EquipmentCategoryType equipmentCategoryType in categoryTypeArray)
		{
			if (equipmentCategoryType != 0)
			{
				num += GetBlackChestSoulsCollected(equipmentCategoryType);
			}
		}
		return num;
	}

	public static int GetChallengeSoulsRewarded(ChallengeType challengeType, ChallengeTrophyRank trophyRank)
	{
		int num = 0;
		switch (trophyRank)
		{
		case ChallengeTrophyRank.Bronze:
			num = 1;
			break;
		case ChallengeTrophyRank.Silver:
			num = 2;
			break;
		case ChallengeTrophyRank.Gold:
			num = 3;
			break;
		}
		ChallengeData challengeData = ChallengeLibrary.GetChallengeData(challengeType);
		if ((bool)challengeData)
		{
			return num * challengeData.Reward;
		}
		return 0;
	}

	public static int GetChallengeSoulsCollected(ChallengeType challengeType)
	{
		return GetChallengeSoulsRewarded(challengeType, ChallengeManager.GetChallengeTrophyRank(challengeType));
	}

	public static int GetTotalChallengeSoulsCollected()
	{
		int num = 0;
		ChallengeType[] typeArray = ChallengeType_RL.TypeArray;
		foreach (ChallengeType challengeType in typeArray)
		{
			if (challengeType != 0)
			{
				num += GetChallengeSoulsCollected(challengeType);
			}
		}
		return num;
	}

	public static int GetTotalNPCDialogueSoulsCollected()
	{
		return NPCDialogueManager.GetTotalCompleteDialogues() * 500;
	}

	public static int GetTotalSoulsCollected(GameModeType gameMode, bool deductSpentSouls)
	{
		int totalBossSoulsCollected = GetTotalBossSoulsCollected(gameMode);
		int totalChallengeSoulsCollected = GetTotalChallengeSoulsCollected();
		int totalNPCDialogueSoulsCollected = GetTotalNPCDialogueSoulsCollected();
		int totalBlackChestSoulsCollected = GetTotalBlackChestSoulsCollected();
		int miscSoulCollected = SaveManager.ModeSaveData.MiscSoulCollected;
		int num = 0;
		if (deductSpentSouls)
		{
			foreach (SoulShopObj value in SaveManager.ModeSaveData.SoulShopTable.Values)
			{
				if (!value.SoulShopData.Disabled && value.CurrentOwnedLevel > 0)
				{
					int currentOwnedLevel = value.CurrentOwnedLevel;
					int maxLevelScalingCap = value.SoulShopData.MaxLevelScalingCap;
					int scalingCost = value.SoulShopData.ScalingCost;
					int maxSoulCostCap = value.SoulShopData.MaxSoulCostCap;
					int num2 = Mathf.Max(currentOwnedLevel - maxLevelScalingCap, 0);
					if (num2 <= 0)
					{
						num += Mathf.RoundToInt((float)(value.InitialCost * value.CurrentOwnedLevel) + (float)(value.CurrentOwnedLevel * (value.CurrentOwnedLevel - 1)) / 2f * (float)scalingCost);
						continue;
					}
					num += Mathf.RoundToInt((float)(value.InitialCost * maxLevelScalingCap) + (float)(maxLevelScalingCap * (maxLevelScalingCap - 1)) / 2f * (float)scalingCost);
					num += num2 * maxSoulCostCap;
				}
			}
		}
		return totalBossSoulsCollected + totalChallengeSoulsCollected + totalNPCDialogueSoulsCollected + totalBlackChestSoulsCollected + miscSoulCollected - num;
	}
}
