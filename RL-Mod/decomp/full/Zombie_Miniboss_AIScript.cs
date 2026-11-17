using UnityEngine;

public class Zombie_Miniboss_AIScript : Zombie_Basic_AIScript
{
	protected override Vector2 WalkTowardsDuration => new Vector2(0.75f, 1f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.75f, 1f);

	protected override float DigDownAnimSpeed => 0.9f;

	protected override float DigUpAnimSpeed => 0.9f;

	protected override Vector2 swing_Dash_AttackSpeed => new Vector2(17f, 0f);

	protected override float m_swing_Dash_AttackTime => 0.25f;

	protected override float m_swing_AttackCD => 0f;

	protected override Vector2 RandomFollowOffsetX => new Vector2(-15f, 15f);

	protected override Vector2 RandomFollowOffsetY => new Vector2(-9f, 9f);
}
