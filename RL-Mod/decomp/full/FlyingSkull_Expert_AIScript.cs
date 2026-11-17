public class FlyingSkull_Expert_AIScript : FlyingSkull_Basic_AIScript
{
	protected override bool m_shoot_ShootNear => true;

	protected override bool m_shoot_ShootFar => true;

	protected override int m_shoot_TotalShots => 2;

	protected override bool m_shoot_ShootMirror => true;
}
