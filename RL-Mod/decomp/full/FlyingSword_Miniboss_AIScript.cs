public class FlyingSword_Miniboss_AIScript : FlyingSword_Basic_AIScript
{
	protected override float m_thrust_Attack_TurnRate => 45f;

	protected override float m_thrustFast_DashAttackAmount => 2f;

	protected override bool m_thrust_SpawnMinibossProjectilesAtEnd => true;

	protected override int m_vertSpin_Attack_TotalLoops => 5;
}
