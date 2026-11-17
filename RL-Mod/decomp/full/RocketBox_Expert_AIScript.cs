public class RocketBox_Expert_AIScript : RocketBox_Basic_AIScript
{
	protected override int m_shoot_NumberOfBullets => 6;

	protected override bool m_shoot_VerticalBullets => false;

	protected override bool m_shoot_HomingBullets => true;
}
