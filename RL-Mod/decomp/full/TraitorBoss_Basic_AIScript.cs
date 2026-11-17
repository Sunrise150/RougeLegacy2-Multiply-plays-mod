using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using RLAudio;
using UnityEngine;

public class TraitorBoss_Basic_AIScript : BaseAIScript
{
	protected enum TraitorRelicType
	{
		DamageZone = 1,
		Jump = 2,
		Dash = 4,
		Block = 8,
		TwinDamageZone = 0x10000,
		TwinJump = 0x20000,
		TwinDash = 0x40000,
		TwinBlock = 0x80000
	}

	[SerializeField]
	private LineRenderer m_aimLine;

	[SerializeField]
	private GameObject m_endAimIndicator;

	private static readonly float[] m_advancedModeshiftArray = new float[3] { 0.75f, 0.5f, 0.25f };

	private static readonly float[] m_expertModeshiftArray = new float[2] { 0.66f, 0.33f };

	private static readonly float[] m_minibossModeshiftArray = new float[2] { 0.66f, 0.33f };

	private const int RELIC_BLOCK_HIT_COUNT = 3;

	private const float RELIC_DAMAGE_ZONE_TRIGGER_INTERVAL = 10f;

	private const float RELIC_DAMAGE_ZONE_DAMAGE_DURATION = 2f;

	private const float RELIC_LAND_SHOCKWAVE_FALL_DURATION = 0.3f;

	private const string ATTACK_SFX_NAME_MALE = "event:/SFX/Enemies/Jonah/vo_jonah_attack";

	private const string ATTACK_SFX_NAME_FEMALE = "event:/SFX/Enemies/vo_jonah_female_attack";

	protected const string MAGMA_PROJECTILE = "TraitorMagmaProjectile";

	protected const string SPOON_PROJECTILE = "TraitorSpoonProjectile";

	protected const string HOMING_PROJECTILE = "TraitorHomingBoltProjectile";

	protected const string AXE_SPIN_PROJECTILE = "TraitorAxeSpinProjectile";

	protected const string BOOMERANG_PROJECTILE = "TraitorBoomerangProjectile";

	protected const string LIGHTNING_PROJECTILE = "TraitorLightningProjectile";

	protected const string LIGHTNING_WARNING_PROJECTILE = "TraitorLightningWarningProjectile";

	protected const string SCYTHE_PROJECTILE = "TraitorScytheProjectile";

	protected const string SCYTHE_SECOND_PROJECTILE = "TraitorScytheSecondProjectile";

	protected const string ARROW_PROJECTILE = "TraitorArrowProjectile";

	protected const string TELESLICE_PROJECTILE = "TraitorTelesliceProjectile";

	protected const string RELIC_LAND_PROJECTILE = "TraitorRelicLandProjectile";

	protected const string RELIC_DASH_VOID_PROJECTILE = "TraitorRelicDashVoidProjectile";

	protected const string RELIC_DASH_WARNING_PROJECTILE = "TraitorRelicDashVoidWarningProjectile";

	protected const string RELIC_DAMAGE_ZONE_PROJECTILE = "TraitorRelicDamageAuraProjectile";

	protected const string DASH_CURSE_PROJECTILE = "TraitorRelicDashCurseProjectile";

	protected const string SHOUT_ATTACK_EXPLOSION_PROJECTILE_NAME = "TraitorBossShoutExplosionProjectile";

	protected const string SHOUT_ATTACK_WARNING_PROJECTILE_NAME = "TraitorBossShoutWarningProjectile";

	protected const string STAFFTHROW_BEAM_PROJECTILE = "TraitorForwardBeamProjectile";

	protected const string STAFFTHROW_BEAM_WARNING_PROJECTILE = "TraitorWarningForwardBeamProjectile";

	protected const int MID_POS_INDEX = 1;

	private static TraitorRelicType[] m_traitorRelicTypeArray;

	protected const int NUM_RELIC_TYPES = 4;

	protected float m_modeShift_Damage_Mod = 0.1f;

	private bool m_isWalking;

	private bool m_isJumping;

	private bool m_isAxeSpinning;

	private bool m_isAimingSpread;

	private bool m_isAimingTeleslice;

	private bool m_isModeShifting;

	private bool m_prevIsGrounded = true;

	private float m_prevIsGroundedStartTime;

	protected int m_modeShiftIndex;

	private int m_relicHitCount;

	protected TraitorBossGeoController m_weaponGeoController;

	private float m_aimAngle;

	private List<RaycastHit2D> m_teleslicePlatformHitList = new List<RaycastHit2D>(5);

	private int m_relicMask;

	private int[] m_relicIndices;

	protected const string MAGMA_ATTACK_INTRO = "SpellCast_Attack_Intro";

	protected const string MAGMA_ATTACK_HOLD = "SpellCast_Attack_Hold";

	protected const string MAGMA_EXIT = "SpellCast_Exit";

	protected Vector2 m_magma_AngleRandomizerAdd = new Vector2(-30f, 30f);

	protected Vector2 m_magma_PowerRandomizerMod = new Vector2(0.8f, 1.2f);

	protected int m_magma_InitialAngle = 90;

	private EventInstance m_spoonThrowEventInstance;

	protected const string AXE_TELL_INTRO = "AxeGrounded_Tell_Intro";

	protected const string AXE_TELL_HOLD = "AxeGrounded_Tell_Hold";

	protected const string AXE_ATTACK_INTRO = "AxeAirborne_Attack_Intro";

	protected const string AXE_ATTACK_HOLD = "AxeAirborne_Attack_Hold";

	protected const string AXE_EXIT = "AxeAirborne_Exit";

	protected float m_axe_TellIntro_AnimSpeed = 1.25f;

	protected float m_axe_TellHold_AnimSpeed = 1.25f;

	protected float m_axe_TellIntroAndHold_Delay = 0.35f;

	protected float m_axe_AttackHold_AnimSpeed = 2f;

	protected float m_axe_AttackHold_Delay;

	protected float m_axe_Exit_AnimSpeed = 0.65f;

	protected float m_axe_Exit_Delay;

	protected float m_axe_Exit_IdleDuration = 0.1f;

	protected float m_axe_AttackCD = 1f;

	protected Projectile_RL m_axeSpinProjectile;

	protected const string TELESLICE_TELL_INTRO = "Teleport_Tell_Intro";

	protected const string TELESLICE_TELL_HOLD = "Teleport_Tell_Hold";

	protected const string TELESLICE_ATTACK_INTRO = "Teleport_Attack_Intro";

	protected const string TELESLICE_ATTACK_HOLD = "Teleport_Attack_Hold";

	protected const string TELESLICE_EXIT = "Teleport_Exit";

	protected float m_teleslice_TellIntro_AnimSpeed = 1f;

	protected float m_teleslice_TellHold_AnimSpeed = 1f;

	protected float m_teleslice_TellIntroAndHold_Delay = 0.8f;

	protected float m_teleslice_AttackIntro_AnimSpeed = 1f;

	protected float m_teleslice_AttackIntro_Delay;

	protected float m_teleslice_AttackHold_AnimSpeed = 1f;

	protected float m_teleslice_AttackHold_Delay = 0.1f;

	protected float m_teleslice_Exit_AnimSpeed = 1f;

	protected float m_teleslice_Exit_Delay;

	protected float m_teleslice_Exit_IdleDuration;

	protected float m_teleslice_AttackCD = 1f;

	protected Vector2 TELESLICE_PROJECTILE_OFFSET = new Vector2(0f, 1f);

	protected const string BOOMERANG_TELL_INTRO = "Chakram_Tell_Intro";

	protected const string BOOMERANG_TELL_HOLD = "Chakram_Tell_Hold";

	protected const string BOOMERANG_ATTACK_INTRO = "Chakram_Attack_Intro";

	protected const string BOOMERANG_ATTACK_HOLD = "Chakram_Attack_Hold";

	protected const string BOOMERANG_EXIT = "Chakram_Exit";

	protected float m_boomerang_TellIntro_AnimSpeed = 1f;

	protected float m_boomerang_TellHold_AnimSpeed = 1f;

	protected float m_boomerang_TellIntroAndHold_Delay = 0.25f;

	protected float m_boomerang_AttackIntro_AnimSpeed = 1f;

	protected float m_boomerang_AttackIntro_Delay;

	protected float m_boomerang_AttackHold_AnimSpeed = 1f;

	protected float m_boomerang_AttackHold_Delay;

	protected float m_boomerang_Exit_AnimSpeed = 0.65f;

	protected float m_boomerang_Exit_Delay;

	protected float m_boomerang_Exit_IdleDuration = 0.1f;

	protected float m_boomerang_AttackCD = 10f;

	protected float m_boomerang_LoopDelay = 0.5f;

	protected float m_boomerang_ThrowPowerScaling = 0.5f;

	protected const string SPREAD_TELL_INTRO = "Bow_Forward_Tell_Intro";

	protected const string SPREAD_TELL_HOLD = "Bow_Forward_Tell_Hold";

	protected const string SPREAD_ATTACK_INTRO = "Bow_Forward_Attack_Intro";

	protected const string SPREAD_ATTACK_HOLD = "Bow_Forward_Attack_Hold";

	protected const string SPREAD_EXIT = "Bow_Forward_Exit";

	protected bool m_spread_AimAtPlayer = true;

	protected float m_spread_TellIntro_AnimSpeed = 1f;

	protected float m_spread_TellHold_AnimSpeed = 1f;

	protected float m_spread_TellIntroAndHold_Delay;

	protected float m_spread_AttackIntro_AnimSpeed = 1f;

	protected float m_spread_AttackIntro_Delay;

	protected float m_spread_AttackHold_AnimSpeed = 1f;

	protected float m_spread_AttackHold_Delay;

	protected float m_spread_Exit_AnimSpeed = 1f;

	protected float m_spread_Exit_Delay;

	protected float m_spread_Exit_IdleDuration;

	protected float m_spread_AttackCD = 1f;

	protected Vector2 ARROW_POS_OFFSET = new Vector2(1f, 0.25f);

	protected const string LIGHTNING_TELL_INTRO = "OmniSpellCast_Tell_Intro";

	protected const string LIGHTNING_TELL_HOLD = "OmniSpellCast_Tell_Hold";

	protected const string LIGHTNING_ATTACK_INTRO = "OmniSpellCast_Attack_Intro";

	protected const string LIGHTNING_ATTACK_HOLD = "OmniSpellCast_Attack_Hold";

	protected const string LIGHTNING_EXIT = "OmniSpellCast_Exit";

	protected const string SCYTHE_TELL_INTRO = "Scythe_Tell_Intro";

	protected const string SCYTHE_TELL_HOLD = "Scythe_Tell_Hold";

	protected const string SCYTHE_ATTACK_INTRO = "Scythe_Attack_Intro";

	protected const string SCYTHE_ATTACK_HOLD = "Scythe_Attack_Hold";

	protected const string SCYTHE_TELL_INTRO2 = "Scythe_Tell_Intro_2";

	protected const string SCYTHE_TELL_HOLD2 = "Scythe_Tell_Hold_2";

	protected const string SCYTHE_ATTACK_INTRO2 = "Scythe_Attack_Intro_2";

	protected const string SCYTHE_ATTACK_HOLD2 = "Scythe_Attack_Hold_2";

	protected const string SCYTHE_EXIT = "Scythe_Exit";

	protected Vector2 m_jump_Random_AddX = new Vector2(-1f, 10f);

	protected Vector2 m_jump_Random_AddY = new Vector2(-3f, 3f);

	private Projectile_RL m_dashRelicWarningProjectile;

	protected const string RELIC_MODESHIFT_TELL_INTRO = "RetirePose";

	protected const string RELIC_MODESHIFT_TELL_HOLD = "Victory";

	protected float m_relicModeShift_Exit_IdleDuration = 0.1f;

	protected float m_relicModeShift_AttackCD = 99f;

	protected Coroutine m_damageZoneCoroutine;

	protected Projectile_RL m_damageZoneProjectile;

	protected const string DEATH_INTRO = "Stunned";

	protected const string DEATH_HOLD = "Death_";

	protected float m_death_Intro_Delay = 1f;

	protected float m_death_Hold_Delay = 1f;

	protected const string SPAWN_IDLE = "Idle";

	protected const string SPAWN_INTRO = "Bow";

	protected float m_spawn_Idle_AnimSpeed = 1f;

	protected float m_spawn_Idle_Delay;

	protected float m_spawn_Intro_AnimSpeed = 1f;

	protected float m_spawn_Intro_Delay;

	protected float[] MODESHIFT_ARRAY => base.EnemyController.EnemyRank switch
	{
		EnemyRank.Expert => m_expertModeshiftArray, 
		EnemyRank.Miniboss => m_minibossModeshiftArray, 
		_ => m_advancedModeshiftArray, 
	};

	public string GetAttackSFXName
	{
		get
		{
			if ((bool)base.EnemyController && base.EnemyController.EnemyRank == EnemyRank.Expert)
			{
				return "event:/SFX/Enemies/vo_jonah_female_attack";
			}
			return "event:/SFX/Enemies/Jonah/vo_jonah_attack";
		}
	}

	protected virtual bool is_Advanced => false;

	protected virtual bool is_Gregory => false;

	protected override Vector2 IdleDuration => new Vector2(0.25f, 0.7f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.45f, 0.85f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.45f, 0.85f);

	protected virtual int KNOCKBACK_DEFENSE_OVERRIDE => 7;

	protected virtual float DEFAULT_WEAPON_SWAP_DELAY => 0.125f;

	protected float m_magma_AttackIntro_AnimationSpeed => 1f;

	protected float m_magma_AttackIntro_Delay => 0f;

	protected float m_magma_AttackHold_AnimationSpeed => 1f;

	protected float m_magma_AttackHold_Delay => 0f;

	protected float m_magma_Exit_AnimationSpeed => 1f;

	protected float m_magma_Exit_Delay => 0.1f;

	protected virtual Vector2 m_magma_JumpDelay => new Vector2(0.15f, 0.35f);

	protected virtual int m_magma_ProjectilesFired => 1;

	protected virtual float m_magma_ProjectileDelay => 0.1f;

	protected virtual float m_magma_Exit_ForceIdle => 0.2f;

	protected virtual float m_magma_Exit_AttackCD => 8.5f;

	protected virtual int m_magma_angleAdder => 30;

	protected float m_teleslice_DelayBetweenSlices => 0.5f;

	protected float m_teleslice_TeleportDistance => 18f;

	protected virtual Vector2 m_boomerang_JumpDelay => new Vector2(0.3f, 0.65f);

	protected virtual int m_boomerang_ThrowLoopCount => 1;

	protected virtual Vector2 m_spread_JumpDelay => new Vector2(0.3f, 0.45f);

	protected virtual float m_lightning_TellIntro_AnimSpeed => 1f;

	protected virtual float m_lightning_TellIntro_Delay => 0f;

	protected virtual float m_lightning_TellHold_AnimSpeed => 1f;

	protected virtual float m_lightning_TellHold_Delay => 0.45f;

	protected virtual float m_lightning_AttackIntro_AnimSpeed => 1f;

	protected virtual float m_lightning_AttackIntro_Delay => 0f;

	protected virtual float m_lightning_AttackHold_AnimSpeed => 1f;

	protected virtual float m_lightning_AttackHold_Delay => 0f;

	protected virtual float m_lightning_Exit_AnimSpeed => 1f;

	protected virtual float m_lightning_Exit_Delay => 0f;

	protected virtual float m_lightning_Exit_ForceIdle => 0.2f;

	protected virtual float m_lightning_Exit_AttackCD => 8.5f;

	protected virtual float m_lightning_Attack_DelayBetweenStrikesFirstPause => 0.25f;

	protected virtual float m_lightning_Attack_DelayBetweenStrikesSubsequent => 0.075f;

	protected virtual float m_lightning_Attack_Duration => 0.6f;

	protected virtual int m_lightning_NumStrikes => 10;

	protected virtual float m_lightning_StrikeOffsetX => 6f;

	protected virtual float m_scythe_TellIntro_AnimSpeed => 1f;

	protected virtual float m_scythe_TellIntro_Delay => 0f;

	protected virtual float m_scythe_TellHold_AnimSpeed => 1f;

	protected virtual float m_scythe_TellHold_Delay => 0.35f;

	protected virtual float m_scythe_TellHold_Delay2 => 0f;

	protected virtual float m_scythe_AttackIntro_AnimSpeed => 1f;

	protected virtual float m_scythe_AttackIntro_Delay => 0f;

	protected virtual float m_scythe_AttackHold_AnimSpeed => 1f;

	protected virtual float m_scythe_AttackHold_Delay => 0f;

	protected virtual float m_scythe_Exit_AnimSpeed => 1f;

	protected virtual float m_scythe_Exit_Delay => 0f;

	protected virtual float m_scythe_Exit_ForceIdle => 0.1f;

	protected virtual float m_scythe_Exit_AttackCD => 5.5f;

	protected virtual float m_scythe_Attack_ForwardSpeedOverride => 35.5f;

	protected virtual float m_scythe_Attack_Duration => 0.4f;

	protected virtual bool m_scythe_RaiseKnockbackDefenseWhileAttacking => true;

	protected virtual int m_scythe_KnockbackDefenseBoostOverride => 99;

	protected virtual float m_scythe_AttackDuration => 0.375f;

	protected virtual float m_scythe_TimeBetweenAttacks => 0.1f;

	protected virtual Vector2 m_scythe_JumpDelay => new Vector2(0.2f, 0.35f);

	protected virtual float m_dashMove_Exit_ForceIdle => 0f;

	protected virtual float m_dashMove_Exit_AttackCD => 1.5f;

	protected virtual float m_jump_Exit_Delay => 0f;

	protected virtual float m_jump_PowerX => 10f;

	protected virtual float m_jump_PowerY => 33f;

	protected virtual float m_lowjump_PowerX => 26f;

	protected virtual float m_lowjump_PowerY => 18f;

	protected virtual float m_straightJump_PowerX => 3f;

	protected virtual float m_straightJump_PowerY => 28f;

	protected virtual float m_dash_ForwardSpeedOverride => 27.5f;

	protected virtual float m_dash_BackwardSpeedOverride => -27.5f;

	protected virtual float m_dashRelicWarningDuration => 1.1f;

	protected virtual float m_relicModeShift_TellIntro_AnimSpeed => 0.5f;

	protected virtual float m_relicModeShift_TellIntro_Delay => 2.25f;

	protected virtual float m_relicModeShift_TellHold_AnimSpeed => 1f;

	protected virtual float m_relicModeShift_TellHold_Delay => 1.75f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[20]
		{
			"TraitorMagmaProjectile", "TraitorSpoonProjectile", "TraitorHomingBoltProjectile", "TraitorAxeSpinProjectile", "TraitorBoomerangProjectile", "TraitorLightningProjectile", "TraitorLightningWarningProjectile", "TraitorScytheProjectile", "TraitorScytheSecondProjectile", "TraitorArrowProjectile",
			"TraitorTelesliceProjectile", "TraitorRelicLandProjectile", "TraitorRelicDashVoidProjectile", "TraitorRelicDashVoidWarningProjectile", "TraitorRelicDashCurseProjectile", "TraitorRelicDamageAuraProjectile", "TraitorBossShoutExplosionProjectile", "TraitorBossShoutWarningProjectile", "TraitorForwardBeamProjectile", "TraitorWarningForwardBeamProjectile"
		};
	}

	protected RelicType GetRelicType(TraitorRelicType traitorRelicType)
	{
		return traitorRelicType switch
		{
			TraitorRelicType.DamageZone => RelicType.DamageAuraOnHit, 
			TraitorRelicType.Jump => RelicType.LandShockwave, 
			TraitorRelicType.Dash => RelicType.DashStrikeDamageUp, 
			TraitorRelicType.Block => RelicType.FreeHitRegenerate, 
			_ => RelicType.None, 
		};
	}

	protected string GetRelicLocID(TraitorRelicType traitorRelicType)
	{
		return traitorRelicType switch
		{
			TraitorRelicType.DamageZone => "LOC_ID_TRAITOR_RELICS_DAMAGEZONE_1", 
			TraitorRelicType.Jump => "LOC_ID_TRAITOR_RELICS_JUMP_1", 
			TraitorRelicType.Dash => "LOC_ID_TRAITOR_RELICS_DASH_1", 
			TraitorRelicType.Block => "LOC_ID_TRAITOR_RELICS_BLOCK_1", 
			_ => null, 
		};
	}

	protected StatusBarEntryType GetStatusBarIcon(TraitorRelicType traitorRelicType)
	{
		return traitorRelicType switch
		{
			TraitorRelicType.DamageZone => StatusBarEntryType.TraitorRelic_DamageZone, 
			TraitorRelicType.Jump => StatusBarEntryType.TraitorRelic_Jump, 
			TraitorRelicType.Dash => StatusBarEntryType.TraitorRelic_Dash, 
			TraitorRelicType.Block => StatusBarEntryType.TraitorRelic_Block, 
			_ => StatusBarEntryType.None, 
		};
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		if (m_traitorRelicTypeArray == null)
		{
			m_traitorRelicTypeArray = Enum.GetValues(typeof(TraitorRelicType)) as TraitorRelicType[];
		}
		base.EnemyController.HealthChangeRelay.AddListener(OnBossHit);
		m_weaponGeoController = enemyController.GetComponent<TraitorBossGeoController>();
		m_relicIndices = new int[4];
		DefaultAnimationLayer = 1;
		OnEnable();
		InitializeAimLine();
		if (!m_spoonThrowEventInstance.isValid())
		{
			m_spoonThrowEventInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/Jonah/sfx_jonah_spoon_throwing_loop", base.EnemyController.transform);
		}
	}

	private void InitializeAimLine()
	{
		float num = m_teleslice_TeleportDistance / (float)m_aimLine.positionCount;
		for (int i = 0; i < m_aimLine.positionCount; i++)
		{
			m_aimLine.SetPosition(i, new Vector3(num * (float)i, 0f, 0f));
		}
		m_endAimIndicator.transform.localPosition = new Vector3(m_teleslice_TeleportDistance, 0f, 0f);
		m_aimLine.transform.localScale = new Vector3(1f / base.EnemyController.transform.lossyScale.x, 1f / base.EnemyController.transform.lossyScale.y, 1f);
		m_aimLine.transform.position = base.EnemyController.Midpoint;
		base.LogicController.DisableLogicActivationByDistance = true;
	}

	public override void ResetScript()
	{
		base.ResetScript();
		m_isWalking = false;
		m_isJumping = false;
		m_isAxeSpinning = false;
		m_isAimingSpread = false;
		m_isAimingTeleslice = false;
		m_relicMask = 0;
		base.Animator.SetBool("IsTraitorBoss", value: true);
		base.Animator.SetFloat("Walk_Anim_Speed", 0.8f);
		m_isModeShifting = false;
		m_modeShiftIndex = 0;
		base.EnemyController.StatusBarController.StopAllUIEffects();
		m_aimLine.gameObject.SetActive(value: false);
		m_prevIsGrounded = true;
		m_relicHitCount = 0;
		base.EnemyController.TakesNoDamage = false;
		base.EnemyController.StatusEffectController.ImmuneToAllStatusEffects = false;
		HideAllWeaponGeos();
	}

	private void OnEnable()
	{
		if (base.IsInitialized)
		{
			m_isWalking = false;
			m_isJumping = false;
			m_isAxeSpinning = false;
			m_isAimingSpread = false;
			m_isAimingTeleslice = false;
			base.Animator.SetBool("IsTraitorBoss", value: true);
			base.Animator.SetFloat("Walk_Anim_Speed", 0.8f);
			m_aimLine.gameObject.SetActive(value: false);
			HideAllWeaponGeos();
			for (int i = 0; i < 4; i++)
			{
				m_relicIndices[i] = i;
			}
			CDGHelper.Shuffle(m_relicIndices);
			m_relicMask = 0;
		}
	}

	private void OnDestroy()
	{
		if ((bool)base.EnemyController)
		{
			base.EnemyController.HealthChangeRelay.RemoveListener(OnBossHit);
		}
		if (m_spoonThrowEventInstance.isValid())
		{
			m_spoonThrowEventInstance.release();
		}
	}

	private void OnBossHit(object sender, HealthChangeEventArgs args)
	{
		if (base.EnemyController.IsDead)
		{
			return;
		}
		if (HasRelic(TraitorRelicType.Block))
		{
			m_relicHitCount++;
			if (m_relicHitCount > 3)
			{
				m_relicHitCount = 0;
				base.EnemyController.TakesNoDamage = false;
				AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Jonah/sfx_jonah_coesShell_damagePrevented", base.EnemyController.transform.position);
			}
			else if (m_relicHitCount == 3)
			{
				base.EnemyController.TakesNoDamage = true;
			}
			base.EnemyController.StatusBarController.ApplyUIEffect(StatusBarEntryType.TraitorRelic_Block, 3, m_relicHitCount);
		}
		if (!(args.PrevHealthValue <= args.NewHealthValue) && !m_isModeShifting && m_modeShiftIndex < MODESHIFT_ARRAY.Length && base.EnemyController.EnemyRank >= EnemyRank.Advanced)
		{
			float num = MODESHIFT_ARRAY[m_modeShiftIndex] * (float)base.EnemyController.ActualMaxHealth;
			if (base.EnemyController.CurrentHealth <= num)
			{
				m_modeShiftIndex++;
				base.LogicController.StopAllLogic(resetCurrentState: true);
				base.LogicController.ForceExecuteLogicBlockName_OnceOnly = "Relic_Mode_Shift";
			}
		}
	}

	private void LateUpdate()
	{
		if (!base.IsInitialized)
		{
			return;
		}
		if (base.EnemyController.CurrentHealth > 0f)
		{
			base.Animator.SetBool("Walking", m_isWalking && base.EnemyController.Velocity.x != 0f && base.EnemyController.IsGrounded);
			base.Animator.SetBool("Grounded", base.EnemyController.IsGrounded);
			base.Animator.SetFloat("ySpeed", base.EnemyController.Velocity.y);
			if (m_isJumping && base.EnemyController.IsGrounded)
			{
				m_isJumping = false;
				base.Animator.SetBool("Jumping", value: false);
			}
		}
		if (m_isAimingSpread)
		{
			Vector3 vector = new Vector3(base.EnemyController.Midpoint.x, base.EnemyController.Midpoint.y + ARROW_POS_OFFSET.y, base.EnemyController.Midpoint.z);
			Vector3 midpoint = base.EnemyController.TargetController.Midpoint;
			m_aimAngle = Mathf.Atan2(midpoint.y - vector.y, midpoint.x - vector.x);
			m_aimAngle = CDGHelper.ToDegrees(m_aimAngle);
			m_aimAngle = CDGHelper.WrapAngleDegrees(m_aimAngle, convertToHalfRadius: true);
			if (!base.EnemyController.IsFacingRight)
			{
				m_aimAngle = CDGHelper.WrapAngleDegrees(-180f - m_aimAngle, convertToHalfRadius: true);
			}
			float t = (m_aimAngle - -90f) / 180f;
			float value = Mathf.Lerp(4f, 0f, t);
			base.EnemyController.Animator.SetFloat("Attack_Direction", value);
		}
		if (m_isAimingTeleslice)
		{
			m_aimAngle = CDGHelper.AngleBetweenPts(base.EnemyController.Midpoint, base.EnemyController.TargetController.Midpoint);
			if (!m_aimLine.gameObject.activeSelf)
			{
				m_aimLine.gameObject.SetActive(value: true);
			}
			m_aimLine.transform.localEulerAngles = new Vector3(0f, 0f, m_aimAngle);
		}
		else if (m_aimLine.gameObject.activeSelf)
		{
			m_aimLine.gameObject.SetActive(value: false);
		}
		if (HasRelic(TraitorRelicType.Jump) && !m_prevIsGrounded && base.EnemyController.IsGrounded && Time.time - m_prevIsGroundedStartTime > 0.3f)
		{
			this.FireProjectileAbsPos("TraitorRelicLandProjectile", base.EnemyController.transform.position, matchFacing: false);
			this.FireProjectile("TraitorMagmaProjectile", 0, matchFacing: true, 85f);
			this.FireProjectile("TraitorMagmaProjectile", 0, matchFacing: true, 95f);
		}
		m_prevIsGrounded = base.EnemyController.IsGrounded;
		if (m_prevIsGrounded)
		{
			m_prevIsGroundedStartTime = Time.time;
		}
	}

	public override void OnLBCompleteOrCancelled()
	{
		m_isWalking = false;
		m_isAimingSpread = false;
		m_isAimingTeleslice = false;
		base.EnemyController.DisableFriction = false;
		m_isAxeSpinning = false;
		StopProjectile(ref m_axeSpinProjectile);
		base.Animator.SetBool("Dashing", value: false);
		base.Animator.SetBool("Victory", value: false);
		Single_Enable_Gravity();
		Single_Disable_KnockbackDefense();
		base.Animator.Play("Empty", DefaultAnimationLayer);
		base.EnemyController.LockFlip = false;
		if (m_spoonThrowEventInstance.isValid())
		{
			AudioManager.Stop(m_spoonThrowEventInstance);
		}
		base.OnLBCompleteOrCancelled();
	}

	public override IEnumerator WalkTowards()
	{
		m_isWalking = true;
		yield return base.WalkTowards();
	}

	public override IEnumerator WalkAway()
	{
		m_isWalking = true;
		yield return base.WalkAway();
	}

	public override void Pause()
	{
		base.Pause();
		if ((bool)m_axeSpinProjectile && m_axeSpinProjectile.isActiveAndEnabled && m_axeSpinProjectile.OwnerController == base.EnemyController)
		{
			m_isAxeSpinning = true;
			StopProjectile(ref m_axeSpinProjectile);
		}
	}

	public override void Unpause()
	{
		base.Unpause();
		if (m_isAxeSpinning)
		{
			m_isAxeSpinning = false;
			m_axeSpinProjectile = this.FireProjectile("TraitorAxeSpinProjectile", 0, matchFacing: false);
		}
	}

	protected IEnumerator SetWeaponGeo(GameObject geo, float holdDelay)
	{
		if (!geo.activeSelf)
		{
			HideAllWeaponGeos();
			geo.SetActive(value: true);
			base.Animator.SetBool("Victory", value: true);
			yield return null;
			yield return WaitUntilAnimComplete(0);
			yield return Wait(holdDelay);
			base.Animator.SetBool("Victory", value: false);
			yield return null;
			yield return WaitUntilAnimComplete(0);
		}
	}

	private IEnumerator SetWeaponGeos(GameObject geo1, GameObject geo2, float holdDelay)
	{
		if (!geo1.activeSelf)
		{
			HideAllWeaponGeos();
			geo1.SetActive(value: true);
			geo2.SetActive(value: true);
			base.Animator.SetBool("Victory", value: true);
			yield return null;
			yield return WaitUntilAnimComplete(0);
			yield return Wait(holdDelay);
			base.Animator.SetBool("Victory", value: false);
			yield return null;
			yield return WaitUntilAnimComplete(0);
		}
	}

	protected void HideAllWeaponGeos()
	{
		for (int i = 0; i < m_weaponGeoController.AllWeaponGeo.Length; i++)
		{
			if (m_weaponGeoController.AllWeaponGeo[i].activeSelf)
			{
				m_weaponGeoController.AllWeaponGeo[i].SetActive(value: false);
			}
		}
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator MagmaCombo()
	{
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Jonah/sfx_jonah_spoon_equip", base.EnemyController.transform.position);
		yield return SetWeaponGeo(m_weaponGeoController.Ladle, DEFAULT_WEAPON_SWAP_DELAY);
		CDGHelper.RandomPlusMinus();
		yield return Single_Jump();
		float seconds = UnityEngine.Random.Range(m_magma_JumpDelay.x, m_magma_JumpDelay.y);
		yield return Wait(seconds);
		if (m_spoonThrowEventInstance.isValid())
		{
			AudioManager.PlayAttached(null, m_spoonThrowEventInstance, base.EnemyController.gameObject);
		}
		yield return Default_Animation("SpellCast_Attack_Intro", m_magma_AttackIntro_AnimationSpeed, m_magma_AttackIntro_Delay);
		yield return Default_Animation("SpellCast_Attack_Hold", m_magma_AttackHold_AnimationSpeed, m_magma_AttackHold_Delay, waitUntilAnimationComplete: false);
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Jonah/vo_jonah_attack", base.EnemyController.transform.position);
		for (int i = 0; i < m_magma_ProjectilesFired; i++)
		{
			TraitorBoss_Basic_AIScript aiScript = this;
			_ = m_magma_angleAdder;
			aiScript.FireProjectile("TraitorSpoonProjectile", 0, matchFacing: true, 90f);
			this.FireProjectile("TraitorSpoonProjectile", 0, matchFacing: true, 90 + m_magma_angleAdder);
			this.FireProjectile("TraitorSpoonProjectile", 0, matchFacing: true, 90 + m_magma_angleAdder * -1);
			this.FireProjectile("TraitorSpoonProjectile", 0, matchFacing: true, 90 + m_magma_angleAdder * 2);
			this.FireProjectile("TraitorSpoonProjectile", 0, matchFacing: true, 90 + m_magma_angleAdder * -2);
			if (is_Advanced)
			{
				this.FireProjectile("TraitorSpoonProjectile", 0, matchFacing: true, 90 + m_magma_angleAdder * 3);
				this.FireProjectile("TraitorSpoonProjectile", 0, matchFacing: true, 90 + m_magma_angleAdder * -3);
			}
			if (m_magma_ProjectileDelay != 0f)
			{
				yield return Wait(m_magma_ProjectileDelay);
			}
		}
		if (m_spoonThrowEventInstance.isValid())
		{
			AudioManager.Stop(m_spoonThrowEventInstance);
		}
		yield return Default_Animation("SpellCast_Exit", m_magma_Exit_AnimationSpeed, m_magma_Exit_Delay);
		base.Animator.SetTrigger("Change_Ability_Anim");
		base.EnemyController.LockFlip = false;
		yield return Single_WaitUntilGrounded();
		HideAllWeaponGeos();
		yield return Default_Attack_Cooldown(m_magma_Exit_ForceIdle, m_magma_Exit_AttackCD);
		base.EnemyController.ResetBaseValues();
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator AxeCombo()
	{
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		Single_Enable_KnockbackDefense();
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Jonah/sfx_jonah_axe_equip", base.EnemyController.transform.position);
		yield return SetWeaponGeo(m_weaponGeoController.Axe, DEFAULT_WEAPON_SWAP_DELAY);
		yield return Default_TellIntroAndLoop("AxeGrounded_Tell_Intro", m_axe_TellIntro_AnimSpeed, "AxeGrounded_Tell_Hold", m_axe_TellHold_AnimSpeed, m_axe_TellIntroAndHold_Delay);
		yield return Default_Animation("AxeAirborne_Attack_Hold", m_axe_AttackHold_AnimSpeed, m_axe_AttackHold_Delay, waitUntilAnimationComplete: false);
		m_axeSpinProjectile = this.FireProjectile("TraitorAxeSpinProjectile", 1, matchFacing: false);
		AudioManager.PlayOneShot(null, GetAttackSFXName, base.EnemyController.transform.position);
		yield return Single_Jump();
		yield return Single_WaitUntilGrounded();
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Jonah/sfx_jonah_axe_bounce", base.EnemyController.transform.position);
		base.EnemyController.LockFlip = false;
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		yield return Single_Jump(randomizeJump: false, lowJump: true);
		yield return Single_WaitUntilGrounded();
		if (is_Advanced)
		{
			AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Jonah/sfx_jonah_axe_bounce", base.EnemyController.transform.position);
			base.EnemyController.LockFlip = false;
			StopAndFaceTarget();
			base.EnemyController.LockFlip = true;
			yield return Single_Jump(randomizeJump: false, lowJump: true);
			yield return Single_WaitUntilGrounded();
		}
		StopProjectile(ref m_axeSpinProjectile);
		Single_Disable_KnockbackDefense();
		yield return Default_Animation("AxeAirborne_Exit", m_axe_Exit_AnimSpeed, m_axe_Exit_Delay);
		base.Animator.SetTrigger("Change_Ability_Anim");
		base.EnemyController.LockFlip = false;
		HideAllWeaponGeos();
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_axe_Exit_IdleDuration, m_axe_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator TeleSliceCombo()
	{
		StopAndFaceTarget();
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Jonah/sfx_jonah_sword_equip", base.EnemyController.transform.position);
		yield return SetWeaponGeo(m_weaponGeoController.Katana, DEFAULT_WEAPON_SWAP_DELAY);
		m_isAimingTeleslice = true;
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Jonah/sfx_jonah_sword_prep", base.EnemyController.transform.position);
		yield return Default_TellIntroAndLoop("Teleport_Tell_Intro", m_teleslice_TellIntro_AnimSpeed, "Teleport_Tell_Hold", m_teleslice_TellHold_AnimSpeed, m_teleslice_TellIntroAndHold_Delay);
		m_isAimingTeleslice = false;
		base.EnemyController.LockFlip = true;
		Single_TeleSlice();
		yield return Default_Animation("Teleport_Attack_Hold", m_teleslice_AttackHold_AnimSpeed, m_teleslice_AttackHold_Delay);
		base.EnemyController.LockFlip = false;
		m_isAimingTeleslice = true;
		yield return Default_TellIntroAndLoop("Teleport_Tell_Intro", m_teleslice_TellIntro_AnimSpeed, "Teleport_Tell_Hold", m_teleslice_TellHold_AnimSpeed, m_teleslice_TellIntroAndHold_Delay);
		m_isAimingTeleslice = false;
		base.EnemyController.LockFlip = true;
		Single_TeleSlice();
		yield return Default_Animation("Teleport_Attack_Hold", m_teleslice_AttackHold_AnimSpeed, m_teleslice_AttackHold_Delay);
		if (is_Advanced)
		{
			m_isAimingTeleslice = true;
			yield return Default_TellIntroAndLoop("Teleport_Tell_Intro", m_teleslice_TellIntro_AnimSpeed, "Teleport_Tell_Hold", m_teleslice_TellHold_AnimSpeed, m_teleslice_TellIntroAndHold_Delay);
			m_isAimingTeleslice = false;
			base.EnemyController.LockFlip = true;
			Single_TeleSlice();
			yield return Default_Animation("Teleport_Attack_Hold", m_teleslice_AttackHold_AnimSpeed, m_teleslice_AttackHold_Delay);
		}
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Jonah/sfx_jonah_sword_flourish", base.EnemyController.transform.position);
		yield return Default_Animation("Teleport_Exit", m_teleslice_Exit_AnimSpeed, m_teleslice_Exit_Delay);
		base.Animator.SetTrigger("Change_Ability_Anim");
		HideAllWeaponGeos();
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_teleslice_Exit_IdleDuration, m_teleslice_AttackCD);
	}

	private void Single_TeleSlice()
	{
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Jonah/sfx_jonah_sword_dash", base.EnemyController.transform.position);
		AudioManager.PlayOneShot(null, GetAttackSFXName, base.EnemyController.transform.position);
		Vector2 vector = CDGHelper.AngleToVector(m_aimAngle);
		LayerMask layerMask = (int)base.EnemyController.ControllerCorgi.SavedPlatformMask & ~(int)base.EnemyController.ControllerCorgi.OneWayPlatformMask;
		ContactFilter2D contactFilter = default(ContactFilter2D);
		contactFilter.NoFilter();
		contactFilter.SetLayerMask(layerMask);
		m_teleslicePlatformHitList.Clear();
		Physics2D.Raycast(base.EnemyController.Midpoint, vector, contactFilter, m_teleslicePlatformHitList, m_teleslice_TeleportDistance);
		RaycastHit2D raycastHit2D = default(RaycastHit2D);
		foreach (RaycastHit2D teleslicePlatformHit in m_teleslicePlatformHitList)
		{
			if ((bool)teleslicePlatformHit)
			{
				raycastHit2D = teleslicePlatformHit;
				break;
			}
		}
		Vector3 localPosition = base.EnemyController.transform.localPosition;
		if ((bool)raycastHit2D)
		{
			Vector2 vector2 = -vector;
			Vector3 vector3 = base.EnemyController.Midpoint - base.EnemyController.transform.localPosition;
			if (vector.x >= 0f)
			{
				localPosition.x = Mathf.Max(raycastHit2D.point.x - vector3.x + vector2.x, localPosition.x);
			}
			else
			{
				localPosition.x = Mathf.Min(raycastHit2D.point.x - vector3.x + vector2.x, localPosition.x);
			}
			if (vector.y >= 0f)
			{
				localPosition.y = Mathf.Max(raycastHit2D.point.y - vector3.y + vector2.y, localPosition.y);
			}
			else
			{
				localPosition.y = Mathf.Min(raycastHit2D.point.y - vector3.y + vector2.y, localPosition.y);
			}
		}
		else
		{
			localPosition.x += vector.x * m_teleslice_TeleportDistance;
			localPosition.y += vector.y * m_teleslice_TeleportDistance;
		}
		Rect boundsRect = PlayerManager.GetCurrentPlayerRoom().BoundsRect;
		float num = 0f;
		float num2 = 0f;
		if (localPosition.x < boundsRect.xMin)
		{
			num = boundsRect.xMin - localPosition.x;
		}
		else if (localPosition.x > boundsRect.xMax)
		{
			num = boundsRect.xMax - localPosition.x;
		}
		if (localPosition.y < boundsRect.yMin)
		{
			num2 = boundsRect.yMin - localPosition.y;
		}
		else if (localPosition.y > boundsRect.yMax)
		{
			num2 = boundsRect.yMax - localPosition.y;
		}
		localPosition.x += num;
		localPosition.y += num2;
		if (num != 0f || num2 != 0f)
		{
			Vector2 vector4 = CDGHelper.AngleToVector(CDGHelper.AngleBetweenPts(base.EnemyController.Midpoint, localPosition));
			raycastHit2D = Physics2D.Raycast(base.EnemyController.Midpoint, vector4, m_teleslice_TeleportDistance, layerMask);
			if ((bool)raycastHit2D)
			{
				Vector2 vector5 = -vector4;
				Vector3 vector6 = base.EnemyController.Midpoint - base.EnemyController.transform.localPosition;
				localPosition.x = raycastHit2D.point.x - vector6.x + vector5.x;
				localPosition.y = raycastHit2D.point.y - vector6.y + vector5.y;
			}
		}
		base.EnemyController.transform.localPosition = localPosition;
		base.EnemyController.ControllerCorgi.SetRaysParameters();
		base.EnemyController.SetVelocity(0f, 0f, additive: false);
		Vector2 offset = CDGHelper.RotatedPoint(new Vector2(TELESLICE_PROJECTILE_OFFSET.x, 0f), Vector2.zero, m_aimAngle);
		offset.y += TELESLICE_PROJECTILE_OFFSET.y;
		this.FireProjectile("TraitorTelesliceProjectile", offset, matchFacing: false, m_aimAngle);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator BoomerangCombo()
	{
		SetVelocityX(0f, additive: false);
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Jonah/sfx_jonah_pizza_equip", base.EnemyController.transform.position);
		yield return SetWeaponGeo(m_weaponGeoController.Pizza, DEFAULT_WEAPON_SWAP_DELAY);
		float dieRollJump = CDGHelper.RandomPlusMinus();
		if (dieRollJump > 0f)
		{
			yield return Single_Jump();
			float seconds = UnityEngine.Random.Range(m_boomerang_JumpDelay.x, m_boomerang_JumpDelay.y);
			yield return Wait(seconds);
		}
		Single_Disable_Gravity();
		Single_Enable_KnockbackDefense();
		base.EnemyController.LockFlip = false;
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Jonah/sfx_jonah_pizza_prep", base.EnemyController.transform.position);
		yield return Default_TellIntroAndLoop("Chakram_Tell_Intro", m_boomerang_TellIntro_AnimSpeed, "Chakram_Tell_Hold", m_boomerang_TellHold_AnimSpeed, m_boomerang_TellIntroAndHold_Delay);
		for (int i = 0; i < m_boomerang_ThrowLoopCount; i++)
		{
			if (i > 0)
			{
				base.EnemyController.LockFlip = false;
				StopAndFaceTarget();
				base.EnemyController.LockFlip = true;
			}
			yield return Default_Animation("Chakram_Attack_Intro", m_boomerang_AttackIntro_AnimSpeed, m_boomerang_AttackIntro_Delay);
			yield return Default_Animation("Chakram_Attack_Hold", m_boomerang_AttackHold_AnimSpeed, m_boomerang_AttackHold_Delay, waitUntilAnimationComplete: false);
			AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Jonah/sfx_jonah_pizza_throw", base.EnemyController.transform.position);
			AudioManager.PlayOneShot(null, GetAttackSFXName, base.EnemyController.transform.position);
			if (!base.EnemyController.IsGrounded)
			{
				if (is_Gregory)
				{
					this.FireProjectile("TraitorBoomerangProjectile", 0, matchFacing: false, 0f, 1f + (float)i * m_boomerang_ThrowPowerScaling);
					this.FireProjectile("TraitorBoomerangProjectile", 0, matchFacing: false, 180f, 1f + (float)i * m_boomerang_ThrowPowerScaling);
				}
				else if (is_Advanced)
				{
					this.FireProjectile("TraitorBoomerangProjectile", 0, matchFacing: false, 0f, 1f + (float)i * m_boomerang_ThrowPowerScaling);
					this.FireProjectile("TraitorBoomerangProjectile", 0, matchFacing: false, 45f, 1f + (float)i * m_boomerang_ThrowPowerScaling);
					this.FireProjectile("TraitorBoomerangProjectile", 0, matchFacing: false, 180f, 1f + (float)i * m_boomerang_ThrowPowerScaling);
					this.FireProjectile("TraitorBoomerangProjectile", 0, matchFacing: false, 135f, 1f + (float)i * m_boomerang_ThrowPowerScaling);
				}
				else
				{
					this.FireProjectile("TraitorBoomerangProjectile", 0, matchFacing: false, 0f, 1f + (float)i * m_boomerang_ThrowPowerScaling);
					this.FireProjectile("TraitorBoomerangProjectile", 0, matchFacing: false, 90f, 1f + (float)i * m_boomerang_ThrowPowerScaling);
					this.FireProjectile("TraitorBoomerangProjectile", 0, matchFacing: false, 180f, 1f + (float)i * m_boomerang_ThrowPowerScaling);
				}
			}
			else if (is_Gregory)
			{
				this.FireProjectile("TraitorBoomerangProjectile", 0, matchFacing: false, 25f, 1f + (float)i * m_boomerang_ThrowPowerScaling);
				this.FireProjectile("TraitorBoomerangProjectile", 0, matchFacing: false, 155f, 1f + (float)i * m_boomerang_ThrowPowerScaling);
			}
			else if (is_Advanced)
			{
				this.FireProjectile("TraitorBoomerangProjectile", 0, matchFacing: false, 25f, 1f + (float)i * m_boomerang_ThrowPowerScaling);
				this.FireProjectile("TraitorBoomerangProjectile", 0, matchFacing: false, 55f, 1f + (float)i * m_boomerang_ThrowPowerScaling);
				this.FireProjectile("TraitorBoomerangProjectile", 0, matchFacing: false, 125f, 1f + (float)i * m_boomerang_ThrowPowerScaling);
				this.FireProjectile("TraitorBoomerangProjectile", 0, matchFacing: false, 155f, 1f + (float)i * m_boomerang_ThrowPowerScaling);
			}
			else
			{
				this.FireProjectile("TraitorBoomerangProjectile", 0, matchFacing: false, 25f, 1f + (float)i * m_boomerang_ThrowPowerScaling);
				this.FireProjectile("TraitorBoomerangProjectile", 0, matchFacing: false, 90f, 1f + (float)i * m_boomerang_ThrowPowerScaling);
				this.FireProjectile("TraitorBoomerangProjectile", 0, matchFacing: false, 155f, 1f + (float)i * m_boomerang_ThrowPowerScaling);
			}
			if (m_boomerang_LoopDelay > 0f)
			{
				yield return Wait(m_boomerang_LoopDelay);
			}
		}
		yield return Default_Animation("Chakram_Exit", m_boomerang_Exit_AnimSpeed, m_boomerang_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.Animator.SetTrigger("Change_Ability_Anim");
		base.EnemyController.Animator.ResetTrigger("Turn");
		Single_Enable_Gravity();
		Single_Disable_KnockbackDefense();
		if (dieRollJump > 0f)
		{
			yield return Single_WaitUntilGrounded();
		}
		HideAllWeaponGeos();
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_boomerang_Exit_IdleDuration, m_boomerang_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator SpreadCombo()
	{
		yield return Single_Dash(dashForwards: false, 0.25f);
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		yield return Wait(0.2f);
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Jonah/sfx_jonah_longbow_equip", base.EnemyController.transform.position);
		yield return SetWeaponGeos(m_weaponGeoController.Bow, m_weaponGeoController.Arrow, DEFAULT_WEAPON_SWAP_DELAY);
		float dieRollJump = CDGHelper.RandomPlusMinus();
		if (dieRollJump > 0f)
		{
			yield return Single_Jump();
			float seconds = UnityEngine.Random.Range(m_spread_JumpDelay.x, m_spread_JumpDelay.y);
			yield return Wait(seconds);
		}
		base.EnemyController.LockFlip = false;
		if (m_spread_AimAtPlayer)
		{
			m_isAimingSpread = true;
		}
		else
		{
			if (base.EnemyController.IsGrounded)
			{
				m_aimAngle = UnityEngine.Random.Range(0, 90);
			}
			else
			{
				m_aimAngle = UnityEngine.Random.Range(-90, 90);
			}
			float t = (m_aimAngle - -90f) / 180f;
			float value = Mathf.Lerp(4f, 0f, t);
			base.EnemyController.Animator.SetFloat("Attack_Direction", value);
		}
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Jonah/sfx_jonah_longbow_prep", base.EnemyController.transform.position);
		yield return Default_TellIntroAndLoop("Bow_Forward_Tell_Intro", m_spread_TellIntro_AnimSpeed, "Bow_Forward_Tell_Hold", m_spread_TellHold_AnimSpeed, m_spread_TellIntroAndHold_Delay);
		yield return Default_Animation("Bow_Forward_Attack_Intro", m_spread_AttackIntro_AnimSpeed, m_spread_AttackIntro_Delay);
		yield return Default_Animation("Bow_Forward_Attack_Hold", m_spread_AttackHold_AnimSpeed, 0f, waitUntilAnimationComplete: false);
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Jonah/sfx_jonah_longbow_shoot", base.EnemyController.transform.position);
		AudioManager.PlayOneShot(null, GetAttackSFXName, base.EnemyController.transform.position);
		Vector2 posOffset2 = ARROW_POS_OFFSET;
		posOffset2 = CDGHelper.RotatedPoint(posOffset2, Vector2.zero, m_aimAngle);
		posOffset2 += (Vector2)(base.EnemyController.Midpoint - base.EnemyController.transform.localPosition);
		if (!is_Gregory)
		{
			this.FireProjectile("TraitorArrowProjectile", posOffset2, matchFacing: true, m_aimAngle);
			this.FireProjectile("TraitorArrowProjectile", posOffset2, matchFacing: true, m_aimAngle + 25f);
			this.FireProjectile("TraitorArrowProjectile", posOffset2, matchFacing: true, m_aimAngle - 25f);
			if (is_Advanced)
			{
				this.FireProjectile("TraitorArrowProjectile", posOffset2, matchFacing: true, m_aimAngle + 50f);
				this.FireProjectile("TraitorArrowProjectile", posOffset2, matchFacing: true, m_aimAngle - 50f);
			}
		}
		else
		{
			for (int i = 0; i < 2; i++)
			{
				this.FireProjectile("TraitorArrowProjectile", posOffset2, matchFacing: true, m_aimAngle, 1.2f);
				this.FireProjectile("TraitorArrowProjectile", posOffset2, matchFacing: true, m_aimAngle + 25f, 1.2f);
				this.FireProjectile("TraitorArrowProjectile", posOffset2, matchFacing: true, m_aimAngle - 25f, 1.2f);
				yield return Wait(0.15f);
				if (i != 0)
				{
					yield return Default_Animation("Bow_Forward_Attack_Intro", m_spread_AttackIntro_AnimSpeed, m_spread_AttackIntro_Delay);
					yield return Default_Animation("Bow_Forward_Attack_Hold", m_spread_AttackHold_AnimSpeed, 0f, waitUntilAnimationComplete: false);
				}
			}
		}
		if (m_spread_AttackHold_Delay > 0f)
		{
			yield return Wait(m_spread_AttackHold_Delay);
		}
		yield return Default_Animation("Bow_Forward_Exit", m_spread_Exit_AnimSpeed, m_spread_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.Animator.SetTrigger("Change_Ability_Anim");
		m_isAimingSpread = false;
		if (dieRollJump > 0f)
		{
			yield return Single_WaitUntilGrounded();
		}
		HideAllWeaponGeos();
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_spread_Exit_IdleDuration, m_spread_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator LightningAttack()
	{
		StopAndFaceTarget();
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Jonah/sfx_jonah_lightning_prepCharge_fullScreen", CameraController.GameCamera.transform.position);
		Vector2 lightningOriginPos = base.EnemyController.transform.position;
		if (!is_Advanced)
		{
			for (int k = 0; k < m_lightning_NumStrikes; k++)
			{
				if (k == 0)
				{
					this.FireProjectile("TraitorLightningWarningProjectile", Vector2.zero, matchFacing: false, 90f);
					continue;
				}
				this.FireProjectile("TraitorLightningWarningProjectile", new Vector2((0f - m_lightning_StrikeOffsetX) * (float)k, 0f), matchFacing: false, 90f);
				this.FireProjectile("TraitorLightningWarningProjectile", new Vector2(m_lightning_StrikeOffsetX * (float)k, 0f), matchFacing: false, 90f);
			}
		}
		else
		{
			for (int l = 0; l < m_lightning_NumStrikes; l++)
			{
				if (l == 0)
				{
					this.FireProjectile("TraitorLightningWarningProjectile", Vector2.zero, matchFacing: false, 90f);
					this.FireProjectile("TraitorLightningWarningProjectile", new Vector2((0f - m_lightning_StrikeOffsetX) / 2f, 0f), matchFacing: false, 90f);
					this.FireProjectile("TraitorLightningWarningProjectile", new Vector2(m_lightning_StrikeOffsetX / 2f, 0f), matchFacing: false, 90f);
					continue;
				}
				float num = (float)l * m_lightning_StrikeOffsetX * 2f;
				this.FireProjectile("TraitorLightningWarningProjectile", new Vector2(0f - num, 0f), matchFacing: false, 90f);
				this.FireProjectile("TraitorLightningWarningProjectile", new Vector2(0f - num - m_lightning_StrikeOffsetX / 2f, 0f), matchFacing: false, 90f);
				this.FireProjectile("TraitorLightningWarningProjectile", new Vector2(0f - num + m_lightning_StrikeOffsetX / 2f, 0f), matchFacing: false, 90f);
				this.FireProjectile("TraitorLightningWarningProjectile", new Vector2(num, 0f), matchFacing: false, 90f);
				this.FireProjectile("TraitorLightningWarningProjectile", new Vector2(num - m_lightning_StrikeOffsetX / 2f, 0f), matchFacing: false, 90f);
				this.FireProjectile("TraitorLightningWarningProjectile", new Vector2(num + m_lightning_StrikeOffsetX / 2f, 0f), matchFacing: false, 90f);
			}
		}
		yield return Default_Animation("OmniSpellCast_Tell_Intro", m_lightning_TellIntro_AnimSpeed, m_lightning_TellIntro_Delay);
		yield return Default_Animation("OmniSpellCast_Tell_Hold", m_lightning_TellHold_AnimSpeed, m_lightning_TellHold_Delay);
		base.EnemyController.LockFlip = true;
		yield return Default_Animation("OmniSpellCast_Attack_Intro", m_lightning_AttackIntro_AnimSpeed, m_lightning_AttackIntro_Delay);
		yield return Default_Animation("OmniSpellCast_Attack_Hold", m_lightning_AttackHold_AnimSpeed, 0f, waitUntilAnimationComplete: false);
		AudioManager.PlayOneShot(null, GetAttackSFXName, base.EnemyController.transform.position);
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Jonah/sfx_jonah_lightning_attack_start_fullScreen", CameraController.GameCamera.transform.position);
		if (!is_Advanced)
		{
			for (int i = 0; i < m_lightning_NumStrikes; i++)
			{
				if (i == 0)
				{
					this.FireProjectileAbsPos("TraitorLightningProjectile", lightningOriginPos, matchFacing: false, 90f);
					yield return Wait(m_lightning_Attack_DelayBetweenStrikesFirstPause);
				}
				else
				{
					this.FireProjectileAbsPos("TraitorLightningProjectile", lightningOriginPos + new Vector2((0f - m_lightning_StrikeOffsetX) * (float)i, 0f), matchFacing: false, 90f);
					this.FireProjectileAbsPos("TraitorLightningProjectile", lightningOriginPos + new Vector2(m_lightning_StrikeOffsetX * (float)i, 0f), matchFacing: false, 90f);
					yield return Wait(m_lightning_Attack_DelayBetweenStrikesSubsequent);
				}
			}
		}
		else
		{
			for (int i = 0; i < m_lightning_NumStrikes; i++)
			{
				if (i == 0)
				{
					this.FireProjectileAbsPos("TraitorLightningProjectile", lightningOriginPos, matchFacing: false, 90f);
					this.FireProjectileAbsPos("TraitorLightningProjectile", lightningOriginPos + new Vector2((0f - m_lightning_StrikeOffsetX) / 2f, 0f), matchFacing: false, 90f);
					this.FireProjectileAbsPos("TraitorLightningProjectile", lightningOriginPos + new Vector2(m_lightning_StrikeOffsetX / 2f, 0f), matchFacing: false, 90f);
					yield return Wait(m_lightning_Attack_DelayBetweenStrikesFirstPause);
					continue;
				}
				float num2 = (float)i * m_lightning_StrikeOffsetX * 2f;
				this.FireProjectileAbsPos("TraitorLightningProjectile", lightningOriginPos + new Vector2(0f - num2, 0f), matchFacing: false, 90f);
				this.FireProjectileAbsPos("TraitorLightningProjectile", lightningOriginPos + new Vector2(0f - num2 - m_lightning_StrikeOffsetX / 2f, 0f), matchFacing: false, 90f);
				this.FireProjectileAbsPos("TraitorLightningProjectile", lightningOriginPos + new Vector2(0f - num2 + m_lightning_StrikeOffsetX / 2f, 0f), matchFacing: false, 90f);
				this.FireProjectileAbsPos("TraitorLightningProjectile", lightningOriginPos + new Vector2(num2, 0f), matchFacing: false, 90f);
				this.FireProjectileAbsPos("TraitorLightningProjectile", lightningOriginPos + new Vector2(num2 - m_lightning_StrikeOffsetX / 2f, 0f), matchFacing: false, 90f);
				this.FireProjectileAbsPos("TraitorLightningProjectile", lightningOriginPos + new Vector2(num2 + m_lightning_StrikeOffsetX / 2f, 0f), matchFacing: false, 90f);
				yield return Wait(m_lightning_Attack_DelayBetweenStrikesSubsequent);
			}
		}
		if (m_lightning_Attack_Duration > 0f)
		{
			yield return Wait(m_lightning_Attack_Duration);
		}
		yield return Default_Animation("OmniSpellCast_Exit", m_lightning_Exit_AnimSpeed, m_lightning_Exit_Delay);
		base.Animator.SetTrigger("Change_Ability_Anim");
		base.EnemyController.LockFlip = false;
		yield return Default_Attack_Cooldown(m_lightning_Exit_ForceIdle, m_lightning_Exit_AttackCD);
		base.EnemyController.ResetBaseValues();
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator ScytheAttack()
	{
		StopAndFaceTarget();
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Jonah/sfx_jonah_scythe_equip", base.EnemyController.transform.position);
		yield return SetWeaponGeo(m_weaponGeoController.Scythe, DEFAULT_WEAPON_SWAP_DELAY);
		float dieRollJump = CDGHelper.RandomPlusMinus();
		if (dieRollJump > 0f)
		{
			yield return Single_Jump(randomizeJump: true, lowJump: false, straightJump: true);
			float seconds = UnityEngine.Random.Range(m_scythe_JumpDelay.x, m_scythe_JumpDelay.y);
			yield return Wait(seconds);
		}
		float scytheSpeed = m_scythe_Attack_ForwardSpeedOverride;
		if (!base.EnemyController.IsTargetToMyRight)
		{
			scytheSpeed = 0f - scytheSpeed;
		}
		this.FaceTarget();
		base.EnemyController.LockFlip = true;
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Jonah/sfx_jonah_scythe_prep", base.EnemyController.transform.position);
		yield return Default_Animation("Scythe_Tell_Intro", m_scythe_TellIntro_AnimSpeed, m_scythe_TellIntro_Delay);
		yield return Default_Animation("Scythe_Tell_Hold", m_scythe_TellHold_AnimSpeed, m_scythe_TellHold_Delay);
		Single_Disable_Gravity();
		yield return Default_Animation("Scythe_Attack_Intro", m_scythe_AttackIntro_AnimSpeed, m_scythe_AttackIntro_Delay);
		yield return Default_Animation("Scythe_Attack_Hold", m_scythe_AttackHold_AnimSpeed, m_scythe_AttackHold_Delay, waitUntilAnimationComplete: false);
		AudioManager.PlayOneShot(null, GetAttackSFXName, base.EnemyController.transform.position);
		AudioManager.PlayOneShotAttached(null, "event:/SFX/Enemies/Jonah/sfx_jonah_scythe_dash", base.EnemyController.gameObject);
		if (m_scythe_RaiseKnockbackDefenseWhileAttacking && base.EnemyController.BaseKnockbackDefense < (float)m_scythe_KnockbackDefenseBoostOverride)
		{
			base.EnemyController.BaseKnockbackDefense = m_scythe_KnockbackDefenseBoostOverride;
		}
		SetVelocityX(scytheSpeed, additive: false);
		base.EnemyController.GroundHorizontalVelocity = scytheSpeed;
		base.EnemyController.DisableFriction = true;
		this.FireProjectile("TraitorScytheProjectile", 0);
		if (is_Advanced)
		{
			this.FireProjectile("TraitorRelicDashCurseProjectile", 0, matchFacing: true, 70f);
		}
		yield return Wait(m_scythe_TimeBetweenAttacks);
		yield return Default_Animation("Scythe_Tell_Intro_2", m_scythe_TellIntro_AnimSpeed, m_scythe_TellIntro_Delay);
		yield return Default_Animation("Scythe_Tell_Hold_2", m_scythe_TellHold_AnimSpeed, m_scythe_TellHold_Delay2);
		yield return Default_Animation("Scythe_Attack_Intro_2", m_scythe_AttackIntro_AnimSpeed, m_scythe_AttackIntro_Delay);
		yield return Default_Animation("Scythe_Attack_Hold_2", m_scythe_AttackHold_AnimSpeed, m_scythe_AttackHold_Delay, waitUntilAnimationComplete: false);
		this.FireProjectile("TraitorScytheSecondProjectile", 0);
		if (is_Advanced)
		{
			this.FireProjectile("TraitorRelicDashCurseProjectile", 0, matchFacing: true, 110f);
		}
		if (m_scythe_AttackDuration > 0f)
		{
			yield return Wait(m_scythe_AttackDuration);
		}
		SetVelocityX(0f, additive: false);
		base.EnemyController.GroundHorizontalVelocity = 0f;
		base.EnemyController.DisableFriction = false;
		Single_Enable_Gravity();
		if (m_scythe_Attack_Duration > 0f)
		{
			yield return Wait(m_scythe_Attack_Duration);
		}
		yield return Default_Animation("Scythe_Exit", m_scythe_Exit_AnimSpeed, m_scythe_Exit_Delay);
		base.Animator.SetTrigger("Change_Ability_Anim");
		base.EnemyController.LockFlip = false;
		if (dieRollJump > 0f)
		{
			yield return Single_WaitUntilGrounded();
		}
		HideAllWeaponGeos();
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_scythe_Exit_ForceIdle, m_scythe_Exit_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[RestLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator DashMove()
	{
		if (CDGHelper.RandomPlusMinus() > 0)
		{
			yield return Single_Dash(dashForwards: false, 0.5f);
		}
		else
		{
			yield return Single_Dash(dashForwards: true, 0.5f);
		}
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_dashMove_Exit_ForceIdle, m_dashMove_Exit_AttackCD);
	}

	public IEnumerator Single_Jump(bool randomizeJump = true, bool lowJump = false, bool straightJump = false)
	{
		float num = m_jump_PowerX;
		float num2 = m_jump_PowerY;
		if (lowJump)
		{
			num = m_lowjump_PowerX;
			num2 = m_lowjump_PowerY;
		}
		if (straightJump)
		{
			num = m_lowjump_PowerX;
			num2 = m_lowjump_PowerY;
		}
		if (randomizeJump)
		{
			num += UnityEngine.Random.Range(m_jump_Random_AddX.x, m_jump_Random_AddX.y);
			num2 += UnityEngine.Random.Range(m_jump_Random_AddY.x, m_jump_Random_AddY.y);
		}
		if (base.EnemyController.IsFacingRight)
		{
			SetVelocityX(num, additive: true);
		}
		else
		{
			SetVelocityX(0f - num, additive: true);
		}
		SetVelocityY(num2, additive: false);
		AudioManager.PlayOneShot(null, GetAttackSFXName, base.EnemyController.transform.position);
		base.Animator.SetBool("Jumping", value: true);
		yield return Wait(0.05f);
	}

	protected IEnumerator Single_WaitUntilGrounded()
	{
		yield return WaitUntilIsGrounded();
		SetVelocityX(0f, additive: false);
		if (m_jump_Exit_Delay > 0f)
		{
			yield return Wait(m_jump_Exit_Delay);
		}
	}

	private void Single_Disable_Gravity()
	{
		base.EnemyController.SetVelocity(0f, 0f, additive: false);
		base.EnemyController.FallMultiplierOverride = 0f;
		base.EnemyController.ControllerCorgi.StickWhenWalkingDownSlopes = false;
	}

	private void Single_Enable_Gravity()
	{
		base.EnemyController.SetVelocity(0f, 0f, additive: false);
		base.EnemyController.FallMultiplierOverride = 1f;
		base.EnemyController.ControllerCorgi.StickWhenWalkingDownSlopes = true;
	}

	public void Single_Enable_KnockbackDefense()
	{
		base.EnemyController.BaseKnockbackDefense = KNOCKBACK_DEFENSE_OVERRIDE;
	}

	protected void Single_Disable_KnockbackDefense()
	{
		base.EnemyController.BaseKnockbackDefense = base.EnemyController.EnemyData.KnockbackDefence;
	}

	private IEnumerator Single_Dash(bool dashForwards, float duration)
	{
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		AudioManager.PlayOneShotAttached(null, "event:/SFX/Enemies/Jonah/sfx_jonah_dash", base.EnemyController.gameObject);
		if (HasRelic(TraitorRelicType.Dash))
		{
			this.RunPersistentCoroutine(SpawnDashProjectileCoroutine(m_dashRelicWarningDuration));
		}
		float num = m_dash_ForwardSpeedOverride;
		if (!dashForwards)
		{
			num = m_dash_BackwardSpeedOverride;
		}
		if (!base.EnemyController.IsTargetToMyRight)
		{
			num = 0f - num;
		}
		base.Animator.SetBool("Dashing", value: true);
		Single_Disable_Gravity();
		SetVelocity(new Vector2(num, 0f), additive: false);
		base.EnemyController.GroundHorizontalVelocity = num;
		base.EnemyController.DisableFriction = true;
		yield return Wait(duration);
		base.Animator.SetBool("Dashing", value: false);
		Single_Enable_Gravity();
		base.EnemyController.GroundHorizontalVelocity = 0f;
		base.EnemyController.LockFlip = false;
	}

	private IEnumerator SpawnDashProjectileCoroutine(float duration)
	{
		_ = Vector3.zero;
		m_dashRelicWarningProjectile = this.FireProjectileAbsPos("TraitorRelicDashVoidWarningProjectile", base.EnemyController.transform.position, matchFacing: false);
		Vector3 warningSpawnPos = m_dashRelicWarningProjectile.transform.position;
		float delay = duration + Time.time;
		while (Time.time < delay)
		{
			yield return null;
		}
		this.FireProjectileAbsPos("TraitorRelicDashVoidProjectile", warningSpawnPos, matchFacing: false);
		StopProjectile(ref m_dashRelicWarningProjectile);
	}

	public IEnumerator Mode_Shift()
	{
		int num = 0;
		if (false)
		{
			m_relicMask |= 1 << 16 + m_relicIndices[num];
		}
		else
		{
			m_relicMask |= 1 << m_relicIndices[num];
		}
		yield break;
	}

	private bool HasRelic(TraitorRelicType relicType)
	{
		return ((uint)relicType & (uint)m_relicMask) != 0;
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Relic_Mode_Shift()
	{
		int num = UnityEngine.Random.Range(0, 4);
		TraitorRelicType chosenRelic = m_traitorRelicTypeArray[num];
		int i;
		for (i = 0; i < 4; i++)
		{
			if (!HasRelic(chosenRelic))
			{
				break;
			}
			num++;
			if (num >= 4)
			{
				num = 0;
			}
			chosenRelic = m_traitorRelicTypeArray[num];
		}
		if (i < 4)
		{
			this.ToDo("Relic ModeShift");
			RelicType actualRelic = GetRelicType(chosenRelic);
			m_isModeShifting = true;
			yield return base.DeathAnim();
			if (!base.EnemyController.IsGrounded)
			{
				yield return WaitUntilIsGrounded();
			}
			SetVelocityX(0f, additive: false);
			RemoveStatusEffects(includeCommanderBuffs: false);
			base.EnemyController.LockFlip = true;
			base.EnemyController.StatusEffectController.ImmuneToAllStatusEffects = true;
			base.EnemyController.ModeshiftDamageMod = m_modeShift_Damage_Mod;
			base.EnemyController.BaseKnockbackDefense = 99999f;
			HideAllWeaponGeos();
			this.SetAnimationSpeedMultiplier(m_relicModeShift_TellIntro_AnimSpeed);
			base.EnemyController.Animator.SetBool("RetirePose", value: true);
			base.EnemyController.Animator.Play("RetirePose", 0);
			AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Jonah/sfx_jonah_death_retire", base.EnemyController.transform.position);
			if (m_relicModeShift_TellIntro_Delay > 0f)
			{
				yield return Wait(m_relicModeShift_TellIntro_Delay);
			}
			base.EnemyController.Animator.SetBool("RetirePose", value: false);
			this.SetAnimationSpeedMultiplier(m_relicModeShift_TellHold_AnimSpeed);
			base.EnemyController.Animator.SetBool("Victory", value: true);
			base.EnemyController.Animator.Play("Victory", 0);
			AudioManager.PlayOneShot(null, GetAttackSFXName, base.EnemyController.transform.position);
			m_relicMask |= (int)chosenRelic;
			yield return Wait(0.25f);
			if (!RelicLibrary.GetRelicData(actualRelic))
			{
				Debug.Log("ActualRelic: " + actualRelic.ToString() + ", ChosenRelic: " + chosenRelic);
			}
			Vector2 position = new Vector2(base.EnemyController.transform.position.x, base.EnemyController.VisualBounds.max.y - 1f);
			(TextPopupManager.DisplayLocIDText(TextPopupType.TraitorRelic, GetRelicLocID(chosenRelic), StringGenderType.Male, position) as TraitorRelic_TextPopup).RelicType = actualRelic;
			switch (chosenRelic)
			{
			case TraitorRelicType.Dash:
				AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Jonah/sfx_jonah_vanguard_equip", base.EnemyController.transform.position);
				break;
			case TraitorRelicType.Jump:
				AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Jonah/sfx_jonah_marble_equip", base.EnemyController.transform.position);
				break;
			case TraitorRelicType.Block:
				AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Jonah/sfx_jonah_coesShell_equip", base.EnemyController.transform.position);
				break;
			case TraitorRelicType.DamageZone:
				AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Jonah/sfx_jonah_voltaicCrown_equip", base.EnemyController.transform.position);
				break;
			}
			if (m_relicModeShift_TellHold_Delay > 0f)
			{
				yield return Wait(m_relicModeShift_TellHold_Delay);
			}
			base.EnemyController.Animator.SetBool("Victory", value: false);
			m_isModeShifting = false;
			switch (chosenRelic)
			{
			case TraitorRelicType.Block:
				base.EnemyController.StatusBarController.ApplyUIEffect(StatusBarEntryType.TraitorRelic_Block, 3, 0);
				break;
			case TraitorRelicType.DamageZone:
				StopDamageZoneCoroutine();
				m_damageZoneCoroutine = this.RunPersistentCoroutine(DamageZoneRelicCoroutine());
				break;
			default:
				base.EnemyController.StatusBarController.ApplyUIEffect(GetStatusBarIcon(chosenRelic));
				break;
			}
			base.EnemyController.StatusEffectController.ImmuneToAllStatusEffects = false;
			base.EnemyController.ResetBaseValues();
			yield return Default_Attack_Cooldown(m_relicModeShift_Exit_IdleDuration, m_relicModeShift_AttackCD);
		}
	}

	private IEnumerator DamageZoneRelicCoroutine()
	{
		while (true)
		{
			base.EnemyController.StatusBarController.ApplyUIEffect(StatusBarEntryType.TraitorRelic_DamageZone, 10f);
			float timer2 = Time.time + 10f;
			while (Time.time < timer2)
			{
				yield return null;
			}
			StopProjectile(ref m_damageZoneProjectile);
			m_damageZoneProjectile = this.FireProjectile("TraitorRelicDamageAuraProjectile", 1, matchFacing: false);
			timer2 = Time.time + 2f;
			while (Time.time < timer2)
			{
				yield return null;
			}
			StopProjectile(ref m_damageZoneProjectile);
		}
	}

	private void StopDamageZoneCoroutine()
	{
		this.StopPersistentCoroutine(m_damageZoneCoroutine);
		m_damageZoneCoroutine = null;
		StopProjectile(ref m_damageZoneProjectile);
	}

	public override IEnumerator DeathAnim()
	{
		if (!ChallengeManager.IsInChallenge)
		{
			SaveManager.PlayerSaveData.TimesBeatenTraitor++;
		}
		base.EnemyController.StatusBarController.StopAllUIEffects();
		StopDamageZoneCoroutine();
		base.Animator.SetBool("Walking", value: false);
		base.Animator.SetFloat("ySpeed", 0f);
		base.Animator.SetBool("Stunned", value: true);
		base.Animator.Play("Empty", DefaultAnimationLayer);
		DefaultAnimationLayer = 0;
		this.ChangeAnimationState("Stunned");
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Jonah/sfx_jonah_death_hit", base.EnemyController.transform.position);
		yield return base.DeathAnim();
		RLTimeScale.SetTimeScale(TimeScaleType.Cutscene, 0.5f);
		if (m_death_Intro_Delay > 0f)
		{
			yield return Wait(m_death_Intro_Delay, unscaledTime: true);
		}
		while (!base.EnemyController.ControllerCorgi.State.IsGrounded)
		{
			yield return null;
		}
		base.Animator.SetBool("Stunned", value: false);
		if (MusicManager.CurrentMusicInstance.isValid())
		{
			RuntimeManager.StudioSystem.setParameterByName("bossEncounterProgress_cain", 1f);
		}
		RLTimeScale.SetTimeScale(TimeScaleType.Cutscene, 1f);
		int num = UnityEngine.Random.Range(0, 12);
		while (num == 9 || num == 8)
		{
			num = UnityEngine.Random.Range(0, 12);
		}
		yield return this.ChangeAnimationState("Death_" + num);
		yield return WaitUntilAnimComplete(DefaultAnimationLayer);
		DefaultAnimationLayer = 1;
		if (m_death_Hold_Delay > 0f)
		{
			yield return Wait(m_death_Hold_Delay, unscaledTime: true);
		}
	}

	public override IEnumerator SpawnAnim()
	{
		int storedAnimLayer = DefaultAnimationLayer;
		DefaultAnimationLayer = 0;
		yield return Default_Animation("Idle", m_spawn_Idle_AnimSpeed, m_spawn_Idle_Delay);
		base.EnemyController.Animator.SetTrigger("Bow");
		yield return Wait(3f);
		yield return Default_Animation("Idle", 1f, 0f, waitUntilAnimationComplete: false);
		DefaultAnimationLayer = storedAnimLayer;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (m_spoonThrowEventInstance.isValid())
		{
			AudioManager.Stop(m_spoonThrowEventInstance);
		}
	}
}
