using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class DebugTrace_SummonRule : BaseSummonRule
{
	[SerializeField]
	private string m_debugString;

	public override Color BoxColor => Color.yellow;

	public override SummonRuleType RuleType => SummonRuleType.DebugTrace;

	public override string RuleLabel => "Debug Trace";

	public override IEnumerator RunSummonRule()
	{
		base.IsRuleComplete = true;
		yield break;
	}
}
