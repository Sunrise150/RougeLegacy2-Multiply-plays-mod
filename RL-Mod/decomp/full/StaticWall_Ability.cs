using System;
using System.Collections;
using UnityEngine;

public class StaticWall_Ability : GenericSpell_Ability, ITalent, IAbility
{
	private const float FLASH_KICK_IN_DURATION = 1f;

	private Action<Projectile_RL, GameObject> m_resumeCooldown;

	protected override void Awake()
	{
		base.Awake();
		m_resumeCooldown = ResumeCooldown;
	}

	protected override void FireProjectile()
	{
		base.FireProjectile();
		StartCoroutine(FlashCoroutine());
		base.DecreaseCooldownWhenHit = false;
		base.DisplayPausedAbilityCooldown = true;
		if ((bool)m_firedProjectile)
		{
			m_firedProjectile.OnDeathRelay.AddOnce(m_resumeCooldown);
		}
	}

	private IEnumerator FlashCoroutine()
	{
		if ((bool)m_firedProjectile)
		{
			m_firedProjectile.Animator.SetBool("Flashing", value: false);
			float lifespan = Time.time + m_firedProjectile.Lifespan - 1f;
			while (Time.time < lifespan)
			{
				yield return null;
			}
			if ((bool)m_firedProjectile)
			{
				m_firedProjectile.Animator.SetBool("Flashing", value: true);
			}
		}
	}

	private void ResumeCooldown(Projectile_RL proj, GameObject obj)
	{
		base.DecreaseCooldownWhenHit = true;
		base.DisplayPausedAbilityCooldown = false;
	}
}
