using System;
using TMPro;
using UnityEngine;

public class EndGameStatsStatsEntry : MonoBehaviour
{
	[SerializeField]
	private EndGameStatsEntryType m_statsEntryType;

	[SerializeField]
	private TMP_Text m_amount;

	[SerializeField]
	private TMP_Text m_score;

	private static string[] m_letterGradeArray = new string[20]
	{
		"C", "C", "C+", "C+", "C+", "C++", "C++", "C++", "B", "B",
		"B+", "B+", "B++", "B++", "A", "A+", "A++", "S", "S+", "S++"
	};

	private static string[] m_ratingArray = new string[4] { "Amateur Adventurer", "Enthusiastic Explorer", "Intrepid Traveler", "A True Rogue" };

	public void UpdateStat(bool animate)
	{
		string text = null;
		int num = CalculateScore(m_statsEntryType);
		switch (m_statsEntryType)
		{
		case EndGameStatsEntryType.HeirsLost:
		{
			int goldSpent = SaveManager.PlayerSaveData.TimesDied;
			text = goldSpent.ToString();
			break;
		}
		case EndGameStatsEntryType.EnemiesDefeated:
		{
			int goldSpent = SaveManager.PlayerSaveData.EnemiesKilled;
			text = goldSpent.ToString();
			break;
		}
		case EndGameStatsEntryType.GoldSpent:
		{
			int goldSpent = SaveManager.PlayerSaveData.GoldSpent;
			text = goldSpent + " g";
			break;
		}
		case EndGameStatsEntryType.TimePlayed:
			text = TimeSpan.FromSeconds((float)SaveManager.PlayerSaveData.SecondsPlayed + GameTimer.TotalSessionAccumulatedTime).ToString("hh\\:mm\\:ss");
			break;
		case EndGameStatsEntryType.FinalScore:
			m_score.text = num.ToString();
			return;
		case EndGameStatsEntryType.Rating:
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
			m_score.text = "+" + num;
		}
	}

	private int CalculateScore(EndGameStatsEntryType statType)
	{
		switch (statType)
		{
		case EndGameStatsEntryType.HeirsLost:
		{
			int num10 = 25000;
			int num11 = 625;
			int num12 = SaveManager.PlayerSaveData.TimesDied * num11;
			return Mathf.Clamp(num10 - num12, 1000, num10);
		}
		case EndGameStatsEntryType.EnemiesDefeated:
		{
			int num6 = 25000;
			int num7 = (SaveManager.PlayerSaveData.TimesDied + 1) * 30;
			int enemiesKilled = SaveManager.PlayerSaveData.EnemiesKilled;
			int num8 = 150;
			int num9 = Mathf.Max(num7 - enemiesKilled, 0) * num8;
			return Mathf.Clamp(num6 - num9, 1000, num6);
		}
		case EndGameStatsEntryType.GoldSpent:
		{
			int num4 = 25000;
			int num5 = (int)((float)SaveManager.PlayerSaveData.GoldSpent * 0.5f);
			return Mathf.Clamp(num4 - num5, 1000, num4);
		}
		case EndGameStatsEntryType.TimePlayed:
		{
			int num2 = 25000;
			int num3 = (int)(((float)SaveManager.PlayerSaveData.SecondsPlayed + GameTimer.TotalSessionAccumulatedTime) * 0.00027777778f * 1650f);
			return Mathf.Clamp(num2 - num3, 1000, num2);
		}
		case EndGameStatsEntryType.FinalScore:
		{
			int num = 0;
			{
				foreach (EndGameStatsEntryType value in Enum.GetValues(typeof(EndGameStatsEntryType)))
				{
					if (value != statType)
					{
						num += CalculateScore(value);
					}
				}
				return num;
			}
		}
		default:
			return 0;
		}
	}

	private string CalculateLetterGrade()
	{
		int num = 100000;
		int num2 = CalculateScore(EndGameStatsEntryType.FinalScore);
		int num3 = m_letterGradeArray.Length;
		int num4 = Mathf.Clamp((int)((float)num2 / (float)num * (float)num3), 0, num3 - 1);
		return m_letterGradeArray[num4];
	}

	private string CalculateRating()
	{
		int num = 100000;
		int num2 = CalculateScore(EndGameStatsEntryType.FinalScore);
		int num3 = m_ratingArray.Length;
		int num4 = Mathf.Clamp((int)((float)num2 / (float)num * (float)num3), 0, num3 - 1);
		return m_ratingArray[num4];
	}
}
