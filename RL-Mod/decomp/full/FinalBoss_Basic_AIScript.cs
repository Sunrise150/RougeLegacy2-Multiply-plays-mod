using System.Collections;
using FMOD.Studio;
using RLAudio;
using RL_Windows;
using Sigtrap.Relays;
using UnityEngine;

public class FinalBoss_Basic_AIScript : BaseAIScript
{
	public const bool START_IN_WHITE_MODE = true;

	protected const string VERTICAL_BEAM_BLACK_STOP_SFX_NAME = "event:/SFX/Enemies/Cain/sfx_cain_dark_staff_attack_lightning_end";

	protected const string VERTICAL_BEAM_BLACK_LOOP_SFX_NAME = "event:/SFX/Enemies/Cain/sfx_cain_dark_staff_attack_lightning_loop";

	protected const string VERTICAL_BEAM_BLACK_FLOOR_LOOP_SFX_NAME = "event:/SFX/Enemies/Cain/sfx_cain_dark_staff_attack_floor_loop";

	private Relay<bool, float> m_colourShiftRelay = new Relay<bool, float>();

	private static float[] MODESHIFT_ARRAY = new float[2] { 0.75f, 0.5f };

	private static Vector2Int COLOURSHIFT_MINMAX_COUNT = new Vector2Int(1, 2);

	private EventInstance m_rumbleEventInstance;

	private EventInstance m_collapseSpiritsEventInstance;

	private EventInstance m_verticalBeamBlackEventInstance;

	private EventInstance m_verticalBeamBlackFloorLoopEventInstance;

	private bool m_isInWhiteMode;

	private bool m_isModeShifting;

	protected int m_modeShiftIndex;

	private int m_currentColourShiftCount;

	private int m_requiredColourShiftCount;

	private GameObject m_regularWeaponHitbox;

	private GameObject m_jumpingWeaponHitbox;

	private GameObject m_regularBodyHitbox;

	private GameObject m_jumpingBodyHitbox;

	private GameObject m_prayingBodyHitbox;

	private FinalBossColourShiftController m_colourShiftController;

	private GameObject m_statue;

	private GameObject m_model;

	protected const string MODESHIFT_DOWNED = "ModeShift_Intro";

	protected const string MODESHIFT_GETUP = "ModeShift_GetUp";

	protected const string MODESHIFT_TELL_INTRO = "ModeShift_Scream_Intro";

	protected const string MODESHIFT_TELL_HOLD = "ModeShift_Tell_Loop";

	protected const string MODESHIFT_ATTACK_INTRO = "ModeShift_Scream_Hold";

	protected const string MODESHIFT_ATTACK_HOLD = "ModeShift_Action_Loop";

	protected const string MODESHIFT_EXIT = "ModeShift_Scream_Exit";

	protected float m_modeShift_Damage_Mod = 0.1f;

	protected float m_modeShift_Downed_AnimSpeed = 1f;

	protected float m_modeShift_Downed_Delay;

	protected float m_modeShift_GetUp_AnimSpeed = 1f;

	protected float m_modeShift_TellIntro_AnimSpeed = 1.25f;

	protected float m_modeShift_TellHold_AnimSpeed = 1.25f;

	protected float m_modeShift_TellIntroAndHold_Delay = 2.5f;

	protected float m_modeShift_AttackIntro_AnimSpeed = 1.25f;

	protected float m_modeShift_AttackHold_AnimSpeed = 1.25f;

	protected float m_modeShift_AttackHold_Delay = 1.5f;

	protected float m_modeShift_Exit_AnimSpeed = 0.65f;

	protected float m_modeShift_Exit_Delay;

	protected float m_modeShift_Exit_IdleDuration = 0.1f;

	protected float m_modeShift_AttackCD = 99f;

	protected string MODESHIFT_BOLT_PROJECTILE = "FinalBossModeShiftBoltProjectile";

	protected string MODESHIFT_SHOUT_PROJECTILE = "FinalBossModeShiftShoutProjectile";

	protected string MODESHIFT_SHOUT_WARNING_PROJECTILE = "FinalBossModeShiftShoutWarningProjectile";

	protected int MODESHIFT_SHOUT_POSITION_INDEX = 1;

	protected int MODESHIFT_POSITION_INDEX = 14;

	protected Vector2 m_modeShift_ThrowAngle = new Vector2(30f, 85f);

	protected Vector2 m_modeShift_ThrowPower = new Vector2(1f, 2.65f);

	protected float m_modeShift_LoopDelay = 0.225f;

	protected int m_modeShift_Loops = 15;

	private Projectile_RL m_modeShiftShoutWarningProj;

	private Projectile_RL m_modeShiftShoutProj;

	private Coroutine m_modeShiftProjectileCoroutine;

	protected const string SUMMON_TREE_EXPLOSION_PROJECTILE = "FinalBossTreeExplosionProjectile";

	protected const string SUMMON_TREE_WARNING_PROJECTILE = "FinalBossTreeWarningProjectile";

	protected const string SUMMON_TREE_PROJECTILE_WHITE = "FinalBossWhiteTreeProjectile";

	protected const string SUMMON_TREE_PROJECTILE_BLACK = "FinalBossBlackTreeProjectile";

	protected const string TREE_CHASE_PROJECTILE = "FinalBossWhiteTreeBoltProjectile";

	protected const string TREE_VOID_PROJECTILE = "FinalBossBlackTreeBoltProjectile";

	protected float m_summonTree_AttackDuration = 2f;

	protected float m_summonTree_TellIntro_AnimSpeed = 1.2f;

	protected float m_summonTree_TellHold_AnimSpeed = 1.2f;

	protected float m_summonTree_TellIntroAndHold_Delay = 0.95f;

	protected float m_summonTree_Warning_Delay = 1f;

	protected float m_summonTree_AttackIntro_AnimSpeed = 1.2f;

	protected float m_summonTree_AttackIntro_Delay;

	protected float m_summonTree_AttackHold_AnimSpeed = 1.2f;

	protected float m_summonTree_AttackHold_Delay;

	protected float m_summonTree_AttackHold_ExitDelay = 0.75f;

	protected float m_summonTree_Exit_AnimSpeed = 1.2f;

	protected float m_summonTree_Exit_Delay;

	protected float m_summonTree_Exit_IdleDuration = 0.1f;

	protected float m_summonTree_AttackCD = 12f;

	protected float m_treeProjectile_InitialDelay = 2.5f;

	protected float m_whiteTreeProjectile_Delay = 8f;

	protected float m_blackTreeProjectile_Delay = 8f;

	protected int m_numTreeVoidProjectiles = 8;

	protected float m_treeVoidSpeedMod = 0.65f;

	private Projectile_RL m_summonTreeProjectile;

	private Projectile_RL m_summonTreeWarningProjectile;

	private Coroutine m_treePersistentCoroutine;

	private bool m_summonedWhiteTree;

	protected const string PRAYER_EXPLOSION_PROJECTILE = "FinalBossTreeExplosionProjectile";

	protected const string PRAYER_WARNING_PROJECTILE = "FinalBossTreeWarningProjectile";

	protected const string PRAYER_VOID_PROJECTILE = "FinalBossPrayerVoidProjectile";

	protected const string PRAYER_BOUNCE_PROJECTILE = "FinalBossPrayerBounceProjectile";

	protected const int PRAYER_VOID_POS = 0;

	protected const int PRAYER_BOUNCE_POS_1 = 9;

	protected const int PRAYER_BOUNCE_POS_2 = 10;

	protected const int PRAYER_BOUNCE_POS_3 = 11;

	protected const int PRAYER_BOUNCE_POS_4 = 12;

	protected const int PRAYER_BOUNCE_POS_5 = 13;

	protected float m_prayer_AttackDuration = 2f;

	protected float m_prayer_TellIntro_AnimSpeed = 1.2f;

	protected float m_prayer_TellHold_AnimSpeed = 1.2f;

	protected float m_prayer_TellIntroAndHold_Delay = 1.25f;

	protected float m_prayer_AttackIntro_AnimSpeed = 1.2f;

	protected float m_prayer_AttackIntro_Delay;

	protected float m_prayer_AttackHold_AnimSpeed = 1.2f;

	protected float m_prayer_AttackHold_Delay;

	protected float m_prayer_AttackHold_ExitDelay;

	protected float m_prayer_Exit_AnimSpeed = 0.65f;

	protected float m_prayer_Exit_Delay;

	protected float m_prayer_Exit_IdleDuration = 0.1f;

	protected float m_prayer_AttackCD = 12f;

	protected float m_prayer_VoidLoopDelay = 0.75f;

	protected float m_prayer_VoidLoopAdvancedFollowUpDelay = 0.25f;

	protected float m_prayer_BounceLoopDelay = 1f;

	protected float m_prayer_BounceLoopAdvancedDelay = 0.35f;

	protected float m_prayer_BounceAdvancedVoidInitialDelay = 0.85f;

	protected const string VERTICAL_BEAM_PROJECTILE_WHITE = "FinalBossWhiteVerticalProjectile";

	protected const string VERTICAL_BEAM_PROJECTILE_BLACK = "FinalBossBlackVerticalProjectile";

	protected const string VERTICAL_BEAM_VERTICAL_BOLT_PROJECTILE_BLACK = "FinalBossBlackVerticalBoltProjectile";

	protected const string VERTICAL_BEAM_BLACK_WARNING_PROJECTILE = "FinalBossBlackVerticalWarningProjectile";

	protected const string VERTICAL_BEAM_BLACK_BOUNCE = "FinalBossBounceOrbVerticalProjectile";

	protected const string VERTICAL_BEAM_BLACK_VOID = "FinalBossVerticalVoidSmallProjectile";

	protected const string VERTICAL_CURSE_PROJECTILE = "FinalBossVerticalCurseProjectile";

	protected const string VERTICAL_CURSE_BLUE_PROJECTILE = "FinalBossVerticalCurseBlueProjectile";

	protected const int VERTICAL_BEAM_WHITE_POS_INDEX = 2;

	protected float m_verticalBeam_TellIntro_AnimSpeed = 1.2f;

	protected float m_verticalBeam_TellHold_AnimSpeed = 1.2f;

	protected float m_verticalBeam_TellIntroAndHold_Delay = 1.1f;

	protected float m_verticalBeam_AttackIntro_AnimSpeed = 1.2f;

	protected float m_verticalBeam_AttackIntro_Delay;

	protected float m_verticalBeam_AttackHold_AnimSpeed = 1.2f;

	protected float m_verticalBeam_AttackHold_Delay;

	protected float m_verticalBeam_AttackHold_ExitDelay = 0.45f;

	protected float m_verticalBeam_Exit_AnimSpeed = 1.2f;

	protected float m_verticalBeam_Exit_Delay;

	protected float m_verticalBeam_Exit_IdleDuration = 0.1f;

	protected float m_verticalBeam_AttackCD = 12f;

	protected float m_verticalBeam_AttackDuration = 2f;

	protected int m_verticalBeamFireballSpread = 720;

	protected float m_verticalBeamBlackInitialDelay = 1.35f;

	protected float m_verticalBeamBlackLoopDelay = 0.6f;

	protected Projectile_RL m_verticalBeamBlackWarningProjectile;

	protected Projectile_RL m_verticalBeamBlackProjectile;

	protected Projectile_RL m_verticalBeamBoltBlackProjectile;

	private Coroutine m_delayVerticalBeamBlackCoroutine;

	protected const string SPINNING_JUMP_AXE_SPIN_PROJECTILE = "SpellSwordAxeSpinProjectile";

	protected const string SPINNING_JUMP_SWORD_SLASH_PROJECTILE = "FinalBossSwordSlashProjectile";

	protected const string SPINNING_JUMP_LAND_PROJECTILE_MAGMA = "FinalBossMagmaProjectile";

	protected const string SPINNING_JUMP_LAND_PROJECTILE_ROLLING = "FinalBossRollingProjectile";

	protected const int SPINNING_JUMP_SPIN_PROJ_INDEX = 3;

	protected const int SPINNING_JUMP_LAND_SWORD_PROJ_INDEX = 4;

	protected const int SPINNING_JUMP_LAND_CENTRE_PROJ_INDEX = 0;

	protected const int SPINNING_JUMP_LAND_BACK_PROJ_INDEX = 5;

	protected const int SPINNING_JUMP_LAND_FRONT_PROJ_INDEX = 6;

	protected float m_jump_TellIntro_AnimSpeed = 1.2f;

	protected float m_jump_TellIntroAndHold_Delay = 0.65f;

	protected float m_spinningJump_TellIntro_AnimSpeed = 1.5f;

	protected float m_spinningJump_TellHold_AnimSpeed = 1.5f;

	protected float m_spinningJump_TellIntroAndHold_Delay = 0.85f;

	protected float m_spinningJump_AttackIntro_AnimSpeed = 1.5f;

	protected float m_spinningJump_AttackIntro_Delay;

	protected float m_spinningJump_AttackHold_AnimSpeed = 1.5f;

	protected float m_spinningJump_AttackHold_Delay;

	protected float m_spinningJump_AttackHold_ExitDelay;

	protected float m_spinningJump_ExitHold_Delay = 0.375f;

	protected float m_spinningJump_ExitHold_AnimSpeed = 1.2f;

	protected float m_spinningJump_Exit_AnimSpeed = 1.2f;

	protected float m_spinningJump_Exit_Delay;

	protected float m_spinningJump_Exit_IdleDuration = 0.1f;

	protected float m_spinningJump_AttackCD = 12f;

	protected Vector2 m_spinningJump_Land_ThrowAngle = new Vector2(85f, 85f);

	protected Vector2 m_spinningJump_Land_ThrowAngle2 = new Vector2(76f, 76f);

	protected Vector2 m_spinningJump_Land_ThrowAngle3 = new Vector2(71f, 71f);

	protected Vector2 m_spinningJump_Land_ThrowAngle4 = new Vector2(55f, 55f);

	protected Vector2 m_spinningJump_Land_ThrowAngle5 = new Vector2(82f, 82f);

	protected Vector2 m_spinningJump_Land_ThrowAngle6 = new Vector2(74f, 74f);

	protected Vector2 m_spinningJump_Land_ThrowPower = new Vector2(1f, 1f);

	protected float m_spinningJump_Land_ThrowPowerHighMod = 1.2f;

	protected float m_spinningJump_Land_ThrowPowerHighModAdvanced = 1.4f;

	protected float m_spinningJump_Land_RollingPowerMod = 0.3f;

	protected float m_spinningJump_Land_RollingPowerModAdvanced = 0.15f;

	protected bool m_spinningJump_Spawn_AxeSpin;

	protected Projectile_RL m_spinningJumpProjectile;

	protected Vector2 m_spinningJump_AttackVelocity = new Vector2(11f, 36f);

	protected const string SWORD_PROJECTILE = "SpellSwordSlashDownProjectile";

	protected const string SWORD_BONE_BIG_PROJECTILE = "FinalBossBoneBigProjectile";

	protected const string SWORD_BONE_BIG_HORIZONTAL_PROJECTILE = "FinalBossBoneBigHorizontalProjectile";

	protected const string SWORD_BONE_BIG_DIAGONAL_PROJECTILE = "FinalBossBoneBigDiagonalProjectile";

	protected const string SWORD_BONE_SMALL_PROJECTILE = "FinalBossBoneSmallProjectile";

	protected const string SWORD_WRAPPED_BOUNCE_SMALL_PROJECTILE = "FinalBossWrappedBounceSmallProjectile";

	protected const string SWORD_WRAPPED_VOID_SMALL_PROJECTILE = "FinalBossWrappedVoidSmallProjectile";

	protected const string SWORD_WRAPPED_BOUNCE_BIG_PROJECTILE = "FinalBossWrappedBounceBigProjectile";

	protected const string SWORD_WRAPPED_VOID_BIG_PROJECTILE = "FinalBossWrappedVoidBigProjectile";

	protected const string SWORD_POTION_PROJECTILE = "FinalBossPotionProjectile";

	protected const string SWORD_POTION_EXPLOSION_PROJECTILE = "FinalBossPotionExplosionProjectile";

	protected const int SWORD_WRAPPED_POS_INDEX = 15;

	protected const int SWORD_WRAPPED_POS_INDEX2 = 16;

	protected const int SWORD_BIG_ROTATING_POS_INDEX = 7;

	protected const int SWORD_SMALL_ROTATING_POS_INDEX = 8;

	protected const int SWORD_SWING_POS_INDEX = 17;

	protected float m_sword_AttackSpeed = 16f;

	protected float m_sword_AttackDuration = 0.185f;

	protected float m_sword_TellIntro_AnimSpeed = 1.2f;

	protected float m_sword_TellHold_AnimSpeed = 1.2f;

	protected float m_sword_TellIntroAndHold_Delay = 0.95f;

	protected float m_sword_AttackIntro_AnimSpeed = 1.2f;

	protected float m_sword_AttackIntro_Delay;

	protected float m_sword_AttackHold_AnimSpeed = 1.2f;

	protected float m_sword_AttackHold_Delay;

	protected float m_sword_Exit_AnimSpeed = 0.65f;

	protected float m_sword_Exit_Delay;

	protected float m_sword_Exit_IdleDuration = 0.1f;

	protected float m_sword_AttackCD = 12f;

	protected float m_sword_BlackProjectile_Delay = 0.65f;

	private Coroutine m_swordAttackBlackSecondProjectileCoroutine;

	protected const string PORTAL_PROJECTILE_LINE = "FinalBossPortalLineProjectile";

	protected const string PORTAL_PROJECTILE_BOLT = "FinalBossPortalBoltProjectile";

	protected const string PORTAL_PROJECTILE_EXPLOSION = "FinalBossPortalExplosionProjectile";

	protected const string PORTAL_WARNING_PROJECTILE = "FinalBossPortalWarningProjectile";

	protected const string PORTAL_WARNING_INVERSE_PROJECTILE = "FinalBossPortalInverseWarningProjectile";

	protected float m_portal_AttackSpeed = 16f;

	protected float m_portal_AttackDuration = 0.185f;

	protected float m_portal_TellIntro_AnimSpeed = 1.2f;

	protected float m_portal_TellHold_AnimSpeed = 1.2f;

	protected float m_portal_TellIntroAndHold_Delay = 1.25f;

	protected float m_portal_AttackIntro_AnimSpeed = 1.2f;

	protected float m_portal_AttackIntro_Delay;

	protected float m_portal_AttackHold_AnimSpeed = 1.2f;

	protected float m_portal_AttackHold_Delay;

	protected float m_portal_Exit_AnimSpeed = 0.65f;

	protected float m_portal_Exit_Delay;

	protected float m_portal_Exit_IdleDuration = 0.1f;

	protected float m_portal_AttackCD = 12f;

	protected Projectile_RL m_portalWarningProj;

	protected Projectile_RL m_portalProjectile;

	protected Coroutine m_portalWhiteAttackPersistentCoroutine;

	protected const string DEATH_INTRO = "Death_Intro";

	protected const string DEATH_HOLD = "Death";

	protected float m_death_Intro_AnimSpeed = 1f;

	protected float m_death_Intro_Delay;

	protected float m_death_Hold_AnimSpeed = 1f;

	protected float m_death_Hold_Delay = 5.5f;

	protected const string SPAWN_INTRO = "Transform";

	public IRelayLink<bool, float> ColourShiftRelay => m_colourShiftRelay.link;

	public bool IsInWhiteMode => m_isInWhiteMode;

	protected virtual bool m_isPrime_Version => false;

	protected virtual float m_Prime_Version_Final_Mode_Delay_Force_Idle_Add => 1.35f;

	protected override Vector2 IdleDuration => new Vector2(0.25f, 0.75f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.75f, 1.15f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.75f, 1.15f);

	protected override float WalkAnimSpeedMod => 1f;

	protected string SUMMON_TREE_TELL_INTRO
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "PrayBlack_Tell_Intro";
			}
			return "PrayWhite_Tell_Intro";
		}
	}

	protected string SUMMON_TREE_TELL_HOLD
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "PrayBlack_Tell_Hold";
			}
			return "PrayWhite_Tell_Hold";
		}
	}

	protected string SUMMON_TREE_ATTACK_INTRO
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "PrayBlack_Attack_Intro";
			}
			return "PrayWhite_Attack_Intro";
		}
	}

	protected string SUMMON_TREE_ATTACK_HOLD
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "PrayBlack_Attack_Hold";
			}
			return "PrayWhite_Attack_Hold";
		}
	}

	protected string SUMMON_TREE_EXIT
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "PrayBlack_Exit";
			}
			return "PrayWhite_Exit ";
		}
	}

	protected string PRAYER_TELL_INTRO
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "PrayBlack_Tell_Intro";
			}
			return "PrayWhite_Tell_Intro";
		}
	}

	protected string PRAYER_TELL_HOLD
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "PrayBlack_Tell_Hold";
			}
			return "PrayWhite_Tell_Hold";
		}
	}

	protected string PRAYER_ATTACK_INTRO
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "PrayBlack_Attack_Intro";
			}
			return "PrayWhite_Attack_Intro";
		}
	}

	protected string PRAYER_ATTACK_HOLD
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "PrayBlack_Attack_Hold";
			}
			return "PrayWhite_Attack_Hold";
		}
	}

	protected string PRAYER_EXIT
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "PrayBlack_Exit";
			}
			return "PrayWhite_Exit";
		}
	}

	protected string VERTICAL_BEAM_TELL_INTRO
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "RaiseWeaponBlack_Tell_Intro";
			}
			return "RaiseWeaponWhite_Tell_Intro";
		}
	}

	protected string VERTICAL_BEAM_TELL_HOLD
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "RaiseWeaponBlack_Tell_Hold";
			}
			return "RaiseWeaponWhite_Tell_Hold";
		}
	}

	protected string VERTICAL_BEAM_ATTACK_INTRO
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "RaiseWeaponBlack_Attack_Intro";
			}
			return "RaiseWeaponWhite_Attack_Intro";
		}
	}

	protected string VERTICAL_BEAM_ATTACK_HOLD
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "RaiseWeaponBlack_Attack_Hold";
			}
			return "RaiseWeaponWhite_Attack_Hold";
		}
	}

	protected string VERTICAL_BEAM_EXIT
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "RaiseWeaponBlack_Exit";
			}
			return "RaiseWeaponWhite_Exit";
		}
	}

	protected virtual int m_numVerticalBeamProjectiles => 20;

	protected virtual float m_numVerticalBeamAdvancedFollowUpDelay => 0.35f;

	protected virtual int m_numVerticalBeamAdvancedFollowUpProjectiles => 5;

	protected string SPINNING_JUMP_TELL_INTRO
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "JumpAttackBlack_Tell_Intro";
			}
			return "JumpAttackWhite_Tell_Intro";
		}
	}

	protected string SPINNING_JUMP_TELL_HOLD
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "JumpAttackBlack_Tell_Hold";
			}
			return "JumpAttackWhite_Tell_Hold";
		}
	}

	protected string SPINNING_JUMP_ATTACK_INTRO
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "JumpAttackBlack_Attack_Intro";
			}
			return "JumpAttackWhite_Attack_Intro";
		}
	}

	protected string SPINNING_JUMP_ATTACK_HOLD
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "JumpAttackBlack_Attack_Hold";
			}
			return "JumpAttackWhite_Attack_Hold";
		}
	}

	protected string SPINNING_JUMP_LAND
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "JumpAttackBlack_Land";
			}
			return "JumpAttackWhite_Land";
		}
	}

	protected string SPINNING_JUMP_EXIT
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "JumpAttackBlack_Exit";
			}
			return "JumpAttackWhite_Exit";
		}
	}

	protected string SWORD_TELL_INTRO
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "SwordAttackBlack_Tell_Intro";
			}
			return "SwordAttackWhite_Tell_Intro";
		}
	}

	protected string SWORD_TELL_HOLD
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "SwordAttackBlack_Tell_Hold";
			}
			return "SwordAttackWhite_Tell_Hold";
		}
	}

	protected string SWORD_ATTACK_INTRO
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "SwordAttackBlack_Attack_Intro";
			}
			return "SwordAttackWhite_Attack_Intro";
		}
	}

	protected string SWORD_ATTACK_HOLD
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "SwordAttackBlack_Attack_Hold";
			}
			return "SwordAttackWhite_Attack_Hold";
		}
	}

	protected string SWORD_EXIT
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "SwordAttackBlack_Exit";
			}
			return "SwordAttackWhite_Exit";
		}
	}

	protected string PORTAL_TELL_INTRO
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "PortalAttackBlack_Tell_Intro";
			}
			return "PortalAttackWhite_Tell_Intro";
		}
	}

	protected string PORTAL_TELL_HOLD
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "PortalAttackBlack_Tell_Hold";
			}
			return "PortalAttackWhite_Tell_Hold";
		}
	}

	protected string PORTAL_ATTACK_INTRO
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "PortalAttackBlack_Attack_Intro";
			}
			return "PortalAttackWhite_Attack_Intro";
		}
	}

	protected string PORTAL_ATTACK_HOLD
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "PortalAttackBlack_Attack_Hold";
			}
			return "PortalAttackWhite_Attack_Hold";
		}
	}

	protected string PORTAL_EXIT
	{
		get
		{
			if (!m_isInWhiteMode)
			{
				return "PortalAttackBlack_Exit";
			}
			return "PortalAttackWhite_Exit";
		}
	}

	protected virtual int Portal_Outward_Explosion_Amount => 12;

	protected virtual int Portal_Outward_Explosion_Angle => 30;

	protected virtual int Portal_Explosion_Bolt_Amount => 15;

	protected virtual int Portal_Explosion_Bolt_Angle => 24;

	protected virtual float Portal_Explosion_Bolt_Delay => 0.5f;

	protected virtual float Portal_Advanced_Black_To_White_FollowUp_Delay => 1.55f;

	protected virtual float Portal_Advanced_White_To_Black_FollowUp_Delay => 1.55f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[39]
		{
			"FinalBossWhiteTreeProjectile", "FinalBossBlackTreeProjectile", "FinalBossTreeExplosionProjectile", "FinalBossTreeWarningProjectile", "FinalBossWhiteTreeBoltProjectile", "FinalBossBlackTreeBoltProjectile", "FinalBossWhiteVerticalProjectile", "FinalBossVerticalCurseProjectile", "FinalBossVerticalCurseBlueProjectile", "FinalBossBlackVerticalProjectile",
			"FinalBossBlackVerticalWarningProjectile", "FinalBossBounceOrbVerticalProjectile", "FinalBossVerticalVoidSmallProjectile", "FinalBossBlackVerticalBoltProjectile", "SpellSwordAxeSpinProjectile", "FinalBossSwordSlashProjectile", "FinalBossMagmaProjectile", "FinalBossRollingProjectile", "SpellSwordSlashDownProjectile", "FinalBossBoneBigProjectile",
			"FinalBossBoneBigHorizontalProjectile", "FinalBossBoneBigDiagonalProjectile", "FinalBossBoneSmallProjectile", "FinalBossWrappedBounceSmallProjectile", "FinalBossWrappedVoidSmallProjectile", "FinalBossWrappedBounceBigProjectile", "FinalBossWrappedVoidBigProjectile", "FinalBossPotionProjectile", "FinalBossPotionExplosionProjectile", "FinalBossPortalLineProjectile",
			"FinalBossPortalBoltProjectile", "FinalBossPortalExplosionProjectile", "FinalBossPortalWarningProjectile", "FinalBossPortalInverseWarningProjectile", "FinalBossPrayerVoidProjectile", "FinalBossPrayerBounceProjectile", MODESHIFT_BOLT_PROJECTILE, MODESHIFT_SHOUT_PROJECTILE, MODESHIFT_SHOUT_WARNING_PROJECTILE
		};
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		base.EnemyController.HealthChangeRelay.AddListener(OnBossHit);
		m_colourShiftController = enemyController.GetComponent<FinalBossColourShiftController>();
		HitboxController hitboxController = base.EnemyController.HitboxController as HitboxController;
		GameObject[] weaponHitboxList = hitboxController.WeaponHitboxList;
		foreach (GameObject gameObject in weaponHitboxList)
		{
			if (gameObject.name.Contains("Jumping"))
			{
				m_jumpingWeaponHitbox = gameObject;
			}
			else
			{
				m_regularWeaponHitbox = gameObject;
			}
		}
		weaponHitboxList = hitboxController.BodyHitboxList;
		foreach (GameObject gameObject2 in weaponHitboxList)
		{
			if (gameObject2.name.Contains("Jumping"))
			{
				m_jumpingBodyHitbox = gameObject2;
			}
			else if (gameObject2.name.Contains("Praying"))
			{
				m_prayingBodyHitbox = gameObject2;
			}
			else
			{
				m_regularBodyHitbox = gameObject2;
			}
		}
		m_regularWeaponHitbox.SetActive(value: true);
		m_regularBodyHitbox.SetActive(value: true);
		m_jumpingWeaponHitbox.SetActive(value: false);
		m_jumpingBodyHitbox.SetActive(value: false);
		m_prayingBodyHitbox.SetActive(value: false);
		base.LogicController.DisableLogicActivationByDistance = true;
		m_model = enemyController.gameObject.FindObjectReference("Model");
		m_statue = enemyController.gameObject.FindObjectReference("Statue");
		m_statue.gameObject.SetActive(value: false);
		m_isInWhiteMode = true;
		if (!m_rumbleEventInstance.isValid())
		{
			m_rumbleEventInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/Cain/sfx_cain_death_finalGlow_start_rumble_loop", base.EnemyController.transform);
		}
		if (!m_collapseSpiritsEventInstance.isValid())
		{
			m_collapseSpiritsEventInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/Cain/sfx_cain_death_collapse_spirits_loop", base.EnemyController.transform);
		}
		if (!m_verticalBeamBlackEventInstance.isValid())
		{
			m_verticalBeamBlackEventInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/Cain/sfx_cain_dark_staff_attack_lightning_loop", base.EnemyController.transform);
		}
		if (!m_verticalBeamBlackFloorLoopEventInstance.isValid())
		{
			m_verticalBeamBlackFloorLoopEventInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/Cain/sfx_cain_dark_staff_attack_floor_loop", base.EnemyController.transform);
		}
	}

	private void OnBossHit(object sender, HealthChangeEventArgs args)
	{
		if (!m_isModeShifting && m_modeShiftIndex < MODESHIFT_ARRAY.Length && !base.EnemyController.IsDead && !(args.PrevHealthValue <= args.NewHealthValue))
		{
			float num = MODESHIFT_ARRAY[m_modeShiftIndex] * (float)base.EnemyController.ActualMaxHealth;
			if (base.EnemyController.CurrentHealth <= num)
			{
				m_modeShiftIndex++;
				base.LogicController.StopAllLogic(resetCurrentState: true);
				base.LogicController.ForceExecuteLogicBlockName_OnceOnly = "Mode_Shift";
			}
		}
	}

	public override void ResetScript()
	{
		m_isInWhiteMode = true;
		m_isModeShifting = false;
		m_modeShiftIndex = 0;
		m_currentColourShiftCount = 0;
		m_requiredColourShiftCount = 0;
		if (!GameManager.IsApplicationClosing)
		{
			ColourShift(shiftToWhite: true, 0f, runEvents: false);
		}
		base.EnemyController.StatusEffectController.ImmuneToAllStatusEffects = false;
		StopTreePersistentCoroutine();
		StopPortalWhitePersistentCoroutine();
		this.StopPersistentCoroutine(m_swordAttackBlackSecondProjectileCoroutine);
		if ((bool)m_model)
		{
			m_model.SetActive(value: true);
		}
		if ((bool)m_statue)
		{
			m_statue.SetActive(value: false);
		}
		base.ResetScript();
	}

	public override void OnLBCompleteOrCancelled()
	{
		base.OnLBCompleteOrCancelled();
		StopProjectile(ref m_spinningJumpProjectile);
		StopProjectile(ref m_portalWarningProj);
		StopProjectile(ref m_summonTreeWarningProjectile);
		StopProjectile(ref m_verticalBeamBlackWarningProjectile);
		StopProjectile(ref m_verticalBeamBlackProjectile);
		StopProjectile(ref m_verticalBeamBoltBlackProjectile);
		StopProjectile(ref m_modeShiftShoutWarningProj);
		StopProjectile(ref m_modeShiftShoutProj);
		if (m_modeShiftProjectileCoroutine != null)
		{
			StopCoroutine(m_modeShiftProjectileCoroutine);
		}
		if (m_delayVerticalBeamBlackCoroutine != null)
		{
			StopCoroutine(m_delayVerticalBeamBlackCoroutine);
		}
		m_delayVerticalBeamBlackCoroutine = null;
		if (m_verticalBeamBlackEventInstance.isValid())
		{
			AudioManager.Stop(m_verticalBeamBlackEventInstance);
		}
		if (m_verticalBeamBlackFloorLoopEventInstance.isValid())
		{
			AudioManager.Stop(m_verticalBeamBlackFloorLoopEventInstance);
		}
		m_regularWeaponHitbox.SetActive(value: true);
		m_regularBodyHitbox.SetActive(value: true);
		m_jumpingWeaponHitbox.SetActive(value: false);
		m_jumpingBodyHitbox.SetActive(value: false);
		m_prayingBodyHitbox.SetActive(value: false);
	}

	private void PerformColourShiftCheck()
	{
		if (m_modeShiftIndex >= MODESHIFT_ARRAY.Length)
		{
			m_currentColourShiftCount++;
			if (m_currentColourShiftCount >= m_requiredColourShiftCount)
			{
				m_currentColourShiftCount = 0;
				m_requiredColourShiftCount = Random.Range(COLOURSHIFT_MINMAX_COUNT.x, COLOURSHIFT_MINMAX_COUNT.y + 1);
				ColourShift(!m_isInWhiteMode, 0.1f, runEvents: true);
			}
		}
	}

	private void PerformForcedColourShift()
	{
	}

	private void PerformColourShiftCheckRandomChance()
	{
		if (m_modeShiftIndex >= MODESHIFT_ARRAY.Length && CDGHelper.RandomPlusMinus() > 0)
		{
			m_currentColourShiftCount++;
			if (m_currentColourShiftCount >= m_requiredColourShiftCount)
			{
				m_currentColourShiftCount = 0;
				m_requiredColourShiftCount = Random.Range(COLOURSHIFT_MINMAX_COUNT.x, COLOURSHIFT_MINMAX_COUNT.y + 1);
				ColourShift(!m_isInWhiteMode, 0.1f, runEvents: true);
			}
		}
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[RestLogic]
	[WanderLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Mode_Shift()
	{
		m_isModeShifting = true;
		this.ToDo("Mode Shift");
		RemoveStatusEffects(includeCommanderBuffs: false);
		base.EnemyController.StatusEffectController.ImmuneToAllStatusEffects = true;
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		SetVelocityX(0f, additive: false);
		base.EnemyController.ModeshiftDamageMod = m_modeShift_Damage_Mod;
		yield return base.DeathAnim();
		yield return Default_Animation("ModeShift_Intro", m_modeShift_Downed_AnimSpeed, m_modeShift_Downed_Delay);
		yield return Default_Animation("ModeShift_GetUp", m_modeShift_GetUp_AnimSpeed, 0f);
		m_modeShiftShoutWarningProj = this.FireProjectile(MODESHIFT_SHOUT_WARNING_PROJECTILE, MODESHIFT_SHOUT_POSITION_INDEX, matchFacing: false);
		yield return Default_TellIntroAndLoop("ModeShift_Scream_Intro", m_modeShift_TellIntro_AnimSpeed, "ModeShift_Tell_Loop", m_modeShift_TellHold_AnimSpeed, m_modeShift_TellIntroAndHold_Delay);
		SetAttackingWithContactDamage(enable: true);
		yield return Default_Animation("ModeShift_Scream_Hold", m_modeShift_AttackIntro_AnimSpeed, 0f, waitUntilAnimationComplete: false);
		ColourShift(!m_isInWhiteMode, 0.1f, runEvents: true);
		StopProjectile(ref m_modeShiftShoutWarningProj);
		m_modeShiftShoutProj = this.FireProjectile(MODESHIFT_SHOUT_PROJECTILE, MODESHIFT_SHOUT_POSITION_INDEX, matchFacing: false);
		m_modeShiftProjectileCoroutine = StartCoroutine(ModeShiftFireProjectilesCoroutine());
		yield return Default_Animation("ModeShift_Action_Loop", m_modeShift_AttackHold_AnimSpeed, m_modeShift_AttackHold_Delay);
		SetAttackingWithContactDamage(enable: false);
		if (m_modeShiftProjectileCoroutine != null)
		{
			StopCoroutine(m_modeShiftProjectileCoroutine);
		}
		StopProjectile(ref m_modeShiftShoutProj);
		yield return Default_Animation("ModeShift_Scream_Exit", m_modeShift_Exit_AnimSpeed, m_modeShift_Exit_Delay);
		base.EnemyController.ModeshiftDamageMod = 1f;
		base.EnemyController.ResetBaseValues();
		base.EnemyController.StatusEffectController.ImmuneToAllStatusEffects = false;
		yield return Default_Attack_Cooldown(m_modeShift_Exit_IdleDuration, m_modeShift_AttackCD);
		base.EnemyController.LockFlip = false;
		m_isModeShifting = false;
	}

	private IEnumerator ModeShiftFireProjectilesCoroutine()
	{
		for (int i = 0; i < m_modeShift_Loops; i++)
		{
			float speedMod = Random.Range(m_modeShift_ThrowPower.x, m_modeShift_ThrowPower.y);
			int num = Random.Range((int)m_modeShift_ThrowAngle.x, (int)m_modeShift_ThrowAngle.y);
			float speedMod2 = Random.Range(m_modeShift_ThrowPower.x, m_modeShift_ThrowPower.y);
			int num2 = Random.Range((int)m_modeShift_ThrowAngle.x, (int)m_modeShift_ThrowAngle.y);
			this.FireProjectile(MODESHIFT_BOLT_PROJECTILE, MODESHIFT_POSITION_INDEX, matchFacing: false, num, speedMod);
			this.FireProjectile(MODESHIFT_BOLT_PROJECTILE, MODESHIFT_POSITION_INDEX, matchFacing: false, 180 - num2, speedMod2);
			float delay = m_modeShift_LoopDelay + Time.time;
			while (Time.time < delay)
			{
				yield return null;
			}
		}
		m_modeShiftProjectileCoroutine = null;
	}

	private void ColourShift(bool shiftToWhite, float lerpSpeed, bool runEvents)
	{
		this.ToDo("Colour Shift");
		m_colourShiftController.ShiftColour(shiftToWhite, lerpSpeed, base.EnemyController.EnemyRank > EnemyRank.Basic);
		if (runEvents)
		{
			if (shiftToWhite)
			{
				EffectManager.PlayEffect(base.EnemyController.gameObject, base.EnemyController.Animator, "FinalBoss_DarkToLight_Effect", base.EnemyController.Midpoint, 0f, EffectStopType.Gracefully).transform.SetParent(base.EnemyController.transform, worldPositionStays: true);
			}
			else
			{
				EffectManager.PlayEffect(base.EnemyController.gameObject, base.EnemyController.Animator, "FinalBoss_LightToDark_Effect", base.EnemyController.Midpoint, 0f, EffectStopType.Gracefully).transform.SetParent(base.EnemyController.transform, worldPositionStays: true);
			}
			m_colourShiftRelay.Dispatch(shiftToWhite, lerpSpeed);
		}
		m_isInWhiteMode = shiftToWhite;
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator SummonTree()
	{
		if (!m_summonTreeProjectile || !m_summonTreeProjectile.isActiveAndEnabled || m_isInWhiteMode != m_summonedWhiteTree)
		{
			this.ToDo("Summon Tree");
			SetVelocityX(0f, additive: false);
			base.EnemyController.LockFlip = true;
			yield return Default_TellIntroAndLoop(SUMMON_TREE_TELL_INTRO, m_summonTree_TellIntro_AnimSpeed, SUMMON_TREE_TELL_HOLD, m_summonTree_TellHold_AnimSpeed, m_summonTree_TellIntroAndHold_Delay);
			Vector2 summonTreeWarningPos = new Vector2(base.EnemyController.TargetController.Midpoint.x, base.EnemyController.Midpoint.y);
			Vector2 summonTreeSpawnPos = new Vector2(base.EnemyController.TargetController.Midpoint.x, base.EnemyController.transform.position.y);
			m_summonTreeWarningProjectile = this.FireProjectileAbsPos("FinalBossTreeWarningProjectile", summonTreeWarningPos, matchFacing: false);
			if (m_summonTree_Warning_Delay > 0f)
			{
				yield return Wait(m_summonTree_Warning_Delay);
			}
			StopProjectile(ref m_summonTreeWarningProjectile);
			yield return Default_Animation(SUMMON_TREE_ATTACK_INTRO, m_summonTree_AttackIntro_AnimSpeed, m_summonTree_AttackIntro_Delay);
			yield return Default_Animation(SUMMON_TREE_ATTACK_HOLD, m_summonTree_AttackHold_AnimSpeed, m_summonTree_AttackHold_Delay, waitUntilAnimationComplete: false);
			this.FireProjectileAbsPos("FinalBossTreeExplosionProjectile", summonTreeWarningPos, matchFacing: false);
			StopTreePersistentCoroutine();
			m_treePersistentCoroutine = this.RunPersistentCoroutine(TreePersistentCoroutine(m_isInWhiteMode, summonTreeSpawnPos));
			if (m_summonTree_AttackHold_ExitDelay > 0f)
			{
				yield return Wait(m_summonTree_AttackHold_ExitDelay);
			}
			base.EnemyController.LockFlip = false;
			base.EnemyController.ResetBaseValues();
			yield return Default_Attack_Cooldown(m_summonTree_Exit_IdleDuration, m_summonTree_AttackCD);
			PerformColourShiftCheck();
		}
	}

	private IEnumerator TreePersistentCoroutine(bool summonWhiteTree, Vector2 treeSpawnPos)
	{
		m_summonedWhiteTree = summonWhiteTree;
		StopProjectile(ref m_summonTreeProjectile);
		string projectileName = "FinalBossWhiteTreeProjectile";
		if (!summonWhiteTree)
		{
			projectileName = "FinalBossBlackTreeProjectile";
		}
		m_summonTreeProjectile = this.FireProjectileAbsPos(projectileName, treeSpawnPos, matchFacing: false);
		yield return null;
		CameraLayerController component = m_summonTreeProjectile.GetComponent<CameraLayerController>();
		if (component.CameraLayer != CameraLayer.Background_ORTHO)
		{
			component.SetCameraLayer(CameraLayer.Background_ORTHO);
		}
		BlinkPulseEffect blinkPulseEffect = m_summonTreeProjectile.GetComponent<BlinkPulseEffect>();
		Vector2 treeMidPos = m_summonTreeProjectile.transform.position;
		Vector2 treeLeftPos = m_summonTreeProjectile.transform.position;
		Vector2 treeRightPos = m_summonTreeProjectile.transform.position;
		float num = 5.5f;
		float num2 = 10f;
		treeMidPos.y += num;
		treeLeftPos.y += num;
		treeLeftPos.x += num2;
		treeRightPos.y += num;
		treeRightPos.x += 0f - num2;
		float fireDelay3 = m_treeProjectile_InitialDelay + Time.time;
		blinkPulseEffect.StartInvincibilityEffect(m_treeProjectile_InitialDelay);
		while (Time.time < fireDelay3)
		{
			yield return null;
		}
		float blinkDelay;
		if (summonWhiteTree)
		{
			while ((bool)m_summonTreeProjectile && m_summonTreeProjectile.isActiveAndEnabled)
			{
				this.FireProjectileAbsPos("FinalBossWhiteTreeBoltProjectile", treeLeftPos, matchFacing: false);
				this.FireProjectileAbsPos("FinalBossWhiteTreeBoltProjectile", treeRightPos, matchFacing: false, 180f);
				fireDelay3 = m_whiteTreeProjectile_Delay - 1f + Time.time;
				while (Time.time < fireDelay3)
				{
					yield return null;
				}
				blinkPulseEffect.StartInvincibilityEffect(1f);
				blinkDelay = 1f + Time.time;
				while (Time.time < blinkDelay)
				{
					yield return null;
				}
			}
			yield break;
		}
		blinkDelay = 360f / (float)m_numTreeVoidProjectiles;
		while (m_summonTreeProjectile.isActiveAndEnabled)
		{
			for (int i = 0; i < m_numTreeVoidProjectiles; i++)
			{
				this.FireProjectileAbsPos("FinalBossBlackTreeBoltProjectile", treeMidPos, matchFacing: false, (float)i * blinkDelay);
				this.FireProjectileAbsPos("FinalBossBlackTreeBoltProjectile", treeMidPos, matchFacing: false, (float)i * blinkDelay + blinkDelay / 2f, m_treeVoidSpeedMod);
			}
			fireDelay3 = m_blackTreeProjectile_Delay + Time.time;
			while (Time.time < fireDelay3)
			{
				yield return null;
			}
		}
	}

	private void StopTreePersistentCoroutine()
	{
		this.StopPersistentCoroutine(m_treePersistentCoroutine);
		StopProjectile(ref m_summonTreeProjectile);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Prayer()
	{
		SetVelocityX(0f, additive: false);
		base.EnemyController.LockFlip = true;
		m_regularWeaponHitbox.SetActive(value: true);
		m_regularBodyHitbox.SetActive(value: false);
		m_jumpingWeaponHitbox.SetActive(value: false);
		m_jumpingBodyHitbox.SetActive(value: false);
		m_prayingBodyHitbox.SetActive(value: true);
		yield return Default_TellIntroAndLoop(PRAYER_TELL_INTRO, m_prayer_TellIntro_AnimSpeed, PRAYER_TELL_HOLD, m_prayer_TellHold_AnimSpeed, m_prayer_TellIntroAndHold_Delay);
		yield return Default_Animation(PRAYER_ATTACK_INTRO, m_prayer_AttackIntro_AnimSpeed, m_prayer_AttackIntro_Delay);
		yield return Default_Animation(PRAYER_ATTACK_HOLD, m_prayer_AttackHold_AnimSpeed, m_prayer_AttackHold_Delay, waitUntilAnimationComplete: false);
		if (m_isInWhiteMode)
		{
			if (m_isPrime_Version && m_modeShiftIndex >= MODESHIFT_ARRAY.Length)
			{
				PerformForcedColourShift();
				if (m_prayer_BounceAdvancedVoidInitialDelay > 0f)
				{
					yield return Wait(m_prayer_BounceAdvancedVoidInitialDelay);
				}
				this.FireProjectile("FinalBossPrayerVoidProjectile", 0, matchFacing: false);
				Projectile_RL projectile_RL = this.FireProjectile("FinalBossPrayerVoidProjectile", 0, matchFacing: false, 180f);
				projectile_RL.transform.SetLocalScaleX(0f - projectile_RL.transform.localScale.x);
			}
			AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Cain/sfx_cain_light_prayer_launch_firstWave", base.EnemyController.Midpoint);
			this.FireProjectile("FinalBossPrayerBounceProjectile", 9, matchFacing: false);
			this.FireProjectile("FinalBossPrayerBounceProjectile", 9, matchFacing: false, 180f);
			this.FireProjectile("FinalBossPrayerBounceProjectile", 10, matchFacing: false);
			this.FireProjectile("FinalBossPrayerBounceProjectile", 10, matchFacing: false, 180f);
			this.FireProjectile("FinalBossPrayerBounceProjectile", 11, matchFacing: false);
			this.FireProjectile("FinalBossPrayerBounceProjectile", 11, matchFacing: false, 180f);
			this.FireProjectile("FinalBossPrayerBounceProjectile", 12, matchFacing: false);
			this.FireProjectile("FinalBossPrayerBounceProjectile", 12, matchFacing: false, 180f);
			if (m_isPrime_Version)
			{
				if (CDGHelper.RandomPlusMinus() > 0)
				{
					if (m_prayer_BounceLoopDelay > 0f)
					{
						yield return Wait(m_prayer_BounceLoopAdvancedDelay);
					}
					this.FireProjectile("FinalBossPrayerBounceProjectile", 12, matchFacing: false);
					this.FireProjectile("FinalBossPrayerBounceProjectile", 12, matchFacing: false, 180f);
					if (m_prayer_BounceLoopDelay > 0f)
					{
						yield return Wait(m_prayer_BounceLoopAdvancedDelay * 2f);
					}
				}
				else
				{
					if (m_prayer_BounceLoopDelay > 0f)
					{
						yield return Wait(m_prayer_BounceLoopAdvancedDelay * 2f);
					}
					this.FireProjectile("FinalBossPrayerBounceProjectile", 11, matchFacing: false);
					this.FireProjectile("FinalBossPrayerBounceProjectile", 11, matchFacing: false, 180f);
					if (m_prayer_BounceLoopDelay > 0f)
					{
						yield return Wait(m_prayer_BounceLoopAdvancedDelay);
					}
				}
			}
			else if (m_prayer_BounceLoopDelay > 0f)
			{
				yield return Wait(m_prayer_BounceLoopDelay);
			}
			AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Cain/sfx_cain_dark_prayer_launch_secondWave", base.EnemyController.Midpoint);
			this.FireProjectile("FinalBossPrayerBounceProjectile", 9, matchFacing: false);
			this.FireProjectile("FinalBossPrayerBounceProjectile", 13, matchFacing: false);
			this.FireProjectile("FinalBossPrayerBounceProjectile", 13, matchFacing: false, 180f);
			this.FireProjectile("FinalBossPrayerBounceProjectile", 11, matchFacing: false);
			this.FireProjectile("FinalBossPrayerBounceProjectile", 11, matchFacing: false, 180f);
			this.FireProjectile("FinalBossPrayerBounceProjectile", 9, matchFacing: false);
			this.FireProjectile("FinalBossPrayerBounceProjectile", 9, matchFacing: false, 180f);
			this.FireProjectile("FinalBossPrayerBounceProjectile", 12, matchFacing: false);
			this.FireProjectile("FinalBossPrayerBounceProjectile", 12, matchFacing: false, 180f);
			if (m_isPrime_Version && m_modeShiftIndex >= MODESHIFT_ARRAY.Length)
			{
				PerformForcedColourShift();
				if (m_prayer_BounceAdvancedVoidInitialDelay > 0f)
				{
					yield return Wait(m_prayer_BounceAdvancedVoidInitialDelay);
				}
				this.FireProjectile("FinalBossPrayerVoidProjectile", 0, matchFacing: false);
				Projectile_RL projectile_RL2 = this.FireProjectile("FinalBossPrayerVoidProjectile", 0, matchFacing: false, 180f);
				projectile_RL2.transform.SetLocalScaleX(0f - projectile_RL2.transform.localScale.x);
				if (m_prayer_VoidLoopAdvancedFollowUpDelay > 0f)
				{
					yield return Wait(m_prayer_VoidLoopAdvancedFollowUpDelay);
				}
				this.FireProjectile("FinalBossPrayerVoidProjectile", 0, matchFacing: false);
				Projectile_RL projectile_RL3 = this.FireProjectile("FinalBossPrayerVoidProjectile", 0, matchFacing: false, 180f);
				projectile_RL3.transform.SetLocalScaleX(0f - projectile_RL3.transform.localScale.x);
				if (m_prayer_VoidLoopAdvancedFollowUpDelay > 0f)
				{
					yield return Wait(m_prayer_VoidLoopAdvancedFollowUpDelay);
				}
				this.FireProjectile("FinalBossPrayerVoidProjectile", 0, matchFacing: false);
				Projectile_RL projectile_RL4 = this.FireProjectile("FinalBossPrayerVoidProjectile", 0, matchFacing: false, 180f);
				projectile_RL4.transform.SetLocalScaleX(0f - projectile_RL4.transform.localScale.x);
			}
		}
		else
		{
			AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Cain/sfx_cain_dark_prayer_launch_firstWave", base.EnemyController.Midpoint);
			this.FireProjectile("FinalBossPrayerVoidProjectile", 0, matchFacing: false);
			Projectile_RL projectile_RL5 = this.FireProjectile("FinalBossPrayerVoidProjectile", 0, matchFacing: false, 180f);
			projectile_RL5.transform.SetLocalScaleX(0f - projectile_RL5.transform.localScale.x);
			if (m_isPrime_Version)
			{
				if (m_prayer_VoidLoopAdvancedFollowUpDelay > 0f)
				{
					yield return Wait(m_prayer_VoidLoopAdvancedFollowUpDelay);
				}
				this.FireProjectile("FinalBossPrayerVoidProjectile", 0, matchFacing: false);
				Projectile_RL projectile_RL6 = this.FireProjectile("FinalBossPrayerVoidProjectile", 0, matchFacing: false, 180f);
				projectile_RL6.transform.SetLocalScaleX(0f - projectile_RL6.transform.localScale.x);
				if (m_prayer_VoidLoopAdvancedFollowUpDelay > 0f)
				{
					yield return Wait(m_prayer_VoidLoopAdvancedFollowUpDelay);
				}
				this.FireProjectile("FinalBossPrayerVoidProjectile", 0, matchFacing: false);
				Projectile_RL projectile_RL7 = this.FireProjectile("FinalBossPrayerVoidProjectile", 0, matchFacing: false, 180f);
				projectile_RL7.transform.SetLocalScaleX(0f - projectile_RL7.transform.localScale.x);
			}
			if (m_prayer_VoidLoopDelay > 0f)
			{
				yield return Wait(m_prayer_VoidLoopDelay);
			}
			AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Cain/sfx_cain_dark_prayer_launch_secondWave", base.EnemyController.Midpoint);
			this.FireProjectile("FinalBossPrayerVoidProjectile", 0, matchFacing: false);
			Projectile_RL projectile_RL8 = this.FireProjectile("FinalBossPrayerVoidProjectile", 0, matchFacing: false, 180f);
			projectile_RL8.transform.SetLocalScaleX(0f - projectile_RL8.transform.localScale.x);
			if (m_isPrime_Version)
			{
				if (m_prayer_VoidLoopAdvancedFollowUpDelay > 0f)
				{
					yield return Wait(m_prayer_VoidLoopAdvancedFollowUpDelay);
				}
				this.FireProjectile("FinalBossPrayerVoidProjectile", 0, matchFacing: false);
				Projectile_RL projectile_RL9 = this.FireProjectile("FinalBossPrayerVoidProjectile", 0, matchFacing: false, 180f);
				projectile_RL9.transform.SetLocalScaleX(0f - projectile_RL9.transform.localScale.x);
				if (m_prayer_VoidLoopAdvancedFollowUpDelay > 0f)
				{
					yield return Wait(m_prayer_VoidLoopAdvancedFollowUpDelay);
				}
				this.FireProjectile("FinalBossPrayerVoidProjectile", 0, matchFacing: false);
				Projectile_RL projectile_RL10 = this.FireProjectile("FinalBossPrayerVoidProjectile", 0, matchFacing: false, 180f);
				projectile_RL10.transform.SetLocalScaleX(0f - projectile_RL10.transform.localScale.x);
			}
			if (m_isPrime_Version && m_modeShiftIndex >= MODESHIFT_ARRAY.Length)
			{
				PerformForcedColourShift();
				if (m_prayer_VoidLoopAdvancedFollowUpDelay > 0f)
				{
					yield return Wait(m_prayer_VoidLoopAdvancedFollowUpDelay);
				}
				this.FireProjectile("FinalBossPrayerBounceProjectile", 9, matchFacing: false);
				this.FireProjectile("FinalBossPrayerBounceProjectile", 9, matchFacing: false, 180f);
				this.FireProjectile("FinalBossPrayerBounceProjectile", 10, matchFacing: false);
				this.FireProjectile("FinalBossPrayerBounceProjectile", 10, matchFacing: false, 180f);
				this.FireProjectile("FinalBossPrayerBounceProjectile", 11, matchFacing: false);
				this.FireProjectile("FinalBossPrayerBounceProjectile", 11, matchFacing: false, 180f);
				this.FireProjectile("FinalBossPrayerBounceProjectile", 12, matchFacing: false);
				this.FireProjectile("FinalBossPrayerBounceProjectile", 12, matchFacing: false, 180f);
			}
		}
		yield return Default_Animation(PRAYER_EXIT, m_prayer_Exit_AnimSpeed, m_prayer_Exit_Delay);
		m_regularWeaponHitbox.SetActive(value: true);
		m_regularBodyHitbox.SetActive(value: true);
		m_jumpingWeaponHitbox.SetActive(value: false);
		m_jumpingBodyHitbox.SetActive(value: false);
		m_prayingBodyHitbox.SetActive(value: false);
		if (m_prayer_AttackHold_ExitDelay > 0f)
		{
			yield return Wait(m_prayer_AttackHold_ExitDelay);
		}
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetBaseValues();
		if (m_isPrime_Version && m_modeShiftIndex >= MODESHIFT_ARRAY.Length)
		{
			yield return Default_Attack_Cooldown(m_Prime_Version_Final_Mode_Delay_Force_Idle_Add + m_prayer_Exit_IdleDuration, m_prayer_AttackCD);
		}
		else
		{
			yield return Default_Attack_Cooldown(m_prayer_Exit_IdleDuration, m_prayer_AttackCD);
		}
		PerformColourShiftCheck();
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator VerticalBeam()
	{
		this.ToDo("Vertical Beam");
		SetVelocityX(0f, additive: false);
		base.EnemyController.LockFlip = true;
		yield return Default_TellIntroAndLoop(VERTICAL_BEAM_TELL_INTRO, m_verticalBeam_TellIntro_AnimSpeed, VERTICAL_BEAM_TELL_HOLD, m_verticalBeam_TellHold_AnimSpeed, m_verticalBeam_TellIntroAndHold_Delay);
		yield return Default_Animation(VERTICAL_BEAM_ATTACK_INTRO, m_verticalBeam_AttackIntro_AnimSpeed, m_verticalBeam_AttackIntro_Delay);
		yield return Default_Animation(VERTICAL_BEAM_ATTACK_HOLD, m_verticalBeam_AttackHold_AnimSpeed, m_verticalBeam_AttackHold_Delay, waitUntilAnimationComplete: false);
		if (m_isInWhiteMode)
		{
			float fireInterval = m_verticalBeam_AttackDuration / (float)m_numVerticalBeamProjectiles;
			float startingAngle = Random.Range(0, 360);
			int num = CDGHelper.RandomPlusMinus();
			float fireAngleInterval = m_verticalBeamFireballSpread / (m_numVerticalBeamProjectiles - 1) * num;
			for (int i = 0; i < m_numVerticalBeamProjectiles; i++)
			{
				float angle = startingAngle + fireAngleInterval * (float)i;
				this.FireProjectile("FinalBossWhiteVerticalProjectile", 2, matchFacing: false, angle);
				if (m_isPrime_Version && i % 6 == 5)
				{
					float angle2 = Random.Range(0, 360);
					this.FireProjectile("FinalBossVerticalCurseProjectile", 2, matchFacing: false, angle2);
				}
				yield return Wait(fireInterval);
			}
			if (m_isPrime_Version && m_modeShiftIndex >= MODESHIFT_ARRAY.Length)
			{
				PerformForcedColourShift();
				Vector2 projPos = new Vector2(base.EnemyController.Room.BoundsRect.x, base.EnemyController.transform.localPosition.y);
				m_verticalBeamBlackWarningProjectile = this.FireProjectileAbsPos("FinalBossBlackVerticalWarningProjectile", projPos, matchFacing: false);
				if (m_verticalBeamBlackInitialDelay > 0f)
				{
					yield return Wait(m_verticalBeamBlackInitialDelay);
				}
				StopProjectile(ref m_verticalBeamBlackWarningProjectile);
				m_verticalBeamBlackProjectile = this.FireProjectileAbsPos("FinalBossBlackVerticalProjectile", projPos, matchFacing: false);
				float num2 = 1f;
				Vector2 pos = base.EnemyController.transform.position;
				if (base.EnemyController.IsFacingRight)
				{
					pos.x += num2;
				}
				else
				{
					pos.x -= num2;
				}
				m_verticalBeamBoltBlackProjectile = this.FireProjectileAbsPos("FinalBossBlackVerticalBoltProjectile", pos, matchFacing: false);
				Vector3 absoluteSpawnPositionAtIndex = GetAbsoluteSpawnPositionAtIndex(2, matchFacing: false);
				float angle3 = CDGHelper.VectorToAngle(base.EnemyController.TargetController.Midpoint - absoluteSpawnPositionAtIndex);
				if (CDGHelper.RandomPlusMinus() > 0)
				{
					this.FireProjectile("FinalBossBounceOrbVerticalProjectile", 2, matchFacing: false, angle3);
				}
				else
				{
					this.FireProjectile("FinalBossVerticalVoidSmallProjectile", 2, matchFacing: false, angle3);
				}
			}
		}
		else
		{
			Vector2 projPos = new Vector2(base.EnemyController.Room.BoundsRect.x, base.EnemyController.transform.localPosition.y);
			m_verticalBeamBlackWarningProjectile = this.FireProjectileAbsPos("FinalBossBlackVerticalWarningProjectile", projPos, matchFacing: false);
			if (m_delayVerticalBeamBlackCoroutine != null)
			{
				StopCoroutine(m_delayVerticalBeamBlackCoroutine);
			}
			m_delayVerticalBeamBlackCoroutine = StartCoroutine(PlayDelayedVerticalBeamBlackSFX(1f));
			if (m_verticalBeamBlackInitialDelay > 0f)
			{
				yield return Wait(m_verticalBeamBlackInitialDelay);
			}
			StopProjectile(ref m_verticalBeamBlackWarningProjectile);
			m_verticalBeamBlackProjectile = this.FireProjectileAbsPos("FinalBossBlackVerticalProjectile", projPos, matchFacing: false);
			float num3 = 1f;
			Vector2 pos2 = base.EnemyController.transform.position;
			if (base.EnemyController.IsFacingRight)
			{
				pos2.x += num3;
			}
			else
			{
				pos2.x -= num3;
			}
			m_verticalBeamBoltBlackProjectile = this.FireProjectileAbsPos("FinalBossBlackVerticalBoltProjectile", pos2, matchFacing: false);
			Vector3 vertBeamPos = GetAbsoluteSpawnPositionAtIndex(2, matchFacing: false);
			float angle4 = CDGHelper.VectorToAngle(base.EnemyController.TargetController.Midpoint - vertBeamPos);
			this.FireProjectile("FinalBossBounceOrbVerticalProjectile", 2, matchFacing: false, angle4);
			yield return Wait(m_verticalBeamBlackLoopDelay);
			if (m_isPrime_Version)
			{
				if (CDGHelper.RandomPlusMinus() > 0)
				{
					angle4 = CDGHelper.VectorToAngle(base.EnemyController.TargetController.Midpoint - vertBeamPos);
					this.FireProjectile("FinalBossVerticalVoidSmallProjectile", 2, matchFacing: false, angle4);
					yield return Wait(m_verticalBeamBlackLoopDelay);
					angle4 = CDGHelper.VectorToAngle(base.EnemyController.TargetController.Midpoint - vertBeamPos);
					this.FireProjectile("FinalBossBounceOrbVerticalProjectile", 2, matchFacing: false, angle4);
					yield return Wait(m_verticalBeamBlackLoopDelay);
				}
				else
				{
					angle4 = CDGHelper.VectorToAngle(base.EnemyController.TargetController.Midpoint - vertBeamPos);
					this.FireProjectile("FinalBossBounceOrbVerticalProjectile", 2, matchFacing: false, angle4);
					yield return Wait(m_verticalBeamBlackLoopDelay);
					angle4 = CDGHelper.VectorToAngle(base.EnemyController.TargetController.Midpoint - vertBeamPos);
					this.FireProjectile("FinalBossVerticalVoidSmallProjectile", 2, matchFacing: false, angle4);
					yield return Wait(m_verticalBeamBlackLoopDelay);
				}
			}
			else
			{
				angle4 = CDGHelper.VectorToAngle(base.EnemyController.TargetController.Midpoint - vertBeamPos);
				this.FireProjectile("FinalBossBounceOrbVerticalProjectile", 2, matchFacing: false, angle4);
				yield return Wait(m_verticalBeamBlackLoopDelay);
				angle4 = CDGHelper.VectorToAngle(base.EnemyController.TargetController.Midpoint - vertBeamPos);
				this.FireProjectile("FinalBossBounceOrbVerticalProjectile", 2, matchFacing: false, angle4);
				yield return Wait(m_verticalBeamBlackLoopDelay);
			}
			if (m_isPrime_Version && m_modeShiftIndex >= MODESHIFT_ARRAY.Length)
			{
				PerformForcedColourShift();
				yield return Wait(m_numVerticalBeamAdvancedFollowUpDelay);
				this.FireProjectile("FinalBossWhiteVerticalProjectile", 2, matchFacing: false, 65f);
				this.FireProjectile("FinalBossWhiteVerticalProjectile", 2, matchFacing: false, 130f);
				this.FireProjectile("FinalBossWhiteVerticalProjectile", 2, matchFacing: false, 195f);
				this.FireProjectile("FinalBossWhiteVerticalProjectile", 2, matchFacing: false, 260f);
				this.FireProjectile("FinalBossWhiteVerticalProjectile", 2, matchFacing: false, 325f);
			}
		}
		if (m_verticalBeam_AttackHold_ExitDelay > 0f)
		{
			yield return Wait(m_verticalBeam_AttackHold_ExitDelay);
		}
		StopProjectile(ref m_verticalBeamBlackProjectile);
		StopProjectile(ref m_verticalBeamBoltBlackProjectile);
		if (!m_isInWhiteMode)
		{
			if (m_verticalBeamBlackEventInstance.isValid())
			{
				AudioManager.Stop(m_verticalBeamBlackEventInstance);
			}
			if (m_verticalBeamBlackFloorLoopEventInstance.isValid())
			{
				AudioManager.Stop(m_verticalBeamBlackFloorLoopEventInstance);
			}
			AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Cain/sfx_cain_dark_staff_attack_lightning_end", base.EnemyController.Midpoint);
		}
		yield return Default_Animation(VERTICAL_BEAM_EXIT, m_verticalBeam_Exit_AnimSpeed, m_verticalBeam_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetBaseValues();
		if (m_isPrime_Version && m_modeShiftIndex >= MODESHIFT_ARRAY.Length)
		{
			yield return Default_Attack_Cooldown(m_Prime_Version_Final_Mode_Delay_Force_Idle_Add + m_prayer_Exit_IdleDuration, m_verticalBeam_AttackCD);
		}
		else
		{
			yield return Default_Attack_Cooldown(m_verticalBeam_Exit_IdleDuration, m_verticalBeam_AttackCD);
		}
		PerformColourShiftCheck();
	}

	private IEnumerator PlayDelayedVerticalBeamBlackSFX(float delay)
	{
		delay += Time.time;
		while (Time.time < delay)
		{
			yield return null;
		}
		if (m_verticalBeamBlackEventInstance.isValid())
		{
			AudioManager.Play(null, m_verticalBeamBlackEventInstance);
		}
		if (m_verticalBeamBlackFloorLoopEventInstance.isValid())
		{
			AudioManager.Play(null, m_verticalBeamBlackFloorLoopEventInstance);
		}
		m_delayVerticalBeamBlackCoroutine = null;
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator SpinningJump()
	{
		this.ToDo("Spinning Jump Attack");
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		yield return Default_TellIntroAndLoop(SPINNING_JUMP_TELL_INTRO, m_spinningJump_TellIntro_AnimSpeed, SPINNING_JUMP_TELL_HOLD, m_spinningJump_TellHold_AnimSpeed, m_spinningJump_TellIntroAndHold_Delay);
		SetAttackingWithContactDamage(enable: true);
		Vector2 spinningJump_AttackVelocity = m_spinningJump_AttackVelocity;
		if (!base.EnemyController.IsFacingRight)
		{
			spinningJump_AttackVelocity.x = 0f - spinningJump_AttackVelocity.x;
		}
		SetVelocity(spinningJump_AttackVelocity, additive: false);
		m_regularWeaponHitbox.SetActive(value: false);
		m_regularBodyHitbox.SetActive(value: false);
		m_jumpingWeaponHitbox.SetActive(value: true);
		m_jumpingBodyHitbox.SetActive(value: true);
		m_prayingBodyHitbox.SetActive(value: false);
		yield return Default_Animation(SPINNING_JUMP_ATTACK_INTRO, m_spinningJump_AttackIntro_AnimSpeed, m_spinningJump_AttackIntro_Delay);
		yield return Wait(0.1f);
		yield return Default_Animation(SPINNING_JUMP_ATTACK_HOLD, m_spinningJump_AttackHold_AnimSpeed, m_spinningJump_AttackHold_Delay, waitUntilAnimationComplete: false);
		if (m_spinningJump_Spawn_AxeSpin)
		{
			m_spinningJumpProjectile = this.FireProjectile("SpellSwordAxeSpinProjectile", 3, matchFacing: false);
		}
		while (base.EnemyController.Velocity.y > 0f)
		{
			yield return null;
		}
		if (m_isInWhiteMode)
		{
			AudioManager.PlayOneShotAttached(null, "event:/SFX/Enemies/Cain/sfx_cain_light_jump_preLand", base.EnemyController.gameObject);
		}
		else
		{
			AudioManager.PlayOneShotAttached(null, "event:/SFX/Enemies/Cain/sfx_cain_dark_jump_preLand", base.EnemyController.gameObject);
		}
		yield return WaitUntilIsGrounded();
		m_regularWeaponHitbox.SetActive(value: true);
		m_regularBodyHitbox.SetActive(value: true);
		m_jumpingWeaponHitbox.SetActive(value: false);
		m_jumpingBodyHitbox.SetActive(value: false);
		m_prayingBodyHitbox.SetActive(value: false);
		this.FireProjectile("FinalBossSwordSlashProjectile", 4);
		if (m_spinningJump_Spawn_AxeSpin)
		{
			StopProjectile(ref m_spinningJumpProjectile);
		}
		if (m_isInWhiteMode)
		{
			float num = Random.Range(m_spinningJump_Land_ThrowPower.x, m_spinningJump_Land_ThrowPower.y);
			int num2 = Random.Range((int)m_spinningJump_Land_ThrowAngle.x, (int)m_spinningJump_Land_ThrowAngle.y);
			int num3 = Random.Range((int)m_spinningJump_Land_ThrowAngle2.x, (int)m_spinningJump_Land_ThrowAngle2.y);
			int num4 = Random.Range((int)m_spinningJump_Land_ThrowAngle3.x, (int)m_spinningJump_Land_ThrowAngle3.y);
			int num5 = Random.Range((int)m_spinningJump_Land_ThrowAngle4.x, (int)m_spinningJump_Land_ThrowAngle4.y);
			this.FireProjectile("FinalBossMagmaProjectile", 0, matchFacing: true, num2, num);
			this.FireProjectile("FinalBossMagmaProjectile", 0, matchFacing: true, 180 - num2, num);
			this.FireProjectile("FinalBossMagmaProjectile", 0, matchFacing: true, num3, num);
			this.FireProjectile("FinalBossMagmaProjectile", 0, matchFacing: true, 180 - num3, num);
			this.FireProjectile("FinalBossMagmaProjectile", 6, matchFacing: true, num4, num);
			this.FireProjectile("FinalBossMagmaProjectile", 5, matchFacing: true, 180 - num4, num);
			this.FireProjectile("FinalBossMagmaProjectile", 6, matchFacing: true, num5, num);
			this.FireProjectile("FinalBossMagmaProjectile", 5, matchFacing: true, 180 - num5, num);
			this.FireProjectile("FinalBossMagmaProjectile", 0, matchFacing: true, num2 + 3, num * m_spinningJump_Land_ThrowPowerHighMod);
			this.FireProjectile("FinalBossMagmaProjectile", 0, matchFacing: true, 180 - num2 - 3, num * m_spinningJump_Land_ThrowPowerHighMod);
			this.FireProjectile("FinalBossMagmaProjectile", 0, matchFacing: true, num3 + 7, num * m_spinningJump_Land_ThrowPowerHighMod);
			this.FireProjectile("FinalBossMagmaProjectile", 0, matchFacing: true, 180 - num3 - 7, num * m_spinningJump_Land_ThrowPowerHighMod);
			this.FireProjectile("FinalBossMagmaProjectile", 6, matchFacing: true, num4 + 8, num * m_spinningJump_Land_ThrowPowerHighMod);
			this.FireProjectile("FinalBossMagmaProjectile", 5, matchFacing: true, 180 - num4 - 8, num * m_spinningJump_Land_ThrowPowerHighMod);
			this.FireProjectile("FinalBossMagmaProjectile", 6, matchFacing: true, num5 + 12, num * m_spinningJump_Land_ThrowPowerHighMod);
			this.FireProjectile("FinalBossMagmaProjectile", 5, matchFacing: true, 180 - num5 - 12, num * m_spinningJump_Land_ThrowPowerHighMod);
			if (m_isPrime_Version)
			{
				this.FireProjectile("FinalBossMagmaProjectile", 0, matchFacing: true, num2 + 3, num * m_spinningJump_Land_ThrowPowerHighModAdvanced);
				this.FireProjectile("FinalBossMagmaProjectile", 0, matchFacing: true, 180 - num2 - 3, num * m_spinningJump_Land_ThrowPowerHighModAdvanced);
				this.FireProjectile("FinalBossMagmaProjectile", 0, matchFacing: true, num3 + 7, num * m_spinningJump_Land_ThrowPowerHighModAdvanced);
				this.FireProjectile("FinalBossMagmaProjectile", 0, matchFacing: true, 180 - num3 - 7, num * m_spinningJump_Land_ThrowPowerHighModAdvanced);
				this.FireProjectile("FinalBossMagmaProjectile", 6, matchFacing: true, num4 + 8, num * m_spinningJump_Land_ThrowPowerHighModAdvanced);
				this.FireProjectile("FinalBossMagmaProjectile", 5, matchFacing: true, 180 - num4 - 8, num * m_spinningJump_Land_ThrowPowerHighModAdvanced);
				this.FireProjectile("FinalBossMagmaProjectile", 6, matchFacing: true, num5 + 12, num * m_spinningJump_Land_ThrowPowerHighModAdvanced);
				this.FireProjectile("FinalBossMagmaProjectile", 5, matchFacing: true, 180 - num5 - 12, num * m_spinningJump_Land_ThrowPowerHighModAdvanced);
			}
			if (m_isPrime_Version && m_modeShiftIndex >= MODESHIFT_ARRAY.Length)
			{
				this.FireProjectile("FinalBossRollingProjectile", 0, matchFacing: false).Flip();
				this.FireProjectile("FinalBossRollingProjectile", 0, matchFacing: false, 180f);
			}
		}
		else
		{
			Projectile_RL projectile_RL = this.FireProjectile("FinalBossRollingProjectile", 0, matchFacing: false);
			this.FireProjectile("FinalBossRollingProjectile", 0, matchFacing: false, 0f, m_spinningJump_Land_RollingPowerMod);
			if (m_isPrime_Version)
			{
				this.FireProjectile("FinalBossRollingProjectile", 0, matchFacing: false, 0f, m_spinningJump_Land_RollingPowerModAdvanced);
			}
			projectile_RL.Flip();
			this.FireProjectile("FinalBossRollingProjectile", 0, matchFacing: false, 180f);
			this.FireProjectile("FinalBossRollingProjectile", 0, matchFacing: false, 180f, m_spinningJump_Land_RollingPowerMod);
			if (m_isPrime_Version)
			{
				this.FireProjectile("FinalBossRollingProjectile", 0, matchFacing: false, 180f, m_spinningJump_Land_RollingPowerModAdvanced);
			}
			if (m_isPrime_Version && m_modeShiftIndex >= MODESHIFT_ARRAY.Length)
			{
				PerformForcedColourShift();
				float num6 = Random.Range(m_spinningJump_Land_ThrowPower.x, m_spinningJump_Land_ThrowPower.y);
				int num7 = Random.Range((int)m_spinningJump_Land_ThrowAngle.x, (int)m_spinningJump_Land_ThrowAngle.y);
				int num8 = Random.Range((int)m_spinningJump_Land_ThrowAngle2.x, (int)m_spinningJump_Land_ThrowAngle2.y);
				int num9 = Random.Range((int)m_spinningJump_Land_ThrowAngle3.x, (int)m_spinningJump_Land_ThrowAngle3.y);
				int num10 = Random.Range((int)m_spinningJump_Land_ThrowAngle4.x, (int)m_spinningJump_Land_ThrowAngle4.y);
				this.FireProjectile("FinalBossMagmaProjectile", 0, matchFacing: true, num7 + 3, num6 * m_spinningJump_Land_ThrowPowerHighModAdvanced);
				this.FireProjectile("FinalBossMagmaProjectile", 0, matchFacing: true, 180 - num7 - 3, num6 * m_spinningJump_Land_ThrowPowerHighModAdvanced);
				this.FireProjectile("FinalBossMagmaProjectile", 0, matchFacing: true, num8 + 7, num6 * m_spinningJump_Land_ThrowPowerHighModAdvanced);
				this.FireProjectile("FinalBossMagmaProjectile", 0, matchFacing: true, 180 - num8 - 7, num6 * m_spinningJump_Land_ThrowPowerHighModAdvanced);
				this.FireProjectile("FinalBossMagmaProjectile", 6, matchFacing: true, num9 + 8, num6 * m_spinningJump_Land_ThrowPowerHighModAdvanced);
				this.FireProjectile("FinalBossMagmaProjectile", 5, matchFacing: true, 180 - num9 - 8, num6 * m_spinningJump_Land_ThrowPowerHighModAdvanced);
				this.FireProjectile("FinalBossMagmaProjectile", 6, matchFacing: true, num10 + 12, num6 * m_spinningJump_Land_ThrowPowerHighModAdvanced);
				this.FireProjectile("FinalBossMagmaProjectile", 5, matchFacing: true, 180 - num10 - 12, num6 * m_spinningJump_Land_ThrowPowerHighModAdvanced);
			}
		}
		SetVelocityX(0f, additive: false);
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation(SPINNING_JUMP_LAND, m_spinningJump_Exit_AnimSpeed, m_spinningJump_Exit_Delay);
		if (m_spinningJump_ExitHold_Delay > 0f)
		{
			yield return Wait(m_spinningJump_ExitHold_Delay);
		}
		yield return Default_Animation(SPINNING_JUMP_EXIT, m_spinningJump_Exit_AnimSpeed, m_spinningJump_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetBaseValues();
		if (m_isPrime_Version && m_modeShiftIndex >= MODESHIFT_ARRAY.Length)
		{
			yield return Default_Attack_Cooldown(m_Prime_Version_Final_Mode_Delay_Force_Idle_Add + m_prayer_Exit_IdleDuration, m_spinningJump_AttackCD);
		}
		else
		{
			yield return Default_Attack_Cooldown(m_spinningJump_Exit_IdleDuration, m_spinningJump_AttackCD);
		}
		PerformColourShiftCheck();
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Sword_Attack()
	{
		this.ToDo("Sword Attack");
		SetVelocityX(0f, additive: false);
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		float dashSpeed = m_sword_AttackSpeed;
		if (!base.EnemyController.IsTargetToMyRight)
		{
			dashSpeed = 0f - dashSpeed;
		}
		yield return Default_TellIntroAndLoop(SWORD_TELL_INTRO, m_sword_TellIntro_AnimSpeed, SWORD_TELL_HOLD, m_sword_TellHold_AnimSpeed, m_sword_TellIntroAndHold_Delay);
		SetAttackingWithContactDamage(enable: true);
		yield return Default_Animation(SWORD_ATTACK_INTRO, m_sword_AttackIntro_AnimSpeed, m_sword_AttackIntro_Delay);
		yield return Default_Animation(SWORD_ATTACK_HOLD, m_sword_AttackHold_AnimSpeed, m_sword_AttackHold_Delay, waitUntilAnimationComplete: false);
		this.FireProjectile("FinalBossSwordSlashProjectile", 17);
		if (m_isInWhiteMode)
		{
			Projectile_RL projectile_RL = this.FireProjectile("FinalBossBoneBigProjectile", 7);
			projectile_RL.RotationSpeed = projectile_RL.InitialRotationSpeed;
			Projectile_RL projectile_RL2 = this.FireProjectile("FinalBossBoneBigHorizontalProjectile", 7);
			projectile_RL2.RotationSpeed = projectile_RL2.InitialRotationSpeed;
			if (m_isPrime_Version)
			{
				Projectile_RL projectile_RL3 = this.FireProjectile("FinalBossBoneBigDiagonalProjectile", 7);
				projectile_RL3.RotationSpeed = projectile_RL3.InitialRotationSpeed;
			}
			this.FireProjectile("FinalBossBoneSmallProjectile", 8, matchFacing: true, 60f);
			this.FireProjectile("FinalBossBoneSmallProjectile", 8, matchFacing: true, 55f);
			this.FireProjectile("FinalBossBoneSmallProjectile", 8, matchFacing: true, 50f);
			if (m_isPrime_Version && m_modeShiftIndex >= MODESHIFT_ARRAY.Length)
			{
				PerformForcedColourShift();
				this.FireProjectile("FinalBossPotionProjectile", 8, matchFacing: true, 60f);
			}
		}
		else
		{
			this.FireProjectile("FinalBossWrappedBounceBigProjectile", 15);
			this.FireProjectile("FinalBossWrappedVoidSmallProjectile", 15);
			this.FireProjectile("FinalBossWrappedBounceBigProjectile", 16, matchFacing: true, 180f);
			this.FireProjectile("FinalBossWrappedVoidSmallProjectile", 16, matchFacing: true, 180f);
			if (m_isPrime_Version)
			{
				this.FireProjectile("FinalBossPotionProjectile", 8, matchFacing: true, 60f);
			}
			if (m_isPrime_Version && m_modeShiftIndex >= MODESHIFT_ARRAY.Length)
			{
				PerformForcedColourShift();
				this.FireProjectile("FinalBossBoneSmallProjectile", 8, matchFacing: true, 60f);
				this.FireProjectile("FinalBossBoneSmallProjectile", 8, matchFacing: true, 55f);
				this.FireProjectile("FinalBossBoneSmallProjectile", 8, matchFacing: true, 50f);
			}
		}
		SetVelocityX(dashSpeed, additive: false);
		if (m_sword_AttackDuration > 0f)
		{
			yield return Wait(m_sword_AttackDuration);
		}
		SetVelocityX(0f, additive: false);
		SetAttackingWithContactDamage(enable: false);
		SetVelocityX(0f, additive: false);
		yield return Default_Animation(SWORD_EXIT, m_sword_Exit_AnimSpeed, m_sword_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetBaseValues();
		if (m_isPrime_Version && m_modeShiftIndex >= MODESHIFT_ARRAY.Length)
		{
			yield return Default_Attack_Cooldown(m_Prime_Version_Final_Mode_Delay_Force_Idle_Add + m_prayer_Exit_IdleDuration, m_sword_AttackCD);
		}
		else
		{
			yield return Default_Attack_Cooldown(m_sword_Exit_IdleDuration, m_sword_AttackCD);
		}
		PerformColourShiftCheck();
	}

	private IEnumerator SwordAttackBlackSecondProjectileCoroutine()
	{
		float delay = Time.time + m_sword_BlackProjectile_Delay;
		while (Time.time < delay)
		{
			yield return null;
		}
		this.FireProjectile("FinalBossWrappedVoidBigProjectile", 15);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Portal_Attack()
	{
		this.ToDo("Portal Attack");
		SetVelocityX(0f, additive: false);
		base.EnemyController.LockFlip = true;
		Vector2 projSpawnPos = base.EnemyController.TargetController.Midpoint;
		if (m_isInWhiteMode)
		{
			m_portalWarningProj = this.FireProjectileAbsPos("FinalBossPortalInverseWarningProjectile", projSpawnPos, matchFacing: false);
		}
		else
		{
			m_portalWarningProj = this.FireProjectileAbsPos("FinalBossPortalWarningProjectile", projSpawnPos, matchFacing: false);
		}
		yield return Default_TellIntroAndLoop(PORTAL_TELL_INTRO, m_portal_TellIntro_AnimSpeed, PORTAL_TELL_HOLD, m_portal_TellHold_AnimSpeed, m_portal_TellIntroAndHold_Delay);
		yield return Default_Animation(PORTAL_ATTACK_INTRO, m_portal_AttackIntro_AnimSpeed, m_portal_AttackIntro_Delay);
		yield return Default_Animation(PORTAL_ATTACK_HOLD, m_portal_AttackHold_AnimSpeed, m_portal_AttackHold_Delay, waitUntilAnimationComplete: false);
		StopProjectile(ref m_portalWarningProj);
		StopPortalWhitePersistentCoroutine();
		if (m_isInWhiteMode)
		{
			AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Cain/sfx_cain_light_voidWave_aoe_explode", projSpawnPos);
			for (int i = 0; i <= Portal_Outward_Explosion_Amount; i++)
			{
				Vector2 vector = CDGHelper.RotatedPoint(new Vector2(7.5f, 0f), Vector2.zero, i * Portal_Outward_Explosion_Angle);
				this.FireProjectileAbsPos("FinalBossPortalLineProjectile", projSpawnPos + vector, matchFacing: false, i * Portal_Outward_Explosion_Angle);
			}
			if (m_isPrime_Version)
			{
				yield return Wait(0.25f);
				for (int j = 0; j <= Portal_Outward_Explosion_Amount; j++)
				{
					Vector2 vector2 = CDGHelper.RotatedPoint(new Vector2(7.5f, 0f), Vector2.zero, j * Portal_Outward_Explosion_Angle);
					this.FireProjectileAbsPos("FinalBossPortalLineProjectile", projSpawnPos + vector2, matchFacing: false, j * Portal_Outward_Explosion_Angle);
				}
			}
			if (m_isPrime_Version && m_modeShiftIndex >= MODESHIFT_ARRAY.Length)
			{
				PerformForcedColourShift();
				m_portalWarningProj = this.FireProjectileAbsPos("FinalBossPortalWarningProjectile", projSpawnPos, matchFacing: false);
				if (Portal_Advanced_White_To_Black_FollowUp_Delay != 0f)
				{
					yield return Wait(Portal_Advanced_White_To_Black_FollowUp_Delay);
				}
				StopProjectile(ref m_portalWarningProj);
				this.FireProjectileAbsPos("FinalBossPortalExplosionProjectile", projSpawnPos, matchFacing: false);
				_ = Portal_Outward_Explosion_Angle;
				_ = Portal_Outward_Explosion_Amount;
				for (int k = 0; k <= Portal_Explosion_Bolt_Amount; k++)
				{
					this.FireProjectileAbsPos("FinalBossPortalBoltProjectile", projSpawnPos, matchFacing: false, k * Portal_Explosion_Bolt_Angle);
				}
			}
		}
		else
		{
			AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Cain/sfx_cain_dark_voidWave_aoe_explode_first", projSpawnPos);
			this.FireProjectileAbsPos("FinalBossPortalExplosionProjectile", projSpawnPos, matchFacing: false);
			_ = Portal_Outward_Explosion_Angle;
			_ = Portal_Outward_Explosion_Amount;
			for (int l = 0; l <= Portal_Explosion_Bolt_Amount; l++)
			{
				this.FireProjectileAbsPos("FinalBossPortalBoltProjectile", projSpawnPos, matchFacing: false, l * Portal_Explosion_Bolt_Angle);
			}
			if (Portal_Explosion_Bolt_Delay != 0f)
			{
				yield return Wait(Portal_Explosion_Bolt_Delay);
			}
			AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Cain/sfx_cain_dark_voidWave_aoe_explode_second", projSpawnPos);
			for (int m = 0; m <= Portal_Explosion_Bolt_Amount; m++)
			{
				this.FireProjectileAbsPos("FinalBossPortalBoltProjectile", projSpawnPos, matchFacing: false, m * Portal_Explosion_Bolt_Angle + Portal_Explosion_Bolt_Angle / 2);
			}
			if (m_isPrime_Version)
			{
				if (Portal_Explosion_Bolt_Delay != 0f)
				{
					yield return Wait(Portal_Explosion_Bolt_Delay);
				}
				for (int n = 0; n <= Portal_Explosion_Bolt_Amount; n++)
				{
					this.FireProjectileAbsPos("FinalBossPortalBoltProjectile", projSpawnPos, matchFacing: false, n * Portal_Explosion_Bolt_Angle);
				}
			}
			if (m_isPrime_Version && m_modeShiftIndex >= MODESHIFT_ARRAY.Length)
			{
				PerformForcedColourShift();
				m_portalWarningProj = this.FireProjectileAbsPos("FinalBossPortalInverseWarningProjectile", projSpawnPos, matchFacing: false);
				if (Portal_Advanced_Black_To_White_FollowUp_Delay != 0f)
				{
					yield return Wait(Portal_Advanced_Black_To_White_FollowUp_Delay);
				}
				StopProjectile(ref m_portalWarningProj);
				for (int num = 0; num <= Portal_Outward_Explosion_Amount; num++)
				{
					Vector2 vector3 = CDGHelper.RotatedPoint(new Vector2(7.5f, 0f), Vector2.zero, num * Portal_Outward_Explosion_Angle);
					this.FireProjectileAbsPos("FinalBossPortalLineProjectile", projSpawnPos + vector3, matchFacing: false, num * Portal_Outward_Explosion_Angle);
				}
			}
		}
		yield return Default_Animation(PORTAL_EXIT, m_portal_Exit_AnimSpeed, m_portal_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetBaseValues();
		if (m_isPrime_Version && m_modeShiftIndex >= MODESHIFT_ARRAY.Length)
		{
			yield return Default_Attack_Cooldown(m_Prime_Version_Final_Mode_Delay_Force_Idle_Add + m_prayer_Exit_IdleDuration, m_portal_AttackCD);
		}
		else
		{
			yield return Default_Attack_Cooldown(m_portal_Exit_IdleDuration, m_portal_AttackCD);
		}
		PerformColourShiftCheck();
	}

	private IEnumerator PortalWhiteAttackPersistentCoroutine(Vector2 spawnPos)
	{
		int flipper = CDGHelper.RandomPlusMinus();
		int SpinningFireballCycles = Portal_Outward_Explosion_Angle * Portal_Outward_Explosion_Amount;
		for (int i = 0; i <= SpinningFireballCycles; i += Portal_Outward_Explosion_Angle)
		{
			this.FireProjectileAbsPos("FinalBossPortalLineProjectile", spawnPos, matchFacing: false, i * flipper);
			this.FireProjectileAbsPos("FinalBossPortalLineProjectile", spawnPos, matchFacing: false, 180 + i * flipper);
			yield return Wait(0.175f);
		}
	}

	private void StopPortalWhitePersistentCoroutine()
	{
		StopProjectile(ref m_portalProjectile);
		this.StopPersistentCoroutine(m_portalWhiteAttackPersistentCoroutine);
	}

	public override IEnumerator DeathAnim()
	{
		FinalBossRoomController finalRoom = base.EnemyController.Room.GetComponent<FinalBossRoomController>();
		if ((bool)finalRoom)
		{
			finalRoom.ColourShiftToDefault();
		}
		yield return base.DeathAnim();
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Cain/sfx_cain_death_start", CameraController.GameCamera.transform.position);
		yield return Default_Animation("Death_Intro", m_death_Intro_AnimSpeed, m_death_Intro_Delay);
		yield return Wait(0.5f, unscaledTime: true);
		AudioManager.PlayOneShot(this, "event:/SFX/Enemies/Cain/vo_cain_monster_greet", base.transform.position);
		DialogueManager.StartNewDialogue();
		DialogueManager.AddDialogue("LOC_ID_NAME_CAIN_NAME_1", "LOC_ID_CAIN_ENDING_VENGEANCE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		if (!WindowManager.GetIsWindowLoaded(WindowID.Dialogue))
		{
			WindowManager.LoadWindow(WindowID.Dialogue);
		}
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		while (WindowManager.GetIsWindowOpen(WindowID.Dialogue))
		{
			yield return null;
		}
		yield return Wait(0.25f, unscaledTime: true);
		if (m_rumbleEventInstance.isValid())
		{
			AudioManager.Play(null, m_rumbleEventInstance);
		}
		if (m_collapseSpiritsEventInstance.isValid())
		{
			AudioManager.Play(null, m_collapseSpiritsEventInstance);
		}
		yield return Default_Animation("Death", m_death_Hold_AnimSpeed, m_death_Hold_Delay - 1.8f, waitUntilAnimationComplete: false);
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Cain/sfx_boss_cain_death_screenFlash", CameraController.GameCamera.transform.position);
		EffectManager.PlayEffect(base.gameObject, null, "ScreenFlashEffect", Vector3.zero, 0f, EffectStopType.Gracefully);
		yield return Wait(1.5f, unscaledTime: true);
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Cain/sfx_boss_cain_death_screenFlash", CameraController.GameCamera.transform.position);
		EffectManager.PlayEffect(base.gameObject, null, "ScreenFlashEffect", Vector3.zero, 0f, EffectStopType.Gracefully);
		yield return Wait(0.3f, unscaledTime: true);
		RLTimeScale.SetTimeScale(TimeScaleType.Cutscene, 0.25f);
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Cain/sfx_boss_cain_death_screenFlash", CameraController.GameCamera.transform.position);
		EffectManager.PlayEffect(base.gameObject, null, "ScreenFlashEffect", Vector3.zero, 0f, EffectStopType.Gracefully);
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Cain/sfx_cain_death_finalGlow_flash_turnToStone", base.EnemyController.Midpoint);
		if (m_collapseSpiritsEventInstance.isValid())
		{
			AudioManager.Stop(m_collapseSpiritsEventInstance);
		}
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Cain/sfx_cain_death_spirits_end", base.EnemyController.Midpoint);
		m_statue.gameObject.SetActive(value: true);
		m_statue.gameObject.SetLayerRecursively(0);
		m_model.SetActive(value: false);
		if (m_rumbleEventInstance.isValid())
		{
			AudioManager.Stop(m_rumbleEventInstance);
		}
		finalRoom.SmashWindows();
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Cain/vo_cain_death", base.EnemyController.Midpoint);
		foreach (BaseEffect effect in EffectManager.GetEffectList("SpellswordBoss_DeathScream_Effect"))
		{
			effect.Stop(EffectStopType.Gracefully);
		}
		foreach (BaseEffect effect2 in EffectManager.GetEffectList("CameraShakeSmall_Effect"))
		{
			effect2.Stop(EffectStopType.Gracefully);
		}
		yield return Wait(0.5f);
		RLTimeScale.SetTimeScale(TimeScaleType.Cutscene, 1f);
	}

	private IEnumerator SoulsEscapeSFXCoroutine()
	{
		if (m_collapseSpiritsEventInstance.isValid())
		{
			AudioManager.Play(null, m_collapseSpiritsEventInstance);
		}
		BaseEffect deathScreamEffect = EffectManager.GetEffectList("FinalBoss_DeathScream_Effect")[0];
		while ((bool)deathScreamEffect && deathScreamEffect.IsPlaying)
		{
			yield return null;
		}
		if (m_collapseSpiritsEventInstance.isValid())
		{
			AudioManager.Stop(m_collapseSpiritsEventInstance);
		}
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Cain/sfx_cain_death_spirits_end", base.EnemyController.Midpoint);
	}

	public override IEnumerator SpawnAnim()
	{
		if (!SaveManager.PlayerSaveData.SpokenToFinalBoss)
		{
			yield return this.ChangeAnimationState("Transform");
			yield return WaitUntilAnimComplete(DefaultAnimationLayer);
			MusicManager.PlayMusic(SongID.FinalBoss_ASITP, isOverride: false);
			yield return Default_Animation(BaseAIScript.NEUTRAL_STATE, 1f, 1f, waitUntilAnimationComplete: false);
		}
		else
		{
			MusicManager.PlayMusic(SongID.FinalBoss_ASITP, isOverride: false);
			yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		}
	}

	protected override void OnDisable()
	{
		if (m_rumbleEventInstance.isValid())
		{
			AudioManager.Stop(m_rumbleEventInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);
		}
		if (m_collapseSpiritsEventInstance.isValid())
		{
			AudioManager.Stop(m_collapseSpiritsEventInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);
		}
		if (m_verticalBeamBlackEventInstance.isValid())
		{
			AudioManager.Stop(m_verticalBeamBlackEventInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);
		}
		if (m_verticalBeamBlackFloorLoopEventInstance.isValid())
		{
			AudioManager.Stop(m_verticalBeamBlackFloorLoopEventInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);
		}
		base.OnDisable();
	}

	private void OnDestroy()
	{
		if ((bool)base.EnemyController)
		{
			base.EnemyController.HealthChangeRelay.RemoveListener(OnBossHit);
		}
		if (m_rumbleEventInstance.isValid())
		{
			m_rumbleEventInstance.release();
		}
		if (m_collapseSpiritsEventInstance.isValid())
		{
			m_collapseSpiritsEventInstance.release();
		}
		if (m_verticalBeamBlackEventInstance.isValid())
		{
			m_verticalBeamBlackEventInstance.release();
		}
		if (m_verticalBeamBlackFloorLoopEventInstance.isValid())
		{
			m_verticalBeamBlackFloorLoopEventInstance.release();
		}
	}
}
