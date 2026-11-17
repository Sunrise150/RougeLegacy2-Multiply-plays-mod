public class FlyingShield_Miniboss_AIScript : FlyingShield_Basic_AIScript
{
	protected override float m_spinMove_Attack_MoveSpeed => 27f;

	protected override float m_spinMove_Attack_Duration => 4f;

	protected override bool m_hasTailSpin => true;

	protected override bool m_hasTailRam => true;
}
