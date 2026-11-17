using System.Collections;
using UnityEngine;

public class FlyingShield_Basic_AIScript : BaseAIScript
{
	[SerializeField]
	private GameObject m_aimSprite;

	private float m_fireball_TailCounter;

	private bool m_enableTail;

	private bool m_isRamAttack;

	private bool m_aim;

	private BounceCollision m_bounceLogic;

	protected Vector3 m_ramMove_SetRamPosition = new Vector3(-1f, 0f, 0f);

	private float m_spinAttackTurnSpeedOverride = 175f;

	protected override Vector2 IdleDuration => new Vector2(0.7f, 1f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.8f, 1.1f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.8f, 1.1f);

	protected virtual float m_spinMove_TellIntro_AnimationSpeed => 0.85f;

	protected virtual float m_spinMove_TellIntro_EndDelay => 0.15f;

	protected virtual float m_spinMove_TellHold_AnimationSpeed => 1f;

	protected virtual float m_spinMove_TellHold_Duration => 0.425f;

	protected virtual float m_spinMove_AttackIntro_AnimationSpeed => 0.85f;

	protected virtual float m_spinMove_AttackIntro_EndDelay => 0f;

	protected virtual float m_spinMove_Attack_FlameDrops => 0f;

	protected virtual float m_spinMove_Attack_Duration => 3.25f;

	protected virtual float m_spinMove_Attack_MoveSpeed => 22f;

	protected virtual float m_spinMove_ExitIntro_AnimationSpeed => 0.85f;

	protected virtual float m_spinMove_ExitIntro_EndDelay => 0.25f;

	protected virtual bool m_hasTailSpin => false;

	protected virtual bool m_hasTailRam => false;

	protected virtual float m_fireball_TailSpawnDelay => 0.05f;

	protected virtual float m_fireball_TailRamSpawnDelay => 0.015f;

	protected virtual float m_spinMove_Exit_ForceIdle => 0.5f;

	protected virtual float m_spinMove_Exit_AttackCD => 0f;

	protected virtual bool m_raiseKnockbackDefenseWhileAttacking => true;

	protected virtual int m_knockbackDefenseBoostOverride => 5;

	protected virtual float m_ramMove_ChaseDuration => 1.25f;

	protected virtual float m_ramMove_TellIntro_AnimationSpeed => 0.9f;

	protected virtual float m_ramMove_TellHold_AnimationSpeed => 1f;

	protected virtual float m_ramMove_TellIntroAndHold_Delay => 0.65f;

	protected virtual float m_ramMove_AttackIntro_AnimationSpeed => 1f;

	protected virtual float m_ramMove_AttackIntro_Delay => 0f;

	protected virtual float m_ramMove_AttackHold_AnimationSpeed => 1f;

	protected virtual float m_ramMove_AttackHold_Duration => 0f;

	protected virtual float m_ramMove_Attack_Duration => 0.3f;

	protected virtual float m_ramMove_Attack_Speed => 41f;

	protected virtual float m_ramMove_ExitIntro_AnimationSpeed => 1f;

	protected virtual float m_ramMove_ExitIntro_EndDelay => 0.35f;

	protected virtual float m_ramMove_ExitIntro_ForceIdle => 0.25f;

	protected virtual float m_ramMove_Attack_CD => 6f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[3] { "FlyingShieldWaterProjectile", "FlyingShieldWaterExpertProjectile", "FlyingShieldWaterMinibossProjectile" };
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		base.EnemyController.DisableFriction = true;
		m_fireball_TailCounter = m_fireball_TailSpawnDelay;
		m_bounceLogic = base.EnemyController.GetComponent<BounceCollision>();
		m_bounceLogic.enabled = false;
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Ram_Attack()
	{
		base.EnemyController.FlyingMovementType = FlyingMovementType.Towards;
		base.EnemyController.GenerateRandomFollowOffset(new Vector2(1f, 1f), Vector2.zero);
		if (m_ramMove_ChaseDuration > 0f)
		{
			yield return Wait(m_ramMove_ChaseDuration);
		}
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		yield return Default_TellIntroAndLoop("Ram_Tell_Intro", m_ramMove_TellIntro_AnimationSpeed, "Ram_Tell_Hold", m_ramMove_TellHold_AnimationSpeed, m_ramMove_TellIntroAndHold_Delay);
		SetAttackingWithContactDamage(enable: true);
		base.EnemyController.FlyingMovementType = FlyingMovementType.Override;
		base.EnemyController.AlwaysFacing = false;
		yield return Default_Animation("Ram_Attack_Intro", m_ramMove_AttackIntro_AnimationSpeed, m_ramMove_AttackIntro_Delay);
		yield return Default_Animation("Ram_Attack_Hold", m_ramMove_AttackHold_AnimationSpeed, m_ramMove_AttackHold_Duration, waitUntilAnimationComplete: false);
		m_enableTail = m_hasTailRam;
		m_isRamAttack = true;
		if (base.EnemyController.IsFacingRight)
		{
			SetVelocityX(m_ramMove_Attack_Speed, additive: false);
		}
		else
		{
			SetVelocityX(0f - m_ramMove_Attack_Speed, additive: false);
		}
		if (m_ramMove_Attack_Duration > 0f)
		{
			yield return Wait(m_ramMove_Attack_Duration);
		}
		base.EnemyController.ResetBaseValues();
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		m_enableTail = false;
		m_isRamAttack = false;
		yield return Default_Animation("Ram_Exit", m_ramMove_ExitIntro_AnimationSpeed, m_ramMove_ExitIntro_EndDelay);
		SetAttackingWithContactDamage(enable: false);
		base.EnemyController.AlwaysFacing = true;
		yield return Default_Attack_Cooldown(m_ramMove_ExitIntro_ForceIdle, m_ramMove_Attack_CD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Spin_Attack()
	{
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		base.EnemyController.FollowTarget = true;
		base.EnemyController.FollowOffset = Vector3.zero;
		base.EnemyController.ForceFaceTarget();
		base.EnemyController.BaseTurnSpeed = m_spinAttackTurnSpeedOverride;
		m_aim = true;
		yield return Default_Animation("Spin_Tell_Intro", m_spinMove_TellIntro_AnimationSpeed, m_spinMove_TellIntro_EndDelay);
		SetAttackingWithContactDamage(enable: true);
		yield return Default_Animation("Spin_Tell_Hold", m_spinMove_TellIntro_AnimationSpeed, 0f);
		yield return Default_Animation("Spin_Attack_Intro", m_spinMove_TellIntro_AnimationSpeed, 0f);
		yield return Default_Animation("Spin_Attack_Hold", m_spinMove_TellHold_AnimationSpeed, m_spinMove_TellHold_Duration);
		base.EnemyController.FollowTarget = false;
		if (m_raiseKnockbackDefenseWhileAttacking)
		{
			base.EnemyController.BaseKnockbackDefense = m_knockbackDefenseBoostOverride;
		}
		base.EnemyController.FlyingMovementType = FlyingMovementType.Override;
		SetVelocity(base.EnemyController.HeadingX * m_spinMove_Attack_MoveSpeed, base.EnemyController.HeadingY * m_spinMove_Attack_MoveSpeed, additive: false);
		m_bounceLogic.enabled = true;
		m_aim = false;
		base.EnemyController.HitboxController.CollisionType = CollisionType.EnemyProjectile;
		m_enableTail = m_hasTailSpin;
		if (m_spinMove_Attack_FlameDrops > 0f)
		{
			for (int i = 0; (float)i < m_spinMove_Attack_FlameDrops; i++)
			{
				if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
				{
					this.FireProjectile("FlyingShieldWaterMinibossProjectile", 0, matchFacing: false, 0f, 0f);
				}
				else
				{
					this.FireProjectile("FlyingShieldWaterProjectile", 0, matchFacing: false, 0f, 0f);
				}
				if (m_spinMove_Attack_Duration / m_spinMove_Attack_FlameDrops > 0f)
				{
					yield return Wait(m_spinMove_Attack_Duration / m_spinMove_Attack_FlameDrops);
				}
			}
		}
		else if (m_spinMove_Attack_Duration > 0f)
		{
			yield return Wait(m_spinMove_Attack_Duration);
		}
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		m_bounceLogic.enabled = false;
		base.EnemyController.HitboxController.CollisionType = CollisionType.Enemy;
		SetAttackingWithContactDamage(enable: false);
		yield return this.ChangeAnimationState("Spin_Exit");
		yield return WaitUntilAnimComplete(DefaultAnimationLayer);
		base.Animator.ResetTrigger("Turn");
		base.EnemyController.ForceFaceTarget();
		base.EnemyController.FollowTarget = true;
		m_enableTail = false;
		base.EnemyController.ResetBaseValues();
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		this.TriggerAttackCooldown(m_spinMove_Exit_AttackCD, ignoreMods: false);
		this.TriggerRestState(ignoreMods: false);
	}

	public override void OnLBCompleteOrCancelled()
	{
		base.OnLBCompleteOrCancelled();
		m_aim = false;
		m_aimSprite.SetActive(value: false);
		m_enableTail = false;
		base.EnemyController.HitboxController.CollisionType = CollisionType.Enemy;
	}

	public override void ResetScript()
	{
		base.ResetScript();
		m_bounceLogic.enabled = false;
		m_enableTail = false;
		m_fireball_TailCounter = m_fireball_TailSpawnDelay;
		m_aim = false;
		m_aimSprite.SetActive(value: false);
	}

	protected void Update()
	{
		if (!base.IsInitialized)
		{
			return;
		}
		if (m_enableTail && m_fireball_TailCounter > 0f)
		{
			m_fireball_TailCounter -= Time.deltaTime;
			if (m_fireball_TailCounter <= 0f)
			{
				if (m_isRamAttack)
				{
					m_fireball_TailCounter = m_fireball_TailRamSpawnDelay;
				}
				else
				{
					m_fireball_TailCounter = m_fireball_TailSpawnDelay;
				}
				if (base.LogicController.EnemyLogicType == EnemyLogicType.Advanced)
				{
					this.FireProjectile("FlyingShieldWaterProjectile", 0, matchFacing: false, 0f, 0f);
				}
				else if (base.LogicController.EnemyLogicType == EnemyLogicType.Expert)
				{
					this.FireProjectile("FlyingShieldWaterExpertProjectile", 0, matchFacing: false, 0f, 0f);
				}
				else
				{
					this.FireProjectile("FlyingShieldWaterMinibossProjectile", 0, matchFacing: false, 0f, 0f);
				}
			}
		}
		if (m_aim)
		{
			if (!m_aimSprite.activeSelf)
			{
				m_aimSprite.SetActive(value: true);
			}
			m_aimSprite.transform.SetLocalEulerZ(CDGHelper.VectorToAngle(base.EnemyController.Heading));
		}
		else if (m_aimSprite.activeSelf)
		{
			m_aimSprite.SetActive(value: false);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (m_aimSprite.activeSelf)
		{
			m_aimSprite.SetActive(value: false);
		}
	}
}
