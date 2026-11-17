using UnityEngine;

public class Ghost_Miniboss_AIScript : Ghost_Basic_AIScript
{
	protected override Vector2 RandomFollowOffsetX => new Vector2(-5f, 5f);

	protected override Vector2 RandomFollowOffsetY => new Vector2(-4f, 4f);

	protected override float m_scare_TellIntroAndHold_Delay => 1.25f;

	protected override float m_scare_Exit_ForceIdle => 5.5f;

	protected override bool m_scare_fireCardinalProjectiles => false;

	protected override bool m_scare_fireDiagonalProjectiles => false;

	protected override bool m_scare_fireMinibossProjectiles => true;

	protected override bool m_scare_isMiniboss => true;
}
