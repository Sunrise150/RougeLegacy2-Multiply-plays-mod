using System.Collections.Generic;
using UnityEngine;

public static class Hazard_EV
{
	public const bool USE_ROOM_LEVEL_FOR_DAMAGE_CALCULATION = true;

	public const float HAZARD_DAMAGE_FLAT = 16f;

	public const float HAZARD_DAMAGE_ENEMY_MULTIPLIER = 0.2f;

	public const float SPIKE_DAMAGE_MULTIPLIER = 0.2f;

	public const float SPIKETRAP_BASE_DAMAGE = 30f;

	public const float SPIKETRAP_EXTENSION_DELAY = 0.425f;

	public const float SPIKETRAP_RETRACTION_DELAY = 0.45f;

	public const float BREAKABLE_SPIKE_ON_ENTER_REGEN_DELAY = 0.2f;

	public const float BREAKABLE_SPIKE_DAMAGE_DELAY = 1f;

	public const float BREAKABLE_SPIKE_TALL_ON_ENTER_REGEN_DELAY = 0.2f;

	public const float BREAKABLE_TALL_SPIKE_DAMAGE_DELAY = 1.3f;

	public const float BREAKABLE_SPIKE_TALL_ON_HIT_REGEN_DELAY = 6.5f;

	public const float REGULAR_TELEPORTING_HAZARD_DAMAGE_MULTIPLIER = 0.15f;

	public const float HEIRLOOM_TELEPORTING_HAZARD_DAMAGE_MULTIPLIER = 0f;

	public const float BUZZSAW_MOVE_SPEED = 10f;

	public const float BUZZSAW_RETRACT_DURATION = 2f;

	public const float PRESSURE_PLATE_DELAY_BEFORE_FIRING = 0.2f;

	public const float PRESSURE_PLATE_FIRE_DURATION = 0.6f;

	public const float SPRING_POWER = 42f;

	public const float SPRING_RESET_DELAY = 0.25f;

	public const bool SPRING_TRIGGERS_ON_JUMP = false;

	public const float CONVEYOR_SPEED = 10f;

	public const float TURRET_ACTIVATION_DISTANCE = 55f;

	public const float TURRET_BASE_DAMAGE = 25f;

	public const float TURRET_TELL_DELAY = 0.75f;

	public const float TURRET_FORCED_INITIAL_DELAY = 0.875f;

	public const float TURRET_NORMAL_LOOP_DELAY = 2.5f;

	public const float TURRET_NORMAL_PROJ_SPEED_MOD = 10f;

	public const float TURRET_SLOW_LOOP_DELAY = 3.5f;

	public const float TURRET_SLOW_PROJ_SPEED_MOD = 10f;

	public const float TURRET_FAST_LOOP_DELAY = 1.5f;

	public const float TURRET_FAST_PROJ_SPEED_MOD = 10f;

	public const float FLAME_TURRET_ON_DURATION_MOD = 0.5f;

	public const float FLAME_TURRET_OFF_DURATION_MOD = 1.5f;

	public const float RAYCAST_TURRET_INITIALIZATION_DELAY = 1f;

	public const float RAYCAST_TURRET_FIRE_DELAY = 0.5f;

	public const float RAYCAST_TURRET_DELAY_BETWEEN_SHOTS = 4f;

	public const float RAYCAST_TURRET_DETECTED_RANGE = 25f;

	public const float RAYCAST_CURSE_TURRET_INITIALIZATION_DELAY = 1f;

	public const float RAYCAST_CURSE_TURRET_FIRE_DELAY = 0.5f;

	public const float RAYCAST_CURSE_TURRET_DELAY_BETWEEN_SHOTS = 8f;

	public const float RAYCAST_CURSE_TURRET_DETECTED_RANGE = 15f;

	public const float ORBITER_DAMAGE_MULTIPLIER = 0.25f;

	public const float ORBITER_BASE_DAMAGE = 30f;

	public const float ORBITER_BASE_ROTATION_SPEED = 100f;

	public const float ORBITER_EXPANSION_DURATION = 2.5f;

	public const float ORBITER_ENABLE_DAMAGE_DELAY = 0.25f;

	public static Dictionary<BiomeType, float> POINT_HAZARD_RADIUS_MULTIPLIER = new Dictionary<BiomeType, float>();

	public const float WINDMILL_BASE_ROTATION_SPEED = 75f;

	public const float PROXIMITY_MINE_ATTACK_COOLDOWN = 7.5f;

	public const float PROXIMITY_MINE_RADIUS_MOD = 2f;

	public const float PROXIMITY_MINE_ATTACK_TELL_DELAY = 2.25f;

	public const float PROXIMITY_MINE_TRIGGER_RADIUS = 5f;

	public const float PROXIMITY_PROJECTILE_ATTACK_COOLDOWN = 8.5f;

	public const float PROXIMITY_PROJECTILE_ATTACK_TELL_DELAY = 1.25f;

	public const float PROXIMITY_PROJECTILE_TRIGGER_RADIUS = 5f;

	public const float VOID_TRAP_COOLDOWN = 10f;

	public const string VOID_TRAP_PROJECTILE_NAME = "VoidTrapHazardProjectile";

	public static Vector2 VOID_TRAP_CAROUSEL_OFFSET = Vector2.zero;

	public const float VOID_TRAP_RADIUS_MOD = 1.17f;

	public const float VOID_TRAP_TELL_DELAY = 0.25f;

	public const float VOID_TRAP_ATTACK_DURATION = 5f;

	public const float VOID_TRAP_TRIGGER_RADIUS = 3f;

	public const int VOID_TRAP_PROJECTILE_COUNT = 8;

	public const float VOID_TRAP_ROTATION_SPEED = 360f;

	public const float SHRINKONDASH_STATE_ONE_SCALE = 6f;

	public const float SHRINKONDASH_STATE_TWO_SCALE = 6f;

	public const float SHRINKONDASH_SHRINK_SCALE = 0.25f;

	public const float SHRINKONDASH_RESIZE_DELAY = 2f;

	public const float SHRINKONDASH_RESIZE_SPEED = 1.5f;

	public const float LOCAL_TELEPORTER_EXIT_IMMUNITY_WINDOW = 0.2f;

	public const float CLOUD_RESPAWN_DURATION = 0.25f;

	public const float SNOWMOUND_MOVEMENT_MOD = 0.35f;

	public const bool SNOWMOUND_ONLY_SLOWS_ON_GROUND = true;

	public const float SENTRY_ATTACK_COOLDOWN = 0.5f;

	public const string SENTRY_PROJECTILE_NAME = "SentryBounceBoltProjectile";

	public static Vector2 SENTRY_PROJECTILE_OFFSET = new Vector2(0f, 0.5f);

	public const float SENTRY_RADIUS_RANGE_CHECK_MOD = 1.75f;

	public const float SENTRY_ATTACK_TELL_DELAY = 0.25f;

	public const float SENTRY_REST_DURATION = 9f;

	public const float SENTRY_FORCED_INITIAL_DELAY = 0.875f;

	public const float ICE_CRYSTAL_GROWTH_SPEED = 0.3f;

	public const float ICE_CRYSTAL_INITIAL_GROWTH_DELAY = 0.75f;

	public const float ICE_CRYSTAL_SHATTER_GROWTH_DELAY = 2.75f;

	public const float ICE_CRYSTAL_SHATTER_DAMAGE_DELAY = 0.65f;

	public const bool ICE_CRYSTAL_SYNC_DISABLE_GROWTH_WHILE_ASLEEP = true;

	public const bool ICE_CRYSTAL_SYNC_GROW_ON_AWAKEN = true;

	public const float CORPSE_SPAWN_ODDS = 0.375f;

	public const float EXHAUST_POINT_RADIUS_RANGE_CHECK_MOD = 1.75f;

	public const int EXHAUST_POINT_EXHAUST_AMOUNT = 20;

	public const float BRIDGE_BIOME_UP_CANNONBALL_INITIAL_DELAY = 5f;

	public const float BRIDGE_BIOME_UP_CANNONBALL_MIN_DELAY = 8f;

	public const float BRIDGE_BIOME_UP_CANNONBALL_MAX_DELAY = 13f;

	public static Vector2 VOID_WAVE_INITIAL_DELAY = new Vector2(2f, 4f);

	public static Vector2 VOID_WAVE_RANDOM_DELAY = new Vector2(8f, 12.5f);

	public const float VOID_WAVE_WARNING_DURATION = 2.5f;

	public const float NIGHTMARE_LANCER_WARNING_DURATION = 1.75f;

	public const bool TOWER_LANCER_SPAWNS_RELATIVE_TO_SCREEN = false;

	public const float FLYING_WEAPON_WAVE_WARNING_DURATION = 1.5f;

	public const bool FLYING_WEAPON_ENABLE_LOCK_IN_GRACE_DELAY = true;

	public const float FLYING_WEAPON_WAVE_WARNING_LOCKED_DURATION = 0.35f;

	public const float WATER_RISING_DELAY = 3f;

	public const float RISING_WATER_SPEED = 5f;

	public static Vector2 NIGHTMARE_LANCER_INITIAL_DELAY => new Vector2(2f, 4f);

	public static Vector2 NIGHTMARE_LANCER_RANDOM_DELAY => new Vector2(3.5f, 6f);

	public static Vector2Int NIGHTMARE_LANCER_RANDOM_AMOUNT => new Vector2Int(1, 2);

	public static Vector2 NIGHTMARE_LANCER_RANDOM_SPAWN_INTERVAL => new Vector2(0.4f, 0.75f);

	public static Vector2 NIGHTMARE_LANCER_RANDOM_X_OFFSET => new Vector2(8f, 20f);

	public static Vector2 NIGHTMARE_LANCER_RANDOM_Y_OFFSET => new Vector2(0f, 10f);

	public static Vector2 FLYING_WEAPON_WAVE_INITIAL_DELAY => new Vector2(5.5f, 7.5f);

	public static Vector2 FLYING_WEAPON_WAVE_RANDOM_DELAY => new Vector2(11f, 14.5f);

	public static Vector2 FLYING_WEAPON_WAVE_RANDOM_POS_OFFSET => new Vector2(-3f, 3f);

	public static Vector2 FLYING_WEAPON_WAVE_RANDOM_ANGLE => new Vector2(-10f, 10f);

	public static float GetDamageAmount(BaseRoom room)
	{
		float result = 0f;
		if ((bool)room)
		{
			int level = room.Level;
			float num = 0.007f * (float)SaveManager.PlayerSaveData.NewGamePlusLevel;
			result = ((float)level * 0.0575f + num + 1f) * 16f;
			result *= 1f + BurdenManager.GetBurdenStatGain(BurdenType.RoomThreat);
		}
		else if (PlayerManager.IsInstantiated)
		{
			PlayerManager.GetPlayerController();
			result = 16f;
			result *= 1f + BurdenManager.GetBurdenStatGain(BurdenType.RoomThreat);
		}
		return result;
	}
}
