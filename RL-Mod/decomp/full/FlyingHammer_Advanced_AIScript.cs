using UnityEngine;

public class FlyingHammer_Advanced_AIScript : FlyingHammer_Basic_AIScript
{
	protected override int m_projectileSpawnAmount => 2;

	protected override Vector2 m_fireballSpeedMod => new Vector2(0.25f, 1.5f);

	protected override float m_shockwave_initialProjectileDelay => 0f;

	protected override float m_shockwave_exitProjectileDelay => 0f;

	protected override int m_shockwave_projectileSpawnAmount => 1;

	protected override int m_shockwave_projectileSpawnLoopCount => 10;
}
