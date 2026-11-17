using System.Collections.Generic;
using UnityEngine;

public class StatusEffect_EV
{
	public const int MAX_PROJECTILE_EFFECTS = 5;

	public const float ENEMY_BURN_DAMAGE_TIC_RATE = 0.5f;

	public const float ENEMY_BURN_DAMAGE_SCALE_BASE = 0.55f;

	public const float ENEMY_BURN_DURATION_OVERRIDE = 3.05f;

	public const float ENEMY_BURN_CRIT_DURATION_MOD = 0.35f;

	public const float BURN_IMMUNITY_DURATION_OVERRIDE = 0f;

	public const float ENEMY_BLEED_DAMAGE_TIC_RATE = 0.5f;

	public const float ENEMY_BLEED_DAMAGE_SCALE_BASE = 0.25f;

	public const float ENEMY_BLEED_DURATION_OVERRIDE = 1.55f;

	public const float ENEMY_BANE_DAMAGE_TIC_RATE = 1.5f;

	public const float ENEMY_BANE_DAMAGE_SCALE_BASE = 1f;

	public const float ENEMY_BANE_DURATION_OVERRIDE = 1.55f;

	public const float ENEMY_POISON_DAMAGE_TIC_RATE = 0.5f;

	public const float ENEMY_POISON_DAMAGE_SCALE_BASE = 0.05f;

	public const float ENEMY_POISON_DURATION_OVERRIDE = 4f;

	public const float ENEMY_POISON_SKILL_CRIT_TRIGGER_DURATION = 6f;

	public const int ENEMY_POISON_MAX_STACKS = 10;

	public const float FREEZE_DURATION_OVERRIDE = 3f;

	public const bool BREAK_FREEZE_ON_HIT = true;

	public const float KNOCKBACK_MOD_ON_FREEZE = 0f;

	public const float FREEZE_UNBREAKABLE_DURATION = 0.25f;

	public const float FREEZE_BOSS_DURATION_MOD = 0.15f;

	public const float FREEZE_IMMUNITY_AUTO_APPLY_DURATION = 0f;

	public const float FREEZE_IMMUNITY_DURATION_OVERRIDE = 0f;

	public const float GOD_MODE_DMG_DEALT_MOD = 5f;

	public const float GOD_MODE_SPEED_MOD = 2f;

	public const float GOD_MODE_DMG_TAKEN_MOD = 0f;

	public const float GOD_MOD_DURATION_OVERRIDE = 1f;

	public const float MANA_BURN_DEFAULT_DURATION = 2.55f;

	public const float MANA_BURN_DEFAULT_TIC_RATE = 0.1f;

	public const float MANA_BURN_DEFAULT_MANA_GAIN = 1f;

	public const float MANA_BURN_DEATH_MANA_GAIN = 20f;

	public const float SPORE_BURST_DEFAULT_DURATION = 1.25f;

	public const float ARMOR_BREAK_DEFAULT_DURATION = 2.5f;

	public const float ARMOR_BREAK_DAMAGE_INCREASE = 1.2f;

	public const float MAGIC_BREAK_DEFAULT_DURATION = 2.5f;

	public const float MAGIC_BREAK_DAMAGE_INCREASE = 1.2f;

	public const float SUAVE_DEFAULT_DURATION = 9999999f;

	public const float SUAVE_INTELLIGENCE_DAMAGE_MOD = 0.15f;

	public const float PHASED_DEFAULT_DURATION = 3f;

	public const float DIZZY_DEFAULT_DURATION = 3f;

	public const float DEATH_DELAY_DEFAULT_DURATION = 3f;

	public const float COMBO_DELAY_DEFAULT_DURATION = 2f;

	public const int COMBO_MAX_STACKS = 30;

	public const float COMBO_STACK_DAMAGE_MOD = 0.02f;

	public const int COMBO_STACK_GUARANTEED_CRIT_COUNT = 15;

	public const bool APPLY_PLAYER_COMBO_INSTEAD_OF_ENEMY_COMBO = true;

	public const int DANCE_MAX_STACKS = 5;

	public const float DANCE_STACK_DAMAGE_MOD = 0.15f;

	public const float KNOCKOUT_DEFAULT_DURATION = 0.6f;

	public const float KNOCKOUT_PUSH_MAGNITUDE = 33f;

	public const float KNOCKOUT_LEVEL_MAGIC_MOD = 0.3f;

	public const float KNOCKOUT_DISABLE_ONEWAY_COLLISION_DURATION = 0.25f;

	public const bool KNOCKOUT_TIMEOUT_EXPLOSION = true;

	public const string KNOCKOUT_PROJECTILE_NAME = "KnockoutStatusEffectProjectile";

	public const float VULNERABLE_DEFAULT_DURATION = 2f;

	public const bool VULNERABLE_CONSUME_ON_HIT = false;

	public const float FREE_CRIT_DEFAULT_DURATION = 2f;

	public const float NO_CONTACT_DAMAGE_DEFAULT_DURATION = 9999999f;

	public const float ENEMY_APPLIESINVULN_RADIUS = 25f;

	public const float ENEMY_APPLIESINVULN_DEFAULT_DURATION = float.MaxValue;

	public const string ENEMY_APPLIESINVULN_RADIUS_PROJECTILE_NAME = "StatusEffectTeamInvincibilityProjectile";

	public const float ENEMY_INVULN_DEFAULT_DURATION = 0.5f;

	public const float ENEMY_CURSE_PROJECTILE_DEFAULT_DURATION = float.MaxValue;

	public const int ENEMY_CURSE_PROJECTILE_NUM_PROJECTILES = 2;

	public const float ENEMY_CURSE_PROJECTILE_COOLDOWN = 2f;

	public const float ENEMY_CURSE_PROJECTILE_INTERVAL = 3.5f;

	public const float ENEMY_CURSE_PROJECTILE_DETECTION_DISTANCE = 35f;

	public const string ENEMY_CURSE_PROJECTILE_NAME = "StatusEffectCurseProjectile";

	public const string ENEMY_EXPLODE_PROJECTILE_NAME = "StatusEffectExplosionProjectile";

	public const string ENEMY_EXPLODE_WARNING_PROJECTILE_NAME = "StatusEffectExplosionWarningProjectile";

	public const float ENEMY_EXPLODE_RADIUS = 11f;

	public const float ENEMY_EXPLODE_DEFAULT_DURATION = float.MaxValue;

	public const float ENEMY_EXPLODE_TIME_INTERVAL = 1.25f;

	public const float ENEMY_EXPLODE_WARNING_DURATION = 1.25f;

	public const float ENEMY_EXPLODE_COOLDOWN = 4.25f;

	public const int ENEMY_FREEHIT_COUNT = 4;

	public const float ENEMY_FREEHIT_RECHARGE_TIMER = 3.5f;

	public const float ENEMY_SIZE_MOD = 1.25f;

	public const float ENEMY_SIZE_HP_MOD = 2f;

	public const float ENEMY_SIZE_DAMAGE_MOD = 1.5f;

	public const float ENEMY_SIZE_DEFAULT_DURATION = float.MaxValue;

	public const float ENEMY_ARMORSHRED_MAX_ARMOR_MOD = 0.1f;

	public const float ENEMY_ARMORSHRED_DEFAULT_DURATION = float.MaxValue;

	public const float PLAYER_EXHAUST_MAX_HP_PER_STACK = 0.01f;

	public const float PLAYER_EXHAUST_DEFAULT_DURATION = 2.5f;

	public const float ENEMY_INVULNWINDOW_INVULN_DURATION = 5f;

	public const float ENEMY_INVULNWINDOW_HEALTH_TRIGGER_AMOUNT = 0.01f;

	public const float ENEMY_INVULNWINDOW_ATTACKS_PER_SECOND = 1.25f;

	public const float ENEMY_INVULNWINDOW_ATTACK_INITIAL_DELAY = 1.15f;

	public static Vector2 ENEMY_INVULNWINDOW_PROJECTILE_OFFSET = new Vector2(-35f, 35f);

	public const string ENEMY_INVULNWINDOW_PROJECTILE_NAME = "StatusEffectInvulnWindowProjectile";

	public const int ENEMY_AGGRO_MAX_STACKS = 3;

	public const float ENEMY_AGGRO_DAMAGE_DEALT_MOD = 0.08f;

	public const float ENEMY_AGGRO_DAMAGE_TAKEN_MOD = 0.15f;

	public const float ENEMY_AGGRO_MOVE_SPEED_MOD = 0.12f;

	public const float ENEMY_AGGRO_STACK_DROP_DELAY = 2.75f;

	public const float ENEMY_FLAMER_DELAY_BETWEEN_WAVES = 2.25f;

	public const int ENEMY_FLAMER_PROJECTILES_PER_WAVE = 2;

	public const float ENEMY_FLAMER_DELAY_BETWEEN_PROJECTILES = 0.25f;

	public static Vector2 ENEMY_FLAME_PROJECTILE_OFFSET = new Vector2(-5f, 5f);

	public const string ENEMY_FLAMER_PROJECTILE_NAME = "StatusEffectFireballProjectile";

	public const float ENEMY_INVULNTIMER_DEFAULT_DURATION = 5f;

	public const float ENEMY_SPEED_MOD = 1.25f;

	public const float ENEMY_SPEED_DEFAULT_DURATION = float.MaxValue;

	public const float ENEMY_DISARM_DEFAULT_DURATION = float.MaxValue;

	public const float PLAYER_DISARMED_DEFAULT_DURATION = 3f;

	public static StatusEffectType[] COMMANDER_STATUS_EFFECT_ARRAY = new StatusEffectType[9]
	{
		StatusEffectType.Enemy_Size,
		StatusEffectType.Enemy_AppliesInvuln,
		StatusEffectType.Enemy_FreeHit,
		StatusEffectType.Enemy_Curse_Projectile,
		StatusEffectType.Enemy_Explode,
		StatusEffectType.Enemy_ArmorShred,
		StatusEffectType.Enemy_InvulnWindow,
		StatusEffectType.Enemy_Aggro,
		StatusEffectType.Enemy_Flamer
	};

	public const string COMMANDER_LOC_ID = "LOC_ID_INDEX_COMMANDER_BUFFS_COMMANDER_TITLE_1";

	public static Dictionary<StatusEffectType, string> STATUS_EFFECT_TITLE_LOC_IDS = new Dictionary<StatusEffectType, string>
	{
		{
			StatusEffectType.Player_Suave,
			"LOC_ID_INDEX_STATUS_EFFECTS_SUAVE_TITLE_1"
		},
		{
			StatusEffectType.Player_NoContactDamage,
			"LOC_ID_INDEX_STATUS_EFFECTS_NO_CONTACT_DAMAGE_TITLE_1"
		},
		{
			StatusEffectType.Player_Cloak,
			"LOC_ID_INDEX_STATUS_EFFECTS_CLOAK_TITLE_1"
		},
		{
			StatusEffectType.Player_Combo,
			"LOC_ID_INDEX_STATUS_EFFECTS_COMBO_TITLE_1"
		},
		{
			StatusEffectType.Enemy_Knockout,
			"LOC_ID_INDEX_STATUS_EFFECTS_KNOCK_OUT_TITLE_1"
		},
		{
			StatusEffectType.Player_Dance,
			"LOC_ID_INDEX_STATUS_EFFECTS_DANCE_TITLE_1"
		},
		{
			StatusEffectType.Player_Exhaust,
			"LOC_ID_INDEX_STATUS_EFFECTS_EXHAUST_TITLE_1"
		},
		{
			StatusEffectType.Player_FreeCrit,
			"LOC_ID_INDEX_STATUS_EFFECTS_FREE_CRIT_TITLE_1"
		},
		{
			StatusEffectType.Player_Disarmed,
			"LOC_ID_INDEX_STATUS_EFFECTS_DISARM_TITLE_1"
		},
		{
			StatusEffectType.Enemy_ArmorBreak,
			"LOC_ID_INDEX_STATUS_EFFECTS_ARMOR_BREAK_TITLE_1"
		},
		{
			StatusEffectType.Enemy_MagicBreak,
			"LOC_ID_INDEX_STATUS_EFFECTS_MAGIC_BREAK_TITLE_1"
		},
		{
			StatusEffectType.Enemy_Freeze,
			"LOC_ID_INDEX_STATUS_EFFECTS_FROZEN_TITLE_1"
		},
		{
			StatusEffectType.Enemy_Burn,
			"LOC_ID_INDEX_STATUS_EFFECTS_BURN_TITLE_1"
		},
		{
			StatusEffectType.Enemy_SporeBurst,
			"LOC_ID_INDEX_STATUS_EFFECTS_SPORE_BURST_TITLE_1"
		},
		{
			StatusEffectType.Enemy_ManaBurn,
			"LOC_ID_INDEX_STATUS_EFFECTS_MANA_LEECH_TITLE_1"
		},
		{
			StatusEffectType.Enemy_Poison,
			"LOC_ID_INDEX_STATUS_EFFECTS_POISON_TITLE_1"
		},
		{
			StatusEffectType.Enemy_Vulnerable,
			"LOC_ID_INDEX_STATUS_EFFECTS_VULNERABLE_TITLE_1"
		},
		{
			StatusEffectType.Enemy_Size,
			"LOC_ID_INDEX_COMMANDER_BUFFS_SIZE_TITLE_1"
		},
		{
			StatusEffectType.Enemy_Speed,
			"LOC_ID_INDEX_COMMANDER_BUFFS_SPEED_TITLE_1"
		},
		{
			StatusEffectType.Enemy_AppliesInvuln,
			"LOC_ID_INDEX_COMMANDER_BUFFS_APPLIES_INVULN_TITLE_1"
		},
		{
			StatusEffectType.Enemy_FreeHit,
			"LOC_ID_INDEX_COMMANDER_BUFFS_FREE_HIT_TITLE_1"
		},
		{
			StatusEffectType.Enemy_Disarm,
			"LOC_ID_INDEX_COMMANDER_BUFFS_DISARM_TITLE_1"
		},
		{
			StatusEffectType.Enemy_Curse_Projectile,
			"LOC_ID_INDEX_COMMANDER_BUFFS_CURSED_TITLE_1"
		},
		{
			StatusEffectType.Enemy_Explode,
			"LOC_ID_INDEX_COMMANDER_BUFFS_EXPLOSIVE_TITLE_1"
		},
		{
			StatusEffectType.Enemy_ArmorShred,
			"LOC_ID_INDEX_COMMANDER_BUFFS_ARMOR_SHRED_TITLE_1"
		},
		{
			StatusEffectType.Enemy_InvulnWindow,
			"LOC_ID_INDEX_COMMANDER_BUFFS_INVULNWINDOW_TITLE_1"
		},
		{
			StatusEffectType.Enemy_Aggro,
			"LOC_ID_INDEX_COMMANDER_BUFFS_AGGRO_TITLE_1"
		},
		{
			StatusEffectType.Enemy_Flamer,
			"LOC_ID_INDEX_COMMANDER_BUFFS_FLAMER_TITLE_1"
		}
	};

	public static Dictionary<StatusEffectType, string> STATUS_EFFECT_DESC_LOC_IDS = new Dictionary<StatusEffectType, string>
	{
		{
			StatusEffectType.Player_Suave,
			"LOC_ID_INDEX_STATUS_EFFECTS_SUAVE_DESCRIPTION_1"
		},
		{
			StatusEffectType.Player_NoContactDamage,
			"LOC_ID_INDEX_STATUS_EFFECTS_NO_CONTACT_DAMAGE_DESCRIPTION_1"
		},
		{
			StatusEffectType.Player_Cloak,
			"LOC_ID_INDEX_STATUS_EFFECTS_CLOAK_DESCRIPTION_1"
		},
		{
			StatusEffectType.Player_Combo,
			"LOC_ID_INDEX_STATUS_EFFECTS_COMBO_DESCRIPTION_1"
		},
		{
			StatusEffectType.Enemy_Knockout,
			"LOC_ID_INDEX_STATUS_EFFECTS_KNOCK_OUT_DESCRIPTION_1"
		},
		{
			StatusEffectType.Player_Dance,
			"LOC_ID_INDEX_STATUS_EFFECTS_DANCE_DESCRIPTION_1"
		},
		{
			StatusEffectType.Player_Exhaust,
			"LOC_ID_INDEX_STATUS_EFFECTS_EXHAUST_DESCRIPTION_1"
		},
		{
			StatusEffectType.Player_FreeCrit,
			"LOC_ID_INDEX_STATUS_EFFECTS_FREE_CRIT_DESCRIPTION_1"
		},
		{
			StatusEffectType.Player_Disarmed,
			"LOC_ID_INDEX_STATUS_EFFECTS_DISARM_DESCRIPTION_1"
		},
		{
			StatusEffectType.Enemy_ArmorBreak,
			"LOC_ID_INDEX_STATUS_EFFECTS_ARMOR_BREAK_DESCRIPTION_1"
		},
		{
			StatusEffectType.Enemy_MagicBreak,
			"LOC_ID_INDEX_STATUS_EFFECTS_MAGIC_BREAK_DESCRIPTION_1"
		},
		{
			StatusEffectType.Enemy_Freeze,
			"LOC_ID_INDEX_STATUS_EFFECTS_FROZEN_DESCRIPTION_1"
		},
		{
			StatusEffectType.Enemy_Burn,
			"LOC_ID_INDEX_STATUS_EFFECTS_BURN_DESCRIPTION_1"
		},
		{
			StatusEffectType.Enemy_SporeBurst,
			"LOC_ID_INDEX_STATUS_EFFECTS_SPORE_BURST_DESCRIPTION_1"
		},
		{
			StatusEffectType.Enemy_ManaBurn,
			"LOC_ID_INDEX_STATUS_EFFECTS_MANA_LEECH_DESCRIPTION_1"
		},
		{
			StatusEffectType.Enemy_Poison,
			"LOC_ID_INDEX_STATUS_EFFECTS_POISON_DESCRIPTION_1"
		},
		{
			StatusEffectType.Enemy_Vulnerable,
			"LOC_ID_INDEX_STATUS_EFFECTS_VULNERABLE_DESCRIPTION_1"
		},
		{
			StatusEffectType.Enemy_Size,
			"LOC_ID_INDEX_COMMANDER_BUFFS_SIZE_DESCRIPTION_1"
		},
		{
			StatusEffectType.Enemy_Speed,
			"LOC_ID_INDEX_COMMANDER_BUFFS_SPEED_DESCRIPTION_1"
		},
		{
			StatusEffectType.Enemy_AppliesInvuln,
			"LOC_ID_INDEX_COMMANDER_BUFFS_APPLIES_INVULN_DESCRIPTION_1"
		},
		{
			StatusEffectType.Enemy_FreeHit,
			"LOC_ID_INDEX_COMMANDER_BUFFS_FREE_HIT_DESCRIPTION_1"
		},
		{
			StatusEffectType.Enemy_Disarm,
			"LOC_ID_INDEX_COMMANDER_BUFFS_DISARM_DESCRIPTION_1"
		},
		{
			StatusEffectType.Enemy_Curse_Projectile,
			"LOC_ID_INDEX_COMMANDER_BUFFS_CURSED_DESCRIPTION_1"
		},
		{
			StatusEffectType.Enemy_Explode,
			"LOC_ID_INDEX_COMMANDER_BUFFS_EXPLOSIVE_DESCRIPTION_1"
		},
		{
			StatusEffectType.Enemy_ArmorShred,
			"LOC_ID_INDEX_COMMANDER_BUFFS_ARMOR_SHRED_DESCRIPTION_1"
		},
		{
			StatusEffectType.Enemy_InvulnWindow,
			"LOC_ID_INDEX_COMMANDER_BUFFS_INVULNWINDOW_DESCRIPTION_1"
		},
		{
			StatusEffectType.Enemy_Aggro,
			"LOC_ID_INDEX_COMMANDER_BUFFS_AGGRO_DESCRIPTION_1"
		},
		{
			StatusEffectType.Enemy_Flamer,
			"LOC_ID_INDEX_COMMANDER_BUFFS_FLAMER_DESCRIPTION_1"
		}
	};
}
