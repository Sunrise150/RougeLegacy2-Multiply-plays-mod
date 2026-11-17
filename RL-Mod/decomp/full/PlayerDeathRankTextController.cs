using System.Collections;
using Sigtrap.Relays;
using TMPro;
using UnityEngine;

public class PlayerDeathRankTextController : MonoBehaviour
{
	[SerializeField]
	private TMP_Text m_rankText;

	[SerializeField]
	private TMP_Text m_rankCounterText;

	private int m_currentRank;

	private bool m_tallyStarted;

	public Relay<int, int> XPGainedRelay = new Relay<int, int>();

	public Relay<int> LevelGainedRelay = new Relay<int>();

	public Relay XPGainStartRelay = new Relay();

	public Relay XPGainCompleteRelay = new Relay();

	public TMP_Text CounterText => m_rankCounterText;

	public TMP_Text Text => m_rankText;

	public bool IsComplete { get; private set; }

	public void UpdateMessage()
	{
		ClassType classType = SaveManager.PlayerSaveData.CurrentCharacter.ClassType;
		m_currentRank = SaveManager.PlayerSaveData.GetClassMasteryRank(classType);
		int classXP = SaveManager.PlayerSaveData.GetClassXP(classType);
		int num = Mastery_EV.XP_REQUIRED[m_currentRank];
		string arg = "NULL CLASS";
		ClassData classData = ClassLibrary.GetClassData(classType);
		if (classData != null)
		{
			arg = LocalizationManager.GetString(classData.PassiveData.Title, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
		if (Mastery_EV.IsMaxMasteryRank(classType, 0, includeRunXP: false))
		{
			int num2 = Mastery_EV.XP_REQUIRED[Mastery_EV.GetMaxMasteryRank()];
			m_rankCounterText.text = num2 + "/" + num2;
			m_rankText.text = string.Format(LocalizationManager.GetString("LOC_ID_RANK_MAX_RANK_FORMATTER_1", isFemale: false), arg, m_currentRank);
		}
		else
		{
			m_rankText.text = string.Format(LocalizationManager.GetString("LOC_ID_RANK_RANK_FORMATTER_1", isFemale: false), arg, m_currentRank);
			m_rankCounterText.text = classXP + "/" + num;
		}
	}

	public void StartTally()
	{
		IsComplete = false;
		m_tallyStarted = true;
		StartCoroutine(RankCoroutineV2());
	}

	private IEnumerator RankCoroutineV2()
	{
		IsComplete = false;
		if (Mastery_EV.IsMaxMasteryRank(SaveManager.PlayerSaveData.CurrentCharacter.ClassType, 0, includeRunXP: false))
		{
			RankComplete();
			yield break;
		}
		XPGainStartRelay.Dispatch();
		ClassType classType = SaveManager.PlayerSaveData.CurrentCharacter.ClassType;
		m_currentRank = SaveManager.PlayerSaveData.GetClassMasteryRank(classType);
		if (m_currentRank <= Mastery_EV.GetMaxMasteryRank())
		{
			float elapsedDuration = 1f / 60f;
			_ = Time.unscaledTime;
			int currentXP = SaveManager.PlayerSaveData.GetClassXP(classType);
			int endXP = currentXP + SaveManager.PlayerSaveData.RunAccumulatedXP;
			int xpRequired = Mastery_EV.XP_REQUIRED[m_currentRank];
			string className = "NULL CLASS";
			ClassData classData = ClassLibrary.GetClassData(classType);
			if ((bool)classData)
			{
				className = LocalizationManager.GetString(classData.PassiveData.Title, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			}
			m_rankText.text = string.Format(LocalizationManager.GetString("LOC_ID_RANK_RANK_FORMATTER_1", isFemale: false), className, m_currentRank);
			int num = currentXP;
			int xpTally = num;
			m_rankCounterText.text = num + "/" + xpRequired;
			ApplyXPToSaveFile();
			while (xpTally < endXP)
			{
				float startTime = Time.unscaledTime;
				int num2 = (int)(1000f * elapsedDuration);
				xpTally += num2;
				if (xpTally >= xpRequired)
				{
					if (currentXP + xpTally >= Mastery_EV.XP_REQUIRED[Mastery_EV.GetMaxMasteryRank()])
					{
						RankUpAnim(1);
						break;
					}
					m_currentRank++;
					xpRequired = Mastery_EV.XP_REQUIRED[m_currentRank];
					m_rankText.text = string.Format(LocalizationManager.GetString("LOC_ID_RANK_RANK_FORMATTER_1", isFemale: false), className, m_currentRank);
					RankUpAnim(1);
				}
				m_rankCounterText.text = xpTally + "/" + xpRequired;
				XPGainedRelay.Dispatch(xpTally, xpRequired);
				while (Time.unscaledTime < startTime + elapsedDuration)
				{
					yield return null;
				}
			}
		}
		RankComplete();
	}

	private void RankComplete()
	{
		IsComplete = true;
		m_tallyStarted = false;
		XPGainCompleteRelay.Dispatch();
		ClassType classType = SaveManager.PlayerSaveData.CurrentCharacter.ClassType;
		int classMasteryRank = SaveManager.PlayerSaveData.GetClassMasteryRank(classType);
		int classXP = SaveManager.PlayerSaveData.GetClassXP(classType);
		string arg = "NULL CLASS";
		ClassData classData = ClassLibrary.GetClassData(classType);
		if ((bool)classData)
		{
			arg = LocalizationManager.GetString(classData.PassiveData.Title, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
		if (Mastery_EV.IsMaxMasteryRank(classType, 0, includeRunXP: true))
		{
			int num = Mastery_EV.XP_REQUIRED[Mastery_EV.GetMaxMasteryRank()];
			m_rankCounterText.text = num + "/" + num;
			m_rankText.text = string.Format(LocalizationManager.GetString("LOC_ID_RANK_MAX_RANK_FORMATTER_1", isFemale: false), arg, classMasteryRank);
		}
		else
		{
			int num2 = Mastery_EV.XP_REQUIRED[classMasteryRank];
			m_rankCounterText.text = classXP + "/" + num2;
			m_rankText.text = string.Format(LocalizationManager.GetString("LOC_ID_RANK_RANK_FORMATTER_1", isFemale: false), arg, classMasteryRank);
		}
	}

	private void RankUpAnim(int numRanks)
	{
		Vector3 localScale = m_rankText.transform.localScale;
		m_rankText.transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);
		TweenManager.TweenTo_UnscaledTime(m_rankText.transform, 0.25f, Ease.Back.EaseOut, "localScale.x", localScale.x, "localScale.y", localScale.y, "localScale.z", localScale.z);
		localScale = m_rankCounterText.transform.localScale;
		m_rankCounterText.transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);
		TweenManager.TweenTo_UnscaledTime(m_rankCounterText.transform, 0.25f, Ease.Back.EaseOut, "localScale.x", localScale.x, "localScale.y", localScale.y, "localScale.z", localScale.z);
		if (numRanks > 0)
		{
			LevelGainedRelay.Dispatch(m_currentRank);
			EffectManager.PlayEffect(base.gameObject, null, "PlayerRankUp_Effect", m_rankText.transform.position, 0f, EffectStopType.Gracefully);
			DisplayRankUpText(numRanks);
		}
	}

	private void DisplayRankUpText(int numRanks)
	{
		string text = LocalizationManager.GetString("LOC_ID_RANK_RANK_UP_1", isFemale: false) + "\n";
		ClassType classType = SaveManager.PlayerSaveData.CurrentCharacter.ClassType;
		MasteryBonusType value = MasteryBonusType.None;
		if (Mastery_EV.MasteryBonusTypeTable.TryGetValue(classType, out value))
		{
			text = ((!Mastery_EV.MasteryBonusLocIDTable.TryGetValue(value, out var value2)) ? (text + value.ToString() + " NOT FOUND IN LOC ID TABLE.") : (text + LocalizationManager.GetString(value2, isFemale: false)));
			if (Mastery_EV.MasteryBonusAmountTable.TryGetValue(value, out var value3))
			{
				value3 = (float)numRanks * value3;
				if (value3 != (float)(int)value3)
				{
					value3 = Mathf.CeilToInt(value3 * 100f);
					text = text + " +" + string.Format(LocalizationManager.GetString("LOC_ID_GENERAL_UI_PERCENT_1", isFemale: false), value3);
				}
				else
				{
					text = text + " +" + value3;
				}
			}
		}
		else
		{
			text = text + value.ToString() + " NOT FOUND IN TYPE TABLE.";
		}
		TextPopupManager.DisplayTextAtAbsPos(TextPopupType.PlayerRankUp, text, m_rankText.transform.position);
	}

	public void SkipTally()
	{
		if (!IsComplete && m_tallyStarted)
		{
			StopAllCoroutines();
			ClassType classType = SaveManager.PlayerSaveData.CurrentCharacter.ClassType;
			int classMasteryRank = SaveManager.PlayerSaveData.GetClassMasteryRank(classType);
			RankUpAnim(classMasteryRank - m_currentRank);
			RankComplete();
		}
	}

	private void ApplyXPToSaveFile()
	{
		ClassType classType = SaveManager.PlayerSaveData.CurrentCharacter.ClassType;
		SaveManager.PlayerSaveData.SetClassXP(classType, SaveManager.PlayerSaveData.RunAccumulatedXP, additive: true, forceOverride: false, ignoreMaxLevelClamping: false);
		SaveManager.PlayerSaveData.RunAccumulatedXP = 0;
		if (Mastery_EV.GetTotalMasteryRank() >= 150)
		{
			StoreAPIManager.GiveAchievement(AchievementType.UnlockHighMastery, StoreType.All);
		}
	}
}
