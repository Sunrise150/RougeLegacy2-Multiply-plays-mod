using System.Collections;
using UnityEngine;

public class RogueKnight_Basic_AIScript : BaseAIScript
{
	protected float m_throw_TellIntro_AnimationSpeed = 0.75f;

	protected float m_throw_TellHold_AnimationSpeed = 0.65f;

	protected float m_throw_TellIntroAndHold_Delay = 0.9f;

	protected float m_throw_RepeatTellIntroAndHold_Delay = 0.5f;

	protected float m_throw_AttackIntro_AnimationSpeed = 2f;

	protected float m_throw_AttackIntro_Delay;

	protected float m_throw_AttackHold_AnimationSpeed = 2f;

	protected float m_throw_AttackHold_Delay = 0.25f;

	protected float m_throw_Exit_AnimationSpeed = 0.45f;

	protected float m_throw_Exit_Delay = 0.15f;

	[SerializeField]
	private GameObject m_aimIndicator;

	protected override Vector2 IdleDuration => new Vector2(0.35f, 0.75f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.5f, 0.85f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.5f, 0.5f);

	protected virtual float m_dash_TellIntro_AnimationSpeed => 1.5f;

	protected virtual float m_dash_TellHold_AnimationSpeed => 1f;

	protected virtual float m_dash_TellIntroAndHold_Delay => 0.8f;

	protected virtual float m_dash_RepeatTellIntroAndHold_Delay => 0.5f;

	protected virtual float m_dash_AttackIntro_AnimationSpeed => 1f;

	protected virtual float m_dash_AttackIntro_Delay => 0f;

	protected virtual float m_dash_AttackHold_AnimationSpeed => 1f;

	protected virtual float m_dash_AttackHold_Delay => 0f;

	protected virtual float m_dash_Exit_AnimationSpeed => 0.65f;

	protected virtual float m_dash_Exit_Delay => 0f;

	protected virtual float m_dash_Exit_ForceIdle => 0.3f;

	protected virtual float m_dash_AttackCD => 2.5f;

	protected virtual bool m_raiseKnockbackDefenseWhileAttacking => true;

	protected virtual int m_knockbackDefenseBoostOverride => 5;

	protected virtual float m_dash_Attack_Duration => 0.325f;

	protected virtual float m_dash_Attack_LockDelay => 0.25f;

	protected virtual float m_dash_Attack_Speed => 22f;

	protected virtual float m_dash_RepeatAttack_Duration => 0.325f;

	protected virtual float m_dash_RepeatAttack_LockDelay => 0.25f;

	protected virtual float m_dash_RepeatAttack_Speed => 22f;

	protected virtual float m_dash_EndGravity_Delay => 0.15f;

	protected virtual int m_dash_NumDashes => 1;

	protected virtual int m_dash_OddstoThrowVersusDashing => 0;

	protected virtual float m_throw_Exit_ForceIdle => 0.4f;

	protected virtual float m_throw_AttackCD => 3.5f;

	protected virtual bool m_throw_Attack_TargetPlayer => true;

	protected virtual int m_throw_Attack_ProjectileAmount => 3;

	protected virtual float m_throw_Attack_ProjectileDelay => 0.1f;

	protected virtual bool m_throw_RepeatAttack_TargetPlayer => true;

	protected virtual int m_throw_RepeatAttack_ProjectileAmount => 1;

	protected virtual float m_throw_RepeatAttack_ProjectileDelay => 0.1f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[1] { "RogueKnightBounceBoltProjectile" };
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		Vector3 midpoint = base.EnemyController.Midpoint;
		midpoint.z = -3f;
		m_aimIndicator.transform.position = midpoint;
		m_aimIndicator.layer = 29;
		m_aimIndicator.SetActive(value: false);
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
		base.EnemyController.ControllerCorgi.GravityActive(state: false);
		base.EnemyController.LockFlip = false;
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		m_aimIndicator.SetActive(value: true);
		float desiredRotation = CDGHelper.AngleBetweenPts(base.EnemyController.Midpoint, base.EnemyController.TargetController.Midpoint);
		Vector3 localEulerAngles = m_aimIndicator.transform.localEulerAngles;
		localEulerAngles.z = desiredRotation;
		m_aimIndicator.transform.localEulerAngles = localEulerAngles;
		yield return Default_TellIntroAndLoop("Dash_Tell_Intro", m_dash_TellIntro_AnimationSpeed, "Dash_Tell_Hold", m_dash_TellHold_AnimationSpeed, m_dash_TellIntroAndHold_Delay - m_dash_Attack_LockDelay);
		localEulerAngles = m_aimIndicator.transform.localEulerAngles;
		localEulerAngles.z = desiredRotation;
		m_aimIndicator.transform.localEulerAngles = localEulerAngles;
		m_aimIndicator.SetActive(value: false);
		if (m_dash_Attack_LockDelay > 0f)
		{
			yield return Wait(m_dash_Attack_LockDelay);
		}
		yield return Default_Animation("Dash_Attack_Intro", m_dash_AttackIntro_AnimationSpeed, m_dash_AttackIntro_Delay);
		yield return Default_Animation("Dash_Attack_Hold", m_dash_AttackHold_AnimationSpeed, m_dash_AttackHold_Delay, waitUntilAnimationComplete: false);
		Vector2 vector = CDGHelper.AngleToVector(desiredRotation);
		if (m_raiseKnockbackDefenseWhileAttacking)
		{
			base.EnemyController.BaseKnockbackDefense = m_knockbackDefenseBoostOverride;
		}
		base.EnemyController.ControllerCorgi.DisableOneWayCollision = true;
		base.EnemyController.DisableFriction = true;
		base.EnemyController.FallLedge = true;
		base.EnemyController.SetVelocity(vector.x * m_dash_Attack_Speed, vector.y * m_dash_Attack_Speed, additive: false);
		if (m_dash_Attack_Duration > 0f)
		{
			yield return Wait(m_dash_Attack_Duration);
		}
		base.EnemyController.SetVelocity(0f, 0f, additive: false);
		base.EnemyController.ControllerCorgi.DisableOneWayCollision = false;
		base.EnemyController.DisableFriction = false;
		base.EnemyController.FallLedge = false;
		if (Random.Range(0, m_dash_OddstoThrowVersusDashing) == 0)
		{
			if (m_dash_NumDashes >= 2)
			{
				base.EnemyController.LockFlip = false;
				for (int i = 1; i < m_dash_NumDashes; i++)
				{
					yield return DashRepeat();
				}
			}
			if (m_dash_EndGravity_Delay > 0f)
			{
				yield return Wait(m_dash_EndGravity_Delay);
			}
			base.EnemyController.LockFlip = false;
			base.EnemyController.ControllerCorgi.GravityActive(state: true);
			yield return Default_Animation("Dash_Exit", m_dash_Exit_AnimationSpeed, m_dash_Exit_Delay);
		}
		else
		{
			yield return ThrowRepeat();
			if (m_dash_EndGravity_Delay > 0f)
			{
				yield return Wait(m_dash_EndGravity_Delay);
			}
			base.EnemyController.LockFlip = false;
			base.EnemyController.ControllerCorgi.GravityActive(state: true);
			yield return Default_Animation("Throw_Exit", m_throw_Exit_AnimationSpeed, m_throw_Exit_Delay);
		}
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_dash_Exit_ForceIdle, m_dash_AttackCD);
	}

	protected IEnumerator DashRepeat()
	{
		base.EnemyController.LockFlip = false;
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		m_aimIndicator.SetActive(value: true);
		float desiredRotation = CDGHelper.AngleBetweenPts(m_aimIndicator.transform.position, base.EnemyController.TargetController.Midpoint);
		Vector3 localEulerAngles = m_aimIndicator.transform.localEulerAngles;
		localEulerAngles.z = desiredRotation;
		m_aimIndicator.transform.localEulerAngles = localEulerAngles;
		yield return Default_TellIntroAndLoop("Dash_Tell_Intro", m_dash_TellIntro_AnimationSpeed, "Dash_Tell_Hold", m_dash_TellHold_AnimationSpeed, m_dash_RepeatTellIntroAndHold_Delay - m_dash_RepeatAttack_LockDelay);
		localEulerAngles = m_aimIndicator.transform.localEulerAngles;
		localEulerAngles.z = desiredRotation;
		m_aimIndicator.transform.localEulerAngles = localEulerAngles;
		m_aimIndicator.SetActive(value: false);
		if (m_dash_RepeatAttack_LockDelay > 0f)
		{
			yield return Wait(m_dash_RepeatAttack_LockDelay);
		}
		yield return Default_Animation("Dash_Attack_Intro", m_dash_AttackIntro_AnimationSpeed, m_dash_AttackIntro_Delay);
		yield return Default_Animation("Dash_Attack_Hold", m_dash_AttackHold_AnimationSpeed, m_dash_AttackHold_Delay, waitUntilAnimationComplete: false);
		if (m_raiseKnockbackDefenseWhileAttacking)
		{
			base.EnemyController.BaseKnockbackDefense = m_knockbackDefenseBoostOverride;
		}
		Vector2 vector = CDGHelper.AngleToVector(desiredRotation);
		base.EnemyController.SetVelocity(vector.x * m_dash_RepeatAttack_Speed, vector.y * m_dash_RepeatAttack_Speed, additive: false);
		if (m_dash_RepeatAttack_Duration > 0f)
		{
			yield return Wait(m_dash_RepeatAttack_Duration);
		}
		base.EnemyController.SetVelocity(0f, 0f, additive: false);
		base.EnemyController.LockFlip = false;
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[RestLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator ThrowAttack()
	{
		StopAndFaceTarget();
		if (!m_throw_Attack_TargetPlayer)
		{
			base.EnemyController.LockFlip = true;
		}
		yield return Default_TellIntroAndLoop("Throw_Tell_Intro", m_throw_TellIntro_AnimationSpeed, "Throw_Tell_Hold", m_throw_TellHold_AnimationSpeed, m_throw_TellIntroAndHold_Delay);
		yield return Default_Animation("Throw_Attack_Intro", m_throw_AttackIntro_AnimationSpeed, m_throw_AttackIntro_Delay);
		yield return Default_Animation("Throw_Attack_Hold", m_throw_AttackHold_AnimationSpeed, 0f, waitUntilAnimationComplete: false);
		for (int i = 0; i < m_throw_Attack_ProjectileAmount; i++)
		{
			if (m_throw_Attack_TargetPlayer)
			{
				float rads = Mathf.Atan2(base.EnemyController.TargetController.Midpoint.y - base.EnemyController.Midpoint.y, base.EnemyController.TargetController.Midpoint.x - base.EnemyController.Midpoint.x);
				rads = CDGHelper.WrapAngleDegrees(CDGHelper.ToDegrees(rads), convertToHalfRadius: false);
				this.FireProjectile("RogueKnightBounceBoltProjectile", 1, matchFacing: false, rads);
			}
			else
			{
				this.FireProjectile("RogueKnightBounceBoltProjectile", 1);
			}
			if (m_throw_Attack_ProjectileAmount > 1 && m_throw_Attack_ProjectileDelay > 0f)
			{
				yield return Wait(m_throw_Attack_ProjectileDelay);
			}
		}
		yield return Wait(m_throw_AttackHold_Delay);
		yield return Default_Animation("Throw_Exit", m_throw_Exit_AnimationSpeed, m_throw_Exit_Delay);
		base.EnemyController.LockFlip = false;
		yield return Default_Attack_Cooldown(m_throw_Exit_ForceIdle, m_throw_AttackCD);
	}

	protected IEnumerator ThrowRepeat()
	{
		StopAndFaceTarget();
		if (!m_throw_Attack_TargetPlayer)
		{
			base.EnemyController.LockFlip = true;
		}
		yield return Default_TellIntroAndLoop("Throw_Tell_Intro", m_throw_TellIntro_AnimationSpeed, "Throw_Tell_Hold", m_throw_TellHold_AnimationSpeed, m_throw_RepeatTellIntroAndHold_Delay);
		yield return Default_Animation("Throw_Attack_Intro", m_throw_AttackIntro_AnimationSpeed, m_throw_AttackIntro_Delay);
		yield return Default_Animation("Throw_Attack_Hold", m_throw_AttackHold_AnimationSpeed, 0f, waitUntilAnimationComplete: false);
		for (int i = 0; i < m_throw_RepeatAttack_ProjectileAmount; i++)
		{
			if (m_throw_RepeatAttack_TargetPlayer)
			{
				float rads = Mathf.Atan2(base.EnemyController.TargetController.Midpoint.y - base.EnemyController.Midpoint.y, base.EnemyController.TargetController.Midpoint.x - base.EnemyController.Midpoint.x);
				rads = CDGHelper.WrapAngleDegrees(CDGHelper.ToDegrees(rads), convertToHalfRadius: false);
				this.FireProjectile("RogueKnightBounceBoltProjectile", 1, matchFacing: false, rads);
			}
			else
			{
				this.FireProjectile("RogueKnightBounceBoltProjectile", 1);
			}
			if (m_throw_RepeatAttack_ProjectileAmount > 1 && m_throw_Attack_ProjectileDelay > 0f)
			{
				yield return Wait(m_throw_RepeatAttack_ProjectileDelay);
			}
		}
	}

	public override void OnLBCompleteOrCancelled()
	{
		base.OnLBCompleteOrCancelled();
		base.EnemyController.ControllerCorgi.DisableOneWayCollision = false;
		base.EnemyController.DisableFriction = false;
		m_aimIndicator.SetActive(value: false);
		base.EnemyController.ControllerCorgi.GravityActive(state: true);
		base.EnemyController.LockFlip = false;
	}
}
