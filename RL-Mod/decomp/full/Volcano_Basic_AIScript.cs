using System.Collections;
using UnityEngine;

public class Volcano_Basic_AIScript : BaseAIScript
{
	protected const string FLAME_TELL_PROJECTILE = "VolcanoWarningProjectile";

	protected Projectile_RL m_flameTellProjectile;

	protected Projectile_RL m_projectile1;

	protected Projectile_RL m_projectile2;

	protected Projectile_RL m_projectile3;

	protected bool m_wasFrozenWhileFiring;

	protected const string FLAMEGOUT_TELL_INTRO = "Flame_Tell_Intro";

	protected const string FLAMEGOUT_TELL_HOLD = "Flame_Tell_Hold";

	protected const string FLAMEGOUT_ATTACK_INTRO = "Flame_Attack_Intro";

	protected const string FLAMEGOUT_ATTACK_HOLD = "Flame_Attack_Hold";

	protected const string FLAMEGOUT_EXIT = "Flame_Exit";

	protected const string FLAMEGOUT_LINEBOLT_PROJECTILE = "VolcanoLineBoltProjectile";

	protected const string FLAMEGOUT_WHITEBOLT_PROJECTILE = "VolcanoWhiteBoltProjectile";

	protected const string FLAMEGOUT_WHITEBOLT_MINIBOSS_PROJECTILE = "VolcanoWhiteBoltMinibossProjectile";

	protected float m_flameGout_TellIntro_AnimationSpeed = 0.85f;

	protected float m_flameGout_TellHold_AnimationSpeed = 1.3f;

	protected float m_flameGout_Tell_Delay = 1.15f;

	protected float m_flameGout_AttackIntro_AnimationSpeed = 1f;

	protected float m_flameGout_AttackIntro_Delay;

	protected float m_flameGout_AttackHold_AnimationSpeed = 1f;

	protected float m_flameGout_AttackHold_Delay;

	protected float m_flameGout_Exit_AnimationSpeed = 1f;

	protected float m_flameGout_Exit_Delay;

	protected const string HOMINGSHOT_TELL_INTRO = "HomingShot_Tell_Intro";

	protected const string HOMINGSHOT_TELL_HOLD = "HomingShot_Tell_Hold";

	protected const string HOMINGSHOT_ATTACK_INTRO = "HomingShot_Attack_Intro";

	protected const string HOMINGSHOT_ATTACK_HOLD = "HomingShot_Attack_Hold";

	protected const string HOMINGSHOT_EXIT = "HomingShot_Exit";

	private const string FLAMEGOUT_HOMINGBOLT_PROJECTILE = "VolcanoHomingBoltProjectile";

	protected float m_alternateGout_TellIntro_AnimationSpeed = 0.85f;

	protected float m_alternateGout_TellHold_AnimationSpeed = 1.3f;

	protected float m_alternateGout_Tell_Delay = 1.45f;

	protected float m_alternateGout_AttackIntro_AnimationSpeed = 1f;

	protected float m_alternateGout_AttackIntro_Delay;

	protected float m_alternateGout_AttackHold_AnimationSpeed = 1f;

	protected float m_alternateGout_AttackHold_Delay;

	protected float m_alternateGout_Exit_AnimationSpeed = 1f;

	protected float m_alternateGout_Exit_Delay;

	protected virtual float m_flameGout_Exit_ForceIdle => 1.25f;

	protected virtual float m_flameGout_AttackCD => 0f;

	protected virtual bool m_flameGout_hasMultipleFlames => true;

	protected virtual float m_flameGout_AttackDuration => 1.25f;

	protected virtual int m_flameGout_FireProjectileLoopAmount => 8;

	protected virtual int m_flameGout_FireProjectileBulletAmount => 1;

	protected virtual Vector2 m_flameGout_FireProjectileAngle => new Vector2(35f, 145f);

	protected virtual Vector2 m_flameGout_FireProjectileSpeedMod => new Vector2(1f, 1.25f);

	protected virtual float m_flameGout_AttackMoveSpeed => 0f;

	protected virtual float m_alternateGout_Exit_ForceIdle => 1.25f;

	protected virtual float m_alternateGout_AttackCD => 0f;

	protected virtual float m_alternateGout_AttackDuration => 1f;

	protected virtual int m_alternateGout_FireProjectileLoopAmount => 1;

	protected virtual int m_alternateGout_FireProjectileBulletAmount => 1;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[5] { "VolcanoLineBoltProjectile", "VolcanoWhiteBoltMinibossProjectile", "VolcanoWhiteBoltProjectile", "VolcanoHomingBoltProjectile", "VolcanoWarningProjectile" };
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator CastFlameGout()
	{
		StopAndFaceTarget();
		bool isFacingRight = base.EnemyController.IsFacingRight;
		base.EnemyController.LockFlip = true;
		m_flameTellProjectile = this.FireProjectile("VolcanoWarningProjectile", 1, matchFacing: true, 90f);
		yield return Default_TellIntroAndLoop("Flame_Tell_Intro", m_flameGout_TellIntro_AnimationSpeed, "Flame_Tell_Hold", m_flameGout_TellHold_AnimationSpeed, m_flameGout_Tell_Delay);
		StopProjectile(ref m_flameTellProjectile);
		SetAttackingWithContactDamage(enable: true);
		yield return Default_Animation("Flame_Attack_Intro", m_flameGout_AttackIntro_AnimationSpeed, m_flameGout_AttackIntro_Delay);
		yield return Default_Animation("Flame_Attack_Hold", m_flameGout_AttackHold_AnimationSpeed, m_flameGout_AttackHold_Delay, waitUntilAnimationComplete: false);
		m_projectile1 = this.FireProjectile("VolcanoLineBoltProjectile", 1, matchFacing: true, 90f);
		if (m_flameGout_hasMultipleFlames)
		{
			m_projectile2 = this.FireProjectile("VolcanoLineBoltProjectile", 1, matchFacing: true, 80f);
			m_projectile3 = this.FireProjectile("VolcanoLineBoltProjectile", 1, matchFacing: true, 100f);
		}
		StartCoroutine(DelayProjectileWeaponHitbox(m_projectile1, 0.25f));
		StartCoroutine(DelayProjectileWeaponHitbox(m_projectile2, 0.25f));
		StartCoroutine(DelayProjectileWeaponHitbox(m_projectile3, 0.25f));
		if (isFacingRight)
		{
			SetVelocityX(m_flameGout_AttackMoveSpeed, additive: false);
		}
		else
		{
			SetVelocityX(0f - m_flameGout_AttackMoveSpeed, additive: false);
		}
		float delayBetweenProjectileAttacks = m_flameGout_AttackDuration / (float)m_flameGout_FireProjectileLoopAmount;
		for (int i = 0; i < m_flameGout_FireProjectileLoopAmount; i++)
		{
			for (int j = 0; j < m_flameGout_FireProjectileBulletAmount; j++)
			{
				int num = (int)Random.Range(m_flameGout_FireProjectileAngle.x, m_flameGout_FireProjectileAngle.y);
				float speedMod = Random.Range(m_flameGout_FireProjectileSpeedMod.x, m_flameGout_FireProjectileSpeedMod.y);
				if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
				{
					this.FireProjectile("VolcanoWhiteBoltMinibossProjectile", 2, matchFacing: true, num, speedMod);
				}
				else
				{
					this.FireProjectile("VolcanoWhiteBoltProjectile", 2, matchFacing: true, num, speedMod);
				}
			}
			if (delayBetweenProjectileAttacks > 0f)
			{
				yield return Wait(delayBetweenProjectileAttacks);
			}
		}
		SetVelocityX(0f, additive: false);
		StopProjectile(ref m_projectile1);
		StopProjectile(ref m_projectile2);
		StopProjectile(ref m_projectile3);
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("Flame_Exit", m_flameGout_Exit_AnimationSpeed, m_flameGout_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		base.EnemyController.LockFlip = false;
		yield return Default_Attack_Cooldown(m_flameGout_Exit_ForceIdle, m_flameGout_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator CastAlternateGout()
	{
		StopAndFaceTarget();
		_ = base.EnemyController.IsFacingRight;
		base.EnemyController.LockFlip = true;
		yield return Default_TellIntroAndLoop("HomingShot_Tell_Intro", m_alternateGout_TellIntro_AnimationSpeed, "HomingShot_Tell_Hold", m_alternateGout_TellHold_AnimationSpeed, m_alternateGout_Tell_Delay);
		SetAttackingWithContactDamage(enable: true, 0f);
		yield return Default_Animation("HomingShot_Attack_Intro", m_alternateGout_AttackIntro_AnimationSpeed, m_alternateGout_AttackIntro_Delay);
		yield return Default_Animation("HomingShot_Attack_Hold", m_alternateGout_AttackHold_AnimationSpeed, m_alternateGout_AttackHold_Delay, waitUntilAnimationComplete: false);
		SetAttackingWithContactDamage(enable: false);
		float delayBetweenProjectileAttacks = m_alternateGout_AttackDuration / (float)m_alternateGout_FireProjectileLoopAmount;
		for (int i = 0; i < m_alternateGout_FireProjectileLoopAmount; i++)
		{
			for (int j = 0; j < m_alternateGout_FireProjectileBulletAmount; j++)
			{
				this.FireProjectile("VolcanoHomingBoltProjectile", 1, matchFacing: true, 75f);
				this.FireProjectile("VolcanoHomingBoltProjectile", 1, matchFacing: true, 105f);
				if (base.LogicController.EnemyLogicType == EnemyLogicType.Expert)
				{
					this.FireProjectile("VolcanoHomingBoltProjectile", 1, matchFacing: true, 60f);
					this.FireProjectile("VolcanoHomingBoltProjectile", 1, matchFacing: true, 120f);
				}
			}
			if (delayBetweenProjectileAttacks > 0f)
			{
				yield return Wait(delayBetweenProjectileAttacks);
			}
		}
		SetVelocityX(0f, additive: false);
		yield return Default_Animation("HomingShot_Exit", m_alternateGout_Exit_AnimationSpeed, m_alternateGout_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		base.EnemyController.LockFlip = false;
		yield return Default_Attack_Cooldown(m_alternateGout_Exit_ForceIdle, m_alternateGout_AttackCD);
	}

	public override void Pause()
	{
		base.Pause();
		if ((bool)m_projectile1 && !m_projectile1.IsFreePoolObj && m_projectile1.Owner == base.EnemyController.gameObject)
		{
			m_wasFrozenWhileFiring = true;
			StopProjectile(ref m_projectile1);
		}
		if ((bool)m_projectile2 && !m_projectile2.IsFreePoolObj && m_projectile2.Owner == base.EnemyController.gameObject)
		{
			m_wasFrozenWhileFiring = true;
			StopProjectile(ref m_projectile2);
		}
		if ((bool)m_projectile3 && !m_projectile3.IsFreePoolObj && m_projectile3.Owner == base.EnemyController.gameObject)
		{
			m_wasFrozenWhileFiring = true;
			StopProjectile(ref m_projectile3);
		}
	}

	public override void Unpause()
	{
		base.Unpause();
		if (m_wasFrozenWhileFiring)
		{
			m_projectile1 = this.FireProjectile("VolcanoLineBoltProjectile", 1, matchFacing: true, 90f);
			if (m_flameGout_hasMultipleFlames)
			{
				m_projectile2 = this.FireProjectile("VolcanoLineBoltProjectile", 1, matchFacing: true, 80f);
				m_projectile3 = this.FireProjectile("VolcanoLineBoltProjectile", 1, matchFacing: true, 100f);
			}
			m_wasFrozenWhileFiring = false;
		}
	}

	public override void OnLBCompleteOrCancelled()
	{
		StopProjectile(ref m_projectile1);
		StopProjectile(ref m_projectile2);
		StopProjectile(ref m_projectile3);
		StopProjectile(ref m_flameTellProjectile);
		base.OnLBCompleteOrCancelled();
	}

	public override void ResetScript()
	{
		m_wasFrozenWhileFiring = false;
		base.ResetScript();
	}

	protected IEnumerator DelayProjectileWeaponHitbox(Projectile_RL projectile, float delay)
	{
		if (delay > 0f)
		{
			if ((bool)projectile && !projectile.IsFreePoolObj && projectile.Owner == base.EnemyController.gameObject)
			{
				projectile.HitboxController.SetHitboxActiveState(HitboxType.Weapon, active: false);
			}
			yield return Wait(delay);
			if ((bool)projectile && !projectile.IsFreePoolObj && projectile.Owner == base.EnemyController.gameObject)
			{
				projectile.HitboxController.SetHitboxActiveState(HitboxType.Weapon, active: true);
			}
		}
	}
}
