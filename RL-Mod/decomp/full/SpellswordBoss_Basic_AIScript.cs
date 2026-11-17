using System.Collections;
using RLAudio;
using UnityEngine;
using UnityEngine.Events;

public class SpellswordBoss_Basic_AIScript : BaseAIScript
{
	public UnityEvent ModeShiftEvent;

	protected const int SPAWN_POS_INDEX = 0;

	protected const int MID_POS_INDEX = 1;

	protected const int TOP_POS_INDEX = 2;

	protected const int FRONT_POS_INDEX = 3;

	protected const int MID_FRONT_POS_INDEX = 4;

	protected const int FOOT_FRONT_POS_INDEX = 5;

	protected const int FOOT_BACK_POS_INDEX = 6;

	protected const int STAFF_TOP_POS_INDEX = 7;

	protected const int STAFF_FRONT_POS_INDEX = 8;

	protected const int SHOUT_MOUTH_POS_INDEX = 9;

	protected const int FAR_FRONT_POS_INDEX = 10;

	protected const int VERY_FAR_POS_INDEX = 11;

	protected const int SUPER_FAR_POS_INDEX = 12;

	protected const int LOW_GROUND_POS_INDEX = 13;

	private bool m_inSecondMode;

	private BossModeShiftController m_modeShiftController;

	private EnemyModeShiftEventArgs m_modeShiftEventArgs;

	protected const string THRUST_TELL_INTRO = "SpearThrust_Tell_Intro";

	protected const string THRUST_TELL_HOLD = "SpearThrust_Tell_Hold";

	protected const string THRUST_ATTACK_INTRO = "SpearThrust_Attack_Intro";

	protected const string THRUST_ATTACK_HOLD = "SpearThrust_Attack_Hold";

	protected const string THRUST_EXIT = "SpearThrust_Exit";

	protected const string THRUST_SPEAR_PROJECTILE = "SpellSwordSpearBounceBoltProjectile";

	protected float m_thrust_AttackSpeed = 35f;

	protected float m_thrust_AttackDuration = 0.65f;

	protected float m_thrust_TellIntro_AnimSpeed = 1.2f;

	protected float m_thrust_TellHold_AnimSpeed = 1.2f;

	protected float m_thrust_TellIntroAndHold_Delay = 0.9f;

	protected float m_thrust_AttackIntro_AnimSpeed = 1.2f;

	protected float m_thrust_AttackIntro_Delay;

	protected float m_thrust_AttackHold_AnimSpeed = 1.2f;

	protected float m_thrust_AttackHold_Delay;

	protected float m_thrust_Exit_AnimSpeed = 0.65f;

	protected float m_thrust_Exit_Delay = 0.45f;

	protected float m_thrust_Exit_IdleDuration = 0.15f;

	protected float m_thrust_AttackCD = 12f;

	protected int m_numThrustDaggers = 8;

	protected int m_thrustDaggerSpread = 90;

	protected const string STAFF_TELL_INTRO = "StaffCircle_Tell_Intro";

	protected const string STAFF_TELL_HOLD = "StaffCircle_Tell_Hold";

	protected const string STAFF_ATTACK_INTRO = "StaffCircle_Attack_Intro";

	protected const string STAFF_ATTACK_HOLD = "StaffCircle_Attack_Hold";

	protected const string STAFF_EXIT = "StaffCircle_Exit";

	protected const string STAFF_CIRCLE_PROJECTILE = "SpellSwordStaffCircleBoltProjectile";

	protected const string STAFF_CIRCLEHOMING_PROJECTILE = "SpellSwordStaffCircleHomingBoltProjectile";

	protected float m_staff_AttackDuration = 2f;

	protected float m_staff_TellIntro_AnimSpeed = 1.2f;

	protected float m_staff_TellHold_AnimSpeed = 1.2f;

	protected float m_staff_TellIntroAndHold_Delay = 0.95f;

	protected float m_staff_AttackIntro_AnimSpeed = 1.2f;

	protected float m_staff_AttackIntro_Delay;

	protected float m_staff_AttackHold_AnimSpeed = 1.2f;

	protected float m_staff_AttackHold_Delay;

	protected float m_staff_AttackHold_ExitDelay = 0.75f;

	protected float m_staff_Exit_AnimSpeed = 1.2f;

	protected float m_staff_Exit_Delay;

	protected float m_staff_Exit_IdleDuration = 0.15f;

	protected float m_staff_AttackCD = 12f;

	private Projectile_RL m_staffShieldProjectile;

	protected int m_numStaffFireballs_Variant = 15;

	protected int m_staffFireballSpread = 720;

	protected const string SWORD1_TELL_INTRO = "Sword1_Tell_Intro";

	protected const string SWORD1_TELL_HOLD = "Sword1_Tell_Hold";

	protected const string SWORD1_ATTACK_INTRO = "Sword1_Attack_Intro";

	protected const string SWORD1_ATTACK_HOLD = "Sword1_Attack_Hold";

	protected const string SWORD2_TELL_INTRO = "Sword2_Tell_Intro";

	protected const string SWORD2_TELL_HOLD = "Sword2_Tell_Hold";

	protected const string SWORD2_ATTACK_INTRO = "Sword2_Attack_Intro";

	protected const string SWORD2_ATTACK_HOLD = "Sword2_Attack_Hold";

	protected const string SWORD2_EXIT = "Sword2_Exit";

	protected const string SWORD_FLAMEGOUT_PROJECTILE = "SpellSwordFlameGoutProjectile";

	protected const string SWORD1_PROJECTILE = "SpellSwordSlashDownProjectile";

	protected const string SWORD2_PROJECTILE = "SpellSwordSlashUpProjectile";

	protected float m_sword_AttackSpeed = 16f;

	protected float m_sword_AttackDuration = 0.185f;

	protected float m_sword_PostAttack_Delay = 0.1f;

	protected float m_sword2_PostAttack_Delay = 0.75f;

	protected float m_sword2_AttackSpeed = 15f;

	protected float m_sword2_AttackDuration = 0.175f;

	protected float m_sword_TellIntro_AnimSpeed = 1.2f;

	protected float m_sword_TellHold_AnimSpeed = 1.2f;

	protected float m_sword_TellIntroAndHold_Delay = 0.75f;

	protected float m_sword_AttackIntro_AnimSpeed = 1.2f;

	protected float m_sword_AttackIntro_Delay;

	protected float m_sword_AttackHold_AnimSpeed = 1.2f;

	protected float m_sword_AttackHold_Delay;

	protected float m_sword2_TellIntro_AnimSpeed = 1.2f;

	protected float m_sword2_TellHold_AnimSpeed = 1.2f;

	protected float m_sword2_TellIntroAndHold_Delay = 0.35f;

	protected float m_sword2_AttackIntro_AnimSpeed = 1.2f;

	protected float m_sword2_AttackIntro_Delay;

	protected float m_sword2_AttackHold_AnimSpeed = 1.2f;

	protected float m_sword2_AttackHold_Delay;

	protected float m_sword_Exit_AnimSpeed = 0.65f;

	protected float m_sword_Exit_Delay;

	protected float m_sword_Exit_IdleDuration = 0.15f;

	protected float m_sword_AttackCD = 8f;

	private Projectile_RL m_swordFlameProjectile;

	private Projectile_RL m_swordFlameProjectile2;

	private Projectile_RL m_swordFlameProjectile3;

	private Projectile_RL m_swordFlameProjectile4;

	protected const string DAGGERTHROW_TELL_INTRO = "DaggerThrow_Tell_Intro";

	protected const string DAGGERTHROW_TELL_HOLD = "DaggerThrow_Tell_Hold";

	protected const string DAGGERTHROW_ATTACK_INTRO = "DaggerThrow_Attack_Intro";

	protected const string DAGGERTHROW_ATTACK_HOLD = "DaggerThrow_Attack_Hold";

	protected const string DAGGERTHROW_EXIT = "DaggerThrow_Exit";

	protected const string DAGGERTHROW_PROJECTILE = "SpellSwordDaggerBoltBlueProjectile";

	protected float m_daggerThrowPirhouetteDelay = 0.15f;

	protected int m_numDaggerThrowDaggers = 3;

	protected int m_numDaggerThrowDaggers_Variant = 3;

	protected float m_daggerThrow_AttackSpeed = 28f;

	protected float m_daggerThrow_AttackDuration = 0.25f;

	protected float m_daggerThrow_TellIntro_AnimSpeed = 1f;

	protected float m_daggerThrow_TellHold_AnimSpeed = 1f;

	protected float m_daggerThrow_TellIntroAndHold_Delay = 0.75f;

	protected float m_daggerThrow_AttackIntro_AnimSpeed = 1f;

	protected float m_daggerThrow_AttackIntro_Delay;

	protected float m_daggerThrow_AttackHold_AnimSpeed = 1f;

	protected float m_daggerThrow_AttackHold_Delay;

	protected float m_daggerThrow_Exit_AnimSpeed = 0.65f;

	protected float m_daggerThrow_Exit_Delay;

	protected float m_daggerThrow_Exit_IdleDuration = 0.15f;

	protected float m_daggerThrow_AttackCD = 12f;

	protected const string AXE_TELL_INTRO = "AxeJump_Tell_Intro";

	protected const string AXE_TELL_HOLD = "AxeJump_Tell_Hold";

	protected const string AXE_ATTACK_INTRO = "AxeJump_Attack_Intro";

	protected const string AXE_ATTACK_HOLD = "AxeJump_Attack_Hold";

	protected const string AXE_EXIT_BLADE_OUT = "AxeJump_Land";

	protected const string AXE_EXIT_RETRACT = "AxeJump_Exit";

	protected const string AXE_SPIN_PROJECTILE = "SpellSwordAxeSpinProjectile";

	protected const string AXE_LAND_PROJECTILE = "SpellSwordGroundRubbleProjectile";

	protected const string AXE_LAND_ADVANCED_PROJECTILE = "SpellSwordVoidProjectile";

	private Projectile_RL m_axeSpinProjectile;

	protected Vector2 m_axeAttackVelocity = new Vector2(12f, 32f);

	protected Vector2 m_axeAttack_Land_ThrowAngle = new Vector2(85f, 85f);

	protected Vector2 m_axeAttack_Land_ThrowAngle2 = new Vector2(78f, 78f);

	protected Vector2 m_axeAttack_Land_ThrowAngle3 = new Vector2(71f, 71f);

	protected Vector2 m_axeAttack_Land_ThrowAngle4 = new Vector2(55f, 55f);

	protected Vector2 m_axeAttack_Land_ThrowAngle5 = new Vector2(82f, 82f);

	protected Vector2 m_axeAttack_Land_ThrowAngle6 = new Vector2(74f, 74f);

	protected Vector2 m_axeAttack_Land_ThrowPower = new Vector2(1f, 1f);

	protected float m_jump_TellIntro_AnimSpeed = 1.2f;

	protected float m_jump_TellHold_AnimSpeed = 1.2f;

	protected float m_jump_TellIntroAndHold_Delay = 0.65f;

	protected float m_axeAttack_TellIntro_AnimSpeed = 1.5f;

	protected float m_axeAttack_TellHold_AnimSpeed = 1.5f;

	protected float m_axeAttack_TellIntroAndHold_Delay;

	protected float m_axeAttack_AttackIntro_AnimSpeed = 1f;

	protected float m_axeAttack_AttackIntro_Delay;

	protected float m_axeAttack_AttackHold_AnimSpeed = 1.5f;

	protected float m_axeAttack_AttackHold_Delay;

	protected float m_axeAttack_ExitHold_Delay = 0.375f;

	protected float m_axeAttack_ExitHold_AnimSpeed = 1.2f;

	protected float m_axeAttack_Exit_AnimSpeed = 1.2f;

	protected float m_axeAttack_Exit_Delay;

	protected float m_axeAttack_Exit_IdleDuration = 0.15f;

	protected float m_axeAttack_AttackCD = 5f;

	protected const string STAFFTHROW_TELL_INTRO = "StaffForward_Tell_Intro";

	protected const string STAFFTHROW_TELL_HOLD = "StaffForward_Tell_Hold";

	protected const string STAFFTHROW_ATTACK_INTRO = "StaffForward_Attack_Intro";

	protected const string STAFFTHROW_ATTACK_HOLD = "StaffForward_Attack_Hold";

	protected const string STAFFTHROW_EXIT = "StaffForward_Attack_Exit";

	protected const string STAFFTHROW_PROJECTILE = "SpellSwordStaffForwardBoltProjectile";

	protected const string STAFFTHROW_BEAM_PROJECTILE = "SpellSwordStaffForwardBeamProjectile";

	protected const string STAFFTHROW_BEAM_WARNING_PROJECTILE = "SpellSwordStaffWarningForwardBeamProjectile";

	protected const string STAFFTHROW_BEAM_ADVANCED_PROJECTILE = "SpellSwordStaffForwardBeamAdvancedProjectile";

	protected const string STAFFTHROW_BEAM_WARNING_ADVANCED_PROJECTILE = "SpellSwordStaffWarningForwardBeamAdvancedProjectile";

	protected float m_staffThrow_BeamAttack_Duration = 0.75f;

	protected float m_staffThrow_BeamAttack_Variant_Duration = 1f;

	protected float m_staffThrow_AttackSpeed = 28f;

	protected float m_staffThrow_AttackDuration_EndDelay = 1f;

	protected float m_staffThrow_TellIntro_AnimSpeed = 1.5f;

	protected float m_staffThrow_TellHold_AnimSpeed = 1f;

	protected float m_staffThrow_TellIntroAndHold_Delay = 0.9f;

	protected float m_staffThrow_AttackIntro_AnimSpeed = 1.2f;

	protected float m_staffThrow_AttackIntro_Delay;

	protected float m_staffThrow_AttackHold_AnimSpeed = 1.2f;

	protected float m_staffThrow_AttackHold_Delay;

	protected float m_staffThrow_Exit_AnimSpeed = 1f;

	protected float m_staffThrow_Exit_Delay;

	protected float m_staffThrow_Exit_IdleDuration = 0.15f;

	protected float m_staffThrow_AttackCD = 12f;

	private Projectile_RL m_staffBeamProjectile;

	private Projectile_RL m_staffBeamWarningProjectile;

	protected const string MODESHIFT_DOWNED = "ModeShift_Intro";

	protected const string MODESHIFT_GETUP = "ModeShift_GetUp";

	protected const string MODESHIFT_ATTACK_INTRO = "ModeShift_Scream_Intro";

	protected const string MODESHIFT_ATTACK_HOLD = "ModeShift_Scream_Hold";

	protected const string MODESHIFT_EXIT = "ModeShift_Scream_Exit";

	protected const string MODESHIFT_PROJECTILE_WARNING = "SpellSwordShoutWarningProjectile";

	protected const string MODESHIFT_PROJECTILE_ATTACK = "SpellSwordShoutAttackProjectile";

	protected const string MODESHIFT_PROJECTILE_HOMING = "SpellSwordShoutHomingBoltProjectile";

	private Projectile_RL m_modeShiftProjectile;

	private Projectile_RL m_modeShiftProjectile2;

	protected float m_modeShift_AttackSpeed = 28f;

	protected float m_modeShift_AttackDuration_Initial_Delay = 0.5f;

	protected float m_modeShift_AttackDuration_Fire_Duration = 0.05f;

	protected float m_modeShift_AttackDuration_Exit_Delay = 0.5f;

	protected float m_modeShift_AttackDuration_SecondShot_Delay = 1.5f;

	protected float m_modeShift_ProjectilesThrown_Variant = 5f;

	protected float m_modeShift_Damage_Mod = 0.1f;

	protected Vector2 m_modeShift_Shout_ThrowAngle = new Vector2(-40f, 40f);

	protected Vector2 m_modeShift_Shout_ThrowPower = new Vector2(0.525f, 0.625f);

	protected float m_modeShift_Downed_AnimSpeed = 1f;

	protected float m_modeShift_Downed_Delay = 1.3f;

	protected float m_modeShift_GetUp_AnimSpeed = 1f;

	protected float m_modeShift_GetUp_Delay = 1f;

	protected float m_modeShift_AttackIntro_AnimSpeed = 1.25f;

	protected float m_modeShift_AttackIntro_Delay;

	protected float m_modeShift_AttackHold_AnimSpeed = 1.25f;

	protected float m_modeShift_AttackHold_Delay;

	protected float m_modeShift_Exit_AnimSpeed = 0.65f;

	protected float m_modeShift_Exit_Delay;

	protected float m_modeShift_Exit_IdleDuration = 0.15f;

	protected float m_modeShift_AttackCD = 99f;

	protected const string DEATH_INTRO = "Death_Intro";

	protected const string DEATH_HOLD = "Death_Loop";

	protected float m_death_Intro_AnimSpeed = 1f;

	protected float m_death_Intro_Delay;

	protected float m_death_Hold_AnimSpeed = 1f;

	protected float m_death_Hold_Delay = 4.5f;

	protected const string SPAWN_IDLE = "Intro_Idle";

	protected const string SPAWN_INTRO = "Intro";

	protected float m_spawn_Idle_AnimSpeed = 1f;

	protected float m_spawn_Idle_Delay = 2f;

	protected float m_spawn_Intro_AnimSpeed = 1f;

	protected float m_spawn_Intro_Delay;

	protected bool m_isAxeSpinning;

	protected bool m_isStaffBeaming;

	protected virtual float m_modeShiftHealthMod => 0.5f;

	protected override Vector2 IdleDuration => new Vector2(0.75f, 1.25f);

	protected override Vector2 WalkTowardsDuration => new Vector2(1f, 1.5f);

	protected override Vector2 WalkAwayDuration => new Vector2(1f, 1.5f);

	protected override Vector2 RandomFollowOffsetX => new Vector2(1f, 6f);

	protected override Vector2 RandomFollowOffsetY => new Vector2(2f, 11f);

	protected virtual bool UseVariant => false;

	protected virtual int m_numStaffFireballs => 25;

	protected virtual int m_numStaffFireballs_AddSecondMode => 20;

	protected virtual int m_numDaggerThrowPirhouettes => 3;

	protected virtual int m_numDaggerThrowDaggers_SecondModeAdd => 4;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[19]
		{
			"SpellSwordSpearBounceBoltProjectile", "SpellSwordStaffCircleBoltProjectile", "SpellSwordStaffCircleHomingBoltProjectile", "SpellSwordFlameGoutProjectile", "SpellSwordSlashDownProjectile", "SpellSwordSlashUpProjectile", "SpellSwordDaggerBoltBlueProjectile", "SpellSwordAxeSpinProjectile", "SpellSwordGroundRubbleProjectile", "SpellSwordStaffForwardBoltProjectile",
			"SpellSwordStaffForwardBeamProjectile", "SpellSwordStaffWarningForwardBeamProjectile", "SpellSwordStaffForwardBeamAdvancedProjectile", "SpellSwordStaffWarningForwardBeamAdvancedProjectile", "SpellSwordShoutWarningProjectile", "SpellSwordShoutAttackProjectile", "SpellSwordShoutHomingBoltProjectile", "SpellSwordCurseBoltProjectile", "SpellSwordVoidProjectile"
		};
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		base.LogicController.DisableLogicActivationByDistance = true;
		base.EnemyController.HealthChangeRelay.AddListener(OnBossHit);
		m_modeShiftController = base.EnemyController.GetComponent<BossModeShiftController>();
	}

	public override void ResetScript()
	{
		base.ResetScript();
		m_inSecondMode = false;
		base.EnemyController.StatusEffectController.ImmuneToAllStatusEffects = false;
		base.LogicController.DisableLogicActivationByDistance = true;
	}

	private void OnDestroy()
	{
		if ((bool)base.EnemyController)
		{
			base.EnemyController.HealthChangeRelay.RemoveListener(OnBossHit);
		}
	}

	private void OnBossHit(object sender, HealthChangeEventArgs args)
	{
		if (!m_inSecondMode && !base.EnemyController.IsDead && !(args.PrevHealthValue <= args.NewHealthValue) && base.EnemyController.CurrentHealth <= (float)base.EnemyController.ActualMaxHealth * m_modeShiftHealthMod)
		{
			m_inSecondMode = true;
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

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Spear_Thrust()
	{
		StopAndFaceTarget();
		float dashSpeed = m_thrust_AttackSpeed;
		if (!base.EnemyController.IsTargetToMyRight)
		{
			dashSpeed = 0f - dashSpeed;
		}
		base.EnemyController.LockFlip = true;
		yield return Default_TellIntroAndLoop("SpearThrust_Tell_Intro", m_thrust_TellIntro_AnimSpeed, "SpearThrust_Tell_Hold", m_thrust_TellHold_AnimSpeed, m_thrust_TellIntroAndHold_Delay);
		SetAttackingWithContactDamage(enable: true);
		yield return Default_Animation("SpearThrust_Attack_Intro", m_thrust_AttackIntro_AnimSpeed, m_thrust_AttackIntro_Delay);
		yield return Default_Animation("SpearThrust_Attack_Hold", m_thrust_AttackHold_AnimSpeed, m_thrust_AttackHold_Delay, waitUntilAnimationComplete: false);
		SetVelocityX(dashSpeed, additive: false);
		if (UseVariant && m_thrust_AttackDuration > 0f)
		{
			float fireInterval = m_thrust_AttackDuration / (float)m_numThrustDaggers;
			_ = m_thrustDaggerSpread / (m_numThrustDaggers - 1);
			for (int i = 0; i < m_numThrustDaggers; i++)
			{
				float angle = 90f;
				if (!base.EnemyController.IsFacingRight)
				{
					angle = 90f;
				}
				this.FireProjectile("SpellSwordSpearBounceBoltProjectile", 13, matchFacing: false, angle);
				yield return Wait(fireInterval);
			}
		}
		else if (m_thrust_AttackDuration > 0f)
		{
			yield return Wait(m_thrust_AttackDuration);
		}
		SetVelocityX(0f, additive: false);
		SetAttackingWithContactDamage(enable: false);
		base.EnemyController.ResetTurnTrigger();
		yield return Default_Animation("SpearThrust_Exit", m_thrust_Exit_AnimSpeed, m_thrust_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_thrust_Exit_IdleDuration, m_thrust_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Staff_Attack()
	{
		SetVelocityX(0f, additive: false);
		base.EnemyController.LockFlip = true;
		yield return Default_TellIntroAndLoop("StaffCircle_Tell_Intro", m_staff_TellIntro_AnimSpeed, "StaffCircle_Tell_Hold", m_staff_TellHold_AnimSpeed, m_staff_TellIntroAndHold_Delay);
		yield return Default_Animation("StaffCircle_Attack_Intro", m_staff_AttackIntro_AnimSpeed, m_staff_AttackIntro_Delay);
		yield return Default_Animation("StaffCircle_Attack_Hold", m_staff_AttackHold_AnimSpeed, m_staff_AttackHold_Delay, waitUntilAnimationComplete: false);
		if (m_staff_AttackDuration > 0f)
		{
			int fireballAmount = ((!UseVariant) ? m_numStaffFireballs : m_numStaffFireballs_Variant);
			if (m_inSecondMode)
			{
				fireballAmount += m_numStaffFireballs_AddSecondMode;
			}
			float fireInterval = m_staff_AttackDuration / (float)fireballAmount;
			float startingAngle = Random.Range(0, 360);
			int num = CDGHelper.RandomPlusMinus();
			float fireAngleInterval = m_staffFireballSpread / (fireballAmount - 1) * num;
			for (int i = 0; i < fireballAmount; i++)
			{
				float angle = startingAngle + fireAngleInterval * (float)i;
				if (UseVariant)
				{
					this.FireProjectile("SpellSwordStaffCircleHomingBoltProjectile", 7, matchFacing: false, angle);
				}
				else
				{
					this.FireProjectile("SpellSwordStaffCircleBoltProjectile", 7, matchFacing: false, angle);
				}
				yield return Wait(fireInterval);
			}
		}
		else if (m_staff_AttackDuration > 0f)
		{
			yield return Wait(m_staff_AttackDuration);
		}
		StopProjectile(ref m_staffShieldProjectile);
		if (m_staff_AttackHold_ExitDelay > 0f)
		{
			yield return Wait(m_staff_AttackHold_ExitDelay);
		}
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("StaffCircle_Exit", m_staff_Exit_AnimSpeed, m_staff_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_staff_Exit_IdleDuration, m_staff_AttackCD);
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
		SetVelocityX(0f, additive: false);
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		float dashSpeed2 = m_sword_AttackSpeed;
		if (!base.EnemyController.IsTargetToMyRight)
		{
			dashSpeed2 = 0f - dashSpeed2;
		}
		yield return Default_TellIntroAndLoop("Sword1_Tell_Intro", m_sword_TellIntro_AnimSpeed, "Sword1_Tell_Hold", m_sword_TellHold_AnimSpeed, m_sword_TellIntroAndHold_Delay);
		SetAttackingWithContactDamage(enable: true);
		yield return Default_Animation("Sword1_Attack_Intro", m_sword_AttackIntro_AnimSpeed, m_sword_AttackIntro_Delay);
		yield return Default_Animation("Sword1_Attack_Hold", m_sword_AttackHold_AnimSpeed, m_sword_AttackHold_Delay, waitUntilAnimationComplete: false);
		this.FireProjectile("SpellSwordSlashDownProjectile", 0);
		SetVelocityX(dashSpeed2, additive: false);
		if (m_sword_AttackDuration > 0f)
		{
			yield return Wait(m_sword_AttackDuration);
		}
		SetVelocityX(0f, additive: false);
		if (m_sword_PostAttack_Delay > 0f)
		{
			yield return Wait(m_sword_PostAttack_Delay);
		}
		yield return Default_Animation("Sword2_Tell_Intro", m_sword2_TellIntro_AnimSpeed, 0f);
		base.EnemyController.LockFlip = false;
		StopAndFaceTarget();
		base.Animator.SetBool("Turn", value: false);
		base.EnemyController.LockFlip = true;
		dashSpeed2 = m_sword2_AttackSpeed;
		if (!base.EnemyController.IsFacingRight)
		{
			dashSpeed2 = 0f - dashSpeed2;
		}
		yield return Default_Animation("Sword2_Tell_Hold", m_sword2_TellHold_AnimSpeed, m_sword2_TellIntroAndHold_Delay);
		yield return Default_Animation("Sword2_Attack_Intro", m_sword2_AttackIntro_AnimSpeed, m_sword2_AttackIntro_Delay);
		yield return Default_Animation("Sword2_Attack_Hold", m_sword2_AttackHold_AnimSpeed, m_sword2_AttackHold_Delay, waitUntilAnimationComplete: false);
		this.FireProjectile("SpellSwordSlashUpProjectile", 0);
		m_swordFlameProjectile = this.FireProjectile("SpellSwordFlameGoutProjectile", 3);
		m_swordFlameProjectile2 = this.FireProjectile("SpellSwordFlameGoutProjectile", 10);
		if ((UseVariant || m_inSecondMode) && base.LogicController.EnemyLogicType != EnemyLogicType.Expert)
		{
			m_swordFlameProjectile3 = this.FireProjectile("SpellSwordFlameGoutProjectile", 11);
		}
		if (UseVariant && m_inSecondMode)
		{
			m_swordFlameProjectile4 = this.FireProjectile("SpellSwordFlameGoutProjectile", 12);
		}
		SetVelocityX(dashSpeed2, additive: false);
		if (m_sword2_AttackDuration > 0f)
		{
			yield return Wait(m_sword2_AttackDuration);
		}
		SetVelocityX(0f, additive: false);
		if (m_sword2_PostAttack_Delay > 0f)
		{
			yield return Wait(m_sword2_PostAttack_Delay);
		}
		StopProjectile(ref m_swordFlameProjectile);
		StopProjectile(ref m_swordFlameProjectile2);
		StopProjectile(ref m_swordFlameProjectile3);
		StopProjectile(ref m_swordFlameProjectile4);
		yield return WaitUntilAnimComplete(DefaultAnimationLayer);
		SetVelocityX(0f, additive: false);
		SetAttackingWithContactDamage(enable: false);
		SetVelocityX(0f, additive: false);
		yield return Default_Animation("Sword2_Exit", m_sword_Exit_AnimSpeed, m_sword_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_sword_Exit_IdleDuration, m_sword_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Dagger_Throw()
	{
		base.EnemyController.LockFlip = false;
		base.EnemyController.SetVelocityX(0f, additive: false);
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		yield return Default_TellIntroAndLoop("DaggerThrow_Tell_Intro", m_daggerThrow_TellIntro_AnimSpeed, "DaggerThrow_Tell_Hold", m_daggerThrow_TellHold_AnimSpeed, m_daggerThrow_TellIntroAndHold_Delay);
		for (int j = 0; j < m_numDaggerThrowPirhouettes; j++)
		{
			if (j > 0)
			{
				base.EnemyController.LockFlip = false;
				base.EnemyController.SetVelocityX(0f, additive: false);
				StopAndFaceTarget();
				base.EnemyController.LockFlip = true;
			}
			yield return Default_Animation("DaggerThrow_Attack_Intro", m_daggerThrow_AttackIntro_AnimSpeed, m_daggerThrow_AttackIntro_Delay);
			yield return Default_Animation("DaggerThrow_Attack_Hold", m_daggerThrow_AttackHold_AnimSpeed, m_daggerThrow_AttackHold_Delay, waitUntilAnimationComplete: false);
			int numDaggers = m_numDaggerThrowDaggers;
			if (UseVariant)
			{
				numDaggers = m_numDaggerThrowDaggers_Variant;
			}
			if (m_inSecondMode)
			{
				numDaggers += m_numDaggerThrowDaggers_SecondModeAdd;
			}
			float fireInterval = m_daggerThrow_AttackDuration / (float)numDaggers;
			float fireAngle = CDGHelper.VectorToAngle(base.EnemyController.TargetController.Midpoint - base.EnemyController.Midpoint);
			for (int i = 0; i < numDaggers; i++)
			{
				this.FireProjectile("SpellSwordDaggerBoltBlueProjectile", 1, matchFacing: false, fireAngle);
				if (UseVariant)
				{
					this.FireProjectile("SpellSwordDaggerBoltBlueProjectile", 1, matchFacing: false, fireAngle + 5f);
					this.FireProjectile("SpellSwordDaggerBoltBlueProjectile", 1, matchFacing: false, fireAngle - 5f);
				}
				yield return Wait(fireInterval);
			}
			if (m_daggerThrowPirhouetteDelay > 0f)
			{
				yield return Wait(m_daggerThrowPirhouetteDelay);
			}
		}
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("DaggerThrow_Exit", m_daggerThrow_Exit_AnimSpeed, m_daggerThrow_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetBaseValues();
		base.EnemyController.Animator.ResetTrigger("Turn");
		yield return Default_Attack_Cooldown(m_daggerThrow_Exit_IdleDuration, m_daggerThrow_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Axe_Attack()
	{
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		yield return Default_Animation("Jump_Tell", m_jump_TellIntro_AnimSpeed, m_jump_TellIntroAndHold_Delay);
		SetAttackingWithContactDamage(enable: true);
		yield return this.ChangeAnimationState("Jump_Up_Intro");
		Vector2 axeAttackVelocity = m_axeAttackVelocity;
		if (!base.EnemyController.IsFacingRight)
		{
			axeAttackVelocity.x = 0f - axeAttackVelocity.x;
		}
		SetVelocity(axeAttackVelocity, additive: false);
		yield return Wait(0.1f);
		yield return Default_TellIntroAndLoop("AxeJump_Tell_Intro", m_axeAttack_TellIntro_AnimSpeed, "AxeJump_Tell_Hold", m_axeAttack_TellHold_AnimSpeed, m_axeAttack_TellIntroAndHold_Delay);
		yield return Default_Animation("AxeJump_Attack_Intro", m_axeAttack_AttackIntro_AnimSpeed, m_axeAttack_AttackIntro_Delay);
		yield return Default_Animation("AxeJump_Attack_Hold", m_axeAttack_AttackHold_AnimSpeed, m_axeAttack_AttackHold_Delay, waitUntilAnimationComplete: false);
		m_axeSpinProjectile = this.FireProjectile("SpellSwordAxeSpinProjectile", 1, matchFacing: false);
		yield return WaitUntilIsGrounded();
		StopProjectile(ref m_axeSpinProjectile);
		float speedMod = Random.Range(m_axeAttack_Land_ThrowPower.x, m_axeAttack_Land_ThrowPower.y);
		int num = Random.Range((int)m_axeAttack_Land_ThrowAngle.x, (int)m_axeAttack_Land_ThrowAngle.y);
		int num2 = Random.Range((int)m_axeAttack_Land_ThrowAngle2.x, (int)m_axeAttack_Land_ThrowAngle2.y);
		int num3 = Random.Range((int)m_axeAttack_Land_ThrowAngle3.x, (int)m_axeAttack_Land_ThrowAngle3.y);
		int num4 = Random.Range((int)m_axeAttack_Land_ThrowAngle4.x, (int)m_axeAttack_Land_ThrowAngle4.y);
		int num5 = Random.Range((int)m_axeAttack_Land_ThrowAngle5.x, (int)m_axeAttack_Land_ThrowAngle5.y);
		int num6 = Random.Range((int)m_axeAttack_Land_ThrowAngle6.x, (int)m_axeAttack_Land_ThrowAngle6.y);
		this.FireProjectile("SpellSwordGroundRubbleProjectile", 0, matchFacing: true, num, speedMod);
		this.FireProjectile("SpellSwordGroundRubbleProjectile", 0, matchFacing: true, 180 - num, speedMod);
		this.FireProjectile("SpellSwordGroundRubbleProjectile", 5, matchFacing: true, num2, speedMod);
		this.FireProjectile("SpellSwordGroundRubbleProjectile", 5, matchFacing: true, 180 - num2, speedMod);
		this.FireProjectile("SpellSwordGroundRubbleProjectile", 5, matchFacing: true, num3, speedMod);
		this.FireProjectile("SpellSwordGroundRubbleProjectile", 5, matchFacing: true, 180 - num3, speedMod);
		if (base.LogicController.EnemyLogicType != EnemyLogicType.Expert)
		{
			if (m_inSecondMode)
			{
				this.FireProjectile("SpellSwordGroundRubbleProjectile", 5, matchFacing: true, num4, speedMod);
				this.FireProjectile("SpellSwordGroundRubbleProjectile", 5, matchFacing: true, 180 - num4, speedMod);
			}
			if (UseVariant)
			{
				this.FireProjectile("SpellSwordGroundRubbleProjectile", 5, matchFacing: true, num5, speedMod);
				this.FireProjectile("SpellSwordGroundRubbleProjectile", 5, matchFacing: true, 180 - num5, speedMod);
				this.FireProjectile("SpellSwordVoidProjectile", 5, matchFacing: false);
				this.FireProjectile("SpellSwordVoidProjectile", 5, matchFacing: false, 180f).Flip();
			}
			if (UseVariant && m_inSecondMode)
			{
				this.FireProjectile("SpellSwordGroundRubbleProjectile", 5, matchFacing: true, num6, speedMod);
				this.FireProjectile("SpellSwordGroundRubbleProjectile", 5, matchFacing: true, 180 - num6, speedMod);
			}
		}
		SetVelocityX(0f, additive: false);
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("AxeJump_Land", m_axeAttack_Exit_AnimSpeed, m_axeAttack_Exit_Delay);
		if (m_axeAttack_ExitHold_Delay > 0f)
		{
			yield return Wait(m_axeAttack_ExitHold_Delay);
		}
		yield return Default_Animation("AxeJump_Exit", m_axeAttack_Exit_AnimSpeed, m_axeAttack_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_axeAttack_Exit_IdleDuration, m_axeAttack_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Staff_Throw()
	{
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		if (UseVariant)
		{
			m_staffBeamWarningProjectile = this.FireProjectile("SpellSwordStaffWarningForwardBeamAdvancedProjectile", 8);
			yield return Default_TellIntroAndLoop("StaffForward_Tell_Intro", m_staffThrow_TellIntro_AnimSpeed, "StaffForward_Tell_Hold", m_staffThrow_TellHold_AnimSpeed, m_staffThrow_TellIntroAndHold_Delay);
		}
		else
		{
			m_staffBeamWarningProjectile = this.FireProjectile("SpellSwordStaffWarningForwardBeamProjectile", 8);
			yield return Default_TellIntroAndLoop("StaffForward_Tell_Intro", m_staffThrow_TellIntro_AnimSpeed, "StaffForward_Tell_Hold", m_staffThrow_TellHold_AnimSpeed, m_staffThrow_TellIntroAndHold_Delay);
		}
		SetAttackingWithContactDamage(enable: true);
		yield return Default_Animation("StaffForward_Attack_Intro", m_staffThrow_AttackIntro_AnimSpeed, m_staffThrow_AttackIntro_Delay);
		yield return Default_Animation("StaffForward_Attack_Hold", m_staffThrow_AttackHold_AnimSpeed, m_staffThrow_AttackHold_Delay, waitUntilAnimationComplete: false);
		StopProjectile(ref m_staffBeamWarningProjectile);
		if (UseVariant)
		{
			m_staffBeamProjectile = this.FireProjectile("SpellSwordStaffForwardBeamAdvancedProjectile", 8);
		}
		else
		{
			m_staffBeamProjectile = this.FireProjectile("SpellSwordStaffForwardBeamProjectile", 8);
		}
		if (UseVariant)
		{
			if (m_staffThrow_BeamAttack_Variant_Duration > 0f)
			{
				yield return Wait(m_staffThrow_BeamAttack_Variant_Duration);
			}
		}
		else if (m_staffThrow_BeamAttack_Duration > 0f)
		{
			yield return Wait(m_staffThrow_BeamAttack_Duration);
		}
		StopProjectile(ref m_staffBeamProjectile);
		SetAttackingWithContactDamage(enable: false);
		yield return Wait(m_staffThrow_AttackDuration_EndDelay);
		yield return Default_Animation("StaffForward_Attack_Exit", m_staffThrow_Exit_AnimSpeed, m_staffThrow_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_staffThrow_Exit_IdleDuration, m_staffThrow_AttackCD);
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
		yield return base.DeathAnim();
		base.EnemyController.LockFlip = true;
		SetVelocityX(0f, additive: false);
		base.EnemyController.ModeshiftDamageMod = m_modeShift_Damage_Mod;
		yield return Default_Animation("ModeShift_Intro", m_modeShift_Downed_AnimSpeed, m_modeShift_Downed_Delay);
		yield return Default_Animation("ModeShift_GetUp", m_modeShift_GetUp_AnimSpeed, 0f);
		if (ModeShiftEvent != null)
		{
			ModeShiftEvent.Invoke();
		}
		base.LogicController.SetLogicBlockEnabled("Axe_Attack", enabled: true);
		base.LogicController.SetLogicBlockEnabled("Staff_Throw", enabled: true);
		m_modeShiftProjectile = this.FireProjectile("SpellSwordShoutWarningProjectile", 9);
		if (m_modeShift_GetUp_Delay > 0f)
		{
			yield return Wait(m_modeShift_GetUp_Delay);
		}
		SetAttackingWithContactDamage(enable: true);
		yield return Default_Animation("ModeShift_Scream_Intro", m_modeShift_AttackIntro_AnimSpeed, m_modeShift_AttackIntro_Delay);
		StopProjectile(ref m_modeShiftProjectile);
		yield return Default_Animation("ModeShift_Scream_Hold", m_modeShift_AttackHold_AnimSpeed, m_modeShift_AttackHold_Delay, waitUntilAnimationComplete: false);
		m_modeShiftProjectile2 = this.FireProjectile("SpellSwordShoutAttackProjectile", 9);
		if (m_modeShift_AttackDuration_Initial_Delay > 0f)
		{
			yield return Wait(m_modeShift_AttackDuration_Initial_Delay);
		}
		CDGHelper.VectorToAngle(base.EnemyController.TargetController.Midpoint - base.EnemyController.Midpoint);
		if (UseVariant)
		{
			for (int j = 0; (float)j < m_modeShift_ProjectilesThrown_Variant; j++)
			{
				float speedMod = Random.Range(m_modeShift_Shout_ThrowPower.x, m_modeShift_Shout_ThrowPower.y);
				CDGHelper.VectorToAngle(base.EnemyController.TargetController.Midpoint - base.EnemyController.Midpoint);
				float angle = 90 + Random.Range((int)m_modeShift_Shout_ThrowAngle.x, (int)m_modeShift_Shout_ThrowAngle.y);
				this.FireProjectile("SpellSwordShoutHomingBoltProjectile", 9, matchFacing: false, angle, speedMod);
				yield return Wait(m_modeShift_AttackDuration_Fire_Duration / m_modeShift_ProjectilesThrown_Variant);
			}
			if (m_modeShift_AttackDuration_SecondShot_Delay > 0f)
			{
				yield return Wait(m_modeShift_AttackDuration_SecondShot_Delay);
			}
		}
		for (int j = 0; (float)j < m_modeShift_ProjectilesThrown_Variant; j++)
		{
			float speedMod2 = Random.Range(m_modeShift_Shout_ThrowPower.x, m_modeShift_Shout_ThrowPower.y);
			CDGHelper.VectorToAngle(base.EnemyController.TargetController.Midpoint - base.EnemyController.Midpoint);
			float angle2 = 90 + Random.Range((int)m_modeShift_Shout_ThrowAngle.x, (int)m_modeShift_Shout_ThrowAngle.y);
			this.FireProjectile("SpellSwordShoutHomingBoltProjectile", 9, matchFacing: false, angle2, speedMod2);
			yield return Wait(m_modeShift_AttackDuration_Fire_Duration / m_modeShift_ProjectilesThrown_Variant);
		}
		if (m_modeShift_AttackDuration_Exit_Delay > 0f)
		{
			yield return Wait(m_modeShift_AttackDuration_Exit_Delay);
		}
		SetAttackingWithContactDamage(enable: false);
		StopProjectile(ref m_modeShiftProjectile2);
		yield return Default_Animation("ModeShift_Scream_Exit", m_modeShift_Exit_AnimSpeed, m_modeShift_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ModeshiftDamageMod = 1f;
		base.EnemyController.ResetBaseValues();
		base.EnemyController.StatusEffectController.ImmuneToAllStatusEffects = false;
		yield return Default_Attack_Cooldown(m_modeShift_Exit_IdleDuration, m_modeShift_AttackCD);
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

	public override IEnumerator SpawnAnim()
	{
		yield return Default_Animation("Intro_Idle", m_spawn_Idle_AnimSpeed, m_spawn_Idle_Delay);
		MusicManager.PlayMusic(SongID.CastleBossBGM_ASTIP_Boss1_Phase1, isOverride: false);
		yield return Default_Animation("Intro", m_spawn_Intro_AnimSpeed, m_spawn_Intro_Delay);
		yield return Default_Animation(BaseAIScript.NEUTRAL_STATE, 1f, 1f, waitUntilAnimationComplete: false);
	}

	public override void OnLBCompleteOrCancelled()
	{
		StopProjectile(ref m_swordFlameProjectile);
		StopProjectile(ref m_swordFlameProjectile2);
		StopProjectile(ref m_swordFlameProjectile3);
		StopProjectile(ref m_staffBeamWarningProjectile);
		StopProjectile(ref m_staffBeamProjectile);
		m_isStaffBeaming = false;
		StopProjectile(ref m_modeShiftProjectile);
		StopProjectile(ref m_modeShiftProjectile2);
		StopProjectile(ref m_axeSpinProjectile);
		m_isAxeSpinning = false;
		base.OnLBCompleteOrCancelled();
	}

	public override void Pause()
	{
		base.Pause();
		if ((bool)m_axeSpinProjectile && m_axeSpinProjectile.isActiveAndEnabled && m_axeSpinProjectile.OwnerController == base.EnemyController)
		{
			m_isAxeSpinning = true;
			StopProjectile(ref m_axeSpinProjectile);
		}
		if ((bool)m_staffBeamProjectile && m_staffBeamProjectile.isActiveAndEnabled && m_staffBeamProjectile.OwnerController == base.EnemyController)
		{
			m_isStaffBeaming = true;
			StopProjectile(ref m_staffBeamProjectile);
			if (UseVariant)
			{
				m_staffBeamWarningProjectile = this.FireProjectile("SpellSwordStaffWarningForwardBeamAdvancedProjectile", 8);
			}
			else
			{
				m_staffBeamWarningProjectile = this.FireProjectile("SpellSwordStaffWarningForwardBeamProjectile", 8);
			}
		}
		if ((bool)m_swordFlameProjectile && m_swordFlameProjectile.isActiveAndEnabled && m_swordFlameProjectile.OwnerController == base.EnemyController)
		{
			StopProjectile(ref m_swordFlameProjectile);
			StopProjectile(ref m_swordFlameProjectile2);
			StopProjectile(ref m_swordFlameProjectile3);
		}
	}

	public override void Unpause()
	{
		base.Unpause();
		if (m_isAxeSpinning)
		{
			m_isAxeSpinning = false;
			m_axeSpinProjectile = this.FireProjectile("SpellSwordAxeSpinProjectile", 1, matchFacing: false);
		}
		if (m_isStaffBeaming)
		{
			m_isStaffBeaming = false;
			if (UseVariant)
			{
				m_staffBeamProjectile = this.FireProjectile("SpellSwordStaffForwardBeamAdvancedProjectile", 8);
			}
			else
			{
				m_staffBeamProjectile = this.FireProjectile("SpellSwordStaffForwardBeamProjectile", 8);
			}
			StopProjectile(ref m_staffBeamWarningProjectile);
		}
	}
}
