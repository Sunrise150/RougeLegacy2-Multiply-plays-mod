public class FlyingSword_Expert_AIScript : FlyingSword_Basic_AIScript
{
	protected override float m_thrust_Attack_TurnRate => 55f;

	protected override float m_thrustFast_DashAttackAmount => 1f;

	protected override bool m_thrust_SpawnProjectilesAtEnd => true;

	protected override int m_vertSpin_Attack_TotalLoops => 4;
}
