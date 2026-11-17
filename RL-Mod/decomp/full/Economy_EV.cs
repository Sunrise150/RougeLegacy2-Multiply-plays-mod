using System;
using System.Collections.Generic;
using UnityEngine;

public class Economy_EV
{
	public const int FIRST_DEATH_GRACE_GOLD = 200;

	public const float BRIDGE_BIOME_GOLD_GAIN_MOD_ADD = 0.2f;

	public const float ITEM_DROP_HP_GAIN_PER_MAGIC_DAMAGE = 2f;

	public const float ITEM_DROP_MANA_GAIN_PERCENT = 0.5f;

	public const float ITEM_DROP_HP_GAIN_PERCENT = 0f;

	public const float PIZZA_DROP_HP_GAIN_MOD = 0.2f;

	public static Vector2 ITEM_DROP_ACCELERATION = new Vector2(50f, 50f);

	public static Vector2 ITEM_DROP_MAX_MAGNET_SPEED = new Vector2(20f, 20f);

	public const float ITEM_DROP_TIMEOUT_BLINK_DURATION = 1.5f;

	public static Vector2 ITEM_DROP_REGULAR_SPURT_MINMAX_X = new Vector2(0.5f, 5f);

	public static Vector2 ITEM_DROP_REGULAR_SPURT_MINMAX_Y = new Vector2(10f, 15f);

	public static Vector2 ITEM_DROP_LARGE_SPURT_MINMAX_X = new Vector2(0.5f, 15f);

	public static Vector2 ITEM_DROP_LARGE_SPURT_MINMAX_Y = new Vector2(20f, 30f);

	public const float FAIRY_CHEST_HIDDEN_OPACITY = 0.125f;

	public const float FAIRY_CHEST_HIDDEN_PARTICLES_OPACITY = 0.0875f;

	public const int EQUIPMENT_ORE_FLAT = 175;

	public const float EQUIPMENT_ORE_MOD = 4.5f;

	public const int RUNE_ORE_FLAT = 150;

	public const float RUNE_ORE_MOD = 2f;

	public const float SILVER_CHEST_ORE_MOD = 0.5f;

	public const float GOLD_CHEST_ORE_MOD = 1.5f;

	public const float GOLD_CHEST_RUNE_ORE_MOD = 0.5f;

	public const int STARTING_EQUIPMENT_ORE = 1500;

	public const int STARTING_RUNE_ORE = 1500;

	public static int EXPERT_ENEMY_BASE_ORE_DROP = 20;

	public static float EXPERT_ENEMY_ORE_SCALING_PER_ENEMY_LEVEL = 1.25f;

	public static int EXPERT_ENEMY_BASE_BLOODSTONE_DROP = 15;

	public const bool ENABLE_SIMPLE_EQUIPMENT_COST_SCALING = true;

	public static int[] EQUIPMENT_LEVEL_GOLD_MOD = new int[10] { 1, 3, 6, 10, 15, 21, 28, 36, 45, 55 };

	public static int[] EQUIPMENT_LEVEL_ORE_MOD = new int[10] { 1, 3, 6, 10, 15, 21, 28, 36, 45, 55 };

	public const float EQUIPMENT_MIN_LEVEL_SCALING_PER_EQUIPMENT_FOUND = 0.75f;

	public const bool ENABLE_SIMPLE_RUNE_COST_SCALING = true;

	public static int[] RUNE_LEVEL_GOLD_MOD = new int[10] { 1, 4, 10, 18, 28, 40, 55, 75, 100, 125 };

	public static int[] RUNE_LEVEL_ORE_MOD = new int[10] { 1, 4, 10, 18, 28, 40, 55, 75, 100, 125 };

	public const float RUNE_MIN_LEVEL_SCALING_PER_RUNE_FOUND = 1.25f;

	public const float NG_BOSSCHEST_DROP_MOD = 0.75f;

	public const float NG_BOSSCHEST_VARIANT_MOD_ADD = 1.25f;

	public const int REDUCE_GOLD_PER_NG = 1;

	public static Vector2Int ENEMY_BASE_GOLD_DROP_AMOUNT = new Vector2Int(1, 2);

	public static Vector2 ENEMY_GOLD_DROP_PER_LEVEL_ADD = new Vector2(0.22f, 0.28f);

	public static float[] ENEMY_TYPE_GOLD_MOD = new float[5] { 1f, 1.3f, 2f, 6f, 12f };

	public static Dictionary<ChestType, float> CHEST_TYPE_SPAWN_ODDS = new Dictionary<ChestType, float>
	{
		{
			ChestType.Bronze,
			0.92f
		},
		{
			ChestType.Silver,
			0.08f
		},
		{
			ChestType.Gold,
			0f
		},
		{
			ChestType.Fairy,
			0f
		}
	};

	public static Vector2 CHEST_GOLD_DROP_PER_LEVEL_ADD = new Vector2(0.23f, 0.29f);

	public static Dictionary<ChestType, Vector2Int> BASE_GOLD_DROP_AMOUNT = new Dictionary<ChestType, Vector2Int>
	{
		{
			ChestType.Bronze,
			new Vector2Int(2, 3)
		},
		{
			ChestType.Silver,
			new Vector2Int(2, 3)
		},
		{
			ChestType.Gold,
			new Vector2Int(2, 3)
		},
		{
			ChestType.Boss,
			new Vector2Int(2, 3)
		},
		{
			ChestType.Fairy,
			new Vector2Int(0, 0)
		},
		{
			ChestType.Black,
			new Vector2Int(0, 0)
		}
	};

	public static Dictionary<ChestType, float> CHEST_TYPE_GOLD_MOD = new Dictionary<ChestType, float>
	{
		{
			ChestType.Bronze,
			2.5f
		},
		{
			ChestType.Silver,
			5f
		},
		{
			ChestType.Gold,
			8f
		},
		{
			ChestType.Boss,
			15f
		},
		{
			ChestType.Fairy,
			0f
		},
		{
			ChestType.Black,
			0f
		}
	};

	public static Dictionary<BossID, BossDrop> BOSS_DROP_TABLE = new Dictionary<BossID, BossDrop>
	{
		{
			BossID.Castle_Boss,
			new BossDrop(1000, 0, 500, 500)
		},
		{
			BossID.Bridge_Boss,
			new BossDrop(1500, 0, 750, 750)
		},
		{
			BossID.Forest_Boss,
			new BossDrop(2000, 0, 1000, 1000)
		},
		{
			BossID.Study_Boss,
			new BossDrop(2500, 0, 1250, 1250)
		},
		{
			BossID.Tower_Boss,
			new BossDrop(3000, 0, 1500, 1500)
		},
		{
			BossID.Cave_Boss,
			new BossDrop(3500, 0, 1750, 1750)
		},
		{
			BossID.Garden_Boss,
			new BossDrop(4000, 0, 2000, 2000)
		},
		{
			BossID.Final_Boss,
			new BossDrop(4500, 0, 2250, 2250)
		}
	};

	public const float ENEMY_ITEM_DROP_CHANCE = 0f;

	public const float PIZZA_ITEM_DROP_CHANCE = 0.1f;

	public static Vector2[] BREAKABLE_ITEM_DROP_TYPE_ODDS = new Vector2[5]
	{
		new Vector2(0f, 0.56f),
		new Vector2(40f, 0.37f),
		new Vector2(10f, 0.01f),
		new Vector2(60f, 0.03f),
		new Vector2(50f, 0.03f)
	};

	public static Vector2[] BRONZE_CHEST_ITEM_TYPE_ODDS = new Vector2[5]
	{
		new Vector2(10f, 0.85f),
		new Vector2(60f, 0f),
		new Vector2(40f, 0f),
		new Vector2(30f, 0.15f),
		new Vector2(20f, 0f)
	};

	public static Vector2[] SILVER_CHEST_ITEM_TYPE_ODDS = new Vector2[5]
	{
		new Vector2(10f, 0.01f),
		new Vector2(60f, 0f),
		new Vector2(40f, 0f),
		new Vector2(30f, 0.99f),
		new Vector2(20f, 0f)
	};

	public static Vector2[] GOLD_CHEST_ITEM_TYPE_ODDS = new Vector2[6]
	{
		new Vector2(10f, 1f),
		new Vector2(60f, 0f),
		new Vector2(40f, 0f),
		new Vector2(30f, 0f),
		new Vector2(20f, 0f),
		new Vector2(110f, 0f)
	};

	public static Vector2[] FAIRY_CHEST_ITEM_TYPE_ODDS = new Vector2[5]
	{
		new Vector2(10f, 0f),
		new Vector2(60f, 0f),
		new Vector2(40f, 1f),
		new Vector2(30f, 0f),
		new Vector2(110f, 0f)
	};

	public static Vector2[] BOSS_CHEST_ITEM_TYPE_ODDS = new Vector2[5]
	{
		new Vector2(10f, 1f),
		new Vector2(60f, 0f),
		new Vector2(40f, 0f),
		new Vector2(30f, 0f),
		new Vector2(20f, 0f)
	};

	private static Dictionary<ChestType, Vector2Int> m_rollRanges = null;

	public static int GetGoldilocksLimit()
	{
		return 100 + SkillTreeManager.GetTotalSkillObjLevel() * 3;
	}

	public static Vector2[] GetChestItemTypeOdds(ChestType chestType)
	{
		return chestType switch
		{
			ChestType.Bronze => BRONZE_CHEST_ITEM_TYPE_ODDS, 
			ChestType.Silver => SILVER_CHEST_ITEM_TYPE_ODDS, 
			ChestType.Gold => GOLD_CHEST_ITEM_TYPE_ODDS, 
			ChestType.Boss => BOSS_CHEST_ITEM_TYPE_ODDS, 
			ChestType.Fairy => FAIRY_CHEST_ITEM_TYPE_ODDS, 
			_ => new Vector2[0], 
		};
	}

	public static Dictionary<ChestType, Vector2Int> GetChestTypeRollRanges()
	{
		if (m_rollRanges == null)
		{
			m_rollRanges = new Dictionary<ChestType, Vector2Int>();
			float num = 0f;
			foreach (KeyValuePair<ChestType, float> cHEST_TYPE_SPAWN_ODD in CHEST_TYPE_SPAWN_ODDS)
			{
				num += cHEST_TYPE_SPAWN_ODD.Value;
			}
			if (num != 1f)
			{
				throw new ArgumentOutOfRangeException("SPAWN_ODDS", "Odds must add up to exactly 1.0");
			}
			int num2 = 0;
			foreach (KeyValuePair<ChestType, float> cHEST_TYPE_SPAWN_ODD2 in CHEST_TYPE_SPAWN_ODDS)
			{
				if (cHEST_TYPE_SPAWN_ODD2.Value != 0f)
				{
					int num3 = num2 + Mathf.RoundToInt(100f * cHEST_TYPE_SPAWN_ODD2.Value) - 1;
					m_rollRanges.Add(cHEST_TYPE_SPAWN_ODD2.Key, new Vector2Int(num2, num3));
					num2 = num3 + 1;
				}
				else
				{
					m_rollRanges.Add(cHEST_TYPE_SPAWN_ODD2.Key, new Vector2Int(1000, 1000));
				}
			}
		}
		return m_rollRanges;
	}

	public static int GetItemDropValue(ItemDropType itemDrop, bool getGoldValueOnly = false)
	{
		switch (itemDrop)
		{
		case ItemDropType.Coin:
			return 10;
		case ItemDropType.MoneyBag:
			return 100;
		case ItemDropType.Crystal:
			return 1000;
		case ItemDropType.Diamond:
			return 10000;
		case ItemDropType.Soul:
			if (getGoldValueOnly)
			{
				return 0;
			}
			return 100;
		case ItemDropType.EquipmentOre:
		case ItemDropType.RuneOre:
			if (getGoldValueOnly)
			{
				return 0;
			}
			return 10;
		case ItemDropType.HealthDrop:
		case ItemDropType.PizzaDrop:
		case ItemDropType.MushroomDrop:
		case ItemDropType.CandyDrop:
		case ItemDropType.CookieDrop:
			if (getGoldValueOnly)
			{
				return 0;
			}
			return 1;
		default:
			return 0;
		}
	}

	public static ItemDropType GetBaseItemDropType(ItemDropType itemDrop)
	{
		switch (itemDrop)
		{
		case ItemDropType.MoneyBag:
		case ItemDropType.Diamond:
		case ItemDropType.Crystal:
		case ItemDropType.Coin:
			return ItemDropType.Coin;
		default:
			return itemDrop;
		}
	}

	public static float GetGoldGainMod()
	{
		float num = 0f;
		num += SkillTreeLogicHelper.GetGoldGainMod();
		num += RuneLogicHelper.GetGoldGainPercent();
		num += EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.GoldGain);
		num += BridgeGoldGain_BiomeRule.GoldGainMod;
		num += (float)SaveManager.PlayerSaveData.GetRelic(RelicType.GoldCombatChallengeUsed).Level * 0.2f;
		int num2 = BurdenManager.BurdenRequiredForNG(SaveManager.PlayerSaveData.NewGamePlusLevel);
		if (num2 < 0)
		{
			SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType.BurdenOverload);
			if (!soulShopObj.IsNativeNull())
			{
				num += soulShopObj.CurrentStatGain * (float)Mathf.Abs(num2);
			}
		}
		return num;
	}

	public static float GetOreGainMod()
	{
		return 0f + SkillTreeLogicHelper.GetEquipmentOreMod() + RuneLogicHelper.GetOreGainPercent() + EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.OreAetherGain) + (float)SaveManager.PlayerSaveData.GetRelic(RelicType.GoldCombatChallengeUsed).Level * 0.2f + Mastery_EV.GetTotalMasteryBonus(MasteryBonusType.Ore_And_Aether_Up);
	}

	public static float GetRuneOreGainMod()
	{
		return 0f + SkillTreeLogicHelper.GetRuneOreMod() + RuneLogicHelper.GetRuneOreGainPercent() + EquipmentManager.Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType.OreAetherGain) + (float)SaveManager.PlayerSaveData.GetRelic(RelicType.GoldCombatChallengeUsed).Level * 0.2f + Mastery_EV.GetTotalMasteryBonus(MasteryBonusType.Ore_And_Aether_Up);
	}

	public static bool CanDropPizza()
	{
		return SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.PizzaGirlUnlocked);
	}
}
