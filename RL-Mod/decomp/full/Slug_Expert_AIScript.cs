public class Slug_Expert_AIScript : Slug_Basic_AIScript
{
	protected override int m_verticalShot_RepeatAttackPattern => 1;

	protected override int m_verticalShot_TotalShotSpread => 1;

	protected override float m_verticalShot_SpeedMod => 1.25f;

	protected override int m_verticalShot_SideBulletAngleOffset => 25;
}
