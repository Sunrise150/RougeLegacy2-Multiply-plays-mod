public class Fireball_Advanced_AIScript : Fireball_Basic_AIScript
{
	protected override bool m_dropsFireballsWhileWalking => true;

	protected override float m_timeBetweenWalkTowardFireballDrops => 1.5f;

	protected override bool m_dropsFireballsDuringDashAttack => false;
}
