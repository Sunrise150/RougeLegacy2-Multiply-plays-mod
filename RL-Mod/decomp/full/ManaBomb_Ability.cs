public class ManaBomb_Ability : BaseAbility_RL, ITalent, IAbility
{
	protected override void FireProjectile()
	{
		base.FireProjectile();
		m_abilityController.PlayerController.SetVelocityY(16f, additive: false);
		m_abilityController.PlayerController.CharacterJump.ResetBrakeForce();
	}
}
