using TMPro;
using UnityEngine;

public class TotemWindowEntry : MonoBehaviour
{
	private enum TotemEntryType
	{
		Stat,
		Header,
		Footer
	}

	[SerializeField]
	private TotemEntryType m_entryType;

	[SerializeField]
	private ClassType m_classType;

	[SerializeField]
	private TMP_Text m_titleText;

	[SerializeField]
	private TMP_Text m_valueText;

	[SerializeField]
	private bool m_isBonusType;

	private bool m_classUnlocked;

	public void OnEnable()
	{
		if (m_entryType == TotemEntryType.Stat && ClassLibrary.GetClassData(m_classType) != null)
		{
			m_classUnlocked = SkillTreeLogicHelper.IsClassUnlocked(m_classType);
		}
		if (!m_isBonusType)
		{
			UpdateClassTitle();
			UpdateClassValue();
		}
		else
		{
			UpdateBonusTitle();
			UpdateBonusValue();
		}
	}

	private void UpdateClassTitle()
	{
		if (m_entryType == TotemEntryType.Stat)
		{
			if (m_classUnlocked)
			{
				string title = ClassLibrary.GetClassData(m_classType).PassiveData.Title;
				title = LocalizationManager.GetString(title, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
				m_titleText.text = title;
			}
			else
			{
				m_titleText.text = "???????";
			}
		}
	}

	private void UpdateClassValue()
	{
		if (m_entryType == TotemEntryType.Stat)
		{
			if (m_classUnlocked)
			{
				MasteryBonusType value = MasteryBonusType.None;
				if (Mastery_EV.MasteryBonusTypeTable.TryGetValue(m_classType, out value))
				{
					float value2 = 0f;
					if (Mastery_EV.MasteryBonusAmountTable.TryGetValue(value, out value2))
					{
						int classMasteryRank = SaveManager.PlayerSaveData.GetClassMasteryRank(m_classType);
						m_valueText.text = classMasteryRank.ToString();
					}
					else
					{
						m_valueText.text = value.ToString() + " NOT FOUND IN AMOUNT TABLE.";
					}
				}
				else
				{
					m_valueText.text = value.ToString() + " NOT FOUND IN TYPE TABLE.";
				}
			}
			else
			{
				m_valueText.text = "???";
			}
		}
		else if (m_entryType == TotemEntryType.Footer)
		{
			int totalMasteryRank = Mastery_EV.GetTotalMasteryRank();
			m_valueText.text = totalMasteryRank.ToString();
			if (totalMasteryRank >= 150)
			{
				StoreAPIManager.GiveAchievement(AchievementType.UnlockHighMastery, StoreType.All);
			}
		}
	}

	private void UpdateBonusTitle()
	{
		if (m_entryType != 0)
		{
			return;
		}
		if (m_classUnlocked)
		{
			MasteryBonusType value = MasteryBonusType.None;
			if (Mastery_EV.MasteryBonusTypeTable.TryGetValue(m_classType, out value))
			{
				if (Mastery_EV.MasteryBonusLocIDTable.TryGetValue(value, out var value2))
				{
					m_titleText.text = LocalizationManager.GetString(value2, isFemale: false);
				}
				else
				{
					m_titleText.text = value.ToString() + " NOT FOUND IN LOC ID TABLE.";
				}
			}
			else
			{
				m_titleText.text = value.ToString() + " NOT FOUND IN TYPE TABLE.";
			}
		}
		else
		{
			m_titleText.text = "???????";
		}
	}

	private void UpdateBonusValue()
	{
		if (m_entryType == TotemEntryType.Stat)
		{
			if (m_classUnlocked)
			{
				MasteryBonusType value = MasteryBonusType.None;
				if (Mastery_EV.MasteryBonusTypeTable.TryGetValue(m_classType, out value))
				{
					float value2 = 0f;
					if (Mastery_EV.MasteryBonusAmountTable.TryGetValue(value, out value2))
					{
						float num = (float)SaveManager.PlayerSaveData.GetClassMasteryRank(m_classType) * value2;
						if (CDGHelper.IsPercent(value2))
						{
							num = Mathf.CeilToInt(num * 100f);
							m_valueText.text = "+" + string.Format(LocalizationManager.GetString("LOC_ID_GENERAL_UI_PERCENT_1", isFemale: false), num);
						}
						else
						{
							m_valueText.text = "+" + num;
						}
					}
					else
					{
						m_valueText.text = value.ToString() + " NOT FOUND IN AMOUNT TABLE.";
					}
				}
				else
				{
					m_valueText.text = value.ToString() + " NOT FOUND IN TYPE TABLE.";
				}
			}
			else
			{
				m_valueText.text = "???";
			}
		}
		else if (m_entryType == TotemEntryType.Footer)
		{
			float totalMasteryBonus = Mastery_EV.GetTotalMasteryBonus(MasteryBonusType.RuneWeight_Up);
			if (totalMasteryBonus != (float)(int)totalMasteryBonus)
			{
				totalMasteryBonus = Mathf.CeilToInt(totalMasteryBonus * 100f);
				m_valueText.text = "+" + totalMasteryBonus + string.Format(LocalizationManager.GetString("LOC_ID_GENERAL_UI_PERCENT_1", isFemale: false), totalMasteryBonus);
			}
			else
			{
				m_valueText.text = "+" + totalMasteryBonus;
			}
		}
	}
}
