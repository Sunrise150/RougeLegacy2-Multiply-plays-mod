using UnityEngine;

public class Ability_EV
{
	public const float ABILITY_CD_REDUC_DELAY = 0.5f;

	public const float CLOAK_BASE_DURATION = 3f;

	public const float CLOAK_SPEED_MOD = 0.5f;

	public const float CLOAK_DISABLE_INVINCIBILITY_DURATION = 0.1f;

	public static Vector2 CLOAKSTRIKE_INITIAL_VELOCITY = new Vector2(0f, 16.5f);

	public static Vector2 CLOAKSTRIKE_ATTACK_VELOCITY = new Vector2(21f, -31f);

	public static Vector2 CLOAKSTRIKE_RICOCHET_MOD = new Vector2(1f, 1.25f);

	public const float CLOAK_VULNERABLE_DURATON = 2f;

	public const float ROLL_SPEED = 22f;

	public const float ROLL_DURATION = 0.3f;

	public const float ROLL_EXIT_INVINCIBILITY_DURATION = 0.1f;

	public const float ROLL_DAMAGE_INCREASE_DURATION = 0f;

	public const float ROLL_DAMAGE_INCREASE_AMOUNT = 0f;

	public const float ROLL_FREE_CRIT_DURATION = 0.5f;

	public const PlayerStat LIFESTEAL_RUNE_SET_STAT = PlayerStat.Strength;

	public const PlayerStat SOULSTEAL_RUNE_SET_STAT = PlayerStat.Magic;

	public const PlayerStat RETURNDAMAGE_RUNE_SET_STAT = PlayerStat.Vitality;

	public const float LANCE_ATTACK_FALL_MOD = 0.025f;

	public static Vector2 LANCE_SWING_ATTACK_KNOCKBACK = new Vector2(0f, 8f);

	public const bool LANCE_SWING_ATTACK_APPLY_KNOCKBACK_ON_GROUND = false;

	public const float LANCE_DASH_ATTACK_ADD_CD = 0f;

	public const bool LANCE_CHANGE_DURING_CHARGE = true;

	public const float LANCE_DASH_1_MIN_CHARGE = 0f;

	public const float LANCE_DASH_1_DISTANCE = 0f;

	public const float LANCE_DASH_1_SPEED = 0f;

	public const int LANCE_DASH_1_STR_ADD = 0;

	public const float LANCE_DASH_2_MIN_CHARGE = 0.55f;

	public const float LANCE_DASH_2_DISTANCE = 30f;

	public const float LANCE_DASH_2_SPEED = 34f;

	public const float LANCE_DASH_VERTICAL_MOVEMENT = 18f;

	public const float SABER_DASH_DISTANCE = 0f;

	public const float SABER_DASH_SPEED = 0f;

	public const float SABER_DASH_GRAVITY_DELAY = float.NaN;

	public const bool BOW_GROUNDED_DISABLE_KNOCKBACK = true;

	public const bool SPEAR_SPIN_RESETS_CD = true;

	public const int SPEAR_SPIN_MANA_REGEN_AMOUNT = 15;

	public const float SPEAR_SPIN_SLOW_DURATION = 0.175f;

	public const float SPEAR_SPIN_SLOW_AMOUNT = 0.05f;

	public const float SPEAR_SPIN_GRACE = 0.05f;

	public const float SHIELD_BLOCK_INVINCIBILITY_DURATION = 1.25f;

	public const float SHIELD_BLOCK_SLOWDOWN_DURATION = 0.25f;

	public const float SHIELD_BLOCK_SLOWNDOWN_AMOUNT = 0.1f;

	public const float SHIELD_BLOCK_PROJECTILE_DELAY = 0.15f;

	public static Vector2 SHIELD_BLOCK_KNOCKBACK_ADD = new Vector2(11f, 11.15f);

	public const bool SHIELD_BLOCK_USE_PERCENTAGE_OVER_MAGIC_SCALE = true;

	public const float SHIELD_BLOCK_PERFECT_TIMING = 0.135f;

	public const float SHIELD_BLOCK_REGULAR_SCALING = 1.5f;

	public const float SHIELD_BLOCK_PERFECT_SCALING = 2.5f;

	public const float SHIELD_BLOCK_REGULAR_DAMAGE_REDUCTION = 0.5f;

	public const float SHIELD_BLOCK_PERFECT_DAMAGE_REDUCTION = 0f;

	public const float CREATE_PLATFORM_VERTICAL_PUSH = 10f;

	public const int POOL_BALL_NUM_BOUNCES = 3;

	public const float POOL_BALL_DAMAGE_MOD_PER_BOUNCE = 7f;

	public const float POOL_BALL_MAX_BOUNCES__APPLIED_TO_MULTIPLIER = 1f;

	public const float SUPERFART_PUSH_AMOUNT = 25f;

	public const float FLAME_BARRIER_TIC_RATE = 0.5f;

	public const float TIME_SLOW_AMOUNT = 0.2f;

	public const float TIME_SLOW_MOVEMENT_INCREASE = 5f;

	public const float TIME_SLOW_TIC_RATE = 0.5f;

	public const float COOKING_MAGIC_SCALE_HEAL_AMOUNT = 2f;

	public const float COOKING_MAGIC_SCALE_MANA_REGEN_AMOUNT = 0f;

	public const float COOKING_MAGIC_FLAT_MANA_REGEN_AMOUNT = 100f;

	public const float RELOAD_SPECIAL_MESSAGE_ODDS = 0.025f;

	public const float DEFAULT_RELOAD_TIME = 0.6f;

	public const float RELOAD_MIN_RELOAD_TIME = 0.1f;

	public const float KNOCKOUT_SLOW_AMOUNT = 0.05f;

	public static Vector2 KNOCKOUT_MIN_MAX_ANGLE = new Vector2(10f, 170f);

	public const bool ENABLE_CIRCLE_SHIELD = false;

	public const int KATANA_MAX_ANGLE = 35;

	public const float TELESLICE_TELEPORT_DISTANCE = 12f;

	public const float TELESLICE_EXIT_INVINCIBILITY_DURATION = 0.1f;

	public const bool TELESLICE_RESET_ON_KILL = true;

	public const int PISTOL_GUARANTEED_CRIT_NUMBER = 8;

	public const int DRAGON_PISTOL_FIREBALL_NUMBER = 5;

	public const float LADLE_ABILITY_FREE_CRIT_DURATION = 3f;

	public const int MAGIC_WAND_MANA_COST_FOR_FREE_CRIT = 50;

	public const float MAGIC_WAND_FREE_CRIT_DURATION = 3f;

	public const int LUTE_MAX_PROJECTILES = 3;

	public const int MANA_BOMB_Y_PUSH_AMOUNT = 16;

	public const bool SCYTHE_DASH_ON_ATTACK = true;

	public const float SCYTHE_DASH_SPEED = 22f;

	public const float KINETIC_BOW_RELOAD_CHARGE = 1f;

	public const bool CANNON_CAN_ONLY_SHOOT_GROUNDED = false;

	public const float CROWS_NEST_MAX_MOVE = 50f;

	public const float CROWS_NEST_MOVE_SPEED = 14f;

	public static Vector2 CROWS_NEST_RANDOM_BULLET_ANGLES = new Vector2(-1f, 4f);

	public const float CROWS_NEST_DESTROY_PROJ_INITIAL_ANGLE = 0f;

	public const float CROWS_NEST_FIRE_REPEAT_DURATION = 0.15f;

	public const float CROWS_NEST_STOP_FIRE_ELAPSED_PERCENT = 0.8f;

	public const bool CROWS_NEST_DESTROY_ON_EXIT_BOAT = false;

	public const bool CROWS_NEST_ALWAYS_FIRE = true;

	public const bool CROWS_NEST_FULL_MOVEMENT = true;

	public const float COMET_BASE_DURATION = 1.5f;

	public const float COMET_SPEED_MOD = 1f;

	public const float COMET_DISABLE_INVINCIBILITY_DURATION = 0.1f;

	public const bool COMET_MAKES_PLAYER_INVINCIBLE = true;

	public const int COMET_MANA_REQUIRED_PER_CHARGE = 1000;

	public const int COMET_NUM_STARTING_CHARGES = 1;
}
