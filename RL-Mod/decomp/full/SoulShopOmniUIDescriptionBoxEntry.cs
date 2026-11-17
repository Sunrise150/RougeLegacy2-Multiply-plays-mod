using RL_Windows;
using UnityEngine;

public class SoulShopOmniUIDescriptionBoxEntry : BaseOmniUIDescriptionBoxEntry<SoulShopOmniUIDescriptionEventArgs, SoulShopOmniUIDescriptionBoxEntry.SoulShopOmniUIDescriptionBoxType>
{
	public enum SoulShopOmniUIDescriptionBoxType
	{
		None,
		Title,
		Description,
		Stats
	}

	protected override void DisplayNullDescriptionBox(MonoBehaviour sender)
	{
		base.DisplayNullDescriptionBox(sender);
		SoulShopType selectedSoulShopType = (WindowManager.GetWindowController(WindowID.SoulShop) as SoulShopOmniUIWindowController).SelectedSoulShopType;
		if (m_descriptionType == SoulShopOmniUIDescriptionBoxType.Title)
		{
			m_titleText.text = "???";
			m_icon.sprite = IconLibrary.GetDefaultSprite();
		}
		else if (m_descriptionType == SoulShopOmniUIDescriptionBoxType.Description)
		{
			SoulShopData soulShopData = SoulShopLibrary.GetSoulShopData(selectedSoulShopType);
			int unlockLevel = soulShopData.UnlockLevel;
			if (SaveManager.ModeSaveData.GetTotalSoulShopObjOwnedLevel() < unlockLevel)
			{
				string @string = LocalizationManager.GetString("LOC_ID_SOULSHOP_MISC_SKILL_LOCKED_1", isFemale: false);
				@string = string.Format(@string, soulShopData.UnlockLevel - SaveManager.ModeSaveData.GetTotalSoulShopObjOwnedLevel());
				m_titleText.text = @string;
			}
		}
		else
		{
			m_iconGO.gameObject.SetActive(value: false);
		}
	}

	protected override void DisplayDescriptionBox(SoulShopOmniUIDescriptionEventArgs args)
	{
		SoulShopType soulShopType = args.SoulShopType;
		if (soulShopType == SoulShopType.None)
		{
			return;
		}
		SoulShopData soulShopData = SoulShopLibrary.GetSoulShopData(soulShopType);
		SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(soulShopType);
		switch (m_descriptionType)
		{
		case SoulShopOmniUIDescriptionBoxType.Title:
		{
			string string2 = LocalizationManager.GetString(soulShopData.Title, isFemale: false);
			m_titleText.text = string2;
			m_icon.sprite = IconLibrary.GetSoulShopIcon(soulShopType);
			break;
		}
		case SoulShopOmniUIDescriptionBoxType.Description:
		{
			string string3 = LocalizationManager.GetString(soulShopData.Description, isFemale: false);
			m_titleText.text = string3;
			break;
		}
		case SoulShopOmniUIDescriptionBoxType.Stats:
		{
			if (soulShopType == SoulShopType.AetherOreSwap || soulShopType == SoulShopType.OreAetherSwap || soulShopType == SoulShopType.SoulSwap)
			{
				m_iconGO.SetActive(value: false);
				m_text2.gameObject.SetActive(value: false);
			}
			else
			{
				m_iconGO.SetActive(value: true);
				m_text2.gameObject.SetActive(value: true);
			}
			string @string = LocalizationManager.GetString(soulShopData.StatsTitle, isFemale: false);
			m_titleText.text = @string;
			if (soulShopType == SoulShopType.AetherOreSwap || soulShopType == SoulShopType.OreAetherSwap || soulShopType == SoulShopType.SoulSwap)
			{
				switch (soulShopType)
				{
				case SoulShopType.OreAetherSwap:
					m_text1.text = $"{SoulShopOmniUIIncrementResourceButton.OreTransferAmount} [EquipmentOre_Icon] : {Mathf.RoundToInt(SoulShopOmniUIIncrementResourceButton.OreTransferAmount / 2)} [RuneOre_Icon]";
					break;
				case SoulShopType.AetherOreSwap:
					m_text1.text = $"{SoulShopOmniUIIncrementResourceButton.AetherTransferAmount} [RuneOre_Icon] : {Mathf.RoundToInt(SoulShopOmniUIIncrementResourceButton.AetherTransferAmount / 2)} [EquipmentOre_Icon]";
					break;
				default:
				{
					int soulSwapCost = Souls_EV.GetSoulSwapCost(SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType.SoulSwap).CurrentOwnedLevel);
					int num = 150;
					m_text1.text = $"{soulSwapCost} [RuneOre_Icon] [EquipmentOre_Icon] : {num} [Soul_Icon]";
					break;
				}
				}
				m_text2.text = LocalizationManager.GetString("LOC_ID_GENERAL_UI_NA_1", isFemale: false);
				break;
			}
			float currentStatGain = soulShopObj.CurrentStatGain;
			float value = soulShopObj.GetStatGainAtLevel(soulShopObj.CurrentEquippedLevel + 1) - currentStatGain;
			bool flag = CDGHelper.IsPercent(soulShopObj.GetStatGainAtLevel(1));
			bool lowerIsBetter = soulShopObj.GetStatGainAtLevel(1) < 0f;
			int maxLevel = soulShopObj.MaxLevel;
			bool num2 = soulShopObj.CurrentEquippedLevel >= maxLevel;
			string text = (flag ? PercentString(currentStatGain) : currentStatGain.ToString());
			string arg = ColoredValueString(value, addBrackets: true, flag, lowerIsBetter);
			if (!num2)
			{
				m_text1.text = $"{text} {arg}";
			}
			else
			{
				m_text1.text = text;
			}
			string text2 = $"{soulShopObj.CurrentOwnedLevel}/{maxLevel}";
			m_text2.text = text2;
			break;
		}
		}
	}
}
