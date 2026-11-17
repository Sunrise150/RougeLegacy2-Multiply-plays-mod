using System.Collections;
using UnityEngine;

public class FlameThrower_Hazard : Turret_Hazard
{
	private bool m_startFlameCounter;

	private float m_flameDurationEndTime;

	private Projectile_RL m_flameProjectile;

	private Projectile_RL m_flameWarningProjectile;

	public override string[] ProjectileNameArray
	{
		get
		{
			if (m_projectileNameArray == null)
			{
				m_projectileNameArray = new string[2] { m_projectileName, "WallTurretFlameThrowerWarningProjectile_Template" };
			}
			return m_projectileNameArray;
		}
	}

	public float FlameDuration => base.LoopFireDelay * 0.5f;

	public override float LoopFireDelay => base.LoopFireDelay * 1.5f + FlameDuration;

	protected override void PlayTell()
	{
		float num = m_startingProjectileAngle + base.transform.localEulerAngles.z;
		Vector2 projectileOffset = m_projectileOffset;
		if (base.transform.localScale.x < 0f)
		{
			num += 180f;
			projectileOffset.x = 0f - projectileOffset.x;
		}
		if (base.transform.localScale.y < 0f)
		{
			num = 0f - num;
			projectileOffset.y = 0f - projectileOffset.y;
		}
		if (Projectile_RL.OwnsProjectile(base.gameObject, m_flameWarningProjectile))
		{
			m_flameWarningProjectile.FlagForDestruction();
			m_flameWarningProjectile = null;
		}
		m_flameWarningProjectile = ProjectileManager.FireProjectile(base.gameObject, "WallTurretFlameThrowerWarningProjectile_Template", projectileOffset, matchFacing: true, 0f, 0f);
		base.PlayTell();
	}

	protected override void FireProjectile()
	{
		if (base.InAttackRange)
		{
			if (Projectile_RL.OwnsProjectile(base.gameObject, m_flameWarningProjectile))
			{
				m_flameWarningProjectile.FlagForDestruction();
				m_flameWarningProjectile = null;
			}
			float num = m_startingProjectileAngle + base.transform.localEulerAngles.z;
			Vector2 projectileOffset = m_projectileOffset;
			if (base.transform.localScale.x < 0f)
			{
				num += 180f;
				projectileOffset.x = 0f - projectileOffset.x;
			}
			if (base.transform.localScale.y < 0f)
			{
				num = 0f - num;
				projectileOffset.y = 0f - projectileOffset.y;
			}
			m_flameProjectile = ProjectileManager.FireProjectile(base.gameObject, m_projectileName, projectileOffset, matchFacing: true, 0f, 0f);
			m_flameDurationEndTime = Time.time + FlameDuration;
			m_startFlameCounter = true;
			base.Animator.SetTrigger("Fire");
			base.Animator.ResetTrigger("Tell");
		}
		else
		{
			base.Animator.ResetTrigger("Fire");
			base.Animator.ResetTrigger("Tell");
			base.Animator.SetTrigger("Reset");
		}
		m_timesFired++;
		PrepFiringLogic();
	}

	protected override void Update()
	{
		base.Update();
		if (m_startFlameCounter && Time.time > m_flameDurationEndTime)
		{
			m_startFlameCounter = false;
			m_flameProjectile.FlagForDestruction();
			m_flameProjectile = null;
		}
	}

	protected override void PrepFiringLogic()
	{
		base.PrepFiringLogic();
		if (Projectile_RL.OwnsProjectile(base.gameObject, m_flameWarningProjectile))
		{
			m_flameWarningProjectile.FlagForDestruction();
			m_flameWarningProjectile = null;
		}
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

	public override void ResetHazard()
	{
		m_startFlameCounter = false;
		if (Projectile_RL.OwnsProjectile(base.gameObject, m_flameProjectile))
		{
			m_flameProjectile.gameObject.SetActive(value: false);
		}
		m_flameProjectile = null;
		if (Projectile_RL.OwnsProjectile(base.gameObject, m_flameWarningProjectile))
		{
			m_flameWarningProjectile.gameObject.SetActive(value: false);
		}
		m_flameWarningProjectile = null;
		base.ResetHazard();
	}
}
