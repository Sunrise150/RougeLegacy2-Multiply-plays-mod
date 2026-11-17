using TMPro;
using UnityEngine;

public class ChallengeCompleteStatsEntry : MonoBehaviour
{
	[SerializeField]
	private ChallengeCompleteStatsEntryType m_statsEntryType;

	[SerializeField]
	private TMP_Text m_amount;

	[SerializeField]
	private TMP_Text m_score;

	private static string[] m_ratingArray = new string[4] { "Amateur Adventurer", "Enthusiastic Explorer", "Intrepid Traveler", "A True Rogue" };

	public void UpdateStat(ChallengeTrophyRank goalRank)
	{
		string text = null;
		float num = ChallengeManager.CalculateActiveChallengeScore(m_statsEntryType);
		switch (m_statsEntryType)
		{
		case ChallengeCompleteStatsEntryType.BaseScore:
			m_score.text = 2500.ToString();
			return;
		case ChallengeCompleteStatsEntryType.HitsTaken:
			text = ChallengeManager.HitsTaken.ToString();
			break;
		case ChallengeCompleteStatsEntryType.Resolve:
			text = Mathf.RoundToInt(SaveManager.PlayerSaveData.GetTotalRelicResolveCost() * 100f).ToString();
			break;
		case ChallengeCompleteStatsEntryType.Timer:
			text = GlobalTimerHUDController.GetTimerString();
			break;
		case ChallengeCompleteStatsEntryType.ParTime:
		{
			float num2 = ChallengeManager.ActiveChallenge.ChallengeData.ParTime;
			if (ChallengeManager.ActiveChallenge.ScoringType == ChallengeScoringType.Platform)
			{
				switch (goalRank)
				{
				case ChallengeTrophyRank.Bronze:
					num2 += 10f;
					break;
				case ChallengeTrophyRank.Silver:
					num2 += 3f;
					break;
				case ChallengeTrophyRank.Gold:
					num2 += 0f;
					break;
				}
			}
			int num3 = (int)(num2 % 3600f / 60f);
			int num4 = (int)(num2 % 60f);
			int num5 = (int)((num2 - (float)(int)num2) * 100f);
			text = string.Format(LocalizationManager.GetString("LOC_ID_CHALLENGE_UI_END_PAR_TIME_1", isFemale: false), num3, num4, num5);
			break;
		}
		case ChallengeCompleteStatsEntryType.HandicapMod:
			text = ChallengeManager.ActiveChallenge.EquippedLevel.ToString();
			break;
		case ChallengeCompleteStatsEntryType.FinalScore:
			m_score.text = num.ToString();
			return;
		case ChallengeCompleteStatsEntryType.Rating:
			m_amount.text = CalculateLetterGrade();
			m_score.text = CalculateRating();
			return;
		}
		if (m_amount != null)
		{
			m_amount.text = text;
		}
		if (m_score != null)
		{
			if (m_statsEntryType == ChallengeCompleteStatsEntryType.HandicapMod)
			{
				m_score.text = "x" + num.ToString("F1", LocalizationManager.GetCurrentCultureInfo());
			}
			else
			{
				m_score.text = "+" + num;
			}
		}
	}

	private string CalculateLetterGrade()
	{
		return ChallengeManager.GetChallengeLetterGrade(ChallengeManager.ActiveChallenge.ChallengeType, ChallengeManager.CalculateActiveChallengeScore(ChallengeCompleteStatsEntryType.FinalScore));
	}

	private string CalculateRating()
	{
		float num = ChallengeManager.CalculateActiveChallengeScore(ChallengeCompleteStatsEntryType.FinalScore);
		int num2 = m_ratingArray.Length;
		int num3 = Mathf.Clamp((int)(num / 50000f * (float)num2), 0, num2 - 1);
		return m_ratingArray[num3];
	}
}
