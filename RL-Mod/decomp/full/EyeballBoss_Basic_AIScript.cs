using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using RLAudio;
using Sigtrap.Relays;
using UnityEngine;

public class EyeballBoss_Basic_AIScript : Eyeball_Basic_AIScript, IAudioEventEmitter
{
	protected enum EyeballType
	{
		Left,
		Right,
		Bottom,
		Middle
	}

	[SerializeField]
	protected EyeballType m_eyeballType;

	protected const float PROJECTILE_SPAWN_X_OFFSET = 3f;

	private static bool m_deathWhiteFlashPlayed = false;

	private static bool m_modeShiftWhiteFlashPlayed = false;

	private EnemyModeShiftEventArgs m_modeShiftEventArgs;

	protected const string POSITION_ANIM_PARAM = "EyeballPosition";

	private EventInstance m_burstShotPrepLoopInstance;

	private EventInstance m_bounceShotPrepLoopInstance;

	private Action<object, EventArgs> m_onPlayerEnterRoom;

	private Action<object, HealthChangeEventArgs> m_onBossHit;

	protected static List<EyeballBoss_Basic_AIScript> m_eyeballSyncList_STATIC = new List<EyeballBoss_Basic_AIScript>();

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

	protected const string ZONE_BULLET_VARIANT_PROJECTILE = "EyeballPassLineBoltPrimeProjectile";

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

	protected const string ELECTRO_HOMING_FIREBALL_TELL_INTRO = "BounceBullets_Tell_Intro";

	protected const string ELECTRO_HOMING_FIREBALL_TELL_HOLD = "BounceBullets_Tell_Hold";

	protected const string ELECTRO_HOMING_FIREBALL_ATTACK_INTRO = "BounceBullets_Attack_Intro";

	protected const string ELECTRO_HOMING_FIREBALL_ATTACK_HOLD = "BounceBullets_Attack_Hold";

	protected const string ELECTRO_HOMING_FIREBALL_EXIT = "BounceBullets_Exit";

	protected const string HOMING_FIREBALL_PROJECTILE = "EyeballBounceBoltMinibossProjectile";

	protected const string CURSE_FIREBALL_PROJECTILE = "EyeballCurseBoltProjectile";

	protected const string CURSE_FIREBALL_BLUE_PROJECTILE = "EyeballCurseBoltBlueProjectile";

	protected const string SPAWN_IDLE = "Intro_Idle";

	protected const string SPAWN_INTRO = "Closed_Moving";

	protected const string SPAWN_OPEN = "Open";

	private const string SFX_EXPERT_SPAWN_INTRO_NAME = "event:/SFX/Enemies/sfx_khidirBoss_intro_rl1Grow_layer";

	protected float m_spawn_Intro_AnimSpeed = 1f;

	protected float m_spawn_Idle_AnimSpeed = 1f;

	protected float m_spawn_Open_AnimSpeed = 1f;

	protected float m_spawn_Open_Delay = 1f;

	protected float m_leftSpawn_Idle_Delay = 1f;

	protected float m_leftSpawn_Intro_Delay = 1f;

	protected float m_rightSpawn_Idle_Delay = 1.4f;

	protected float m_rightSpawn_Intro_Delay = 1.6f;

	protected const string DEATH_INTRO = "Death_Intro";

	protected const string DEATH_HOLD = "Death_Loop";

	private const string SFX_EXPERT_DEATH_INTRO_NAME = "event:/SFX/Enemies/sfx_khidirBoss_death_rl1Layer";

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

	private const float m_modeShift_openEye_Idle_Delay = 0.8f;

	private const float m_modeShift_openEye_Intro_AnimSpeed = 1f;

	private const float m_modeShift_openEye_Intro_Delay = 1.4f;

	private const float m_modeShift_openEye_Open_AnimSpeed = 1f;

	private const float m_modeShift_openEye_Open_Delay = 0.2f;

	public bool StartsDisabled { get; set; }

	public bool DisableModeshift { get; set; }

	protected bool IsKhidr
	{
		get
		{
			if ((bool)base.EnemyController && base.EnemyController.EnemyRank == EnemyRank.Expert)
			{
				return base.EnemyController.EnemyType == EnemyType.EyeballBoss_Middle;
			}
			return false;
		}
	}

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

	protected virtual bool ZoneBlast_Variant => false;

	public IRelayLink ShoutAttackWarningAppearedRelay => m_shoutAttackWarningAppearedRelay.link;

	public IRelayLink ShoutAttackExplodedRelay => m_shoutAttackExplodedRelay.link;

	protected virtual int Spinning_Fireball_Amount => 8;

	protected virtual int SpinngFireball_Angle => 30;

	protected virtual bool SpinningFireball_Sync => false;

	protected virtual int Spinning_Fireball_Variant_Amount => 15;

	protected virtual int SpinngFireball_Variant_Angle => 16;

	protected virtual bool SpinningFireball_Variant_Sync => false;

	protected virtual bool SpinningFireball_Variant => false;

	protected virtual bool HomingFireball_Sync => false;

	protected virtual bool HomingFireball_Variant => false;

	protected virtual float HomingFireball_Variant_SpeedMod => 2.25f;

	protected virtual bool HomingFireball_Variant_Blue => false;

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
		m_projectileNameArray = new string[8] { "EyeballBoltMinibossProjectile", "EyeballBounceBoltMinibossProjectile", "EyeballExplosionProjectile", "EyeballPassLineBoltProjectile", "EyeballPassLineBoltPrimeProjectile", "EyeballWarningProjectile", "EyeballCurseBoltProjectile", "EyeballCurseBoltBlueProjectile" };
	}

	private void Awake()
	{
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
		m_onBossHit = OnBossHit;
	}

	private void OnEnable()
	{
		IsReady = false;
		m_modeShifted = false;
		m_modeShiftWhiteFlashPlayed = false;
		if (!m_eyeballSyncList_STATIC.Contains(this))
		{
			m_eyeballSyncList_STATIC.Add(this);
		}
		if (base.IsInitialized)
		{
			InitializeModeShiftEye();
			if (IsKhidr)
			{
				Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		AudioManager.Stop(m_burstShotPrepLoopInstance);
		AudioManager.Stop(m_bounceShotPrepLoopInstance);
		if (m_eyeballSyncList_STATIC.Contains(this))
		{
			m_eyeballSyncList_STATIC.Remove(this);
		}
		DisableModeshift = false;
		StartsDisabled = false;
		if (IsKhidr)
		{
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
		}
		if (!GameManager.IsApplicationClosing && (bool)base.Animator)
		{
			base.Animator.WriteDefaultValues();
		}
	}

	private void OnDestroy()
	{
		if ((bool)base.EnemyController)
		{
			base.EnemyController.HealthChangeRelay.RemoveListener(m_onBossHit);
		}
		if (m_burstShotPrepLoopInstance.isValid())
		{
			m_burstShotPrepLoopInstance.release();
		}
		if (m_bounceShotPrepLoopInstance.isValid())
		{
			m_bounceShotPrepLoopInstance.release();
		}
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		InitializeModeShiftEye();
		base.EnemyController.HealthChangeRelay.AddListener(m_onBossHit);
		switch (m_eyeballType)
		{
		case EyeballType.Left:
			base.Animator.SetInteger("EyeballPosition", 0);
			m_burstShotPrepLoopInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/sfx_enemy_eyeballBoss_shootBlue_prepLoop_left", base.transform);
			break;
		case EyeballType.Bottom:
			base.Animator.SetInteger("EyeballPosition", 1);
			m_burstShotPrepLoopInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/sfx_enemy_eyeballBoss_shootBlue_prepLoop_center", base.transform);
			break;
		case EyeballType.Right:
			base.Animator.SetInteger("EyeballPosition", 2);
			m_burstShotPrepLoopInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/sfx_enemy_eyeballBoss_shootBlue_prepLoop_right", base.transform);
			break;
		case EyeballType.Middle:
			base.Animator.SetInteger("EyeballPosition", 1);
			m_burstShotPrepLoopInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/sfx_enemy_eyeballBoss_shootBlue_prepLoop_center", base.transform);
			break;
		}
		m_bounceShotPrepLoopInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/sfx_enemy_eyeballBoss_shootBounce_prepLoop_center", base.transform);
		if (IsKhidr)
		{
			OnPlayerEnterRoom(null, null);
		}
	}

	private void OnPlayerEnterRoom(object sender, EventArgs args)
	{
		if (base.IsInitialized)
		{
			base.EnemyController.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
			base.EnemyController.Heading = Vector2.zero;
			base.EnemyController.FollowTarget = false;
			base.Animator.SetBool("Centered", value: true);
			base.Animator.Play(BaseAIScript.NEUTRAL_STATE, 0, 1f);
			if (!base.EnemyController.CharacterCorgi.IsFacingRight)
			{
				base.EnemyController.CharacterCorgi.Flip(IgnoreFlipOnDirectionChange: true, ignoreTurnParam: true);
			}
		}
	}

	private void InitializeModeShiftEye()
	{
		if (StartsDisabled)
		{
			base.EnemyController.TakesNoDamage = true;
			base.EnemyController.StatusBarController.Active = false;
			base.EnemyController.LogicController.enabled = false;
			base.EnemyController.LogicController.DisableLogicActivationByDistance = true;
			base.EnemyController.StatusEffectController.ImmuneToAllStatusEffects = true;
		}
		base.LogicController.ForceExecuteLogicBlockName_OnceOnly = null;
	}

	private void OnBossHit(object sender, HealthChangeEventArgs args)
	{
		if (m_modeShifted || base.EnemyController.IsDead || DisableModeshift || (args.PrevHealthValue <= args.NewHealthValue && !StartsDisabled) || !(base.EnemyController.CurrentHealth <= (float)base.EnemyController.ActualMaxHealth * 0.5f))
		{
			return;
		}
		m_modeShifted = true;
		if (StartsDisabled)
		{
			if (base.EnemyController.CurrentHealth > 0f)
			{
				base.EnemyController.TakesNoDamage = false;
				base.EnemyController.StatusBarController.Active = true;
				base.EnemyController.LogicController.enabled = true;
				base.EnemyController.LogicController.DisableLogicActivationByDistance = false;
				base.LogicController.ForceExecuteLogicBlockName_OnceOnly = "Mode_Shift_OpenEye";
			}
			else
			{
				base.EnemyController.KillCharacter(PlayerManager.GetPlayerController().gameObject, broadcastEvent: true);
			}
			if (m_modeShiftEventArgs == null)
			{
				m_modeShiftEventArgs = new EnemyModeShiftEventArgs(base.EnemyController);
			}
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.EnemyModeShift, this, m_modeShiftEventArgs);
		}
		else
		{
			base.LogicController.StopAllLogic(resetCurrentState: true);
			base.LogicController.ForceExecuteLogicBlockName_OnceOnly = "Mode_Shift";
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
		int randomMove = UnityEngine.Random.Range(0, 100);
		int OddSplitter = 60;
		if (m_eyeballType == EyeballType.Bottom || randomMove <= OddSplitter)
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
		if (m_eyeballType == EyeballType.Bottom || randomMove <= OddSplitter)
		{
			m_shoutWarningProjectile = this.FireProjectile("EyeballWarningProjectile", 0, matchFacing: false);
		}
		base.EnemyController.Animator.SetBool("Centered", value: true);
		yield return Default_TellIntroAndLoop(TELL_INTRO, ZoneBlast_TellIntro_AnimSpeed, TELL_HOLD, ZoneBlast_TellHold_AnimSpeed, ZoneBlast_TellHold_Duration);
		yield return Default_Animation(ATTACK_INTRO, ZoneBlast_AttackIntro_AnimSpeed, ZoneBlast_AttackIntro_Delay);
		CDGHelper.VectorToAngle(base.EnemyController.Heading);
		yield return this.ChangeAnimationState(ATTACK_HOLD);
		this.SetAnimationSpeedMultiplier(ZoneBlast_AttackHold_AnimSpeed);
		if (m_eyeballType == EyeballType.Bottom || randomMove <= OddSplitter)
		{
			m_shoutWarningProjectile.transform.position = base.EnemyController.Midpoint;
			m_shoutWarningProjectile.transform.SetParent(base.EnemyController.transform, worldPositionStays: true);
			m_shoutAttackWarningAppearedRelay.Dispatch();
			StopProjectile(ref m_shoutWarningProjectile);
			m_shoutAttackExplodedRelay.Dispatch();
			this.FireProjectile("EyeballExplosionProjectile", 0, matchFacing: false);
			if (ZoneBlast_Variant && m_eyeballType == EyeballType.Middle)
			{
				float num = CDGHelper.VectorToAngle(base.EnemyController.TargetController.Midpoint - base.EnemyController.Midpoint);
				float num2 = 15f;
				float num3 = 24f;
				Vector2 vector = new Vector2(-9f, 9f);
				float speedMod = 1f;
				if (base.LogicController.EnemyLogicType != EnemyLogicType.Expert)
				{
					for (int i = 0; (float)i < num2; i++)
					{
						float num4 = UnityEngine.Random.Range(vector.x, vector.y);
						this.FireProjectile("EyeballBoltMinibossProjectile", 0, matchFacing: false, num + num4 + num3 * (float)i, speedMod);
					}
				}
			}
		}
		else
		{
			string projectileName = "EyeballPassLineBoltProjectile";
			if (ZoneBlast_Variant && m_eyeballType == EyeballType.Middle)
			{
				projectileName = "EyeballPassLineBoltPrimeProjectile";
			}
			this.FireProjectile(projectileName, 0, matchFacing: false);
			this.FireProjectile(projectileName, 0, matchFacing: false, 30f);
			this.FireProjectile(projectileName, 0, matchFacing: false, 60f);
			this.FireProjectile(projectileName, 0, matchFacing: false, 90f);
			this.FireProjectile(projectileName, 0, matchFacing: false, 120f);
			this.FireProjectile(projectileName, 0, matchFacing: false, 150f);
			this.FireProjectile(projectileName, 0, matchFacing: false, 180f);
			this.FireProjectile(projectileName, 0, matchFacing: false, 210f);
			this.FireProjectile(projectileName, 0, matchFacing: false, 240f);
			this.FireProjectile(projectileName, 0, matchFacing: false, 270f);
			this.FireProjectile(projectileName, 0, matchFacing: false, 300f);
			this.FireProjectile(projectileName, 0, matchFacing: false, 330f);
		}
		yield return Default_Animation(EXIT, ZoneBlast_Exit_AnimSpeed, ZoneBlast_Exit_Duration);
		base.EnemyController.Animator.SetBool("Centered", value: false);
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
		AudioManager.PlayAttached(this, m_burstShotPrepLoopInstance, base.gameObject);
		yield return Default_TellIntroAndLoop("BurstShot_Tell_Intro", MultiShot_TellIntro_AnimSpeed, "BurstShot_Tell_Hold", MultiShot_TellHold_AnimSpeed, MultiShot_TellHold_Duration);
		yield return Default_Animation("BurstShot_Attack_Intro", MultiShot_AttackIntro_AnimSpeed, MultiShot_AttackIntro_Delay);
		CDGHelper.VectorToAngle(base.EnemyController.Heading);
		yield return this.ChangeAnimationState("BurstShot_Attack_Hold");
		this.SetAnimationSpeedMultiplier(MultiShot_AttackHold_AnimSpeed);
		float num = UnityEngine.Random.Range(0, 2);
		int flipper = -1;
		if (num >= 1f)
		{
			flipper = 1;
		}
		int spinAngle = SpinngFireball_Angle;
		int num2 = Spinning_Fireball_Amount;
		if (SpinningFireball_Variant && m_eyeballType == EyeballType.Middle)
		{
			spinAngle = Spinning_Fireball_Variant_Amount;
			num2 = Spinning_Fireball_Variant_Amount;
		}
		int SpinningFireballCycles = spinAngle * num2;
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Expert && m_eyeballType == EyeballType.Middle)
		{
			SpinningFireballCycles += spinAngle * 8;
		}
		for (int i = 0; i <= SpinningFireballCycles; i += spinAngle)
		{
			this.FireProjectile("EyeballBoltMinibossProjectile", 0, matchFacing: false, i * flipper);
			this.FireProjectile("EyeballBoltMinibossProjectile", 0, matchFacing: false, 180 + i * flipper);
			if (SpinningFireball_Variant && m_eyeballType == EyeballType.Middle && base.LogicController.EnemyLogicType == EnemyLogicType.Expert)
			{
				yield return Wait(0.125f);
			}
			else
			{
				yield return Wait(0.175f);
			}
		}
		yield return Wait(MultiShot_AttackHold_Delay);
		base.EnemyController.Animator.SetBool("Centered", value: false);
		AudioManager.Stop(m_burstShotPrepLoopInstance);
		yield return Default_Animation("BurstShot_Exit", MultiShot_Exit_AnimSpeed, MultiShot_Exit_Duration);
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
		int num = UnityEngine.Random.Range(0, 2);
		bool centreEyeball = false;
		bool doingElectroAttack = m_eyeballType == EyeballType.Bottom || num == 1;
		if (doingElectroAttack)
		{
			TELL_INTRO = "BounceBullets_Tell_Intro";
			TELL_HOLD = "BounceBullets_Tell_Hold";
			ATTACK_INTRO = "BounceBullets_Attack_Intro";
			ATTACK_HOLD = "BounceBullets_Attack_Hold";
			EXIT = "BounceBullets_Exit";
		}
		if (!doingElectroAttack && IsKhidr)
		{
			TELL_INTRO = "BurstShot_Tell_Intro";
			TELL_HOLD = "BurstShot_Tell_Hold";
			ATTACK_INTRO = "BurstShot_Attack_Intro";
			ATTACK_HOLD = "BurstShot_Attack_Hold";
			EXIT = "BurstShot_Exit";
			centreEyeball = true;
		}
		if (HomingFireball_Sync)
		{
			IsReady = true;
			while (!CanFire())
			{
				yield return null;
			}
		}
		if (doingElectroAttack)
		{
			AudioManager.PlayAttached(this, m_bounceShotPrepLoopInstance, base.gameObject);
		}
		if (centreEyeball)
		{
			base.Animator.SetBool("Centered", value: true);
		}
		else
		{
			base.EnemyController.FollowTarget = false;
		}
		float fireAngle = CDGHelper.VectorToAngle(base.EnemyController.Heading);
		yield return Default_TellIntroAndLoop(TELL_INTRO, SingleShot_TellIntro_AnimSpeed, TELL_HOLD, MultiShot_TellHold_AnimSpeed, MultiShot_TellHold_Duration);
		yield return Default_Animation(ATTACK_INTRO, SingleShot_AttackIntro_AnimSpeed, SingleShot_AttackIntro_Delay);
		string curseProjectileName = "EyeballCurseBoltProjectile";
		if (HomingFireball_Variant_Blue)
		{
			curseProjectileName = "EyeballCurseBoltBlueProjectile";
		}
		if (doingElectroAttack)
		{
			if (HomingFireball_Variant && m_eyeballType == EyeballType.Middle)
			{
				Projectile_RL projectile = this.FireProjectile("EyeballBounceBoltMinibossProjectile", 1, matchFacing: false, fireAngle, HomingFireball_Variant_SpeedMod);
				RotateProjectileToHeading(projectile);
				if (base.LogicController.EnemyLogicType == EnemyLogicType.Expert)
				{
					yield return Wait(0.35f);
				}
				else
				{
					yield return Wait(0.75f);
				}
				projectile = this.FireProjectile("EyeballBounceBoltMinibossProjectile", 1, matchFacing: false, fireAngle, HomingFireball_Variant_SpeedMod);
				RotateProjectileToHeading(projectile);
				if (base.LogicController.EnemyLogicType == EnemyLogicType.Expert)
				{
					yield return Wait(0.35f);
				}
				else
				{
					yield return Wait(0.75f);
				}
				projectile = this.FireProjectile("EyeballBounceBoltMinibossProjectile", 1, matchFacing: false, fireAngle, HomingFireball_Variant_SpeedMod);
				RotateProjectileToHeading(projectile);
			}
			else
			{
				Projectile_RL projectile = this.FireProjectile("EyeballBounceBoltMinibossProjectile", 1, matchFacing: false, fireAngle);
				RotateProjectileToHeading(projectile);
				yield return Wait(1.75f);
				projectile = this.FireProjectile("EyeballBounceBoltMinibossProjectile", 1, matchFacing: false, fireAngle);
				RotateProjectileToHeading(projectile);
			}
		}
		else if (HomingFireball_Variant && m_eyeballType == EyeballType.Middle)
		{
			float speedMod2 = 1f;
			if (base.LogicController.EnemyLogicType == EnemyLogicType.Expert)
			{
				float fireAngle2 = CDGHelper.VectorToAngle(base.EnemyController.TargetController.Midpoint - base.EnemyController.Midpoint);
				float shotcount = 10f;
				float angleSpread = 36f;
				Vector2 randomizer = new Vector2(-6f, 6f);
				speedMod2 = 1f;
				for (int i = 0; i < 3; i++)
				{
					for (int j = 0; (float)j < shotcount; j++)
					{
						float num2 = UnityEngine.Random.Range(randomizer.x, randomizer.y);
						this.FireProjectile("EyeballBoltMinibossProjectile", 0, matchFacing: false, fireAngle2 + num2 + angleSpread * (float)j, speedMod2);
					}
					yield return Wait(0.8f);
				}
			}
			else
			{
				Projectile_RL projectile2 = this.FireProjectile(curseProjectileName, 1, matchFacing: false, fireAngle, speedMod2);
				RotateProjectileToHeading(projectile2);
				yield return Wait(0.25f);
				projectile2 = this.FireProjectile(curseProjectileName, 1, matchFacing: false, fireAngle, speedMod2);
				RotateProjectileToHeading(projectile2);
				yield return Wait(0.25f);
				projectile2 = this.FireProjectile(curseProjectileName, 1, matchFacing: false, fireAngle, speedMod2);
				RotateProjectileToHeading(projectile2);
				yield return Wait(0.25f);
				projectile2 = this.FireProjectile(curseProjectileName, 1, matchFacing: false, fireAngle, speedMod2);
				RotateProjectileToHeading(projectile2);
				yield return Wait(0.25f);
				projectile2 = this.FireProjectile(curseProjectileName, 1, matchFacing: false, fireAngle, speedMod2);
				RotateProjectileToHeading(projectile2);
			}
		}
		else
		{
			Projectile_RL projectile2 = this.FireProjectile(curseProjectileName, 1, matchFacing: false, fireAngle);
			RotateProjectileToHeading(projectile2);
			yield return Wait(1.75f);
			projectile2 = this.FireProjectile(curseProjectileName, 1, matchFacing: false, fireAngle);
			RotateProjectileToHeading(projectile2);
		}
		yield return Default_Animation(ATTACK_HOLD, SingleShot_AttackHold_AnimSpeed, SingleShot_AttackHold_Delay);
		if (doingElectroAttack)
		{
			AudioManager.Stop(m_bounceShotPrepLoopInstance);
		}
		yield return Default_Animation(EXIT, SingleShot_Exit_AnimSpeed, SingleShot_Exit_Duration);
		if (centreEyeball)
		{
			base.Animator.SetBool("Centered", value: false);
		}
		base.EnemyController.FollowTarget = true;
		yield return Default_Attack_Cooldown(SingleShot_Exit_ForceIdle, SingleShot_Exit_AttackCD);
		IsReady = false;
	}

	public override IEnumerator SpawnAnim()
	{
		if (IsKhidr)
		{
			m_faceForward = true;
			float scale = EnemyClassLibrary.GetEnemyData(base.EnemyController.EnemyType, base.EnemyController.EnemyRank).Scale;
			AudioManager.PlayOneShot(null, "event:/SFX/Enemies/sfx_khidirBoss_intro_rl1Grow_layer", base.EnemyController.Midpoint);
			TweenManager.TweenBy(base.EnemyController.transform, 3f, Ease.Quad.EaseInOut, "localEulerAngles.z", -1800);
			yield return TweenManager.TweenTo(base.EnemyController.transform, 2f, Ease.Bounce.EaseOut, "delay", 1, "localScale.x", scale, "localScale.y", scale).TweenCoroutine;
			base.Animator.SetBool("Centered", value: false);
			m_faceForward = false;
		}
		else
		{
			m_faceForward = true;
			float delay;
			float introDelay;
			if (m_eyeballType == EyeballType.Left)
			{
				delay = m_leftSpawn_Idle_Delay;
				introDelay = m_leftSpawn_Intro_Delay;
			}
			else
			{
				delay = m_rightSpawn_Idle_Delay;
				introDelay = m_rightSpawn_Intro_Delay;
			}
			yield return Default_Animation("Intro_Idle", m_spawn_Idle_AnimSpeed, delay);
			yield return Default_Animation("Closed_Moving", m_spawn_Intro_AnimSpeed, introDelay);
			m_faceForward = false;
			base.EnemyController.ForceFaceTarget();
			MusicManager.PlayMusic(SongID.TowerBossBGM_ASITP_Boss_3, isOverride: false);
			yield return Default_Animation("Open", m_spawn_Open_AnimSpeed, m_spawn_Open_Delay);
			yield return Default_Animation(BaseAIScript.NEUTRAL_STATE, 1f, 0f, waitUntilAnimationComplete: false);
		}
	}

	public override IEnumerator DeathAnim()
	{
		if (IsKhidr)
		{
			AudioManager.PlayOneShot(null, "event:/SFX/Enemies/sfx_khidirBoss_death_rl1Layer", base.EnemyController.Midpoint);
		}
		if (!m_deathWhiteFlashPlayed)
		{
			m_deathWhiteFlashPlayed = true;
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
		RemoveStatusEffects(includeCommanderBuffs: false);
		base.EnemyController.StatusEffectController.ImmuneToAllStatusEffects = true;
		StopAttackEffects();
		base.EnemyController.ModeshiftDamageMod = m_modeShift_Damage_Mod;
		if (!m_modeShiftWhiteFlashPlayed)
		{
			m_modeShiftWhiteFlashPlayed = true;
			yield return base.DeathAnim();
		}
		base.EnemyController.Animator.SetBool("Centered", value: false);
		base.EnemyController.FollowTarget = true;
		if (base.EnemyController.EnemyRank != 0)
		{
			yield return Default_Animation("ModeShift_Intro", 1f, 0f);
		}
		else
		{
			yield return Default_Animation("ModeShift_Intro", 1f, 3.25f);
		}
		base.EnemyController.Animator.SetBool("Centered", value: true);
		base.EnemyController.FollowTarget = false;
		yield return Default_Animation("ModeShift_Scream_Intro", 1f, 0f);
		if (base.EnemyController.EnemyRank != 0 && !IsKhidr)
		{
			yield return Default_Animation("ModeShift_Scream_Hold", 1f, 10f);
		}
		else
		{
			yield return Default_Animation("ModeShift_Scream_Hold", 1f, 1.25f);
		}
		yield return Default_Animation("ModeShift_Scream_Exit", 1f, 0f);
		base.EnemyController.ModeshiftDamageMod = 1f;
		base.EnemyController.ResetBaseValues();
		base.EnemyController.Animator.SetBool("Centered", value: false);
		base.EnemyController.StatusEffectController.ImmuneToAllStatusEffects = false;
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
		RemoveStatusEffects(includeCommanderBuffs: false);
		base.EnemyController.StatusEffectController.ImmuneToAllStatusEffects = true;
		base.EnemyController.Animator.SetBool("Centered", value: true);
		base.EnemyController.ModeshiftDamageMod = m_modeShift_Damage_Mod;
		if (!m_modeShiftWhiteFlashPlayed)
		{
			m_modeShiftWhiteFlashPlayed = true;
			yield return base.DeathAnim();
		}
		yield return Default_Animation("Intro_Idle", 1f, 0.8f);
		yield return Default_Animation("Closed_Moving", 1f, 1.4f);
		yield return Default_Animation("Open", 1f, 0.2f);
		yield return Default_Animation("ModeShift_Scream_Intro", 1f, 0f);
		yield return Default_Animation("ModeShift_Scream_Hold", 1f, 1.25f);
		yield return Default_Animation("ModeShift_Scream_Exit", 1f, 0f);
		base.EnemyController.TakesNoDamage = false;
		base.EnemyController.StatusBarController.Active = true;
		base.EnemyController.Animator.SetBool("Centered", value: false);
		base.EnemyController.FollowTarget = true;
		m_faceForward = false;
		base.EnemyController.StatusEffectController.ImmuneToAllStatusEffects = false;
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
		AudioManager.Stop(m_burstShotPrepLoopInstance);
		AudioManager.Stop(m_bounceShotPrepLoopInstance);
	}

	public override void ResetScript()
	{
		base.EnemyController.StatusEffectController.ImmuneToAllStatusEffects = false;
		string forceExecuteLogicBlockName_OnceOnly = base.LogicController.ForceExecuteLogicBlockName_OnceOnly;
		base.ResetScript();
		base.LogicController.ForceExecuteLogicBlockName_OnceOnly = forceExecuteLogicBlockName_OnceOnly;
		base.LogicController.DisableLogicActivationByDistance = true;
		m_deathWhiteFlashPlayed = false;
	}

	public override void OnEnemyActivated()
	{
		base.OnEnemyActivated();
		if (IsKhidr)
		{
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.SummonRuleBroadcast, null, null);
		}
	}
}
