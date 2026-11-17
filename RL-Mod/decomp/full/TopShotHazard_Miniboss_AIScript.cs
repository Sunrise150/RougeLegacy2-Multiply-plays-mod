public class TopShotHazard_Miniboss_AIScript : TopShotHazard_Basic_AIScript
{
	protected override float SHOT_TRIGGER_WIDTH => 20f;

	protected override bool m_snapToFloor => true;

	protected override bool m_fireBullet_TargetPlayer => true;

	protected override float m_fireBullet_AdditionalSpreadBullets => 0f;

	protected override float m_fireBullet_ShotLoop => 7f;

	protected override float m_fireBullet_ShotLoopDelay => 0.225f;
}
