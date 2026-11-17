public class SwordBeam_Ability : Sword_Ability
{
	private const string BEAM_PROJECTILE_NAME = "SwordBeamWeaponProjectile";

	protected override float TellIntroAnimSpeed => 2.25f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 10f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 10f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 1f;

	protected override float AttackAnimExitDelay => 0.15f;

	protected override float ExitAnimSpeed => 1f;

	protected override float ExitAnimExitDelay => 0f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[3] { m_projectileName, "SwordWeaponBreakablesProjectile", "SwordBeamWeaponProjectile" };
	}

	protected override void FireProjectile()
	{
		base.FireProjectile();
		Projectile_RL projectile = ProjectileManager.FireProjectile(m_abilityController.gameObject, "SwordBeamWeaponProjectile", ProjectileOffset);
		m_abilityController.InitializeProjectile(projectile);
	}
}
