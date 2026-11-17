public class RicochetSpikes_Ability : BaseAbility_RL, ISpell, IAbility
{
	private Projectile_RL m_spike1;

	private Projectile_RL m_spike2;

	protected override void FireProjectile()
	{
		if (ProjectileName != null)
		{
			m_spike1 = ProjectileManager.FireProjectile(m_abilityController.gameObject, ProjectileName, ProjectileOffset, matchFacing: true, 45f);
			m_spike2 = ProjectileManager.FireProjectile(m_abilityController.gameObject, ProjectileName, ProjectileOffset, matchFacing: true, -45f);
			m_abilityController.InitializeProjectile(m_spike1);
			m_abilityController.InitializeProjectile(m_spike2);
			ApplyAbilityCosts();
		}
	}
}
