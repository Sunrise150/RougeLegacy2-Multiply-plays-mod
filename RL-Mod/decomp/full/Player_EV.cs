using UnityEngine;

public class Player_EV
{
	public const int PLAYER_NUM_DEATH_ANIMS = 12;

	public static int PLAYER_DEATH_ANIM_OVERRIDE = -1;

	public const int PLAYER_HEIRLOOM_DEATH_INDEX = 8;

	public const int PLAYER_FART_DEATH_INDEX = 9;

	public static ClassType[] STARTING_UNLOCKED_CLASSES = new ClassType[1] { ClassType.SwordClass };

	public const ClassType PLAYER_STARTING_CLASS = ClassType.SwordClass;

	public const AbilityType PLAYER_STARTING_SPELL = AbilityType.FireballSpell;

	public const AbilityType PLAYER_STARTING_TALENT = AbilityType.ShieldBlockTalent;

	public static float CHARACTER_HIT_SINGLE_BLINK_DURATION = 0.1f;

	public const float PLAYER_QUICK_DROP_ANGLE_AMOUNT = 15f;

	public const float PLAYER_ORIGINAL_SCALE = 1.4f;

	public const float PLAYER_MAX_SCALE = 2.4f;

	public const float PLAYER_MIN_SCALE = 0.7f;

	public const float PLAYER_ATTACK_FLIP_CHECK_DURATION = 0.285f;

	public const float PLAYER_ATTACK_FLIP_BOX_SIZE_X_ADD = 1.25f;

	public const float PLAYER_ATTACK_FlIP_BOX_SIZE_Y_ADD = 1.6f;

	public const bool PLAYER_ATTACK_FLIP_FORCE_DIRECTION = false;

	public const float PLAYER_KNOCKBACK_MOVEMENT_DISABLE_DURATION = 0.25f;

	public const int BASE_EQUIPMENT_WEIGHT = 50;

	public const int BASE_RUNE_WEIGHT = 50;

	public const int PLAYER_BASE_STAT = 15;

	public const float PLAYER_BASE_STRENGTH = 15f;

	public const float PLAYER_BASE_MAGIC = 15f;

	public const int PLAYER_BASE_VITALITY = 15;

	public const float PLAYER_BASE_RESOLVE = 1f;

	public const int PLAYER_GRACE_BONUS_HP = 100;

	public const bool USE_NEW_ARMOR_ALG = true;

	public const int PLAYER_BASE_ARMOR = 0;

	public const float PLAYER_ARMOR_REDUCTION_CONST = 0.7f;

	public const float NEW_ARMOR_MIN_BLOCK_AMOUNT = 0.35f;

	public const int NEW_ARMOR_ONHIT_DEGRADATION = 0;

	public const float PLAYER_BASE_DEXTERITY = 5f;

	public const float PLAYER_BASE_MAGIC_DEXTERITY = 5f;

	public const float PLAYER_BASE_CRIT_CHANCE = 0f;

	public const float PLAYER_BASE_CRIT_DAMAGE = 1.1f;

	public const float PLAYER_SUPERCRIT_CHANCE_MOD = 0.5f;

	public const float PLAYER_SUPERCRIT_DAMAGE_MOD = 0.35f;

	public const float PLAYER_GUARANTEED_CRIT_CHANCE_ADD = 100f;

	public static Color PLAYER_GUARANTEED_CRIT_COLOR = new Color(1f, 0.9f, 0f, 0.5f);

	public const float PLAYER_BASE_MAGIC_CRIT_CHANCE = 0f;

	public const float PLAYER_BASE_MAGIC_CRIT_DAMAGE = 1.1f;

	public const float PLAYER_BASE_DEXTERITY_DIVIDER = 200f;

	public const float PLAYER_FINAL_DEXTERITY_DIVIDER = 1f;

	public const float PLAYER_HEALTH_PER_VITALITY = 10f;

	public const float PLAYER_MAGIC_CRIT_CHANCE_BONUS_DAMAGE = 0.005f;

	public const float HP_LOST_PER_RESOLVE = 0.01f;

	public const float RESOLVE_YELLOW_PERCENT = 1f;

	public const float RESOLVE_RED_PERCENT = 0.5f;

	public const float STARTING_MANA = 1f;

	public const int BASE_MANA = 100;

	public const float BASE_MANA_GAIN_PER_SECOND = 0.75f;

	public const float REGEN_MANA_WHEN_BELOW_PERCENT = 1f;

	public const float MANA_REGEN_INITIAL_DELAY = 2f;

	public const float BASE_MANA_GAIN_PER_ENEMY_HIT = 0.01f;

	public const float BASE_MANA_GAIN_PER_ENEMY_KILL = 0.05f;

	public const float BASE_MANA_GAIN_PER_GOLD = 0.1f;

	public const float BASE_MANA_GAIN_PER_BLOCK = 0.1f;

	public const float BASE_MANA_GAIN_PER_PLAYER_HIT = 0.1f;

	public const float PLAYER_HIT_INVINCIBILITY_DURATION = 1f;

	public const float PLAYER_HIT_STUN_DURATION = 99f;

	public const float PLAYER_HIT_SLOWDOWN_AMOUNT = 0.1f;

	public const float PLAYER_HIT_SLOWDOWN_DURATION = 0.5f;

	public static Vector2 PLAYER_BASE_KNOCKBACK_DISTANCE = new Vector2(10.5f, 13.75f);

	public const float PLAYER_RICOCHET_LOCKOUT_DURATION = 0.45f;

	public static Vector2 BASE_RICOCHET_DISTANCE = new Vector2(-10.5f, 13.75f);

	public const float PLAYER_HIT_AIR_RECOVERY_DELAY = 0.15f;

	public const float PLAYER_AIR_RECOVERY_SLOWDOWN_AMOUNT = 0.1f;

	public const float PLAYER_AIR_RECOVERY_SLOWDOWN_DURATION = 0.1f;

	public const float ACCELERATION_ON_GROUND = 120f;

	public const float ACCELERATION_IN_AIR = 120f;

	public const float WALK_SPEED = 12f;

	public const float STARTING_FLIGHT_DURATION = 5f;

	public const bool TOUCHING_GROUND_STOPS_FLIGHT = false;

	public const bool ENABLE_CUSTOM_MAGE_TALENT_POOL = true;

	public const int STARTING_NUM_JUMPS = 1;

	public const float JUMP_HEIGHT = 8.75f;

	public const float DOUBLE_JUMP_HEIGHT = 4.75f;

	public const float JUMP_TIME_WINDOW = 0.1375f;

	public const float JUMP_RELEASE_FORCE = 4.5f;

	public const float JUMP_LEEWAY_AMOUNT = 0.2f;

	public const float DOUBLE_JUMP_LOCKOUT_DURATION = 0.1f;

	public const float JUMP_QUEUE_DURATION = 0.2f;

	public const bool CAN_JUMP_WHILE_DASHING = true;

	public const bool CAN_JUMP_WHILE_VOID_DASHING = true;

	public const bool ENABLE_OMNIDASH = false;

	public const float DASH_SPEED = 26f;

	public const float DASH_DISTANCE = 8f;

	public const float DASH_COOLDOWN = 0f;

	public const float DASH_VOID_DISTANCE_MOD = 2f;

	public const int STARTING_NUM_DASHES = 0;

	public const float DASH_DAMAGE_MOD = 1.25f;

	public const float DASH_KNOCKBACK_MOD = 0.5f;

	public const bool CAN_DASH_RECOVER = true;

	public const bool CAN_ALWAYS_DASH_GROUNDED = true;

	public static Vector2Int DOWN_STRIKE_DOWNKICK_MINMAX_ANGLE = new Vector2Int(-115, -65);

	public static Vector2Int DOWN_STRIKE_FORWARDKICK_MINMAX_ANGLE = new Vector2Int(-140, -35);

	public const float DOWN_STRIKE_FORWARDKICK_ANGLE = -55f;

	public const float DOWN_STRIKE_SPEED = 32f;

	public const float DOWN_STRIKE_BOUNCE_HEIGHT = 22f;

	public const bool DOWN_STRIKE_RESET_DOUBLEJUMP = false;

	public const bool DOWN_STRIKE_RESET_DASH = false;

	public const float DOWN_STRIKE_DEADZONE = 1f;

	public const bool DOWN_STRIKE_CAN_DASH = true;

	public const bool DOWN_STRIKE_DASH_RETAIN_INERTIA = true;

	public const float SPINKICK_IMMUNITY_GRACE = 0.015f;

	public const float LOOK_MAX_RANGE = 5.25f;

	public const float LOOK_SPEED = 0.25f;

	public const float ABILITY_QUEUE_DURATION = 0.175f;

	public const float SPINKICK_ATTACK_LOCK_DURATION = 0.175f;

	public const float DOWN_STRIKE_BOUNCE_INPUTLOCK_DURATION = 0.275f;

	public const float DUAL_BLADE_CLASS_SUPER_CRIT_ADD = 0.1f;

	public const float SABER_CLASS_FREE_CRIT_DURATION = 1f;

	public const float LADLE_CLASS_BURN_DURATION = 3.05f;

	public const int ASTRO_CLASS_BONUS_MANA_ADD = 1;

	public static float CalculateCritChance(float dexterity)
	{
		return dexterity / (dexterity + 200f) / 1f;
	}
}
