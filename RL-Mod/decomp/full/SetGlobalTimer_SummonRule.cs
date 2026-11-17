using System.Collections;
using UnityEngine;

public class SetGlobalTimer_SummonRule : BaseSummonRule
{
	[SerializeField]
	private bool m_startTimer;

	[SerializeField]
	private bool m_stopTimer;

	[SerializeField]
	private bool m_displayTimer;

	[SerializeField]
	private bool m_hideTimer;

	[SerializeField]
	private bool m_resetTimer;

	[SerializeField]
	private float m_slowDuration;

	public override Color BoxColor => Color.yellow;

	public override SummonRuleType RuleType => SummonRuleType.SetGlobalTimer;

	public override string RuleLabel => "Set Global Timer";

	public override IEnumerator RunSummonRule()
	{
		if (m_resetTimer)
		{
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.ResetGlobalTimer, null, null);
		}
		if (m_startTimer)
		{
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.StartGlobalTimer, null, null);
		}
		else if (m_stopTimer)
		{
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.StopGlobalTimer, null, null);
		}
		if (m_displayTimer)
		{
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayGlobalTimer, null, null);
		}
		else if (m_hideTimer)
		{
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.HideGlobalTimer, null, null);
		}
		base.IsRuleComplete = true;
		yield break;
	}
}
