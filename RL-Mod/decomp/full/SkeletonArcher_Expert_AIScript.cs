using UnityEngine;

public class SkeletonArcher_Expert_AIScript : SkeletonArcher_Basic_AIScript
{
	protected override bool m_shoot_ExtraArrowCount => true;

	protected override Vector2 m_jump_Power => new Vector2(10f, 32f);
}
