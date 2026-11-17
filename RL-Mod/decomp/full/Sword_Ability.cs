using UnityEngine;

public class Sword_Ability : BaseAbility_RL, IAttack, IAbility
{
	protected const string BREAKABLES_PROJECTILE_NAME = "SwordWeaponBreakablesProjectile";

	[SerializeField]
	private bool m_spawnBreakablesProjectile;

	private Projectile_RL m_breakablesProjectile;

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
		m_projectileNameArray = new string[2] { m_projectileName, "SwordWeaponBreakablesProjectile" };
	}

	protected override void FireProjectile()
	{
		base.FireProjectile();
		if (m_spawnBreakablesProjectile)
		{
			m_breakablesProjectile = ProjectileManager.FireProjectile(m_abilityController.gameObject, "SwordWeaponBreakablesProjectile", ProjectileOffset);
			m_abilityController.InitializeProjectile(m_breakablesProjectile);
		}
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		if (m_firedProjectile != null)
		{
			m_firedProjectile.FlagForDestruction();
			m_firedProjectile = null;
		}
		if (m_breakablesProjectile != null)
		{
			m_breakablesProjectile.FlagForDestruction();
			m_breakablesProjectile = null;
		}
		base.StopAbility(abilityInterrupted);
	}
}
