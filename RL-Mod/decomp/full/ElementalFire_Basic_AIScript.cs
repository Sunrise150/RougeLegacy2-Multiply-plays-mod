using System.Collections;
using UnityEngine;

public class ElementalFire_Basic_AIScript : BaseAIScript
{
	[SerializeField]
	private int m_elementalType;

	protected const string ELEMENTAL_FIRE_TELL_INTRO = "Shoot_Tell_Intro";

	protected const string ELEMENTAL_FIRE_TELL_HOLD = "Shoot_Tell_Hold";

	protected const string ELEMENTAL_FIRE_ATTACK_INTRO = "Shoot_Attack_Intro";

	protected const string ELEMENTAL_FIRE_ATTACK_HOLD = "Shoot_Attack_Hold";

	protected const string ELEMENTAL_FIRE_EXIT = "Shoot_Exit";

	protected const string ELEMENTAL_FIRE_PROJECTILE = "ElementalFireBoltProjectile";

	protected const string SPINATTACK_TELL_INTRO = "Spin_Tell_Intro";

	protected const string SPINATTACK_TELL_HOLD = "Spin_Tell_Hold";

	protected const string SPINATTACK_ATTACK_INTRO = "Spin_Attack_Intro";

	protected const string SPINATTACK_ATTACK_HOLD = "Spin_Attack_Hold";

	protected const string SPINATTACK_EXIT = "Spin_Exit";

	protected const string SPINATTACK_PROJECTILE = "ElementalFireBoltLargePassProjectile";

	protected override Vector2 IdleDuration => new Vector2(0.25f, 0.7f);

	protected override Vector2 WalkTowardsDuration => new Vector2(1.25f, 2f);

	protected override Vector2 WalkAwayDuration => new Vector2(1.25f, 2f);

	protected override Vector2 RandomFollowOffsetX => new Vector2(3.5f, 10f);

	protected override Vector2 RandomFollowOffsetY => new Vector2(1f, 6f);

	protected virtual float m_shoot_TellIntro_AnimSpeed => 1f;

	protected virtual float m_shoot_TellHold_AnimSpeed => 1f;

	protected virtual float m_shoot_Tell_Delay => 1.1f;

	protected virtual float m_shoot_AttackIntro_AnimSpeed => 1f;

	protected virtual float m_shoot_AttackIntro_Delay => 0f;

	protected virtual float m_shoot_AttackHold_AnimSpeed => 1.25f;

	protected virtual float m_shoot_AttackHold_Delay => 0.4f;

	protected virtual float m_shoot_Exit_AnimSpeed => 1f;

	protected virtual float m_shoot_Exit_Delay => 0f;

	protected virtual float m_shoot_Exit_ForceIdle => 0.75f;

	protected virtual float m_shoot_Exit_AttackCD => 0f;

	protected virtual int m_shoot_TotalShots => 4;

	protected virtual float m_shoot_TotalShotDuration => 0.9f;

	protected virtual float m_shoot_RandAngleOffset => 18f;

	protected virtual float m_spinAttack_TellIntro_AnimSpeed => 1f;

	protected virtual float m_spinAttack_TellHold_AnimSpeed => 1f;

	protected virtual float m_spinAttack_Tell_Delay => 1.1f;

	protected virtual float m_spinAttack_AttackIntro_AnimSpeed => 1f;

	protected virtual float m_spinAttack_AttackIntro_Delay => 0f;

	protected virtual float m_spinAttack_AttackHold_AnimSpeed => 1.25f;

	protected virtual float m_spinAttack_AttackHold_PreDelay => 0.4f;

	protected virtual float m_spinAttack_AttackHold_Delay => 0.4f;

	protected virtual float m_spinAttack_Exit_AnimSpeed => 1f;

	protected virtual float m_spinAttack_Exit_Delay => 0f;

	protected virtual float m_spinAttack_Exit_ForceIdle => 0.75f;

	protected virtual float m_spinAttack_Exit_AttackCD => 5f;

	protected virtual float m_spinAttack_TimesShotDelay => 0.5f;

	protected virtual int m_spinAttack_TotalShots => 1;

	protected virtual int m_spinAttack_ShotPatternLoops => 1;

	protected virtual float m_spinAttack_projectile_RandomSpread => 0f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[2] { "ElementalFireBoltProjectile", "ElementalFireBoltLargePassProjectile" };
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		enemyController.Animator.SetInteger("ElementalType", m_elementalType);
		if (enemyController.EnemyRank == EnemyRank.Expert)
		{
			enemyController.Animator.SetBool("ThrowGiantBall", value: true);
		}
	}

	private void OnEnable()
	{
		if (base.IsInitialized)
		{
			base.EnemyController.Animator.SetInteger("ElementalType", m_elementalType);
		}
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
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		yield return Default_TellIntroAndLoop("Shoot_Tell_Intro", m_shoot_TellIntro_AnimSpeed, "Shoot_Tell_Hold", m_shoot_TellHold_AnimSpeed, m_shoot_Tell_Delay);
		yield return Default_Animation("Shoot_Attack_Intro", m_shoot_AttackIntro_AnimSpeed, m_shoot_AttackIntro_Delay);
		yield return Default_Animation("Shoot_Attack_Hold", m_shoot_AttackHold_AnimSpeed, 0f, waitUntilAnimationComplete: false);
		float shotInterval = m_shoot_TotalShotDuration / (float)m_shoot_TotalShots;
		for (int i = 0; i < m_shoot_TotalShots; i++)
		{
			float num = CDGHelper.VectorToAngle(base.EnemyController.TargetController.Midpoint - base.EnemyController.Midpoint);
			float num2 = Random.Range(0f - m_shoot_RandAngleOffset, m_shoot_RandAngleOffset);
			num += num2;
			this.FireProjectile("ElementalFireBoltProjectile", 0, matchFacing: false, num);
			if (shotInterval > 0f && i < m_shoot_TotalShots - 1)
			{
				yield return Wait(shotInterval);
			}
		}
		yield return Default_Animation("Shoot_Exit", m_shoot_Exit_AnimSpeed, m_shoot_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		yield return Default_Attack_Cooldown(m_shoot_Exit_ForceIdle, m_shoot_Exit_AttackCD);
		base.EnemyController.AlwaysFacing = true;
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator SpinAttack()
	{
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		bool alwaysFacing = true;
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Expert || base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
		{
			alwaysFacing = false;
		}
		base.EnemyController.AlwaysFacing = alwaysFacing;
		yield return Default_TellIntroAndLoop("Spin_Tell_Intro", m_spinAttack_TellIntro_AnimSpeed, "Spin_Tell_Hold", m_spinAttack_TellHold_AnimSpeed, m_spinAttack_Tell_Delay);
		yield return Default_Animation("Spin_Attack_Intro", m_spinAttack_AttackIntro_AnimSpeed, 0f, waitUntilAnimationComplete: false);
		yield return Default_Animation("Spin_Attack_Hold", m_spinAttack_AttackHold_AnimSpeed, 0f, waitUntilAnimationComplete: false);
		CDGHelper.VectorToAngle(base.EnemyController.TargetController.Midpoint - base.EnemyController.transform.position);
		for (int i = 0; i < m_spinAttack_ShotPatternLoops; i++)
		{
			float num = CDGHelper.VectorToAngle(base.EnemyController.TargetController.Midpoint - base.EnemyController.transform.position);
			for (int j = 0; j < m_spinAttack_TotalShots; j++)
			{
				float angle = num + Random.Range(0f - m_spinAttack_projectile_RandomSpread, m_spinAttack_projectile_RandomSpread);
				this.FireProjectile("ElementalFireBoltLargePassProjectile", 0, matchFacing: false, angle);
			}
			if (i < m_spinAttack_ShotPatternLoops - 1 && m_spinAttack_TimesShotDelay > 0f)
			{
				yield return Wait(m_spinAttack_TimesShotDelay);
			}
		}
		yield return Default_Animation("Spin_Exit", m_spinAttack_Exit_AnimSpeed, m_spinAttack_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		yield return Default_Attack_Cooldown(m_spinAttack_Exit_ForceIdle, m_spinAttack_Exit_AttackCD);
		base.EnemyController.AlwaysFacing = true;
	}
}
