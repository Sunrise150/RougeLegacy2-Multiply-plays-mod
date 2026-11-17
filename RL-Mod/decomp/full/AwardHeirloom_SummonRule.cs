using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class AwardHeirloom_SummonRule : BaseSummonRule
{
	[SerializeField]
	private HeirloomType m_heirloomType;

	public override SummonRuleType RuleType => SummonRuleType.AwardHeirloom;

	public override string RuleLabel => "Award Heirloom";

	public override IEnumerator RunSummonRule()
	{
		SaveManager.PlayerSaveData.SetHeirloomLevel(m_heirloomType, 1, additive: false, broadcast: true);
		PlayerManager.GetPlayerController().InitializeAbilities();
		base.IsRuleComplete = true;
		yield break;
	}
}
