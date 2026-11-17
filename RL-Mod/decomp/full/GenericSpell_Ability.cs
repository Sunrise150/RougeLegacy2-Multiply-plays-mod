using UnityEngine;

public class GenericSpell_Ability : BaseAbility_RL, ISpell, IAbility
{
	[SerializeField]
	private bool m_projectileMatchFacing = true;

	[SerializeField]
	private float m_fireAngle;

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

	protected override void FireProjectile()
	{
		if (!string.IsNullOrEmpty(ProjectileName))
		{
			m_firedProjectile = ProjectileManager.FireProjectile(m_abilityController.gameObject, ProjectileName, ProjectileOffset, m_projectileMatchFacing, m_fireAngle);
			m_abilityController.InitializeProjectile(m_firedProjectile);
			if (!m_projectileMatchFacing && m_firedProjectile.transform.parent == m_abilityController.PlayerController.Pivot.transform)
			{
				m_firedProjectile.transform.SetParent(m_abilityController.PlayerController.transform, worldPositionStays: false);
			}
			ApplyAbilityCosts();
		}
	}
}
