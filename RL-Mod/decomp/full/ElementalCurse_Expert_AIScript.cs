public class ElementalCurse_Expert_AIScript : ElementalCurse_Basic_AIScript
{
	protected override int m_shoot_TotalShots => 3;

	protected override float m_shoot_TotalShotDuration => 1.125f;

	protected override int m_spinAttack_TotalShots => 1;

	protected override int m_spinAttack_ShotPatternLoops => 5;
}
