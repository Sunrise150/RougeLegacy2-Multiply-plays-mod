using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class WaitForBroadcast_SummonRule : BaseSummonRule
{
	private bool m_continueRule;

	private Action<MonoBehaviour, EventArgs> m_onSummonRuleBroadcast;

	public override SummonRuleType RuleType => SummonRuleType.WaitForBroadcast;

	public override string RuleLabel => "Wait For Broadcast";

	public override void Initialize(SummonRuleController summonController)
	{
		base.Initialize(summonController);
		m_onSummonRuleBroadcast = OnSummonRuleBroadcast;
	}

	public override IEnumerator RunSummonRule()
	{
		m_continueRule = false;
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.SummonRuleBroadcast, m_onSummonRuleBroadcast);
		while (!m_continueRule)
		{
			yield return null;
		}
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.SummonRuleBroadcast, m_onSummonRuleBroadcast);
	}

	private void OnSummonRuleBroadcast(object sender, EventArgs args)
	{
		m_continueRule = true;
	}
}
