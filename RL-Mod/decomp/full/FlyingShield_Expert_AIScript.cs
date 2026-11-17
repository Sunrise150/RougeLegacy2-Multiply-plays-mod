public class FlyingShield_Expert_AIScript : FlyingShield_Basic_AIScript
{
	protected override bool m_hasTailSpin => true;

	protected override bool m_hasTailRam => true;

	protected override float m_spinMove_Attack_MoveSpeed => 24f;

	protected override float m_spinMove_Attack_FlameDrops => 0f;
}
