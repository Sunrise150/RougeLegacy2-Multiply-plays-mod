using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LineageDescriptionUpdater : MonoBehaviour, ILocalizable
{
	public enum LineageDescriptionType
	{
		None,
		Header,
		Class,
		Talent,
		Weapon,
		Spell,
		TraitOne,
		TraitTwo
	}

	private const int DEFAULT_MIN_FONTSIZE = 20;

	private const int KOREAN_MIN_FONTSIZE = 17;

	public LineageDescriptionType DescriptionType;

	[SerializeField]
	private Image m_icon;

	[SerializeField]
	private Image m_lockIcon;

	[SerializeField]
	private TMP_Text m_titleText;

	[SerializeField]
	private TMP_Text m_descriptionText;

	private bool m_classLocked;

	private bool m_spellLocked;

	private CharacterData m_storedCharData;

	private Action<MonoBehaviour, EventArgs> m_onHeirSelected;

	private Action<MonoBehaviour, EventArgs> m_refreshText;

	private bool m_koreanSizeApplied;

	public void SetLocked(bool locked)
	{
		m_lockIcon.gameObject.SetActive(locked);
	}

	private void Awake()
	{
		m_onHeirSelected = OnHeirSelected;
		m_refreshText = RefreshText;
	}

	private void OnEnable()
	{
		UpdateFontResize();
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.Lineage_SelectedNewHeir, m_onHeirSelected);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_refreshText);
		SetLocked(locked: false);
	}

	private void OnDisable()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.Lineage_SelectedNewHeir, m_onHeirSelected);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_refreshText);
	}

	private void UpdateFontResize()
	{
		if ((bool)m_descriptionText)
		{
			if (LocalizationManager.CurrentLanguageType == LanguageType.Korean && !m_koreanSizeApplied)
			{
				m_descriptionText.fontSizeMin = 17f;
				m_koreanSizeApplied = true;
			}
			else if (LocalizationManager.CurrentLanguageType != LanguageType.Korean && m_koreanSizeApplied)
			{
				m_descriptionText.fontSizeMin = 20f;
				m_koreanSizeApplied = false;
			}
		}
	}

	private void OnHeirSelected(MonoBehaviour sender, EventArgs args)
	{
		LineageHeirChangedEventArgs lineageHeirChangedEventArgs = args as LineageHeirChangedEventArgs;
		UpdateText(m_storedCharData = lineageHeirChangedEventArgs.CharacterData, lineageHeirChangedEventArgs.ClassLocked, lineageHeirChangedEventArgs.SpellLocked);
	}

	private void UpdateText(CharacterData charData, bool classLocked, bool spellLocked)
	{
		ClassData classData = ClassLibrary.GetClassData(charData.ClassType);
		bool isFemale = charData.IsFemale;
		string text = null;
		string text2 = null;
		TraitType traitType = ((DescriptionType == LineageDescriptionType.TraitOne) ? charData.TraitOne : charData.TraitTwo);
		RelicType relicType = ((DescriptionType == LineageDescriptionType.TraitOne) ? charData.AntiqueOneOwned : charData.AntiqueTwoOwned);
		bool flag = true;
		switch (DescriptionType)
		{
		case LineageDescriptionType.Header:
		{
			string localizedPlayerName = LocalizationManager.GetLocalizedPlayerName(charData);
			m_titleText.text = localizedPlayerName;
			text2 = classData.PassiveData.Description;
			break;
		}
		case LineageDescriptionType.Class:
		{
			m_icon.sprite = IconLibrary.GetClassIcon(charData.ClassType);
			if ((bool)classData)
			{
				text = classData.PassiveData.Title;
				text2 = classData.PassiveData.Controls;
			}
			else
			{
				flag = false;
				text = "NULL CLASS (" + charData.ClassType.ToString() + ")";
				text2 = "NULL CLASS DESCRIPTION";
			}
			string text5 = LocalizationManager.GetString(text, isFemale);
			string @string = LocalizationManager.GetString(text2, isFemale);
			if (SkillTreeLogicHelper.IsTotemUnlocked())
			{
				int classMasteryRank = SaveManager.PlayerSaveData.GetClassMasteryRank(charData.ClassType);
				text5 += string.Format(LocalizationManager.GetString("LOC_ID_HUD_TITLE_RANK_LEVEL_1", isFemale: false), classMasteryRank);
				if ((LocalizationManager.CurrentLanguageType == LanguageType.English || LocalizationManager.CurrentLanguageType == LanguageType.Chinese_Trad || LocalizationManager.CurrentLanguageType == LanguageType.Chinese_Simp || LocalizationManager.CurrentLanguageType == LanguageType.Korean) && classMasteryRank > 0)
				{
					string arg3 = null;
					string arg4 = null;
					string string2 = LocalizationManager.GetString("LOC_ID_LINEAGE_RANK_BONUS_FORMATTER_1", isFemale: false);
					string2 = LocalizationManager.GetFormatterGenderForcedString(string2, out var isFemale2);
					if (Mastery_EV.MasteryBonusTypeTable.TryGetValue(charData.ClassType, out var value) && Mastery_EV.MasteryBonusLocIDTable.TryGetValue(value, out var value2))
					{
						arg3 = LocalizationManager.GetString(value2, isFemale2);
					}
					if (Mastery_EV.MasteryBonusTypeTable.TryGetValue(charData.ClassType, out value) && Mastery_EV.MasteryBonusAmountTable.TryGetValue(value, out var value3))
					{
						float num3 = (float)classMasteryRank * value3;
						if (CDGHelper.IsPercent(value3))
						{
							num3 = Mathf.CeilToInt(num3 * 100f);
							arg4 = "+" + string.Format(LocalizationManager.GetString("LOC_ID_GENERAL_UI_PERCENT_1", isFemale: false), num3);
						}
						else
						{
							arg4 = "+" + num3;
						}
					}
					text5 = text5 + " " + string.Format(string2, arg4, arg3);
				}
			}
			m_titleText.text = text5;
			m_descriptionText.text = @string;
			m_classLocked = classLocked;
			SetLocked(classLocked);
			text = null;
			text2 = null;
			break;
		}
		case LineageDescriptionType.Talent:
		{
			m_icon.sprite = IconLibrary.GetAbilityIcon(charData.Talent, returnNullIfNotFound: false);
			BaseAbility_RL ability = AbilityLibrary.GetAbility(charData.Talent);
			if ((bool)ability)
			{
				if (charData.ClassType == ClassType.MagicWandClass && AbilityType_RL.IsMageSpellOrTalent(charData.Talent))
				{
					if (SaveManager.PlayerSaveData.GetSpellSeenState(charData.Talent))
					{
						text = ability.AbilityData.Title;
						text2 = ability.AbilityData.Description;
					}
					else
					{
						text = LocalizationManager.GetString(ability.AbilityData.Title, charData.IsFemale);
						text2 = LocalizationManager.GetString("LOC_ID_LINEAGE_HIDDEN_SPELL_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
						flag = false;
					}
				}
				else
				{
					text = ability.AbilityData.Title;
					text2 = ability.AbilityData.Description;
				}
			}
			else
			{
				flag = false;
				text = "NULL TALENT (" + charData.Talent.ToString() + ")";
				text2 = "NULL TALENT DESCRIPTION";
			}
			break;
		}
		case LineageDescriptionType.Weapon:
		{
			m_icon.sprite = IconLibrary.GetAbilityIcon(charData.Weapon, returnNullIfNotFound: false);
			BaseAbility_RL ability2 = AbilityLibrary.GetAbility(charData.Weapon);
			if (ability2 != null)
			{
				text = ability2.AbilityData.Title;
				text2 = ability2.AbilityData.Description;
			}
			else
			{
				flag = false;
				text = "NULL WEAPON (" + charData.Weapon.ToString() + ")";
				text2 = "NULL WEAPON DESCRIPTION";
			}
			break;
		}
		case LineageDescriptionType.Spell:
		{
			m_icon.sprite = IconLibrary.GetAbilityIcon(charData.Spell, returnNullIfNotFound: false);
			BaseAbility_RL ability3 = AbilityLibrary.GetAbility(charData.Spell);
			if ((bool)ability3)
			{
				if (SaveManager.PlayerSaveData.GetSpellSeenState(charData.Spell))
				{
					text = ability3.AbilityData.Title;
					text2 = ability3.AbilityData.Description;
				}
				else
				{
					text = LocalizationManager.GetString(ability3.AbilityData.Title, charData.IsFemale);
					text2 = LocalizationManager.GetString("LOC_ID_LINEAGE_HIDDEN_SPELL_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
					flag = false;
				}
			}
			else
			{
				flag = false;
				text = "NULL SPELL (" + charData.Spell.ToString() + ")";
				text2 = "NULL SPELL DESCRIPTION";
			}
			m_spellLocked = spellLocked;
			SetLocked(spellLocked);
			break;
		}
		case LineageDescriptionType.TraitOne:
		case LineageDescriptionType.TraitTwo:
		{
			if (traitType == TraitType.None)
			{
				m_icon.transform.parent.gameObject.SetActive(value: false);
				m_titleText.gameObject.SetActive(value: false);
				m_descriptionText.gameObject.SetActive(value: false);
				break;
			}
			m_icon.transform.parent.gameObject.SetActive(value: true);
			m_titleText.gameObject.SetActive(value: true);
			m_descriptionText.gameObject.SetActive(value: true);
			if (traitType == TraitType.Antique)
			{
				m_icon.sprite = IconLibrary.GetRelicSprite(relicType, getLargeIcon: false);
			}
			else
			{
				m_icon.sprite = IconLibrary.GetTraitIcon(traitType);
			}
			TraitData traitData = TraitLibrary.GetTraitData(traitType);
			if ((bool)traitData)
			{
				RectTransform component = m_titleText.transform.parent.GetComponent<RectTransform>();
				Vector2 sizeDelta = component.sizeDelta;
				sizeDelta.x = 540f;
				component.sizeDelta = sizeDelta;
				int num = Mathf.RoundToInt(TraitManager.GetActualTraitGoldGain(traitType) * 100f);
				string text3 = LocalizationManager.GetString(traitData.GetTraitTitleLocID(), isFemale);
				RelicData relicData = ((relicType != 0) ? RelicLibrary.GetRelicData(relicType) : null);
				if (traitType == TraitType.Antique)
				{
					float costAmount = relicData.CostAmount;
					float relicCostMod = SkillTreeLogicHelper.GetRelicCostMod();
					int num2 = Mathf.RoundToInt(Mathf.Clamp(costAmount - relicCostMod, 0f, float.MaxValue) * 100f);
					string arg = ((relicData != null) ? LocalizationManager.GetString(relicData.Title, isFemale: false) : "NULL RELIC");
					text3 = string.Format(LocalizationManager.GetString("LOC_ID_TRAIT_TITLE_Antique_1", isFemale), arg);
					string arg2 = string.Format(LocalizationManager.GetString("LOC_ID_GENERAL_UI_PERCENT_1", isFemale: false), -num2);
					text3 += $" • <sprite=\"IconGlyph_Spritesheet_Texture\" name=\"Resolve_Icon\"> {arg2}";
				}
				if (num != 0)
				{
					string text4 = ((num > 0) ? "+" : "");
					m_titleText.text = "<uppercase>" + text3 + "</uppercase>";
					m_titleText.text += string.Format(LocalizationManager.GetString("LOC_ID_HUD_TITLE_GOLD_AMOUNT_1", isFemale: false), text4 + num);
				}
				else
				{
					m_titleText.text = "<uppercase>" + text3 + " </uppercase>";
				}
				if (TraitManager.GetTraitSeenState(traitType) < TraitSeenState.SeenOnce)
				{
					text2 = LocalizationManager.GetString("LOC_ID_LINEAGE_HIDDEN_TRAIT_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
					flag = false;
					break;
				}
				switch (traitType)
				{
				case TraitType.Disposition:
					text2 = Gay_Trait.GetDispositionLocID(charData);
					break;
				case TraitType.Antique:
					if (relicData != null)
					{
						if (SaveManager.PlayerSaveData.GetRelic(relicType).WasSeen)
						{
							text2 = relicData.Description;
							break;
						}
						text2 = LocalizationManager.GetString("LOC_ID_LINEAGE_HIDDEN_ANTIQUE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
						flag = false;
					}
					else
					{
						text2 = "NULL ANTIQUE DESCRIPTION (" + relicType.ToString() + ")";
						flag = false;
					}
					break;
				default:
					text2 = traitData.Description;
					break;
				}
			}
			else
			{
				flag = false;
				text = "NULL TRAIT (" + traitType.ToString() + ")";
				text2 = "NULL TRAIT DESCRIPTION";
			}
			break;
		}
		}
		if (flag)
		{
			if (text != null)
			{
				m_titleText.text = LocalizationManager.GetString(text, isFemale);
			}
			if (!string.IsNullOrEmpty(text2))
			{
				string text6 = text2.Replace("_1", "_3");
				if (LocalizationManager.ContainsLocID(text6, isFemale: false))
				{
					text2 = text6;
				}
				m_descriptionText.text = LocalizationManager.GetString(text2, isFemale, out var isFormatString);
				if (isFormatString && traitType == TraitType.Antique)
				{
					float altValue;
					float relicFormatString = Relic_EV.GetRelicFormatString(relicType, 1, out altValue);
					m_descriptionText.text = string.Format(m_descriptionText.text, relicFormatString.ToCIString(), altValue.ToCIString());
				}
			}
		}
		else
		{
			if (!string.IsNullOrEmpty(text))
			{
				m_titleText.text = text;
			}
			if (!string.IsNullOrEmpty(text2))
			{
				m_descriptionText.text = text2;
			}
		}
	}

	public void RefreshText(object sender, EventArgs args)
	{
		UpdateFontResize();
		if (m_storedCharData != null)
		{
			UpdateText(m_storedCharData, m_classLocked, m_spellLocked);
		}
	}
}
