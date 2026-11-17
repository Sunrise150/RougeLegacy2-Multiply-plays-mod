using System.Collections;
using UnityEngine;

public class Starburst_Basic_AIScript : BaseAIScript
{
	protected float m_shoot_TellIntro_AnimationSpeed = 1f;

	protected float m_shoot_TellHold_AnimationSpeed = 2f;

	protected float m_shoot_TellHold_Delay = 0.5f;

	protected float m_shoot_AttackIntro_AnimationSpeed = 1f;

	protected float m_shoot_AttackIntro_Delay;

	protected float m_shoot_AttackHold_AnimationSpeed = 1f;

	protected float m_shoot_AttackHold_Delay;

	protected float m_shoot_Exit_AnimationSpeed = 1f;

	protected float m_shoot_Exit_Delay;

	protected float m_shoot_Exit_ForceIdle = 0.15f;

	protected float m_shoot_Exit_AttackCD;

	protected int AlternateCount;

	protected override Vector2 IdleDuration => new Vector2(0.3f, 0.7f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.45f, 0.8f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.45f, 0.8f);

	protected override Vector2 RandomFollowOffsetX => new Vector2(1f, 6f);

	protected override Vector2 RandomFollowOffsetY => new Vector2(2f, 11f);

	protected virtual int NumberOfShots => 4;

	protected virtual bool AlternateShots => false;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[6] { "StarburstPassBoltProjectile", "StarburstBoltProjectile", "StarburstVoidProjectile", "StarburstSlashBoltMinibossProjectile", "StarburstBoltMinibossProjectile", "StarburstBoltPassMinibossProjectile" };
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Shoot()
	{
		yield return Default_TellIntroAndLoop("SingleShot_Tell_Intro", m_shoot_TellIntro_AnimationSpeed, "SingleShot_Tell_Hold", m_shoot_TellHold_AnimationSpeed, m_shoot_TellHold_Delay);
		yield return Default_Animation("SingleShot_Attack_Intro", m_shoot_AttackIntro_AnimationSpeed, m_shoot_AttackIntro_Delay);
		yield return Default_Animation("SingleShot_Attack_Hold", m_shoot_AttackHold_AnimationSpeed, m_shoot_AttackHold_Delay, waitUntilAnimationComplete: false);
		float num = 360f / (float)NumberOfShots;
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
		for (int i = 0; i < NumberOfShots; i++)
		{
			if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
			{
				this.FireProjectile("StarburstBoltPassMinibossProjectile", 0, matchFacing: false, num * (float)i + num2);
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
		yield return Default_Animation("SingleShot_Exit", m_shoot_Exit_AnimationSpeed, m_shoot_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		yield return Default_Attack_Cooldown(m_shoot_Exit_ForceIdle, m_shoot_Exit_AttackCD);
	}

	private void FixedUpdate()
	{
		if (base.IsInitialized && base.EnemyController.Target != null)
		{
			UpdateEyeball();
		}
	}

	private void UpdateEyeball()
	{
		base.EnemyController.Animator.SetFloat("LookDirectionX", base.EnemyController.HeadingX);
		base.EnemyController.Animator.SetFloat("LookDirectionY", base.EnemyController.HeadingY);
	}
}
