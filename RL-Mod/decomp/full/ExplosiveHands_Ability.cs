using UnityEngine;

public class ExplosiveHands_Ability : BaseAbility_RL, IAttack, IAbility
{
	[SerializeField]
	private string m_altSpellCast;

	[SerializeField]
	private bool m_projectileMatchFacing = true;

	[SerializeField]
	private float m_fireAngle;

	private bool m_useAltCast;

	private float m_timeSinceLastCast;

	protected override float TellIntroAnimSpeed => 3f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 3f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 2f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 2f;

	protected override float AttackAnimExitDelay => 0.25f;

	protected override float ExitAnimSpeed => 1f;

	protected override float ExitAnimExitDelay => 0f;

	public override string AbilityTellIntroName
	{
		get
		{
			if (m_useAltCast)
			{
				return m_altSpellCast;
			}
			return base.AbilityTellIntroName;
		}
	}

	public override void PreCastAbility()
	{
		if (Time.time - m_timeSinceLastCast < 1f)
		{
			m_useAltCast = !m_useAltCast;
		}
		else
		{
			m_useAltCast = false;
		}
		m_timeSinceLastCast = Time.time;
		base.PreCastAbility();
	}

	protected override void FireProjectile()
	{
		if (!string.IsNullOrEmpty(ProjectileName))
		{
			m_firedProjectile = ProjectileManager.FireProjectile(m_abilityController.gameObject, ProjectileName, ProjectileOffset, m_projectileMatchFacing, m_fireAngle);
			m_abilityController.InitializeProjectile(m_firedProjectile);
			ApplyAbilityCosts();
		}
	}
}
