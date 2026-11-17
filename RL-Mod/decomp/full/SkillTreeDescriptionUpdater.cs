using System;
using TMPro;
using UnityEngine;

public class SkillTreeDescriptionUpdater : MonoBehaviour
{
	public enum SkillTreeDescriptionType
	{
		Title,
		Description,
		Stat,
		Level,
		Cost,
		GoldOwned,
		CostMultiple,
		Purchase
	}

	protected Color BenefitColor = new Color(1f / 17f, 26f / 51f, 0f);

	protected Color DeficitColor = new Color(0.8392157f, 0f, 0f);

	protected Color OriginalColor = new Color(0.3254902f, 21f / 85f, 14f / 51f);

	public SkillTreeDescriptionType DescriptionType;

	public LocalizationItem DescriptionLocItem;

	public TMP_Text CurrentValue;

	private Action<MonoBehaviour, EventArgs> m_onHighlightedSkillChanged;

	private Action<MonoBehaviour, EventArgs> m_onSkillLevelChanged;

	private Action<MonoBehaviour, EventArgs> m_onGoldChanged;

	private void Awake()
	{
		m_onHighlightedSkillChanged = OnHighlightedSkillChanged;
		m_onSkillLevelChanged = OnSkillLevelChanged;
		m_onGoldChanged = OnGoldChanged;
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.SkillTree_HighlightedSkillChanged, m_onHighlightedSkillChanged);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.SkillLevelChanged, m_onSkillLevelChanged);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.GoldChanged, m_onGoldChanged);
	}

	private void OnDestroy()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.SkillTree_HighlightedSkillChanged, m_onHighlightedSkillChanged);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.SkillLevelChanged, m_onSkillLevelChanged);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.GoldChanged, m_onGoldChanged);
	}

	private void OnHighlightedSkillChanged(MonoBehaviour sender, EventArgs args)
	{
		HighlightedSkillChangedEventArgs highlightedSkillChangedEventArgs = args as HighlightedSkillChangedEventArgs;
		UpdateText(highlightedSkillChangedEventArgs.SkillTreeType);
	}

	private void OnSkillLevelChanged(MonoBehaviour sender, EventArgs args)
	{
		if (args != null)
		{
			SkillLevelChangedEventArgs skillLevelChangedEventArgs = args as SkillLevelChangedEventArgs;
			UpdateText(skillLevelChangedEventArgs.SkillTreeType);
		}
	}

	private void UpdateText(SkillTreeType skillType)
	{
		SkillTreeObj skillTreeObj = SkillTreeManager.GetSkillTreeObj(skillType);
		SkillTreeData skillTreeData = SkillTreeLibrary.GetSkillTreeData(skillType);
		if ((bool)DescriptionLocItem)
		{
			DescriptionLocItem.gameObject.SetActive(value: true);
		}
		if ((bool)CurrentValue)
		{
			CurrentValue.gameObject.SetActive(value: true);
		}
		bool isLocked = skillTreeObj.IsLocked;
		bool isSoulLocked = skillTreeObj.IsSoulLocked;
		bool isLevelLocked = skillTreeObj.IsLevelLocked;
		switch (DescriptionType)
		{
		case SkillTreeDescriptionType.Title:
			if (isLocked)
			{
				DescriptionLocItem.SetString("LOC_ID_SKILL_LOCKED_TITLE_1");
			}
			else if (isSoulLocked)
			{
				DescriptionLocItem.SetString("LOC_ID_SKILL_SOUL_LOCKED_TITLE_1");
			}
			else
			{
				DescriptionLocItem.SetString(skillTreeData.Title);
			}
			break;
		case SkillTreeDescriptionType.Description:
			if (isLocked)
			{
				DescriptionLocItem.SetString("LOC_ID_SKILL_LOCKED_DESCRIPTION_1");
				break;
			}
			if (isSoulLocked)
			{
				string arg = "NULL";
				if (Enum.TryParse<SoulShopType>(skillTreeData.SoulShopTag, out var result))
				{
					arg = LocalizationManager.GetString(SoulShopLibrary.GetSoulShopData(result).Title, isFemale: false);
				}
				DescriptionLocItem.SetString("LOC_ID_SKILL_SOUL_LOCKED_DESCRIPTION_1");
				DescriptionLocItem.TextObj.text = string.Format(DescriptionLocItem.TextObj.text, arg);
				break;
			}
			DescriptionLocItem.SetString(skillTreeData.Description);
			if (skillTreeObj.SkillTreeType == SkillTreeType.Weight_CD_Reduce)
			{
				int num = Mathf.RoundToInt((0.2f + SkillTreeManager.GetSkillTreeObj(SkillTreeType.Weight_CD_Reduce).CurrentStatGain) * 100f);
				DescriptionLocItem.TextObj.text = string.Format(DescriptionLocItem.TextObj.text, num);
			}
			if (isLevelLocked)
			{
				TMP_Text textObj = DescriptionLocItem.TextObj;
				textObj.text = textObj.text + "\n\n" + string.Format(LocalizationManager.GetString("LOC_ID_SKILL_TREE_UI_LEVEL_WARNING_1", isFemale: false), skillTreeObj.UnlockLevel);
			}
			break;
		case SkillTreeDescriptionType.Stat:
			if (!isLocked && !isSoulLocked)
			{
				bool flag = CDGHelper.IsPercent(skillTreeObj.SkillTreeData.FirstLevelStatGain);
				DescriptionLocItem.SetString(skillTreeData.StatTitle);
				float currentStatGain = skillTreeObj.CurrentStatGain;
				string text = (flag ? PercentString(currentStatGain) : ((skillTreeObj.SkillTreeType != SkillTreeType.Gold_Saved_Cap_Up) ? currentStatGain.ToString() : (currentStatGain + 1000f).ToString()));
				float value = skillTreeObj.GetStatGainAtLevel(skillTreeObj.Level + 1) - currentStatGain;
				if (skillTreeObj.Level < skillTreeObj.MaxLevel)
				{
					CurrentValue.text = text + " " + ColoredValueString(value, addBrackets: true, flag, lowerIsBetter: false, hideZero: false);
				}
				else
				{
					CurrentValue.text = text;
				}
			}
			else
			{
				DescriptionLocItem.gameObject.SetActive(value: false);
				CurrentValue.gameObject.SetActive(value: false);
			}
			break;
		case SkillTreeDescriptionType.Level:
			if (!isLocked && !isSoulLocked)
			{
				CurrentValue.text = $"{skillTreeObj.ClampedLevel} / {skillTreeObj.MaxLevel}";
				break;
			}
			DescriptionLocItem.gameObject.SetActive(value: false);
			CurrentValue.gameObject.SetActive(value: false);
			break;
		case SkillTreeDescriptionType.Cost:
			if (!isLocked && !isSoulLocked)
			{
				if (skillTreeObj.Level < skillTreeObj.MaxLevel)
				{
					CurrentValue.text = skillTreeObj.GoldCostWithLevelAppreciation.ToString();
				}
				else
				{
					CurrentValue.text = LocalizationManager.GetString("LOC_ID_GENERAL_UI_NA_1", isFemale: false);
				}
			}
			else
			{
				CurrentValue.text = LocalizationManager.GetString("LOC_ID_GENERAL_UI_NA_1", isFemale: false);
			}
			break;
		case SkillTreeDescriptionType.CostMultiple:
			if (skillTreeObj.MaxLevel <= 1)
			{
				if (base.gameObject.activeSelf)
				{
					base.gameObject.SetActive(value: false);
				}
				break;
			}
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: true);
			}
			if (!isLocked && !isSoulLocked)
			{
				int levelsToAdd2 = Mathf.Clamp(5, 0, skillTreeObj.MaxLevel - skillTreeObj.Level);
				int numLevelsPurchaseableWithGold2 = skillTreeObj.GetNumLevelsPurchaseableWithGold(levelsToAdd2, SaveManager.PlayerSaveData.GoldCollectedIncludingBank);
				if (numLevelsPurchaseableWithGold2 > 0)
				{
					CurrentValue.text = skillTreeObj.GetGoldCostWithAppreciationWhenAddingLevels(numLevelsPurchaseableWithGold2).ToString();
					DescriptionLocItem.TextObj.text = string.Format(LocalizationManager.GetString("LOC_ID_SKILL_TREE_UI_COST_MULTIPLE_1", isFemale: false), numLevelsPurchaseableWithGold2);
				}
				else
				{
					CurrentValue.text = LocalizationManager.GetString("LOC_ID_GENERAL_UI_NA_1", isFemale: false);
					DescriptionLocItem.TextObj.text = string.Format(LocalizationManager.GetString("LOC_ID_SKILL_TREE_UI_COST_MULTIPLE_1", isFemale: false), 0);
				}
			}
			else
			{
				CurrentValue.text = LocalizationManager.GetString("LOC_ID_GENERAL_UI_NA_1", isFemale: false);
			}
			break;
		case SkillTreeDescriptionType.GoldOwned:
			CurrentValue.text = SaveManager.PlayerSaveData.GetActualAvailableGoldString();
			break;
		case SkillTreeDescriptionType.Purchase:
			if (skillTreeObj.ClampedLevel >= skillTreeObj.MaxLevel)
			{
				if (base.gameObject.activeSelf)
				{
					base.gameObject.SetActive(value: false);
				}
				break;
			}
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: true);
			}
			CurrentValue.gameObject.SetActive(value: false);
			if (!isLocked && !isSoulLocked)
			{
				int levelsToAdd = Mathf.Clamp(5, 0, skillTreeObj.MaxLevel - skillTreeObj.Level);
				int numLevelsPurchaseableWithGold = skillTreeObj.GetNumLevelsPurchaseableWithGold(levelsToAdd, SaveManager.PlayerSaveData.GoldCollectedIncludingBank);
				DescriptionLocItem.TextObj.text = string.Format(LocalizationManager.GetString("LOC_ID_SKILL_TREE_UI_PURCHASE_MULTIPLE_1", isFemale: false), numLevelsPurchaseableWithGold);
			}
			else
			{
				DescriptionLocItem.gameObject.SetActive(value: false);
			}
			break;
		}
	}

	private void OnGoldChanged(MonoBehaviour sender, EventArgs args)
	{
		if (DescriptionType == SkillTreeDescriptionType.GoldOwned)
		{
			CurrentValue.text = SaveManager.PlayerSaveData.GetActualAvailableGoldString();
		}
	}

	protected string ColoredString(string text, Color color)
	{
		return "<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">" + text + "</color>";
	}

	protected string PercentString(float value)
	{
		value *= 100f;
		string text = value.ToString("F1", LocalizationManager.GetCurrentCultureInfo());
		if (text[text.Length - 1] == '0')
		{
			text = Mathf.RoundToInt(value).ToString();
		}
		return string.Format(LocalizationManager.GetString("LOC_ID_GENERAL_UI_PERCENT_1", isFemale: false), text);
	}

	protected string PlusSymbolString(float value, bool isPercent, bool addSymbolToZero = true)
	{
		string text = (isPercent ? PercentString(value) : value.ToString());
		if (addSymbolToZero)
		{
			if (!(value >= 0f))
			{
				return text;
			}
			return "+" + text;
		}
		if (!(value > 0f))
		{
			return text;
		}
		return "+" + text;
	}

	protected string ColoredValueString(float value, bool addBrackets, bool isPercent, bool lowerIsBetter, bool hideZero = true)
	{
		if (hideZero && value == 0f)
		{
			return "";
		}
		Color color = OriginalColor;
		if (value != 0f)
		{
			color = (((!(value > 0f) || lowerIsBetter) && (!(value < 0f) || !lowerIsBetter)) ? DeficitColor : BenefitColor);
		}
		string text = (isPercent ? PercentString(value) : value.ToString());
		if (value >= 0f)
		{
			text = PlusSymbolString(value, isPercent);
		}
		if (addBrackets)
		{
			text = "(" + text + ")";
		}
		return ColoredString(text, color);
	}
}
