using UnityEngine;

public class FlyingAxe_Miniboss_AIScript : FlyingAxe_Basic_AIScript
{
	protected override Vector2 RandomFollowOffsetX => new Vector2(-3f, 3f);

	protected override Vector2 RandomFollowOffsetY => new Vector2(1f, 3f);

	protected override float m_sideSpin_Attack_TurnSpeed => 95f;

	protected override float m_sideSpin_Attack_MovementSpeed => 12.5f;

	protected override float m_sideSpin_Exit_AttackCD => 3f;

	protected override float m_vertSpin_Attack_MovementSpeed => 0f;

	protected override float m_vertSpin_Attack_ChaseDuration => 2.75f;

	protected override bool m_vertSpin_Attack_FireBulletsWhileSpinning => true;
}
