using System;
using UnityEngine;

public class HPAmountCheckSkillCritProjectileLogic : BaseProjectileLogic
{
	private enum AmountCheckType
	{
		None,
		Greater,
		GreaterOrEqual,
		Less,
		LessOrEqual
	}

	[SerializeField]
	private AmountCheckType m_amountCheckType;

	[SerializeField]
	private float m_hpAmount;

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
		if (!obj.CompareTag("Enemy"))
		{
			return;
		}
		EnemyController component = obj.GetComponent<EnemyController>();
		if (!component)
		{
			return;
		}
		if (HPConditionMet(component))
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

	private bool HPConditionMet(BaseCharacterController character)
	{
		return m_amountCheckType switch
		{
			AmountCheckType.Greater => character.CurrentHealth > (float)character.ActualMaxHealth * m_hpAmount, 
			AmountCheckType.GreaterOrEqual => character.CurrentHealth >= (float)character.ActualMaxHealth * m_hpAmount, 
			AmountCheckType.Less => character.CurrentHealth < (float)character.ActualMaxHealth * m_hpAmount, 
			AmountCheckType.LessOrEqual => character.CurrentHealth <= (float)character.ActualMaxHealth * m_hpAmount, 
			_ => true, 
		};
	}
}
