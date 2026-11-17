public class Spoons_Ability : BaseAbility_RL, IAttack, IAbility
{
	private float m_fireAngle = 48f;

	protected override float TellIntroAnimSpeed => 1f;

	protected virtual float Spoon_Anim_Setter => 1.75f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => Spoon_Anim_Setter;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => Spoon_Anim_Setter;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => Spoon_Anim_Setter;

	protected override float AttackAnimExitDelay => 0.45f;

	protected override float ExitAnimSpeed => Spoon_Anim_Setter;

	protected override float ExitAnimExitDelay => 0f;

	protected override void FireProjectile()
	{
		if (!string.IsNullOrEmpty(ProjectileName))
		{
			Projectile_RL projectile = ProjectileManager.FireProjectile(m_abilityController.gameObject, ProjectileName, ProjectileOffset, matchFacing: true, m_fireAngle);
			m_abilityController.InitializeProjectile(projectile);
			ApplyAbilityCosts();
		}
	}
}
