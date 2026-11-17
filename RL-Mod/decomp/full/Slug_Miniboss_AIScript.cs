using UnityEngine;

public class Slug_Miniboss_AIScript : Slug_Basic_AIScript
{
	protected override Vector2 IdleDuration => new Vector2(0.5f, 0.5f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.75f, 0.75f);

	protected override float m_WalkTowards_ForceIdle => 0f;

	protected override int m_trail_ProjectileAmount => 3;

	protected override int m_verticalShot_TotalShotSpread => 3;

	protected override float m_verticalShot_SpeedMod => 2.5f;

	protected override int m_verticalShot_RepeatAttackPattern => 5;

	protected override float m_verticalShot_RepeatAttackPatternDelay => 0.25f;
}
