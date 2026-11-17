using UnityEngine;

public class MimicChestBoss_Advanced_AIScript : MimicChestBoss_Basic_AIScript
{
	protected override Vector2 JumpHeight => new Vector2(28f, 35f);

	protected override int NumCoinsFiredOnLanding => 1;

	protected override bool m_advancedBoss => true;

	protected override int m_verticalShot_TotalShotSpread => 3;

	protected override int m_verticalShot_TotalLoops => 8;

	protected override int m_verticalShot_InitialAngle => 90;

	protected override Vector2 m_verticalShot_RandomAngleAngleOffset => new Vector2(-15f, 6f);

	protected override float m_verticalShot_LoopDelay => 0.7f;

	protected override float m_verticalShot_SpeedMod => 1.15f;
}
