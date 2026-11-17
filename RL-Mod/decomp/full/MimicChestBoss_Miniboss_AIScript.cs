using UnityEngine;

public class MimicChestBoss_Miniboss_AIScript : MimicChestBoss_Basic_AIScript
{
	protected override float m_dashAttackSpeed => 25f;

	protected override Vector2 m_dashAttackDuration => new Vector2(2f, 2f);

	protected override Vector2 JumpHeight => new Vector2(17f, 20f);
}
