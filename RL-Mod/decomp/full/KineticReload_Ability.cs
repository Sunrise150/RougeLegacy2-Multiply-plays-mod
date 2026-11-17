public class KineticReload_Ability : BaseAbility_RL, ITalent, IAbility
{
	protected override float TellIntroAnimSpeed => 1f;

	protected override float TellIntroAnimExitDelay => 1f;

	protected override float TellAnimSpeed => 1f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 1.1f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 1.1f;

	protected override float AttackAnimExitDelay => 0f;

	protected override float ExitAnimSpeed => 2f;

	protected override float ExitAnimExitDelay => 0f;

	protected override void OnEnterTellIntroLogic()
	{
		base.OnEnterTellIntroLogic();
		if (!string.IsNullOrEmpty(ProjectileName))
		{
			m_firedProjectile = ProjectileManager.FireProjectile(m_abilityController.gameObject, ProjectileName, ProjectileOffset);
			m_abilityController.InitializeProjectile(m_firedProjectile);
			m_firedProjectile.transform.SetParent(m_abilityController.PlayerController.transform, worldPositionStays: true);
			ApplyAbilityCosts();
		}
	}

	protected override void FireProjectile()
	{
		if ((bool)m_firedProjectile)
		{
			m_firedProjectile.FlagForDestruction();
		}
		m_abilityController.ResetAbilityAmmo(CastAbilityType.Weapon);
	}
}
