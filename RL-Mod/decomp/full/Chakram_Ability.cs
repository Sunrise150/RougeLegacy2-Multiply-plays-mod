public class Chakram_Ability : BaseAbility_RL, IAttack, IAbility
{
	protected override float TellIntroAnimSpeed => 1f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 10f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 10f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 0.9f;

	protected override float AttackAnimExitDelay => 0.5f;

	protected override float ExitAnimSpeed => 0.85f;

	protected override float ExitAnimExitDelay => 0f;
}
