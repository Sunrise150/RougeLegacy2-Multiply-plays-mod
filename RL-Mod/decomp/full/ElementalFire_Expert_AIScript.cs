public class ElementalFire_Expert_AIScript : ElementalFire_Basic_AIScript
{
	protected override int m_shoot_TotalShots => 12;

	protected override float m_shoot_TotalShotDuration => 1.6f;

	protected override float m_shoot_RandAngleOffset => 28f;

	protected override float m_spinAttack_TimesShotDelay => 0.75f;

	protected override int m_spinAttack_TotalShots => 1;

	protected override int m_spinAttack_ShotPatternLoops => 2;

	protected override float m_spinAttack_projectile_RandomSpread => 0f;
}
