public class FlyingHammer_Miniboss_AIScript : FlyingHammer_Basic_AIScript
{
	protected override bool m_shockwave_IsLarge => true;

	protected override float m_shockwave_Attack_TurnSpeed => 100f;

	protected override float m_shockwave_initialProjectileDelay => 0f;

	protected override float m_shockwave_exitProjectileDelay => 0f;

	protected override int m_shockwave_projectileSpawnAmount => 3;

	protected override int m_shockwave_projectileSpawnLoopCount => 5;
}
