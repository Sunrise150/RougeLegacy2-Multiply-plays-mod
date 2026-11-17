using UnityEngine;

public static class Trait_EV
{
	public const float TRAIT_RARITY_ONE_ODDS = 1f;

	public const float TRAIT_RARITY_TWO_ODDS = 0f;

	public const float TRAIT_RARITY_THREE_ODDS = 0f;

	public const float TRAIT_ODDS_OF_GETTING_FIRST_TRAIT = 0.685f;

	public const float TRAIT_ODDS_OF_GETTING_SECOND_TRAIT = 0.365f;

	public static readonly EnemyType[] TRAIT_EFFECT_EXCEPTION_ARRAY = new EnemyType[5]
	{
		EnemyType.PaintingEnemy,
		EnemyType.MimicChest,
		EnemyType.MimicChestBoss,
		EnemyType.Dummy,
		EnemyType.Target
	};

	public const float ANTIQUE_CHANCE = 0.22f;

	public const float PLAYER_GIGANTISM_SIZE_MOD = 1.5f;

	public const float PLAYER_DWARFISM_SIZE_MOD = 0.55f;

	public static Vector2 PLAYER_KNOCKED_FAR_KNOCKBACK_MOD = new Vector2(1.4f, 1.3f);

	public static Vector2 PLAYER_KNOCKED_LOW_KNOCKBACK_MOD = new Vector2(0.35f, 0.2f);

	public static Vector2 ENEMY_KNOCKED_FAR_KNOCKBACK_MOD = new Vector2(1.4f, 1.4f);

	public static Vector2 ENEMY_KNOCKED_LOW_KNOCKBACK_MOD = new Vector2(0.5f, 0.5f);

	public const float HIGH_JUMP_MAX_JUMP_HEIGHT_MOD = 1.5f;

	public const float LOWER_STORE_PRICE_DISCOUNT_MOD = 0.85f;

	public const float COLOR_TRAILS_TAIL_DURATION = 18f;

	public const float COLOR_TRAILS_SIZE_MOD = 1.5f;

	public const float SMALL_HITBOX_HEALTH_MOD = 0.75f;

	public const float SMALL_HITBOX_SIZE_MOD = 0.1f;

	public const float CANNOT_ATTACK_HEALTH_MOD = 0.4f;

	public const float CAN_NOW_ATTACK_HEALTH_MOD = 0.4f;

	public const float DAMAGE_BOOST_WEAPON_DAMAGE_MOD = 1.5f;

	public const float DAMAGE_BOOST_HEALTH_MOD = 0.7f;

	public const float DAMAGE_BOOST_MANA_MOD = 1f;

	public const float MAGIC_BOOST_MAGIC_DAMAGE_MOD = 1.5f;

	public const float MAGIC_BOOST_HEALTH_MOD = 0.7f;

	public const float MAGIC_BOOST_MANA_MOD = 1.5f;

	public const float MANA_REGEN_MAGIC_MOD = 1f;

	public const float MANA_REGEN_WEAPON_DAMAGE_MOD = 0.5f;

	public const float MANA_REGEN_HEALTH_MOD = 0.35f;

	public const float MANA_REGEN_MANA_MOD = 0.75f;

	public const float BONUS_STRENGTH_STRENGTH_MOD = 1.1f;

	public const float BONUS_HEALTH_HEALTH_MOD = 0.1f;

	public const float BLUR_ON_HIT_DURATION = 2.5f;

	public const float BLUR_ON_HIT_RADIUS = 4f;

	public const float BLURRY_CLOSE_RADIUS = 10f;

	public const float BLURRY_FAR_RADIUS = 8f;

	public const float HORIZONTAL_DARKNESS_VISION_SCALE = 6f;

	public const float DARK_SCREEN_RADIUS = 15.5f;

	public const float CHEER_ON_KILLS_SPOTLIGHT_SCALE = 5.5f;

	public const bool CHEER_ON_KILLS_SPOTLIGHT_ENEMIES = true;

	public static Vector3 FART_POSITION = new Vector3(-1.25f, 1.2f, 0f);

	public const float FART_CHANCE = 0.16f;

	public const float SUPER_FART_CHANCE = 0.09f;

	public static Vector2 SUPER_FART_ACCELERATION = new Vector2(0f, 2f);

	public const float SUPER_FART_DURATION = 0.15f;

	public const float BREAK_PROPS_FOR_MANA_REGEN_AMOUNT = 0.01f;

	public const float BREAK_PROPS_FOR_MANA_CD_REDUC_AMOUNT = 0f;

	public static Vector2 FAKE_SELF_DAMAGE_DAMAGE_TAKEN_MOD = new Vector2(1.75f, 6.5f);

	public static Vector2 FAKE_SELF_DAMAGE_DAMAGE_DEALT_MOD = new Vector2(4f, 15f);

	public const float FAKE_SELF_DAMAGE_SCREENSHAKE_MOD = 2.5f;

	public const float MANACOSTANDDAMAGEUP_CD_MOD = 1f;

	public const float MANACOSTANDDAMAGEUP_SPELLCOST_MOD = 2f;

	public const float MANACOSTANDDAMAGEUP_DMG_MOD = 2f;

	public const float LOWER_GRAVITY_GRAVITY_ASCENT_MOD = 1f;

	public const float LOWER_GRAVITY_GRAVITY_DESCENT_MOD = 0.35f;

	public const float LONGER_CD_MOD = 1.25f;

	public const float LONGER_CD_ADD = 5f;

	public const float NO_MEAT_DAMAGE_MOD = 1f;

	public const byte DISPOSITION_NUM_SIDS = 4;

	public const byte DISPOSITION_NUM_GIDS = 3;

	public const float VAMPIRISM_DAMAGE_INCREASE_MOD = 2.25f;

	public const float VAMPIRISM_LIFE_STEAL_MOD = 0.2f;

	public const float NO_IMMUNITY_WINDOW_ON_HIT_IMMUNITY_DURATION = 0.1f;

	public const float MEGA_HEALTH_HEALTH_MOD = 2f;

	public const float MANA_FROM_HURT_MANA_GAIN = 0.5f;

	public static Vector2 BONUS_CHEST_GOLD_DIE_ROLL = new Vector2(0f, 7f);

	public static float BONUS_CHEST_GOLD_DIE_MOD = 0.5f;

	public static Vector2 BONUS_CHEST_GOLD_GOLD_MOD = new Vector2(0f, 0f);

	public const float REVEAL_ALL_CHESTS_HEALTH_MOD = 0.8f;

	public const float SUPER_HEALER_HEALTH_MOD = 1f;

	public const float SUPER_HEALER_HEALTH_REGEN_DELAY = 2.5f;

	public const float SUPER_HEALER_REGEN_PERCENT = 0.05f;

	public const float SUPER_HEALER_MAX_HEALTH_PERCENT_MOD = 0.0625f;

	public const float SUPER_HEALER_REGEN_TICK = 0.25f;

	public const float OMNI_DASH_DISTANCE_MOD = 1f;

	public const float OMNI_DASH_HEALTH_MOD = 0.8f;

	public const float BOUNCE_TERRAIN_HEALTH_MOD = 0.7f;

	public const float DISARM_ON_HURT_DURATION = 3.5f;

	public const float NOT_MOVING_SLOW_GAME_IDLE_MOD = 0.01f;

	public const float NO_MANA_CAP_DAMAGE_MOD = 0.02f;

	public const float NO_MANA_CAP_TICK_DURATION = 1f;

	public const string EXPLOSIVE_ENEMIES_PROJECTILE_NAME = "ExplosiveEnemiesPotionProjectile";

	public const string EXPLOSIVE_ENEMIES_EXPLOSION_PROJECTILE_NAME = "ExplosiveEnemiesPotionExplosionProjectile";

	public static Vector2 EXPLOSIVE_ENEMIES_PROJECTILE_ANGLE = new Vector2(85f, 95f);

	public const string EXPLOSIVE_CHESTS_PROJECTILE_NAME = "ExplosiveChestsPotionProjectile";

	public const string EXPLOSIVE_CHESTS_EXPLOSION_PROJECTILE_NAME = "ExplosiveChestsPotionExplosionProjectile";

	public static Vector2 EXPLOSIVE_CHEST_PROJECTILE_ANGLE = new Vector2(60f, 120f);

	public const float ANGRY_ON_HIT_DURATION = 7.5f;

	public const float ANGRY_ON_HIT_DAMAGE_BONUS = 1.25f;

	public const float ANGRY_ON_HIT_MOVESPEED_BONUS = 1.5f;

	public const float ANGRY_ON_HIT_RED_TINT_OPACITY = 0.75f;

	public const float INVULN_DASH_HEALTH_MOD = 0.5f;

	public static Vector2 GAME_SHAKE_TIME_RANDOMIZER = new Vector2(8f, 18f);

	public static Vector2 GAME_SHAKE_DURATION = new Vector2(1f, 3f);

	public static Vector2 GAME_SHAKE_INTENSITY = new Vector2(0.5f, 2f);
}
