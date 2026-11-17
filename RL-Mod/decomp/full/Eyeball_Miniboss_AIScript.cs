using System.Collections;
using System.Collections.Generic;
using Sigtrap.Relays;
using UnityEngine;

public class Eyeball_Miniboss_AIScript : Eyeball_Basic_AIScript
{
	private const float PROJECTILE_SPAWN_X_OFFSET = 3f;

	public const EnemyRank MODESHIFT_EYE_RANK = EnemyRank.Expert;

	private static List<Eyeball_Miniboss_AIScript> m_eyeballSyncList_STATIC = new List<Eyeball_Miniboss_AIScript>();

	protected const string ZONE_BLAST_TELL_INTRO = "AreaShot_Tell_Intro";

	protected const string ZONE_BLAST_TELL_HOLD = "AreaShot_Tell_Hold";

	protected const string ZONE_BLAST_ATTACK_INTRO = "AreaShot_Attack_Intro";

	protected const string ZONE_BLAST_ATTACK_HOLD = "AreaShot_Attack_Hold";

	protected const string ZONE_BLAST_EXIT = "AreaShot_Exit";

	protected const string EXPERT_ZONE_BLAST_TELL_INTRO = "Explosion_Tell_Intro";

	protected const string EXPERT_ZONE_BLAST_TELL_HOLD = "Explosion_Tell_Hold";

	protected const string EXPERT_ZONE_BLAST_ATTACK_INTRO = "Explosion_Attack_Intro";

	protected const string EXPERT_ZONE_BLAST_ATTACK_HOLD = "Explosion_Attack_Hold";

	protected const string EXPERT_ZONE_BLAST_EXIT = "Explosion_Exit";

	protected const string ZONE_BULLET_PROJECTILE = "EyeballPassLineBoltProjectile";

	protected const string ZONE_BLAST_PROJECTILE = "EyeballExplosionProjectile";

	protected const string ZONE_BLASTWARNING_PROJECTILE = "EyeballWarningProjectile";

	private Relay m_shoutAttackWarningAppearedRelay = new Relay();

	private Relay m_shoutAttackExplodedRelay = new Relay();

	protected const string SPINNING_FIREBALL_TELL_INTRO = "BurstShot_Tell_Intro";

	protected const string SPINNING_FIREBALL_TELL_HOLD = "BurstShot_Tell_Hold";

	protected const string SPINNING_FIREBALL_ATTACK_INTRO = "BurstShot_Attack_Intro";

	protected const string SPINNING_FIREBALL_ATTACK_HOLD = "BurstShot_Attack_Hold";

	protected const string SPINNING_FIREBALL_EXIT = "BurstShot_Exit";

	protected const string SPINNING_FIREBALL_PROJECTILE = "EyeballBoltMinibossProjectile";

	private Projectile_RL m_shoutWarningProjectile;

	protected const string HOMING_FIREBALL_TELL_INTRO = "HomingShot_Tell_Intro";

	protected const string HOMING_FIREBALL_TELL_HOLD = "HomingShot_Tell_Hold";

	protected const string HOMING_FIREBALL_ATTACK_INTRO = "HomingShot_Attack_Intro";

	protected const string HOMING_FIREBALL_ATTACK_HOLD = "HomingShot_Attack_Hold";

	protected const string HOMING_FIREBALL_EXIT = "HomingShot_Exit";

	protected const string EXPERT_HOMING_FIREBALL_TELL_INTRO = "BounceBullets_Tell_Intro";

	protected const string EXPERT_HOMING_FIREBALL_TELL_HOLD = "BounceBullets_Tell_Hold";

	protected const string EXPERT_HOMING_FIREBALL_ATTACK_INTRO = "BounceBullets_Attack_Intro";

	protected const string EXPERT_HOMING_FIREBALL_ATTACK_HOLD = "BounceBullets_Attack_Hold";

	protected const string EXPERT_HOMING_FIREBALL_EXIT = "BounceBullets_Exit";

	protected const string HOMING_FIREBALL_PROJECTILE = "EyeballBounceBoltMinibossProjectile";

	protected const string CURSE_FIREBALL_PROJECTILE = "EyeballCurseBoltProjectile";

	protected const string SPAWN_IDLE = "Intro_Idle";

	protected const string SPAWN_INTRO = "Closed_Moving";

	protected const string SPAWN_OPEN = "Open";

	protected static int m_spawnOrder_STATIC = 0;

	protected const int NUM_OF_STARTING_EYES = 2;

	protected float m_spawn_Idle_AnimSpeed = 1f;

	protected float m_spawn_Idle_Delay = 1f;

	protected float m_spawn_Intro_AnimSpeed = 1f;

	protected float m_spawn_Intro_Delay = 0.3f;

	protected float m_spawn_Open_AnimSpeed = 1f;

	protected float m_spawn_Open_Delay = 1f;

	protected float m_secondSpawn_Idle_Delay = 1.2f;

	protected float m_secondSpawn_Intro_Delay = 0.4f;

	protected const string DEATH_INTRO = "Death_Intro";

	protected const string DEATH_HOLD = "Death_Loop";

	protected float m_death_Intro_AnimSpeed = 1f;

	protected float m_death_Intro_Delay;

	protected float m_death_Hold_AnimSpeed = 1f;

	protected float m_death_Hold_Delay = 4.5f;

	private const float m_global_AnimationOverride = 1f;

	private bool m_modeShifted;

	private const string MODE_SHIFT_INTRO = "ModeShift_Intro";

	private const string MODE_SHIFT_SCREAM_INTRO = "ModeShift_Scream_Intro";

	private const string MODE_SHIFT_SCREAM_HOLD = "ModeShift_Scream_Hold";

	private const string MODE_SHIFT_EXIT = "ModeShift_Scream_Exit";

	private const float m_modeShift_Intro_AnimSpeed = 1f;

	private const float m_modeShift_Intro_Delay = 3.25f;

	private const float m_modeShift_Scream_Intro_AnimSpeed = 1f;

	private const float m_modeShift_Scream_Intro_Delay = 0f;

	private const float m_modeShift_Scream_Hold_AnimSpeed = 1f;

	private const float m_modeShift_Scream_Hold_Delay = 1.25f;

	private const float m_modeShift_Exit_AnimSpeed = 1f;

	private const float m_modeShift_Exit_Delay = 0f;

	private const float m_modeShift_IdleDuration = 0.1f;

	private const float m_modeShift_AttackCD = 99999f;

	public const float ModeShift_HealthMod = 0.5f;

	protected float m_modeShift_Damage_Mod = 0.1f;

	private const float m_modeShift_openEye_Idle_AnimSpeed = 1f;

	private const float m_modeShift_openEye_Idle_Delay = 0.5f;

	private const float m_modeShift_openEye_Intro_AnimSpeed = 1f;

	private const float m_modeShift_openEye_Intro_Delay = 0.75f;

	private const float m_modeShift_openEye_Open_AnimSpeed = 1f;

	private const float m_modeShift_openEye_Open_Delay = 1f;

	public bool IsReady { get; private set; }

	protected virtual float ZoneBlast_TellIntro_AnimSpeed => 1f;

	protected virtual float ZoneBlast_TellHold_AnimSpeed => 1.75f;

	protected virtual float ZoneBlast_TellHold_Duration => 1f;

	protected virtual float ZoneBlast_AttackIntro_AnimSpeed => 1f;

	protected virtual float ZoneBlast_AttackIntro_Delay => 0f;

	protected virtual float ZoneBlast_AttackHold_AnimSpeed => 1f;

	protected virtual float ZoneBlast_AttackHold_Delay => 0f;

	protected virtual float ZoneBlast_Exit_AnimSpeed => 1f;

	protected virtual float ZoneBlast_Exit_Duration => 0f;

	protected virtual float ZoneBlast_Exit_ForceIdle => 0.15f;

	protected virtual float ZoneBlast_Exit_AttackCD => 0.75f;

	protected virtual int ZoneBlast_Attack_Amount => 5;

	protected virtual float ZoneBlast_AttackHold_DelayBetweenShots => 0f;

	protected virtual float ZoneBlast_Attack_AngleSpread => 75f;

	protected virtual bool ZoneBlast_Sync => false;

	public IRelayLink ShoutAttackWarningAppearedRelay => m_shoutAttackWarningAppearedRelay.link;

	public IRelayLink ShoutAttackExplodedRelay => m_shoutAttackExplodedRelay.link;

	protected virtual int Spinning_Fireball_Amount => 8;

	protected virtual int SpinngFireball_Angle => 30;

	protected virtual bool SpinningFireball_Sync => false;

	protected virtual bool HomingFireball_Sync => false;

	public static bool CanFire()
	{
		for (int i = 0; i < m_eyeballSyncList_STATIC.Count; i++)
		{
			if (!m_eyeballSyncList_STATIC[i])
			{
				m_eyeballSyncList_STATIC.RemoveAt(i);
				i--;
			}
			else if (!m_eyeballSyncList_STATIC[i].IsReady && !m_eyeballSyncList_STATIC[i].EnemyController.IsDead)
			{
				return false;
			}
		}
		return true;
	}

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[6] { "EyeballBoltMinibossProjectile", "EyeballBounceBoltMinibossProjectile", "EyeballExplosionProjectile", "EyeballPassLineBoltProjectile", "EyeballWarningProjectile", "EyeballCurseBoltProjectile" };
	}

	private void OnEnable()
	{
		IsReady = false;
		m_modeShifted = false;
		if (!m_eyeballSyncList_STATIC.Contains(this))
		{
			m_eyeballSyncList_STATIC.Add(this);
		}
		if (base.IsInitialized)
		{
			InitializeModeShiftEye();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (m_eyeballSyncList_STATIC.Contains(this))
		{
			m_eyeballSyncList_STATIC.Remove(this);
		}
	}

	private void OnDestroy()
	{
		if (base.EnemyController != null)
		{
			base.EnemyController.HealthChangeRelay.RemoveListener(OnBossHit);
		}
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		InitializeModeShiftEye();
		base.EnemyController.HealthChangeRelay.AddListener(OnBossHit);
	}

	private void InitializeModeShiftEye()
	{
		if (base.EnemyController.EnemyRank == EnemyRank.Expert)
		{
			base.EnemyController.TakesNoDamage = true;
			base.EnemyController.StatusBarController.Active = false;
			base.EnemyController.LogicController.enabled = false;
			base.EnemyController.LogicController.DisableLogicActivationByDistance = true;
		}
	}

	private void OnBossHit(object sender, HealthChangeEventArgs args)
	{
		if (!m_modeShifted && !base.EnemyController.IsDead && (!(args.PrevHealthValue <= args.NewHealthValue) || base.EnemyController.EnemyRank == EnemyRank.Expert) && base.EnemyController.CurrentHealth <= (float)base.EnemyController.ActualMaxHealth * 0.5f)
		{
			m_modeShifted = true;
			if (base.EnemyController.EnemyRank == EnemyRank.Expert)
			{
				base.EnemyController.TakesNoDamage = false;
				base.EnemyController.StatusBarController.Active = true;
				base.EnemyController.LogicController.enabled = true;
				base.EnemyController.LogicController.DisableLogicActivationByDistance = false;
				base.LogicController.ForceExecuteLogicBlockName_OnceOnly = "Mode_Shift_OpenEye";
			}
			else
			{
				base.LogicController.StopAllLogic(resetCurrentState: true);
				base.LogicController.ForceExecuteLogicBlockName_OnceOnly = "Mode_Shift";
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
	public IEnumerator ZoneBlast()
	{
		string TELL_INTRO = "AreaShot_Tell_Intro";
		string TELL_HOLD = "AreaShot_Tell_Hold";
		string ATTACK_INTRO = "AreaShot_Attack_Intro";
		string ATTACK_HOLD = "AreaShot_Attack_Hold";
		string EXIT = "AreaShot_Exit";
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Expert)
		{
			TELL_INTRO = "Explosion_Tell_Intro";
			TELL_HOLD = "Explosion_Tell_Hold";
			ATTACK_INTRO = "Explosion_Attack_Intro";
			ATTACK_HOLD = "Explosion_Attack_Hold";
			EXIT = "Explosion_Exit";
		}
		if (ZoneBlast_Sync)
		{
			IsReady = true;
			while (!CanFire())
			{
				yield return null;
			}
		}
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Expert)
		{
			m_shoutWarningProjectile = this.FireProjectile("EyeballWarningProjectile", 0, matchFacing: false);
		}
		base.EnemyController.Animator.SetBool("Centered", value: true);
		yield return Default_TellIntroAndLoop(TELL_INTRO, ZoneBlast_TellIntro_AnimSpeed, TELL_HOLD, ZoneBlast_TellHold_AnimSpeed, ZoneBlast_TellHold_Duration);
		yield return Default_Animation(ATTACK_INTRO, ZoneBlast_AttackIntro_AnimSpeed, ZoneBlast_AttackIntro_Delay);
		base.EnemyController.FollowTarget = false;
		CDGHelper.VectorToAngle(base.EnemyController.Heading);
		yield return this.ChangeAnimationState(ATTACK_HOLD);
		this.SetAnimationSpeedMultiplier(ZoneBlast_AttackHold_AnimSpeed);
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Expert)
		{
			m_shoutWarningProjectile.transform.position = base.EnemyController.Midpoint;
			m_shoutWarningProjectile.transform.SetParent(base.EnemyController.transform, worldPositionStays: true);
			m_shoutAttackWarningAppearedRelay.Dispatch();
			StopProjectile(ref m_shoutWarningProjectile);
			m_shoutAttackExplodedRelay.Dispatch();
			this.FireProjectile("EyeballExplosionProjectile", 0, matchFacing: false);
		}
		else
		{
			this.FireProjectile("EyeballPassLineBoltProjectile", 0, matchFacing: false);
			this.FireProjectile("EyeballPassLineBoltProjectile", 0, matchFacing: false, 30f);
			this.FireProjectile("EyeballPassLineBoltProjectile", 0, matchFacing: false, 60f);
			this.FireProjectile("EyeballPassLineBoltProjectile", 0, matchFacing: false, 90f);
			this.FireProjectile("EyeballPassLineBoltProjectile", 0, matchFacing: false, 120f);
			this.FireProjectile("EyeballPassLineBoltProjectile", 0, matchFacing: false, 150f);
			this.FireProjectile("EyeballPassLineBoltProjectile", 0, matchFacing: false, 180f);
			this.FireProjectile("EyeballPassLineBoltProjectile", 0, matchFacing: false, 210f);
			this.FireProjectile("EyeballPassLineBoltProjectile", 0, matchFacing: false, 240f);
			this.FireProjectile("EyeballPassLineBoltProjectile", 0, matchFacing: false, 270f);
			this.FireProjectile("EyeballPassLineBoltProjectile", 0, matchFacing: false, 300f);
			this.FireProjectile("EyeballPassLineBoltProjectile", 0, matchFacing: false, 330f);
		}
		yield return Default_Animation(EXIT, ZoneBlast_Exit_AnimSpeed, ZoneBlast_Exit_Duration);
		base.EnemyController.Animator.SetBool("Centered", value: false);
		base.EnemyController.FollowTarget = false;
		yield return Default_Attack_Cooldown(ZoneBlast_Exit_ForceIdle, ZoneBlast_Exit_AttackCD);
		IsReady = false;
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public override IEnumerator ShootSpinningFireball()
	{
		if (SpinningFireball_Sync)
		{
			IsReady = true;
			while (!CanFire())
			{
				yield return null;
			}
		}
		base.EnemyController.Animator.SetBool("Centered", value: true);
		yield return Default_TellIntroAndLoop("BurstShot_Tell_Intro", MultiShot_TellIntro_AnimSpeed, "BurstShot_Tell_Hold", MultiShot_TellHold_AnimSpeed, MultiShot_TellHold_Duration);
		yield return Default_Animation("BurstShot_Attack_Intro", MultiShot_AttackIntro_AnimSpeed, MultiShot_AttackIntro_Delay);
		base.EnemyController.FollowTarget = false;
		CDGHelper.VectorToAngle(base.EnemyController.Heading);
		yield return this.ChangeAnimationState("BurstShot_Attack_Hold");
		this.SetAnimationSpeedMultiplier(MultiShot_AttackHold_AnimSpeed);
		float num = Random.Range(0, 2);
		int flipper = -1;
		if (num > 1f)
		{
			flipper = 1;
		}
		int SpinningFireballCycles = SpinngFireball_Angle * Spinning_Fireball_Amount;
		for (int i = 0; i <= SpinningFireballCycles; i += SpinngFireball_Angle)
		{
			this.FireProjectile("EyeballBoltMinibossProjectile", 0, matchFacing: false, i * flipper);
			this.FireProjectile("EyeballBoltMinibossProjectile", 0, matchFacing: false, 180 + i * flipper);
			yield return Wait(0.175f);
		}
		yield return Wait(MultiShot_AttackHold_Delay);
		base.EnemyController.Animator.SetBool("Centered", value: false);
		yield return Default_Animation("BurstShot_Exit", MultiShot_Exit_AnimSpeed, MultiShot_Exit_Duration);
		base.EnemyController.FollowTarget = false;
		yield return Default_Attack_Cooldown(MultiShot_Exit_ForceIdle, MultiShot_Exit_AttackCD);
		IsReady = false;
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public override IEnumerator ShootHomingFireball()
	{
		string TELL_INTRO = "HomingShot_Tell_Intro";
		string TELL_HOLD = "HomingShot_Tell_Hold";
		string ATTACK_INTRO = "HomingShot_Attack_Intro";
		string ATTACK_HOLD = "HomingShot_Attack_Hold";
		string EXIT = "HomingShot_Exit";
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Expert)
		{
			TELL_INTRO = "BounceBullets_Tell_Intro";
			TELL_HOLD = "BounceBullets_Tell_Hold";
			ATTACK_INTRO = "BounceBullets_Attack_Intro";
			ATTACK_HOLD = "BounceBullets_Attack_Hold";
			EXIT = "BounceBullets_Exit";
		}
		if (HomingFireball_Sync)
		{
			IsReady = true;
			while (!CanFire())
			{
				yield return null;
			}
		}
		base.EnemyController.FollowTarget = false;
		float fireAngle = CDGHelper.VectorToAngle(base.EnemyController.Heading);
		yield return Default_TellIntroAndLoop(TELL_INTRO, SingleShot_TellIntro_AnimSpeed, TELL_HOLD, MultiShot_TellHold_AnimSpeed, MultiShot_TellHold_Duration);
		yield return Default_Animation(ATTACK_INTRO, SingleShot_AttackIntro_AnimSpeed, SingleShot_AttackIntro_Delay);
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Expert)
		{
			for (int i = 0; i < 2; i++)
			{
				Projectile_RL projectile = this.FireProjectile("EyeballBounceBoltMinibossProjectile", 1, matchFacing: false, fireAngle);
				RotateProjectileToHeading(projectile);
				yield return Wait(1.75f);
			}
		}
		else
		{
			for (int i = 0; i < 2; i++)
			{
				Projectile_RL projectile2 = this.FireProjectile("EyeballCurseBoltProjectile", 1, matchFacing: false, fireAngle);
				RotateProjectileToHeading(projectile2);
				yield return Wait(1.75f);
			}
		}
		yield return Default_Animation(ATTACK_HOLD, SingleShot_AttackHold_AnimSpeed, SingleShot_AttackHold_Delay);
		yield return Default_Animation(EXIT, SingleShot_Exit_AnimSpeed, SingleShot_Exit_Duration);
		base.EnemyController.FollowTarget = false;
		yield return Default_Attack_Cooldown(SingleShot_Exit_ForceIdle, SingleShot_Exit_AttackCD);
		IsReady = false;
	}

	public override IEnumerator SpawnAnim()
	{
		m_faceForward = true;
		int spawnOrder_STATIC = m_spawnOrder_STATIC;
		m_spawnOrder_STATIC = (m_spawnOrder_STATIC + 1) % 2;
		float delay = m_spawn_Idle_Delay;
		float introDelay = m_spawn_Intro_Delay;
		if (spawnOrder_STATIC == 1)
		{
			delay = m_secondSpawn_Idle_Delay;
			introDelay = m_secondSpawn_Intro_Delay;
		}
		yield return Default_Animation("Intro_Idle", m_spawn_Idle_AnimSpeed, delay);
		yield return Default_Animation("Closed_Moving", m_spawn_Intro_AnimSpeed, introDelay);
		m_faceForward = false;
		base.EnemyController.ForceFaceTarget();
		yield return Default_Animation("Open", m_spawn_Open_AnimSpeed, m_spawn_Open_Delay);
		yield return Default_Animation(BaseAIScript.NEUTRAL_STATE, 1f, 0f, waitUntilAnimationComplete: false);
	}

	public override IEnumerator DeathAnim()
	{
		if (base.EnemyController.EnemyRank == EnemyRank.Expert || m_eyeballSyncList_STATIC.Count <= 1)
		{
			yield return base.DeathAnim();
			StopAttackEffects();
			RLTimeScale.SetTimeScale(TimeScaleType.Cutscene, 0.5f);
			yield return Default_Animation("Death_Intro", m_death_Intro_AnimSpeed, m_death_Intro_Delay);
			RLTimeScale.SetTimeScale(TimeScaleType.Cutscene, 1f);
			base.EnemyController.GetComponent<BossModeShiftController>().DissolveMaterials(m_death_Hold_Delay);
			yield return Default_Animation("Death_Loop", m_death_Hold_AnimSpeed, m_death_Hold_Delay);
		}
		else
		{
			yield return Default_Animation("Death_Intro", m_death_Intro_AnimSpeed, m_death_Intro_Delay);
			base.EnemyController.GetComponent<BossModeShiftController>().DissolveMaterials(m_death_Hold_Delay);
			yield return Default_Animation("Death_Loop", m_death_Hold_AnimSpeed, m_death_Hold_Delay);
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
		base.EnemyController.ModeshiftDamageMod = m_modeShift_Damage_Mod;
		base.EnemyController.Animator.SetBool("Centered", value: true);
		yield return Default_Animation("ModeShift_Intro", 1f, 3.25f);
		yield return Default_Animation("ModeShift_Scream_Intro", 1f, 0f);
		yield return Default_Animation("ModeShift_Scream_Hold", 1f, 1.25f);
		yield return Default_Animation("ModeShift_Scream_Exit", 1f, 0f);
		base.EnemyController.ModeshiftDamageMod = 1f;
		base.EnemyController.ResetBaseValues();
		base.EnemyController.Animator.SetBool("Centered", value: false);
		yield return Default_Attack_Cooldown(0.1f, 99999f);
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
	public IEnumerator Mode_Shift_OpenEye()
	{
		base.EnemyController.Animator.SetBool("Centered", value: true);
		StopAttackEffects();
		yield return base.DeathAnim();
		base.EnemyController.ModeshiftDamageMod = m_modeShift_Damage_Mod;
		yield return Default_Animation("Intro_Idle", 1f, 0.5f);
		yield return Default_Animation("Closed_Moving", 1f, 0.75f);
		yield return Default_Animation("Open", 1f, 1f);
		yield return Default_Animation("ModeShift_Scream_Intro", 1f, 0f);
		yield return Default_Animation("ModeShift_Scream_Hold", 1f, 1.25f);
		yield return Default_Animation("ModeShift_Scream_Exit", 1f, 0f);
		base.EnemyController.TakesNoDamage = false;
		base.EnemyController.StatusBarController.Active = true;
		base.EnemyController.Animator.SetBool("Centered", value: false);
		base.EnemyController.FollowTarget = true;
		m_faceForward = false;
		base.EnemyController.ForceFaceTarget();
		base.EnemyController.ModeshiftDamageMod = 1f;
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(0.1f, 99999f);
	}

	public override void OnLBCompleteOrCancelled()
	{
		StopProjectile(ref m_shoutWarningProjectile);
		base.OnLBCompleteOrCancelled();
		IsReady = false;
	}

	private void RotateProjectileToHeading(Projectile_RL projectile)
	{
		Vector3 position = projectile.transform.position;
		Vector3 vector = position;
		vector.x += 3f;
		Vector2 vector2 = CDGHelper.RotatedPoint(vector, position, CDGHelper.VectorToAngle(base.EnemyController.Heading));
		vector.x = vector2.x;
		vector.y = vector2.y;
		projectile.transform.position = vector;
	}

	private void StopAttackEffects()
	{
		EffectManager.DisableAllEffectWithName("EyeballBossBlueFireballTell_Effect");
		EffectManager.DisableAllEffectWithName("EyeballBossBounceBulletsTell_Effect");
		EffectManager.DisableAllEffectWithName("EyeballBossCurseTell_Effect");
	}
}
