public class Fireball_Miniboss_AIScript : Fireball_Basic_AIScript
{
	protected override bool m_dropsFireballsWhileWalking => true;

	protected override bool m_dropsFireballsDuringDashAttack => true;

	protected override float m_timeBetweenWalkTowardFireballDrops => 1.5f;

	protected override float m_dash_Exit_ForceIdle => 0.5f;
}
