using UnityEngine;

public class HeavyBow_Ability : Bow_Ability
{
	protected override float TellIntroAnimSpeed => 0.9f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 1f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 1f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 1f;

	protected override float AttackAnimExitDelay => 0.15f;

	protected override float ExitAnimSpeed => 0.8f;

	protected override float ExitAnimExitDelay => 0f;

	protected override float GravityReduction => 0f;

	protected override Vector2 BowPushbackAmount => new Vector2(17f, 18f);
}
