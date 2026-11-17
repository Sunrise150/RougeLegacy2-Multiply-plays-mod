using System.Collections;
using UnityEngine;

public class TimeLimit_FairyRule : FairyRule
{
	[SerializeField]
	protected int m_timeLimit = 10;

	public override string Description => string.Format(LocalizationManager.GetString("LOC_ID_FAIRY_RULE_TIME_LIMIT_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), m_timeLimit);

	public virtual string TimeRemainingDescription
	{
		get
		{
			if (TimeRemaining <= 0f)
			{
				return string.Format(LocalizationManager.GetString("LOC_ID_FAIRY_RULE_TIME_LIMIT_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), 0);
			}
			return string.Format(LocalizationManager.GetString("LOC_ID_FAIRY_RULE_TIME_LIMIT_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), (int)TimeRemaining);
		}
	}

	public override FairyRuleID ID => FairyRuleID.TimeLimit;

	public override bool LockChestAtStart => false;

	public float TimeElapsed { get; private set; }

	public float TimeRemaining => Mathf.Clamp(Mathf.CeilToInt((float)m_timeLimit - TimeElapsed), 0, m_timeLimit);

	public override void RunRule(FairyRoomController fairyRoomController)
	{
		TimeElapsed = 0f;
		base.State = FairyRoomState.Running;
		StartCoroutine(RunTimer(fairyRoomController));
	}

	private IEnumerator RunTimer(FairyRoomController fairyRoomController)
	{
		while (TimeElapsed <= (float)m_timeLimit)
		{
			bool flag = true;
			foreach (FairyRoomRuleEntry fairyRoomRuleEntry in fairyRoomController.FairyRoomRuleEntries)
			{
				if (!(fairyRoomRuleEntry.FairyRule == this) && fairyRoomRuleEntry.FairyRule.State != FairyRoomState.Passed)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				SetIsPassed();
				StopAllCoroutines();
				yield break;
			}
			TimeElapsed += Time.deltaTime;
			yield return null;
		}
		SetIsFailed();
	}

	public override void StopRule()
	{
		base.StopRule();
		StopAllCoroutines();
	}

	public override void ResetRule()
	{
		TimeElapsed = 0f;
		base.ResetRule();
	}
}
