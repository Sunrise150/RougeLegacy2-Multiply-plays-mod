using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class DisplayObjectiveComplete_SummonRule : BaseSummonRule
{
	[SerializeField]
	private ObjectiveCompleteHUDType m_hudType;

	[SerializeField]
	private InsightType m_insightType;

	[SerializeField]
	private bool m_insightDiscovered;

	[SerializeField]
	private string m_bossLocIDOverride;

	private InsightObjectiveCompleteHUDEventArgs m_insightCompleteEventArgs = new InsightObjectiveCompleteHUDEventArgs(InsightType.None, discovered: false);

	private BossObjectiveCompleteHUDEventArgs m_bossCompleteEventArgs = new BossObjectiveCompleteHUDEventArgs(EnemyType.None, EnemyRank.None);

	public override SummonRuleType RuleType => SummonRuleType.DisplayObjectiveComplete;

	public override string RuleLabel => "Display Objective Complete";

	public override IEnumerator RunSummonRule()
	{
		ObjectiveCompleteHUDEventArgs objectiveCompleteHUDEventArgs = null;
		switch (m_hudType)
		{
		case ObjectiveCompleteHUDType.Insight:
			m_insightCompleteEventArgs.Initialize(m_insightType, m_insightDiscovered);
			objectiveCompleteHUDEventArgs = m_insightCompleteEventArgs;
			break;
		case ObjectiveCompleteHUDType.Boss:
			if (!string.IsNullOrEmpty(m_bossLocIDOverride))
			{
				string @string = LocalizationManager.GetString(m_bossLocIDOverride, isFemale: false);
				m_bossCompleteEventArgs.Initialize(EnemyType.None, EnemyRank.None, 5f, @string);
			}
			objectiveCompleteHUDEventArgs = m_bossCompleteEventArgs;
			break;
		}
		if (objectiveCompleteHUDEventArgs != null)
		{
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayObjectiveCompleteHUD, null, objectiveCompleteHUDEventArgs);
		}
		base.IsRuleComplete = true;
		yield break;
	}
}
