using System.Collections;
using UnityEngine;

public class FlyingBurst_Basic_AIScript : BaseAIScript
{
	protected float m_shoot_TellIntro_AnimationSpeed = 1f;

	protected float m_shoot_TellHold_AnimationSpeed = 2f;

	protected float m_shoot_Tell_Delay = 1.15f;

	protected float m_shoot_AttackIntro_AnimationSpeed = 1f;

	protected float m_shoot_AttackIntro_Delay;

	protected float m_shoot_AttackHold_AnimationSpeed = 1.25f;

	protected float m_shoot_AttackHold_Delay = 0.4f;

	protected float m_shoot_Exit_AnimationSpeed = 1f;

	protected float m_shoot_Exit_Delay;

	protected float m_shoot_Exit_ForceIdle = 0.15f;

	protected float m_shoot_Exit_AttackCD;

	protected float m_bigShake_TellIntro_AnimationSpeed = 1f;

	protected float m_bigShake_TellHold_AnimationSpeed = 2f;

	protected float m_bigShake_Tell_Delay = 1.15f;

	protected float m_bigShake_AttackIntro_AnimationSpeed = 1f;

	protected float m_bigShake_AttackIntro_Delay;

	protected float m_bigShake_AttackHold_AnimationSpeed = 1.25f;

	protected float m_bigShake_AttackHold_Delay = 0.4f;

	protected float m_bigShake_Exit_AnimationSpeed = 1f;

	protected float m_bigShake_Exit_Delay;

	protected float m_bigShake_Exit_ForceIdle = 0.15f;

	protected float m_bigShake_Exit_AttackCD;

	protected override Vector2 IdleDuration => new Vector2(0.3f, 0.7f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.55f, 1.1f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.55f, 1.1f);

	protected virtual int ShootPatternTotalLoops => 1;

	protected virtual float ShootPatternTotalLoopsDelay => 0f;

	protected virtual int NumFireballsMelee => 3;

	protected virtual int NumFireballsBasic => 0;

	protected virtual float MeleeFireSpread => 120f;

	protected virtual float BasicFireSpread => 120f;

	protected virtual Vector2 FireballIntervalDelay => new Vector2(0.3f, 0.3f);

	protected virtual Vector2 FireballRandSpeedMod => new Vector2(1f, 1f);

	protected virtual float FireballDelayBetweenBasicAndMelee => 0.1f;

	protected virtual int m_bigShake_NumFireballsBasic => 1;

	protected virtual float m_bigShake_BasicFireSpread => 50f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[6] { "FlyingBurstBounceBoltMinibossProjectile", "FlyingBurstSlashBoltProjectile", "FlyingBurstSuperBounceBoltMinibossProjectile", "FlyingBurstBlueBoltProjectile", "FlyingBurstBlueBoltMinibossProjectile", "FlyingBurstSlashBoltMinibossProjectile" };
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator ShootFireball()
	{
		if (base.LogicController.EnemyLogicType != EnemyLogicType.Miniboss)
		{
			StopAndFaceTarget();
		}
		base.EnemyController.AlwaysFacing = false;
		yield return Default_TellIntroAndLoop("Shake_Tell_Intro", m_shoot_TellIntro_AnimationSpeed, "Shake_Tell_Hold", m_shoot_TellHold_AnimationSpeed, m_shoot_Tell_Delay);
		yield return Default_Animation("Shake_Attack_Intro", m_shoot_AttackIntro_AnimationSpeed, m_shoot_AttackIntro_Delay);
		yield return Default_Animation("Shake_Attack_Hold", m_shoot_AttackHold_AnimationSpeed, 0f, waitUntilAnimationComplete: false);
		for (int i = 0; i < ShootPatternTotalLoops; i++)
		{
			for (int k = 0; k < NumFireballsMelee; k++)
			{
				float num = CDGHelper.WrapAngleDegrees(CDGHelper.ToDegrees(Mathf.Atan2(base.EnemyController.TargetController.Midpoint.y - base.EnemyController.Midpoint.y, base.EnemyController.TargetController.Midpoint.x - base.EnemyController.Midpoint.x)), convertToHalfRadius: false) - MeleeFireSpread / 2f;
				float speedMod = Random.Range(FireballRandSpeedMod.x, FireballRandSpeedMod.y);
				float angle = Random.Range(num, num + MeleeFireSpread);
				if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
				{
					this.FireProjectile("FlyingBurstBounceBoltMinibossProjectile", 1, matchFacing: false, angle, speedMod);
				}
				else
				{
					this.FireProjectile("FlyingBurstSlashBoltProjectile", 1, matchFacing: false, angle, speedMod);
				}
				float num2 = Random.Range(FireballIntervalDelay.x, FireballIntervalDelay.y);
				if (num2 > 0f)
				{
					yield return Wait(num2);
				}
			}
			if (FireballDelayBetweenBasicAndMelee > 0f)
			{
				yield return Wait(FireballDelayBetweenBasicAndMelee);
			}
			for (int k = 0; k < NumFireballsBasic; k++)
			{
				float num3 = CDGHelper.WrapAngleDegrees(CDGHelper.ToDegrees(Mathf.Atan2(base.EnemyController.TargetController.Midpoint.y - base.EnemyController.Midpoint.y, base.EnemyController.TargetController.Midpoint.x - base.EnemyController.Midpoint.x)), convertToHalfRadius: false) - BasicFireSpread / 2f;
				float speedMod2 = Random.Range(FireballRandSpeedMod.x, FireballRandSpeedMod.y);
				float angle2 = Random.Range(num3, num3 + BasicFireSpread);
				if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
				{
					this.FireProjectile("FlyingBurstSuperBounceBoltMinibossProjectile", 1, matchFacing: false, angle2, speedMod2);
				}
				else
				{
					this.FireProjectile("FlyingBurstBlueBoltProjectile", 1, matchFacing: false, angle2, speedMod2);
				}
				float num4 = Random.Range(FireballIntervalDelay.x, FireballIntervalDelay.y);
				if (num4 > 0f)
				{
					yield return Wait(num4);
				}
			}
			if (ShootPatternTotalLoopsDelay > 0f)
			{
				yield return Wait(ShootPatternTotalLoopsDelay);
			}
		}
		if (m_shoot_AttackHold_Delay > 0f)
		{
			yield return Wait(m_shoot_AttackHold_Delay);
		}
		yield return Default_Animation("Shake_Exit", m_shoot_Exit_AnimationSpeed, m_shoot_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		base.EnemyController.AlwaysFacing = true;
		yield return Default_Attack_Cooldown(m_shoot_Exit_ForceIdle, m_shoot_Exit_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator BigShakeAttack()
	{
		StopAndFaceTarget();
		yield return Default_TellIntroAndLoop("BigShake_Tell_Intro", m_bigShake_TellIntro_AnimationSpeed, "BigShake_Tell_Hold", m_bigShake_TellHold_AnimationSpeed, m_bigShake_Tell_Delay);
		yield return Default_Animation("BigShake_Attack_Intro", m_bigShake_AttackIntro_AnimationSpeed, m_bigShake_AttackIntro_Delay);
		yield return Default_Animation("BigShake_Attack_Hold", m_bigShake_AttackHold_AnimationSpeed, 0f, waitUntilAnimationComplete: false);
		float rads = Mathf.Atan2(base.EnemyController.TargetController.Midpoint.y - base.EnemyController.Midpoint.y, base.EnemyController.TargetController.Midpoint.x - base.EnemyController.Midpoint.x);
		rads = CDGHelper.WrapAngleDegrees(CDGHelper.ToDegrees(rads), convertToHalfRadius: false);
		this.FireProjectile("FlyingBurstBlueBoltProjectile", 1, matchFacing: false, rads);
		this.FireProjectile("FlyingBurstBlueBoltProjectile", 1, matchFacing: false, rads + 10f);
		this.FireProjectile("FlyingBurstBlueBoltProjectile", 1, matchFacing: false, rads - 10f);
		for (int i = 0; i < m_bigShake_NumFireballsBasic; i++)
		{
			this.FireProjectile("FlyingBurstBlueBoltProjectile", 1, matchFacing: false, rads + m_bigShake_BasicFireSpread * (float)(i + 1));
			this.FireProjectile("FlyingBurstBlueBoltProjectile", 1, matchFacing: false, rads - m_bigShake_BasicFireSpread * (float)(i + 1));
		}
		if (m_bigShake_AttackHold_Delay > 0f)
		{
			yield return Wait(m_bigShake_AttackHold_Delay);
		}
		yield return Default_Animation("BigShake_Exit", m_bigShake_Exit_AnimationSpeed, m_bigShake_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		base.EnemyController.AlwaysFacing = true;
		yield return Default_Attack_Cooldown(m_bigShake_Exit_ForceIdle, m_bigShake_Exit_AttackCD);
	}
}
