using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCardRightPageEntry : MonoBehaviour, ILocalizable
{
	private enum PlayerRightCardType
	{
		None,
		Class,
		Weapon,
		Talent,
		Spell,
		TraitOne,
		TraitTwo
	}

	[SerializeField]
	private TMP_Text m_descriptionText;

	[SerializeField]
	private TMP_Text m_titleText;

	[SerializeField]
	private Image m_icon;

	[SerializeField]
	private PlayerRightCardType m_cardType;

	private Action<MonoBehaviour, EventArgs> m_refreshText;

	private void Awake()
	{
		m_refreshText = RefreshText;
	}

	private void OnEnable()
	{
		UpdateCard();
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_refreshText);
	}

	private void OnDisable()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_refreshText);
	}

	public void RefreshText(object sender, EventArgs args)
	{
		UpdateCard();
	}

	private void UpdateCard()
	{
		CharacterData currentCharacter = SaveManager.PlayerSaveData.CurrentCharacter;
		bool isFemale = currentCharacter.IsFemale;
		ClassData classData = ClassLibrary.GetClassData(currentCharacter.ClassType);
		bool flag = true;
		string text = null;
		string text2 = null;
		TraitType traitType = ((m_cardType == PlayerRightCardType.TraitOne) ? currentCharacter.TraitOne : currentCharacter.TraitTwo);
		RelicType relicType = ((m_cardType == PlayerRightCardType.TraitOne) ? currentCharacter.AntiqueOneOwned : currentCharacter.AntiqueTwoOwned);
		switch (m_cardType)
		{
		case PlayerRightCardType.Class:
			m_icon.sprite = IconLibrary.GetClassIcon(currentCharacter.ClassType);
			if (classData != null)
			{
				text = classData.PassiveData.Title;
				text2 = classData.PassiveData.Controls;
			}
			else
			{
				flag = false;
				text = "NULL CLASS (" + currentCharacter.ClassType.ToString() + ")";
				text2 = "NULL CLASS DESCRIPTION";
			}
			break;
		case PlayerRightCardType.Weapon:
		{
			m_icon.sprite = IconLibrary.GetAbilityIcon(currentCharacter.Weapon, returnNullIfNotFound: false);
			BaseAbility_RL ability = AbilityLibrary.GetAbility(currentCharacter.Weapon);
			if (ability != null)
			{
				text = ability.AbilityData.Title;
				text2 = ability.AbilityData.Description;
			}
			else
			{
				flag = false;
				text = "NULL WEAPON (" + currentCharacter.Weapon.ToString() + ")";
				text2 = "NULL WEAPON DESCRIPTION";
			}
			break;
		}
		case PlayerRightCardType.Talent:
		{
			m_icon.sprite = IconLibrary.GetAbilityIcon(currentCharacter.Talent, returnNullIfNotFound: false);
			BaseAbility_RL ability3 = AbilityLibrary.GetAbility(currentCharacter.Talent);
			if (ability3 != null)
			{
				text = ability3.AbilityData.Title;
				text2 = ability3.AbilityData.Description;
			}
			else
			{
				flag = false;
				text = "NULL TALENT (" + currentCharacter.Talent.ToString() + ")";
				text2 = "NULL TALENT DESCRIPTION";
			}
			break;
		}
		case PlayerRightCardType.Spell:
		{
			m_icon.sprite = IconLibrary.GetAbilityIcon(currentCharacter.Spell, returnNullIfNotFound: false);
			BaseAbility_RL ability2 = AbilityLibrary.GetAbility(currentCharacter.Spell);
			if (ability2 != null)
			{
				text = ability2.AbilityData.Title;
				text2 = ability2.AbilityData.Description;
			}
			else
			{
				flag = false;
				text = "NULL SPELL (" + currentCharacter.Spell.ToString() + ")";
				text2 = "NULL SPELL DESCRIPTION";
			}
			break;
		}
		case PlayerRightCardType.TraitOne:
		case PlayerRightCardType.TraitTwo:
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
			if (traitData != null)
			{
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
				if (TraitManager.GetTraitSeenState(traitType) < TraitSeenState.SeenTwice)
				{
					flag = false;
					text2 = LocalizationManager.GetString("LOC_ID_LINEAGE_HIDDEN_TRAIT_1", isFemale: false);
					break;
				}
				switch (traitType)
				{
				case TraitType.Disposition:
					text2 = Gay_Trait.GetDispositionLocID(currentCharacter);
					break;
				case TraitType.Antique:
					if ((bool)relicData)
					{
						text2 = relicData.Description;
						break;
					}
					text2 = "NULL ANTIQUE DESCRIPTION (" + relicType.ToString() + ")";
					flag = false;
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
			if (!string.IsNullOrEmpty(text))
			{
				m_titleText.text = LocalizationManager.GetString(text, isFemale);
			}
			if (!string.IsNullOrEmpty(text2))
			{
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
}
