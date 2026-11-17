public class ScreenSlice_Ability : BaseAbility_RL, ISpell, IAbility
{
	protected override float TellIntroAnimSpeed => 2f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 2f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 1.1f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 1.1f;

	protected override float AttackAnimExitDelay => 0f;

	protected override float ExitAnimSpeed => 2f;

	protected override float ExitAnimExitDelay => 0f;
}
