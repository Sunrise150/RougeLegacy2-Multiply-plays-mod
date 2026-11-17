public class ArcThrower_Expert_AIScript : ArcThrower_Basic_AIScript
{
	protected override float m_shoot_NumberMediumShots => 3f;

	protected override float m_shoot_NumberHighShots => 3f;

	protected override float m_spray_ProjectileDelay => 0.075f;

	protected override float m_spray_ProjectileAmount => 6f;

	protected override float m_spray_TiltSpeed => 16f;

	protected override float m_spray_LowAngle => 10f;
}
