using UnityEngine;

public class Zombie_Expert_AIScript : Zombie_Basic_AIScript
{
	protected override float m_tunnel_moveSpeed => 6f;

	protected override float Lunge_Count => 3f;

	protected override float Delay_Between_Lunges => 0.15f;

	protected override Vector2 swing_Dash_AttackSpeed => new Vector2(17f, 0f);
}
