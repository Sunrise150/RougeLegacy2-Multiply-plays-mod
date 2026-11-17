using System;
using UnityEngine;

public class PressurePlate_Hazard : Hazard, IHasProjectileNameArray
{
	[NonSerialized]
	private string[] m_projectileNameArray;

	private Projectile_RL m_firedProjectile;

	public string[] ProjectileNameArray
	{
		get
		{
			if (m_projectileNameArray == null)
			{
				m_projectileNameArray = new string[1] { "PressurePlateProjectile" };
			}
			return m_projectileNameArray;
		}
	}

	public void Shoot()
	{
		m_firedProjectile = ProjectileManager.FireProjectile(base.gameObject, ProjectileNameArray[0], Vector2.zero, matchFacing: false);
		Vector3 localEulerAngles = m_firedProjectile.transform.localEulerAngles;
		localEulerAngles.z = 90f;
		m_firedProjectile.transform.localEulerAngles = localEulerAngles;
	}

	public void StopShooting()
	{
		if (m_firedProjectile != null && m_firedProjectile.isActiveAndEnabled)
		{
			StopAllCoroutines();
			m_firedProjectile.FlagForDestruction();
		}
	}

	public override void ResetHazard()
	{
		StopShooting();
	}
}
