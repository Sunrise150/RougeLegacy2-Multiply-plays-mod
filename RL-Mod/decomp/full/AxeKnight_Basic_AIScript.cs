using System.Collections;
using FMOD.Studio;
using RLAudio;
using UnityEngine;

public class AxeKnight_Basic_AIScript : BaseAIScript
{
	private EventInstance m_axeSpinAudioEvent;

	private const string JUMP_TELL_INTRO = "JumpSpin_Tell_Intro";

	private const string JUMP_TELL_HOLD = "JumpSpin_Tell_Hold";

	private const string JUMP_ATTACK_INTRO = "JumpSpin_Attack_Intro";

	private const string JUMP_ATTACK_HOLD = "JumpSpin_Attack_Hold";

	private const string JUMP_EXIT = "JumpSpin_Exit";

	private const string JUMP_PROJECTILE_NAME = "AxeKnightSpinProjectile";

	protected float m_jump_Tell_Delay = 0.4f;

	protected float m_jump_Tell_AnimationSpeed = 0.75f;

	protected float m_jump_AttackIntro_Delay;

	protected float m_jump_AttackIntro_AnimationSpeed = 0.75f;

	protected float m_jump_AttackHold_Delay;

	protected float m_jump_AttackHold_AnimationSpeed = 0.75f;

	protected float m_jump_Exit_Delay;

	protected float m_jump_Exit_ForceIdle = 0.15f;

	protected float m_jump_Exit_AttackCD;

	protected Vector2 m_jump_Power = new Vector2(13f, 26f);

	private Projectile_RL m_axeJumpProjectile;

	private Projectile_RL m_axeSpinAttackProjectile;

	private const string AXE_TELL_INTRO = "AxeThrow_Tell_Intro";

	private const string AXE_TELL_HOLD = "AxeThrow_Tell_Hold";

	private const string AXE_ATTACK_INTRO = "AxeThrow_Attack_Intro";

	private const string AXE_ATTACK_HOLD = "AxeThrow_Attack_Hold";

	private const string AXE_EXIT = "AxeThrow_Exit";

	private const string AXE_PROJECTILE_NAME = "AxeKnightBoltProjectile";

	protected float m_throwAxe_TellIntro_AnimationSpeed = 0.75f;

	protected float m_throwAxe_TellHold_AnimationSpeed = 1.35f;

	protected const float m_throwAxe_TellIntroAndHold_Delay = 0.85f;

	protected float m_throwAxe_AttackIntro_AnimationSpeed = 1.1f;

	protected float m_throwAxe_AttackIntro_Delay;

	protected float m_throwAxe_AttackHold_AnimationSpeed = 1.1f;

	protected float m_throwAxe_AttackHold_Delay = 1.25f;

	protected float m_throwAxe_Exit_AnimationSpeed = 1f;

	protected float m_throwAxe_Exit_Delay;

	protected float m_throwAxe_Exit_ForceIdle = 0.15f;

	protected float m_throwAxe_Exit_AttackCD;

	protected int m_throwAxe_Angle;

	protected int m_throwAxe_Angle2 = 10;

	private const string SPIN_ATTACK_TELL_INTRO = "BodySpin_Tell_Intro";

	private const string SPIN_ATTACK_TELL_HOLD = "BodySpin_Tell_Hold";

	private const string SPIN_ATTACK_ATTACK_INTRO = "BodySpin_Attack_Intro";

	private const string SPIN_ATTACK_ATTACK_HOLD = "BodySpin_Attack_Hold";

	private const string SPIN_ATTACK_EXIT = "BodySpin_Exit";

	private const string SPIN_ATTACK_PROJECTILE_NAME = "AxeKnightWhirlingProjectile";

	private const string SPIN_ATTACK_LAUNCHER_PROJECTILE_NAME = "AxeKnightWhirlingLauncherProjectile";

	protected float m_spinAttack_TellIntro_AnimationSpeed = 0.75f;

	protected float m_spinAttack_TellHold_AnimationSpeed = 1.35f;

	protected const float m_spinAttack_TellIntroAndHold_Delay = 1.1f;

	protected float m_spinAttack_AttackIntro_AnimationSpeed = 1.1f;

	protected float m_spinAttack_AttackIntro_Delay;

	protected float m_spinAttack_AttackHold_AnimationSpeed = 2f;

	protected float m_spinAttack_AttackHold_Delay = 3f;

	protected float m_spinAttack_Exit_AnimationSpeed = 1f;

	protected float m_spinAttack_Exit_Delay;

	protected float m_spinAttack_Exit_ForceIdle = 0.15f;

	protected float m_spinAttack_Exit_AttackCD;

	protected int m_spinAttack_Angle = 90;

	protected int m_spinAttack_Angle2 = 10;

	protected float m_spinAttack_Acceleration = 0.08f;

	private Coroutine m_chasePlayerPersistentCoroutine;

	private Coroutine m_spinAttackBlinkCoroutine;

	private bool m_isJumpAttacking;

	private bool m_isSpinAttacking;

	protected override float WalkAnimSpeedMod => 1.4f;

	protected override Vector2 IdleDuration => new Vector2(0.25f, 0.7f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.45f, 0.85f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.45f, 0.85f);

	protected virtual bool m_jump_spawnAxeOnLand => false;

	protected virtual bool m_raiseKnockbackDefenseWhileAttacking => true;

	protected virtual int m_knockbackDefenseBoostOverride => 7;

	protected virtual bool m_throwSecondAxe => false;

	protected virtual bool m_throwSecondSpinAxe => false;

	protected virtual float m_spinAttack_ChaseSpeed => 11f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[4] { "AxeKnightSpinProjectile", "AxeKnightBoltProjectile", "AxeKnightWhirlingProjectile", "AxeKnightWhirlingLauncherProjectile" };
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		if (!m_axeSpinAudioEvent.isValid())
		{
			m_axeSpinAudioEvent = AudioUtility.GetEventInstance("event:/SFX/Enemies/sfx_enemy_axeKnight_spin_loop", base.EnemyController.transform);
		}
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
		if (m_raiseKnockbackDefenseWhileAttacking)
		{
			base.EnemyController.BaseKnockbackDefense = m_knockbackDefenseBoostOverride;
		}
		yield return WaitUntilIsGrounded();
		StopAndFaceTarget();
		base.EnemyController.AlwaysFacing = false;
		yield return Default_Animation("JumpSpin_Tell_Intro", m_jump_Tell_AnimationSpeed, m_jump_Tell_Delay);
		SetAttackingWithContactDamage(enable: true);
		if (base.EnemyController.IsFacingRight)
		{
			SetVelocityX(m_jump_Power.x, additive: true);
		}
		else
		{
			SetVelocityX(0f - m_jump_Power.x, additive: true);
		}
		SetVelocityY(m_jump_Power.y, additive: false);
		yield return Default_Animation("JumpSpin_Attack_Intro", m_jump_AttackIntro_AnimationSpeed, m_jump_AttackIntro_Delay);
		yield return this.ChangeAnimationState("JumpSpin_Attack_Hold");
		yield return Wait(0.05f);
		m_axeJumpProjectile = this.FireProjectile("AxeKnightSpinProjectile", 1, matchFacing: false);
		yield return WaitUntilIsGrounded();
		SetAttackingWithContactDamage(enable: false);
		SetVelocity(0f, 0f, additive: false);
		if (m_jump_spawnAxeOnLand)
		{
			this.FireProjectile("AxeKnightBoltProjectile", 1, matchFacing: false, m_throwAxe_Angle);
			this.FireProjectile("AxeKnightBoltProjectile", 1, matchFacing: false, 180f);
		}
		StopProjectile(ref m_axeJumpProjectile);
		yield return Default_Animation("JumpSpin_Exit", m_jump_Tell_AnimationSpeed, m_jump_Tell_Delay);
		if (m_jump_Exit_Delay > 0f)
		{
			Wait(m_jump_Exit_Delay);
		}
		base.EnemyController.AlwaysFacing = true;
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_jump_Exit_ForceIdle, m_jump_Exit_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Throw_Axe_Attack()
	{
		StopAndFaceTarget();
		base.EnemyController.AlwaysFacing = false;
		yield return Default_TellIntroAndLoop("AxeThrow_Tell_Intro", m_throwAxe_TellIntro_AnimationSpeed, "AxeThrow_Tell_Hold", m_throwAxe_TellHold_AnimationSpeed, 0.85f);
		yield return Default_Animation("AxeThrow_Attack_Intro", m_throwAxe_AttackIntro_AnimationSpeed, m_throwAxe_AttackIntro_Delay);
		yield return Default_Animation("AxeThrow_Attack_Hold", m_throwAxe_AttackHold_AnimationSpeed, 0f, waitUntilAnimationComplete: false);
		float num = CDGHelper.VectorToAngle(base.EnemyController.TargetController.Midpoint - base.EnemyController.Midpoint);
		if (!base.EnemyController.IsFacingRight)
		{
			num = CDGHelper.WrapAngleDegrees(-180f - num, convertToHalfRadius: true);
		}
		this.FireProjectile("AxeKnightBoltProjectile", 1, matchFacing: true, num + (float)m_throwAxe_Angle);
		if (m_throwSecondAxe)
		{
			this.FireProjectile("AxeKnightBoltProjectile", 1, matchFacing: true, num + (float)m_throwAxe_Angle2, 1.25f);
		}
		SetAttackingWithContactDamage(enable: false);
		yield return Wait(m_throwAxe_AttackHold_Delay);
		yield return Default_Animation("AxeThrow_Exit", m_throwAxe_Exit_AnimationSpeed, m_throwAxe_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		base.EnemyController.AlwaysFacing = true;
		yield return Default_Attack_Cooldown(m_throwAxe_Exit_ForceIdle, m_throwAxe_Exit_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator SpinAttack_Attack()
	{
		StopAndFaceTarget();
		base.EnemyController.AlwaysFacing = false;
		yield return Default_TellIntroAndLoop("BodySpin_Tell_Intro", m_spinAttack_TellIntro_AnimationSpeed, "BodySpin_Tell_Hold", m_spinAttack_TellHold_AnimationSpeed, 1.1f);
		SetAttackingWithContactDamage(enable: true);
		if (m_axeSpinAudioEvent.isValid())
		{
			AudioManager.PlayAttached(null, m_axeSpinAudioEvent, base.EnemyController.gameObject);
		}
		yield return Default_Animation("BodySpin_Attack_Intro", m_spinAttack_AttackIntro_AnimationSpeed, m_spinAttack_AttackIntro_Delay);
		yield return Default_Animation("BodySpin_Attack_Hold", m_spinAttack_AttackHold_AnimationSpeed, 0f, waitUntilAnimationComplete: false);
		this.StopPersistentCoroutine(m_chasePlayerPersistentCoroutine);
		m_chasePlayerPersistentCoroutine = this.RunPersistentCoroutine(ChasePlayerPersistentCoroutine(m_spinAttack_ChaseSpeed));
		m_axeSpinAttackProjectile = this.FireProjectile("AxeKnightWhirlingProjectile", 3, matchFacing: false);
		yield return Wait(m_spinAttack_AttackHold_Delay - 1f);
		Color red = Color.red;
		red.a = 0.5f;
		m_spinAttackBlinkCoroutine = this.RunPersistentCoroutine(SpinAttackBlinkCoroutine(1f, red));
		yield return Wait(1f);
		this.StopPersistentCoroutine(m_spinAttackBlinkCoroutine);
		SetAttackingWithContactDamage(enable: false);
		StopProjectile(ref m_axeSpinAttackProjectile);
		this.FireProjectile("AxeKnightWhirlingLauncherProjectile", 1, matchFacing: true, m_spinAttack_Angle);
		if (m_throwSecondSpinAxe)
		{
			this.FireProjectile("AxeKnightWhirlingLauncherProjectile", 1, matchFacing: true, m_spinAttack_Angle2, 1.25f);
		}
		base.EnemyController.ResetTurnTrigger();
		base.EnemyController.ResetBaseValues();
		base.EnemyController.AlwaysFacing = true;
		this.StopPersistentCoroutine(m_chasePlayerPersistentCoroutine);
		if (m_axeSpinAudioEvent.isValid())
		{
			AudioManager.Stop(m_axeSpinAudioEvent);
		}
		yield return Default_Animation("BodySpin_Exit", m_spinAttack_Exit_AnimationSpeed, m_spinAttack_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		yield return Default_Attack_Cooldown(m_spinAttack_Exit_ForceIdle, m_spinAttack_Exit_AttackCD);
	}

	private IEnumerator ChasePlayerPersistentCoroutine(float speed)
	{
		float currentSpeed = ((!base.EnemyController.IsTargetToMyRight) ? (0f - speed) : speed);
		while (true)
		{
			currentSpeed = (base.IsPaused ? 0f : ((!base.EnemyController.IsTargetToMyRight) ? Mathf.Max(0f - speed, currentSpeed - m_spinAttack_Acceleration) : Mathf.Min(speed, currentSpeed + m_spinAttack_Acceleration)));
			base.EnemyController.SetVelocityX(currentSpeed, additive: false);
			yield return null;
		}
	}

	private IEnumerator SpinAttackBlinkCoroutine(float duration, Color color)
	{
		while (true)
		{
			float blinkDuration = Time.time + base.EnemyController.BlinkPulseEffect.SingleBlinkDuration * 2f;
			base.EnemyController.BlinkPulseEffect.StartSingleBlinkEffect(color);
			while (Time.time < blinkDuration)
			{
				yield return null;
			}
		}
	}

	public override void OnLBCompleteOrCancelled()
	{
		base.OnLBCompleteOrCancelled();
		this.StopPersistentCoroutine(m_chasePlayerPersistentCoroutine);
		this.StopPersistentCoroutine(m_spinAttackBlinkCoroutine);
		StopProjectile(ref m_axeJumpProjectile);
		StopProjectile(ref m_axeSpinAttackProjectile);
		m_isJumpAttacking = false;
		m_isSpinAttacking = false;
		if (m_axeSpinAudioEvent.isValid())
		{
			AudioManager.Stop(m_axeSpinAudioEvent);
		}
	}

	public override void Pause()
	{
		base.Pause();
		if ((bool)m_axeJumpProjectile && m_axeJumpProjectile.isActiveAndEnabled && m_axeJumpProjectile.OwnerController == base.EnemyController)
		{
			m_isJumpAttacking = true;
			StopProjectile(ref m_axeJumpProjectile);
		}
		if ((bool)m_axeSpinAttackProjectile && m_axeSpinAttackProjectile.isActiveAndEnabled && m_axeSpinAttackProjectile.OwnerController == base.EnemyController)
		{
			m_isSpinAttacking = true;
			StopProjectile(ref m_axeSpinAttackProjectile);
		}
	}

	public override void Unpause()
	{
		base.Unpause();
		if (m_isSpinAttacking)
		{
			m_axeSpinAttackProjectile = this.FireProjectile("AxeKnightWhirlingProjectile", 3, matchFacing: false);
			m_isSpinAttacking = false;
		}
		if (m_isJumpAttacking)
		{
			m_axeJumpProjectile = this.FireProjectile("AxeKnightSpinProjectile", 1, matchFacing: false);
			m_isJumpAttacking = false;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		StopProjectile(ref m_axeJumpProjectile);
		StopProjectile(ref m_axeSpinAttackProjectile);
		if (m_axeSpinAudioEvent.isValid())
		{
			AudioManager.Stop(m_axeSpinAudioEvent, FMOD.Studio.STOP_MODE.IMMEDIATE);
		}
	}

	private void OnDestroy()
	{
		if (m_axeSpinAudioEvent.isValid())
		{
			m_axeSpinAudioEvent.release();
		}
	}
}
