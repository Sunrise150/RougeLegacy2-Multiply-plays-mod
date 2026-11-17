using System.Collections;
using UnityEngine;

public class ElementalIce_Basic_AIScript : BaseAIScript
{
	[SerializeField]
	private int m_elementalType;

	protected const string ICEBALL_TELL_INTRO = "Shoot_Tell_Intro";

	protected const string ICEBALL_TELL_HOLD = "Shoot_Tell_Hold";

	protected const string ICEBALL_ATTACK_INTRO = "Shoot_Attack_Intro";

	protected const string ICEBALL_ATTACK_HOLD = "Shoot_Attack_Hold";

	protected const string ICEBALL_EXIT = "Shoot_Exit";

	protected const string ICEBALL_PROJECTILE = "ElementalIceBoltProjectile";

	protected const string SPINATTACK_TELL_INTRO = "Spin_Tell_Intro";

	protected const string SPINATTACK_TELL_HOLD = "Spin_Tell_Hold";

	protected const string SPINATTACK_ATTACK_INTRO = "Spin_Attack_Intro";

	protected const string SPINATTACK_ATTACK_HOLD = "Spin_Attack_Hold";

	protected const string SPINATTACK_EXIT = "Spin_Exit";

	protected const string SPINATTACK_PROJECTILE = "ElementalIceBoltProjectile";

	protected override Vector2 IdleDuration => new Vector2(0.25f, 0.7f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.45f, 0.85f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.45f, 0.85f);

	protected override Vector2 RandomFollowOffsetX => new Vector2(-5f, 5f);

	protected override Vector2 RandomFollowOffsetY => new Vector2(2f, 5f);

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

	protected virtual bool m_shoot_CentreShot => false;

	protected virtual float m_shoot_TimesShotDelay => 0.5f;

	protected virtual int m_shoot_TotalSideShots => 1;

	protected virtual int m_shoot_ShotPatternLoops => 1;

	protected virtual float m_shoot_Projectile_Spread => 30f;

	protected virtual float m_spinAttack_TellIntro_AnimSpeed => 1f;

	protected virtual float m_spinAttack_TellHold_AnimSpeed => 1f;

	protected virtual float m_spinAttack_Tell_Delay => 1.1f;

	protected virtual float m_spinAttack_AttackIntro_AnimSpeed => 1f;

	protected virtual float m_spinAttack_AttackIntro_Delay => 0f;

	protected virtual float m_spinAttack_AttackHold_AnimSpeed => 1.25f;

	protected virtual float m_spinAttack_AttackHold_Delay => 0.4f;

	protected virtual float m_spinAttack_Exit_AnimSpeed => 1f;

	protected virtual float m_spinAttack_Exit_Delay => 0f;

	protected virtual float m_spinAttack_Exit_ForceIdle => 0.75f;

	protected virtual float m_spinAttack_Exit_AttackCD => 10f;

	protected virtual float m_spinAttack_TimesShotDelay => 0.15f;

	protected virtual int m_spinAttack_TotalShots => 1;

	protected virtual int m_spinAttack_ShotPatternLoops => 5;

	protected virtual float m_spinAttack_projectile_RandomSpread => 180f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[2] { "ElementalIceBoltProjectile", "ElementalIceBoltExplosionProjectile" };
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		enemyController.Animator.SetInteger("ElementalType", m_elementalType);
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
	public IEnumerator ShootIceball()
	{
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		bool alwaysFacing = true;
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Expert || base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
		{
			alwaysFacing = false;
		}
		base.EnemyController.AlwaysFacing = alwaysFacing;
		yield return Default_TellIntroAndLoop("Shoot_Tell_Intro", m_shoot_TellIntro_AnimSpeed, "Shoot_Tell_Hold", m_shoot_TellHold_AnimSpeed, m_shoot_Tell_Delay);
		yield return Default_Animation("Shoot_Attack_Intro", m_shoot_AttackIntro_AnimSpeed, m_shoot_AttackIntro_Delay);
		yield return Default_Animation("Shoot_Attack_Hold", m_shoot_AttackHold_AnimSpeed, 0f, waitUntilAnimationComplete: false);
		Vector2 pt = base.EnemyController.TargetController.Midpoint - base.EnemyController.transform.position;
		float initialFireAngle = CDGHelper.VectorToAngle(pt);
		float angleDelta = m_shoot_Projectile_Spread;
		if (m_shoot_CentreShot)
		{
			this.FireProjectile("ElementalIceBoltProjectile", 0, matchFacing: false, initialFireAngle);
		}
		for (int i = 0; i < m_shoot_ShotPatternLoops; i++)
		{
			for (int j = 1; j < m_shoot_TotalSideShots + 1; j++)
			{
				float angle = initialFireAngle + angleDelta * (float)j;
				this.FireProjectile("ElementalIceBoltProjectile", 0, matchFacing: false, angle);
				angle = initialFireAngle - angleDelta * (float)j;
				this.FireProjectile("ElementalIceBoltProjectile", 0, matchFacing: false, angle);
			}
			if (i < m_shoot_ShotPatternLoops - 1 && m_shoot_TimesShotDelay > 0f)
			{
				yield return Wait(m_shoot_TimesShotDelay);
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
		yield return Default_Animation("Spin_Attack_Intro", m_spinAttack_AttackIntro_AnimSpeed, m_spinAttack_AttackIntro_Delay);
		yield return Default_Animation("Spin_Attack_Hold", m_spinAttack_AttackHold_AnimSpeed, 0f, waitUntilAnimationComplete: false);
		Vector2 pt = base.EnemyController.TargetController.Midpoint - base.EnemyController.transform.position;
		float initialFireAngle = CDGHelper.VectorToAngle(pt);
		if (m_spinAttack_TotalShots > 0)
		{
			_ = 360 / m_spinAttack_TotalShots;
		}
		for (int i = 0; i < m_spinAttack_ShotPatternLoops; i++)
		{
			for (int j = 0; j < m_spinAttack_TotalShots; j++)
			{
				float angle = initialFireAngle + Random.Range(0f - m_spinAttack_projectile_RandomSpread, m_spinAttack_projectile_RandomSpread);
				this.FireProjectile("ElementalIceBoltProjectile", 0, matchFacing: false, angle);
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
