using System.Collections;
using RL_Windows;
using UnityEngine;

public class RunDialogue_SummonRule : BaseSummonRule
{
	[SerializeField]
	private string m_titleTextLocID;

	[SerializeField]
	private string m_bodyTextLocID;

	public override SummonRuleType RuleType => SummonRuleType.RunDialogue;

	public override string RuleLabel => "Run Dialogue";

	public override IEnumerator RunSummonRule()
	{
		DialogueManager.StartNewDialogue();
		DialogueManager.AddDialogue(m_titleTextLocID, m_bodyTextLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		while (WindowManager.GetIsWindowOpen(WindowID.Dialogue))
		{
			yield return null;
		}
		base.IsRuleComplete = true;
	}
}
