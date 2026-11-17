public class ArcThrower_Advanced_AIScript : ArcThrower_Basic_AIScript
{
	protected override float m_shoot_NumberMediumShots => 3f;

	protected override float m_spray_ProjectileDelay => 0.125f;

	protected override float m_spray_ProjectileAmount => 5f;

	protected override float m_spray_TiltSpeed => 19.5f;

	protected override float m_spray_LowAngle => 10f;
}
