using System.Collections;
using UnityEngine;

public class Fireball_Basic_AIScript : BaseAIScript, IBodyOnEnterHitResponse, IHitResponse
{
	private TrailRenderer m_trailRenderer;

	private float m_timeSinceLastWalkTowardsFireball;

	protected const string DASH_TELL_INTRO = "Dash_Tell_Intro";

	protected const string DASH_TELL_HOLD = "Dash_Tell_Hold";

	protected const string DASH_ATTACK_INTRO = "Dash_Attack_Intro";

	protected const string DASH_ATTACK_HOLD = "Dash_Attack_Hold";

	protected const string DASH_EXIT = "Dash_Exit";

	protected virtual string FIREBALL_PROJECTILE => "FireballEnemyProjectile";

	protected virtual string FIREBALL_HOMING_PROJECTILE => "FireballMinibossEnemyProjectile";

	protected override Vector2 IdleDuration => new Vector2(0.25f, 0.7f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.5f, 1.5f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.45f, 0.85f);

	protected override Vector2 RandomFollowOffsetX => Vector2.zero;

	protected override Vector2 RandomFollowOffsetY => Vector2.zero;

	protected virtual float m_fireballSpeedMultiplier => 1f;

	protected virtual bool m_dropsFireballsWhileWalking => false;

	protected virtual float m_timeBetweenWalkTowardFireballDrops => 1.5f;

	protected virtual float m_dash_TellIntro_AnimSpeed => 1f;

	protected virtual float m_dash_TellIntro_Delay => 0f;

	protected virtual float m_dash_TellHold_AnimSpeed => 1f;

	protected virtual float m_dash_TellHold_Delay => 0.85f;

	protected virtual float m_dash_AttackIntro_AnimSpeed => 1f;

	protected virtual float m_dash_AttackIntro_Delay => 0f;

	protected virtual float m_dash_AttackHold_AnimSpeed => 1f;

	protected virtual float m_dash_AttackHold_Delay => 0f;

	protected virtual float m_dash_Exit_AnimSpeed => 1f;

	protected virtual float m_dash_Exit_Delay => 0f;

	protected virtual float m_dash_Exit_ForceIdle => 0.25f;

	protected virtual float m_dash_Exit_AttackCD => 5.5f;

	protected virtual float m_dash_Attack_ForwardSpeedOverride => 27.5f;

	protected virtual float m_dash_Attack_Duration => 0.6f;

	protected virtual bool m_dropsFireballsDuringDashAttack => false;

	protected virtual float m_fireballDropDuringDashInterval => 0.075f;

	protected virtual int m_spawnFireballOnHitCount => 0;

	protected virtual Vector2 m_spawnFireballOnHitSpeedModRange => new Vector2(1f, 1f);

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[2] { FIREBALL_PROJECTILE, FIREBALL_HOMING_PROJECTILE };
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		base.EnemyController.LogicController.OverrideLogicDelay(0.7f);
		m_trailRenderer = base.EnemyController.GetComponentInChildren<TrailRenderer>();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if ((bool)m_trailRenderer)
		{
			m_trailRenderer.Clear();
		}
	}

	private void FixedUpdate()
	{
		if (!base.IsInitialized || !m_dropsFireballsWhileWalking || !base.LogicController || base.LogicController.CurrentLogicBlockName == null)
		{
			return;
		}
		if (base.LogicController.CurrentLogicBlockName.StartsWith("Walk"))
		{
			if (m_timeSinceLastWalkTowardsFireball >= m_timeBetweenWalkTowardFireballDrops)
			{
				DropFireball();
				m_timeSinceLastWalkTowardsFireball = 0f;
			}
			else
			{
				m_timeSinceLastWalkTowardsFireball += Time.deltaTime;
			}
		}
		else
		{
			m_timeSinceLastWalkTowardsFireball = 0f;
		}
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
		StopAndFaceTarget();
		yield return Default_Animation("Dash_Tell_Intro", m_dash_TellIntro_AnimSpeed, m_dash_TellIntro_Delay, waitUntilAnimationComplete: false);
		yield return Default_Animation("Dash_Tell_Hold", m_dash_TellHold_AnimSpeed, m_dash_TellHold_Delay, waitUntilAnimationComplete: false);
		base.EnemyController.AttackingWithContactDamage = true;
		base.EnemyController.Heading = base.EnemyController.TargetController.Midpoint - base.EnemyController.Midpoint;
		base.EnemyController.LockFlip = true;
		base.EnemyController.FollowTarget = false;
		yield return Default_Animation("Dash_Attack_Intro", m_dash_AttackIntro_AnimSpeed, m_dash_AttackIntro_Delay);
		yield return Default_Animation("Dash_Attack_Hold", m_dash_AttackHold_AnimSpeed, m_dash_AttackHold_Delay, waitUntilAnimationComplete: false);
		base.EnemyController.FlyingMovementType = FlyingMovementType.Towards;
		base.EnemyController.BaseSpeed = m_dash_Attack_ForwardSpeedOverride;
		if (m_dropsFireballsDuringDashAttack)
		{
			yield return DropFireballDuringDash();
		}
		else if (m_dash_Attack_Duration > 0f)
		{
			yield return Wait(m_dash_Attack_Duration);
		}
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		base.EnemyController.AttackingWithContactDamage = false;
		yield return Default_Animation("Dash_Exit", m_dash_Exit_AnimSpeed, m_dash_Exit_Delay);
		yield return Default_Attack_Cooldown(m_dash_Exit_ForceIdle, m_dash_Exit_AttackCD);
		base.EnemyController.ResetTurnTrigger();
		base.EnemyController.ResetBaseValues();
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator StunAttack()
	{
		StopAndFaceTarget();
		yield return Default_Animation("Stunned", m_dash_TellIntro_AnimSpeed, 5f);
		base.EnemyController.AttackingWithContactDamage = true;
		Vector2 pt = PlayerManager.GetPlayer().transform.position - base.EnemyController.transform.position;
		float angle = CDGHelper.VectorToAngle(pt);
		this.FireProjectile(FIREBALL_HOMING_PROJECTILE, 0, matchFacing: false, angle, m_fireballSpeedMultiplier);
		yield return Wait(0.15f);
		this.FireProjectile(FIREBALL_HOMING_PROJECTILE, 0, matchFacing: false, angle, m_fireballSpeedMultiplier);
		yield return Wait(0.15f);
		this.FireProjectile(FIREBALL_HOMING_PROJECTILE, 0, matchFacing: false, angle, m_fireballSpeedMultiplier);
		yield return Wait(0.15f);
		this.FireProjectile(FIREBALL_HOMING_PROJECTILE, 0, matchFacing: false, angle, m_fireballSpeedMultiplier);
		yield return Wait(0.15f);
		this.FireProjectile(FIREBALL_HOMING_PROJECTILE, 0, matchFacing: false, angle, m_fireballSpeedMultiplier);
		yield return Wait(0.15f);
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		base.EnemyController.AttackingWithContactDamage = false;
		yield return Default_Attack_Cooldown(m_dash_Exit_ForceIdle, m_dash_Exit_AttackCD);
		base.EnemyController.ResetTurnTrigger();
		base.EnemyController.ResetBaseValues();
	}

	private IEnumerator DropFireballDuringDash()
	{
		int numFireballs = (int)(m_dash_Attack_Duration / m_fireballDropDuringDashInterval);
		float remainingTime = m_dash_Attack_Duration - m_fireballDropDuringDashInterval * (float)numFireballs;
		for (int i = 0; i < numFireballs; i++)
		{
			if (base.EnemyController.EnemyRank == EnemyRank.Miniboss)
			{
				DropFireball();
			}
			else
			{
				DropFireball();
			}
			if (m_fireballDropDuringDashInterval > 0f)
			{
				yield return Wait(m_fireballDropDuringDashInterval);
			}
		}
		if (remainingTime > 0f)
		{
			yield return Wait(remainingTime);
		}
	}

	public void BodyOnEnterHitResponse(IHitboxController otherHBController)
	{
		if (m_spawnFireballOnHitCount <= 0)
		{
			return;
		}
		int num = 360 / (m_spawnFireballOnHitCount + 1);
		for (int i = 0; i < m_spawnFireballOnHitCount; i++)
		{
			float angle = Random.Range(num * i, num * (i + 1));
			float speedMod = m_spawnFireballOnHitSpeedModRange.x;
			if (m_spawnFireballOnHitSpeedModRange.x != m_spawnFireballOnHitSpeedModRange.y)
			{
				speedMod = Random.Range(m_spawnFireballOnHitSpeedModRange.x, m_spawnFireballOnHitSpeedModRange.y);
			}
			this.FireProjectile(FIREBALL_PROJECTILE, new Vector2(0f, 0f), matchFacing: false, angle, speedMod);
		}
	}

	private void DropFireball(bool homing = false)
	{
		float num = 270f;
		if (homing)
		{
			num = CDGHelper.VectorToAngle(PlayerManager.GetPlayer().transform.position - base.EnemyController.transform.position);
			this.FireProjectile(FIREBALL_HOMING_PROJECTILE, 0, matchFacing: false, num, m_fireballSpeedMultiplier);
		}
		else if (base.EnemyController.EnemyRank == EnemyRank.Miniboss)
		{
			this.FireProjectile(FIREBALL_PROJECTILE, 0, matchFacing: false, num, m_fireballSpeedMultiplier);
			this.FireProjectile(FIREBALL_PROJECTILE, 0, matchFacing: false, num - 180f, m_fireballSpeedMultiplier);
		}
		else
		{
			this.FireProjectile(FIREBALL_PROJECTILE, 0, matchFacing: false, num, m_fireballSpeedMultiplier);
		}
	}

	public override void OnLBCompleteOrCancelled()
	{
		base.EnemyController.FollowTarget = true;
		base.EnemyController.LockFlip = false;
		base.OnLBCompleteOrCancelled();
	}
}
