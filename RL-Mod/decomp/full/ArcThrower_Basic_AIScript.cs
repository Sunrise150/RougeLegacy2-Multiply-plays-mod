using System.Collections;
using UnityEngine;

public class ArcThrower_Basic_AIScript : BaseAIScript
{
	protected float m_shoot_TellIntro_AnimationSpeed = 1f;

	protected float m_shoot_TellHold_AnimationSpeed = 1f;

	protected float m_shoot_TellIntroAndHold_Delay = 1.4f;

	protected float m_shoot_AttackIntro_AnimationSpeed = 1f;

	protected float m_shoot_AttackIntro_Delay;

	protected float m_shoot_AttackHold_AnimationSpeed = 1f;

	protected float m_shoot_AttackHold_Delay = 1f;

	protected float m_shoot_Exit_AnimationSpeed = 1f;

	protected float m_shoot_Exit_Delay;

	protected float m_shoot_Exit_ForceIdle = 0.15f;

	protected float m_shoot_AttackCD = 2.5f;

	protected float m_LowShotAngle = 22f;

	protected float m_MediumShotAngle = 60f;

	protected float m_HighShotAngle = 80f;

	protected float m_spray_TellIntro_AnimationSpeed = 1.1f;

	protected float m_spray_TellHold_AnimationSpeed = 1.1f;

	protected float m_spray_TellIntroAndHold_Delay = 0.675f;

	protected float m_spray_AttackIntro_AnimationSpeed = 1f;

	protected float m_spray_AttackIntro_Delay;

	protected float m_spray_AttackHold_AnimationSpeed = 1f;

	protected float m_spray_AttackHold_Delay;

	protected float m_spray_Exit_AnimationSpeed = 1f;

	protected float m_spray_Exit_Delay;

	protected float m_spray_Exit_ForceIdle = 0.15f;

	protected float m_spray_AttackCD;

	protected float m_spray_MediumAngle = 50f;

	protected float m_spray_HighAngle = 78f;

	protected float m_jump_Tell_AnimationSpeed = 1f;

	protected float m_jump_Tell_Delay = 0.55f;

	protected float m_jump_AttackIntro_AnimationSpeed = 1.25f;

	protected float m_jump_AttackIntro_Delay;

	protected float m_jump_AttackHold_AnimationSpeed = 1.25f;

	protected float m_jump_AttackHold_Delay;

	protected float m_jump_Exit_AnimationSpeed = 1.25f;

	protected float m_jump_Exit_Delay;

	protected float m_jump_Exit_ForceIdle;

	protected float m_jump_Exit_AttackCD = 10f;

	protected float m_throwBone_TellIntro_AnimationSpeed = 0.85f;

	protected float m_throwBone_TellHold_AnimationSpeed = 1.3f;

	protected const float m_throwBone_TellIntroAndHold_Delay = 1.2f;

	protected float m_throwBone_AttackIntro_AnimationSpeed = 0.8f;

	protected float m_throwBone_AttackIntro_Delay;

	protected float m_throwBone_AttackHold_AnimationSpeed = 0.8f;

	protected const float m_throwBone_AttackHold_Delay = 0.6f;

	protected float m_throwBone_Exit_AnimationSpeed = 0.65f;

	protected float m_throwBone_Exit_Delay = 0.15f;

	protected int m_BoneNear_Angle = 83;

	protected int m_FarBoneAngle = 70;

	protected float m_throwBone_Exit_ForceIdle = 0.15f;

	protected float m_throwBone_AttackCD;

	protected virtual float m_shoot_ProjectileDelay => 0.15f;

	protected virtual float m_shoot_ProjectileAmount => 12f;

	protected virtual float m_shoot_NumberMediumShots => 0f;

	protected virtual float m_shoot_NumberHighShots => 0f;

	protected virtual bool m_shoot_Explosive_Bullets => false;

	protected virtual float m_spray_ProjectileDelay => 0.15f;

	protected virtual float m_spray_ProjectileAmount => 5f;

	protected virtual float m_sprayProjectilesPerLoop => 1f;

	protected virtual float m_spray_LowAngle => 20f;

	protected virtual float m_spray_SpeedMod => 1f;

	protected virtual float m_spray_TiltSpeed => 0f;

	protected virtual Vector2 m_jumpPower => new Vector2(12f, 28f);

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[6] { "ArcThrowerCurseBoltProjectile", "ArcThrowerBoltProjectile", "ArcThrowerPotionProjectile", "ArcThrowerPotionExplosionProjectile", "ArcThrowerTimeBombProjectile", "ArcThrowerTimeBombExplosionProjectile" };
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
		StopAndFaceTarget();
		base.EnemyController.AlwaysFacing = false;
		base.Animator.SetBool("Shoot_Explosive_Bullets", m_shoot_Explosive_Bullets);
		yield return Default_TellIntroAndLoop("ChestBone_Tell_Intro", m_shoot_TellIntro_AnimationSpeed, "ChestBone_Tell_Hold", m_shoot_TellHold_AnimationSpeed, m_shoot_TellIntroAndHold_Delay);
		yield return Default_Animation("ChestBone_Attack_Intro", m_shoot_AttackIntro_AnimationSpeed, m_shoot_AttackIntro_Delay);
		yield return Default_Animation("ChestBone_Attack_Hold", m_shoot_AttackHold_AnimationSpeed, 0f, waitUntilAnimationComplete: false);
		if (m_shoot_Explosive_Bullets)
		{
			this.FireProjectile("ArcThrowerTimeBombProjectile", 0, matchFacing: true, m_LowShotAngle);
			this.FireProjectile("ArcThrowerTimeBombProjectile", 0, matchFacing: true, m_HighShotAngle);
		}
		else
		{
			this.FireProjectile("ArcThrowerCurseBoltProjectile", 0, matchFacing: true, m_LowShotAngle);
			if (base.LogicController.EnemyLogicType == EnemyLogicType.Expert)
			{
				this.FireProjectile("ArcThrowerCurseBoltProjectile", 0, matchFacing: true, m_HighShotAngle);
			}
		}
		if (m_shoot_AttackHold_Delay > 0f)
		{
			yield return Wait(m_shoot_AttackHold_Delay);
		}
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("ChestBone_Exit", m_shoot_Exit_AnimationSpeed, m_shoot_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		yield return Default_Attack_Cooldown(m_shoot_Exit_ForceIdle, m_shoot_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Spray()
	{
		StopAndFaceTarget();
		base.EnemyController.AlwaysFacing = false;
		yield return Default_TellIntroAndLoop("Fire_Tell_Intro", m_spray_TellIntro_AnimationSpeed, "Fire_Tell_Hold", m_spray_TellHold_AnimationSpeed, m_spray_TellIntroAndHold_Delay);
		yield return Default_Animation("Fire_Attack_Intro", m_spray_AttackIntro_AnimationSpeed, m_spray_AttackIntro_Delay);
		yield return Default_Animation("Fire_Attack_Hold", m_spray_AttackHold_AnimationSpeed, 0f, waitUntilAnimationComplete: false);
		for (int i = 0; (float)i < m_spray_ProjectileAmount; i++)
		{
			if (m_sprayProjectilesPerLoop >= 1f)
			{
				this.FireProjectile("ArcThrowerBoltProjectile", 1, matchFacing: true, m_spray_LowAngle + (float)i * m_spray_TiltSpeed, 1.1f * m_spray_SpeedMod);
			}
			if (m_sprayProjectilesPerLoop >= 2f)
			{
				this.FireProjectile("ArcThrowerBoltProjectile", 1, matchFacing: true, m_spray_MediumAngle + (float)i * m_spray_TiltSpeed, 0.7f * m_spray_SpeedMod);
			}
			if (m_sprayProjectilesPerLoop >= 3f)
			{
				this.FireProjectile("ArcThrowerBoltProjectile", 1, matchFacing: true, m_spray_HighAngle + (float)i * m_spray_TiltSpeed, 0.7f * m_spray_SpeedMod);
			}
			yield return Wait(m_spray_ProjectileDelay);
		}
		if (m_spray_AttackHold_Delay > 0f)
		{
			yield return Wait(m_spray_AttackHold_Delay);
		}
		base.EnemyController.AlwaysFacing = true;
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("Fire_Exit", m_spray_Exit_AnimationSpeed, m_spray_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		yield return Default_Attack_Cooldown(m_spray_Exit_ForceIdle, m_spray_AttackCD);
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
		StopAndFaceTarget();
		yield return Wait(0.1f);
		yield return Default_Animation("Jump_Tell", m_jump_Tell_AnimationSpeed, m_jump_Tell_Delay);
		SetAttackingWithContactDamage(enable: true);
		yield return this.ChangeAnimationState("JumpUp");
		if (base.EnemyController.IsFacingRight)
		{
			SetVelocityX(m_jumpPower.x, additive: false);
		}
		else
		{
			SetVelocityX(0f - m_jumpPower.x, additive: false);
		}
		SetVelocityY(m_jumpPower.y, additive: false);
		yield return Wait(0.05f);
		if (base.EnemyController.IsFacingRight)
		{
			base.EnemyController.JumpHorizontalVelocity = m_jumpPower.x;
		}
		else
		{
			base.EnemyController.JumpHorizontalVelocity = 0f - m_jumpPower.x;
		}
		yield return WaitUntilIsGrounded();
		SetVelocityX(0f, additive: false);
		SetAttackingWithContactDamage(enable: false);
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
	public IEnumerator Throw_Attack()
	{
		StopAndFaceTarget();
		base.EnemyController.AlwaysFacing = false;
		yield return Default_TellIntroAndLoop("ThrowBone_Tell_Intro", m_throwBone_TellIntro_AnimationSpeed, "ThrowBone_Tell_Hold", m_throwBone_TellHold_AnimationSpeed, 1.2f);
		yield return Default_Animation("ThrowBone_Attack_Intro", m_throwBone_AttackIntro_AnimationSpeed, m_throwBone_AttackIntro_Delay);
		this.FireProjectile("ArcThrowerPotionProjectile", 0, matchFacing: true, m_BoneNear_Angle);
		this.FireProjectile("ArcThrowerPotionProjectile", 0, matchFacing: true, m_FarBoneAngle);
		yield return Default_Animation("ThrowBone_Attack_Hold", m_throwBone_AttackHold_AnimationSpeed, 0f, waitUntilAnimationComplete: false);
		yield return Wait(0.6f);
		yield return WaitUntilIsGrounded();
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("ThrowBone_Exit", m_throwBone_Exit_AnimationSpeed, m_throwBone_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		base.EnemyController.AlwaysFacing = true;
		yield return Default_Attack_Cooldown(m_throwBone_Exit_ForceIdle, m_throwBone_AttackCD);
	}
}
