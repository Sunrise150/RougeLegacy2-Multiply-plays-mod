using System.Collections;
using UnityEngine;

public class FlameThrower_Ability : BaseAbility_RL, ISpell, IAbility
{
	private bool m_isCasting;

	private int m_frameStopped;

	private int m_frameStarted;

	protected override float TellIntroAnimSpeed => 1.45f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 10f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 10f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 1f;

	protected override float AttackAnimExitDelay => 0.075f;

	protected override float ExitAnimSpeed => 1f;

	protected override float ExitAnimExitDelay => 0f;

	public override void PreCastAbility()
	{
		if (m_isCasting && m_frameStopped != Time.frameCount)
		{
			StopAbility(abilityInterrupted: false);
			return;
		}
		m_frameStarted = Time.frameCount;
		m_isCasting = true;
		base.PreCastAbility();
	}

	protected override IEnumerator ChangeAnim(float duration)
	{
		while (duration > 0f)
		{
			duration -= Time.deltaTime;
			yield return null;
		}
		if (base.CurrentAbilityAnimState == AbilityAnimState.Attack)
		{
			float ticRate = m_firedProjectile.RepeatHitDuration;
			while (m_abilityController.PlayerController.CurrentMana >= (float)base.ActualCost && m_isCasting)
			{
				ticRate -= Time.deltaTime;
				if (ticRate <= 0f)
				{
					ApplyAbilityCosts();
					ticRate = m_firedProjectile.RepeatHitDuration;
				}
				yield return null;
			}
		}
		m_animator.SetTrigger("Change_Ability_Anim");
		PerformTurnAnimCheck();
	}

	protected override void OnEnterAttackLogic()
	{
		FireProjectile();
	}

	protected override void ApplyAbilityCosts()
	{
		m_abilityController.BroadcastAbilityCastEvents(base.CastAbilityType);
		base.ApplyAbilityCosts();
	}

	protected override void OnExitAttackLogic()
	{
		m_firedProjectile.transform.SetParent(null, worldPositionStays: true);
		StartCooldownTimer();
		base.OnExitAttackLogic();
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		if ((bool)m_firedProjectile)
		{
			m_firedProjectile.FlagForDestruction();
			m_firedProjectile = null;
		}
		m_isCasting = false;
		m_frameStopped = Time.frameCount;
		base.StopAbility(abilityInterrupted);
	}

	protected override void Update()
	{
		if (m_isCasting && m_frameStarted != Time.frameCount && Rewired_RL.Player.GetButtonDown(m_abilityController.GetAbilityInputString(base.CastAbilityType)))
		{
			m_isCasting = false;
		}
		base.Update();
	}

	private IEnumerator StopProjectileParticleSystem(Projectile_RL projectile)
	{
		ParticleSystem partSys = projectile.GetComponentInChildren<ParticleSystem>();
		if (partSys != null)
		{
			projectile.transform.SetParent(null, worldPositionStays: true);
			projectile.HitboxController.DisableAllCollisions = true;
			partSys.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
			yield return new WaitUntil(() => !partSys.IsAlive());
			projectile.HitboxController.DisableAllCollisions = false;
			projectile.FlagForDestruction();
		}
		else
		{
			projectile.FlagForDestruction();
		}
	}
}
