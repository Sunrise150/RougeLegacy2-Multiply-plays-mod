using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using RLAudio;
using UnityEngine;
using UnityEngine.Events;

public class CaveBoss_Basic_AIScript : BaseAIScript
{
	private float m_topPoint;

	private float m_bottomPoint;

	private float m_leftPoint;

	private float m_rightPoint;

	private readonly EnemyType[] m_summonEnemyTypeArray = new EnemyType[4]
	{
		EnemyType.FlyingAxe,
		EnemyType.FlyingHammer,
		EnemyType.FlyingShield,
		EnemyType.FlyingSword
	};

	private const float SUMMON_ENEMY_HP_MOD = 0f;

	private BossModeShiftController m_modeShiftController;

	private bool m_isModeShifting;

	private GameObject m_regularWeaponHitbox;

	private GameObject m_jumpingWeaponHitbox;

	private Animator[] m_rootAnimatorsArray;

	private EventInstance m_fallingRubbleEventInstance;

	private EventInstance m_curseHandEventInstance;

	private EnemyModeShiftEventArgs m_modeShiftEventArgs;

	protected const string WALL_ATTACK_TELL_INTRO = "WallAttack_Tell_Intro";

	protected const string WALL_ATTACK_TELL_HOLD = "WallAttack_Tell_Hold";

	protected const string WALL_ATTACK_ATTACK_INTRO = "WallAttack_Attack_Intro";

	protected const string WALL_ATTACK_ATTACK_HOLD = "WallAttack_Attack_Hold";

	protected const string WALL_ATTACK_EXIT = "WallAttack_Exit";

	protected const string WALL_ATTACK_PROJECTILE = "CaveBossVoidWallProjectile";

	protected const string WALL_ATTACK_ADVANCED_PROJECTILE = "CaveBossVoidWallAdvancedProjectile";

	protected float m_wallAttack_TellIntro_AnimSpeed = 1.2f;

	protected float m_wallAttack_TellHold_AnimSpeed = 1.2f;

	protected float m_wallAttack_TellIntroAndHold_Delay = 0.9f;

	protected float m_wallAttack_AttackIntro_AnimSpeed = 1.2f;

	protected float m_wallAttack_AttackIntro_Delay;

	protected float m_wallAttack_AttackHold_AnimSpeed = 1.2f;

	protected float m_wallAttack_AttackHold_Delay = 0.45f;

	protected float m_wallAttack_Exit_AnimSpeed = 0.65f;

	protected float m_wallAttack_Exit_Delay = 0.45f;

	protected float m_wallAttack_Exit_IdleDuration = 0.15f;

	protected float m_wallAttack_AttackCD = 12f;

	protected float m_wallAttack_AttackExitDelay = 0.25f;

	protected float m_wallAttack_VariantAttackDelay = 0.5f;

	protected const string LINE_ATTACK_TELL_INTRO = "StreamProjectile_Tell_Intro";

	protected const string LINE_ATTACK_TELL_HOLD = "StreamProjectile_Tell_Hold";

	protected const string LINE_ATTACK_ATTACK_INTRO = "StreamProjectile_Attack_Intro";

	protected const string LINE_ATTACK_ATTACK_HOLD = "StreamProjectile_Attack_Hold";

	protected const string LINE_ATTACK_EXIT = "StreamProjectile_Exit";

	protected const string LINE_ATTACK_PROJECTILE = "CaveBossCurseProjectile";

	protected const string LINE_ATTACK_ADVANCED_PROJECTILE = "CaveBossCurseAdvancedProjectile";

	protected float m_lineAttack_TellIntro_AnimSpeed = 1.2f;

	protected float m_lineAttack_TellHold_AnimSpeed = 1.2f;

	protected float m_lineAttack_TellIntroAndHold_Delay = 1.1f;

	protected float m_lineAttack_AttackIntro_AnimSpeed = 1.2f;

	protected float m_lineAttack_AttackIntro_Delay;

	protected float m_lineAttack_AttackHold_AnimSpeed = 1.2f;

	protected float m_lineAttack_AttackHold_Delay;

	protected float m_lineAttack_Exit_AnimSpeed = 0.65f;

	protected float m_lineAttack_Exit_Delay = 0.55f;

	protected float m_lineAttack_Exit_IdleDuration = 0.15f;

	protected float m_lineAttack_AttackCD = 12f;

	protected float m_lineAttack_AttackExitDelay = 0.25f;

	protected const string HAMMER_ATTACK_TELL_INTRO = "HammerAttack_Tell_Intro";

	protected const string HAMMER_ATTACK_TELL_HOLD = "HammerAttack_Tell_Hold";

	protected const string HAMMER_ATTACK_ATTACK_INTRO = "HammerAttack_Attack_Intro";

	protected const string HAMMER_ATTACK_ATTACK_HOLD = "HammerAttack_Attack_Hold";

	protected const string HAMMER_ATTACK_EXIT = "HammerAttack_Exit";

	protected const string HAMMER_ATTACK_PROJECTILE = "CaveBossSlashProjectile";

	protected const string HAMMER_ATTACK_EXPLOSION_PROJECTILE = "CaveBossSlashExplosionProjectile";

	protected const string HAMMER_ATTACK_ROLLING_PROJECTILE = "CaveBossRollingProjectile";

	protected const string HAMMER_ATTACK_ROLLING_ADVANCED_PROJECTILE = "CaveBossRollingAdvancedProjectile";

	protected const string HAMMER_ATTACK_VARIANT_PROJECTILE = "CaveBossSlashFireballProjectile";

	protected float m_hammerAttack_TellIntro_AnimSpeed = 0.8f;

	protected float m_hammerAttack_TellHold_AnimSpeed = 0.8f;

	protected float m_hammerAttack_TellIntroAndHold_Delay = 0.95f;

	protected float m_hammerAttack_AttackIntro_AnimSpeed = 1f;

	protected float m_hammerAttack_AttackIntro_Delay;

	protected float m_hammerAttack_AttackHold_AnimSpeed = 1f;

	protected float m_hammerAttack_AttackHold_Delay;

	protected float m_hammerAttack_Exit_AnimSpeed = 0.65f;

	protected float m_hammerAttack_Exit_Delay = 0.45f;

	protected float m_hammerAttack_Exit_IdleDuration = 0.15f;

	protected float m_hammerAttack_AttackCD = 12f;

	protected float m_hammerAttack_AttackExitDelay = 0.25f;

	protected int m_hammerAttackVariantProjectileCount = 9;

	protected const string JUMP_ATTACK_TELL_INTRO = "JumpAttack_Tell_Intro";

	protected const string JUMP_ATTACK_TELL_HOLD = "JumpAttack_Tell_Hold";

	protected const string JUMP_ATTACK_JUMP_INTRO = "JumpAttack_Jump_Intro_Intro";

	protected const string JUMP_ATTACK_JUMP_HOLD = "JumpAttack_Jump_Hold";

	protected const string JUMP_ATTACK_FALL_INTRO = "JumpAttack_Fall_Intro";

	protected const string JUMP_ATTACK_FALL_HOLD = "JumpAttack_Fall_Hold";

	protected const string JUMP_ATTACK_LAND_INTRO = "JumpAttack_Land_Intro";

	protected const string JUMP_ATTACK_LAND_HOLD = "JumpAttack_Land_Hold";

	protected const string JUMP_ATTACK_EXIT = "JumpAttack_Exit";

	protected const string JUMP_ATTACK_MAGMA_PROJECTILE = "CaveBossFallingRubbleProjectile";

	protected const string JUMP_VARIANT_PROJECTILE = "CaveBossJumpExplosionProjectile";

	protected float m_jump_TellIntro_AnimSpeed = 0.8f;

	protected float m_jump_TellHold_AnimSpeed = 0.8f;

	protected float m_jump_TellIntroAndHold_Delay = 1f;

	protected float m_jumpAttack_TellIntro_AnimSpeed = 1f;

	protected float m_jumpAttack_TellHold_AnimSpeed = 1f;

	protected float m_jumpAttack_TellIntroAndHold_Delay;

	protected float m_jumpAttack_AttackIntro_AnimSpeed = 1f;

	protected float m_jumpAttack_AttackHold_AnimSpeed = 1f;

	protected float m_jumpAttack_Fall_Intro_AnimSpeed = 1f;

	protected float m_jumpAttack_Fall_Hold_AnimSpeed = 1f;

	protected float m_jumpAttack_Land_Intro_AnimSpeed = 1f;

	protected float m_jumpAttack_Land_Hold_AnimSpeed = 1f;

	protected float m_jumpAttack_LandHold_Delay = 1f;

	protected float m_jumpAttack_Exit_AnimSpeed = 0.65f;

	protected float m_jumpAttack_Exit_Delay = 0.45f;

	protected float m_jumpAttack_Exit_IdleDuration = 0.15f;

	protected float m_jumpAttack_AttackCD = 12f;

	protected float m_jumpAttack_AttackExitDelay = 0.25f;

	protected float m_jumpAttack_VariantAttackDelay = 0.5f;

	protected Vector2 m_jumpVelocity = new Vector2(10f, 32f);

	protected Vector2 m_jumpAttackMagmaBallSpawnOffset = new Vector2(-15f, 15f);

	protected const string SUMMON_ATTACK_TELL_INTRO = "SummonEnemy_Tell_Intro";

	protected const string SUMMON_ATTACK_TELL_HOLD = "SummonEnemy_Tell_Hold";

	protected const string SUMMON_ATTACK_ATTACK_INTRO = "SummonEnemy_Attack_Intro";

	protected const string SUMMON_ATTACK_ATTACK_HOLD = "SummonEnemy_Attack_Hold";

	protected const string SUMMON_ATTACK_EXIT = "SummonEnemy_Exit";

	protected float m_summonAttack_TellIntro_AnimSpeed = 0.6f;

	protected float m_summonAttack_TellHold_AnimSpeed = 0.6f;

	protected float m_summonAttack_TellIntroAndHold_Delay = 1.1f;

	protected float m_summonAttack_AttackIntro_AnimSpeed = 0.6f;

	protected float m_summonAttack_AttackIntro_Delay;

	protected float m_summonAttack_AttackHold_AnimSpeed = 1.2f;

	protected float m_summonAttack_AttackHold_Delay = 1.15f;

	protected float m_summonAttack_Exit_AnimSpeed = 0.65f;

	protected float m_summonAttack_Exit_Delay = 0.45f;

	protected float m_summonAttack_Exit_IdleDuration = 0.15f;

	protected float m_summonAttack_AttackCD = 25f;

	private Projectile_RL m_summonAttack_WarningProjectile;

	private Projectile_RL m_summonAttack_ShoutProjectile;

	private float m_requiredHPDropToSummon;

	private float HorizontalRoomIndent = 5f;

	private float VerticalRoomIndent = 5f;

	private bool m_summonsHaveFreeHitStatusEffect = true;

	private const float m_hpReducRequiredBeforeSummoning = 0.21f;

	private float m_summonFreeHitRegenerationAmount = 999f;

	protected const string CHASE_ATTACK_TELL_INTRO = "ChaserProjectile_Tell_Intro";

	protected const string CHASE_ATTACK_TELL_HOLD = "ChaserProjectile_Tell_Hold";

	protected const string CHASE_ATTACK_ATTACK_INTRO = "ChaserProjectile_Attack_Intro";

	protected const string CHASE_ATTACK_ATTACK_HOLD = "ChaserProjectile_Attack_Hold";

	protected const string CHASE_ATTACK_EXIT = "ChaserProjectile_Exit";

	protected const string CHASE_ATTACK_PROJECTILE = "CaveBossBombBoltProjectile";

	protected const string CHASE_ATTACK_FLOOR_WARNING_PROJECTILE = "CaveBossFloorWarningProjectile";

	protected const string CHASE_ATTACK_FLOOR_BOLT_PROJECTILE = "CaveBossFloorBoltProjectile";

	protected const string CHASE_ATTACK_VERTICAL_BOLT_PROJECTILE = "CaveBossVerticalBoltProjectile";

	protected float m_chaseAttack_TellIntro_AnimSpeed = 0.8f;

	protected float m_chaseAttack_TellHold_AnimSpeed = 0.8f;

	protected float m_chaseAttack_TellIntroAndHold_Delay = 1.25f;

	protected float m_chaseAttack_AttackIntro_AnimSpeed = 0.8f;

	protected float m_chaseAttack_AttackIntro_Delay;

	protected float m_chaseAttack_AttackHold_AnimSpeed = 0.8f;

	protected float m_chaseAttack_AttackHold_Delay = 1.25f;

	protected float m_chaseAttack_Exit_AnimSpeed = 0.65f;

	protected float m_chaseAttack_Exit_Delay = 0.45f;

	protected float m_chaseAttack_Exit_IdleDuration = 0.15f;

	protected float m_chaseAttack_AttackCD = 12f;

	private Projectile_RL m_chaseAttack_WarningProjectile;

	private Projectile_RL m_chaseAttack_BoltProjectile;

	private Projectile_RL m_chaseAttack_WarningProjectile2;

	private Projectile_RL m_chaseAttack_BoltProjectile2;

	protected const string WAVE_ATTACK_PROJECTILE = "CaveBossWaveProjectile";

	protected const string WAVE_ATTACK_SMALL_PROJECTILE = "CaveBossWaveSmallProjectile";

	protected const string WAVE_ATTACK_WARNING_PROJECTILE = "CaveBossWaveWarningProjectile";

	protected float m_waveAttackWarningDuration = 1.5f;

	protected float m_waveAttack_Initial_Delay = 1.5f;

	private Projectile_RL m_waveAttack_WarningProjectile;

	private Projectile_RL m_waveAttack_Projectile;

	private Projectile_RL m_waveAttack_WarningProjectile2;

	private Projectile_RL m_waveAttack_Projectile2;

	private Coroutine m_waveAttack_PersistentCoroutine;

	private Coroutine m_waveAttack_PersistentCoroutine2;

	private bool m_persistentWaveAttackRunning;

	private bool m_persistentWaveAttackRunning2;

	public UnityEvent ModeShiftEvent;

	private static List<EnemyType> m_enemySummonListHelper = new List<EnemyType>();

	protected const string MODESHIFT_DOWNED = "ModeShift_Intro";

	protected const string MODESHIFT_GETUP = "ModeShift_GetUp";

	protected const string MODESHIFT_ATTACK_INTRO = "ModeShift_Scream_Intro";

	protected const string MODESHIFT_ATTACK_HOLD = "ModeShift_Scream_Hold";

	protected const string MODESHIFT_EXIT = "ModeShift_Scream_Exit";

	protected const string MODESHIFT_PROJECTILE_WARNING = "CaveBossShoutWarningProjectile";

	protected const string MODESHIFT_PROJECTILE_ATTACK = "CaveBossShoutAttackProjectile";

	protected const string MODESHIFT_PROJECTILE_SUMMON = "CaveBossInfiniteChaseProjectile";

	protected const string MODESHIFT_PROJECTILE_SUMMON_VARIANT = "CaveBossInfiniteChaseProjectileVariant";

	protected const string MODESHIFT_PROJECTILE_STATIC_WALL = "CaveBossStaticWallProjectile";

	protected float m_modeShift_Downed_AnimSpeed = 0.8f;

	protected float m_modeShift_Downed_Delay = 1.75f;

	protected float m_modeShift_GetUp_AnimSpeed = 0.8f;

	protected float m_modeShift_GetUp_Delay = 1.5f;

	protected float m_modeShift_AttackIntro_AnimSpeed = 0.8f;

	protected float m_modeShift_AttackIntro_Delay;

	protected float m_modeShift_AttackHold_AnimSpeed = 0.8f;

	protected float m_modeShift_AttackHold_Delay;

	protected float m_modeShift_Exit_AnimSpeed = 0.65f;

	protected float m_modeShift_Exit_Delay;

	protected float m_modeShift_Exit_IdleDuration = 0.15f;

	protected float m_modeShift_AttackCD = 99f;

	protected float m_modeShift_AttackExitDelay = 0.5f;

	protected int m_modeShiftIndex;

	protected float[] m_modeShiftLevels_Basic = new float[2] { 0.7f, 0.35f };

	protected float[] m_modeShiftLevels_Advanced = new float[3] { 0.75f, 0.5f, 0.25f };

	protected float m_modeShift_Damage_Mod = 0.1f;

	protected bool m_canSummonDuplicates;

	private Projectile_RL m_modeShiftWarningProjectile;

	private Projectile_RL m_modeShiftShoutProjectile;

	protected const string SPAWN_IDLE = "Intro_Idle";

	protected const string SPAWN_INTRO = "Intro";

	protected float m_spawn_Idle_AnimSpeed = 1f;

	protected float m_spawn_Idle_Delay = 2f;

	protected float m_spawn_Intro_AnimSpeed = 1f;

	protected float m_spawn_Intro_Delay;

	protected const string DEATH_INTRO = "Death_Intro";

	protected const string DEATH_HOLD = "Death_Loop";

	protected float m_death_Intro_AnimSpeed = 1f;

	protected float m_death_Intro_Delay;

	protected float m_death_Hold_AnimSpeed = 1f;

	protected float m_death_Hold_Delay = 4.5f;

	protected override Vector2 IdleDuration => new Vector2(0.25f, 0.75f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.75f, 1.15f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.75f, 1.15f);

	protected virtual Vector2 m_lineAttackAngleOffset => new Vector2(-50f, 50f);

	protected virtual float m_lineAttackDuration => 1f;

	protected virtual int m_lineAttackCount => 5;

	protected virtual float m_hammerAttackDashSpeed => 15f;

	protected virtual float m_hammerAttackDashDuration => 0.125f;

	protected virtual int m_jumpAttack_NumMagmaBalls => 6;

	protected virtual float m_jumpAttackMagmaBallDuration => 6f;

	protected virtual int m_enemiesSummoned => 3;

	protected virtual int m_maxNumberofEnemiesAllowed => 3;

	protected virtual int m_summonFreeHitStatusEffectCount => 4;

	protected virtual int m_enemyInvulnTimerDuration => 7;

	protected Vector2 m_waveAttackRandomDelay => new Vector2(11f, 14f);

	protected Vector2 m_waveAttackRandomPosOffset => new Vector2(-1f, 1f);

	protected Vector2 m_waveAttackRandomAngle => new Vector2(-10f, 10f);

	protected virtual float m_thrustFast_DashAttackAmount => 0f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[23]
		{
			"CaveBossVoidWallProjectile", "CaveBossVoidWallAdvancedProjectile", "CaveBossCurseProjectile", "CaveBossCurseAdvancedProjectile", "CaveBossSlashProjectile", "CaveBossSlashExplosionProjectile", "CaveBossSlashFireballProjectile", "CaveBossRollingProjectile", "CaveBossRollingAdvancedProjectile", "CaveBossFallingRubbleProjectile",
			"CaveBossJumpExplosionProjectile", "CaveBossBombBoltProjectile", "CaveBossShoutWarningProjectile", "CaveBossShoutAttackProjectile", "CaveBossInfiniteChaseProjectile", "CaveBossInfiniteChaseProjectileVariant", "CaveBossStaticWallProjectile", "CaveBossFloorWarningProjectile", "CaveBossFloorBoltProjectile", "CaveBossVerticalBoltProjectile",
			"CaveBossWaveProjectile", "CaveBossWaveSmallProjectile", "CaveBossWaveWarningProjectile"
		};
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		base.EnemyController.HealthChangeRelay.AddListener(OnBossHit);
		m_modeShiftController = base.EnemyController.GetComponent<BossModeShiftController>();
		GameObject[] weaponHitboxList = (base.EnemyController.HitboxController as HitboxController).WeaponHitboxList;
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
		m_regularWeaponHitbox.SetActive(value: true);
		m_jumpingWeaponHitbox.SetActive(value: false);
		m_rootAnimatorsArray = base.EnemyController.Visuals.GetComponentsInChildren<Animator>();
		base.EnemyController.Animator.SetBool("HammerFire", value: false);
		base.LogicController.DisableLogicActivationByDistance = true;
		m_fallingRubbleEventInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/sfx_tubalBoss_fallingSwords_initiate_loop", base.transform);
		m_curseHandEventInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/sfx_tubalBoss_curseHand_start_loop", base.transform);
	}

	private void OnEnable()
	{
		if (base.IsInitialized)
		{
			base.EnemyController.Animator.SetBool("HammerFire", value: false);
		}
	}

	public override void OnEnemyActivated()
	{
		base.OnEnemyActivated();
		Vector3 center = base.EnemyController.Room.Bounds.center;
		center.x -= 5f;
		center.y -= 5f;
		LayerMask layerMask = 256;
		layerMask = (int)layerMask | 0x800;
		m_topPoint = Physics2D.Raycast(center, Vector2.up, 100f, layerMask).point.y;
		m_bottomPoint = Physics2D.Raycast(center, Vector2.down, 100f, layerMask).point.y;
		m_leftPoint = Physics2D.Raycast(center, Vector2.left, 100f, layerMask).point.x;
		m_rightPoint = Physics2D.Raycast(center, Vector2.right, 100f, layerMask).point.x;
	}

	private void OnBossHit(object sender, HealthChangeEventArgs args)
	{
		if (m_isModeShifting || (base.LogicController.EnemyLogicType == EnemyLogicType.Basic && m_modeShiftIndex >= m_modeShiftLevels_Basic.Length) || (base.LogicController.EnemyLogicType == EnemyLogicType.Advanced && m_modeShiftIndex >= m_modeShiftLevels_Advanced.Length) || base.EnemyController.IsDead || args.PrevHealthValue <= args.NewHealthValue)
		{
			return;
		}
		float num = 0f;
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Basic)
		{
			num = m_modeShiftLevels_Basic[m_modeShiftIndex] * (float)base.EnemyController.ActualMaxHealth;
		}
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Advanced)
		{
			num = m_modeShiftLevels_Advanced[m_modeShiftIndex] * (float)base.EnemyController.ActualMaxHealth;
		}
		if (base.EnemyController.CurrentHealth <= num)
		{
			m_modeShiftIndex++;
			base.LogicController.StopAllLogic(resetCurrentState: true);
			base.LogicController.ForceExecuteLogicBlockName_OnceOnly = "Mode_Shift";
			m_modeShiftController.ChangeMaterials();
			if (m_modeShiftEventArgs == null)
			{
				m_modeShiftEventArgs = new EnemyModeShiftEventArgs(base.EnemyController);
			}
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.EnemyModeShift, this, m_modeShiftEventArgs);
		}
	}

	public override void ResetScript()
	{
		base.ResetScript();
		m_isModeShifting = false;
		m_modeShiftIndex = 0;
		m_requiredHPDropToSummon = 0f;
		base.EnemyController.StatusEffectController.ImmuneToAllStatusEffects = false;
		base.LogicController.DisableLogicActivationByDistance = true;
		StopWaveProjectilePersistentCoroutine();
		StopWaveProjectilePersistent2Coroutine();
	}

	protected override void OnDisable()
	{
		AudioManager.Stop(m_fallingRubbleEventInstance);
		AudioManager.Stop(m_curseHandEventInstance);
	}

	private void OnDestroy()
	{
		if ((bool)base.EnemyController)
		{
			base.EnemyController.HealthChangeRelay.RemoveListener(OnBossHit);
		}
		if (m_fallingRubbleEventInstance.isValid())
		{
			m_fallingRubbleEventInstance.release();
		}
		if (m_curseHandEventInstance.isValid())
		{
			m_curseHandEventInstance.release();
		}
	}

	public override void OnLBCompleteOrCancelled()
	{
		base.OnLBCompleteOrCancelled();
		base.EnemyController.StatusEffectController.RemoveStatusEffectImmunity(StatusEffectType.Enemy_Freeze);
		StopProjectile(ref m_modeShiftWarningProjectile);
		StopProjectile(ref m_modeShiftShoutProjectile);
		StopProjectile(ref m_chaseAttack_WarningProjectile);
		StopProjectile(ref m_chaseAttack_BoltProjectile);
		StopProjectile(ref m_chaseAttack_BoltProjectile2);
		StopProjectile(ref m_summonAttack_WarningProjectile);
		StopProjectile(ref m_summonAttack_ShoutProjectile);
		m_regularWeaponHitbox.SetActive(value: true);
		m_jumpingWeaponHitbox.SetActive(value: false);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Wall_Attack()
	{
		this.ToDo("Wall_Attack");
		SetVelocityX(0f, additive: false);
		base.EnemyController.LockFlip = true;
		yield return Default_TellIntroAndLoop("WallAttack_Tell_Intro", m_wallAttack_TellIntro_AnimSpeed, "WallAttack_Tell_Hold", m_wallAttack_TellHold_AnimSpeed, m_wallAttack_TellIntroAndHold_Delay);
		yield return Default_Animation("WallAttack_Attack_Intro", m_wallAttack_AttackIntro_AnimSpeed, m_wallAttack_AttackIntro_Delay);
		if (base.EnemyController.EnemyRank == EnemyRank.Basic)
		{
			this.FireProjectile("CaveBossVoidWallProjectile", 0, matchFacing: false);
			Projectile_RL projectile_RL = this.FireProjectile("CaveBossVoidWallProjectile", 0, matchFacing: false, 180f);
			projectile_RL.transform.SetLocalScaleX(0f - projectile_RL.transform.localScale.x);
		}
		else if (base.EnemyController.EnemyRank == EnemyRank.Advanced)
		{
			yield return Wait(m_wallAttack_VariantAttackDelay);
			this.FireProjectile("CaveBossVoidWallAdvancedProjectile", 0, matchFacing: false);
			Projectile_RL projectile_RL2 = this.FireProjectile("CaveBossVoidWallAdvancedProjectile", 0, matchFacing: false, 180f);
			projectile_RL2.transform.SetLocalScaleX(0f - projectile_RL2.transform.localScale.x);
		}
		yield return Default_Animation("WallAttack_Attack_Hold", m_wallAttack_AttackHold_AnimSpeed, m_wallAttack_AttackHold_Delay, waitUntilAnimationComplete: false);
		yield return Wait(m_wallAttack_AttackExitDelay);
		yield return Default_Animation("WallAttack_Exit", m_wallAttack_Exit_AnimSpeed, m_wallAttack_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_wallAttack_Exit_IdleDuration, m_wallAttack_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Line_Attack()
	{
		this.ToDo("Line_Attack");
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		AudioManager.PlayAttached(this, m_curseHandEventInstance, base.gameObject);
		yield return Default_TellIntroAndLoop("StreamProjectile_Tell_Intro", m_lineAttack_TellIntro_AnimSpeed, "StreamProjectile_Tell_Hold", m_lineAttack_TellHold_AnimSpeed, m_lineAttack_TellIntroAndHold_Delay);
		yield return Default_Animation("StreamProjectile_Attack_Intro", m_lineAttack_AttackIntro_AnimSpeed, m_lineAttack_AttackIntro_Delay);
		yield return Default_Animation("StreamProjectile_Attack_Hold", m_lineAttack_AttackHold_AnimSpeed, m_lineAttack_AttackHold_Delay, waitUntilAnimationComplete: false);
		float attackIntervalDelay = m_lineAttackDuration / (float)m_lineAttackCount;
		for (int i = 0; i < m_lineAttackCount; i++)
		{
			float angle = Random.Range(m_lineAttackAngleOffset.x, m_lineAttackAngleOffset.y);
			if (base.EnemyController.EnemyRank == EnemyRank.Basic)
			{
				this.FireProjectile("CaveBossCurseProjectile", 8, matchFacing: true, angle);
			}
			else if (base.EnemyController.EnemyRank == EnemyRank.Advanced)
			{
				this.FireProjectile("CaveBossCurseAdvancedProjectile", 8, matchFacing: true, angle);
			}
			yield return Wait(attackIntervalDelay);
		}
		yield return Wait(m_lineAttack_AttackExitDelay);
		AudioManager.Stop(m_curseHandEventInstance);
		yield return Default_Animation("StreamProjectile_Exit", m_lineAttack_Exit_AnimSpeed, m_lineAttack_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_lineAttack_Exit_IdleDuration, m_lineAttack_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Hammer_Attack()
	{
		this.ToDo("Hammer_Attack");
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		float dashSpeed = m_hammerAttackDashSpeed;
		if (!base.EnemyController.IsTargetToMyRight)
		{
			dashSpeed = 0f - dashSpeed;
		}
		yield return Default_TellIntroAndLoop("HammerAttack_Tell_Intro", m_hammerAttack_TellIntro_AnimSpeed, "HammerAttack_Tell_Hold", m_hammerAttack_TellHold_AnimSpeed, m_hammerAttack_TellIntroAndHold_Delay);
		yield return Default_Animation("HammerAttack_Attack_Intro", m_hammerAttack_AttackIntro_AnimSpeed, m_hammerAttack_AttackIntro_Delay);
		this.FireProjectile("CaveBossSlashProjectile", 14);
		SetAttackingWithContactDamage(enable: true, 0f);
		yield return Default_Animation("HammerAttack_Attack_Hold", m_hammerAttack_AttackHold_AnimSpeed, 0f, waitUntilAnimationComplete: false);
		SetVelocityX(dashSpeed, additive: false);
		base.EnemyController.GroundHorizontalVelocity = dashSpeed;
		base.EnemyController.DisableFriction = true;
		yield return Wait(0.1f);
		this.FireProjectile("CaveBossSlashExplosionProjectile", 15);
		if (base.EnemyController.EnemyRank == EnemyRank.Basic)
		{
			this.FireProjectile("CaveBossRollingProjectile", 3, matchFacing: false).Flip();
			this.FireProjectile("CaveBossRollingProjectile", 3, matchFacing: false, 180f);
		}
		else if (base.EnemyController.EnemyRank == EnemyRank.Advanced)
		{
			this.FireProjectile("CaveBossRollingAdvancedProjectile", 3, matchFacing: false).Flip();
			this.FireProjectile("CaveBossRollingAdvancedProjectile", 3, matchFacing: false, 180f);
			this.FireProjectile("CaveBossSlashFireballProjectile", 15, matchFacing: false, 60f);
			this.FireProjectile("CaveBossSlashFireballProjectile", 15, matchFacing: false, 75f);
			this.FireProjectile("CaveBossSlashFireballProjectile", 15, matchFacing: false, 90f);
			this.FireProjectile("CaveBossSlashFireballProjectile", 15, matchFacing: false, 105f);
			this.FireProjectile("CaveBossSlashFireballProjectile", 15, matchFacing: false, 120f);
		}
		yield return Wait(m_hammerAttackDashDuration);
		yield return WaitUntilIsGrounded();
		SetVelocityX(0f, additive: false);
		base.EnemyController.GroundHorizontalVelocity = 0f;
		base.EnemyController.DisableFriction = false;
		SetAttackingWithContactDamage(enable: false);
		yield return Wait(m_hammerAttack_AttackExitDelay);
		yield return Default_Animation("HammerAttack_Exit", m_hammerAttack_Exit_AnimSpeed, m_hammerAttack_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_hammerAttack_Exit_IdleDuration, m_hammerAttack_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Jump_Attack()
	{
		this.ToDo("Jump_Attack");
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		yield return Default_TellIntroAndLoop("JumpAttack_Tell_Intro", m_jump_TellIntro_AnimSpeed, "JumpAttack_Tell_Hold", m_jump_TellHold_AnimSpeed, m_jump_TellIntroAndHold_Delay);
		Vector2 jumpVelocity = m_jumpVelocity;
		if (!base.EnemyController.IsFacingRight)
		{
			jumpVelocity.x = 0f - jumpVelocity.x;
		}
		SetVelocity(jumpVelocity, additive: false);
		SetAttackingWithContactDamage(enable: true, 0f);
		yield return Default_Animation("JumpAttack_Jump_Intro_Intro", m_jumpAttack_AttackIntro_AnimSpeed, 0f);
		yield return Default_Animation("JumpAttack_Jump_Hold", m_jumpAttack_AttackHold_AnimSpeed, 0f, waitUntilAnimationComplete: false);
		yield return Wait(0.05f);
		m_regularWeaponHitbox.SetActive(value: false);
		m_jumpingWeaponHitbox.SetActive(value: true);
		while (base.EnemyController.Velocity.y >= 0f)
		{
			yield return null;
		}
		yield return Default_Animation("JumpAttack_Fall_Intro", m_jumpAttack_AttackIntro_AnimSpeed, 0f);
		yield return Default_Animation("JumpAttack_Fall_Hold", m_jumpAttack_AttackHold_AnimSpeed, 0f, waitUntilAnimationComplete: false);
		yield return WaitUntilIsGrounded();
		SetVelocityX(0f, additive: false);
		this.RunPersistentCoroutine(DropMagmaBalls());
		if (base.EnemyController.EnemyRank == EnemyRank.Advanced)
		{
			this.FireProjectile("CaveBossJumpExplosionProjectile", 0, matchFacing: false);
		}
		yield return Default_Animation("JumpAttack_Land_Intro", m_jumpAttack_AttackIntro_AnimSpeed, 0f);
		m_regularWeaponHitbox.SetActive(value: true);
		m_jumpingWeaponHitbox.SetActive(value: false);
		yield return Default_Animation("JumpAttack_Land_Hold", m_jumpAttack_AttackHold_AnimSpeed, m_jumpAttack_LandHold_Delay);
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("JumpAttack_Exit", m_jumpAttack_Exit_AnimSpeed, m_jumpAttack_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_jumpAttack_Exit_IdleDuration, m_jumpAttack_AttackCD);
	}

	private IEnumerator DropMagmaBalls()
	{
		AudioManager.Play(this, m_fallingRubbleEventInstance);
		Projectile_RL lastProjectile = null;
		float magmaBallInterval = m_jumpAttackMagmaBallDuration / (float)m_jumpAttack_NumMagmaBalls;
		float yPos = base.EnemyController.Room.Bounds.max.y + 4f;
		float yPos2 = base.EnemyController.Room.Bounds.max.y + 9f;
		float yPos3 = base.EnemyController.Room.Bounds.max.y + 14f;
		for (int i = 0; i < m_jumpAttack_NumMagmaBalls; i++)
		{
			float delayTime2 = Time.time + magmaBallInterval;
			float num = Random.Range(m_jumpAttackMagmaBallSpawnOffset.x, m_jumpAttackMagmaBallSpawnOffset.y);
			float x = PlayerManager.GetPlayerController().transform.localPosition.x + num;
			this.FireProjectileAbsPos("CaveBossFallingRubbleProjectile", new Vector2(x, yPos), matchFacing: false, -90f);
			num = Random.Range(m_jumpAttackMagmaBallSpawnOffset.x, m_jumpAttackMagmaBallSpawnOffset.y);
			x = PlayerManager.GetPlayerController().transform.localPosition.x + num * 2f;
			this.FireProjectileAbsPos("CaveBossFallingRubbleProjectile", new Vector2(x, yPos2), matchFacing: false, -90f);
			num = Random.Range(m_jumpAttackMagmaBallSpawnOffset.x, m_jumpAttackMagmaBallSpawnOffset.y);
			x = PlayerManager.GetPlayerController().transform.localPosition.x + num * 3f;
			lastProjectile = this.FireProjectileAbsPos("CaveBossFallingRubbleProjectile", new Vector2(x, yPos3), matchFacing: false, -90f);
			while (Time.time < delayTime2)
			{
				yield return null;
			}
		}
		if ((bool)lastProjectile)
		{
			float delayTime2 = Time.time + 2f;
			while (Time.time < delayTime2)
			{
				yield return null;
			}
			AudioManager.Stop(m_fallingRubbleEventInstance);
		}
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Summon_Attack()
	{
		if (EnemyManager.NumActiveSummonedEnemies >= 2)
		{
			yield break;
		}
		if (m_requiredHPDropToSummon > 0f)
		{
			if (base.EnemyController.CurrentHealth > m_requiredHPDropToSummon)
			{
				yield break;
			}
		}
		else if (m_requiredHPDropToSummon < 0f)
		{
			yield break;
		}
		base.EnemyController.StatusEffectController.AddStatusEffectImmunity(StatusEffectType.Enemy_Freeze);
		this.ToDo("Summon_Attack");
		SetVelocityX(0f, additive: false);
		base.EnemyController.LockFlip = true;
		m_summonAttack_WarningProjectile = this.FireProjectile("CaveBossShoutWarningProjectile", 7);
		yield return Default_TellIntroAndLoop("SummonEnemy_Tell_Intro", m_summonAttack_TellIntro_AnimSpeed, "SummonEnemy_Tell_Hold", m_summonAttack_TellHold_AnimSpeed, m_summonAttack_TellIntroAndHold_Delay);
		StopProjectile(ref m_summonAttack_WarningProjectile);
		yield return Default_Animation("SummonEnemy_Attack_Intro", m_summonAttack_AttackIntro_AnimSpeed, m_summonAttack_AttackIntro_Delay);
		yield return Default_Animation("SummonEnemy_Attack_Hold", m_summonAttack_AttackHold_AnimSpeed, 0f, waitUntilAnimationComplete: false);
		yield return Wait(0.15f);
		m_summonAttack_ShoutProjectile = this.FireProjectile("CaveBossShoutAttackProjectile", 7);
		int num = Random.Range(0, m_summonEnemyTypeArray.Length);
		EnemyType enemyType = m_summonEnemyTypeArray[num];
		EnemyRank rank = EnemyRank.Advanced;
		if (base.EnemyController.EnemyRank == EnemyRank.Advanced)
		{
			rank = EnemyRank.Advanced;
		}
		Bounds bounds = base.EnemyController.Room.Bounds;
		new Vector2(Random.Range(bounds.min.x + HorizontalRoomIndent, bounds.max.x - HorizontalRoomIndent), Random.Range(bounds.min.y + VerticalRoomIndent, bounds.max.y - VerticalRoomIndent));
		for (int i = 0; i < m_enemiesSummoned; i++)
		{
			Vector2 spawnOffset = new Vector2(Random.Range(bounds.min.x + HorizontalRoomIndent, bounds.max.x - HorizontalRoomIndent), Random.Range(bounds.min.y + VerticalRoomIndent, bounds.max.y - VerticalRoomIndent));
			this.RunPersistentCoroutine(AddStatusEffectSummonCoroutine(enemyType, rank, spawnOffset));
		}
		m_requiredHPDropToSummon = base.EnemyController.CurrentHealth - (float)base.EnemyController.ActualMaxHealth * 0.21f;
		yield return Wait(m_summonAttack_AttackHold_Delay);
		StopProjectile(ref m_summonAttack_ShoutProjectile);
		yield return Default_Animation("SummonEnemy_Exit", m_summonAttack_Exit_AnimSpeed, m_summonAttack_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_summonAttack_Exit_IdleDuration, m_summonAttack_AttackCD);
	}

	private IEnumerator AddStatusEffectSummonCoroutine(EnemyType enemyType, EnemyRank rank, Vector2 spawnOffset)
	{
		SummonEnemyController component = base.EnemyController.gameObject.GetComponent<SummonEnemyController>();
		EnemyController enemyToSummon = null;
		if ((bool)component)
		{
			if (component.Contains(enemyType, rank))
			{
				enemyToSummon = EnemyManager.SummonEnemy(base.EnemyController, enemyType, rank, spawnOffset, useAbsPos: true, runSummonAnim: false);
			}
			else
			{
				Debug.Log("Could not summon enemy: " + enemyType.ToString() + " - " + rank.ToString() + ". Enemy not found in SummonEnemyController.");
			}
		}
		if ((bool)enemyToSummon)
		{
			yield return EnemyManager.RunSummonAnimCoroutine(enemyToSummon, 1f, disableEnemyOffset: false, 0f);
			if (m_summonsHaveFreeHitStatusEffect && !enemyToSummon.IsDead)
			{
				EnemyFreeHitStatusEffect obj = enemyToSummon.StatusEffectController.GetStatusEffect(StatusEffectType.Enemy_FreeHit) as EnemyFreeHitStatusEffect;
				obj.FreeHitCountOverride = m_summonFreeHitStatusEffectCount;
				obj.FreeHitRegenerationOverride = m_summonFreeHitRegenerationAmount;
				enemyToSummon.StatusEffectController.StartStatusEffect(StatusEffectType.Enemy_FreeHit, 0f, null);
				obj.FreeHitCountOverride = -1;
				obj.FreeHitRegenerationOverride = -1f;
				enemyToSummon.StatusEffectController.StartStatusEffect(StatusEffectType.Enemy_InvulnTimer, m_enemyInvulnTimerDuration, null);
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
	public IEnumerator Chase_Attack()
	{
		this.ToDo("Chase_Attack");
		SetVelocityX(0f, additive: false);
		base.EnemyController.LockFlip = true;
		m_chaseAttack_WarningProjectile = this.FireProjectileAbsPos("CaveBossFloorWarningProjectile", new Vector2(m_leftPoint, m_bottomPoint), matchFacing: false);
		yield return Default_TellIntroAndLoop("ChaserProjectile_Tell_Intro", m_chaseAttack_TellIntro_AnimSpeed, "ChaserProjectile_Tell_Hold", m_chaseAttack_TellHold_AnimSpeed, m_chaseAttack_TellIntroAndHold_Delay);
		yield return Default_Animation("ChaserProjectile_Attack_Intro", m_chaseAttack_AttackIntro_AnimSpeed, m_chaseAttack_AttackIntro_Delay);
		StopProjectile(ref m_chaseAttack_WarningProjectile);
		m_chaseAttack_BoltProjectile = this.FireProjectileAbsPos("CaveBossFloorBoltProjectile", new Vector2(m_leftPoint, m_bottomPoint), matchFacing: false);
		this.FireProjectileAbsPos("CaveBossVerticalBoltProjectile", base.EnemyController.transform.position, matchFacing: false);
		yield return Default_Animation("ChaserProjectile_Attack_Hold", m_chaseAttack_AttackHold_AnimSpeed, m_chaseAttack_AttackHold_Delay);
		StopProjectile(ref m_chaseAttack_BoltProjectile);
		yield return Default_Animation("ChaserProjectile_Exit", m_chaseAttack_Exit_AnimSpeed, m_chaseAttack_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_chaseAttack_Exit_IdleDuration, m_chaseAttack_AttackCD);
	}

	public IEnumerator Wave_Attack_PersistentCoroutine(bool isSecond)
	{
		if (!isSecond)
		{
			m_persistentWaveAttackRunning = true;
		}
		else
		{
			m_persistentWaveAttackRunning2 = true;
		}
		float delay = Time.time + m_waveAttack_Initial_Delay;
		while (true)
		{
			if (Time.time < delay)
			{
				yield return null;
				continue;
			}
			yield return FireWaveProjectile(isSecond);
			delay = Time.time + Random.Range(m_waveAttackRandomDelay.x, m_waveAttackRandomDelay.y);
		}
	}

	private IEnumerator FireWaveProjectile(bool isSecond)
	{
		Vector2 startingPos = base.EnemyController.TargetController.Midpoint;
		int num = CDGHelper.RandomPlusMinus();
		int num2 = CDGHelper.RandomPlusMinus();
		float num3 = Random.Range(m_waveAttackRandomPosOffset.x, m_waveAttackRandomPosOffset.y);
		float startingAngle3;
		if (num > 0)
		{
			if (num2 > 0)
			{
				startingAngle3 = 0f;
				startingPos.x = base.EnemyController.Room.BoundsRect.xMin - 1f;
			}
			else
			{
				startingAngle3 = 180f;
				startingPos.x = base.EnemyController.Room.BoundsRect.xMax + 1f;
			}
			startingPos.y += num3;
		}
		else
		{
			if (num2 > 0)
			{
				startingAngle3 = -90f;
				startingPos.y = base.EnemyController.Room.BoundsRect.yMax + 1f;
			}
			else
			{
				startingAngle3 = 90f;
				startingPos.y = base.EnemyController.Room.BoundsRect.yMin - 1f;
			}
			startingPos.x += num3;
		}
		float num4 = Random.Range(m_waveAttackRandomAngle.x, m_waveAttackRandomAngle.y);
		startingAngle3 += num4;
		startingAngle3 = CDGHelper.WrapAngleDegrees(startingAngle3, convertToHalfRadius: true);
		if (!isSecond)
		{
			m_waveAttack_WarningProjectile = this.FireProjectileAbsPos("CaveBossWaveWarningProjectile", startingPos, matchFacing: false, startingAngle3);
		}
		else
		{
			m_waveAttack_WarningProjectile2 = this.FireProjectileAbsPos("CaveBossWaveWarningProjectile", startingPos, matchFacing: false, startingAngle3);
		}
		float delay = Time.time + m_waveAttackWarningDuration;
		while (Time.time < delay)
		{
			yield return null;
		}
		if (!isSecond)
		{
			StopProjectile(ref m_waveAttack_WarningProjectile);
			m_waveAttack_Projectile = this.FireProjectileAbsPos("CaveBossWaveProjectile", startingPos, matchFacing: false, startingAngle3);
		}
		else
		{
			StopProjectile(ref m_waveAttack_WarningProjectile2);
			m_waveAttack_Projectile2 = this.FireProjectileAbsPos("CaveBossWaveProjectile", startingPos, matchFacing: false, startingAngle3);
		}
	}

	private void StopWaveProjectilePersistentCoroutine()
	{
		this.StopPersistentCoroutine(m_waveAttack_PersistentCoroutine);
		StopProjectile(ref m_waveAttack_Projectile);
		StopProjectile(ref m_waveAttack_WarningProjectile);
		m_persistentWaveAttackRunning = false;
	}

	private void StopWaveProjectilePersistent2Coroutine()
	{
		this.StopPersistentCoroutine(m_waveAttack_PersistentCoroutine2);
		StopProjectile(ref m_waveAttack_Projectile2);
		StopProjectile(ref m_waveAttack_WarningProjectile2);
		m_persistentWaveAttackRunning2 = false;
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Mode_Shift()
	{
		m_isModeShifting = true;
		RemoveStatusEffects(includeCommanderBuffs: false);
		base.EnemyController.StatusEffectController.ImmuneToAllStatusEffects = true;
		yield return base.DeathAnim();
		this.ToDo("Mode_Shift " + m_modeShiftIndex);
		base.EnemyController.LockFlip = true;
		SetVelocityX(0f, additive: false);
		base.EnemyController.ModeshiftDamageMod = m_modeShift_Damage_Mod;
		if (m_modeShiftIndex == 1)
		{
			MusicManager.SetBossEncounterParam(0.34f);
		}
		else if (m_modeShiftIndex == 2)
		{
			MusicManager.SetBossEncounterParam(0.67f);
		}
		yield return Default_Animation("ModeShift_Intro", m_modeShift_Downed_AnimSpeed, m_modeShift_Downed_Delay);
		yield return Default_Animation("ModeShift_GetUp", m_modeShift_GetUp_AnimSpeed, 0f);
		if (ModeShiftEvent != null)
		{
			ModeShiftEvent.Invoke();
		}
		m_modeShiftWarningProjectile = this.FireProjectile("CaveBossShoutWarningProjectile", 7);
		if (m_modeShift_GetUp_Delay > 0f)
		{
			yield return Wait(m_modeShift_GetUp_Delay);
		}
		yield return Default_Animation("ModeShift_Scream_Intro", m_modeShift_AttackIntro_AnimSpeed, m_modeShift_AttackIntro_Delay);
		StopProjectile(ref m_modeShiftWarningProjectile);
		yield return Default_Animation("ModeShift_Scream_Hold", m_modeShift_AttackHold_AnimSpeed, m_modeShift_AttackHold_Delay, waitUntilAnimationComplete: false);
		m_modeShiftShoutProjectile = this.FireProjectile("CaveBossShoutAttackProjectile", 7);
		if (base.EnemyController.EnemyRank == EnemyRank.Basic)
		{
			if (m_modeShiftIndex == 1)
			{
				this.FireProjectile("CaveBossInfiniteChaseProjectile", 7, matchFacing: true, 90f);
			}
			else if (m_modeShiftIndex == 2)
			{
				float x = base.EnemyController.Room.Bounds.min.x + 2.5f;
				float num = base.EnemyController.Room.Bounds.min.y + 3f;
				this.FireProjectileAbsPos("CaveBossStaticWallProjectile", new Vector2(x, num + 0f), matchFacing: false);
				this.FireProjectileAbsPos("CaveBossStaticWallProjectile", new Vector2(x, num + 11f), matchFacing: false);
				this.FireProjectileAbsPos("CaveBossStaticWallProjectile", new Vector2(x, num + 22f), matchFacing: false);
				x = base.EnemyController.Room.Bounds.max.x - 3.25f;
				this.FireProjectileAbsPos("CaveBossStaticWallProjectile", new Vector2(x, num + 0f), matchFacing: false, 180f);
				this.FireProjectileAbsPos("CaveBossStaticWallProjectile", new Vector2(x, num + 11f), matchFacing: false, 180f);
				this.FireProjectileAbsPos("CaveBossStaticWallProjectile", new Vector2(x, num + 22f), matchFacing: false, 180f);
			}
		}
		else if (base.EnemyController.EnemyRank == EnemyRank.Advanced)
		{
			if (m_modeShiftIndex >= 1 && !m_persistentWaveAttackRunning && base.EnemyController.EnemyRank == EnemyRank.Advanced)
			{
				this.FireProjectile("CaveBossWaveSmallProjectile", 7);
				this.FireProjectile("CaveBossWaveSmallProjectile", 7, matchFacing: true, 45f);
				this.FireProjectile("CaveBossWaveSmallProjectile", 7, matchFacing: true, 90f);
				this.FireProjectile("CaveBossWaveSmallProjectile", 7, matchFacing: true, 135f);
				this.FireProjectile("CaveBossWaveSmallProjectile", 7, matchFacing: true, 180f);
				this.FireProjectile("CaveBossWaveSmallProjectile", 7, matchFacing: true, 225f);
				this.FireProjectile("CaveBossWaveSmallProjectile", 7, matchFacing: true, 270f);
				this.FireProjectile("CaveBossWaveSmallProjectile", 7, matchFacing: true, 315f);
				m_waveAttack_PersistentCoroutine = this.RunPersistentCoroutine(Wave_Attack_PersistentCoroutine(isSecond: false));
			}
			if (m_modeShiftIndex == 2)
			{
				_ = base.EnemyController.Room.Bounds.min.x + 3.25f;
				_ = base.EnemyController.Room.Bounds.min.y + 3f;
				_ = base.EnemyController.Room.Bounds.max.x - 3.25f;
				_ = base.EnemyController.Room.Bounds.min.x + 28f;
				float y = 8.25f;
				float x2 = base.EnemyController.Room.Bounds.min.x + 3.25f;
				this.FireProjectileAbsPos("CaveBossStaticWallProjectile", new Vector2(x2, y), matchFacing: false, 90f);
				x2 = base.EnemyController.Room.Bounds.max.x - 3.25f;
				this.FireProjectileAbsPos("CaveBossStaticWallProjectile", new Vector2(x2, y), matchFacing: false, 90f);
			}
			if (m_modeShiftIndex >= 3 && !m_persistentWaveAttackRunning2 && base.EnemyController.EnemyRank == EnemyRank.Advanced)
			{
				this.FireProjectile("CaveBossWaveSmallProjectile", 7);
				this.FireProjectile("CaveBossWaveSmallProjectile", 7, matchFacing: true, 45f);
				this.FireProjectile("CaveBossWaveSmallProjectile", 7, matchFacing: true, 90f);
				this.FireProjectile("CaveBossWaveSmallProjectile", 7, matchFacing: true, 135f);
				this.FireProjectile("CaveBossWaveSmallProjectile", 7, matchFacing: true, 185f);
				this.FireProjectile("CaveBossWaveSmallProjectile", 7, matchFacing: true, 225f);
				this.FireProjectile("CaveBossWaveSmallProjectile", 7, matchFacing: true, 270f);
				this.FireProjectile("CaveBossWaveSmallProjectile", 7, matchFacing: true, 315f);
				m_waveAttack_PersistentCoroutine2 = this.RunPersistentCoroutine(Wave_Attack_PersistentCoroutine(isSecond: true));
			}
		}
		yield return Wait(m_modeShift_AttackExitDelay);
		StopProjectile(ref m_modeShiftShoutProjectile);
		yield return Default_Animation("ModeShift_Scream_Exit", m_modeShift_Exit_AnimSpeed, m_modeShift_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ModeshiftDamageMod = 1f;
		base.EnemyController.ResetBaseValues();
		base.EnemyController.StatusEffectController.ImmuneToAllStatusEffects = false;
		yield return Default_Attack_Cooldown(m_modeShift_Exit_IdleDuration, m_modeShift_AttackCD);
		m_isModeShifting = false;
	}

	public override IEnumerator SpawnAnim()
	{
		yield return Default_Animation("Intro_Idle", m_spawn_Idle_AnimSpeed, m_spawn_Idle_Delay);
		this.SetAnimationSpeedMultiplier(m_spawn_Intro_AnimSpeed);
		yield return this.ChangeAnimationState("Intro");
		while (base.EnemyController.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.15f)
		{
			yield return null;
		}
		MusicManager.PlayMusic(SongID.CaveBossBGM_Tettix_Blacksmith_Boss, isOverride: false);
		int trigger = Animator.StringToHash("Retract");
		Animator[] rootAnimatorsArray = m_rootAnimatorsArray;
		for (int i = 0; i < rootAnimatorsArray.Length; i++)
		{
			rootAnimatorsArray[i].SetTrigger(trigger);
		}
		base.EnemyController.Animator.SetBool("HammerFire", value: true);
		yield return WaitUntilAnimComplete(DefaultAnimationLayer);
		yield return Default_Animation(BaseAIScript.NEUTRAL_STATE, 1f, 1f, waitUntilAnimationComplete: false);
	}

	public override IEnumerator DeathAnim()
	{
		yield return base.DeathAnim();
		EnemyManager.KillAllSummonedEnemies();
		RLTimeScale.SetTimeScale(TimeScaleType.Cutscene, 0.5f);
		AudioManager.PlayOneShot(this, "event:/SFX/Enemies/sfx_tubalBoss_deathSequence", base.gameObject.transform.position);
		yield return Default_Animation("Death_Intro", m_death_Intro_AnimSpeed, m_death_Intro_Delay);
		RLTimeScale.SetTimeScale(TimeScaleType.Cutscene, 1f);
		base.EnemyController.GetComponent<BossModeShiftController>().DissolveMaterials(m_death_Hold_Delay);
		yield return Default_Animation("Death_Loop", m_death_Hold_AnimSpeed, m_death_Hold_Delay);
	}
}
