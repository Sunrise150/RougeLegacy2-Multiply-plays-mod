public class Zombie_Advanced_AIScript : Zombie_Basic_AIScript
{
	protected override float m_tunnel_moveSpeed => 4f;

	protected override float Lunge_Count => 2f;

	protected override float Delay_Between_Lunges => 0.15f;
}
