public class NPCDialogueEntry
{
	public string LocID;

	public NPCDialogueCondition[] UnlockConditions;

	public string TitleLocID;

	public string ConditionValue;

	public NPCDialogueEntry(string locID, params NPCDialogueCondition[] unlockConditions)
	{
		LocID = locID;
		UnlockConditions = unlockConditions;
	}

	public NPCDialogueEntry(string locID, string titleLocID, string conditionValue, params NPCDialogueCondition[] unlockConditions)
	{
		LocID = locID;
		TitleLocID = titleLocID;
		ConditionValue = conditionValue;
		UnlockConditions = unlockConditions;
	}

	public bool IsUnlocked()
	{
		if (UnlockConditions != null)
		{
			NPCDialogueCondition[] unlockConditions = UnlockConditions;
			foreach (NPCDialogueCondition condition in unlockConditions)
			{
				if (!NPCType_RL.IsNPCDialogueConditionUnlocked(this, condition))
				{
					return false;
				}
			}
		}
		return true;
	}
}
