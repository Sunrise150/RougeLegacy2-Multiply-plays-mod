using UnityEngine;

public class AimedAbilityFast_RL : AimedAbility_RL, ISpell, IAbility
{
	protected override float TellIntroAnimSpeed => 3.25f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 5f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 1f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 1f;

	protected override float AttackAnimExitDelay => 0.1f;

	protected override float ExitAnimSpeed => 1.25f;

	protected override float ExitAnimExitDelay => 0f;

	protected virtual float GravityReduction => 0.425f;

	protected virtual Vector2 BowPushbackAmount => new Vector2(0f, 11.5f);

	public override Vector2 PushbackAmount => BowPushbackAmount;

	public override void Initialize(CastAbility_RL abilityController, CastAbilityType castAbilityType)
	{
		m_gravityReductionModWhenAiming = GravityReduction;
		base.Initialize(abilityController, castAbilityType);
	}
}
