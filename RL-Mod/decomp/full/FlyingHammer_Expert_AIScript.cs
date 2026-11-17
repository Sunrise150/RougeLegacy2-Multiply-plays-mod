using UnityEngine;

public class FlyingHammer_Expert_AIScript : FlyingHammer_Basic_AIScript
{
	protected override Vector2 m_fireballSpeedMod => new Vector2(0.25f, 2f);

	protected override int m_projectileSpawnAmount => 3;

	protected override bool m_shockwave_IsLarge => true;

	protected override float m_shockwave_Attack_TurnSpeed => 100f;

	protected override float m_shockwave_initialProjectileDelay => 0f;

	protected override float m_shockwave_exitProjectileDelay => 0f;

	protected override int m_shockwave_projectileSpawnAmount => 1;

	protected override int m_shockwave_projectileSpawnLoopCount => 12;

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
	}
}
