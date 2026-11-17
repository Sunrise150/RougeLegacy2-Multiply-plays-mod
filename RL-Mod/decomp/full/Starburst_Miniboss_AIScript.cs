using System.Collections;

public class Starburst_Miniboss_AIScript : Starburst_Basic_AIScript
{
	protected const string DASH_TELL_INTRO = "Spin_Tell_Intro";

	protected const string DASH_TELL_HOLD = "Spin_Tell_Hold";

	protected const string DASH_ATTACK_INTRO = "Spin_Attack_Intro";

	protected const string DASH_ATTACK_HOLD = "Spin_Attack_Hold";

	protected const string DASH_EXIT = "Spin_Exit";

	protected override int NumberOfShots => 8;

	protected override bool AlternateShots => true;

	protected virtual int NumberOfCurse => 2;

	protected virtual float m_dash_TellIntro_AnimSpeed => 1f;

	protected virtual float m_dash_TellIntro_Delay => 0f;

	protected virtual float m_dash_TellHold_AnimSpeed => 1f;

	protected virtual float m_dash_TellHold_Delay => 1.25f;

	protected virtual float m_dash_AttackIntro_AnimSpeed => 1f;

	protected virtual float m_dash_AttackIntro_Delay => 0f;

	protected virtual float m_dash_AttackHold_AnimSpeed => 1f;

	protected virtual float m_dash_AttackHold_Delay => 0f;

	protected virtual float m_dash_Exit_AnimSpeed => 1f;

	protected virtual float m_dash_Exit_Delay => 0f;

	protected virtual float m_dash_Exit_ForceIdle => 0f;

	protected virtual float m_dash_Exit_AttackCD => 5.5f;

	protected virtual float m_dash_Attack_ForwardSpeedMod => 4.5f;

	protected virtual float m_dash_Attack_Duration => 0.75f;

	protected virtual bool m_dropsFireballsDuringDashAttack => false;

	protected virtual float m_fireballDropDuringDashInterval => 0.05f;

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[MinibossEnemy]
	public IEnumerator ShootBoss()
	{
		yield return Default_TellIntroAndLoop("Spin_Tell_Intro", m_shoot_TellIntro_AnimationSpeed, "Spin_Tell_Hold", m_shoot_TellHold_AnimationSpeed, m_shoot_TellHold_Delay);
		yield return Default_Animation("Spin_Attack_Intro", m_shoot_AttackIntro_AnimationSpeed, m_shoot_AttackIntro_Delay);
		yield return Default_Animation("Spin_Attack_Hold", m_shoot_AttackHold_AnimationSpeed, m_shoot_AttackHold_Delay, waitUntilAnimationComplete: false);
		float num = 360f / (float)NumberOfCurse;
		float num2 = 0f;
		if (AlternateShots && AlternateCount == 1)
		{
			num2 = num + num / 2f;
			AlternateCount = 0;
		}
		else if (AlternateShots && AlternateCount == 0)
		{
			AlternateCount = 1;
		}
		for (int i = 0; i < NumberOfCurse; i++)
		{
			if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
			{
				this.FireProjectile("StarburstSlashBoltMinibossProjectile", 0, matchFacing: false, num * (float)i + num2);
			}
			else if (base.LogicController.EnemyLogicType == EnemyLogicType.Expert)
			{
				this.FireProjectile("StarburstVoidProjectile", 0, matchFacing: false, num * (float)i + num2);
			}
			else
			{
				this.FireProjectile("StarburstBoltProjectile", 0, matchFacing: false, num * (float)i + num2);
			}
		}
		if (m_shoot_AttackHold_Delay > 0f)
		{
			Wait(m_shoot_AttackHold_Delay);
		}
		yield return Default_Animation("Spin_Exit", m_shoot_Exit_AnimationSpeed, m_shoot_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		yield return Default_Attack_Cooldown(m_shoot_Exit_ForceIdle, m_shoot_Exit_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[MinibossEnemy]
	public IEnumerator DashAttack()
	{
		StopAndFaceTarget();
		yield return Default_Animation("Spin_Tell_Intro", m_dash_TellIntro_AnimSpeed, m_dash_TellIntro_Delay, waitUntilAnimationComplete: false);
		yield return Default_Animation("Spin_Tell_Hold", m_dash_TellHold_AnimSpeed, m_dash_TellHold_Delay, waitUntilAnimationComplete: false);
		base.EnemyController.Heading = base.EnemyController.TargetController.Midpoint - base.EnemyController.Midpoint;
		base.EnemyController.LockFlip = true;
		base.EnemyController.FollowTarget = false;
		yield return Default_Animation("Spin_Attack_Intro", m_dash_AttackIntro_AnimSpeed, m_dash_AttackIntro_Delay);
		yield return Default_Animation("Spin_Attack_Hold", m_dash_AttackHold_AnimSpeed, m_dash_AttackHold_Delay, waitUntilAnimationComplete: false);
		base.EnemyController.FlyingMovementType = FlyingMovementType.Towards;
		base.EnemyController.BaseSpeed = base.EnemyController.BaseSpeed * m_dash_Attack_ForwardSpeedMod;
		if (m_dash_Attack_Duration > 0f)
		{
			yield return Wait(m_dash_Attack_Duration);
		}
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		this.FireProjectile("StarburstSlashBoltMinibossProjectile", 0, matchFacing: false);
		this.FireProjectile("StarburstSlashBoltMinibossProjectile", 0, matchFacing: false, 180f);
		yield return Default_Animation("Spin_Exit", m_dash_Exit_AnimSpeed, m_dash_Exit_Delay);
		yield return Default_Attack_Cooldown(m_dash_Exit_ForceIdle, m_dash_Exit_AttackCD);
		base.EnemyController.ResetTurnTrigger();
		base.EnemyController.ResetBaseValues();
		base.EnemyController.LockFlip = false;
	}
}
