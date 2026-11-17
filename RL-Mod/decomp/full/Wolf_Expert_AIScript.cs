using UnityEngine;

public class Wolf_Expert_AIScript : Wolf_Basic_AIScript
{
	protected override Vector2 m_jumpPower => new Vector2(18f, 26f);

	protected override Vector2 m_dash_AttackHold_ForwardPower => new Vector2(34f, 12f);
}
