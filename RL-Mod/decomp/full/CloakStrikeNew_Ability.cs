public class CloakStrikeNew_Ability : BaseAbility_RL, ITalent, IAbility
{
	protected override float TellIntroAnimSpeed => 2.25f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 10f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 10f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 1f;

	protected override float AttackAnimExitDelay => 0.15f;

	protected override float ExitAnimSpeed => 1f;

	protected override float ExitAnimExitDelay => 0f;
}
