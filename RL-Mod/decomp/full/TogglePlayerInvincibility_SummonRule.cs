using System.Collections;
using UnityEngine;

public class TogglePlayerInvincibility_SummonRule : BaseSummonRule
{
	[SerializeField]
	private bool m_setPlayerInvincible;

	public override SummonRuleType RuleType => SummonRuleType.TogglePlayerInvincibility;

	public override string RuleLabel => "Toggle Player Invincibility";

	public override IEnumerator RunSummonRule()
	{
		PlayerManager.GetPlayerController().TakesNoDamage = m_setPlayerInvincible;
		base.SummonController.PlayerInvincibilityActive = true;
		base.IsRuleComplete = true;
		yield break;
	}
}
