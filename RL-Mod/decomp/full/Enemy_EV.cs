using UnityEngine;

public static class Enemy_EV
{
	public static Vector2 ENEMY_BASE_KNOCKBACK_DISTANCE = new Vector2(6.75f, 12.25f);

	public static Vector2 ENEMY_DEATH_KNOCKBACK_DISTANCE = new Vector2(30f, 20f);

	public const float ENEMY_FLIGHT_KNOCKBACK_DECELERATION = 15f;

	public const float ENEMY_FLIGHT_IDLE_DECELERATION_MOD = 1.5f;

	public const float ENEMY_KILLED_SLOWDOWN_AMOUNT = 0.01f;

	public const float ENEMY_KILLED_SLOWDOWN_DURATION = 0f;

	public const float ENEMY_HIT_SLOWDOWN_AMOUNT = 0.5f;

	public const float ENEMY_HIT_SLOWDOWN_DURATION = 0.065f;

	public const float ENEMY_HIT_SLOWDOWN_CD = 0.55f;

	public const float FORCE_ENEMY_IDLE_DURATION_ON_ROOM_TRANSITION = 0.175f;

	public const float FORCE_ENEMY_IDLE_DURATION_ON_ROOM_TRANSITION_LONG = 0.7f;

	public const float HEALTH_GAIN_PER_LEVEL_MOD = 0.0625f;

	public const float STRENGTH_GAIN_PER_LEVEL_MOD = 0.0575f;

	public static float MIN_AGGRO_DURATION = 3.25f;

	public const float ENEMY_ACTIVATION_WIDTH = 9f;

	public const float ENEMY_ACTIVATION_HEIGHT = 5.5f;

	public const float ENEMY_DEACTIVATION_WIDTH = 16f;

	public const float ENEMY_DEACTIVATION_HEIGHT = 12f;

	public const float ENEMY_POSITION_RESET_TIMEOUT = 5f;

	public const float ENEMY_FLAMETHROWER_HITBOX_SPAWN_DELAY = 0.25f;

	public const float ENEMY_FALSE_LEVEL_SCALING = 2.5f;

	public const float ENEMY_FAR_RANGE_MOD = 0.5f;

	public const bool USE_RECTS_FOR_LOGIC_RANGES = true;

	public const float ENEMY_RANGE_RECT_RATIO = 0.6f;

	public const bool DISABLE_ITEM_DROP_ON_ENEMY_DEATH = false;

	public const bool DISABLE_ITEM_DROP_ON_SUMMONED_ENEMY_DEATH = true;

	public const bool DISABLE_EXPERT_BOUNCY_SPIKES = true;

	public const bool ENABLE_DASH_KILL_INDICATOR = false;

	public const float COMMANDER_HP_MOD = 1.75f;

	public const float COMMANDER_DMG_MOD = 1f;

	public const float ARMOR_SHRED_CRIT_DAMAGE_MOD = 1.15f;

	public const float ATTACKING_WITH_CONTACT_DAMAGE_TRIGGER_DELAY = 0.1f;

	public const bool TEDDY_CUSTOM_CONTACT_DAMAGE_CURATION = true;

	public const float ENEMY_AGGRESSION_MOD_CLAMPED_MIN = 0.25f;

	public static float GetSummonValue(EnemyRank enemyRank)
	{
		return enemyRank switch
		{
			EnemyRank.Expert => 2f, 
			EnemyRank.Miniboss => 10f, 
			_ => 1f, 
		};
	}
}
