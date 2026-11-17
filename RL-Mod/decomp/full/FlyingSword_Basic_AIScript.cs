using System.Collections;
using UnityEngine;

public class FlyingSword_Basic_AIScript : BaseAIScript
{
	private const string GENERIC_PROJECTILE_NAME = "FlyingSwordProjectile";

	private const string MINIBOSS_PROJECTILE_NAME = "FlyingSwordMinibossProjectile";

	private const string DAGGER_PROJECTILE_NAME = "FlyingSwordDaggerProjectile";

	protected float m_thrust_TellIntro_AnimationSpeed = 1f;

	protected float m_thrust_TellIntro_Delay;

	protected float m_thrust_TellHold_AnimationSpeed = 1.5f;

	protected float m_thrust_TellHold_Delay = 0.55f;

	protected float m_thrust_TellHold_ReverseDuration = 0.325f;

	protected float m_thrust_AttackIntro_AnimationSpeed = 2.75f;

	protected float m_thrust_AttackIntro_Delay;

	protected float m_thrust_AttackHold_AnimationSpeed = 2.75f;

	protected float m_thrust_AttackHold_Delay;

	protected float m_thrust_ExitIntro_AnimationSpeed = 1.2f;

	protected float m_thrust_ExitIntro_Delay = 0.175f;

	protected float m_thrust_Exit_AnimationSpeed = 1f;

	protected float m_thrust_Exit_Delay;

	protected float m_thrust_Attack_ReverseSpeed = -3f;

	protected float m_thrust_Attack_ForwardDuration = 0.625f;

	protected float m_thrust_Attack_ForwardSpeed = 28f;

	protected float m_thrust_Exit_AttackCD = 1f;

	protected float m_thrustFast_ExitIntro_AnimationSpeed = 1.25f;

	protected float m_vertSpin_TellIntro_AnimationSpeed = 1f;

	protected float m_vertSpin_TellHold_AnimationSpeed = 1f;

	protected float m_vertSpin_TellIntroAndHold_Delay = 0.6f;

	protected float m_vertSpin_AttackIntro_AnimationSpeed = 1f;

	protected float m_vertSpin_AttackIntro_Delay;

	protected const float m_vertSpin_AttackHold_AnimationSpeed = 1f;

	protected const float m_vertSpin_AttackHold_Delay = 0.5f;

	protected float m_vertSpin_ExitIntro_AnimationSpeed = 1f;

	protected const float m_vertSpin_ExitIntro_EndDelay = 0.15f;

	protected const float m_vertSpin_Exit_ForceIdle = 0.25f;

	protected float m_vertSpin_Exit_AttackCD = 5f;

	protected override Vector2 IdleDuration => new Vector2(0.45f, 0.75f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.8f, 1.1f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.8f, 1.1f);

	protected virtual float m_thrust_Attack_InitialTargetTurnRate => 900f;

	protected virtual float m_thrust_Attack_TurnRate => 0f;

	protected virtual float m_thrust_Exit_ForceIdle => 0.25f;

	protected virtual float m_thrustFast_DashAttackAmount => 0f;

	protected virtual float m_thrustFast_AttackTell_Delay => 0f;

	protected virtual float m_thrustFast_AttackHold_Delay => 0f;

	protected virtual float m_thrustFastAttackExitEndDuration_Fast => 0f;

	protected virtual bool m_thrust_SpawnProjectilesAtEnd => false;

	protected virtual bool m_thrust_SpawnMinibossProjectilesAtEnd => false;

	protected virtual float m_vertSpin_Attack_TurnSpeed => 0f;

	protected virtual float m_vertSpin_Attack_MovementSpeed => 0f;

	protected virtual int m_vertSpin_Attack_TotalLoops => 2;

	protected virtual float m_vertSpin_Attack_DelayPerLoop => 0.225f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[3] { "FlyingSwordProjectile", "FlyingSwordMinibossProjectile", "FlyingSwordDaggerProjectile" };
	}

	public IEnumerator Custom_Tell_Animation(float tellIntroDuration, float tellHoldDuration, float reverseDuration)
	{
		base.EnemyController.FollowOffset = Vector3.zero;
		base.EnemyController.BaseTurnSpeed = m_thrust_Attack_InitialTargetTurnRate;
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		yield return Default_Animation("Thrust_Tell_Intro", m_thrust_TellIntro_AnimationSpeed, 0f, waitUntilAnimationComplete: false);
		SetAttackingWithContactDamage(enable: true);
		yield return Wait(0.25f);
		base.EnemyController.PivotFollowsOrientation = true;
		yield return WaitUntilAnimComplete(DefaultAnimationLayer);
		if (tellIntroDuration > 0f)
		{
			yield return Wait(tellIntroDuration);
		}
		yield return Default_Animation("Thrust_Tell_Hold", m_thrust_TellHold_AnimationSpeed, tellHoldDuration, waitUntilAnimationComplete: false);
		yield return Default_Animation("Thrust_Attack_Intro", m_thrust_AttackIntro_AnimationSpeed, m_thrust_AttackIntro_Delay);
		yield return Default_Animation("Thrust_Attack_Hold_BackUp", m_thrust_AttackHold_AnimationSpeed, 0f, waitUntilAnimationComplete: false);
		base.EnemyController.BaseSpeed *= 0f - m_thrust_Attack_ReverseSpeed;
		base.EnemyController.FlyingMovementType = FlyingMovementType.Away;
		if (reverseDuration > 0f)
		{
			yield return Wait(reverseDuration);
		}
		base.EnemyController.BaseSpeed = base.EnemyController.EnemyData.Speed;
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
	}

	public IEnumerator Custom_Thrust_Attack()
	{
		base.EnemyController.BaseTurnSpeed = m_thrust_Attack_TurnRate;
		base.EnemyController.FollowTarget = true;
		base.EnemyController.LockFlip = true;
		base.EnemyController.PivotFollowsOrientation = true;
		this.SetAnimationSpeedMultiplier(m_thrust_AttackHold_AnimationSpeed);
		yield return Default_Animation("Thrust_Attack_Hold", m_thrust_AttackHold_AnimationSpeed, 0f, waitUntilAnimationComplete: false);
		base.EnemyController.FlyingMovementType = FlyingMovementType.Towards;
		base.EnemyController.BaseSpeed = m_thrust_Attack_ForwardSpeed;
		if (m_thrust_Attack_ForwardDuration > 0f)
		{
			yield return Wait(m_thrust_Attack_ForwardDuration);
		}
		base.EnemyController.BaseSpeed = base.EnemyController.EnemyData.Speed;
		base.EnemyController.BaseTurnSpeed = 0f;
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		if (m_thrust_SpawnProjectilesAtEnd)
		{
			this.FireProjectile("FlyingSwordProjectile", new Vector2(0f, 0f), matchFacing: true, 45f);
			this.FireProjectile("FlyingSwordProjectile", new Vector2(0f, 0f), matchFacing: true, 135f);
			this.FireProjectile("FlyingSwordProjectile", new Vector2(0f, 0f), matchFacing: true, 225f);
			this.FireProjectile("FlyingSwordProjectile", new Vector2(0f, 0f), matchFacing: true, 315f);
		}
		else if (m_thrust_SpawnMinibossProjectilesAtEnd)
		{
			this.FireProjectile("FlyingSwordMinibossProjectile", new Vector2(0f, 0f), matchFacing: true, 45f);
			this.FireProjectile("FlyingSwordMinibossProjectile", new Vector2(0f, 0f), matchFacing: true, 135f);
			this.FireProjectile("FlyingSwordMinibossProjectile", new Vector2(0f, 0f), matchFacing: true, 225f);
			this.FireProjectile("FlyingSwordMinibossProjectile", new Vector2(0f, 0f), matchFacing: true, 315f);
		}
		if (m_thrust_AttackHold_Delay > 0f)
		{
			yield return Wait(m_thrust_AttackHold_Delay);
		}
	}

	public IEnumerator Custom_Fast_Exit_Animation()
	{
		yield return Default_Animation("Thrust_Exit_Intro", m_thrustFast_ExitIntro_AnimationSpeed, m_thrustFastAttackExitEndDuration_Fast);
		base.EnemyController.FollowTarget = true;
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetPivotRotation();
		base.EnemyController.ResetBaseValues();
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Thrust_Attack()
	{
		yield return Custom_Tell_Animation(m_thrust_TellIntro_Delay, m_thrust_TellHold_Delay, m_thrust_TellHold_ReverseDuration);
		SetAttackingWithContactDamage(enable: true);
		yield return Custom_Thrust_Attack();
		for (int i = 0; (float)i < m_thrustFast_DashAttackAmount; i++)
		{
			yield return Custom_Fast_Exit_Animation();
			yield return Custom_Tell_Animation(m_thrustFast_AttackTell_Delay, m_thrustFast_AttackHold_Delay, m_thrust_TellHold_ReverseDuration);
			yield return Custom_Thrust_Attack();
		}
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("Thrust_Exit_Intro", m_thrust_ExitIntro_AnimationSpeed, m_thrust_ExitIntro_Delay);
		base.EnemyController.PivotFollowsOrientation = false;
		base.EnemyController.ResetPivotRotation();
		yield return Default_Animation("Thrust_Exit", m_thrust_Exit_AnimationSpeed, m_thrust_Exit_Delay);
		base.EnemyController.FollowTarget = true;
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetBaseValues();
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		yield return Default_Attack_Cooldown(m_thrust_Exit_ForceIdle, m_thrust_Exit_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Vertical_Spin_Attack()
	{
		base.EnemyController.AlwaysFacing = false;
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		base.EnemyController.FollowOffset = Vector3.zero;
		yield return Default_TellIntroAndLoop("VerticalSpin_Tell_Intro", m_vertSpin_TellIntro_AnimationSpeed, "VerticalSpin_Tell_Hold", m_vertSpin_TellHold_AnimationSpeed, m_vertSpin_TellIntroAndHold_Delay);
		SetAttackingWithContactDamage(enable: true);
		yield return Default_Animation("VerticalSpin_Attack_Intro", m_vertSpin_AttackIntro_AnimationSpeed, m_vertSpin_AttackIntro_Delay);
		base.EnemyController.FlyingMovementType = FlyingMovementType.Towards;
		base.EnemyController.BaseSpeed = m_vertSpin_Attack_MovementSpeed;
		base.EnemyController.BaseTurnSpeed = m_vertSpin_Attack_TurnSpeed;
		yield return Default_Animation("VerticalSpin_Attack_Hold", 1f, 0.5f);
		for (int i = 0; i < m_vertSpin_Attack_TotalLoops; i++)
		{
			float angle = CDGHelper.VectorToAngle(base.EnemyController.TargetController.Midpoint - base.EnemyController.Midpoint);
			this.FireProjectile("FlyingSwordDaggerProjectile", new Vector2(0f, 0f), matchFacing: false, angle);
			yield return Wait(m_vertSpin_Attack_DelayPerLoop);
		}
		base.EnemyController.ResetBaseValues();
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("VerticalSpin_Exit", m_vertSpin_ExitIntro_AnimationSpeed, 0.15f);
		base.EnemyController.ResetTurnTrigger();
		yield return Default_Attack_Cooldown(0.25f, m_vertSpin_Exit_AttackCD);
		base.EnemyController.AlwaysFacing = true;
	}

	public override IEnumerator WalkAway()
	{
		base.EnemyController.ResetPivotRotation();
		return base.WalkAway();
	}

	public override IEnumerator WalkTowards()
	{
		base.EnemyController.ResetPivotRotation();
		return base.WalkTowards();
	}

	public override IEnumerator Idle()
	{
		base.EnemyController.ResetPivotRotation();
		base.EnemyController.FollowTarget = true;
		base.EnemyController.LockFlip = false;
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		this.SetAnimationSpeedMultiplier(IdleAnimSpeedMod);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		float num = Random.Range(IdleDuration.x, IdleDuration.y);
		if (num > 0f)
		{
			yield return Wait(num);
		}
		this.SetAnimationSpeedMultiplier(IdleAnimSpeedMod);
		yield return null;
	}

	public override void OnLBCompleteOrCancelled()
	{
		base.EnemyController.PivotFollowsOrientation = false;
		base.EnemyController.LockFlip = false;
		base.OnLBCompleteOrCancelled();
	}
}
