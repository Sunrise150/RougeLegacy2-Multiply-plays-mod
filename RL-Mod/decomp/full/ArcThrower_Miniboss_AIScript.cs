public class ArcThrower_Miniboss_AIScript : ArcThrower_Basic_AIScript
{
	protected override bool m_shoot_Explosive_Bullets => true;

	protected override float m_spray_ProjectileDelay => 0.075f;

	protected override float m_spray_ProjectileAmount => 10f;

	protected override float m_spray_TiltSpeed => 15f;

	protected override float m_spray_LowAngle => 15f;

	protected override float m_spray_SpeedMod => 1.25f;
}
