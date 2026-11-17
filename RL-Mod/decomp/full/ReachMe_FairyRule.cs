using System.Collections;

public class ReachMe_FairyRule : FairyRule
{
	private Interactable m_chestInteractable;

	public override FairyRuleID ID => FairyRuleID.ReachMe;

	public override string Description => "LOC_ID_FAIRY_RULE_REACH_1";

	public override bool LockChestAtStart => false;

	public override void RunRule(FairyRoomController fairyRoomController)
	{
		base.State = FairyRoomState.Running;
		m_chestInteractable = fairyRoomController.Chest.Interactable;
	}

	private IEnumerator RunTimer()
	{
		while (true)
		{
			if (m_chestInteractable.IsPlayerInTrigger)
			{
				SetIsPassed();
				StopAllCoroutines();
			}
			yield return null;
		}
	}

	public override void StopRule()
	{
		base.StopRule();
		StopAllCoroutines();
	}
}
