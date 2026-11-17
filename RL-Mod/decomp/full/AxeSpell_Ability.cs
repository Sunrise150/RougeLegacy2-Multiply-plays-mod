public class AxeSpell_Ability : BaseAbility_RL, ISpell, IAbility
{
	private float m_fireAngle = 85f;

	private float m_fireAngle2 = 75f;

	private float m_fireAngle3 = 65f;

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
		if (ProjectileName != null)
		{
			m_firedProjectile = ProjectileManager.FireProjectile(m_abilityController.gameObject, ProjectileName, ProjectileOffset, matchFacing: true, m_fireAngle);
			m_abilityController.InitializeProjectile(m_firedProjectile);
			m_firedProjectile = ProjectileManager.FireProjectile(m_abilityController.gameObject, ProjectileName, ProjectileOffset, matchFacing: true, m_fireAngle2);
			m_abilityController.InitializeProjectile(m_firedProjectile);
			m_firedProjectile = ProjectileManager.FireProjectile(m_abilityController.gameObject, ProjectileName, ProjectileOffset, matchFacing: true, m_fireAngle3);
			m_abilityController.InitializeProjectile(m_firedProjectile);
			ApplyAbilityCosts();
		}
	}
}
