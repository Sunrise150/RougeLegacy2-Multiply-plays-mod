using System.Collections;
using UnityEngine;

public class DualBlades_Ability : BaseAbility_RL, IAttack, IAbility
{
	[SerializeField]
	private string m_projectileNameRight;

	[Space(10f)]
	[SerializeField]
	private string m_secondProjectileName;

	[SerializeField]
	private Vector2 m_secondProjectileOffset;

	[SerializeField]
	private string m_thirdProjectileName;

	[SerializeField]
	private Vector2 m_thirdProjectileOffset;

	private const int NUM_ATTACKS = 3;

	private string m_abilityTellIntroRight;

	private float m_canAttackAgainCounter;

	private int m_attackCount;

	protected override float TellIntroAnimSpeed => 1.45f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 10f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 10f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 1.25f;

	protected override float AttackAnimExitDelay => 0.05f;

	protected override float ExitAnimSpeed => 1f;

	protected override float ExitAnimExitDelay => 0f;

	public override string ProjectileName => m_attackCount switch
	{
		1 => m_secondProjectileName, 
		2 => m_thirdProjectileName, 
		_ => base.ProjectileName, 
	};

	public override Vector2 ProjectileOffset => m_attackCount switch
	{
		1 => m_secondProjectileOffset, 
		2 => m_thirdProjectileOffset, 
		_ => m_projectileOffset, 
	};

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[4] { m_projectileName, m_projectileNameRight, m_secondProjectileName, m_thirdProjectileName };
	}

	protected override void Awake()
	{
		base.Awake();
		if (m_abilityTellIntroName != null)
		{
			m_abilityTellIntroRight = m_abilityTellIntroName.Replace("Left", "Right");
		}
	}

	public override void PreCastAbility()
	{
		m_attackCount = 0;
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
			if (m_attackCount < 3)
			{
				m_animator.SetTrigger("ContinueCombo");
			}
			else
			{
				m_animator.SetTrigger("Change_Ability_Anim");
			}
		}
		else
		{
			m_animator.SetTrigger("Change_Ability_Anim");
		}
		PerformTurnAnimCheck();
	}

	protected override void FireProjectile()
	{
		base.FireProjectile();
		m_attackCount++;
		if (m_attackCount >= 3 && m_firedProjectile.ActualCritChance < 100f)
		{
			m_firedProjectile.ActualCritChance += 100f;
		}
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		if ((bool)m_firedProjectile)
		{
			m_firedProjectile.gameObject.SetActive(value: false);
			m_firedProjectile = null;
		}
		base.StopAbility(abilityInterrupted);
	}
}
