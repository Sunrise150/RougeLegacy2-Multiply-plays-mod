using System;
using System.Collections;
using RLAudio;
using RL_Windows;
using UnityEngine;

[Serializable]
public class EndChallenge_SummonRule : BaseSummonRule
{
	[SerializeField]
	private ChallengeType m_challengeType;

	public override SummonRuleType RuleType => SummonRuleType.EndChallenge;

	public override Color BoxColor => Color.red;

	public override string RuleLabel => "End Challenge";

	public override IEnumerator RunSummonRule()
	{
		MusicManager.StopMusic();
		float delayTime = Time.time + 3f;
		while (Time.time < delayTime)
		{
			yield return null;
		}
		if (!WindowManager.GetIsWindowLoaded(WindowID.ChallengeComplete))
		{
			WindowManager.LoadWindow(WindowID.ChallengeComplete);
		}
		WindowManager.SetWindowIsOpen(WindowID.ChallengeComplete, isOpen: true);
		while (WindowManager.GetIsWindowOpen(WindowID.ChallengeComplete))
		{
			yield return null;
		}
		base.SummonController.StopArena(arenaComplete: true);
		base.IsRuleComplete = true;
		if (ChallengeManager.ActiveChallenge != null && ChallengeManager.ActiveChallenge.ChallengeType == ChallengeType.TutorialPurified)
		{
			ChallengeTrophyRank challengeTrophyRank = ChallengeManager.GetChallengeTrophyRank(ChallengeType.TutorialPurified, checkForSisyphusTrophy: false);
			if (challengeTrophyRank == ChallengeTrophyRank.Gold && !SaveManager.ModeSaveData.HasGoldSisyphusTrophy)
			{
				SaveManager.ModeSaveData.HasGoldSisyphusTrophy = true;
			}
			else if (challengeTrophyRank == ChallengeTrophyRank.Silver && !SaveManager.ModeSaveData.HasSilverSisyphusTrophy)
			{
				SaveManager.ModeSaveData.HasSilverSisyphusTrophy = true;
			}
			else if (challengeTrophyRank == ChallengeTrophyRank.Bronze && !SaveManager.ModeSaveData.HasBronzeSisyphusTrophy)
			{
				SaveManager.ModeSaveData.HasBronzeSisyphusTrophy = true;
			}
		}
		ChallengeManager.ReturnToDriftHouseWithTransition();
	}
}
