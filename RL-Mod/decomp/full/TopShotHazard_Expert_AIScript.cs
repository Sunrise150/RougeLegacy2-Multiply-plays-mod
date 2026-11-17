public class TopShotHazard_Expert_AIScript : TopShotHazard_Basic_AIScript
{
	protected override bool m_fireBullet_TargetPlayer => true;

	protected override float m_fireBullet_AdditionalSpreadBullets => 2f;

	protected override bool m_spreadShot_TargetPlayer => true;

	protected override float m_spreadShot_InitialAngle => 90f;

	protected override float m_spreadShot_SpreadAngle => 15f;

	protected override float m_spreadShot_AdditionalSpreadBullets => 0f;

	protected override float m_spreadShot_ShotLoop => 5f;

	protected override float m_spreadShot_ShotLoopDelay => 0.35f;
}
