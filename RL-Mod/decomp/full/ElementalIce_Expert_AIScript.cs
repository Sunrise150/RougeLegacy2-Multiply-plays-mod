public class ElementalIce_Expert_AIScript : ElementalIce_Basic_AIScript
{
	protected override bool m_shoot_CentreShot => true;

	protected override int m_shoot_TotalSideShots => 2;

	protected override float m_spinAttack_TimesShotDelay => 0.15f;

	protected override int m_spinAttack_TotalShots => 1;

	protected override int m_spinAttack_ShotPatternLoops => 7;

	protected override float m_spinAttack_projectile_RandomSpread => 180f;
}
