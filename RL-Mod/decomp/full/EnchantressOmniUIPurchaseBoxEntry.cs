public class EnchantressOmniUIPurchaseBoxEntry : BaseOmniUIPurchaseBoxEntry<EnchantressOmniUIDescriptionEventArgs>
{
	protected override void DisplayNullPurchaseBox()
	{
		base.DisplayNullPurchaseBox();
		switch (m_descriptionType)
		{
		case OmniUIPurchaseBoxType.FlavourText:
			m_text1.text = LocalizationManager.GetString("LOC_ID_ENCHANTRESS_RUNE_PURCHASE_WARNING_1", isFemale: false);
			break;
		case OmniUIPurchaseBoxType.MoneyOwned:
		{
			int runeOreCollected = SaveManager.PlayerSaveData.RuneOreCollected;
			m_text1.text = SaveManager.PlayerSaveData.GetActualAvailableGoldString();
			m_text2.text = runeOreCollected.ToString();
			break;
		}
		case OmniUIPurchaseBoxType.MoneyCost:
			m_text1.text = "???";
			m_text2.text = "???";
			break;
		}
	}

	protected override void DisplayPurchaseBox(EnchantressOmniUIDescriptionEventArgs args)
	{
		RuneObj rune = RuneManager.GetRune(args.RuneType);
		if (rune == null)
		{
			return;
		}
		switch (m_descriptionType)
		{
		case OmniUIPurchaseBoxType.MoneyOwned:
		{
			int runeOreCollected = SaveManager.PlayerSaveData.RuneOreCollected;
			m_text1.text = SaveManager.PlayerSaveData.GetActualAvailableGoldString();
			m_text2.text = runeOreCollected.ToString();
			break;
		}
		case OmniUIPurchaseBoxType.MoneyCost:
		{
			bool flag = args.ButtonType == OmniUIButtonType.Purchasing || args.ButtonType == OmniUIButtonType.Upgrading;
			if (!rune.IsMaxUpgradeLevel && flag)
			{
				int goldCostToUpgrade = rune.GoldCostToUpgrade;
				int oreCostToUpgrade = rune.OreCostToUpgrade;
				m_text1.text = goldCostToUpgrade.ToString();
				m_text2.text = oreCostToUpgrade.ToString();
			}
			else
			{
				m_text1.text = LocalizationManager.GetString("LOC_ID_GENERAL_UI_NA_1", isFemale: false);
				m_text2.text = LocalizationManager.GetString("LOC_ID_GENERAL_UI_NA_1", isFemale: false);
			}
			break;
		}
		case OmniUIPurchaseBoxType.FlavourText:
			break;
		}
	}
}
