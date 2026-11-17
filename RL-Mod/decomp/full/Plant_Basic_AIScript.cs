using System.Collections;
using UnityEngine;

public class Plant_Basic_AIScript : BaseAIScript
{
	protected float m_spore_TellIntro_AnimationSpeed = 0.85f;

	protected float m_spore_TellHold_AnimationSpeed = 1.3f;

	protected float m_spore_Tell_Delay = 1.15f;

	protected float m_spore_AttackIntro_AnimationSpeed = 1f;

	protected float m_spore_AttackIntro_Delay;

	protected float m_spore_AttackHold_AnimationSpeed = 1f;

	protected float m_spore_AttackHold_Delay;

	protected float m_spore_Exit_AnimationSpeed = 1f;

	protected float m_spore_Exit_Delay;

	protected const string SPAWN_PROJECTILE = "PlantSpawnSporeProjectile";

	protected const string FALL_PROJECTILE = "PlantFallSporeProjectile";

	protected float m_altAttack_TellIntro_AnimationSpeed = 0.85f;

	protected float m_altAttack_TellHold_AnimationSpeed = 1.3f;

	protected float m_altAttack_Tell_Delay = 1.15f;

	protected float m_altAttack_AttackIntro_AnimationSpeed = 1f;

	protected float m_altAttack_AttackIntro_Delay;

	protected float m_altAttack_AttackHold_AnimationSpeed = 1f;

	protected float m_altAttack_AttackHold_Delay;

	protected float m_altAttack_Exit_AnimationSpeed = 1f;

	protected float m_altAttack_Exit_Delay;

	protected const string HOMING_PROJECTILE = "PlantHomingSporeProjectile";

	protected const string BOLT_PROJECTILE = "PlantBoltProjectile";

	protected const string HOMING_PROJECTILE_EXPERT = "PlantHomingSporeProjectileExpertVariant";

	protected override Vector2 IdleDuration => new Vector2(0.25f, 0.7f);

	protected virtual Vector2 m_numSporesFired => new Vector2(4f, 7f);

	protected virtual Vector2 m_delayBetweenSporesFired => new Vector2(0f, 0f);

	protected virtual float m_spore_Exit_ForceIdle => 1.25f;

	protected virtual float m_spore_AttackCD => 0f;

	protected virtual float m_altAttack_Exit_ForceIdle => 1.25f;

	protected virtual float m_altAttack_AttackCD => 0f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[5] { "PlantSpawnSporeProjectile", "PlantFallSporeProjectile", "PlantHomingSporeProjectile", "PlantHomingSporeProjectileExpertVariant", "PlantBoltProjectile" };
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		base.EnemyController.SetVelocity(0f, 0f, additive: false);
		base.EnemyController.LockFlip = true;
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator ShootSpores()
	{
		yield return Default_TellIntroAndLoop("Shoot_Tell_Intro", m_spore_TellIntro_AnimationSpeed, "Shoot_Tell_Hold", m_spore_TellHold_AnimationSpeed, m_spore_Tell_Delay);
		yield return Default_Animation("Shoot_Attack_Intro", m_spore_AttackIntro_AnimationSpeed, m_spore_AttackIntro_Delay);
		yield return Default_Animation("Shoot_Attack_Hold", m_spore_AttackHold_AnimationSpeed, m_spore_AttackHold_Delay, waitUntilAnimationComplete: false);
		Random.Range((int)m_numSporesFired.x, (int)m_numSporesFired.y + 1);
		this.FireProjectile("PlantSpawnSporeProjectile", 0, matchFacing: false, 120f);
		this.FireProjectile("PlantSpawnSporeProjectile", 0, matchFacing: false, 90f);
		this.FireProjectile("PlantSpawnSporeProjectile", 0, matchFacing: false, 60f);
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Expert)
		{
			this.FireProjectile("PlantSpawnSporeProjectile", 0, matchFacing: false, 30f);
			this.FireProjectile("PlantSpawnSporeProjectile", 0, matchFacing: false, 150f);
		}
		yield return Default_Animation("Shoot_Exit", m_spore_Exit_AnimationSpeed, m_spore_Exit_Delay);
		yield return Default_Attack_Cooldown(m_spore_Exit_ForceIdle, m_spore_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator AltAttack()
	{
		yield return Default_TellIntroAndLoop("Shoot_Tell_Intro", m_altAttack_TellIntro_AnimationSpeed, "Shoot_Tell_Hold", m_altAttack_TellHold_AnimationSpeed, m_altAttack_Tell_Delay);
		yield return Default_Animation("Shoot_Attack_Intro", m_altAttack_AttackIntro_AnimationSpeed, m_altAttack_AttackIntro_Delay);
		yield return Default_Animation("Shoot_Attack_Hold", m_altAttack_AttackHold_AnimationSpeed, m_altAttack_AttackHold_Delay, waitUntilAnimationComplete: false);
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Expert)
		{
			this.FireProjectile("PlantHomingSporeProjectileExpertVariant", 0, matchFacing: false, 90f);
		}
		else
		{
			this.FireProjectile("PlantHomingSporeProjectile", 0, matchFacing: false, 90f);
		}
		yield return Default_Animation("Shoot_Exit", m_altAttack_Exit_AnimationSpeed, m_spore_Exit_Delay);
		yield return Default_Attack_Cooldown(m_altAttack_Exit_ForceIdle, m_spore_AttackCD);
	}
}
