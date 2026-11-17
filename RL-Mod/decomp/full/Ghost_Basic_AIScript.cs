using System.Collections;
using UnityEngine;

public class Ghost_Basic_AIScript : BaseAIScript
{
	protected float m_scare_TellIntro_AnimationSpeed = 1f;

	protected float m_scare_TellHold_AnimationSpeed = 1f;

	protected float m_scare_AttackIntro_AnimationSpeed = 1f;

	protected float m_scare_AttackIntro_Delay;

	protected const float m_scare_AttackHold_AnimationSpeed = 1f;

	protected const float m_scare_AttackHold_Delay = 0.2f;

	protected float m_scare_ExitIntro_AnimationSpeed = 1f;

	protected const float m_scare_ExitIntro_EndDelay = 0.15f;

	protected float m_scare_Exit_AttackCD;

	private Projectile_RL m_warningProjectile;

	private Projectile_RL m_attackProjectile;

	private SpriteRenderer[] m_tempTransparencyArray;

	private bool m_isInvulnerable;

	private float m_toggleDuration;

	private float m_toggleStartTime;

	protected override Vector2 IdleDuration => new Vector2(0.25f, 0.7f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.45f, 0.85f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.45f, 0.85f);

	protected override Vector2 RandomFollowOffsetX => new Vector2(-1f, 1f);

	protected override Vector2 RandomFollowOffsetY => new Vector2(-1f, 1f);

	protected virtual float m_scare_TellIntroAndHold_Delay => 0.9f;

	protected virtual float m_scare_Exit_ForceIdle => 2f;

	protected virtual float m_scare_initialProjectileDelay => 0f;

	protected virtual float m_scare_exitProjectileDelay => 0f;

	protected virtual bool m_scare_isMiniboss => false;

	protected virtual bool m_scare_fireCardinalProjectiles => false;

	protected virtual bool m_scare_fireDiagonalProjectiles => false;

	protected virtual bool m_scare_fireMinibossProjectiles => false;

	protected virtual float VulnerableDuration => 3f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[6] { "GhostWarningProjectile", "GhostWarningMinibossProjectile", "GhostQuakeProjectile", "GhostQuakeMinibossProjectile", "GhostBoltProjectile", "GhostBoltMinibossProjectile" };
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		m_tempTransparencyArray = enemyController.GetComponentsInChildren<SpriteRenderer>();
		SetInvulnerable(invulnerable: true);
	}

	private void SetInvulnerable(bool invulnerable, float duration = 0f)
	{
		m_isInvulnerable = invulnerable;
		float num = 1f;
		if (invulnerable)
		{
			base.EnemyController.HitboxController.SetHitboxActiveState(HitboxType.Body, active: false);
			base.EnemyController.HitboxController.SetHitboxActiveState(HitboxType.Weapon, active: false);
			num = 0.25f;
		}
		else
		{
			base.EnemyController.HitboxController.SetHitboxActiveState(HitboxType.Body, active: true);
			base.EnemyController.HitboxController.SetHitboxActiveState(HitboxType.Weapon, active: true);
			num = 1f;
		}
		SpriteRenderer[] tempTransparencyArray = m_tempTransparencyArray;
		foreach (SpriteRenderer obj in tempTransparencyArray)
		{
			Color color = obj.color;
			color.a = num;
			obj.color = color;
		}
		m_toggleDuration = duration;
		m_toggleStartTime = Time.time;
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator ScareAttack()
	{
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		base.EnemyController.FollowOffset = Vector3.zero;
		if (!m_scare_isMiniboss)
		{
			m_warningProjectile = this.FireProjectile("GhostWarningProjectile", 0);
		}
		else
		{
			m_warningProjectile = this.FireProjectile("GhostWarningMinibossProjectile", 0);
		}
		SetInvulnerable(invulnerable: false, VulnerableDuration + m_scare_TellIntroAndHold_Delay + 0.2f);
		yield return Default_TellIntroAndLoop("Shake_Tell_Intro", m_scare_TellIntro_AnimationSpeed, "Shake_Tell_Hold", m_scare_TellHold_AnimationSpeed, m_scare_TellIntroAndHold_Delay);
		StopProjectile(ref m_warningProjectile);
		yield return Default_Animation("Shake_Attack_Intro", m_scare_AttackIntro_AnimationSpeed, m_scare_AttackIntro_Delay);
		if (!m_scare_isMiniboss)
		{
			m_attackProjectile = this.FireProjectile("GhostQuakeProjectile", 0);
		}
		else
		{
			m_attackProjectile = this.FireProjectile("GhostQuakeMinibossProjectile", 0);
		}
		if (m_scare_fireCardinalProjectiles)
		{
			this.FireProjectile("GhostBoltProjectile", 0, matchFacing: true, 90f);
			this.FireProjectile("GhostBoltProjectile", 0, matchFacing: true, 210f);
			this.FireProjectile("GhostBoltProjectile", 0, matchFacing: true, 330f);
		}
		if (m_scare_fireDiagonalProjectiles)
		{
			this.FireProjectile("GhostBoltProjectile", 0, matchFacing: true, 270f);
			this.FireProjectile("GhostBoltProjectile", 0, matchFacing: true, 30f);
			this.FireProjectile("GhostBoltProjectile", 0, matchFacing: true, 150f);
		}
		if (m_scare_fireMinibossProjectiles)
		{
			for (int i = 1; i <= 10; i++)
			{
				this.FireProjectile("GhostBoltMinibossProjectile", 0, matchFacing: true, i * 36);
			}
		}
		yield return Default_Animation("Shake_Attack_Hold", 1f, 0f);
		StopProjectile(ref m_attackProjectile);
		base.EnemyController.ResetBaseValues();
		yield return Default_Animation("Shake_Exit", m_scare_ExitIntro_AnimationSpeed, 0.15f);
		base.EnemyController.ResetTurnTrigger();
		yield return Default_Attack_Cooldown(m_scare_Exit_ForceIdle, m_scare_Exit_AttackCD);
	}

	private void FixedUpdate()
	{
		if (m_toggleDuration > 0f && Time.time > m_toggleDuration + m_toggleStartTime)
		{
			SetInvulnerable(!m_isInvulnerable);
		}
	}
}
