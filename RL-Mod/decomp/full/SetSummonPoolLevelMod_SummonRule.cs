using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class SetSummonPoolLevelMod_SummonRule : BaseSummonRule
{
	[SerializeField]
	private bool m_setLevelToRoom;

	[SerializeField]
	private int m_levelMod;

	public override SummonRuleType RuleType => SummonRuleType.SetSummonPoolLevelMod;

	public override string RuleLabel => "Set Summon Pool Level Mod";

	public override IEnumerator RunSummonRule()
	{
		if (m_setLevelToRoom)
		{
			while (!PlayerManager.IsInstantiated || PlayerManager.GetCurrentPlayerRoom() == null)
			{
				yield return null;
			}
			base.SummonController.SummonLevelOverride = PlayerManager.GetCurrentPlayerRoom().Level;
		}
		else
		{
			base.SummonController.SummonLevelOverride = m_levelMod;
		}
		base.IsRuleComplete = true;
	}
}
