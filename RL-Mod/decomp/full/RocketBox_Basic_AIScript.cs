using System;
using System.Collections;
using Sigtrap.Relays;
using UnityEngine;

public class RocketBox_Basic_AIScript : BaseAIScript
{
	private Relay m_shoutAttackWarningAppearedRelay = new Relay();

	private Relay m_shoutAttackExplodedRelay = new Relay();

	private Collider2D m_platformCollider;

	private const string WARNING_PROJECTILE_NAME = "RocketBoxWarningProjectile";

	private const string EXPLOSION_PROJECTILE_NAME = "RocketBoxExplosionProjectile";

	private const string WARNING_MINIBOSS_PROJECTILE_NAME = "RocketBoxWarningMinibossProjectile";

	private const string EXPLOSION_MINIBOSS_PROJECTILE_NAME = "RocketBoxExplosionMinibossProjectile";

	private const string MINIBOSS_BASIC_PROJECTILE_NAME = "RocketBoxBoltMinibossBlueProjectile";

	protected float m_shoot_TellIntro_AnimationSpeed = 1f;

	protected float m_shoot_TellHold_AnimationSpeed = 1f;

	protected float m_shoot_Tell_Delay;

	protected float m_shoot_AttackIntro_AnimationSpeed = 1f;

	protected float m_shoot_AttackIntro_Delay;

	protected float m_shoot_AttackHold_AnimationSpeed = 1f;

	protected float m_shoot_AttackHold_Delay;

	protected float m_shoot_Exit_AnimationSpeed = 1f;

	protected float m_shoot_Exit_Delay;

	protected float m_shoot_Exit_ForceIdle;

	protected float m_shoot_Exit_AttackCD;

	protected float m_shoot_ShotDelay = 0.1f;

	private Projectile_RL m_shoutWarningProjectile;

	public IRelayLink ShoutAttackWarningAppearedRelay => m_shoutAttackWarningAppearedRelay.link;

	public IRelayLink ShoutAttackExplodedRelay => m_shoutAttackExplodedRelay.link;

	protected override Vector2 RandomFollowOffsetX => new Vector2(-7f, 7f);

	protected override Vector2 RandomFollowOffsetY => new Vector2(-6f, 6f);

	protected virtual int m_shoot_NumberOfBullets => 1;

	protected virtual bool m_shoot_VerticalBullets => false;

	protected virtual bool m_shoot_HomingBullets => false;

	protected virtual float m_explosion_WarningDuration => 0.85f;

	protected virtual float m_explosion_AttackCD => 2f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[7] { "RocketBoxBounceHomingBoltProjectile", "RocketBoxBounceBoltProjectile", "RocketBoxWarningProjectile", "RocketBoxExplosionProjectile", "RocketBoxWarningMinibossProjectile", "RocketBoxExplosionMinibossProjectile", "RocketBoxBoltMinibossBlueProjectile" };
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		base.EnemyController.CharacterHitResponse.OnCharacterHitRelay.AddListener(OnEnemyHit);
		m_platformCollider = base.EnemyController.HitboxController.GetCollider(HitboxType.Platform);
	}

	public override void OnEnemyActivated()
	{
		this.TriggerRestState(ignoreMods: true);
	}

	public override void OnLBCompleteOrCancelled()
	{
		base.OnLBCompleteOrCancelled();
		base.EnemyController.AlwaysFacing = true;
		base.EnemyController.LockFlip = false;
		StopProjectile(ref m_shoutWarningProjectile);
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
		float angle = 0f;
		base.EnemyController.AlwaysFacing = false;
		base.EnemyController.LockFlip = true;
		if (!base.EnemyController.IsFacingRight)
		{
			angle = 180f;
		}
		yield return Default_TellIntroAndLoop("SingleShot_Tell_Intro", m_shoot_TellIntro_AnimationSpeed, "SingleShot_Tell_Hold", m_shoot_TellHold_AnimationSpeed, m_shoot_Tell_Delay);
		yield return Default_Animation("SingleShot_Attack_Intro", m_shoot_AttackIntro_AnimationSpeed, m_shoot_AttackIntro_Delay);
		yield return Default_Animation("SingleShot_Attack_Hold", m_shoot_AttackHold_AnimationSpeed, 0f, waitUntilAnimationComplete: false);
		for (int i = 0; i < m_shoot_NumberOfBullets; i++)
		{
			if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
			{
				this.FireProjectile("RocketBoxBoltMinibossBlueProjectile", 1, matchFacing: false, angle);
			}
			else if (m_shoot_HomingBullets)
			{
				this.FireProjectile("RocketBoxBounceHomingBoltProjectile", 0, matchFacing: false, angle);
			}
			else
			{
				this.FireProjectile("RocketBoxBounceBoltProjectile", 0, matchFacing: false, angle);
			}
			if (m_shoot_VerticalBullets)
			{
				if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
				{
					this.FireProjectile("RocketBoxBoltMinibossBlueProjectile", 3, matchFacing: false, 90f);
					this.FireProjectile("RocketBoxBoltMinibossBlueProjectile", 2, matchFacing: false, 180f - angle);
					this.FireProjectile("RocketBoxBoltMinibossBlueProjectile", 4, matchFacing: false, 270f);
				}
				else if (m_shoot_HomingBullets)
				{
					this.FireProjectile("RocketBoxBounceHomingBoltProjectile", 0, matchFacing: false, 90f);
					this.FireProjectile("RocketBoxBounceHomingBoltProjectile", 0, matchFacing: false, 180f - angle);
					this.FireProjectile("RocketBoxBounceHomingBoltProjectile", 0, matchFacing: false, 270f);
				}
				else
				{
					this.FireProjectile("RocketBoxBounceBoltProjectile", 0, matchFacing: false, 90f);
					this.FireProjectile("RocketBoxBounceBoltProjectile", 0, matchFacing: false, 180f - angle);
					this.FireProjectile("RocketBoxBounceBoltProjectile", 0, matchFacing: false, 270f);
				}
			}
			if (m_shoot_NumberOfBullets > 1 && m_shoot_ShotDelay > 0f)
			{
				yield return Wait(m_shoot_ShotDelay);
			}
		}
		base.EnemyController.LockFlip = false;
		if (m_shoot_AttackHold_Delay > 0f)
		{
			yield return Wait(m_shoot_AttackHold_Delay);
		}
		yield return Default_Animation("SingleShot_Exit", m_shoot_Exit_AnimationSpeed, m_shoot_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		base.EnemyController.AlwaysFacing = true;
		base.EnemyController.LockFlip = false;
		yield return Default_Attack_Cooldown(m_shoot_Exit_ForceIdle, m_shoot_Exit_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator AltShoot()
	{
		base.EnemyController.AlwaysFacing = false;
		base.EnemyController.LockFlip = true;
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
		{
			m_shoutWarningProjectile = this.FireProjectileAbsPos("RocketBoxWarningMinibossProjectile", base.EnemyController.Midpoint, matchFacing: false);
		}
		else
		{
			m_shoutWarningProjectile = this.FireProjectileAbsPos("RocketBoxWarningProjectile", base.EnemyController.Midpoint, matchFacing: false);
		}
		m_shoutWarningProjectile.transform.SetParent(base.EnemyController.transform, worldPositionStays: true);
		m_shoutAttackWarningAppearedRelay.Dispatch();
		yield return Default_TellIntroAndLoop("Explosion_Tell_Intro", m_shoot_TellIntro_AnimationSpeed, "Explosion_Tell_Hold", m_shoot_TellHold_AnimationSpeed, m_shoot_Tell_Delay);
		m_shoutAttackWarningAppearedRelay.Dispatch();
		yield return Wait(m_explosion_WarningDuration);
		StopProjectile(ref m_shoutWarningProjectile);
		m_shoutAttackExplodedRelay.Dispatch();
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
		{
			this.FireProjectileAbsPos("RocketBoxExplosionMinibossProjectile", base.EnemyController.Midpoint, matchFacing: false);
		}
		else
		{
			this.FireProjectileAbsPos("RocketBoxExplosionProjectile", base.EnemyController.Midpoint, matchFacing: false);
		}
		yield return Default_Animation("Explosion_Attack_Intro", m_shoot_AttackIntro_AnimationSpeed, m_shoot_AttackIntro_Delay);
		yield return Default_Animation("Explosion_Attack_Hold", m_shoot_AttackHold_AnimationSpeed, 0f, waitUntilAnimationComplete: false);
		base.EnemyController.LockFlip = false;
		if (m_shoot_AttackHold_Delay > 0f)
		{
			yield return Wait(m_shoot_AttackHold_Delay);
		}
		yield return Default_Animation("Explosion_Exit", m_shoot_Exit_AnimationSpeed, m_shoot_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		base.EnemyController.AlwaysFacing = true;
		base.EnemyController.LockFlip = false;
		yield return Default_Attack_Cooldown(m_shoot_Exit_ForceIdle, m_explosion_AttackCD);
	}

	private void OnEnemyHit(object sender, EventArgs args)
	{
		if ((bool)m_platformCollider && base.EnemyController.CurrentHealth > 0f && (args as CharacterHitEventArgs).Attacker.ActualKnockbackStrength > base.EnemyController.ActualKnockbackDefense)
		{
			m_platformCollider.gameObject.layer = 2;
			if (base.gameObject.activeInHierarchy)
			{
				StartCoroutine(ReenablePlatformCoroutine());
			}
		}
	}

	private IEnumerator ReenablePlatformCoroutine()
	{
		float delay = 0.1f + Time.time;
		while (Time.time < delay)
		{
			yield return null;
		}
		while (base.EnemyController.KnockedIntoAir)
		{
			yield return null;
		}
		if (m_platformCollider.gameObject.layer != 11)
		{
			m_platformCollider.gameObject.layer = 11;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if ((bool)m_platformCollider)
		{
			m_platformCollider.gameObject.layer = 11;
		}
		if (!GameManager.IsApplicationClosing && (bool)base.Animator)
		{
			base.Animator.WriteDefaultValues();
		}
	}

	private void OnDestroy()
	{
		if (!GameManager.IsApplicationClosing && base.IsInitialized)
		{
			base.EnemyController.CharacterHitResponse.OnCharacterHitRelay.RemoveListener(OnEnemyHit);
		}
	}
}
