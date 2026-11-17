using System.Collections;
using UnityEngine;

public class Kunai_Ability : Bow_Ability
{
	private int m_numKunaisShot;

	private Coroutine m_fireProjectileCoroutine;

	private WaitRL_Yield m_fireDelayWaitYield;

	protected override float TellIntroAnimSpeed => 4f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 3f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 3f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 3f;

	protected override float AttackAnimExitDelay => 0f;

	protected override float ExitAnimSpeed => 3f;

	protected override float ExitAnimExitDelay => 0f;

	protected override Vector2 BowPushbackAmount => new Vector2(0f, 3f);

	protected override float GravityReduction => 0f;

	protected int NumKunais => 5;

	protected float KunaiFireDelay => 0.1f;

	protected override void Awake()
	{
		base.Awake();
		m_fireDelayWaitYield = new WaitRL_Yield(KunaiFireDelay);
	}

	public override void PreCastAbility()
	{
		m_numKunaisShot = 0;
		base.PreCastAbility();
	}

	protected override void FireProjectile()
	{
		if (m_fireProjectileCoroutine != null)
		{
			StopCoroutine(m_fireProjectileCoroutine);
		}
		m_fireProjectileCoroutine = StartCoroutine(FireProjectileCoroutine());
	}

	private IEnumerator FireProjectileCoroutine()
	{
		if (ProjectileName == null)
		{
			yield break;
		}
		ApplyAbilityCosts();
		for (int i = 0; i < NumKunais; i++)
		{
			Projectile_RL projectile = ProjectileManager.FireProjectile(m_abilityController.gameObject, ProjectileName, ProjectileOffset, matchFacing: true, m_aimAngle);
			m_abilityController.InitializeProjectile(projectile);
			m_numKunaisShot++;
			if (m_numKunaisShot < NumKunais)
			{
				yield return m_fireDelayWaitYield;
			}
		}
	}

	protected override IEnumerator ChangeAnim(float duration)
	{
		if (base.CurrentAbilityAnimState == AbilityAnimState.Attack)
		{
			while (m_numKunaisShot < NumKunais)
			{
				yield return null;
			}
			yield return base.ChangeAnim(0f);
		}
		else
		{
			yield return base.ChangeAnim(duration);
		}
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		if (m_fireProjectileCoroutine != null)
		{
			StopCoroutine(m_fireProjectileCoroutine);
		}
		base.StopAbility(abilityInterrupted);
	}
}
