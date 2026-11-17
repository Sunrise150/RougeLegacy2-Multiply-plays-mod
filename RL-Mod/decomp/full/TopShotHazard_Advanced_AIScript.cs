public class TopShotHazard_Advanced_AIScript : TopShotHazard_Basic_AIScript
{
	protected override bool m_fireBullet_TargetPlayer => true;

	protected override float m_fireBullet_AdditionalSpreadBullets => 1f;

	protected override float m_fireBullet_ShotLoop => 1f;

	protected override float m_fireBullet_ShotLoopDelay => 0.25f;

	protected override float m_spreadShot_ShotLoop => 3f;
}
