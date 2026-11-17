using UnityEngine;

public class Relic_EV
{
	public const float RELIC_RARITY_ONE_ODDS = 1f;

	public const float RELIC_RARITY_TWO_ODDS = 0f;

	public const float RELIC_RARITY_THREE_ODDS = 0f;

	public const float RELIC_MOD_CHANCE = 0.2f;

	public const float EXTRALIFE_RESTORE_HP_MOD = 0.5f;

	public const float EXTRALIFE_RESTORE_HP_MAGIC_DAMAGE_FLAT = 0f;

	public const float EXTRALIFE_UNITY_RESTORE_HP_MOD = 0.5f;

	public const int TAKE_NO_DAMAGE_FREE_HITS = 3;

	public const int BONUS_DMG_ON_HIT_COUNT = 6;

	public const float BONUS_DMG_ON_HIT_DAMAGE_MOD = 1.75f;

	public const float GOD_MODE_DURATION = 25f;

	public const float EXTEND_INVULN_DURATION_FLAT_ADD = 1.25f;

	public const int LOW_MULTI_JUMP_EXTRA_JUMPS = 3;

	public const float FREE_ENEMY_KILL_CHANCE = 0.05f;

	public const int FREE_ENEMY_KILL_CHANCE_CHARGE = 6;

	public const float GOLD_DEATH_CURSE_GOLD_GAIN = 1.35f;

	public const float CHESTS_HEALTH_RESTORE_MAGIC_SCALE = 1f;

	public const float MEAT_MAX_HEALTH_UP_MOD = 0.1f;

	public const int MEAT_MAX_HEALTH_UP_STACK = 3;

	public const float INVULN_DAMAGE_BUFF_MOD = 2f;

	public const float RELIC_AMOUNT_DAMAGE_UP = 0.06f;

	public const float WEAPONS_BURN_ADD_DURATION = 2f;

	public const float WEAPONS_BURN_ADD_DAMAGE_REDUCTION = 0f;

	public const float WEAPONS_POISON_ADD_DURATION = 2f;

	public const float ENEMIES_DROP_MEAT_CHANCE = 0.08f;

	public const float DAMAGE_BUFF_STATUS_EFFECT = 0.2f;

	public const float ATTACK_COOLDOWN_DURATION = 2f;

	public const float ATTACK_COOLDOWN_DAMAGE_MOD = 2f;

	public const float ATTACK_COOLDOWN_GUN_PROJECTILE_LIFESPAN_ADD = 0f;

	public const float ATTACK_COOLDOWN_RELOAD_ADD = 0f;

	public const float ATTACK_COOLDOWN_PISTOL_EXIT_DELAY_ADD = 0.2f;

	public const float ATTACK_COOLDOWN_AXE_SPINNER_MOD = -0.3f;

	public const int KILLS_TO_LEVEL_LILIES = 21;

	public const float BONUS_DAMAGE_CURSE_DAMAGE_MOD = 1f;

	public const float SPINKICK_ARMOR_BREAK_DURATION_OVERRIDE = 3.5f;

	public const float MAGIC_DAMAGE_ENEMY_COUNT_INCREASE_PERCENT = 0.1f;

	public const float MAGIC_DAMAGE_ENEMY_COUNT_MAX_PERCENT = 0.5f;

	public const int MANA_RESTORE_ON_HURT_FLAT_ADD = 100;

	public const int BONUS_MANA_FLAT_ADD = 50;

	public const float BONUS_MANA_MAGIC_DAMAGE_MOD = 1.08f;

	public const float SPELL_SWAP_MAGIC_DAMAGE_MOD = 1.07f;

	public const float WEAPON_SWAP_WEAPON_DAMAGE_MOD = 1.07f;

	public const float TALENT_SWAP_WEAPON_DAMAGE_MOD = 1f;

	public const float TALENT_SWAP_MAGIC_DAMAGE_MOD = 1f;

	public const int RESOLVE_COMBAT_CHALLENGE_NUM_KILLS_REQUIRED = 10;

	public const bool RESOLVE_COMBAT_CHALLENGE_KILL_PLAYER_ON_HIT = true;

	public const float RESOLVE_COMBAT_CHALLENGE_DAMAGE_MOD = 10f;

	public const float NO_GOLD_XP_BONUS_CAP = 3f;

	public const float CRIT_KILLS_HEAL_AMOUNT = 0.06f;

	public const float CRIT_DMG_UP_AMOUNT = 0.2f;

	public const float CRIT_CHANCE_UP_AMOUNT = 0.1f;

	public const float MAX_MANA_DAMAGE_MOD = 0.2f;

	public const int FREE_HIT_REGENERATE_CHARGE = 6;

	public const float MAX_HEALTH_STAT_BONUS_MOD = 0.1f;

	public const float MAX_HEALTH_STAT_BONUS_MAX_HP_AMOUNT = 0.5f;

	public const float LOW_HEALTH_STAT_BONUS_MAX_HP_AMOUNT = 0.5f;

	public const float LOW_HEALTH_STAT_BONUS_MOD = 0.2f;

	public const float AIM_SLOW_TIME_AMOUNT = 0.425f;

	public const float REPLACEMENT_RELIC_MAX_HEALTH_MOD = 0.1f;

	public const float REPLACEMENT_RELIC_STRENGTH_MOD = 0.1f;

	public const float REPLACEMENT_RELIC_MAGIC_MOD = 0.1f;

	public const float DASH_STRIKE_DAMAGE_UP_MOD = 0.15f;

	public const float DASH_STRIKE_DISTANCE_ADD = 0.2f;

	public const float FATAL_BLOW_DODGE_CHANCE = 0.25f;

	public const int GOLD_COMBAT_CHALLENGE_NUM_KILLS_REQUIRED = 15;

	public const float GOLD_COMBAT_CHALLENGE_GOLD_MOD = 0.2f;

	public const float SUPER_CRIT_CHANCE_UP_MOD_AMOUNT = 0.2f;

	public const float FOOD_HEALS_MORE_MOD_AMOUNT = 0.08f;

	public const float SPELL_KILL_MAX_MANA_HP_REQUIREMENT = 0.5f;

	public const int SPELL_KILL_MAX_MANA_MP_CAP = 200;

	public const int SPELL_KILL_MAX_MANA_ADD_AMOUNT = 5;

	public const float ATTACK_EXHAUST_BONUS_DAMAGE_MOD = 1.5f;

	public const int ATTACK_EXHAUST_EXHAUST_ADD_AMOUNT = 25;

	public const float LOW_RESOLVE_WEAPON_DAMAGE_MOD = 1.01f;

	public const float LOW_RESOLVE_MAGIC_DAMAGE_MOD = 1.01f;

	public const float GROUND_DAMAGE_BONUS_DAMAGE_MOD = 0.125f;

	public const float FLIGHT_BONUS_CURSE_DAMAGE_MOD = 1f;

	public const float FLIGHT_BONUS_MOVE_SPEED_MOD = 0.1f;

	public const float RANGE_DAMAGE_BONUS_CURSE_FAR_DISTANCE = 9f;

	public const float RANGE_DAMAGE_BONUS_CURSE_FAR_DAMAGE_MOD = 0.125f;

	public const float RANGE_DAMAGE_BONUS_CURSE_NEAR_DAMAGE_MOD = 0f;

	public const int PROJECTILE_DASH_START_NUM_REQUIRED_DASHES = 0;

	public const float PROJECTILE_DASH_START_BASE_LIFESPAWN = 1f;

	public const float PROJECTILE_DASH_START_LIFESPAN_ADD = 1f;

	public const float DAMAGE_AURA_ON_HIT_LIFESPAN_BASE = 1.5f;

	public const float DAMAGE_AURA_ON_HIT_LIFESPAN_ADD = 1.5f;

	public const float LAND_SHOCKWAVE_DAMAGE_BASE = 0.75f;

	public const float LAND_SHOCKWAVE_DAMAGE_MOD_PER_STACK = 0.75f;

	public const int FOOD_CHALLENGE_NUM_ITEMS_REQUIRED = 2;

	public const int FOOD_CHALLENGE_BONUS_MANA_FLAT_ADD = 50;

	public const float FOOD_CHALLENGE_MAX_HEALTH_MOD = 0.3f;

	public const float DAMAGE_NO_HIT_CHALLENGE_BONUS_DAMAGE_MOD = 1.25f;

	public const float ON_HIT_AREA_BASE_DAMAGE = 1.5f;

	public const float ON_HIT_AREA_DAMAGE_ADD_PER_STACK = 0.75f;

	public const int ON_HIT_AREA_DAMAGE_RECHARGE_TIME = 5;

	public const int ON_HIT_AREA_DAMAGE_RECHARGE_TIME_REDUCTION_PER_STACK = 1;

	public const float MANA_DAMAGE_REDUCTION_MANA_AMOUNT = 150f;

	public const float MANA_DAMAGE_REDUCTION_DAMAGE_MOD = 1f;

	public const int MANA_DAMAGE_REDUCTION_STARTING_CHARGES = 2;

	public const int FREE_CAST_SPELL_REQUIRED_CASTS = 3;

	public const int FREE_CAST_SPELL_CAST_REDUCTION_PER_STACK = 1;

	public const int SPINKICK_LEAVES_CALTROPS_NUM_KICKS_REQUIRED = 4;

	public const int SPINKICK_LEAVES_CALTROPS_KICK_REDUCTION_PER_STACK = 1;

	public const int NO_ATTACK_DAMAGE_BONUS_RECHARGE_TIME = 5;

	public const int NO_ATTACK_DAMAGE_BONUS_TIME_REDUCTION_PER_STACK = 1;

	public const float NO_ATTACK_DAMAGE_BONUS_DAMAGE_MOD = 0.75f;

	public const float SPINKICK_DAMAGE_BONUS_MOD = 0.4f;

	public const float SKILL_CRIT_BONUS_BASE_DURATION = 2.5f;

	public const float SKILL_CRIT_BONUS_DURATION_ADD_PER_STACK = 2.5f;

	public const float MEAT_DROP_CHANCE_UP_MOD_ADD = 0.01f;

	public const float DAMAGE_REDUCTION_STATUS_EFFECT_AMOUNT = 0.15f;

	public const float WEAPON_CRIT_DAMAGE_ADD_AMOUNT = 0.1f;

	public const float MAGIC_CRIT_DAMAGE_ADD_AMOUNT = 0.1f;

	public static float GetRelicFormatString(RelicType relicType, int numRelics, out float altValue)
	{
		altValue = -1f;
		switch (relicType)
		{
		case RelicType.ExtraLife_Unity:
			altValue = Mathf.RoundToInt(50f);
			return numRelics;
		case RelicType.ExtraLife:
			altValue = Mathf.RoundToInt(50f);
			return numRelics;
		case RelicType.TakeNoDamage:
			return 3 * numRelics;
		case RelicType.ExtendInvuln:
			return 1.25f * (float)numRelics;
		case RelicType.BonusDamageOnNextHit:
			altValue = 6f;
			return Mathf.RoundToInt(75f) * numRelics;
		case RelicType.FreeEnemyKill:
			return 6 - (numRelics - 1);
		case RelicType.BonusDamageCurse:
			return Mathf.RoundToInt(100f) * numRelics;
		case RelicType.LowMultiJump:
			return 3 * numRelics;
		case RelicType.MeatMaxHealth:
			altValue = Mathf.RoundToInt(10f);
			return 3 * numRelics;
		case RelicType.ChestHealthRestore:
			return Mathf.RoundToInt(100f) * numRelics;
		case RelicType.InvulnDamageBuff:
			return Mathf.RoundToInt(100f) * numRelics;
		case RelicType.RelicAmountDamageUp:
			return Mathf.RoundToInt(6f) * numRelics;
		case RelicType.WeaponsBurnAdd:
			return 2f * (float)numRelics;
		case RelicType.WeaponsPoisonAdd:
			return numRelics;
		case RelicType.EnemiesDropMeat:
			return Mathf.RoundToInt(8f) * numRelics;
		case RelicType.DamageBuffStatusEffect:
			return Mathf.RoundToInt(20f) * numRelics;
		case RelicType.AttackCooldown:
			altValue = 2f * (float)numRelics;
			return Mathf.RoundToInt(100f) * numRelics;
		case RelicType.SpinKickArmorBreak:
			return 3.5f * (float)numRelics;
		case RelicType.MagicDamageEnemyCount:
			altValue = Mathf.RoundToInt(10f);
			return Mathf.RoundToInt(50f) * numRelics;
		case RelicType.ManaRestoreOnHurt:
			return 100 * numRelics;
		case RelicType.BonusMana:
			altValue = Mathf.RoundToInt(8.000004f) * numRelics;
			return 50 * numRelics;
		case RelicType.WeaponSwap:
			return Mathf.RoundToInt(7.0000052f) * numRelics;
		case RelicType.SpellSwap:
			return Mathf.RoundToInt(7.0000052f) * numRelics;
		case RelicType.TalentSwap:
			return Mathf.RoundToInt(0f) * numRelics;
		case RelicType.NoGoldXPBonus:
			altValue = 3f;
			return Mathf.RoundToInt(Mathf.Min((Economy_EV.GetGoldGainMod() + 1f) * NPC_EV.GetArchitectGoldMod() * 100f, 300f));
		case RelicType.ResolveCombatChallenge:
			altValue = Mathf.RoundToInt(Mathf.Abs(RelicLibrary.GetRelicData(RelicType.ResolveCombatChallengeUsed).CostAmount * 100f));
			return 10f;
		case RelicType.ResolveCombatChallengeUsed:
			return Mathf.RoundToInt(Mathf.Abs(RelicLibrary.GetRelicData(RelicType.ResolveCombatChallengeUsed).CostAmount * 100f));
		case RelicType.CritKillsHeal:
			return Mathf.RoundToInt(6f) * numRelics;
		case RelicType.AllCritChanceUp:
			return Mathf.RoundToInt(10f) * numRelics;
		case RelicType.AllCritDamageUp:
			return Mathf.RoundToInt(20f) * numRelics;
		case RelicType.FreeHitRegenerate:
			return 6 - (numRelics - 1);
		case RelicType.MaxManaDamage:
			return 20f * (float)numRelics;
		case RelicType.LowHealthStatBonus:
			altValue = Mathf.RoundToInt(50f);
			return Mathf.RoundToInt(20f) * numRelics;
		case RelicType.MaxHealthStatBonus:
			altValue = Mathf.RoundToInt(50f);
			return Mathf.RoundToInt(10f) * numRelics;
		case RelicType.DashStrikeDamageUp:
			return 10f * (0.2f + 0.2f * (float)(numRelics - 1));
		case RelicType.ReplacementRelic:
			return 10f * (float)numRelics;
		case RelicType.FatalBlowDodge:
			return 25f * (float)numRelics;
		case RelicType.ManaDamageReduction:
			altValue = 150f;
			return 2 * numRelics;
		case RelicType.GoldCombatChallenge:
			altValue = Mathf.RoundToInt(20f);
			return 15f;
		case RelicType.GoldCombatChallengeUsed:
			return Mathf.RoundToInt(20f);
		case RelicType.FoodHealsMore:
			return Mathf.RoundToInt(8f) * numRelics;
		case RelicType.SuperCritChanceUp:
			return Mathf.RoundToInt(20f) * numRelics;
		case RelicType.SpellKillMaxMana:
			altValue = 200 * numRelics;
			return 5f;
		case RelicType.AttackExhaust:
			altValue = 25f;
			return Mathf.RoundToInt(50f) * numRelics;
		case RelicType.LowResolveWeaponDamage:
			if (PlayerManager.IsInstantiated)
			{
				altValue = Mathf.RoundToInt((float)Mathf.Max(Mathf.RoundToInt((1f - PlayerManager.GetPlayerController().ActualResolve) * 100f), 0) * 0.00999999f * (float)numRelics * 100f);
			}
			return Mathf.RoundToInt(0.99999905f) * numRelics;
		case RelicType.LowResolveMagicDamage:
			if (PlayerManager.IsInstantiated)
			{
				altValue = Mathf.RoundToInt((float)Mathf.Max(Mathf.RoundToInt((1f - PlayerManager.GetPlayerController().ActualResolve) * 100f), 0) * 0.00999999f * (float)numRelics * 100f);
			}
			return Mathf.RoundToInt(0.99999905f) * numRelics;
		case RelicType.DamageNoHitChallenge:
			return Mathf.RoundToInt(125f) * numRelics;
		case RelicType.FoodChallenge:
			if (PlayerManager.IsInstantiated)
			{
				PlayerController playerController = PlayerManager.GetPlayerController();
				float num = playerController.ActualMaxHealth;
				SaveManager.PlayerSaveData.GetRelic(RelicType.FoodChallengeUsed).SetLevel(1, additive: true, broadcast: false);
				playerController.InitializeHealthMods();
				float num2 = (float)playerController.ActualMaxHealth - num;
				SaveManager.PlayerSaveData.GetRelic(RelicType.FoodChallengeUsed).SetLevel(-1, additive: true, broadcast: false);
				playerController.InitializeHealthMods();
				altValue = num2;
			}
			return 2f;
		case RelicType.FoodChallengeUsed:
			altValue = 50f;
			return SaveManager.PlayerSaveData.GetRelic(relicType).FloatValue;
		case RelicType.FlightBonusCurse:
			return Mathf.RoundToInt(100f);
		case RelicType.GroundDamageBonus:
			return Mathf.RoundToInt((float)numRelics * 0.125f * 100f);
		case RelicType.OnHitAreaDamage:
			altValue = Mathf.RoundToInt((1.5f + (float)(numRelics - 1) * 0.75f) * 100f);
			return GetRelicMaxStack(relicType, numRelics);
		case RelicType.NoAttackDamageBonus:
			altValue = 75f;
			return GetRelicMaxStack(relicType, numRelics);
		case RelicType.FreeCastSpell:
			return GetRelicMaxStack(relicType, numRelics);
		case RelicType.DamageAuraOnHit:
			return 1.5f + (float)(numRelics - 1) * 1.5f;
		case RelicType.SkillCritBonus:
			return 2.5f + (float)(numRelics - 1) * 2.5f;
		case RelicType.RangeDamageBonusCurse:
			altValue = Mathf.Abs(Mathf.RoundToInt(0f));
			return Mathf.RoundToInt((float)(100 * numRelics) * 0.125f);
		case RelicType.ProjectileDashStart:
			altValue = Mathf.RoundToInt(1f + (float)(numRelics - 1) * 1f);
			return 0f;
		case RelicType.SpinKickLeavesCaltrops:
			return GetRelicMaxStack(relicType, numRelics);
		case RelicType.LandShockwave:
			return (0.75f + (float)(numRelics - 1) * 0.75f) * 100f;
		case RelicType.SpinKickDamageBonus:
			return 0.4f * (float)numRelics * 100f;
		default:
			return 0f;
		}
	}

	public static int GetRelicMaxStack(RelicType relicType, int numRelics)
	{
		return relicType switch
		{
			RelicType.BonusDamageOnNextHit => 5, 
			RelicType.FreeEnemyKill => 6 - (numRelics - 1), 
			RelicType.MeatMaxHealth => 3 * numRelics, 
			RelicType.MagicDamageEnemyCount => Mathf.RoundToInt(5f) * numRelics, 
			RelicType.ResolveCombatChallenge => 10, 
			RelicType.FreeHitRegenerate => 6 - (numRelics - 1), 
			RelicType.ManaDamageReduction => 2 * numRelics, 
			RelicType.GoldCombatChallenge => 15, 
			RelicType.FoodChallenge => 2, 
			RelicType.OnHitAreaDamage => 5 - (numRelics - 1), 
			RelicType.NoAttackDamageBonus => 5 - (numRelics - 1), 
			RelicType.FreeCastSpell => 3 - (numRelics - 1), 
			RelicType.ProjectileDashStart => -1, 
			RelicType.SpinKickLeavesCaltrops => 4 - (numRelics - 1), 
			_ => -1, 
		};
	}
}
