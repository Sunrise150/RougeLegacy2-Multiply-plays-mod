using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class BlacksmithOmniUIDescriptionBoxEntry : BaseOmniUIDescriptionBoxEntry<BlacksmithOmniUIDescriptionEventArgs, BlacksmithOmniUIDescriptionBoxEntry.BlacksmithOmniUIDescriptionBoxType>
{
	public enum BlacksmithOmniUIDescriptionBoxType
	{
		None,
		Title,
		UnityDescription1,
		UnityDescription2,
		UnityDescription3,
		Weight,
		Encumberance,
		Strength,
		Vitality,
		Magic,
		Armor,
		Dexterity,
		Focus,
		Unity
	}

	[SerializeField]
	private UnityEvent m_unityBonusOnUnityEvent;

	[SerializeField]
	private UnityEvent m_unityBonusOffUnityEvent;

	private Dictionary<EquipmentType, int> m_previousEquipmentSetLevel = new Dictionary<EquipmentType, int>();

	protected override void DisplayNullDescriptionBox(MonoBehaviour sender)
	{
		base.DisplayNullDescriptionBox(sender);
		if (m_descriptionType == BlacksmithOmniUIDescriptionBoxType.Title)
		{
			m_titleText.text = "???";
			m_icon.sprite = IconLibrary.GetDefaultSprite();
		}
	}

	protected override void DisplayDescriptionBox(BlacksmithOmniUIDescriptionEventArgs args)
	{
		if (m_descriptionType != BlacksmithOmniUIDescriptionBoxType.Unity && m_descriptionType != BlacksmithOmniUIDescriptionBoxType.UnityDescription1 && m_descriptionType != BlacksmithOmniUIDescriptionBoxType.UnityDescription2 && m_descriptionType != BlacksmithOmniUIDescriptionBoxType.UnityDescription3)
		{
			if (m_text1 != null)
			{
				m_text1.color = BaseOmniUIDescriptionBoxEntry<BlacksmithOmniUIDescriptionEventArgs, BlacksmithOmniUIDescriptionBoxType>.OriginalColor;
			}
			if (m_text2 != null)
			{
				m_text2.color = BaseOmniUIDescriptionBoxEntry<BlacksmithOmniUIDescriptionEventArgs, BlacksmithOmniUIDescriptionBoxType>.OriginalColor;
			}
		}
		if (m_textCanvasGroup != null)
		{
			m_textCanvasGroup.alpha = 0.35f;
		}
		EquipmentCategoryType categoryType = args.CategoryType;
		EquipmentType equipmentType = args.EquipmentType;
		EquipmentObj equipment = EquipmentManager.GetEquipment(categoryType, equipmentType);
		EquipmentObj equipped = EquipmentManager.GetEquipped(categoryType);
		EquipmentManager.IsEquipped(categoryType, equipmentType);
		EquipmentSetData equipmentSetData = EquipmentSetLibrary.GetEquipmentSetData(equipmentType);
		if (equipment == null)
		{
			return;
		}
		if (!m_previousEquipmentSetLevel.ContainsKey(equipmentType))
		{
			int value = EquipmentManager.Get_EquipmentSet_TotalEquippedLevel(equipmentType);
			m_previousEquipmentSetLevel.Add(equipmentType, value);
		}
		switch (m_descriptionType)
		{
		case BlacksmithOmniUIDescriptionBoxType.Title:
		{
			m_icon.sprite = IconLibrary.GetEquipmentIcon(categoryType, equipmentType);
			string text = LocalizationManager.GetString(equipment.EquipmentData.Title, isFemale: false);
			if (string.IsNullOrEmpty(text) || text.Contains("LOC_ID"))
			{
				text = Equipment_EV.GetFormattedEquipmentName(categoryType, equipmentType);
			}
			if (equipment.ClampedUpgradeLevel > 0)
			{
				text = text + " +" + equipment.ClampedUpgradeLevel;
			}
			m_titleText.text = text;
			break;
		}
		case BlacksmithOmniUIDescriptionBoxType.Weight:
		{
			float playerVal2 = (PlayerManager.IsInstantiated ? PlayerManager.GetPlayerController().ActualAllowedEquipmentWeight : 0);
			float currentStatValue2 = equipment.GetCurrentStatValue(EquipmentStatType.Weight);
			float statValueAtLevel2 = equipment.GetStatValueAtLevel(EquipmentStatType.Weight, equipment.ClampedUpgradeLevel + 1);
			float equippedItemVal2 = equipped?.GetCurrentStatValue(EquipmentStatType.Weight) ?? 0f;
			SetStatString(playerVal2, currentStatValue2, statValueAtLevel2, equippedItemVal2, lowerIsBetter: true, isPercent: false, args);
			m_text2.text = (int)EquipmentManager.GetTotalEquippedStatValue(EquipmentStatType.Weight) + "/" + m_text2.text;
			break;
		}
		case BlacksmithOmniUIDescriptionBoxType.Vitality:
		{
			float playerVal5 = (PlayerManager.IsInstantiated ? PlayerManager.GetPlayerController().ActualVitality : 0);
			float currentStatValue5 = equipment.GetCurrentStatValue(EquipmentStatType.Vitality);
			float statValueAtLevel5 = equipment.GetStatValueAtLevel(EquipmentStatType.Vitality, equipment.ClampedUpgradeLevel + 1);
			float equippedItemVal5 = equipped?.GetCurrentStatValue(EquipmentStatType.Vitality) ?? 0f;
			SetStatString(playerVal5, currentStatValue5, statValueAtLevel5, equippedItemVal5, lowerIsBetter: false, isPercent: false, args);
			break;
		}
		case BlacksmithOmniUIDescriptionBoxType.Strength:
		{
			float playerVal7 = (PlayerManager.IsInstantiated ? Mathf.RoundToInt(PlayerManager.GetPlayerController().ActualStrength) : 0);
			float currentStatValue7 = equipment.GetCurrentStatValue(EquipmentStatType.Strength);
			float statValueAtLevel7 = equipment.GetStatValueAtLevel(EquipmentStatType.Strength, equipment.ClampedUpgradeLevel + 1);
			float equippedItemVal7 = equipped?.GetCurrentStatValue(EquipmentStatType.Strength) ?? 0f;
			SetStatString(playerVal7, currentStatValue7, statValueAtLevel7, equippedItemVal7, lowerIsBetter: false, isPercent: false, args);
			break;
		}
		case BlacksmithOmniUIDescriptionBoxType.Magic:
		{
			float playerVal4 = (PlayerManager.IsInstantiated ? Mathf.RoundToInt(PlayerManager.GetPlayerController().ActualMagic) : 0);
			float currentStatValue4 = equipment.GetCurrentStatValue(EquipmentStatType.Magic);
			float statValueAtLevel4 = equipment.GetStatValueAtLevel(EquipmentStatType.Magic, equipment.ClampedUpgradeLevel + 1);
			float equippedItemVal4 = equipped?.GetCurrentStatValue(EquipmentStatType.Magic) ?? 0f;
			SetStatString(playerVal4, currentStatValue4, statValueAtLevel4, equippedItemVal4, lowerIsBetter: false, isPercent: false, args);
			break;
		}
		case BlacksmithOmniUIDescriptionBoxType.Armor:
		{
			float playerVal8 = (PlayerManager.IsInstantiated ? PlayerManager.GetPlayerController().ActualArmor : 0);
			float currentStatValue8 = equipment.GetCurrentStatValue(EquipmentStatType.Armor);
			float statValueAtLevel8 = equipment.GetStatValueAtLevel(EquipmentStatType.Armor, equipment.ClampedUpgradeLevel + 1);
			float equippedItemVal8 = equipped?.GetCurrentStatValue(EquipmentStatType.Armor) ?? 0f;
			SetStatString(playerVal8, currentStatValue8, statValueAtLevel8, equippedItemVal8, lowerIsBetter: false, isPercent: false, args);
			break;
		}
		case BlacksmithOmniUIDescriptionBoxType.Dexterity:
		{
			float playerVal6 = (PlayerManager.IsInstantiated ? Mathf.RoundToInt(PlayerManager.GetPlayerController().ActualDexterity) : 0);
			float currentStatValue6 = equipment.GetCurrentStatValue(EquipmentStatType.Dexterity_Add);
			float statValueAtLevel6 = equipment.GetStatValueAtLevel(EquipmentStatType.Dexterity_Add, equipment.ClampedUpgradeLevel + 1);
			float equippedItemVal6 = equipped?.GetCurrentStatValue(EquipmentStatType.Dexterity_Add) ?? 0f;
			SetStatString(playerVal6, currentStatValue6, statValueAtLevel6, equippedItemVal6, lowerIsBetter: false, isPercent: false, args);
			break;
		}
		case BlacksmithOmniUIDescriptionBoxType.Focus:
		{
			float playerVal3 = (PlayerManager.IsInstantiated ? Mathf.RoundToInt(PlayerManager.GetPlayerController().ActualFocus) : 0);
			float currentStatValue3 = equipment.GetCurrentStatValue(EquipmentStatType.Focus_Add);
			float statValueAtLevel3 = equipment.GetStatValueAtLevel(EquipmentStatType.Focus_Add, equipment.ClampedUpgradeLevel + 1);
			float equippedItemVal3 = equipped?.GetCurrentStatValue(EquipmentStatType.Focus_Add) ?? 0f;
			SetStatString(playerVal3, currentStatValue3, statValueAtLevel3, equippedItemVal3, lowerIsBetter: false, isPercent: false, args);
			break;
		}
		case BlacksmithOmniUIDescriptionBoxType.Unity:
		{
			float playerVal = EquipmentManager.Get_EquipmentSet_TotalEquippedLevel(args.EquipmentType);
			float currentStatValue = equipment.GetCurrentStatValue(EquipmentStatType.Unity);
			float statValueAtLevel = equipment.GetStatValueAtLevel(EquipmentStatType.Unity, equipment.ClampedUpgradeLevel + 1);
			float equippedItemVal = 0f;
			SetStatString(playerVal, currentStatValue, statValueAtLevel, equippedItemVal, lowerIsBetter: false, isPercent: false, args);
			string @string = LocalizationManager.GetString("LOC_ID_EQUIP_STAT_TITLE_SET_BONUS_1", isFemale: false, disableWordScramble: true);
			bool isFemale = false;
			@string = LocalizationManager.GetFormatterGenderForcedString(@string, out isFemale);
			string string2 = LocalizationManager.GetString(Equipment_EV.GetEquipmentTypeNameLocID(equipmentType), isFemale, disableWordScramble: true);
			m_titleText.text = string.Format(@string, string2);
			break;
		}
		case BlacksmithOmniUIDescriptionBoxType.Encumberance:
		{
			PlayerManager.GetPlayerController();
			int weightLevel = EquipmentManager.GetWeightLevel();
			weightLevel = Mathf.Clamp(weightLevel, 0, Equipment_EV.CRIT_DAMAGE_BONUS_PER_WEIGHT_LEVEL.Length - 1);
			string arg3 = Mathf.RoundToInt(Equipment_EV.RESOLVE_BONUS_PER_WEIGHT_LEVEL[weightLevel] * 100f).ToString();
			string text2 = string.Format(LocalizationManager.GetString("LOC_ID_EQUIP_STAT_TITLE_ENCUMBRANCE_BONUS_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), arg3);
			switch (weightLevel)
			{
			case 0:
				m_titleText.text = ColoredString(LocalizationManager.GetString("LOC_ID_EQUIP_STAT_TITLE_ENCUMBRANCE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), Color.blue);
				m_text2.text = ColoredString(text2, Color.blue);
				break;
			case 1:
				m_titleText.text = ColoredString(LocalizationManager.GetString("LOC_ID_EQUIP_STAT_TITLE_ENCUMBRANCE_2", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), Color.black);
				m_text2.text = ColoredString(text2, Color.black);
				break;
			case 2:
			{
				Color color = new Color(20f / 51f, 4f / 51f, 41f / 51f, 1f);
				m_titleText.text = ColoredString(LocalizationManager.GetString("LOC_ID_EQUIP_STAT_TITLE_ENCUMBRANCE_3", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), color);
				m_text2.text = ColoredString(text2, color);
				break;
			}
			case 3:
				m_titleText.text = ColoredString(LocalizationManager.GetString("LOC_ID_EQUIP_STAT_TITLE_ENCUMBRANCE_4", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), Color.red);
				m_text2.text = ColoredString(text2, Color.red);
				break;
			default:
				m_titleText.text = ColoredString(LocalizationManager.GetString("LOC_ID_EQUIP_STAT_TITLE_ENCUMBRANCE_5", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), Color.red);
				m_text2.text = ColoredString(text2, Color.red);
				break;
			}
			break;
		}
		case BlacksmithOmniUIDescriptionBoxType.UnityDescription1:
		{
			if (!(equipmentSetData != null))
			{
				break;
			}
			int setRequirement2 = equipmentSetData.SetRequirement01;
			m_text1.text = string.Format(LocalizationManager.GetString("LOC_ID_EQUIP_STAT_TITLE_UNITY_REQUIREMENT_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), setRequirement2);
			if (EquipmentManager.Get_EquipmentSet_CurrentLevel(equipmentType) >= setRequirement2)
			{
				string arg2 = ((!CDGHelper.IsPercent(equipmentSetData.SetBonus01.StatGain)) ? equipmentSetData.SetBonus01.StatGain.ToString() : string.Format(LocalizationManager.GetString("LOC_ID_GENERAL_UI_PERCENT_1", isFemale: false), equipmentSetData.SetBonus01.StatGain * 100f));
				if (EquipmentManager.Get_EquipmentSet_TotalEquippedLevel(equipmentType) >= setRequirement2)
				{
					if (EquipmentManager.Get_EquipmentSet_LevelViewed(equipmentType) < 1)
					{
						EquipmentManager.Set_EquipmentSet_LevelViewed(equipmentType, 1, additive: false);
						string newText2 = $"<color=green>+{arg2}</color> ";
						StartCoroutine(RunRevealUnityEffect(m_titleText, newText2));
						FireUnityBonusEvents(equipmentType, setRequirement2);
					}
					else
					{
						m_titleText.alpha = 1f;
						m_titleText.text = $"<color=green>+{arg2}</color> ";
					}
				}
				else
				{
					m_titleText.alpha = 0.4f;
					m_titleText.text = $"+{arg2} ";
				}
				m_titleText.text += LocalizationManager.GetString(EquipmentSetLibrary.GetEquipmentSetBonusLocID(equipmentSetData.SetBonus01.BonusType), isFemale: false);
			}
			else
			{
				m_titleText.alpha = 1f;
				m_titleText.text = "???????";
			}
			break;
		}
		case BlacksmithOmniUIDescriptionBoxType.UnityDescription2:
		{
			if (!(equipmentSetData != null))
			{
				break;
			}
			int setRequirement3 = equipmentSetData.SetRequirement02;
			m_text1.text = string.Format(LocalizationManager.GetString("LOC_ID_EQUIP_STAT_TITLE_UNITY_REQUIREMENT_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), setRequirement3);
			if (EquipmentManager.Get_EquipmentSet_CurrentLevel(equipmentType) >= setRequirement3)
			{
				string arg4 = ((!CDGHelper.IsPercent(equipmentSetData.SetBonus02.StatGain)) ? equipmentSetData.SetBonus02.StatGain.ToString() : string.Format(LocalizationManager.GetString("LOC_ID_GENERAL_UI_PERCENT_1", isFemale: false), equipmentSetData.SetBonus02.StatGain * 100f));
				if (EquipmentManager.Get_EquipmentSet_TotalEquippedLevel(equipmentType) >= setRequirement3)
				{
					if (EquipmentManager.Get_EquipmentSet_LevelViewed(equipmentType) < 2)
					{
						EquipmentManager.Set_EquipmentSet_LevelViewed(equipmentType, 2, additive: false);
						string newText3 = $"<color=green>+{arg4}</color> ";
						StartCoroutine(RunRevealUnityEffect(m_titleText, newText3));
						FireUnityBonusEvents(equipmentType, setRequirement3);
					}
					else
					{
						m_titleText.alpha = 1f;
						m_titleText.text = $"<color=green>+{arg4}</color> ";
					}
				}
				else
				{
					m_titleText.alpha = 0.4f;
					m_titleText.text = $"+{arg4} ";
				}
				m_titleText.text += LocalizationManager.GetString(EquipmentSetLibrary.GetEquipmentSetBonusLocID(equipmentSetData.SetBonus02.BonusType), isFemale: false);
			}
			else
			{
				m_titleText.alpha = 1f;
				m_titleText.text = "???????";
			}
			break;
		}
		case BlacksmithOmniUIDescriptionBoxType.UnityDescription3:
		{
			if (!(equipmentSetData != null))
			{
				break;
			}
			int setRequirement = equipmentSetData.SetRequirement03;
			m_text1.text = string.Format(LocalizationManager.GetString("LOC_ID_EQUIP_STAT_TITLE_UNITY_REQUIREMENT_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), setRequirement);
			int num = EquipmentManager.Get_EquipmentSet_CurrentLevel(equipmentType);
			FireUnityBonusEvents(equipmentType, setRequirement);
			if (num >= setRequirement)
			{
				string arg = ((!CDGHelper.IsPercent(equipmentSetData.SetBonus03.StatGain)) ? equipmentSetData.SetBonus03.StatGain.ToString() : string.Format(LocalizationManager.GetString("LOC_ID_GENERAL_UI_PERCENT_1", isFemale: false), equipmentSetData.SetBonus03.StatGain * 100f));
				if (EquipmentManager.Get_EquipmentSet_TotalEquippedLevel(equipmentType) >= setRequirement)
				{
					if (EquipmentManager.Get_EquipmentSet_LevelViewed(equipmentType) < 3)
					{
						EquipmentManager.Set_EquipmentSet_LevelViewed(equipmentType, 3, additive: false);
						string newText = $"<color=green>+{arg}</color> ";
						StartCoroutine(RunRevealUnityEffect(m_titleText, newText));
						StoreAPIManager.GiveAchievement(AchievementType.UnlockHighestUnity, StoreType.All);
						FireUnityBonusEvents(equipmentType, setRequirement);
					}
					else
					{
						m_titleText.alpha = 1f;
						m_titleText.text = $"<color=green>+{arg}</color> ";
					}
				}
				else
				{
					m_titleText.alpha = 0.4f;
					m_titleText.text = $"+{arg} ";
				}
				m_titleText.text += LocalizationManager.GetString(EquipmentSetLibrary.GetEquipmentSetBonusLocID(equipmentSetData.SetBonus03.BonusType), isFemale: false);
			}
			else
			{
				m_titleText.alpha = 1f;
				m_titleText.text = "???????";
			}
			break;
		}
		}
	}

	private void FireUnityBonusEvents(EquipmentType equipmentType, int requiredUnityLevel)
	{
		int num = EquipmentManager.Get_EquipmentSet_TotalEquippedLevel(equipmentType);
		if (m_previousEquipmentSetLevel[equipmentType] == num)
		{
			return;
		}
		if (num >= requiredUnityLevel)
		{
			if (m_previousEquipmentSetLevel[equipmentType] < requiredUnityLevel && m_unityBonusOnUnityEvent != null)
			{
				m_unityBonusOnUnityEvent.Invoke();
			}
		}
		else if (m_previousEquipmentSetLevel[equipmentType] >= requiredUnityLevel && m_unityBonusOffUnityEvent != null)
		{
			m_unityBonusOffUnityEvent.Invoke();
		}
		m_previousEquipmentSetLevel[equipmentType] = num;
	}

	private IEnumerator RunRevealUnityEffect(TMP_Text text, string newText)
	{
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		RewiredMapController.SetMouseEnabled(enabled: false);
		text.alpha = 0f;
		text.text = newText;
		EffectManager.PlayEffect(base.gameObject, null, "EquipmentSetSparkle_Effect", Vector3.zero, 0.5f, EffectStopType.Gracefully).transform.SetParent(text.transform.parent, worldPositionStays: false);
		yield return TweenManager.TweenTo_UnscaledTime(text, 0.5f, Ease.None, "alpha", 1).TweenCoroutine;
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
		RewiredMapController.SetMouseEnabled(enabled: true);
	}

	private void SetStatString(float playerVal, float itemVal, float itemNextLevelVal, float equippedItemVal, bool lowerIsBetter, bool isPercent, BlacksmithOmniUIDescriptionEventArgs args)
	{
		EquipmentObj equipped = EquipmentManager.GetEquipped(args.CategoryType);
		bool flag = false;
		if (equipped != null)
		{
			flag = equipped.EquipmentType == args.EquipmentType;
		}
		float num = itemVal - equippedItemVal;
		float value = 0f - itemVal;
		float num2 = itemNextLevelVal - itemVal;
		string text = (isPercent ? PercentString(playerVal) : playerVal.ToCIString());
		if (!isPercent)
		{
			itemVal.ToCIString();
		}
		else
		{
			PercentString(itemVal);
		}
		switch (args.ButtonType)
		{
		case OmniUIButtonType.Purchasing:
			m_text1.text = ColoredValueString(itemVal, addBrackets: false, isPercent, lowerIsBetter, hideZero: false);
			if (!flag && itemVal != 0f)
			{
				m_text2.text = text + " " + ColoredValueString(num, addBrackets: true, isPercent, lowerIsBetter, hideZero: false);
			}
			else
			{
				m_text2.text = text + " " + ColoredValueString(num, addBrackets: true, isPercent, lowerIsBetter);
			}
			break;
		case OmniUIButtonType.Upgrading:
			m_text1.text = PlusSymbolString(itemVal, isPercent) + " " + ColoredValueString(num2, addBrackets: true, isPercent, lowerIsBetter);
			if (flag)
			{
				m_text2.text = text + " " + ColoredValueString(num2, addBrackets: true, isPercent, lowerIsBetter);
			}
			else
			{
				m_text2.text = text + " " + ColoredValueString(num + num2, addBrackets: true, isPercent, lowerIsBetter);
			}
			break;
		case OmniUIButtonType.Equipping:
			m_text1.text = PlusSymbolString(itemVal, isPercent);
			if (!flag && itemVal != 0f)
			{
				m_text2.text = text + " " + ColoredValueString(num, addBrackets: true, isPercent, lowerIsBetter, hideZero: false);
			}
			else
			{
				m_text2.text = text + " " + ColoredValueString(num, addBrackets: true, isPercent, lowerIsBetter);
			}
			break;
		case OmniUIButtonType.Unequipping:
			m_text1.text = PlusSymbolString(itemVal, isPercent);
			m_text2.text = text + " " + ColoredValueString(value, addBrackets: true, isPercent, lowerIsBetter);
			break;
		case OmniUIButtonType.CategorySelection:
			m_text2.text = text;
			break;
		}
		if ((bool)m_textCanvasGroup && itemVal != 0f)
		{
			m_textCanvasGroup.alpha = 1f;
		}
	}
}
