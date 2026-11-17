using System;

public class DontTakeDamage_FairyRule : DontGetHit_FairyRule
{
	public override string Description => "LOC_ID_FAIRY_RULE_LOSE_NO_HEALTH_1";

	public override FairyRuleID ID => FairyRuleID.DontTakeDamage;

	public override bool LockChestAtStart => false;

	protected override void OnPlayerTakeDamage(object sender, EventArgs eventArgs)
	{
		if ((eventArgs as CharacterHitEventArgs).DamageTaken > 0f)
		{
			SetIsFailed();
		}
	}
}
