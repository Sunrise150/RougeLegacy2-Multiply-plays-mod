using System;
using UnityEngine;

public class DistanceSkillCritProjectileLogic : BaseProjectileLogic
{
	private enum DistanceCheckType
	{
		GreaterOrEqual,
		LessOrEqual
	}

	[SerializeField]
	private float m_critDistance;

	[SerializeField]
	private DistanceCheckType m_checkType;

	private bool m_critWasApplied;

	private Action<Projectile_RL, GameObject> m_onProjectileCollision;

	protected override void Awake()
	{
		base.Awake();
		m_onProjectileCollision = OnProjectileCollision;
	}

	private void OnEnable()
	{
		base.SourceProjectile.OnCollisionRelay.AddListener(m_onProjectileCollision);
		m_critWasApplied = false;
	}

	private void OnDisable()
	{
		base.SourceProjectile.OnCollisionRelay.RemoveListener(m_onProjectileCollision);
	}

	private void OnProjectileCollision(Projectile_RL projectile, GameObject obj)
	{
		float x = PlayerManager.GetPlayerController().transform.localScale.x;
		x /= 1.4f;
		Vector3 vector = base.SourceProjectile.transform.position;
		Collider2D lastCollidedWith = base.SourceProjectile.HitboxController.LastCollidedWith;
		if ((bool)lastCollidedWith)
		{
			vector = lastCollidedWith.ClosestPoint(vector);
		}
		float num = CDGHelper.DistanceBetweenPts(base.SourceProjectile.transform.position, vector);
		if ((m_checkType != 0) ? (num <= m_critDistance * x) : (num >= m_critDistance * x))
		{
			if (base.SourceProjectile.ActualCritChance < 100f)
			{
				base.SourceProjectile.ActualCritChance += 100f;
				m_critWasApplied = true;
			}
		}
		else if (m_critWasApplied && base.SourceProjectile.ActualCritChance >= 100f)
		{
			base.SourceProjectile.ActualCritChance -= 100f;
			m_critWasApplied = false;
		}
	}
}
