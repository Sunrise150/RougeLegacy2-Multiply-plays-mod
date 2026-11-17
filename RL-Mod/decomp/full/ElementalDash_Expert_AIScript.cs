public class ElementalDash_Expert_AIScript : ElementalDash_Basic_AIScript
{
	protected override int m_shoot_TotalShots => 3;

	protected override float m_shoot_TotalShotDuration => 0.75f;

	protected override bool m_voidWall_CreateSideWalls => true;
}
