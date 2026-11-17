public class RocketBox_Miniboss_AIScript : RocketBox_Basic_AIScript
{
	protected override int m_shoot_NumberOfBullets => 1;

	protected override bool m_shoot_VerticalBullets => true;

	protected override bool m_shoot_HomingBullets => true;

	protected override float m_explosion_WarningDuration => 1.5f;

	protected override float m_explosion_AttackCD => 7f;
}
