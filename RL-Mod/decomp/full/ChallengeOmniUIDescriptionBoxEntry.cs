using UnityEngine;
using UnityEngine.UI;

public class ChallengeOmniUIDescriptionBoxEntry : BaseOmniUIDescriptionBoxEntry<ChallengeOmniUIDescriptionEventArgs, ChallengeOmniUIDescriptionBoxEntry.ChallengeOmniUIDescriptionBoxType>
{
	public enum ChallengeOmniUIDescriptionBoxType
	{
		None,
		Title,
		Description,
		Controls
	}

	[SerializeField]
	private Image m_frame;

	protected override void DisplayDescriptionBox(ChallengeOmniUIDescriptionEventArgs args)
	{
		ChallengeType challengeType = args.ChallengeType;
		ChallengeObj challenge = ChallengeManager.GetChallenge(challengeType);
		if (challenge == null)
		{
			return;
		}
		ChallengeData challengeData = challenge.ChallengeData;
		bool flag = challenge.FoundState > FoundState.NotFound;
		bool flag2 = Challenge_EV.ScarBossRequirementTable.ContainsKey(challengeType) && SaveManager.PlayerSaveData.GetFlag(Challenge_EV.ScarBossRequirementTable[challengeType]);
		switch (m_descriptionType)
		{
		case ChallengeOmniUIDescriptionBoxType.Title:
			if (flag)
			{
				if ((bool)m_iconGO && !m_iconGO.activeSelf)
				{
					m_iconGO.SetActive(value: true);
				}
				if ((bool)m_inactiveIconGO && m_inactiveIconGO.activeSelf)
				{
					m_inactiveIconGO.SetActive(value: false);
				}
				m_icon.sprite = IconLibrary.GetChallengeIcon(challengeType, ChallengeLibrary.ChallengeIconEntryType.Challenge);
			}
			else
			{
				if ((bool)m_iconGO && m_iconGO.activeSelf)
				{
					m_iconGO.SetActive(value: false);
				}
				if ((bool)m_inactiveIconGO && !m_inactiveIconGO.activeSelf)
				{
					m_inactiveIconGO.SetActive(value: true);
				}
				m_icon.sprite = IconLibrary.GetDefaultSprite();
			}
			if ((bool)m_frame)
			{
				switch (ChallengeManager.GetChallengeTrophyRank(challengeType))
				{
				case ChallengeTrophyRank.Bronze:
					m_frame.sprite = IconLibrary.GetMiscIcon(MiscIconType.OmniUIEntry_Frame_Bronze);
					break;
				case ChallengeTrophyRank.Silver:
					m_frame.sprite = IconLibrary.GetMiscIcon(MiscIconType.OmniUIEntry_Frame_Silver);
					break;
				case ChallengeTrophyRank.Gold:
					m_frame.sprite = IconLibrary.GetMiscIcon(MiscIconType.OmniUIEntry_Frame_Gold);
					break;
				default:
					m_frame.sprite = IconLibrary.GetMiscIcon(MiscIconType.OmniUIEntry_Frame_Default);
					break;
				}
			}
			if (flag)
			{
				m_titleText.text = LocalizationManager.GetString(challengeData.Title, isFemale: false);
			}
			else
			{
				m_titleText.text = "???";
			}
			break;
		case ChallengeOmniUIDescriptionBoxType.Description:
			if (flag)
			{
				m_titleText.text = LocalizationManager.GetString(challengeData.Description, isFemale: false);
			}
			else if (flag2)
			{
				m_titleText.text = LocalizationManager.GetString(challengeData.Hint, isFemale: false);
			}
			else
			{
				m_titleText.text = LocalizationManager.GetString(challengeData.BossRequirementHint, isFemale: false);
			}
			break;
		case ChallengeOmniUIDescriptionBoxType.Controls:
		{
			if (!flag)
			{
				m_titleText.text = "";
				break;
			}
			ClassType classType = challenge.ChallengeData.ClassOverride;
			if (classType == ClassType.None)
			{
				classType = SaveManager.PlayerSaveData.CurrentCharacter.ClassType;
			}
			float num = ChallengeManager.GetChallengeClassHighScore(challengeType, classType);
			string @string = LocalizationManager.GetString(ClassLibrary.GetClassData(classType).PassiveData.Title, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			float num2 = challengeData.ScalingHandicap * (float)challenge.EquippedLevel;
			if (challenge.ChallengeData.ScoringType == ChallengeScoringType.Battle)
			{
				num2 *= 100f;
			}
			string text = LocalizationManager.GetString("LOC_ID_CHALLENGE_UI_END_DEFAULT_TIME_1", isFemale: false);
			float bestTimeWithoutHandicaps = challenge.BestTimeWithoutHandicaps;
			if (bestTimeWithoutHandicaps != float.MaxValue)
			{
				text = ToTimeFormat(bestTimeWithoutHandicaps);
			}
			ChallengeTrophyRank challengeTrophyRank = ChallengeManager.GetChallengeTrophyRank(challengeType);
			string locID = "";
			string text2 = "";
			int challengeSoulsCollected = Souls_EV.GetChallengeSoulsCollected(challengeType);
			int num3 = 0;
			float num4 = 0f;
			string locID2;
			int num5;
			switch (challengeTrophyRank)
			{
			case ChallengeTrophyRank.None:
				locID2 = "LOC_ID_CHALLENGE_UI_BRONZE_REQ_1";
				num5 = challengeData.BronzeReq;
				text2 = "LOC_ID_CHALLENGE_UI_BRONZE_TIME_1";
				num4 = challengeData.ParTime + 10f;
				locID = "LOC_ID_CHALLENGE_UI_BRONZE_REWARD_1";
				num3 = Souls_EV.GetChallengeSoulsRewarded(challengeType, ChallengeTrophyRank.Bronze) - challengeSoulsCollected;
				break;
			case ChallengeTrophyRank.Bronze:
				locID2 = "LOC_ID_CHALLENGE_UI_SILVER_REQ_1";
				num5 = challengeData.SilverReq;
				text2 = "LOC_ID_CHALLENGE_UI_SILVER_TIME_1";
				num4 = challengeData.ParTime + 3f;
				locID = "LOC_ID_CHALLENGE_UI_SILVER_REWARD_1";
				num3 = Souls_EV.GetChallengeSoulsRewarded(challengeType, ChallengeTrophyRank.Silver) - challengeSoulsCollected;
				break;
			default:
				locID2 = "LOC_ID_CHALLENGE_UI_GOLD_REQ_1";
				num5 = challengeData.GoldReq;
				text2 = "LOC_ID_CHALLENGE_UI_GOLD_TIME_1";
				num4 = challengeData.ParTime + 0f;
				switch (challengeTrophyRank)
				{
				case ChallengeTrophyRank.Silver:
					locID = "LOC_ID_CHALLENGE_UI_GOLD_REWARD_1";
					num3 = Souls_EV.GetChallengeSoulsRewarded(challengeType, ChallengeTrophyRank.Gold) - challengeSoulsCollected;
					break;
				case ChallengeTrophyRank.Gold:
					locID = "LOC_ID_CHALLENGE_UI_MAX_REWARD_1";
					num3 = Souls_EV.GetChallengeSoulsRewarded(challengeType, ChallengeTrophyRank.Gold);
					break;
				}
				break;
			}
			string text3 = string.Format(LocalizationManager.GetString(locID, isFemale: false, disableWordScramble: true), num3);
			string text4 = string.Format(LocalizationManager.GetString(text2, isFemale: false, disableWordScramble: true), ToTimeFormat(num4));
			if (challengeType == ChallengeType.Tutorial || challengeType == ChallengeType.TutorialPurified)
			{
				m_titleText.text = string.Format(LocalizationManager.GetString(challengeData.Controls, isFemale: false), ChallengeManager.GetTotalTrophyCount() - 1);
				break;
			}
			m_titleText.text = string.Format(LocalizationManager.GetString(challengeData.Controls, isFemale: false), challengeData.BaseHandicap + 15, num2, LocalizationManager.GetString(locID2, isFemale: false), num5, "", challenge.TotalHighScore, @string, num, ChallengeManager.GetChallengeLetterGrade(challengeType, num), text, text3, text4);
			break;
		}
		}
	}

	private string ToTimeFormat(float elapsedSeconds)
	{
		int num = (int)(elapsedSeconds / 3600f);
		int num2 = (int)(elapsedSeconds % 3600f / 60f);
		int num3 = (int)(elapsedSeconds % 60f);
		int num4 = (int)((elapsedSeconds - (float)(int)elapsedSeconds) * 100f);
		if (num == 0)
		{
			return $"{num2:D2}:{num3:D2}:{num4:D2}";
		}
		return $"{num}:{num2:D2}:{num3:D2}:{num4:D2}";
	}
}
