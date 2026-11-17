public class DancingBoss_Advanced_AIScript : DancingBoss_Basic_AIScript
{
	protected override bool m_advancedAttacks => true;

	protected override float m_bomb_projectileDelay => 0.25f;

	protected override int m_bomb_projectileAmount => 5;

	protected override int m_verticalWave_BounceProjectileCount => 8;

	protected override int m_horizontalWave_BounceProjectileCount => 6;
}
