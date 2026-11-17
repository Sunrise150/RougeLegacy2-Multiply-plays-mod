using UnityEngine;

public class MimicChest_Expert_AIScript : MimicChest_Basic_AIScript
{
	protected override Vector2 JumpHeight => new Vector2(16.5f, 16.5f);

	protected override int DeathCoinAttack_CoinSpreadAmount => 3;

	protected override float DeathCoinAttack_AngleSpread => 12f;

	protected override bool m_canDashWake => true;

	protected override float m_dashWakeActivationRange => 5.5f;

	protected override float m_dashWakeAttackOdds => 1f;
}
