using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RLAudio;
using Sigtrap.Relays;
using UnityEngine;

public class DancingBoss_Basic_AIScript : BaseAIScript
{
	public Relay SummonSentriesStartRelay = new Relay();

	public Relay SummonSentriesCompleteRelay = new Relay();

	public Relay<DancingBossWaveFiredEventArgs> WaveFiredRelay = new Relay<DancingBossWaveFiredEventArgs>();

	public Relay SpreadIntroAnimStartRelay = new Relay();

	public Relay SpreadIntroAnimCompleteRelay = new Relay();

	public Relay SpreadAttackAnimStartRelay = new Relay();

	public Relay SpreadAttackAnimCompleteRelay = new Relay();

	public Relay<Vector3> SpreadAttackStartRelay = new Relay<Vector3>();

	public Relay SpreadAttackCompleteRelay = new Relay();

	private EnemyModeShiftEventArgs m_modeShiftEventArgs;

	private int m_currentMode;

	private Transform m_summonPointStorageLocation;

	private Action<object, HealthChangeEventArgs> m_onBossHit;

	private const float HORIZONTAL_INSET = 10f;

	private const float VERTICAL_INSET = 0f;

	private const string BASIC_WAVE_PROJECTILE = "DancingBossWaveBoltProjectile";

	private const string BOUNCE_WAVE_PROJECTILE = "DancingBossWaveBounceProjectile";

	private const string PASS_WAVE_PROJECTILE = "DancingBossWavePassBoltProjectile";

	private const string BASIC_PROJECTILE = "DancingBossBoltProjectile";

	private const string BOUNCE_PROJECTILE = "DancingBossBounceBoltProjectile";

	private const string SUMMON_PROJECTILE = "DancingBossSummonProjectile";

	private const string MODE_SHIFT_WARNING_PROJECTILE = "SpellSwordShoutWarningProjectile";

	protected const string ORBIT_SHOT_PROJECTILE = "DancingBossBounceRotateProjectile";

	protected const string STANDING_PROJECTILE = "DancingBossStandingBoltProjectile";

	protected const string VOID_PROJECTILE = "DancingBossVoidProjectile";

	private const string VERTICAL_TELL_INTRO = "SummonBulletsVertical_Tell_Intro";

	private const string VERTICAL_TELL_HOLD = "SummonBulletsVertical_Tell_Hold";

	private const string VERTICAL_ATTACK_INTRO = "SummonBulletsVertical_Attack_Intro";

	private const string VERTICAL_ATTACK_HOLD = "SummonBulletsVertical_Attack_Hold";

	private const string VERTICAL_ATTACK_EXIT = "SummonBulletsVertical_Exit";

	private const string SPREAD_VERTICAL_TELL_INTRO = "CircularBurst_Tell_Intro";

	private const string SPREAD_TELL_HOLD = "CircularBurst_Tell_Hold";

	private const string SPREAD_VERTICAL_ATTACK_INTRO = "CircularBurst_Attack_Intro";

	private const string SPREAD_VERTICAL_ATTACK_HOLD = "CircularBurst_Attack_Hold";

	private const string SPREAD_VERTICAL_ATTACK_EXIT = "CircularBurst_Exit";

	private const string HORIZONTAL_TELL_INTRO = "SummonBullets_Tell_Intro";

	private const string HORIZONTAL_TELL_HOLD = "SummonBullets_Tell_Hold";

	private const string HORIZONTAL_ATTACK_INTRO = "SummonBullets_Attack_Intro";

	private const string HORIZONTAL_ATTACK_HOLD = "SummonBullets_Attack_Hold";

	private const string HORIZONTAL_ATTACK_EXIT = "SummonBullets_Exit";

	private const string BOMB_TELL_INTRO = "SummonIce_Tell_Intro";

	private const string BOMB_TELL_HOLD = "SummonIce_Tell_Hold";

	private const string BOMB_ATTACK_INTRO = "SummonIce_Attack_Intro";

	private const string BOMB_ATTACK_HOLD = "SummonIce_Attack_Hold";

	private const string BOMB_ATTACK_EXIT = "SummonIce_Exit";

	private const string SUMMON_SHOTS_TELL_INTRO = "CircularBurst_Tell_Intro";

	private const string SUMMON_SHOTS_TELL_HOLD = "CircularBurst_Tell_Hold";

	private const string SUMMON_SHOTS_ATTACK_INTRO = "CircularBurst_Attack_Intro";

	private const string SUMMON_SHOTS_ATTACK_HOLD = "CircularBurst_Attack_Hold";

	private const string SUMMON_SHOTS_ATTACK_EXIT = "CircularBurst_Exit";

	public Relay SummonShotsTellStartRelay = new Relay();

	public Relay SummonShotsTellCompleteRelay = new Relay();

	public Relay SummonShotsAttackStartRelay = new Relay();

	public Relay SummonShotsAttackCompleteRelay = new Relay();

	public Relay<Vector2> SummonShotsPortalOpenedRelay = new Relay<Vector2>();

	private const string DASH_TELL_INTRO = "Dash_Tell_Intro";

	private const string DASH_TELL_HOLD = "Dash_Tell_Hold";

	private const string DASH_ATTACK_INTRO = "Dash_Attack_Intro";

	private const string DASH_ATTACK_HOLD = "Dash_Attack_Hold";

	private const string DASH_ATTACK_EXIT = "Dash_Exit";

	protected const string MODE_SHIFT_01_DOWNED = "ModeShift_Intro";

	protected const string MODE_SHIFT_01_ATTACK_INTRO = "ModeShift_Scream_Intro";

	protected const string MODE_SHIFT_01_ATTACK_HOLD = "ModeShift_Scream_Hold";

	protected const string MODE_SHIFT_01_ATTACK_EXIT = "ModeShift_Scream_Exit";

	private Projectile_RL m_modeShift01WarningProjectile;

	protected const int MODE_SHIFT_01_WARNING_PROJECTILE_POS_INDEX = 0;

	protected const string MODE_SHIFT_02_DOWNED = "Stunned";

	protected const string MODE_SHIFT_02_GETUP = "Neutral";

	protected const string MODE_SHIFT_02_EXIT = "Neutral";

	protected const string MODE_SHIFT_02_TELL_INTRO = "Shoot_Tell_Intro";

	protected const string MODE_SHIFT_02_TELL_HOLD = "Shoot_Tell_Hold";

	protected const string MODE_SHIFT_02_ATTACK_INTRO = "Shoot_Attack_Intro";

	protected const string MODE_SHIFT_02_ATTACK_HOLD = "Shoot_Attack_Hold";

	protected const string MODE_SHIFT_02_ATTACK_EXIT = "Shoot_Exit";

	protected const string FINAL_DANCE_TELL_INTRO = "Shoot_Tell_Intro";

	protected const string FINAL_DANCE_TELL_HOLD = "Shoot_Tell_Hold";

	protected const string FINAL_DANCE_ATTACK_INTRO = "Shoot_Attack_Intro";

	protected const string FINAL_DANCE_ATTACK_HOLD = "Shoot_Attack_Hold";

	protected const string FINAL_DANCE_ATTACK_EXIT = "Neutral";

	private Projectile_RL m_modeShift02WarningProjectile;

	protected const int MODE_SHIFT_02_WARNING_PROJECTILE_POS_INDEX = 0;

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

	private List<Sentry_Hazard> m_sentryHazards;

	private GameObject m_centerOfRoom;

	private DancingBossWaveFiredEventArgs m_waveEventArgs;

	private bool m_areSummoningPointsInitialized;

	private GameObject[] m_leftHorizontalProjectileSummonPoints;

	private GameObject[] m_rightHorizontalProjectileSummonPoints;

	private GameObject[] m_topVerticalProjectileSummonPoints;

	private GameObject[] m_bottomVerticalProjectileSummonPoints;

	private GameObject[] m_summonProjectileSummonPoints;

	protected virtual bool m_advancedAttacks => false;

	protected override Vector2 IdleDuration => new Vector2(0.5f, 1f);

	protected override Vector2 WalkTowardsDuration => new Vector2(1.75f, 2.5f);

	protected override Vector2 WalkAwayDuration => new Vector2(1.75f, 2.5f);

	protected override Vector2 RandomFollowOffsetX => new Vector2(0f, 0f);

	protected override Vector2 RandomFollowOffsetY => new Vector2(0f, 0f);

	protected virtual float m_verticalWave_TellIntro_AnimSpeed => 1f;

	protected virtual float m_verticalWave_TellHold_AnimSpeed => 1f;

	protected virtual float m_verticalWave_TellIntroAndHold_Duration => 1f;

	protected virtual float m_verticalWave_AttackIntro_AnimSpeed => 1f;

	protected virtual float m_verticalWave_AttackIntro_Delay => 0f;

	protected virtual float m_verticalWave_AttackHold_AnimSpeed => 1f;

	protected virtual float m_verticalWave_AttackHold_Delay => 0f;

	protected virtual float m_verticalWave_AttackHoldExit_Delay => 0.25f;

	protected virtual float m_verticalWave_AttackExit_AnimSpeed => 1f;

	protected virtual float m_verticalWave_AttackExit_Delay => 0f;

	protected virtual float m_verticalWave_AttackExit_IdleDuration => 1f;

	protected virtual float m_verticalWave_AttackExit_CooldownDuration => 10f;

	protected virtual float m_wave_SummoningCircleDelay => 1.25f;

	protected virtual int m_verticalWave_ProjectileCount => 17;

	protected virtual int m_verticalWave_BounceProjectileCount => 17;

	protected virtual int m_verticalWave_ProjectileSpeedMod => 1;

	protected virtual bool m_verticalWave_Pause_While_Attacking => false;

	protected virtual float m_spread_TellIntro_AnimSpeed => 1f;

	protected virtual float m_spread_TellHold_AnimSpeed => 1f;

	protected virtual float m_spread_TellIntroAndHold_Duration => 1f;

	protected virtual float m_spread_AttackIntro_AnimSpeed => 1f;

	protected virtual float m_spread_AttackIntro_Delay => 0f;

	protected virtual float m_spread_AttackHold_AnimSpeed => 1f;

	protected virtual float m_spread_AttackHold_Delay => 0f;

	protected virtual float m_spread_AttackHoldExit_Delay => 0.25f;

	protected virtual float m_spread_AttackExit_AnimSpeed => 1f;

	protected virtual float m_spread_AttackExit_Delay => 0f;

	protected virtual float m_spread_AttackExit_IdleDuration => 1f;

	protected virtual float m_spread_AttackExit_CooldownDuration => 10f;

	protected virtual int m_spread_BasicProjectileCount => 17;

	protected virtual int m_spread_BounceProjectileCount => 0;

	protected virtual float m_spread_ProjectileSpeedMod => 2.25f;

	protected virtual float m_spread_TimeBetweenAttacks => 0.3f;

	protected virtual int m_spread_NumberOfAttacks => 8;

	protected virtual float m_spread_SpaceBetweenAttacks => 6.25f;

	protected virtual bool m_spread_summonProjectilesAtPlayerPosition => true;

	protected virtual bool m_spread_Pause_While_Attacking => false;

	protected virtual float m_horizontalWave_TellIntro_AnimSpeed => 1f;

	protected virtual float m_horizontalWave_TellHold_AnimSpeed => 1f;

	protected virtual float m_horizontalWave_TellIntroAndHold_Duration => 1f;

	protected virtual float m_horizontalWave_AttackIntro_AnimSpeed => 1f;

	protected virtual float m_horizontalWave_AttackIntro_Delay => 0f;

	protected virtual float m_horizontalWave_AttackHold_AnimSpeed => 1f;

	protected virtual float m_horizontalWave_AttackHold_Delay => 0f;

	protected virtual float m_horizontalWave_AttackHoldExit_Delay => 0.25f;

	protected virtual float m_horizontalWave_AttackExit_AnimSpeed => 1f;

	protected virtual float m_horizontalWave_AttackExit_Delay => 0f;

	protected virtual float m_horizontalWave_AttackExit_IdleDuration => 1f;

	protected virtual float m_horizontalWave_AttackExit_CooldownDuration => 10f;

	protected virtual int m_horizontalWave_ProjectileCount => 11;

	protected virtual int m_horizontalWave_BounceProjectileCount => 11;

	protected virtual float m_horizontalWave_ProjectileSpeedMod => 1f;

	protected virtual bool m_horizontalWave_Pause_While_Attacking => false;

	protected virtual float m_bomb_TellIntro_AnimSpeed => 1f;

	protected virtual float m_bomb_TellHold_AnimSpeed => 1f;

	protected virtual float m_bomb_TellIntroAndHold_Duration => 1f;

	protected virtual float m_bomb_AttackIntro_AnimSpeed => 1f;

	protected virtual float m_bomb_AttackIntro_Delay => 0f;

	protected virtual float m_bomb_AttackHold_AnimSpeed => 1f;

	protected virtual float m_bomb_AttackHold_Delay => 0f;

	protected virtual float m_bomb_AttackHoldExit_Delay => 0.25f;

	protected virtual float m_bomb_AttackExit_AnimSpeed => 1f;

	protected virtual float m_bomb_AttackExit_Delay => 0f;

	protected virtual float m_bomb_AttackExit_IdleDuration => 1f;

	protected virtual float m_bomb_AttackExit_CooldownDuration => 10f;

	protected virtual bool m_bomb_summonProjectilesAtPlayerPosition => false;

	protected virtual int m_bomb_projectileAmount => 2;

	protected virtual float m_bomb_projectileDelay => 0.75f;

	protected virtual float m_bomb_ProjectileSpeedMod => 1f;

	protected virtual bool m_bomb_Pause_While_Attacking => false;

	protected virtual float m_summonShots_TellIntro_AnimSpeed => 1f;

	protected virtual float m_summonShots_TellHold_AnimSpeed => 1f;

	protected virtual float m_summonShots_TellIntroAndHold_Duration => 0f;

	protected virtual float m_summonShots_AttackIntro_AnimSpeed => 1f;

	protected virtual float m_summonShots_AttackIntro_Delay => 0f;

	protected virtual float m_summonShots_AttackHold_AnimSpeed => 1f;

	protected virtual float m_summonShots_AttackHold_Delay => 0f;

	protected virtual float m_summonShots_AttackHoldExit_Delay => 0f;

	protected virtual float m_summonShots_AttackExit_AnimSpeed => 1f;

	protected virtual float m_summonShots_AttackExit_Delay => 0f;

	protected virtual float m_summonShots_AttackExit_IdleDuration => 1f;

	protected virtual float m_summonShots_AttackExit_CooldownDuration => 10f;

	protected virtual int m_summonShots_ProjectileCount => 8;

	protected virtual int m_summonShots_ProjectileSpeedMod => 1;

	protected virtual float m_dash_TellIntro_AnimSpeed => 1f;

	protected virtual float m_dash_TellHold_AnimSpeed => 1f;

	protected virtual float m_dash_TellIntroAndHold_Duration => 0f;

	protected virtual float m_dash_AttackIntro_AnimSpeed => 1.5f;

	protected virtual float m_dash_AttackIntro_Delay => 1.5f;

	protected virtual float m_dash_AttackHold_AnimSpeed => 1f;

	protected virtual float m_dash_AttackHold_Delay => 0f;

	protected virtual float m_dash_AttackExit_AnimSpeed => 1f;

	protected virtual float m_dash_AttackExit_Delay => 0f;

	protected virtual float m_dash_AttackExit_IdleDuration => 1f;

	protected virtual float m_dash_AttackExit_CooldownDuration => 12f;

	protected virtual float m_dash_Attack_ForwardSpeed => 16f;

	protected virtual float m_dash_Attack_Duration => 1.25f;

	protected virtual bool m_dash_Attack_Pause_While_Attacking => false;

	protected virtual float m_modeShift01_Downed_AnimSpeed => 1f;

	protected virtual float m_modeShift01_Downed_Delay => 1f;

	protected virtual float m_modeShift01_GetUp_AnimSpeed => 1f;

	protected virtual float m_modeShift01_GetUp_Delay => 1f;

	protected virtual float m_modeShift01_Warning_Delay => 1f;

	protected virtual float m_modeShift01_Exit_AnimSpeed => 0.5f;

	protected virtual float m_modeShift01_Exit_Delay => 0f;

	protected virtual float m_modeShift01_Exit_IdleDuration => 0.15f;

	protected virtual float m_modeShift01_AttackCD => 999999f;

	protected virtual float m_modeShift01_Damage_Mod => 0.1f;

	protected virtual float m_firstModeShiftHealthMod => 0.5f;

	protected virtual float m_modeShift02_Downed_AnimSpeed => 1f;

	protected virtual float m_modeShift02_Downed_Delay => 1f;

	protected virtual float m_modeShift02_GetUp_AnimSpeed => 1f;

	protected virtual float m_modeShift02_GetUp_Delay => 1f;

	protected virtual float m_modeShift02_Warning_Delay => 1f;

	protected virtual float m_modeShift02_Exit_AnimSpeed => 0.5f;

	protected virtual float m_modeShift02_Exit_Delay => 0f;

	protected virtual float m_modeShift02_Exit_IdleDuration => 0.15f;

	protected virtual float m_modeShift02_AttackCD => 99f;

	protected virtual float m_finalDance_TellIntro_AnimSpeed => 1f;

	protected virtual float m_finalDance_TellHold_AnimSpeed => 1f;

	protected virtual float m_finalDance_TellIntroAndHold_Duration => 1f;

	protected virtual float m_finalDance_AttackIntro_AnimSpeed => 1f;

	protected virtual float m_finalDance_AttackIntro_Delay => 0f;

	protected virtual float m_finalDance_AttackHold_AnimSpeed => 1f;

	protected virtual float m_finalDance_AttackHold_Delay => 0f;

	protected virtual float m_finalDance_AttackHoldExit_Delay => 1f;

	protected virtual float m_finalDance_AttackExit_AnimSpeed => 1f;

	protected virtual float m_finalDance_AttackExit_Delay => 1f;

	protected virtual float m_finalDance_AttackExit_IdleDuration => 1f;

	protected virtual float m_finalDance_AttackExit_CooldownDuration => 1f;

	protected virtual float m_modeShift02_Damage_Mod => 0.1f;

	protected virtual float m_secondModeShiftHealthMod => 0f;

	protected virtual float m_sentryRestDuration => 10f;

	public List<Sentry_Hazard> SentryHazards => m_sentryHazards;

	private void Awake()
	{
		m_onBossHit = OnBossHit;
	}

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[10] { "DancingBossWaveBoltProjectile", "DancingBossWaveBounceProjectile", "DancingBossWavePassBoltProjectile", "DancingBossBoltProjectile", "DancingBossBounceBoltProjectile", "DancingBossSummonProjectile", "SpellSwordShoutWarningProjectile", "DancingBossBounceRotateProjectile", "DancingBossStandingBoltProjectile", "DancingBossVoidProjectile" };
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator VerticalBulletWave()
	{
		this.ToDo("*** V Bullet Wave ***");
		if (m_verticalWave_Pause_While_Attacking)
		{
			StopAndFaceTarget();
		}
		base.EnemyController.LockFlip = true;
		yield return Default_TellIntroAndLoop("SummonBulletsVertical_Tell_Intro", m_verticalWave_TellIntro_AnimSpeed, "SummonBulletsVertical_Tell_Hold", m_verticalWave_TellHold_AnimSpeed, m_verticalWave_TellIntroAndHold_Duration);
		yield return Default_Animation("SummonBulletsVertical_Attack_Intro", m_verticalWave_AttackIntro_AnimSpeed, m_verticalWave_AttackIntro_Delay);
		yield return Default_Animation("SummonBulletsVertical_Attack_Hold", m_verticalWave_AttackHold_AnimSpeed, m_verticalWave_AttackHold_Delay, waitUntilAnimationComplete: false);
		RoomSide randomSide = GetRandomSide(RoomSide.Bottom, RoomSide.Left, RoomSide.Right);
		bool isVerticalWave = false;
		if (randomSide == RoomSide.Bottom)
		{
			isVerticalWave = true;
		}
		FireWave(randomSide, m_verticalWave_ProjectileCount, m_verticalWave_BounceProjectileCount, m_verticalWave_ProjectileSpeedMod, addSummoningDelay: false, isVerticalWave);
		if (m_verticalWave_AttackHoldExit_Delay > 0f)
		{
			yield return Wait(m_verticalWave_AttackHoldExit_Delay);
		}
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("SummonBulletsVertical_Exit", m_verticalWave_AttackExit_AnimSpeed, m_verticalWave_AttackExit_Delay);
		float idleDuration = m_verticalWave_AttackExit_IdleDuration;
		if (!m_verticalWave_Pause_While_Attacking)
		{
			idleDuration = 0f;
		}
		yield return Default_Attack_Cooldown(idleDuration, m_verticalWave_AttackExit_CooldownDuration);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetTurnTrigger();
		base.EnemyController.ResetBaseValues();
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator SpreadAttack()
	{
		this.ToDo("*** Spread Attack ***");
		if (m_spread_Pause_While_Attacking)
		{
			StopAndFaceTarget();
		}
		base.EnemyController.LockFlip = true;
		RoomSide side = GetRandomSide(RoomSide.Bottom, RoomSide.Left);
		SpreadIntroAnimStartRelay.Dispatch();
		yield return Default_TellIntroAndLoop("CircularBurst_Tell_Intro", m_spread_TellIntro_AnimSpeed, "CircularBurst_Tell_Hold", m_spread_TellHold_AnimSpeed, m_spread_TellIntroAndHold_Duration);
		SpreadIntroAnimCompleteRelay.Dispatch();
		SpreadAttackAnimStartRelay.Dispatch();
		yield return Default_Animation("CircularBurst_Attack_Intro", m_spread_AttackIntro_AnimSpeed, m_spread_AttackIntro_Delay);
		yield return Default_Animation("CircularBurst_Attack_Hold", m_spread_AttackHold_AnimSpeed, m_spread_AttackHold_Delay, waitUntilAnimationComplete: false);
		SpreadAttackAnimCompleteRelay.Dispatch();
		Vector3 originPosition = Vector3.zero;
		if (m_spread_summonProjectilesAtPlayerPosition)
		{
			originPosition = base.EnemyController.TargetController.Midpoint - base.EnemyController.Midpoint;
		}
		Vector3 vector = originPosition;
		SpreadAttackStartRelay.Dispatch(base.EnemyController.Midpoint + vector);
		if (side == RoomSide.Bottom)
		{
			this.SummonProjectile("DancingBossWaveBoltProjectile", vector, matchFacing: false, 90f, m_spread_ProjectileSpeedMod, playSummonAudio: false);
			this.SummonProjectile("DancingBossWaveBoltProjectile", vector, matchFacing: false, 270f, m_spread_ProjectileSpeedMod, playSummonAudio: false);
			if (!m_advancedAttacks)
			{
			}
		}
		else
		{
			this.SummonProjectile("DancingBossWaveBoltProjectile", vector, matchFacing: false, 0f, m_spread_ProjectileSpeedMod, playSummonAudio: false);
			this.SummonProjectile("DancingBossWaveBoltProjectile", vector, matchFacing: false, 180f, m_spread_ProjectileSpeedMod, playSummonAudio: false);
			_ = m_advancedAttacks;
		}
		yield return Wait(m_spread_TimeBetweenAttacks);
		for (int i = 1; i < m_spread_NumberOfAttacks; i++)
		{
			if (side == RoomSide.Bottom)
			{
				vector = originPosition;
				vector.x = originPosition.x - (float)i * m_spread_SpaceBetweenAttacks;
				this.SummonProjectile("DancingBossWaveBoltProjectile", vector, matchFacing: false, 90f, m_spread_ProjectileSpeedMod, playSummonAudio: false);
				this.SummonProjectile("DancingBossWaveBoltProjectile", vector, matchFacing: false, 270f, m_spread_ProjectileSpeedMod, playSummonAudio: false);
				vector.x = originPosition.x + (float)i * m_spread_SpaceBetweenAttacks;
				this.SummonProjectile("DancingBossWaveBoltProjectile", vector, matchFacing: false, 90f, m_spread_ProjectileSpeedMod, playSummonAudio: false);
				this.SummonProjectile("DancingBossWaveBoltProjectile", vector, matchFacing: false, 270f, m_spread_ProjectileSpeedMod, playSummonAudio: false);
				if (m_advancedAttacks && i == 4)
				{
					vector = Vector3.zero;
					this.SummonProjectile("DancingBossSummonProjectile", vector, matchFacing: false, 90f, m_bomb_ProjectileSpeedMod);
				}
			}
			else
			{
				vector = originPosition;
				vector.y = originPosition.y - (float)i * m_spread_SpaceBetweenAttacks;
				this.SummonProjectile("DancingBossWaveBoltProjectile", vector, matchFacing: false, 0f, m_spread_ProjectileSpeedMod, playSummonAudio: false);
				this.SummonProjectile("DancingBossWaveBoltProjectile", vector, matchFacing: false, 180f, m_spread_ProjectileSpeedMod, playSummonAudio: false);
				vector.y = originPosition.y + (float)i * m_spread_SpaceBetweenAttacks;
				this.SummonProjectile("DancingBossWaveBoltProjectile", vector, matchFacing: false, 0f, m_spread_ProjectileSpeedMod, playSummonAudio: false);
				this.SummonProjectile("DancingBossWaveBoltProjectile", vector, matchFacing: false, 180f, m_spread_ProjectileSpeedMod, playSummonAudio: false);
				if (m_advancedAttacks && i == 4)
				{
					vector = Vector3.zero;
					this.SummonProjectile("DancingBossSummonProjectile", vector, matchFacing: false, 90f, m_bomb_ProjectileSpeedMod);
				}
			}
			yield return Wait(m_spread_TimeBetweenAttacks);
		}
		SpreadAttackCompleteRelay.Dispatch();
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("CircularBurst_Exit", m_spread_AttackExit_AnimSpeed, m_spread_AttackExit_Delay);
		float idleDuration = m_verticalWave_AttackExit_IdleDuration;
		if (!m_spread_Pause_While_Attacking)
		{
			idleDuration = 0f;
		}
		yield return Default_Attack_Cooldown(idleDuration, m_spread_AttackExit_CooldownDuration);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetTurnTrigger();
		base.EnemyController.ResetBaseValues();
		ResetSummoningPointPositions(side);
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
		this.ToDo("*** Bomb Attack ***");
		if (m_bomb_Pause_While_Attacking)
		{
			StopAndFaceTarget();
		}
		base.EnemyController.LockFlip = true;
		GetRandomSide(RoomSide.Left, RoomSide.Right);
		yield return Default_TellIntroAndLoop("SummonIce_Tell_Intro", m_bomb_TellIntro_AnimSpeed, "SummonIce_Tell_Hold", m_bomb_TellHold_AnimSpeed, m_bomb_TellIntroAndHold_Duration);
		yield return Default_Animation("SummonIce_Attack_Intro", m_bomb_AttackIntro_AnimSpeed, m_bomb_AttackIntro_Delay);
		yield return Default_Animation("SummonIce_Attack_Hold", m_bomb_AttackHold_AnimSpeed, m_bomb_AttackHold_Delay, waitUntilAnimationComplete: false);
		Vector3 vector = Vector3.zero;
		if (m_bomb_summonProjectilesAtPlayerPosition)
		{
			vector = base.EnemyController.TargetController.Midpoint - base.EnemyController.Midpoint;
		}
		Vector3 summonPosition = vector;
		for (int i = 0; i < m_bomb_projectileAmount; i++)
		{
			this.SummonProjectile("DancingBossSummonProjectile", summonPosition, matchFacing: false, 0f, m_bomb_ProjectileSpeedMod);
			if (m_bomb_projectileDelay > 0f)
			{
				yield return Wait(m_bomb_projectileDelay);
			}
		}
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("SummonIce_Exit", m_bomb_AttackExit_AnimSpeed, m_bomb_AttackExit_Delay);
		_ = m_bomb_AttackExit_IdleDuration;
		_ = m_bomb_Pause_While_Attacking;
		yield return Default_Attack_Cooldown(m_bomb_AttackExit_IdleDuration, m_bomb_AttackExit_CooldownDuration);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetTurnTrigger();
		base.EnemyController.ResetBaseValues();
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator SummonShots()
	{
		this.ToDo("*** Summon Shots ***");
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		SummonShotsTellStartRelay.Dispatch();
		yield return Default_TellIntroAndLoop("CircularBurst_Tell_Intro", m_summonShots_TellIntro_AnimSpeed, "CircularBurst_Tell_Hold", m_summonShots_TellHold_AnimSpeed, m_summonShots_TellIntroAndHold_Duration);
		SummonShotsTellCompleteRelay.Dispatch();
		SummonShotsAttackStartRelay.Dispatch();
		yield return Default_Animation("CircularBurst_Attack_Intro", m_summonShots_AttackIntro_AnimSpeed, m_summonShots_AttackIntro_Delay);
		yield return Default_Animation("CircularBurst_Attack_Hold", m_summonShots_AttackHold_AnimSpeed, m_summonShots_AttackHold_Delay, waitUntilAnimationComplete: false);
		for (int i = 0; i < m_summonShots_ProjectileCount; i++)
		{
			float x = UnityEngine.Random.Range(base.EnemyController.Room.Bounds.min.x, base.EnemyController.Room.Bounds.max.x);
			float y = UnityEngine.Random.Range(base.EnemyController.Room.Bounds.min.y, base.EnemyController.Room.Bounds.max.y);
			Vector2 vector = new Vector2(x, y);
			m_summonProjectileSummonPoints[i].transform.position = vector;
		}
		_ = Vector3.zero;
		Vector3 vector2 = base.EnemyController.TargetController.Midpoint - base.EnemyController.Midpoint;
		float num = 3f;
		Vector3 vector3 = vector2;
		vector3.y = vector2.y + num;
		this.SummonProjectile("DancingBossStandingBoltProjectile", vector3, matchFacing: false, 90f, 0f, playSummonAudio: false);
		vector3 = vector2;
		vector3.y = vector2.y + num * 2f;
		vector3.x = vector2.x - num;
		this.SummonProjectile("DancingBossStandingBoltProjectile", vector3, matchFacing: false, 90f, 0f, playSummonAudio: false);
		vector3 = vector2;
		vector3.y = vector2.y + num * 2f;
		vector3.x = vector2.x + num;
		this.SummonProjectile("DancingBossStandingBoltProjectile", vector3, matchFacing: false, 90f, 0f, playSummonAudio: false);
		vector3 = vector2;
		vector3.x = vector2.x - num * 2f;
		vector3.y = vector2.y + num;
		this.SummonProjectile("DancingBossStandingBoltProjectile", vector3, matchFacing: false, 180f, 0f, playSummonAudio: false);
		vector3.x = vector2.x - num * 2f;
		vector3.y = vector2.y + 0f;
		this.SummonProjectile("DancingBossStandingBoltProjectile", vector3, matchFacing: false, 180f, 0f, playSummonAudio: false);
		vector3 = vector2;
		vector3.x = vector2.x + num * 2f;
		vector3.y = vector2.y + num;
		this.SummonProjectile("DancingBossStandingBoltProjectile", vector3, matchFacing: false, 180f, 0f, playSummonAudio: false);
		vector3.x = vector2.x + num * 2f;
		vector3.y = vector2.y + 0f;
		this.SummonProjectile("DancingBossStandingBoltProjectile", vector3, matchFacing: false, 180f, 0f, playSummonAudio: false);
		vector3.x = vector2.x - num;
		vector3.y = vector2.y - num;
		this.SummonProjectile("DancingBossStandingBoltProjectile", vector3, matchFacing: false, 180f, 0f, playSummonAudio: false);
		vector3.x = vector2.x + num;
		vector3.y = vector2.y - num;
		this.SummonProjectile("DancingBossStandingBoltProjectile", vector3, matchFacing: false, 180f, 0f, playSummonAudio: false);
		vector3 = vector2;
		vector3.y = vector2.y - num * 2f;
		this.SummonProjectile("DancingBossStandingBoltProjectile", vector3, matchFacing: false, 90f, 0f, playSummonAudio: false);
		if (m_summonShots_AttackHoldExit_Delay > 0f)
		{
			yield return Wait(m_summonShots_AttackHoldExit_Delay);
		}
		SummonShotsAttackCompleteRelay.Dispatch();
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("CircularBurst_Exit", m_summonShots_AttackExit_AnimSpeed, m_summonShots_AttackExit_Delay);
		yield return Default_Attack_Cooldown(m_summonShots_AttackExit_IdleDuration, m_summonShots_AttackExit_CooldownDuration);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetTurnTrigger();
		base.EnemyController.ResetBaseValues();
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
		this.ToDo("*** Dash ***");
		if (m_dash_Attack_Pause_While_Attacking)
		{
			StopAndFaceTarget();
		}
		yield return Default_TellIntroAndLoop("Dash_Tell_Intro", m_dash_TellIntro_AnimSpeed, "Dash_Tell_Hold", m_dash_TellHold_AnimSpeed, m_dash_TellIntroAndHold_Duration);
		SetAttackingWithContactDamage(enable: true);
		base.EnemyController.Heading = base.EnemyController.TargetController.Midpoint - base.EnemyController.Midpoint;
		if (m_dash_Attack_Pause_While_Attacking)
		{
			base.EnemyController.LockFlip = true;
			base.EnemyController.FollowTarget = false;
		}
		yield return Default_Animation("Dash_Attack_Intro", m_dash_AttackIntro_AnimSpeed, m_dash_AttackIntro_Delay);
		yield return Default_Animation("Dash_Attack_Hold", m_dash_AttackHold_AnimSpeed, m_dash_AttackHold_Delay, waitUntilAnimationComplete: false);
		if (!m_dash_Attack_Pause_While_Attacking)
		{
			base.EnemyController.LockFlip = true;
			base.EnemyController.FollowTarget = false;
		}
		base.EnemyController.FlyingMovementType = FlyingMovementType.Override;
		Vector2 vector = base.EnemyController.TargetController.Midpoint - base.EnemyController.Midpoint;
		Vector2 vector2 = m_dash_Attack_ForwardSpeed * vector.normalized;
		base.EnemyController.SetVelocity(vector2.x, vector2.y, additive: false);
		if (m_dash_Attack_Duration > 0f)
		{
			yield return Wait(m_dash_Attack_Duration);
		}
		SetAttackingWithContactDamage(enable: false);
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		yield return Default_Animation("Dash_Exit", m_dash_AttackExit_AnimSpeed, m_dash_AttackExit_Delay);
		if (!m_advancedAttacks)
		{
			this.FireProjectile("DancingBossWaveBoltProjectile", 0, matchFacing: false, 0f, 1.1f);
			this.FireProjectile("DancingBossWaveBoltProjectile", 0, matchFacing: false, 45f, 1.1f);
			this.FireProjectile("DancingBossWaveBoltProjectile", 0, matchFacing: false, 90f, 1.1f);
			this.FireProjectile("DancingBossWaveBoltProjectile", 0, matchFacing: false, 135f, 1.1f);
			this.FireProjectile("DancingBossWaveBoltProjectile", 0, matchFacing: false, 180f, 1.1f);
			this.FireProjectile("DancingBossWaveBoltProjectile", 0, matchFacing: false, 225f, 1.1f);
			this.FireProjectile("DancingBossWaveBoltProjectile", 0, matchFacing: false, 270f, 1.1f);
			this.FireProjectile("DancingBossWaveBoltProjectile", 0, matchFacing: false, 315f, 1.1f);
		}
		else
		{
			float speedmod = 0.35f;
			for (int i = 0; i < 3; i++)
			{
				this.FireProjectile("DancingBossWaveBoltProjectile", 0, matchFacing: false, 0f, 1.1f + (float)i * speedmod);
				this.FireProjectile("DancingBossWaveBoltProjectile", 0, matchFacing: false, 45f, 1.1f + (float)i * speedmod);
				this.FireProjectile("DancingBossWaveBoltProjectile", 0, matchFacing: false, 90f, 1.1f + (float)i * speedmod);
				this.FireProjectile("DancingBossWaveBoltProjectile", 0, matchFacing: false, 135f, 1.1f + (float)i * speedmod);
				this.FireProjectile("DancingBossWaveBoltProjectile", 0, matchFacing: false, 180f, 1.1f + (float)i * speedmod);
				this.FireProjectile("DancingBossWaveBoltProjectile", 0, matchFacing: false, 225f, 1.1f + (float)i * speedmod);
				this.FireProjectile("DancingBossWaveBoltProjectile", 0, matchFacing: false, 270f, 1.1f + (float)i * speedmod);
				this.FireProjectile("DancingBossWaveBoltProjectile", 0, matchFacing: false, 315f, 1.1f + (float)i * speedmod);
			}
			yield return Wait(0.5f);
			this.FireProjectile("DancingBossVoidProjectile", 0, matchFacing: false, 22.5f, 1.1f + 4f * speedmod);
			this.FireProjectile("DancingBossVoidProjectile", 0, matchFacing: false, 67.5f, 1.1f + 4f * speedmod);
			this.FireProjectile("DancingBossVoidProjectile", 0, matchFacing: false, 112.5f, 1.1f + 4f * speedmod);
			this.FireProjectile("DancingBossVoidProjectile", 0, matchFacing: false, 157.5f, 1.1f + 4f * speedmod);
			this.FireProjectile("DancingBossVoidProjectile", 0, matchFacing: false, 202.5f, 1.1f + 4f * speedmod);
			this.FireProjectile("DancingBossVoidProjectile", 0, matchFacing: false, 247.5f, 1.1f + 4f * speedmod);
			this.FireProjectile("DancingBossVoidProjectile", 0, matchFacing: false, 292.5f, 1.1f + 4f * speedmod);
			this.FireProjectile("DancingBossVoidProjectile", 0, matchFacing: false, 337.5f, 1.1f + 4f * speedmod);
		}
		base.EnemyController.FollowTarget = true;
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetTurnTrigger();
		base.EnemyController.ResetBaseValues();
		float dash_AttackExit_IdleDuration = m_dash_AttackExit_IdleDuration;
		yield return Default_Attack_Cooldown(dash_AttackExit_IdleDuration, m_dash_AttackExit_CooldownDuration);
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
	public IEnumerator Mode_Shift_01()
	{
		RemoveStatusEffects(includeCommanderBuffs: false);
		base.EnemyController.StatusEffectController.ImmuneToAllStatusEffects = true;
		yield return base.DeathAnim();
		this.ToDo("*** MODE SHIFT 01 ***");
		MusicManager.SetBossEncounterParam(0.51f);
		base.EnemyController.LockFlip = true;
		SetVelocity(0f, 0f, additive: false);
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		base.EnemyController.ModeshiftDamageMod = m_modeShift01_Damage_Mod;
		this.ToDo("*** DOWNED ***");
		yield return Default_Animation("ModeShift_Intro", m_modeShift01_Downed_AnimSpeed, m_modeShift01_Downed_Delay);
		this.ToDo("*** GETTING UP ***");
		yield return this.ChangeAnimationState("ModeShift_Scream_Intro");
		yield return WaitUntilAnimComplete(DefaultAnimationLayer);
		this.ToDo("*** WARNING ***");
		yield return this.ChangeAnimationState("ModeShift_Scream_Hold");
		yield return WaitUntilAnimComplete(DefaultAnimationLayer);
		if (m_modeShift01_Warning_Delay > 0f)
		{
			yield return Wait(m_modeShift01_Warning_Delay);
		}
		this.ToDo("*** SUMMON SENTRYS ***");
		yield return SummonSentries();
		SetAttackingWithContactDamage(enable: false);
		yield return this.ChangeAnimationState("ModeShift_Scream_Exit");
		yield return WaitUntilAnimComplete(DefaultAnimationLayer);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ModeshiftDamageMod = 1f;
		base.EnemyController.StatusEffectController.ImmuneToAllStatusEffects = false;
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_modeShift01_Exit_IdleDuration, m_modeShift01_AttackCD);
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
	public IEnumerator Mode_Shift_02()
	{
		this.ToDo("*** MODE SHIFT 02 ***");
		base.EnemyController.LockFlip = true;
		SetVelocity(0f, 0f, additive: false);
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		base.EnemyController.ModeshiftDamageMod = m_modeShift01_Damage_Mod;
		this.ToDo("*** DOWNED ***");
		yield return Default_Animation("Stunned", m_modeShift02_Downed_AnimSpeed, m_modeShift02_Downed_Delay);
		this.ToDo("*** GETTING UP ***");
		yield return Default_Animation("Neutral", m_modeShift02_GetUp_AnimSpeed, m_modeShift02_GetUp_Delay);
		this.ToDo("*** FLY TO CENTRE OF ROOM ***");
		base.EnemyController.Target = m_centerOfRoom;
		base.EnemyController.FollowTarget = true;
		base.EnemyController.FollowOffset = Vector3.zero;
		base.EnemyController.FlyingMovementType = FlyingMovementType.Towards;
		base.LogicController.ForceExecuteLogicBlockName_OnceOnly = "WalkTowards";
		float sqrMagnitudeToCentreOfRoom = (base.EnemyController.Target.transform.position - base.EnemyController.transform.position).sqrMagnitude;
		while (sqrMagnitudeToCentreOfRoom > 0.5f)
		{
			sqrMagnitudeToCentreOfRoom = (base.EnemyController.Target.transform.position - base.EnemyController.transform.position).sqrMagnitude;
			yield return null;
		}
		base.EnemyController.Target = PlayerManager.GetPlayerController().gameObject;
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		base.EnemyController.SetVelocity(0f, 0f, additive: false);
		this.ToDo("*** PERFORM FINAL DANCE ***");
		yield return Default_TellIntroAndLoop("Shoot_Tell_Intro", m_finalDance_TellIntro_AnimSpeed, "Shoot_Tell_Hold", m_finalDance_TellHold_AnimSpeed, m_finalDance_TellIntroAndHold_Duration);
		base.EnemyController.LockFlip = true;
		base.EnemyController.FollowTarget = false;
		yield return Default_Animation("Shoot_Attack_Intro", m_finalDance_AttackIntro_AnimSpeed, m_finalDance_AttackIntro_Delay);
		yield return Default_Animation("Shoot_Attack_Hold", m_finalDance_AttackHold_AnimSpeed, m_finalDance_AttackHold_Delay, waitUntilAnimationComplete: false);
		if (m_finalDance_AttackHoldExit_Delay > 0f)
		{
			yield return Wait(m_finalDance_AttackHoldExit_Delay);
		}
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		yield return Default_Animation("Neutral", m_finalDance_AttackExit_AnimSpeed, m_finalDance_AttackExit_Delay);
		this.ToDo("*** WARNING ***");
		m_modeShift02WarningProjectile = this.FireProjectile("SpellSwordShoutWarningProjectile", 0);
		if (m_modeShift02_Warning_Delay > 0f)
		{
			yield return Wait(m_modeShift02_Warning_Delay);
		}
		StopProjectile(ref m_modeShift02WarningProjectile);
		this.ToDo("*** SUMMON SENTRYS ***");
		yield return SummonSentries();
		this.ToDo("*** FIRE PROJECTILES ***");
		yield return FireProjectilesInRandomDirections();
		yield return Wait(9999999f);
	}

	private IEnumerator FireProjectilesInRandomDirections()
	{
		yield break;
	}

	private IEnumerator SummonSentries()
	{
		SummonSentriesStartRelay.Dispatch();
		GameObject[] points = m_sentryHazards.Select((Sentry_Hazard entry) => entry.gameObject).ToArray();
		yield return PlaySummoningEffect(points);
		for (int i = 0; i < m_sentryHazards.Count; i++)
		{
			m_sentryHazards[i].gameObject.SetActive(value: true);
			m_sentryHazards[i].StartInRestState(0f);
		}
		SummonSentriesCompleteRelay.Dispatch();
	}

	public override IEnumerator SpawnAnim()
	{
		yield return Default_Animation("Intro_Idle", m_spawn_Idle_AnimSpeed, m_spawn_Idle_Delay);
		MusicManager.PlayMusic(SongID.ForestBossBGM_Tettix_Naamah_180_BPM, isOverride: false);
		yield return Default_Animation("Intro", m_spawn_Intro_AnimSpeed, m_spawn_Intro_Delay);
		yield return Default_Animation(BaseAIScript.NEUTRAL_STATE, 1f, 1f, waitUntilAnimationComplete: false);
	}

	public override IEnumerator DeathAnim()
	{
		yield return base.DeathAnim();
		RLTimeScale.SetTimeScale(TimeScaleType.Cutscene, 0.5f);
		yield return Default_Animation("Death_Intro", m_death_Intro_AnimSpeed, m_death_Intro_Delay);
		RLTimeScale.SetTimeScale(TimeScaleType.Cutscene, 1f);
		for (int i = 0; i < m_sentryHazards.Count; i++)
		{
			if (m_sentryHazards[i].isActiveAndEnabled)
			{
				m_sentryHazards[i].ActivateRestState();
			}
		}
		yield return Wait(0.5f, unscaledTime: true);
		for (int j = 0; j < m_sentryHazards.Count; j++)
		{
			m_sentryHazards[j].gameObject.SetActive(value: false);
		}
		base.EnemyController.GetComponent<BossModeShiftController>().DissolveMaterials(m_death_Hold_Delay);
		yield return Default_Animation("Death_Loop", m_death_Hold_AnimSpeed, m_death_Hold_Delay);
	}

	public override void OnLBCompleteOrCancelled()
	{
		StopProjectile(ref m_modeShift01WarningProjectile);
		StopProjectile(ref m_modeShift02WarningProjectile);
		base.OnLBCompleteOrCancelled();
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		base.LogicController.DisableLogicActivationByDistance = true;
		base.EnemyController.Target = PlayerManager.GetPlayerController().gameObject;
		base.EnemyController.FlyingMovementType = FlyingMovementType.Towards;
	}

	private void InitializeSentryHazards()
	{
		if (m_sentryHazards == null)
		{
			m_sentryHazards = new List<Sentry_Hazard>();
		}
		else
		{
			m_sentryHazards.Clear();
		}
		ISpawnController[] spawnControllers = base.EnemyController.Room.SpawnControllerManager.SpawnControllers;
		for (int i = 0; i < spawnControllers.Length; i++)
		{
			IHazardSpawnController hazardSpawnController = spawnControllers[i] as IHazardSpawnController;
			if (!hazardSpawnController.IsNativeNull() && hazardSpawnController.Type == HazardType.Sentry)
			{
				Sentry_Hazard sentry_Hazard = hazardSpawnController.Hazard as Sentry_Hazard;
				if ((bool)sentry_Hazard)
				{
					sentry_Hazard.SetRestDurationOverride(m_sentryRestDuration);
					sentry_Hazard.gameObject.SetActive(value: false);
					m_sentryHazards.Add(sentry_Hazard);
				}
			}
		}
		if (!m_centerOfRoom)
		{
			m_centerOfRoom = new GameObject("Center of Room");
		}
		m_centerOfRoom.transform.SetParent(base.EnemyController.Room.gameObject.transform);
		m_centerOfRoom.transform.localPosition = Vector3.zero;
	}

	public override void OnEnemyActivated()
	{
		base.OnEnemyActivated();
		base.EnemyController.HealthChangeRelay.AddListener(m_onBossHit);
		CreateSummoningPoints();
		InitializeSentryHazards();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if ((bool)base.EnemyController && base.EnemyController.HealthChangeRelay != null)
		{
			base.EnemyController.HealthChangeRelay.RemoveListener(m_onBossHit);
		}
	}

	private void FireWave(RoomSide side, int basicProjectileAmount, int bounceProjectileAmount, float projectileSpeedMod, bool addSummoningDelay = true, bool isVerticalWave = false)
	{
		StartCoroutine(FireWaveCoroutine(side, basicProjectileAmount, bounceProjectileAmount, projectileSpeedMod, addSummoningDelay, isVerticalWave));
	}

	private IEnumerator FireWaveCoroutine(RoomSide side, int BasicProjectileAmount, int BounceProjectileAmount, float ProjectileSpeedMod, bool addSummoningDelay, bool isVerticalWave)
	{
		float projectileAngle = GetProjectileAngle(side);
		GameObject[] summoningPoints = GetSummoningPoints(side);
		int num = UnityEngine.Random.Range(0, BasicProjectileAmount - BounceProjectileAmount + 1);
		int num2 = 0;
		DispatchWaveRelay(side, summoningPoints, ProjectileSpeedMod);
		for (int i = 0; i < summoningPoints.Length; i++)
		{
			string projectileName = "DancingBossWaveBounceProjectile";
			if (i >= num && num2 < BounceProjectileAmount)
			{
				projectileName = "DancingBossWaveBounceProjectile";
				num2++;
			}
			GameObject summonPoint = summoningPoints[i];
			SummonProjectile(summonPoint, projectileName, matchFacing: false, projectileAngle, ProjectileSpeedMod, playSummonAudio: false, playSpawnAudio: false, playLifetimeAudio: false);
			if (m_advancedAttacks && !isVerticalWave)
			{
				SummonProjectile(summonPoint, "DancingBossWavePassBoltProjectile", matchFacing: false, projectileAngle, ProjectileSpeedMod, playSummonAudio: false, playSpawnAudio: false, playLifetimeAudio: false);
			}
		}
		yield break;
	}

	private void DispatchWaveRelay(RoomSide side, GameObject[] summonPoints, float speedMod)
	{
		Vector2 origin = Vector2.zero;
		switch (side)
		{
		case RoomSide.Left:
		case RoomSide.Right:
			origin = new Vector2(summonPoints[0].transform.position.x, base.EnemyController.Room.Bounds.center.y);
			break;
		case RoomSide.Top:
		case RoomSide.Bottom:
			origin = new Vector2(base.EnemyController.Room.Bounds.center.x, summonPoints[0].transform.position.y);
			break;
		}
		Projectile_RL projectile = ProjectileLibrary.GetProjectile("DancingBossWaveBoltProjectile");
		float speed = projectile.ProjectileData.Speed;
		float speed2 = (float)m_verticalWave_ProjectileSpeedMod * speed;
		float lifespan = projectile.Lifespan;
		if (m_waveEventArgs == null)
		{
			m_waveEventArgs = new DancingBossWaveFiredEventArgs(side, origin, speed2, lifespan);
		}
		else
		{
			m_waveEventArgs.Initialize(side, origin, speed2, lifespan);
		}
		WaveFiredRelay.Dispatch(m_waveEventArgs);
	}

	private IEnumerator FireMultiWaveSpreadCoroutine(RoomSide side, int BasicProjectileAmount, int BounceProjectileAmount, float ProjectileSpeedMod, bool addSummoningDelay)
	{
		float angle = GetProjectileAngle(side);
		GameObject[] summonPoints = GetSummoningPoints(side);
		string projectileName = "DancingBossWaveBoltProjectile";
		int setBounceProjectileLocation = UnityEngine.Random.Range(0, BasicProjectileAmount - BounceProjectileAmount + 1);
		int bounceProjectilesSpawnedCounter = 0;
		int midPoint = Mathf.FloorToInt((float)summonPoints.Length / 2f);
		SummonProjectile(summonPoints[midPoint], projectileName, matchFacing: false, angle, ProjectileSpeedMod);
		yield return Wait(m_spread_TimeBetweenAttacks);
		for (int i = 1; i < midPoint; i++)
		{
			projectileName = "DancingBossWaveBoltProjectile";
			if (i >= setBounceProjectileLocation && bounceProjectilesSpawnedCounter < BounceProjectileAmount)
			{
				projectileName = "DancingBossWaveBounceProjectile";
				bounceProjectilesSpawnedCounter++;
			}
			GameObject summonPoint = summonPoints[summonPoints.Length / 2 - i];
			GameObject summonPoint2 = summonPoints[summonPoints.Length / 2 + i];
			SummonProjectile(summonPoint, projectileName, matchFacing: false, angle, ProjectileSpeedMod);
			SummonProjectile(summonPoint2, projectileName, matchFacing: false, angle, ProjectileSpeedMod);
			yield return Wait(m_spread_TimeBetweenAttacks);
		}
	}

	private void SummonProjectile(GameObject summonPoint, string projectileName, bool matchFacing, float angle, float speedMod, bool playSummonAudio = true, bool playSpawnAudio = true, bool playLifetimeAudio = true, bool playDeathAudio = true)
	{
		Vector2 offset = summonPoint.transform.position - base.EnemyController.transform.position;
		this.SummonProjectile(projectileName, offset, matchFacing, angle, speedMod, playSummonAudio, playSpawnAudio, playLifetimeAudio, playDeathAudio);
	}

	private void OnBossHit(object sender, HealthChangeEventArgs args)
	{
		if (m_currentMode < 1 && !base.EnemyController.IsDead && !(args.PrevHealthValue <= args.NewHealthValue) && base.EnemyController.CurrentHealth <= (float)base.EnemyController.ActualMaxHealth * m_firstModeShiftHealthMod)
		{
			m_currentMode = 1;
			string forceExecuteLogicBlockName_OnceOnly = "Mode_Shift_01";
			base.LogicController.StopAllLogic(resetCurrentState: true);
			base.LogicController.ForceExecuteLogicBlockName_OnceOnly = forceExecuteLogicBlockName_OnceOnly;
			if (m_modeShiftEventArgs == null)
			{
				m_modeShiftEventArgs = new EnemyModeShiftEventArgs(base.EnemyController);
			}
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.EnemyModeShift, this, m_modeShiftEventArgs);
			base.EnemyController.HealthChangeRelay.RemoveListener(OnBossHit);
		}
	}

	private static RoomSide GetRandomSide(params RoomSide[] sides)
	{
		int num = UnityEngine.Random.Range(0, sides.Length);
		return sides[num];
	}

	private float GetProjectileAngle(RoomSide side)
	{
		float num = 0f;
		return side switch
		{
			RoomSide.Left => 0f, 
			RoomSide.Right => 180f, 
			RoomSide.Top => 270f, 
			_ => 90f, 
		};
	}

	private GameObject[] GetSummoningPoints(RoomSide side)
	{
		GameObject[] result = null;
		switch (side)
		{
		case RoomSide.Top:
			result = m_topVerticalProjectileSummonPoints;
			break;
		case RoomSide.Bottom:
			result = m_bottomVerticalProjectileSummonPoints;
			break;
		case RoomSide.Left:
			result = m_leftHorizontalProjectileSummonPoints;
			break;
		case RoomSide.Right:
			result = m_rightHorizontalProjectileSummonPoints;
			break;
		}
		return result;
	}

	private float GetSummonEffectDuration()
	{
		return 1.2f;
	}

	protected IEnumerator PlaySummoningEffect(GameObject[] points)
	{
		float summonEffectDuration = GetSummonEffectDuration();
		for (int i = 0; i < points.Length; i++)
		{
			Vector3 position = points[i].transform.position;
			EffectManager.PlayEffect(null, null, "SpellCircle_Effect", position, summonEffectDuration, EffectStopType.Gracefully);
		}
		yield return Wait(summonEffectDuration);
	}

	private void CreateSummoningPoints()
	{
		m_areSummoningPointsInitialized = true;
		_ = (base.EnemyController.Room as Room).UnitDimensions;
		float distanceBetweenProjectiles = GetDistanceBetweenProjectiles(RoomSide.Left);
		m_leftHorizontalProjectileSummonPoints = new GameObject[m_horizontalWave_ProjectileCount];
		m_rightHorizontalProjectileSummonPoints = new GameObject[m_horizontalWave_ProjectileCount];
		Vector2 initialPosition = GetInitialPosition(RoomSide.Left);
		Vector2 initialPosition2 = GetInitialPosition(RoomSide.Right);
		for (int i = 0; i < m_horizontalWave_ProjectileCount; i++)
		{
			Vector2 position = initialPosition + (float)i * distanceBetweenProjectiles * Vector2.up;
			GameObject gameObject = CreateProjectileSummonPoint(RoomSide.Left, position);
			m_leftHorizontalProjectileSummonPoints[i] = gameObject;
			Vector2 position2 = initialPosition2 + (float)i * distanceBetweenProjectiles * Vector2.up;
			GameObject gameObject2 = CreateProjectileSummonPoint(RoomSide.Right, position2);
			m_rightHorizontalProjectileSummonPoints[i] = gameObject2;
		}
		float distanceBetweenProjectiles2 = GetDistanceBetweenProjectiles(RoomSide.Top);
		m_topVerticalProjectileSummonPoints = new GameObject[m_verticalWave_ProjectileCount];
		m_bottomVerticalProjectileSummonPoints = new GameObject[m_verticalWave_ProjectileCount];
		Vector2 initialPosition3 = GetInitialPosition(RoomSide.Top);
		Vector2 initialPosition4 = GetInitialPosition(RoomSide.Bottom);
		for (int j = 0; j < m_verticalWave_ProjectileCount; j++)
		{
			Vector2 position3 = initialPosition3 + (float)j * distanceBetweenProjectiles2 * Vector2.right;
			GameObject gameObject3 = CreateProjectileSummonPoint(RoomSide.Top, position3);
			m_topVerticalProjectileSummonPoints[j] = gameObject3;
			Vector2 position4 = initialPosition4 + (float)j * distanceBetweenProjectiles2 * Vector2.right;
			GameObject gameObject4 = CreateProjectileSummonPoint(RoomSide.Bottom, position4);
			m_bottomVerticalProjectileSummonPoints[j] = gameObject4;
		}
		m_summonProjectileSummonPoints = new GameObject[m_summonShots_ProjectileCount];
		for (int k = 0; k < m_summonShots_ProjectileCount; k++)
		{
			GameObject gameObject5 = CreateProjectileSummonPoint(RoomSide.None, Vector2.zero);
			m_summonProjectileSummonPoints[k] = gameObject5;
		}
	}

	private void ResetSummoningPointPositions(RoomSide side)
	{
		float distanceBetweenProjectiles = GetDistanceBetweenProjectiles(side);
		Vector2 initialPosition = GetInitialPosition(side);
		GameObject[] summoningPoints = GetSummoningPoints(side);
		for (int i = 0; i < summoningPoints.Length; i++)
		{
			Vector2 vector = ((side != RoomSide.Left && side != RoomSide.Right) ? (initialPosition + (float)i * distanceBetweenProjectiles * Vector2.right) : (initialPosition + (float)i * distanceBetweenProjectiles * Vector2.up));
			summoningPoints[i].transform.position = vector;
		}
	}

	private Vector2 GetInitialPosition(RoomSide side)
	{
		Vector2 result;
		if (side == RoomSide.Left || side == RoomSide.Right)
		{
			float x = base.EnemyController.Room.Bounds.min.x;
			int num = 1;
			if (side == RoomSide.Right)
			{
				x = base.EnemyController.Room.Bounds.max.x;
				num = -1;
			}
			x += (float)num * 10f;
			float y = base.EnemyController.Room.Bounds.center.y - 0.5f * GetProjectileSpread(side);
			result = new Vector2(x, y);
		}
		else
		{
			float x2 = base.EnemyController.Room.Bounds.center.x - 0.5f * GetProjectileSpread(side);
			float y2 = base.EnemyController.Room.Bounds.min.y;
			int num2 = 1;
			if (side == RoomSide.Top)
			{
				y2 = base.EnemyController.Room.Bounds.max.y;
				num2 = -1;
			}
			y2 += (float)num2 * 0f;
			result = new Vector2(x2, y2);
		}
		return result;
	}

	private float GetDistanceBetweenProjectiles(RoomSide side)
	{
		int num = ((side != RoomSide.Left && side != RoomSide.Right) ? m_verticalWave_ProjectileCount : m_horizontalWave_ProjectileCount);
		return GetProjectileSpread(side) / (float)(num - 1);
	}

	private float GetProjectileSpread(RoomSide side)
	{
		Vector2Int unitDimensions = (base.EnemyController.Room as Room).UnitDimensions;
		if (side == RoomSide.Left || side == RoomSide.Right)
		{
			return (float)unitDimensions.y - 0f;
		}
		return (float)unitDimensions.x - 20f;
	}

	private GameObject CreateProjectileSummonPoint(RoomSide side, Vector2 position)
	{
		if (!m_summonPointStorageLocation)
		{
			GameObject gameObject = new GameObject("Summoning Points");
			gameObject.transform.SetParent(base.EnemyController.Room.gameObject.transform);
			gameObject.transform.localPosition = Vector3.zero;
			m_summonPointStorageLocation = gameObject.transform;
		}
		GameObject obj = new GameObject("Projectile Summoning Point - " + side);
		obj.transform.SetParent(m_summonPointStorageLocation);
		obj.transform.position = position;
		return obj;
	}

	public override void ResetScript()
	{
		base.EnemyController.StatusEffectController.ImmuneToAllStatusEffects = false;
		m_currentMode = 0;
		if ((bool)m_summonPointStorageLocation)
		{
			UnityEngine.Object.Destroy(m_summonPointStorageLocation.gameObject);
		}
		base.ResetScript();
	}
}
