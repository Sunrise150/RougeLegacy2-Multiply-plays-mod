using UnityEngine;

public static class Global_EV
{
	public const float FORCE_OF_GRAVITY = -50f;

	private const float TERMINAL_VELOCITY = -38f;

	public const float GLOBAL_FALL_MULTIPLIER = 1f;

	public const float GLOBAL_ASCENT_MULTIPLIER = 1f;

	public const float DEFAULT_RUMBLE_AMOUNT = 0.5f;

	public const float REPEAT_HIT_DURATION = 0.5f;

	public const float PROJECTILE_CAN_HIT_OWNER_DELAY = 0.5f;

	public const int ENEMY_ROTATION_STEPPING_INCREMENT = 40;

	public const int ROOM_COUNT_BURDEN_STARTING_LEVEL = 0;

	public const bool INFINITE_PURCHASING_POWER = false;

	public const FoundState FORCED_EQUIPMENT_FOUNDSTATE = FoundState.NotFound;

	public const FoundState FORCED_RUNE_FOUNDSTATE = FoundState.NotFound;

	public const FoundState FORCED_CHALLENGE_FOUNDSTATE = FoundState.NotFound;

	public const FoundState FORCED_EMPATHY_FOUNDSTATE = FoundState.NotFound;

	public const FoundState FORCED_BURDEN_FOUNDSTATE = FoundState.NotFound;

	public const bool UNLOCK_ALL_EQUIPMENT_SETS = false;

	public const bool UNLOCK_SKILL_TREE = false;

	public const bool UNLOCK_ALL_SHOPS = false;

	public const bool UNLOCK_ALL_JOURNAL_ENTRIES = false;

	public const bool UNLOCK_ALL_GENETICIST_ENTRIES = false;

	public const bool UNLOCK_ALL_TRAITS_SEEN = false;

	public const bool UNLOCK_ALL_SONGS = false;

	public const InsightState FORCED_INSIGHT_STATE = InsightState.Undiscovered;

	public const bool UNLOCK_TRAIT_GOLD_GAIN_SKILL = false;

	public const bool UNLOCK_ALL_TROPHIES = false;

	public const bool ALLOW_INFINITE_CHILD_REROLLS = false;

	public const bool DISABLE_SKILL_TREE_ANIM_LOCKING = false;

	public const bool FORCE_SAVE_FLAGS_TRUE = false;

	public const bool DISABLE_STARTING_SHOP_DIALOGUE = false;

	public const bool RUN_RANDOM_PORTRAITS_TEST = false;

	public const bool DISABLE_SAVING = false;

	public const bool DISABLE_SKILLTREE_LOAD_ON_ENTER_HUBTOWN = false;

	public const bool HIDE_INPUT_ICONS = false;

	public const bool GENERATE_RANDOM_CHAR_CLASS = false;

	public const bool UNLOCK_ALL_CLASSES = false;

	public const bool LOAD_DEMO = false;

	public const bool CYCLE_ENEMY_ATTACKS_FOR_AUDIO_TESTING = false;

	public const bool DISABLE_BOSS_INTROS = false;

	public const bool FORCE_UPDATE_SAVE_FILES = false;

	public const bool DISABLE_DEATH_PANNING = false;

	public const bool DISABLE_TUTORIAL = false;

	public const bool USE_TRAILER_CONFIG = false;

	public const bool DISABLE_CULLING_GROUPS = false;

	public const bool DISABLE_INTRO_CUTSCENE = false;

	public const bool UNLOCK_ALL_HEIRLOOMS = false;

	public const bool DISABLE_HEIRLOOM_REQUIREMENT_LOCKING = false;

	public const bool DISABLE_SAVEFILE_ROOM_AUTOCORRECTION = false;

	public const bool DISABLE_SKILL_TREE_POPUPS = false;

	public const int MASTERY_STARTING_LEVEL = 0;

	public const bool DISABLE_REVISION_FILE_UPDATING = false;

	public const bool DISABLE_PAUSE_ON_LOSE_FOCUS = false;

	public const bool DISABLE_BOSS_BEATEN_FINAL_DOOR_CUTSCENE = false;

	public const bool UNLOCK_INFINITE_RELIC_REROLLS = false;

	public const bool DISABLE_SKILL_TREE_LEVELS = false;

	public const bool ENABLE_NPC_DIALOGUE_TESTING = false;

	public const bool IGNORE_NPC_DIALOGUE_CONDITIONS = false;

	public const DeathSpecialCutsceneState DEATH_SPECIAL_CUTSCENE_STATE = DeathSpecialCutsceneState.Default;

	public const bool UNLOCK_ALL_NGPLUS_LEVELS = false;

	public static Vector2Int RELIC_ROOM_TEST_RELICS = new Vector2Int(0, 0);

	public const int RELIC_ROOM_TEST_NUM_RELICS_GAINED = -1;

	public const int DEBUG_GIVE_SKILLS_AMOUNT = 0;

	public const bool UNLOCK_ALL_SCAR_MEMORIES = false;

	public const bool UNLOCK_LINEAGE_VARIANTS = false;

	public const bool DISABLE_BIOMERULE_ENEMY_MODS = false;

	public const bool UNLOCK_MAX_EQUIPMENT_LEVELS = false;

	public const int STARTING_NUM_JUMPS = 1;

	public const int STARTING_NUM_DASHES = 0;

	public const HolidayType HOLIDAY_TYPE_TESTER = HolidayType.None;

	public const bool UNLOCK_GOLDEN_DOORS = false;

	public const bool KILL_PLAYER_ON_GAME_COMPLETION = true;

	public const bool FORCE_CONTACT_DAMAGE_TESTING = false;

	private static bool m_isTerminalVelocityOverrideSet = false;

	private static float m_terminalVelocityOverride = 0f;

	public static float TerminalVelocity
	{
		get
		{
			if (m_isTerminalVelocityOverrideSet)
			{
				return m_terminalVelocityOverride;
			}
			return -38f;
		}
	}

	public static void SetTerminalVelocityOverride(bool isOverrideSet, float value = 0f)
	{
		m_isTerminalVelocityOverrideSet = isOverrideSet;
		m_terminalVelocityOverride = value;
	}
}
