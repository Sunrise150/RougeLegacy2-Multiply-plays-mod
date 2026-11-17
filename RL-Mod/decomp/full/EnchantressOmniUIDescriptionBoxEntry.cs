using UnityEngine;

public class EnchantressOmniUIDescriptionBoxEntry : BaseOmniUIDescriptionBoxEntry<EnchantressOmniUIDescriptionEventArgs, EnchantressOmniUIDescriptionBoxEntry.EnchantressOmniUIDescriptionBoxType>
{
	public enum EnchantressOmniUIDescriptionBoxType
	{
		None,
		Title,
		Description,
		Stat,
		Weight,
		RunesOwned
	}

	protected override void DisplayNullDescriptionBox(MonoBehaviour sender)
	{
		base.DisplayNullDescriptionBox(sender);
		if (m_descriptionType == EnchantressOmniUIDescriptionBoxType.Title)
		{
			m_titleText.text = "???";
			m_icon.sprite = IconLibrary.GetDefaultSprite();
		}
	}

	protected override void DisplayDescriptionBox(EnchantressOmniUIDescriptionEventArgs args)
	{
		RuneType runeType = args.RuneType;
		RuneObj rune = RuneManager.GetRune(runeType);
		if (rune == null)
		{
			return;
		}
		switch (m_descriptionType)
		{
		case EnchantressOmniUIDescriptionBoxType.Title:
		{
			m_icon.sprite = IconLibrary.GetRuneIcon(runeType);
			string @string = LocalizationManager.GetString(rune.RuneData.Title, isFemale: false);
			m_titleText.text = @string;
			break;
		}
		case EnchantressOmniUIDescriptionBoxType.Description:
		{
			string string2 = LocalizationManager.GetString(rune.RuneData.Description, isFemale: false);
			m_titleText.text = string2;
			break;
		}
		case EnchantressOmniUIDescriptionBoxType.Stat:
			SetStatString(rune, args);
			break;
		case EnchantressOmniUIDescriptionBoxType.Weight:
			if (m_titleText != null)
			{
				m_titleText.text = LocalizationManager.GetString("LOC_ID_RUNE_STAT_TITLE_WEIGHT_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			}
			SetWeightString(rune, args);
			break;
		case EnchantressOmniUIDescriptionBoxType.RunesOwned:
			SetRunesOwnedString(rune, args);
			break;
		}
	}

	private void SetRunesOwnedString(RuneObj runeObj, EnchantressOmniUIDescriptionEventArgs args)
	{
		int clampedUpgradeLevel = runeObj.ClampedUpgradeLevel;
		int equippedLevel = runeObj.EquippedLevel;
		bool isPercent = false;
		string text = equippedLevel + "/" + clampedUpgradeLevel;
		if (args.ButtonType == OmniUIButtonType.Equipping)
		{
			m_text1.text = clampedUpgradeLevel.ToString();
			m_text2.text = text + " " + ColoredValueString(1f, addBrackets: true, isPercent, lowerIsBetter: false);
		}
		else if (args.ButtonType == OmniUIButtonType.Unequipping)
		{
			m_text1.text = clampedUpgradeLevel.ToString();
			m_text2.text = text + " " + ColoredValueString(-1f, addBrackets: true, isPercent, lowerIsBetter: false);
		}
		else
		{
			m_text1.text = clampedUpgradeLevel + " " + ColoredValueString(1f, addBrackets: true, isPercent, lowerIsBetter: false);
			m_text2.text = text.ToString();
		}
	}

	private void SetStatString(RuneObj runeObj, EnchantressOmniUIDescriptionEventArgs args)
	{
		string @string = LocalizationManager.GetString(runeObj.RuneData.Controls, isFemale: false);
		m_titleText.text = @string;
		float currentStatModTotal_ = runeObj.CurrentStatModTotal_1;
		float value = runeObj.GetStatModTotal_1AtLevel(runeObj.EquippedLevel + 1) - currentStatModTotal_;
		float value2 = runeObj.GetStatModTotal_1AtLevel(runeObj.EquippedLevel - 1) - currentStatModTotal_;
		bool lowerIsBetter = runeObj.GetStatModTotal_1AtLevel(1) < 0f;
		if (runeObj.IsMaxEquippedLevel)
		{
			m_text1.text = LocalizationManager.GetString("LOC_ID_GENERAL_UI_MAX_1", isFemale: false);
		}
		else
		{
			m_text1.text = PlusSymbolString(value, isPercent: false);
		}
		bool flag = CDGHelper.IsPercent(runeObj.GetStatModTotal_1AtLevel(1));
		string text = currentStatModTotal_.ToCIString();
		if (flag)
		{
			text = PercentString(currentStatModTotal_);
		}
		OmniUIButtonType buttonType = args.ButtonType;
		if (buttonType == OmniUIButtonType.Equipping || buttonType != OmniUIButtonType.Unequipping)
		{
			m_text2.text = text + " " + ColoredValueString(value, addBrackets: true, flag, lowerIsBetter, hideZero: false);
		}
		else
		{
			m_text2.text = text + " " + ColoredValueString(value2, addBrackets: true, flag, lowerIsBetter, hideZero: false);
		}
	}

	private void SetWeightString(RuneObj runeObj, EnchantressOmniUIDescriptionEventArgs args)
	{
		int num = 999;
		if (PlayerManager.IsInstantiated)
		{
			num = PlayerManager.GetPlayerController().ActualRuneWeight;
		}
		int totalEquippedWeight = RuneManager.GetTotalEquippedWeight();
		int currentWeight = runeObj.CurrentWeight;
		int num2 = runeObj.GetWeightAtLevel(runeObj.EquippedLevel + 1) - currentWeight;
		int num3 = runeObj.GetWeightAtLevel(runeObj.EquippedLevel - 1) - currentWeight;
		bool lowerIsBetter = true;
		if (runeObj.IsMaxEquippedLevel)
		{
			m_text1.text = "0";
		}
		else
		{
			m_text1.text = PlusSymbolString(num2, isPercent: false);
		}
		string text = totalEquippedWeight + "/" + num;
		OmniUIButtonType buttonType = args.ButtonType;
		if (buttonType == OmniUIButtonType.Equipping || buttonType != OmniUIButtonType.Unequipping)
		{
			m_text2.text = text + " " + ColoredValueString(num2, addBrackets: true, isPercent: false, lowerIsBetter, hideZero: false);
		}
		else
		{
			m_text2.text = text + " " + ColoredValueString(num3, addBrackets: true, isPercent: false, lowerIsBetter, hideZero: false);
		}
	}
}
