using TMPro;
using UnityEngine;

public class ChallengeOmniUIScoreboardEntry : MonoBehaviour
{
	[SerializeField]
	private ClassType m_scoreboardClassType;

	[SerializeField]
	private TMP_Text m_titleText;

	[SerializeField]
	private TMP_Text m_valueText;

	[SerializeField]
	private bool m_isTotalScore;

	[SerializeField]
	private bool m_isTrophyEntry;

	public void UpdateEntry(ChallengeType challengeType)
	{
		ChallengeObj challenge = ChallengeManager.GetChallenge(challengeType);
		if (challenge == null)
		{
			return;
		}
		if (m_isTotalScore)
		{
			m_valueText.text = challenge.TotalHighScore.ToString();
			return;
		}
		if (m_isTrophyEntry)
		{
			ChallengeTrophyRank challengeTrophyRank = ChallengeManager.GetChallengeTrophyRank(challengeType);
			int challengeSoulsRewarded = Souls_EV.GetChallengeSoulsRewarded(challengeType, challengeTrophyRank);
			int num = 0;
			string @string;
			switch (challengeTrophyRank)
			{
			default:
				@string = LocalizationManager.GetString("LOC_ID_CHALLENGE_UI_BRONZE_REWARD_1", isFemale: false);
				num = Souls_EV.GetChallengeSoulsRewarded(challengeType, ChallengeTrophyRank.Bronze) - challengeSoulsRewarded;
				break;
			case ChallengeTrophyRank.Bronze:
				@string = LocalizationManager.GetString("LOC_ID_CHALLENGE_UI_SILVER_REWARD_1", isFemale: false);
				num = Souls_EV.GetChallengeSoulsRewarded(challengeType, ChallengeTrophyRank.Silver) - challengeSoulsRewarded;
				break;
			case ChallengeTrophyRank.Silver:
				@string = LocalizationManager.GetString("LOC_ID_CHALLENGE_UI_GOLD_REWARD_1", isFemale: false);
				num = Souls_EV.GetChallengeSoulsRewarded(challengeType, ChallengeTrophyRank.Gold) - challengeSoulsRewarded;
				break;
			case ChallengeTrophyRank.Gold:
				@string = LocalizationManager.GetString("LOC_ID_CHALLENGE_UI_MAX_REWARD_1", isFemale: false);
				num = Souls_EV.GetChallengeSoulsRewarded(challengeType, ChallengeTrophyRank.Gold);
				break;
			}
			m_titleText.text = string.Format(@string, num);
			return;
		}
		ClassData classData = ClassLibrary.GetClassData(m_scoreboardClassType);
		if (classData != null)
		{
			if (SkillTreeLogicHelper.IsClassUnlocked(m_scoreboardClassType))
			{
				m_titleText.text = LocalizationManager.GetString(classData.PassiveData.Title, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
				m_valueText.text = ChallengeManager.GetChallengeClassHighScore(challengeType, m_scoreboardClassType).ToString();
			}
			else
			{
				m_titleText.text = "???????";
				m_valueText.text = "0";
			}
		}
	}
}
