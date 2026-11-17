using System.Collections;
using UnityEngine;

public class ExplosiveBreakable : Breakable
{
	[SerializeField]
	private string m_explosionProjectileName;

	[SerializeField]
	private string m_warningProjectileName;

	[SerializeField]
	private float m_explosionWarningDuration = 1f;

	private bool m_isExploding;

	private Projectile_RL m_warningProjectile;

	public override bool IsBroken
	{
		get
		{
			if (base.IsBroken)
			{
				return !m_isExploding;
			}
			return false;
		}
	}

	private void OnEnable()
	{
		ProjectileManager.Instance.AddProjectileToPool(m_explosionProjectileName);
		ProjectileManager.Instance.AddProjectileToPool(m_warningProjectileName);
	}

	private void OnDisable()
	{
		m_isExploding = false;
		if ((bool)m_warningProjectile && !m_warningProjectile.IsFreePoolObj && m_warningProjectile.Owner == base.gameObject)
		{
			m_warningProjectile.FlagForDestruction();
		}
		m_warningProjectile = null;
	}

	protected override void TriggerCollision(IDamageObj damageObj)
	{
		if (!m_isExploding)
		{
			base.TriggerCollision(damageObj);
		}
	}

	protected override void Break(IDamageObj damageObj)
	{
		StartCoroutine(ExplosionCoroutine(damageObj));
	}

	private IEnumerator ExplosionCoroutine(IDamageObj damageObj)
	{
		m_isExploding = true;
		BroadcastHitEvents(damageObj);
		Vector2 offsetPos = new Vector2(0f, m_hitboxController.GetCollider(HitboxType.Body).bounds.extents.y);
		if (!string.IsNullOrEmpty(m_warningProjectileName))
		{
			m_warningProjectile = ProjectileManager.FireProjectile(base.gameObject, m_warningProjectileName, offsetPos, matchFacing: false);
			m_warningProjectile.transform.SetParent(base.transform, worldPositionStays: true);
		}
		float blinkDuration = Time.time + m_explosionWarningDuration;
		float blinkInterval2 = 0.3f;
		while (Time.time < blinkDuration)
		{
			float blinkIntervalDuration = Time.time + blinkInterval2;
			m_hitEffect.StartSingleBlinkEffect();
			while (Time.time < blinkIntervalDuration)
			{
				yield return null;
			}
			blinkInterval2 -= 0.05f;
			blinkInterval2 = Mathf.Max(blinkInterval2, 0.15f);
		}
		ProjectileManager.FireProjectile(base.gameObject, m_explosionProjectileName, offsetPos, matchFacing: false);
		if ((bool)m_warningProjectile)
		{
			m_warningProjectile.FlagForDestruction();
		}
		m_warningProjectile = null;
		base.Break(damageObj);
		m_isExploding = false;
	}
}
