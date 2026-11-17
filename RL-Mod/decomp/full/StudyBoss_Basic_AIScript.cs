using System;
using System.Collections;
using FMOD.Studio;
using RLAudio;
using Sigtrap.Relays;
using UnityEngine;

public class StudyBoss_Basic_AIScript : BaseAIScript
{
	private EnemyModeShiftEventArgs m_modeShiftEventArgs;

	private Action<object, HealthChangeEventArgs> m_onBossHit;

	private const float m_global_AnimationOverride = 1f;

	private bool m_hasVoidCircleClosedIn;

	private const string VERTICAL_BEAM_TELL_INTRO = "SummonVerticalBeam_Tell_Intro";

	private const string VERTICAL_BEAM_TELL_HOLD = "SummonVerticalBeam_Tell_Hold";

	private const string VERTICAL_BEAM_ATTACK_INTRO = "SummonVerticalBeam_Attack_Intro";

	private const string VERTICAL_BEAM_ATTACK_HOLD = "SummonVerticalBeam_Attack_Hold";

	private const string VERTICAL_BEAM_EXIT = "SummonVerticalBeam_Exit";

	private const string VERTICAL_BEAM_PROJECTILE_NAME = "StudyBossVerticalBeamProjectile";

	private const float VERTICAL_BEAM_OFFSET = 1f;

	private const string VERTICAL_BEAM_WARNING_PROJECTILE_NAME = "StudyBossVerticalBeamWarningProjectile";

	private float m_verticalBeam_TellIntro_AnimSpeed = 1f;

	private float m_verticalBeam_TellHold_AnimSpeed = 1f;

	private float m_verticalBeam_TellIntroAndHold_Delay = 1.75f;

	private float m_verticalBeam_AttackIntro_AnimSpeed = 1f;

	private float m_verticalBeam_AttackIntro_Delay;

	private float m_verticalBeam_AttackHold_AnimSpeed = 1f;

	private float m_verticalBeam_AttackHold_Delay = 1f;

	private float m_verticalBeam_Exit_AnimSpeed = 1f;

	private float m_verticalBeam_Exit_Delay = 0.45f;

	private float m_verticalBeam_Exit_IdleDuration = 0.15f;

	private float m_verticalBeam_AttackCD = 18f;

	private bool m_verticalBeam_stopMovingWhileAttacking = true;

	private Projectile_RL[] m_verticalBeamWarningProjectiles;

	private Projectile_RL[] m_verticalBeamProjectiles;

	private bool m_verticalBeamAudioPlaying;

	private WaitForSeconds m_warningWaitTime;

	private WaitForSeconds m_beamWaitTime;

	private const string RAPID_FIRE_TELL_INTRO = "EyeballShot_Tell_Intro";

	private const string RAPID_FIRE_TELL_HOLD = "EyeballShot_Tell_Hold";

	private const string RAPID_FIRE_ATTACK_INTRO = "EyeballShot_Attack_Intro";

	private const string RAPID_FIRE_ATTACK_HOLD = "EyeballShot_Attack_Hold";

	private const string RAPID_FIRE_EXIT = "EyeballShot_Exit";

	private const string RAPID_FIRE_PROJECTILE_NAME = "StudyBossRapidFireProjectile";

	private const string RAPID_FIRE_VOID_PROJECTILE_NAME = "StudyBossRapidFireVoidProjectile";

	private float m_rapidFire_TellIntro_AnimSpeed = 1f;

	private float m_rapidFire_TellHold_AnimSpeed = 1f;

	private float m_rapidFire_TellIntroAndHold_Delay = 1.5f;

	private float m_rapidFire_AttackIntro_AnimSpeed = 1f;

	private float m_rapidFire_AttackIntro_Delay;

	private float m_rapidFire_AttackHold_AnimSpeed = 1f;

	private float m_rapidFire_AttackHold_Delay = 3.25f;

	private float m_rapidFire_Exit_AnimSpeed = 1f;

	private float m_rapidFire_Exit_Delay = 0.45f;

	private float m_rapidFire_Exit_IdleDuration = 0.15f;

	private float m_rapidFire_AttackCD = 15f;

	private float m_rapidFire_MaxRandomAngleDeviation = 60f;

	private float m_rapidFire_SpeedMod = 1f;

	private float m_rapidFire_VoidProjectileSpeedMod = 1f;

	private float m_rapidFire_VoidShotDelay = 0.15f;

	private float m_rapidFire_VoidShotAdvancedDelay = 0.25f;

	private bool m_rapidFire_stopMovingWhileAttacking = true;

	private const string CIRCLE_ATTACK_TELL_INTRO = "SummonCircularBullets_Tell_Intro";

	private const string CIRCLE_ATTACK_TELL_HOLD = "SummonCircularBullets_Tell_Hold";

	private const string CIRCLE_ATTACK_ATTACK_INTRO = "SummonCircularBullets_Attack_Intro";

	private const string CIRCLE_ATTACK_ATTACK_HOLD = "SummonCircularBullets_Attack_Hold";

	private const string CIRCLE_ATTACK_EXIT = "SummonCircularBullets_Exit";

	private const string CIRCLE_ATTACK_PROJECTILE_NAME = "StudyBossCircleProjectile";

	private float m_circleAttack_TellIntro_AnimSpeed = 1f;

	private float m_circleAttack_TellHold_AnimSpeed = 1f;

	private float m_circleAttack_TellIntroAndHold_Delay = 1.5f;

	private float m_circleAttack_AttackIntro_AnimSpeed = 1f;

	private float m_circleAttack_AttackIntro_Delay;

	private float m_circleAttack_AttackHold_AnimSpeed = 1f;

	private float m_circleAttack_AttackHold_Delay = 1.25f;

	private float m_circleAttack_Exit_AnimSpeed = 1f;

	private float m_circleAttack_Exit_Delay = 0.45f;

	private float m_circleAttack_Exit_IdleDuration = 0.15f;

	private float m_circleAttack_AttackCD = 12f;

	private bool m_circleAttack_stopMovingWhileAttacking;

	private const string BOMB_ATTACK_TELL_INTRO = "SummonBomb_Tell_Intro";

	private const string BOMB_ATTACK_TELL_HOLD = "SummonBomb_Tell_Hold";

	private const string BOMB_ATTACK_ATTACK_INTRO = "SummonBomb_Attack_Intro";

	private const string BOMB_ATTACK_ATTACK_HOLD = "SummonBomb_Attack_Hold";

	private const string BOMB_ATTACK_EXIT = "SummonBomb_Exit";

	private float m_bombAttack_TellIntro_AnimSpeed = 1f;

	private float m_bombAttack_TellHold_AnimSpeed = 1f;

	private float m_bombAttack_TellIntroAndHold_Delay = 2.25f;

	private float m_bombAttack_AttackIntro_AnimSpeed = 1f;

	private float m_bombAttack_AttackIntro_Delay;

	private float m_bombAttack_AttackHold_AnimSpeed = 1f;

	private float m_bombAttack_AttackHold_Delay;

	private float m_bombAttack_Exit_AnimSpeed = 0.65f;

	private float m_bombAttack_Exit_Delay = 0.45f;

	private float m_bombAttack_Exit_IdleDuration = 0.15f;

	private float m_bombAttack_AttackCD = 12f;

	private float m_bombAttack_TimeToExplosion;

	private bool m_bombAttack_stopMovingWhileAttacking = true;

	private float m_bombAttack_Displace_X = 10f;

	private float m_bombAttack_Displace_Y = 3.75f;

	private Projectile_RL m_bombWarningProjectile;

	private Projectile_RL m_bombWarningProjectile2;

	private Projectile_RL m_bombWarningProjectile3;

	private Projectile_RL m_bombWarningProjectile4;

	private Projectile_RL m_bombWarningProjectile5;

	private Relay m_bombAttackWarningAppearedRelay = new Relay();

	private Relay m_bombAttackExplodedRelay = new Relay();

	private const string SHOUT_TELL_INTRO = "Shout_Tell_Intro";

	private const string SHOUT_TELL_HOLD = "Shout_Tell_Hold";

	private const string SHOUT_ATTACK_INTRO = "Shout_Attack_Intro";

	private const string SHOUT_ATTACK_HOLD = "Shout_Attack_Hold";

	private const string SHOUT_EXIT = "Shout_Exit";

	private const string SHOUT_ATTACK_EXPLOSION_PROJECTILE_NAME = "StudyBossShoutExplosionProjectile";

	private const string SHOUT_ATTACK_WARNING_PROJECTILE_NAME = "StudyBossShoutWarningProjectile";

	private float m_shout_TellIntro_AnimSpeed = 1f;

	private float m_shout_TellHold_AnimSpeed = 1f;

	private float m_shout_TellIntroAndHold_Delay = 1.5f;

	private float m_shout_AttackIntro_AnimSpeed = 1f;

	private float m_shout_AttackIntro_Delay;

	private float m_shout_AttackHold_AnimSpeed = 1f;

	private float m_shout_AttackHold_Delay;

	private float m_shout_Exit_AnimSpeed = 0.65f;

	private float m_shout_Exit_Delay;

	private float m_shout_Exit_IdleDuration = 0.35f;

	private float m_shout_AttackCD = 12f;

	private float m_shoutAttackTimeToExplosion;

	private bool m_shout_stopMovingWhileAttacking = true;

	private float m_shoutAttackDelaySecondExplosion = 0.65f;

	private int m_shoutAttackDelaySecondExplosionAngleAdd = 10;

	private Projectile_RL m_shoutWarningProjectile;

	private Relay m_shoutAttackWarningAppearedRelay = new Relay();

	private Relay m_shoutAttackExplodedRelay = new Relay();

	private const string DASH_ATTACK_TELL_INTRO = "Dash_Tell_Intro";

	private const string DASH_ATTACK_TELL_HOLD = "Dash_Tell_Hold";

	private const string DASH_ATTACK_ATTACK_INTRO = "Dash_Attack_Intro";

	private const string DASH_ATTACK_ATTACK_HOLD = "Dash_Attack_Hold";

	private const string DASH_ATTACK_EXIT = "Dash_Exit";

	private const string CURSE_ATTACK_RED_PROJECTILE_NAME = "StudyBossFlowerProjectile";

	private float m_dashAttack_TellIntro_AnimSpeed = 1f;

	private float m_dashAttack_TellHold_AnimSpeed = 1f;

	private float m_dashAttack_TellIntroAndHold_Delay = 0.9f;

	private float m_dashAttack_AttackIntro_AnimSpeed = 1f;

	private float m_dashAttack_AttackIntro_Delay;

	private float m_dashAttack_AttackHold_AnimSpeed = 1f;

	private float m_dashAttack_AttackHold_Delay = 1f;

	private float m_dashAttack_Exit_AnimSpeed = 1f;

	private float m_dashAttack_Exit_Delay = 0.45f;

	private float m_dashAttack_Exit_IdleDuration = 0.15f;

	private float m_dashAttack_AttackCD = 12f;

	private float m_dashAttackSpeed = 16.5f;

	private float m_dashAttackFireballSpeedMod = 1f;

	private const string SHIELD_ATTACK_PROJECTILE_NAME = "StudyBossShieldProjectile";

	private float m_shieldAttackRadius = 13.5f;

	private float m_shieldAttackOrbitSpeed = 25f;

	private float m_shieldAttackTimeBetweenProjectileRespawn = 5f;

	private bool m_isShieldAttackRunning;

	private const string MODE_SHIFT_INTRO = "ModeShift_Intro";

	private const string MODE_SHIFT_SCREAM_INTRO = "ModeShift_Scream_Intro";

	private const string MODE_SHIFT_SCREAM_HOLD = "ModeShift_Scream_Hold";

	private const string MODE_SHIFT_EXIT = "ModeShift_Scream_Exit";

	private float m_modeShift_Intro_AnimSpeed = 1f;

	private float m_modeShift_Intro_Delay = 1.5f;

	private float m_modeShift_Scream_Intro_AnimSpeed = 1f;

	private float m_modeShift_Scream_Intro_Delay;

	private float m_modeShift_Scream_Hold_AnimSpeed = 1f;

	private float m_modeShift_Scream_Hold_Delay = 1.25f;

	private float m_modeShift_Exit_AnimSpeed = 1f;

	private float m_modeShift_Exit_Delay = 0.5f;

	private float m_modeShift_IdleDuration = 1f;

	private float m_modeShift_AttackCD = 99999f;

	private float m_modeShift_HealthMod = 0.5f;

	private float m_modeShift_DamageMod = 0.1f;

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

	private const string PHASE2_HIT_AUDIO_PATH = "event:/SFX/Enemies/sfx_studyBoss_phase2_hit_start";

	private const string FLYING_AUDIO_PATH = "event:/SFX/Enemies/sfx_studyBoss_fly_loop";

	private const string FLYING_SPEED_PARAMETER = "dancingBoss_flySpeed";

	private const string IDLE_AUDIO_PATH = "event:/SFX/Enemies/sfx_studyBoss_idle_loop";

	private const string VERTICAL_BEAM_LOOP_AUDIO_PATH = "event:/SFX/Enemies/sfx_studyBoss_attack_skyBeam_spawn_start_loop";

	private const string VERTICAL_BEAM_DESPAWN_AUDIO_PATH = "event:/SFX/Enemies/sfx_studyBoss_attack_skyBeam_despawn";

	private const string VOID_CIRCLE_CLOSING_AUDIO_PATH = "event:/SFX/Enemies/sfx_studyBoss_attack_voidCircle_closeIn";

	private const string EYEBALL_SHOT_LOOP_AUDIO_PATH = "event:/SFX/Enemies/sfx_studyBoss_attack_eye_shoot_start_loop";

	private const string SHIELD_ATTACK_LOOP_AUDIO_PATH = "event:/SFX/Enemies/sfx_studyBoss_phase2_voidProjectiles_loop";

	private const float MAX_FLYING_SPEED = 5f;

	private bool m_wasFlying;

	private EventInstance m_flyingAudioEventInstance;

	private EventInstance m_idleAudioEventInstance;

	private EventInstance m_verticalBeamLoopAudioEventInstance;

	private EventInstance m_verticalBeamDespawnAudioEventInstance;

	private EventInstance m_voidCircleClosingAudioEventInstance;

	private EventInstance m_eyeballShotLoopAudioEventInstance;

	private EventInstance m_shieldAttackLoopAudioEventInstance;

	private bool m_rapidFireAudioPlaying;

	protected override Vector2 IdleDuration => new Vector2(0.4f, 0.8f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.4f, 0.8f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.4f, 0.8f);

	protected override Vector2 RandomFollowOffsetX => new Vector2(-2f, 2f);

	protected override Vector2 RandomFollowOffsetY => new Vector2(3f, 3f);

	protected virtual bool m_advancedAttack => false;

	protected virtual bool is_Variant => false;

	protected virtual string CURSE_ATTACK_PROJECTILE_NAME => "StudyBossCurseProjectile";

	protected virtual string CURSE_ATTACK_ADVANCED_PROJECTILE_NAME => "StudyBossCurseAdvancedProjectile";

	protected virtual int m_verticalBeamAttackCount => 1;

	protected virtual float m_timeBetweenVerticalBeamAttacks => 0.75f;

	protected virtual float m_verticalBeamWarningLifetime => 1.5f;

	protected virtual float m_verticalBeamLifetime => 8f;

	protected virtual int m_rapidFireShotCount => 35;

	protected virtual bool m_rapidFire_ShootVoidBulletAtEnd => true;

	protected virtual int m_circleAttackProjectileCount => 7;

	protected virtual float m_circleAttackInitialRadius => 8f;

	protected virtual float m_circleAttackInnerRadius => 2.5f;

	protected virtual float m_circleAttackTimeBeforeCircleBeginsToClose => 2f;

	protected virtual float m_circleAttackTimeToClose => 1f;

	protected virtual float m_circleAttackOrbitSpeed => 360f;

	protected virtual string BOMB_ATTACK_EXPLOSION_PROJECTILE_NAME => "StudyBossBombExplosionProjectile";

	protected virtual string BOMB_ATTACK_EXPLOSION_ADVANCED_PROJECTILE_NAME => "StudyBossBombExplosionAdvancedProjectile";

	protected virtual string BOMB_ATTACK_WARNING_PROJECTILE_NAME => "StudyBossBombWarningProjectile";

	protected virtual string BOMB_ATTACK_WARNING_ADVANCED_PROJECTILE_NAME => "StudyBossBombWarningAdvancedProjectile";

	protected virtual string BOMB_ATTACK_FIREBALL_PROJECTILE_NAME => "StudyBossBombFireballProjectile";

	protected virtual int m_bombAttack_FireballCountAtEnd => 1;

	public IRelayLink BombAttackWarningAppearedRelay => m_bombAttackWarningAppearedRelay.link;

	public IRelayLink BombAttackExplodedRelay => m_bombAttackExplodedRelay.link;

	protected virtual int m_shout_fireballsAtEnd => 18;

	public IRelayLink ShoutAttackWarningAppearedRelay => m_shoutAttackWarningAppearedRelay.link;

	public IRelayLink ShoutAttackExplodedRelay => m_shoutAttackExplodedRelay.link;

	protected virtual int m_dashAttackFireballCountAtEnd => 0;

	protected virtual int m_shieldAttackProjectileCount => 4;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[16]
		{
			"StudyBossVerticalBeamWarningProjectile", "StudyBossVerticalBeamProjectile", "StudyBossRapidFireProjectile", "StudyBossRapidFireVoidProjectile", "StudyBossCircleProjectile", BOMB_ATTACK_EXPLOSION_PROJECTILE_NAME, BOMB_ATTACK_EXPLOSION_ADVANCED_PROJECTILE_NAME, BOMB_ATTACK_WARNING_PROJECTILE_NAME, BOMB_ATTACK_WARNING_ADVANCED_PROJECTILE_NAME, BOMB_ATTACK_FIREBALL_PROJECTILE_NAME,
			"StudyBossShoutExplosionProjectile", "StudyBossShoutWarningProjectile", "StudyBossShieldProjectile", CURSE_ATTACK_PROJECTILE_NAME, CURSE_ATTACK_ADVANCED_PROJECTILE_NAME, "StudyBossFlowerProjectile"
		};
	}

	private void Awake()
	{
		m_onBossHit = OnBossHit;
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		base.LogicController.DisableLogicActivationByDistance = true;
		m_verticalBeamWarningProjectiles = new Projectile_RL[m_verticalBeamAttackCount];
		m_verticalBeamProjectiles = new Projectile_RL[m_verticalBeamAttackCount];
		InitializeAudio();
	}

	public override void OnEnemyActivated()
	{
		base.OnEnemyActivated();
		base.EnemyController.HealthChangeRelay.AddListener(m_onBossHit);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator VerticalBeamAttack()
	{
		this.ToDo("VERTICAL BEAM ATTACK");
		if (m_verticalBeam_stopMovingWhileAttacking)
		{
			base.EnemyController.FlyingMovementType = FlyingMovementType.Override;
			StopAndFaceTarget();
			base.EnemyController.LockFlip = true;
		}
		yield return Default_TellIntroAndLoop("SummonVerticalBeam_Tell_Intro", m_verticalBeam_TellIntro_AnimSpeed, "SummonVerticalBeam_Tell_Hold", m_verticalBeam_TellHold_AnimSpeed, m_verticalBeam_TellIntroAndHold_Delay);
		yield return Default_Animation("SummonVerticalBeam_Attack_Intro", m_verticalBeam_AttackIntro_AnimSpeed, m_verticalBeam_AttackIntro_Delay);
		this.SetAnimationSpeedMultiplier(m_verticalBeam_AttackHold_AnimSpeed);
		yield return this.ChangeAnimationState("SummonVerticalBeam_Attack_Hold");
		for (int i = 0; i < m_verticalBeamAttackCount; i++)
		{
			this.RunPersistentCoroutine(FireVerticalBeamWarningAndAttack(i));
			if (m_timeBetweenVerticalBeamAttacks > 0f)
			{
				yield return Wait(m_timeBetweenVerticalBeamAttacks);
			}
		}
		yield return Wait(m_verticalBeam_AttackHold_Delay);
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("SummonVerticalBeam_Exit", m_verticalBeam_Exit_AnimSpeed, m_verticalBeam_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		yield return Default_Attack_Cooldown(m_verticalBeam_Exit_IdleDuration, m_verticalBeam_AttackCD);
	}

	private IEnumerator FireVerticalBeamWarningAndAttack(int index)
	{
		if (m_warningWaitTime == null)
		{
			m_warningWaitTime = new WaitForSeconds(m_verticalBeamWarningLifetime);
			m_beamWaitTime = new WaitForSeconds(m_verticalBeamLifetime);
		}
		Vector2 projectileOffset = GetVerticalBeamProjectileOffset();
		BeginVerticalBeamAudio();
		StopProjectile(ref m_verticalBeamWarningProjectiles[index]);
		m_verticalBeamWarningProjectiles[index] = this.FireProjectileAbsPos("StudyBossVerticalBeamWarningProjectile", projectileOffset, matchFacing: false, 270f);
		yield return m_warningWaitTime;
		StopProjectile(ref m_verticalBeamWarningProjectiles[index]);
		StopProjectile(ref m_verticalBeamProjectiles[index]);
		m_verticalBeamProjectiles[index] = this.FireProjectileAbsPos("StudyBossVerticalBeamProjectile", projectileOffset, matchFacing: false, 270f);
		yield return m_beamWaitTime;
		StopProjectile(ref m_verticalBeamProjectiles[index]);
		EndVerticalBeamAudio();
	}

	private Vector2 GetVerticalBeamProjectileOffset()
	{
		_ = Vector2.zero;
		return GetVerticalBeamOrigin();
	}

	private Vector2 GetVerticalBeamOrigin()
	{
		Vector2 vector = PlayerManager.GetPlayerController().Midpoint;
		float y = (base.EnemyController.Room as Room).Bounds.max.y + 1f;
		return new Vector2(vector.x, y);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator RapidFireAttack()
	{
		this.ToDo("RAPID FIRE ATTACK");
		if (m_rapidFire_stopMovingWhileAttacking)
		{
			StopAndFaceTarget();
			base.EnemyController.LockFlip = true;
		}
		yield return Default_TellIntroAndLoop("EyeballShot_Tell_Intro", m_rapidFire_TellIntro_AnimSpeed, "EyeballShot_Tell_Hold", m_rapidFire_TellHold_AnimSpeed, m_rapidFire_TellIntroAndHold_Delay);
		BeginRapidFireAttackAudio();
		yield return Default_Animation("EyeballShot_Attack_Intro", m_rapidFire_AttackIntro_AnimSpeed, m_rapidFire_AttackIntro_Delay);
		this.SetAnimationSpeedMultiplier(m_rapidFire_AttackHold_AnimSpeed);
		yield return this.ChangeAnimationState("EyeballShot_Attack_Hold");
		float fireInterval = m_rapidFire_AttackHold_Delay / (float)m_rapidFireShotCount;
		for (int j = 0; j < m_rapidFireShotCount; j++)
		{
			float randomRapidFireAngle = GetRandomRapidFireAngle();
			this.FireProjectile("StudyBossRapidFireProjectile", 2, matchFacing: false, randomRapidFireAngle, m_rapidFire_SpeedMod);
			if (fireInterval > 0f)
			{
				yield return Wait(fireInterval);
			}
		}
		if (m_rapidFire_ShootVoidBulletAtEnd)
		{
			float angle = GetRandomRapidFireAngle();
			if (m_rapidFire_VoidShotDelay > 0f)
			{
				yield return Wait(m_rapidFire_VoidShotDelay);
			}
			this.FireProjectile("StudyBossRapidFireVoidProjectile", Vector2.zero, matchFacing: false, angle, m_rapidFire_VoidProjectileSpeedMod);
			this.FireProjectile("StudyBossRapidFireVoidProjectile", Vector2.zero, matchFacing: false, angle - 25f, m_rapidFire_VoidProjectileSpeedMod);
			this.FireProjectile("StudyBossRapidFireVoidProjectile", Vector2.zero, matchFacing: false, angle - 50f, m_rapidFire_VoidProjectileSpeedMod);
			this.FireProjectile("StudyBossRapidFireVoidProjectile", Vector2.zero, matchFacing: false, angle - 75f, m_rapidFire_VoidProjectileSpeedMod);
			this.FireProjectile("StudyBossRapidFireVoidProjectile", Vector2.zero, matchFacing: false, angle + 25f, m_rapidFire_VoidProjectileSpeedMod);
			this.FireProjectile("StudyBossRapidFireVoidProjectile", Vector2.zero, matchFacing: false, angle + 50f, m_rapidFire_VoidProjectileSpeedMod);
			this.FireProjectile("StudyBossRapidFireVoidProjectile", Vector2.zero, matchFacing: false, angle + 75f, m_rapidFire_VoidProjectileSpeedMod);
			if (m_advancedAttack)
			{
				for (int j = 0; j < 3; j++)
				{
					if (m_rapidFire_VoidShotAdvancedDelay > 0f)
					{
						yield return Wait(m_rapidFire_VoidShotAdvancedDelay);
					}
					this.FireProjectile("StudyBossRapidFireVoidProjectile", Vector2.zero, matchFacing: false, angle, m_rapidFire_VoidProjectileSpeedMod);
					this.FireProjectile("StudyBossRapidFireVoidProjectile", Vector2.zero, matchFacing: false, angle - 25f, m_rapidFire_VoidProjectileSpeedMod);
					this.FireProjectile("StudyBossRapidFireVoidProjectile", Vector2.zero, matchFacing: false, angle - 50f, m_rapidFire_VoidProjectileSpeedMod);
					this.FireProjectile("StudyBossRapidFireVoidProjectile", Vector2.zero, matchFacing: false, angle - 75f, m_rapidFire_VoidProjectileSpeedMod);
					this.FireProjectile("StudyBossRapidFireVoidProjectile", Vector2.zero, matchFacing: false, angle + 25f, m_rapidFire_VoidProjectileSpeedMod);
					this.FireProjectile("StudyBossRapidFireVoidProjectile", Vector2.zero, matchFacing: false, angle + 50f, m_rapidFire_VoidProjectileSpeedMod);
					this.FireProjectile("StudyBossRapidFireVoidProjectile", Vector2.zero, matchFacing: false, angle + 75f, m_rapidFire_VoidProjectileSpeedMod);
				}
			}
		}
		SetAttackingWithContactDamage(enable: false);
		EndRapidFireAttackAudio();
		yield return Default_Animation("EyeballShot_Exit", m_rapidFire_Exit_AnimSpeed, m_rapidFire_Exit_Delay);
		base.EnemyController.LockFlip = false;
		yield return Default_Attack_Cooldown(m_rapidFire_Exit_IdleDuration, m_rapidFire_AttackCD);
	}

	private float GetRandomRapidFireAngle(bool disableRandom = false)
	{
		int num = UnityEngine.Random.Range(0, 2);
		int num2 = 1;
		if (num == 1)
		{
			num2 = -1;
		}
		float num3 = UnityEngine.Random.Range(0f, m_rapidFire_MaxRandomAngleDeviation);
		float num4 = CDGHelper.VectorToAngle(PlayerManager.GetPlayerController().Midpoint - base.EnemyController.Midpoint);
		if (!disableRandom)
		{
			num4 += (float)num2 * num3;
		}
		return num4;
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator CircleAttack()
	{
		this.ToDo("CIRCLE ATTACK");
		if (m_circleAttack_stopMovingWhileAttacking)
		{
			base.EnemyController.FlyingMovementType = FlyingMovementType.Override;
			StopAndFaceTarget();
			base.EnemyController.LockFlip = true;
		}
		yield return Default_TellIntroAndLoop("SummonCircularBullets_Tell_Intro", m_circleAttack_TellIntro_AnimSpeed, "SummonCircularBullets_Tell_Hold", m_circleAttack_TellHold_AnimSpeed, m_circleAttack_TellIntroAndHold_Delay);
		yield return Default_Animation("SummonCircularBullets_Attack_Intro", m_circleAttack_AttackIntro_AnimSpeed, m_circleAttack_AttackIntro_Delay);
		this.SetAnimationSpeedMultiplier(m_circleAttack_AttackHold_AnimSpeed);
		yield return this.ChangeAnimationState("SummonCircularBullets_Attack_Hold");
		SpawnOrbitingAttackProjectiles(m_circleAttackProjectileCount, OrbitProjectileAroundPlayer);
		if (m_advancedAttack)
		{
			yield return Wait(0.1f);
			SpawnOrbitingAttackProjectiles(m_circleAttackProjectileCount, OrbitProjectileAroundPlayer);
			yield return Wait(0.1f);
			SpawnOrbitingAttackProjectiles(m_circleAttackProjectileCount, OrbitProjectileAroundPlayer);
		}
		if (m_circleAttack_AttackHold_Delay > 0f)
		{
			yield return Wait(m_circleAttack_AttackHold_Delay);
		}
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("SummonCircularBullets_Exit", m_circleAttack_Exit_AnimSpeed, m_circleAttack_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		yield return Default_Attack_Cooldown(m_circleAttack_Exit_IdleDuration, m_circleAttack_AttackCD);
	}

	private void SpawnOrbitingAttackProjectiles(int projectileCount, Func<float, IEnumerator> orbitCoroutine)
	{
		float num = 360f / (float)projectileCount;
		float num2 = 0f;
		for (int i = 0; i < projectileCount; i++)
		{
			float arg = num2 + (float)i * num;
			this.RunPersistentCoroutine(orbitCoroutine(arg));
		}
		m_hasVoidCircleClosedIn = false;
	}

	private IEnumerator OrbitProjectileAroundPlayer(float initialAngle)
	{
		float num = 0f;
		Vector3 pivot = PlayerManager.GetPlayerController().Midpoint;
		Vector3 vector = GetCircleAttackProjectileOffset(pivot, initialAngle, m_circleAttackInitialRadius + num);
		Projectile_RL projectile = this.FireProjectile("StudyBossCircleProjectile", vector, matchFacing: false);
		float shrinkSpeed = (m_circleAttackInitialRadius + num) / m_circleAttackTimeToClose;
		float angle = initialAngle;
		float radius = m_circleAttackInitialRadius + num;
		float timeStart = Time.time;
		while (Time.time - timeStart < m_circleAttackTimeBeforeCircleBeginsToClose + m_circleAttackTimeToClose)
		{
			if (radius > m_circleAttackInnerRadius)
			{
				pivot = PlayerManager.GetPlayerController().Midpoint;
			}
			vector = GetCircleAttackProjectileOffset(pivot, angle, radius);
			projectile.transform.position = base.EnemyController.Midpoint + vector;
			if (Time.time - timeStart >= m_circleAttackTimeBeforeCircleBeginsToClose)
			{
				angle += m_circleAttackOrbitSpeed * Time.deltaTime;
				radius -= shrinkSpeed * Time.deltaTime;
				if (!m_hasVoidCircleClosedIn)
				{
					m_hasVoidCircleClosedIn = true;
					PlayCircleClosingAudio();
				}
			}
			yield return null;
		}
		StopProjectile(ref projectile);
	}

	private Vector2 GetCircleAttackProjectileOffset(Vector3 pivot, float angle, float currentRadius)
	{
		float f = (float)Math.PI / 180f * angle;
		float num = Mathf.Cos(f);
		float num2 = Mathf.Sin(f);
		Vector2 down = Vector2.down;
		float x = down.x * num - down.y * num2;
		float y = down.x * num2 + down.y * num;
		Vector2 vector = new Vector2(x, y) * currentRadius;
		Vector2 vector2 = pivot - base.EnemyController.Midpoint;
		return vector + vector2;
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator BombAttack()
	{
		this.ToDo("BOMB ATTACK");
		if (m_bombAttack_stopMovingWhileAttacking)
		{
			base.EnemyController.FlyingMovementType = FlyingMovementType.Override;
			SetVelocity(0f, 0f, additive: false);
			base.EnemyController.LockFlip = true;
		}
		string projectileName = BOMB_ATTACK_WARNING_PROJECTILE_NAME;
		if (m_advancedAttack)
		{
			projectileName = BOMB_ATTACK_WARNING_ADVANCED_PROJECTILE_NAME;
		}
		Vector2 vector;
		Vector2 offset5 = (vector = PlayerManager.GetPlayerController().Midpoint);
		Vector2 offset4 = (vector = vector);
		Vector2 offset3 = (vector = vector);
		Vector2 offset2 = (vector = vector);
		Vector2 offset1 = vector;
		float y = Die3Roll();
		offset1 += new Vector2((0f - m_bombAttack_Displace_X) * 2f, y);
		m_bombWarningProjectile = this.FireProjectileAbsPos(projectileName, offset1, matchFacing: false);
		y = Die3Roll();
		offset2 += new Vector2(0f, y);
		m_bombWarningProjectile2 = this.FireProjectileAbsPos(projectileName, offset2, matchFacing: false);
		y = Die3Roll();
		offset3 += new Vector2(0f - m_bombAttack_Displace_X, y);
		m_bombWarningProjectile3 = this.FireProjectileAbsPos(projectileName, offset3, matchFacing: false);
		y = Die3Roll();
		offset4 += new Vector2(m_bombAttack_Displace_X, y);
		m_bombWarningProjectile4 = this.FireProjectileAbsPos(projectileName, offset4, matchFacing: false);
		y = Die3Roll();
		offset5 += new Vector2(m_bombAttack_Displace_X * 2f, y);
		m_bombWarningProjectile5 = this.FireProjectileAbsPos(projectileName, offset5, matchFacing: false);
		m_bombAttackWarningAppearedRelay.Dispatch();
		yield return Default_TellIntroAndLoop("SummonBomb_Tell_Intro", m_bombAttack_TellIntro_AnimSpeed, "SummonBomb_Tell_Hold", m_bombAttack_TellHold_AnimSpeed, m_bombAttack_TellIntroAndHold_Delay);
		if (m_bombAttack_TimeToExplosion > 0f)
		{
			yield return Wait(m_bombAttack_TimeToExplosion);
		}
		StopProjectile(ref m_bombWarningProjectile);
		StopProjectile(ref m_bombWarningProjectile2);
		StopProjectile(ref m_bombWarningProjectile3);
		StopProjectile(ref m_bombWarningProjectile4);
		StopProjectile(ref m_bombWarningProjectile5);
		string projectileName2 = BOMB_ATTACK_EXPLOSION_PROJECTILE_NAME;
		string projectileName3 = CURSE_ATTACK_PROJECTILE_NAME;
		if (m_advancedAttack)
		{
			projectileName2 = BOMB_ATTACK_EXPLOSION_ADVANCED_PROJECTILE_NAME;
			projectileName3 = CURSE_ATTACK_ADVANCED_PROJECTILE_NAME;
		}
		m_bombAttackExplodedRelay.Dispatch();
		this.FireProjectileAbsPos(projectileName2, offset1, matchFacing: false);
		this.FireProjectileAbsPos(projectileName2, offset2, matchFacing: false);
		this.FireProjectileAbsPos(projectileName2, offset3, matchFacing: false);
		this.FireProjectileAbsPos(projectileName2, offset4, matchFacing: false);
		this.FireProjectileAbsPos(projectileName2, offset5, matchFacing: false);
		if (m_bombAttack_FireballCountAtEnd > 0)
		{
			float num = 360 / m_bombAttack_FireballCountAtEnd;
			float num2 = 0f;
			for (int i = 0; i < m_bombAttack_FireballCountAtEnd; i++)
			{
				float angle = num2 + (float)i * num;
				this.FireProjectileAbsPos(projectileName3, offset1, matchFacing: false, angle);
				this.FireProjectileAbsPos(projectileName3, offset2, matchFacing: false, angle);
				this.FireProjectileAbsPos(projectileName3, offset3, matchFacing: false, angle);
				this.FireProjectileAbsPos(projectileName3, offset4, matchFacing: false, angle);
				this.FireProjectileAbsPos(projectileName3, offset5, matchFacing: false, angle);
			}
		}
		yield return Default_Animation("SummonBomb_Attack_Intro", m_bombAttack_AttackIntro_AnimSpeed, m_bombAttack_AttackIntro_Delay);
		yield return Default_Animation("SummonBomb_Attack_Hold", m_bombAttack_AttackHold_AnimSpeed, m_bombAttack_AttackHold_Delay);
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("SummonBomb_Exit", m_bombAttack_Exit_AnimSpeed, m_bombAttack_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		yield return Default_Attack_Cooldown(m_bombAttack_Exit_IdleDuration, m_bombAttack_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator ShoutAttack()
	{
		this.ToDo("SHOUT ATTACK");
		if (m_shout_stopMovingWhileAttacking)
		{
			base.EnemyController.FlyingMovementType = FlyingMovementType.Override;
			StopAndFaceTarget();
			base.EnemyController.LockFlip = true;
		}
		m_shoutWarningProjectile = this.FireProjectile("StudyBossShoutWarningProjectile", 2, matchFacing: false);
		yield return Default_TellIntroAndLoop("Shout_Tell_Intro", m_shout_TellIntro_AnimSpeed, "Shout_Tell_Hold", m_shout_TellHold_AnimSpeed, m_shout_TellIntroAndHold_Delay);
		SetAttackingWithContactDamage(enable: true);
		yield return Default_Animation("Shout_Attack_Intro", m_shout_AttackIntro_AnimSpeed, m_shout_AttackIntro_Delay);
		m_shoutWarningProjectile.transform.position = base.EnemyController.Midpoint;
		m_shoutWarningProjectile.transform.SetParent(base.EnemyController.transform, worldPositionStays: true);
		m_shoutAttackWarningAppearedRelay.Dispatch();
		StopProjectile(ref m_shoutWarningProjectile);
		m_shoutAttackExplodedRelay.Dispatch();
		this.FireProjectile("StudyBossShoutExplosionProjectile", 2, matchFacing: false);
		if (m_shout_fireballsAtEnd > 0)
		{
			float angleBetweenProjectiles = 360 / m_shout_fireballsAtEnd;
			float initialAngle = 0f;
			for (int i = 0; i < m_shout_fireballsAtEnd; i++)
			{
				float angle = initialAngle + (float)i * angleBetweenProjectiles;
				this.FireProjectile(BOMB_ATTACK_FIREBALL_PROJECTILE_NAME, 2, matchFacing: false, angle);
			}
			if (m_advancedAttack)
			{
				if (m_shoutAttackDelaySecondExplosion > 0f)
				{
					yield return Wait(m_shoutAttackDelaySecondExplosion);
				}
				for (int j = 0; j < m_shout_fireballsAtEnd; j++)
				{
					float num = initialAngle + (float)j * angleBetweenProjectiles;
					num += (float)m_shoutAttackDelaySecondExplosionAngleAdd;
					this.FireProjectile(BOMB_ATTACK_FIREBALL_PROJECTILE_NAME, 2, matchFacing: false, num);
				}
				if (m_shoutAttackDelaySecondExplosion > 0f)
				{
					yield return Wait(m_shoutAttackDelaySecondExplosion);
				}
				for (int k = 0; k < m_shout_fireballsAtEnd; k++)
				{
					float angle2 = initialAngle + (float)k * angleBetweenProjectiles;
					this.FireProjectile(BOMB_ATTACK_FIREBALL_PROJECTILE_NAME, 2, matchFacing: false, angle2);
				}
			}
		}
		yield return Default_Animation("Shout_Attack_Hold", m_shout_AttackHold_AnimSpeed, m_shout_AttackHold_Delay);
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("Shout_Exit", m_shout_Exit_AnimSpeed, m_shout_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		yield return Default_Attack_Cooldown(m_shout_Exit_IdleDuration, m_shout_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator DashAttack()
	{
		this.ToDo("Dash Attack");
		StopAndFaceTarget();
		base.EnemyController.FollowTarget = false;
		yield return Default_TellIntroAndLoop("Dash_Tell_Intro", m_dashAttack_TellIntro_AnimSpeed, "Dash_Tell_Hold", m_dashAttack_TellHold_AnimSpeed, m_dashAttack_TellIntroAndHold_Delay);
		SetAttackingWithContactDamage(enable: true);
		base.EnemyController.Heading = base.EnemyController.TargetController.Midpoint - base.EnemyController.Midpoint;
		base.EnemyController.FlyingMovementType = FlyingMovementType.Override;
		Vector2 vector = base.EnemyController.TargetController.Midpoint - base.EnemyController.Midpoint;
		Vector2 dashVelocity = m_dashAttackSpeed * vector.normalized;
		this.FaceTarget();
		base.EnemyController.LockFlip = true;
		yield return Default_Animation("Dash_Attack_Intro", m_dashAttack_AttackIntro_AnimSpeed, m_dashAttack_AttackIntro_Delay);
		base.EnemyController.SetVelocity(dashVelocity.x, dashVelocity.y, additive: false);
		this.SetAnimationSpeedMultiplier(m_dashAttack_AttackHold_AnimSpeed);
		yield return this.ChangeAnimationState("Dash_Attack_Hold");
		if (m_dashAttack_AttackHold_Delay > 0f)
		{
			yield return Wait(m_dashAttack_AttackHold_Delay);
		}
		if (m_dashAttackFireballCountAtEnd > 0)
		{
			float num = 360 / m_dashAttackFireballCountAtEnd;
			float num2 = 0f;
			for (int i = 0; i < m_dashAttackFireballCountAtEnd; i++)
			{
				float angle = num2 + (float)i * num;
				this.FireProjectile("StudyBossFlowerProjectile", Vector2.zero, matchFacing: false, angle, m_dashAttackFireballSpeedMod);
			}
		}
		base.EnemyController.SetVelocity(0f, 0f, additive: false);
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("Dash_Exit", m_dashAttack_Exit_AnimSpeed, m_dashAttack_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.FollowTarget = true;
		base.EnemyController.ResetTurnTrigger();
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_dashAttack_Exit_IdleDuration, m_dashAttack_AttackCD);
	}

	private void ShieldAttack()
	{
		if (!m_isShieldAttackRunning)
		{
			this.ToDo("SHIELD ATTACK");
			m_isShieldAttackRunning = true;
			SpawnOrbitingAttackProjectiles(m_shieldAttackProjectileCount, OrbitShieldProjectileAroundBoss);
			PlayShieldAttackAudio();
		}
	}

	private IEnumerator OrbitShieldProjectileAroundBoss(float initialAngle)
	{
		Vector3 midpoint = base.EnemyController.Midpoint;
		Vector3 pivotDiff = midpoint - base.EnemyController.transform.localPosition;
		Vector3 vector = GetCircleAttackProjectileOffset(midpoint, initialAngle, m_shieldAttackRadius);
		Projectile_RL projectile = this.FireProjectile("StudyBossShieldProjectile", vector, matchFacing: false);
		float angle = initialAngle;
		float timeStart = Time.time;
		bool isRespawnTimerRunning = false;
		while (!base.EnemyController.IsDead)
		{
			if (!projectile.gameObject.activeInHierarchy && !isRespawnTimerRunning)
			{
				isRespawnTimerRunning = true;
				timeStart = Time.time;
			}
			midpoint = base.EnemyController.Midpoint;
			vector = GetCircleAttackProjectileOffset(midpoint, angle, m_shieldAttackRadius);
			vector += pivotDiff;
			projectile.transform.position = base.EnemyController.Midpoint + vector;
			if (isRespawnTimerRunning)
			{
				isRespawnTimerRunning = false;
				if (Time.time - timeStart >= m_shieldAttackTimeBetweenProjectileRespawn)
				{
					projectile = this.FireProjectile("StudyBossShieldProjectile", vector, matchFacing: false);
				}
			}
			angle += m_shieldAttackOrbitSpeed * Time.deltaTime;
			yield return null;
		}
		StopProjectile(ref projectile);
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
	public IEnumerator ModeShift()
	{
		this.ToDo("MODE SHIFT");
		RemoveStatusEffects(includeCommanderBuffs: false);
		base.EnemyController.StatusEffectController.ImmuneToAllStatusEffects = true;
		yield return base.DeathAnim();
		base.EnemyController.LockFlip = true;
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		SetVelocity(0f, 0f, additive: false);
		base.EnemyController.ModeshiftDamageMod = m_modeShift_DamageMod;
		m_shoutWarningProjectile = this.FireProjectile("StudyBossShoutWarningProjectile", 2, matchFacing: false);
		m_shoutAttackWarningAppearedRelay.Dispatch();
		yield return Default_Animation("ModeShift_Intro", m_modeShift_Intro_AnimSpeed, m_modeShift_Intro_Delay);
		yield return Default_Animation("ModeShift_Scream_Intro", m_modeShift_Scream_Intro_AnimSpeed, m_modeShift_Scream_Intro_Delay);
		SetAttackingWithContactDamage(enable: true);
		StopProjectile(ref m_shoutWarningProjectile);
		ShieldAttack();
		this.FireProjectile("StudyBossShoutExplosionProjectile", 2, matchFacing: false);
		yield return Default_Animation("ModeShift_Scream_Hold", m_modeShift_Scream_Hold_AnimSpeed, m_modeShift_Scream_Hold_Delay);
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("ModeShift_Scream_Exit", m_modeShift_Exit_AnimSpeed, m_modeShift_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ModeshiftDamageMod = 1f;
		base.EnemyController.ResetBaseValues();
		base.EnemyController.StatusEffectController.ImmuneToAllStatusEffects = false;
		yield return Default_Attack_Cooldown(m_modeShift_IdleDuration, m_modeShift_AttackCD);
	}

	public override IEnumerator SpawnAnim()
	{
		yield return Default_Animation("Intro_Idle", m_spawn_Idle_AnimSpeed, m_spawn_Idle_Delay);
		yield return Default_Animation("Intro", m_spawn_Intro_AnimSpeed, m_spawn_Intro_Delay);
		MusicManager.PlayMusic(SongID.StudyBossBGM_Boss5_Phase1, isOverride: false);
		yield return Default_Animation(BaseAIScript.NEUTRAL_STATE, 1f, 1f, waitUntilAnimationComplete: false);
	}

	public override IEnumerator DeathAnim()
	{
		yield return base.DeathAnim();
		RLTimeScale.SetTimeScale(TimeScaleType.Cutscene, 0.5f);
		yield return Default_Animation("Death_Intro", m_death_Intro_AnimSpeed, m_death_Intro_Delay);
		RLTimeScale.SetTimeScale(TimeScaleType.Cutscene, 1f);
		base.EnemyController.GetComponent<BossModeShiftController>().DissolveMaterials(m_death_Hold_Delay);
		yield return Default_Animation("Death_Loop", m_death_Hold_AnimSpeed, m_death_Hold_Delay);
	}

	private void OnBossHit(object sender, HealthChangeEventArgs args)
	{
		if (!base.EnemyController.IsDead && !(args.PrevHealthValue <= args.NewHealthValue) && base.EnemyController.CurrentHealth <= (float)base.EnemyController.ActualMaxHealth * m_modeShift_HealthMod)
		{
			string forceExecuteLogicBlockName_OnceOnly = "ModeShift";
			base.LogicController.StopAllLogic(resetCurrentState: true);
			base.LogicController.ForceExecuteLogicBlockName_OnceOnly = forceExecuteLogicBlockName_OnceOnly;
			if (m_modeShiftEventArgs == null)
			{
				m_modeShiftEventArgs = new EnemyModeShiftEventArgs(base.EnemyController);
			}
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.EnemyModeShift, this, m_modeShiftEventArgs);
			base.EnemyController.HealthChangeRelay.RemoveListener(m_onBossHit);
		}
	}

	public override void OnLBCompleteOrCancelled()
	{
		DisableWarningProjectiles();
		base.EnemyController.LockFlip = false;
		EndRapidFireAttackAudio();
		base.OnLBCompleteOrCancelled();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (base.IsInitialized)
		{
			DisableWarningProjectiles();
			DisablePersistentLogicProjectiles();
			DisableAudio();
		}
	}

	private void DisableWarningProjectiles()
	{
		StopProjectile(ref m_shoutWarningProjectile);
		StopProjectile(ref m_bombWarningProjectile);
		StopProjectile(ref m_bombWarningProjectile2);
		StopProjectile(ref m_bombWarningProjectile3);
		StopProjectile(ref m_bombWarningProjectile4);
		StopProjectile(ref m_bombWarningProjectile5);
	}

	private void DisablePersistentLogicProjectiles()
	{
		for (int i = 0; i < m_verticalBeamAttackCount; i++)
		{
			Projectile_RL projectile = m_verticalBeamProjectiles[i];
			StopProjectile(ref projectile);
			Projectile_RL projectile2 = m_verticalBeamWarningProjectiles[i];
			StopProjectile(ref projectile2);
		}
		if (m_verticalBeamAudioPlaying)
		{
			EndVerticalBeamAudio();
		}
	}

	private float Die3Roll()
	{
		float num = UnityEngine.Random.Range(0, 5);
		if (num == 1f)
		{
			return (0f - m_bombAttack_Displace_Y) * 2f;
		}
		if (num == 2f)
		{
			return 0f - m_bombAttack_Displace_Y;
		}
		if (num == 3f)
		{
			return 0f;
		}
		if (num == 4f)
		{
			return m_bombAttack_Displace_Y;
		}
		return m_bombAttack_Displace_Y * 2f;
	}

	public override void ResetScript()
	{
		m_isShieldAttackRunning = false;
		base.EnemyController.StatusEffectController.ImmuneToAllStatusEffects = false;
		base.ResetScript();
	}

	private void InitializeAudio()
	{
		m_flyingAudioEventInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/sfx_studyBoss_fly_loop", base.transform);
		m_idleAudioEventInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/sfx_studyBoss_idle_loop", base.transform);
		m_verticalBeamLoopAudioEventInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/sfx_studyBoss_attack_skyBeam_spawn_start_loop", base.transform);
		m_verticalBeamDespawnAudioEventInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/sfx_studyBoss_attack_skyBeam_despawn", base.transform);
		m_voidCircleClosingAudioEventInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/sfx_studyBoss_attack_voidCircle_closeIn", base.transform);
		m_eyeballShotLoopAudioEventInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/sfx_studyBoss_attack_eye_shoot_start_loop", base.transform);
		m_shieldAttackLoopAudioEventInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/sfx_studyBoss_phase2_voidProjectiles_loop", base.transform);
	}

	private void DisableAudio()
	{
		AudioManager.Stop(m_flyingAudioEventInstance);
		AudioManager.Stop(m_idleAudioEventInstance);
		AudioManager.Stop(m_verticalBeamLoopAudioEventInstance);
		AudioManager.Stop(m_eyeballShotLoopAudioEventInstance);
		AudioManager.Stop(m_shieldAttackLoopAudioEventInstance);
	}

	private void OnDestroy()
	{
		if (m_flyingAudioEventInstance.isValid())
		{
			m_flyingAudioEventInstance.release();
		}
		if (m_idleAudioEventInstance.isValid())
		{
			m_idleAudioEventInstance.release();
		}
		if (m_verticalBeamLoopAudioEventInstance.isValid())
		{
			m_verticalBeamLoopAudioEventInstance.release();
		}
		if (m_verticalBeamDespawnAudioEventInstance.isValid())
		{
			m_verticalBeamDespawnAudioEventInstance.release();
		}
		if (m_voidCircleClosingAudioEventInstance.isValid())
		{
			m_voidCircleClosingAudioEventInstance.release();
		}
		if (m_eyeballShotLoopAudioEventInstance.isValid())
		{
			m_eyeballShotLoopAudioEventInstance.release();
		}
		if (m_shieldAttackLoopAudioEventInstance.isValid())
		{
			m_shieldAttackLoopAudioEventInstance.release();
		}
	}

	protected override void PlayDeathAnimAudio()
	{
		if (base.EnemyController.CurrentHealth > 0f)
		{
			AudioManager.PlayOneShot(this, "event:/SFX/Enemies/sfx_studyBoss_phase2_hit_start", base.gameObject.transform.position);
		}
		else
		{
			base.PlayDeathAnimAudio();
		}
	}

	private void Update()
	{
		bool flag = base.EnemyController.Velocity.sqrMagnitude > 0f;
		UpdateFlyingAudio(flag);
		UpdateIdleAudio(flag);
		m_wasFlying = flag;
	}

	private void UpdateFlyingAudio(bool isFlying)
	{
		if (isFlying && !m_wasFlying)
		{
			AudioManager.PlayAttached(this, m_flyingAudioEventInstance, base.gameObject);
		}
		else if (!isFlying && m_wasFlying)
		{
			AudioManager.Stop(m_flyingAudioEventInstance);
		}
		if (isFlying)
		{
			float magnitude = base.EnemyController.Velocity.magnitude;
			float value = 0f;
			if (magnitude > 0f)
			{
				value = ((!(magnitude < 5f)) ? 1f : (magnitude / 5f));
			}
			if (m_flyingAudioEventInstance.isValid() && AudioUtility.GetHasParameter(m_flyingAudioEventInstance, "dancingBoss_flySpeed"))
			{
				m_flyingAudioEventInstance.setParameterByName("dancingBoss_flySpeed", value);
			}
		}
	}

	private void UpdateIdleAudio(bool isFlying)
	{
		if (isFlying && !m_wasFlying)
		{
			AudioManager.Stop(m_idleAudioEventInstance);
		}
		else if (!isFlying)
		{
			m_idleAudioEventInstance.getPlaybackState(out var state);
			if (state == PLAYBACK_STATE.STOPPED)
			{
				AudioManager.PlayAttached(this, m_idleAudioEventInstance, base.gameObject);
			}
		}
	}

	private void BeginVerticalBeamAudio()
	{
		AudioManager.Play(this, m_verticalBeamLoopAudioEventInstance);
		m_verticalBeamAudioPlaying = true;
	}

	private void EndVerticalBeamAudio()
	{
		AudioManager.Stop(m_verticalBeamLoopAudioEventInstance);
		AudioManager.Play(this, m_verticalBeamDespawnAudioEventInstance);
		m_verticalBeamAudioPlaying = false;
	}

	private void BeginRapidFireAttackAudio()
	{
		if (!m_rapidFireAudioPlaying)
		{
			m_rapidFireAudioPlaying = true;
			AudioManager.PlayAttached(this, m_eyeballShotLoopAudioEventInstance, base.gameObject);
		}
	}

	private void EndRapidFireAttackAudio()
	{
		if (m_rapidFireAudioPlaying)
		{
			m_rapidFireAudioPlaying = false;
			AudioManager.Stop(m_eyeballShotLoopAudioEventInstance);
		}
	}

	private void PlayCircleClosingAudio()
	{
		AudioManager.PlayAttached(this, m_voidCircleClosingAudioEventInstance, base.Target);
	}

	private void PlayShieldAttackAudio()
	{
		AudioManager.PlayAttached(this, m_shieldAttackLoopAudioEventInstance, base.gameObject);
	}
}
