public class Fireball_Expert_AIScript : Fireball_Basic_AIScript
{
	protected override bool m_dropsFireballsWhileWalking => true;

	protected override bool m_dropsFireballsDuringDashAttack => true;

	protected override float m_timeBetweenWalkTowardFireballDrops => 1.5f;
}
