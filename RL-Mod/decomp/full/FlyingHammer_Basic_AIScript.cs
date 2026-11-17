using System.Collections;
using UnityEngine;

public class FlyingHammer_Basic_AIScript : BaseAIScript
{
	private const string GENERIC_PROJECTILE_NAME = "FlyingHammerMagmaProjectile";

	private const string WARNING_MINIBOSS_PROJECTILE_NAME = "FlyingHammerWarningMinibossProjectile";

	private const string WARNING_LARGE_PROJECTILE_NAME = "FlyingHammerWarningLargeProjectile";

	private const string WARNING_PROJECTILE_NAME = "FlyingHammerWarningProjectile";

	private const string QUAKE_MINIBOSS_PROJECTILE_NAME = "FlyingHammerQuakeMinibossProjectile";

	private const string QUAKE_LARGE_PROJECTILE_NAME = "FlyingHammerQuakeLargeProjectile";

	private const string QUAKE_PROJECTILE_NAME = "FlyingHammerQuakeProjectile";

	private const string FIREBALL_MINIBOSS_PROJECTILE_NAME = "FlyingHammerFireballMinibossProjectile";

	private const string CURSEBOLT_PROJECTILE_NAME = "FlyingHammerCurseBoltProjectile";

	private bool m_frozenWhileHammering;

	private bool m_isHammeringAtPlayer;

	protected Projectile_RL m_warningProjectile;

	protected Projectile_RL m_shockwaveProjectile;

	protected Vector3 m_shockwaveProjectileSpawnPos;

	protected float m_vertSpin_TellIntro_AnimationSpeed = 1f;

	protected float m_vertSpin_TellHold_AnimationSpeed = 1f;

	protected float m_vertSpin_TellIntroAndHold_Delay = 0.75f;

	protected float m_vertSpin_AttackIntro_AnimationSpeed = 1f;

	protected float m_vertSpin_AttackIntro_Delay;

	protected const float m_vertSpin_AttackHold_AnimationSpeed = 1f;

	protected const float m_vertSpin_AttackHold_Delay = 0f;

	protected float m_vertSpin_ExitIntro_AnimationSpeed = 1f;

	protected const float m_vertSpin_ExitIntro_EndDelay = 0.15f;

	protected const float m_vertSpin_Exit_ForceIdle = 0.5f;

	protected float m_vertSpin_Exit_AttackCD = 3f;

	protected float m_vertSpin_Attack_TurnSpeed;

	protected float m_shockwave_TellIntro_AnimationSpeed = 1f;

	protected float m_shockwave_TellHold_AnimationSpeed = 1f;

	protected float m_shockwave_TellIntroAndHold_Delay = 1f;

	protected float m_shockwave_AttackIntro_AnimationSpeed = 1f;

	protected float m_shockwave_AttackIntro_Delay;

	protected const float m_shockwave_AttackHold_AnimationSpeed = 1f;

	protected const float m_shockwave_AttackHold_Delay = 1f;

	protected float m_shockwave_ExitIntro_AnimationSpeed = 1f;

	protected const float m_shockwave_ExitIntro_EndDelay = 0.15f;

	protected const float m_shockwave_Exit_ForceIdle = 1.25f;

	protected float m_shockwave_Exit_AttackCD;

	protected float m_shockwaveAtPlayer_TellIntro_AnimationSpeed = 1f;

	protected float m_shockwaveAtPlayer_TellHold_AnimationSpeed = 1f;

	protected float m_shockwaveAtPlayer_TellIntroAndHold_Delay = 1.45f;

	protected float m_shockwaveAtPlayer_AttackIntro_AnimationSpeed = 1f;

	protected float m_shockwaveAtPlayer_AttackIntro_Delay;

	protected const float m_shockwaveAtPlayer_AttackHold_AnimationSpeed = 1f;

	protected const float m_shockwaveAtPlayer_AttackHold_Delay = 1.55f;

	protected float m_shockwaveAtPlayer_ExitIntro_AnimationSpeed = 1f;

	protected const float m_shockwaveAtPlayer_ExitIntro_EndDelay = 0.15f;

	protected const float m_shockwaveAtPlayer_Exit_ForceIdle = 1.25f;

	protected float m_shockwaveAtPlayer_Exit_AttackCD = 4f;

	protected const string SHOCKWAVE_AT_PLAYER_PROJECTILE = "FlyingHammerWhirlwindProjectile";

	protected const string SHOCKWAVE_WARNING_AT_PLAYER_PROJECTILE = "FlyingHammerWarningWhirlwindProjectile";

	protected override Vector2 IdleDuration => new Vector2(0.7f, 1f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.8f, 1.1f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.8f, 1.1f);

	protected override Vector2 RandomFollowOffsetX => new Vector2(0f, 0f);

	protected override Vector2 RandomFollowOffsetY => new Vector2(0f, 0f);

	protected virtual float m_vertSpin_Attack_MovementSpeed => 15f;

	protected virtual Vector2 m_fireballSpeedMod => new Vector2(0.5f, 1f);

	protected virtual Vector2 m_fireballAngle => new Vector2(180f, 0f);

	protected virtual float m_initialProjectileDelay => 0.35f;

	protected virtual float m_exitProjectileDelay => 0.55f;

	protected virtual int m_projectileSpawnAmount => 1;

	protected virtual int m_projectileSpawnLoopCount => 12;

	protected virtual float m_projectileRateOfFire => 0.125f;

	protected virtual bool m_shockwave_IsLarge => false;

	protected virtual float m_shockwave_Attack_TurnSpeed => 100f;

	protected virtual float m_shockwave_Attack_MovementSpeed => 0f;

	protected virtual Vector2 m_shockwave_fireballSpeedMod => new Vector2(0.5f, 1.25f);

	protected virtual Vector2 m_shockwave_fireballAngle => new Vector2(360f, 0f);

	protected virtual float m_shockwave_initialProjectileDelay => 0f;

	protected virtual float m_shockwave_exitProjectileDelay => 0f;

	protected virtual int m_shockwave_projectileSpawnAmount => 0;

	protected virtual int m_shockwave_projectileSpawnLoopCount => 0;

	protected virtual bool m_causesRicochet => true;

	protected virtual Vector2 m_ricochetKnockbackMod => new Vector2(10.5f, 13.5f);

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[11]
		{
			"FlyingHammerMagmaProjectile", "FlyingHammerWarningMinibossProjectile", "FlyingHammerWarningLargeProjectile", "FlyingHammerWarningProjectile", "FlyingHammerQuakeMinibossProjectile", "FlyingHammerQuakeLargeProjectile", "FlyingHammerQuakeProjectile", "FlyingHammerFireballMinibossProjectile", "FlyingHammerCurseBoltProjectile", "FlyingHammerWhirlwindProjectile",
			"FlyingHammerWarningWhirlwindProjectile"
		};
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Vertical_Spin_Attack()
	{
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		base.EnemyController.FollowOffset = Vector3.zero;
		yield return Default_TellIntroAndLoop("SideSpin_Tell_Intro", m_vertSpin_TellIntro_AnimationSpeed, "SideSpin_Tell_Hold", m_vertSpin_TellHold_AnimationSpeed, m_vertSpin_TellIntroAndHold_Delay);
		SetAttackingWithContactDamage(enable: true);
		yield return Default_Animation("SideSpin_Attack_Intro", m_vertSpin_AttackIntro_AnimationSpeed, m_vertSpin_AttackIntro_Delay);
		yield return Default_Animation("SideSpin_Attack_Hold", 1f, 0f, waitUntilAnimationComplete: false);
		base.EnemyController.FlyingMovementType = FlyingMovementType.Towards;
		base.EnemyController.BaseSpeed = m_vertSpin_Attack_MovementSpeed;
		base.EnemyController.BaseTurnSpeed = m_vertSpin_Attack_TurnSpeed;
		if (m_initialProjectileDelay > 0f)
		{
			yield return Wait(m_initialProjectileDelay);
		}
		for (int i = 0; i < m_projectileSpawnLoopCount; i++)
		{
			for (int j = 0; j < m_projectileSpawnAmount; j++)
			{
				int num = (int)Random.Range(m_fireballAngle.x, m_fireballAngle.y);
				float speedMod = Random.Range(m_fireballSpeedMod.x, m_fireballSpeedMod.y);
				this.FireProjectile("FlyingHammerMagmaProjectile", new Vector2(0f, 0f), matchFacing: true, num, speedMod);
			}
			if (m_projectileRateOfFire > 0f)
			{
				yield return Wait(m_projectileRateOfFire);
			}
		}
		base.EnemyController.ResetBaseValues();
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("SideSpin_Exit", m_vertSpin_ExitIntro_AnimationSpeed, 0.15f);
		base.EnemyController.ResetTurnTrigger();
		yield return Default_Attack_Cooldown(0.5f, m_vertSpin_Exit_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Shockwave_Attack()
	{
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		base.EnemyController.FollowOffset = Vector3.zero;
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
		{
			m_warningProjectile = this.FireProjectile("FlyingHammerWarningMinibossProjectile", 0);
		}
		else if (m_shockwave_IsLarge)
		{
			m_warningProjectile = this.FireProjectile("FlyingHammerWarningLargeProjectile", 0);
		}
		else
		{
			m_warningProjectile = this.FireProjectile("FlyingHammerWarningProjectile", 0);
		}
		yield return Default_TellIntroAndLoop("WallHit_Tell_Intro", m_shockwave_TellIntro_AnimationSpeed, "WallHit_Tell_Hold", m_shockwave_TellHold_AnimationSpeed, m_shockwave_TellIntroAndHold_Delay);
		StopProjectile(ref m_warningProjectile);
		SetAttackingWithContactDamage(enable: true);
		yield return Default_Animation("WallHit_Attack_Intro", m_shockwave_AttackIntro_AnimationSpeed, m_shockwave_AttackIntro_Delay);
		base.EnemyController.FlyingMovementType = FlyingMovementType.Towards;
		base.EnemyController.BaseSpeed = m_shockwave_Attack_MovementSpeed;
		base.EnemyController.BaseTurnSpeed = m_shockwave_Attack_TurnSpeed;
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
		{
			m_shockwaveProjectile = this.FireProjectile("FlyingHammerQuakeMinibossProjectile", 0);
		}
		else if (m_shockwave_IsLarge)
		{
			m_shockwaveProjectile = this.FireProjectile("FlyingHammerQuakeLargeProjectile", 0);
		}
		else
		{
			m_shockwaveProjectile = this.FireProjectile("FlyingHammerQuakeProjectile", 0);
		}
		m_shockwaveProjectileSpawnPos = m_shockwaveProjectile.transform.position;
		yield return Default_Animation("WallHit_Attack_Hold", 1f, 0f);
		if (m_initialProjectileDelay > 0f)
		{
			yield return Wait(m_shockwave_initialProjectileDelay);
		}
		for (int i = 0; i < m_shockwave_projectileSpawnLoopCount + 1; i++)
		{
			for (int j = 0; j < m_shockwave_projectileSpawnAmount; j++)
			{
				int num = (int)Random.Range(m_shockwave_fireballAngle.x, m_shockwave_fireballAngle.y);
				float speedMod = Random.Range(m_shockwave_fireballSpeedMod.x, m_shockwave_fireballSpeedMod.y);
				if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
				{
					this.FireProjectile("FlyingHammerFireballMinibossProjectile", new Vector2(0f, 0f), matchFacing: false, num, speedMod);
				}
				else
				{
					this.FireProjectile("FlyingHammerCurseBoltProjectile", new Vector2(0f, 0f), matchFacing: false, num, speedMod);
				}
			}
			if (1f / (float)m_shockwave_projectileSpawnLoopCount > 0f)
			{
				yield return Wait(1f / (float)(m_shockwave_projectileSpawnLoopCount + 1));
			}
		}
		StopProjectile(ref m_shockwaveProjectile);
		base.EnemyController.ResetBaseValues();
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("WallHit_Exit", m_shockwave_ExitIntro_AnimationSpeed, 0.15f);
		base.EnemyController.ResetTurnTrigger();
		yield return Default_Attack_Cooldown(1.25f, m_shockwave_Exit_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator ShockwaveAtPlayer_Attack()
	{
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		base.EnemyController.FollowOffset = Vector3.zero;
		base.EnemyController.LockFlip = true;
		Vector3 shockwaveSpawnPos = base.EnemyController.TargetController.Midpoint;
		m_isHammeringAtPlayer = true;
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
		{
			m_warningProjectile = this.FireProjectileAbsPos("FlyingHammerWarningWhirlwindProjectile", shockwaveSpawnPos, matchFacing: false);
		}
		else if (m_shockwave_IsLarge)
		{
			m_warningProjectile = this.FireProjectileAbsPos("FlyingHammerWarningWhirlwindProjectile", shockwaveSpawnPos, matchFacing: false);
		}
		else
		{
			m_warningProjectile = this.FireProjectileAbsPos("FlyingHammerWarningWhirlwindProjectile", shockwaveSpawnPos, matchFacing: false);
		}
		yield return Default_TellIntroAndLoop("VerticalSpin_Tell_Intro", m_shockwaveAtPlayer_TellIntro_AnimationSpeed, "VerticalSpin_Tell_Hold", m_shockwaveAtPlayer_TellHold_AnimationSpeed, m_shockwaveAtPlayer_TellIntroAndHold_Delay);
		SetAttackingWithContactDamage(enable: true);
		StopProjectile(ref m_warningProjectile);
		yield return Default_Animation("VerticalSpin_Attack_Intro", m_shockwaveAtPlayer_AttackIntro_AnimationSpeed, m_shockwaveAtPlayer_AttackIntro_Delay);
		base.EnemyController.FlyingMovementType = FlyingMovementType.Towards;
		base.EnemyController.BaseSpeed = m_shockwave_Attack_MovementSpeed;
		base.EnemyController.BaseTurnSpeed = m_shockwave_Attack_TurnSpeed;
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
		{
			m_shockwaveProjectile = this.FireProjectileAbsPos("FlyingHammerWhirlwindProjectile", shockwaveSpawnPos, matchFacing: false);
		}
		else if (m_shockwave_IsLarge)
		{
			m_shockwaveProjectile = this.FireProjectileAbsPos("FlyingHammerWhirlwindProjectile", shockwaveSpawnPos, matchFacing: false);
		}
		else
		{
			m_shockwaveProjectile = this.FireProjectileAbsPos("FlyingHammerWhirlwindProjectile", shockwaveSpawnPos, matchFacing: false);
		}
		m_shockwaveProjectileSpawnPos = m_shockwaveProjectile.transform.position;
		if (m_shockwave_IsLarge)
		{
			yield return Default_Animation("VerticalSpin_Attack_Hold", 1f, 0f);
		}
		else
		{
			yield return Default_Animation("VerticalSpin_Attack_Hold", 1f, 1.55f);
		}
		if (m_initialProjectileDelay > 0f)
		{
			yield return Wait(m_shockwave_initialProjectileDelay);
		}
		if (m_shockwave_IsLarge)
		{
			for (int i = 0; i < m_shockwave_projectileSpawnLoopCount + 1; i++)
			{
				for (int j = 0; j < m_shockwave_projectileSpawnAmount; j++)
				{
					int num = (int)Random.Range(m_shockwave_fireballAngle.x, m_shockwave_fireballAngle.y);
					float speedMod = Random.Range(m_shockwave_fireballSpeedMod.x, m_shockwave_fireballSpeedMod.y);
					if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
					{
						this.FireProjectileAbsPos("FlyingHammerFireballMinibossProjectile", shockwaveSpawnPos, matchFacing: false, num, speedMod);
					}
					else
					{
						this.FireProjectileAbsPos("FlyingHammerCurseBoltProjectile", shockwaveSpawnPos, matchFacing: false, num, speedMod);
					}
				}
				if (1f / (float)m_shockwave_projectileSpawnLoopCount > 0f)
				{
					yield return Wait(1f / (float)(m_shockwave_projectileSpawnLoopCount + 1));
				}
			}
		}
		StopProjectile(ref m_shockwaveProjectile);
		m_isHammeringAtPlayer = false;
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("VerticalSpin_Exit", m_shockwaveAtPlayer_ExitIntro_AnimationSpeed, 0.15f);
		base.EnemyController.ResetTurnTrigger();
		base.EnemyController.ResetBaseValues();
		base.EnemyController.LockFlip = false;
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		yield return Default_Attack_Cooldown(1.25f, m_shockwaveAtPlayer_Exit_AttackCD);
	}

	public override void Pause()
	{
		base.Pause();
		if (!m_shockwaveProjectile || m_shockwaveProjectile.IsFreePoolObj || !(m_shockwaveProjectile.Owner == base.EnemyController.gameObject))
		{
			return;
		}
		m_frozenWhileHammering = true;
		StopProjectile(ref m_shockwaveProjectile);
		if (!m_isHammeringAtPlayer)
		{
			if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
			{
				m_warningProjectile = this.FireProjectileAbsPos("FlyingHammerWarningMinibossProjectile", m_shockwaveProjectileSpawnPos, matchFacing: false);
			}
			else if (m_shockwave_IsLarge)
			{
				m_warningProjectile = this.FireProjectileAbsPos("FlyingHammerWarningLargeProjectile", m_shockwaveProjectileSpawnPos, matchFacing: false);
			}
			else
			{
				m_warningProjectile = this.FireProjectileAbsPos("FlyingHammerWarningProjectile", m_shockwaveProjectileSpawnPos, matchFacing: false);
			}
		}
		else if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
		{
			m_warningProjectile = this.FireProjectileAbsPos("FlyingHammerWarningWhirlwindProjectile", m_shockwaveProjectileSpawnPos, matchFacing: false);
		}
		else if (m_shockwave_IsLarge)
		{
			m_warningProjectile = this.FireProjectileAbsPos("FlyingHammerWarningWhirlwindProjectile", m_shockwaveProjectileSpawnPos, matchFacing: false);
		}
		else
		{
			m_warningProjectile = this.FireProjectileAbsPos("FlyingHammerWarningWhirlwindProjectile", m_shockwaveProjectileSpawnPos, matchFacing: false);
		}
	}

	public override void Unpause()
	{
		base.Unpause();
		if (!m_frozenWhileHammering)
		{
			return;
		}
		if (!m_isHammeringAtPlayer)
		{
			if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
			{
				m_shockwaveProjectile = this.FireProjectileAbsPos("FlyingHammerQuakeMinibossProjectile", m_shockwaveProjectileSpawnPos, matchFacing: false);
			}
			else if (m_shockwave_IsLarge)
			{
				m_shockwaveProjectile = this.FireProjectileAbsPos("FlyingHammerQuakeLargeProjectile", m_shockwaveProjectileSpawnPos, matchFacing: false);
			}
			else
			{
				m_shockwaveProjectile = this.FireProjectileAbsPos("FlyingHammerQuakeProjectile", m_shockwaveProjectileSpawnPos, matchFacing: false);
			}
		}
		else if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
		{
			m_shockwaveProjectile = this.FireProjectileAbsPos("FlyingHammerWhirlwindProjectile", m_shockwaveProjectileSpawnPos, matchFacing: false);
		}
		else if (m_shockwave_IsLarge)
		{
			m_shockwaveProjectile = this.FireProjectileAbsPos("FlyingHammerWhirlwindProjectile", m_shockwaveProjectileSpawnPos, matchFacing: false);
		}
		else
		{
			m_shockwaveProjectile = this.FireProjectileAbsPos("FlyingHammerWhirlwindProjectile", m_shockwaveProjectileSpawnPos, matchFacing: false);
		}
		StopProjectile(ref m_warningProjectile);
		m_frozenWhileHammering = false;
	}

	public override void OnLBCompleteOrCancelled()
	{
		StopProjectile(ref m_warningProjectile);
		StopProjectile(ref m_shockwaveProjectile);
		m_frozenWhileHammering = false;
		base.OnLBCompleteOrCancelled();
	}

	public override void ResetScript()
	{
		m_frozenWhileHammering = false;
		m_isHammeringAtPlayer = false;
		base.ResetScript();
	}
}
