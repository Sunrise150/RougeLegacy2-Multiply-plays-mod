public class SoulShopOmniUIPurchaseBoxEntry : BaseOmniUIPurchaseBoxEntry<SoulShopOmniUIDescriptionEventArgs>
{
	public int CurrentNewGamePlusLevel => NewGamePlusOmniUIWindowController.CurrentNewGamePlusLevel;

	protected override void DisplayNullPurchaseBox()
	{
		if (m_descriptionType != OmniUIPurchaseBoxType.FlavourText)
		{
			base.DisplayNullPurchaseBox();
		}
		switch (m_descriptionType)
		{
		case OmniUIPurchaseBoxType.MoneyOwned:
		{
			int totalSoulsCollected = Souls_EV.GetTotalSoulsCollected(GameModeType.Regular, deductSpentSouls: true);
			m_text1.text = totalSoulsCollected.ToString();
			break;
		}
		case OmniUIPurchaseBoxType.MoneyCost:
			m_text1.text = LocalizationManager.GetString("LOC_ID_GENERAL_UI_NA_1", isFemale: false);
			break;
		case OmniUIPurchaseBoxType.FlavourText:
			break;
		}
	}

	protected override void DisplayPurchaseBox(SoulShopOmniUIDescriptionEventArgs args)
	{
		SoulShopType soulShopType = args.SoulShopType;
		SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(soulShopType);
		if (soulShopObj.IsNativeNull())
		{
			return;
		}
		switch (m_descriptionType)
		{
		case OmniUIPurchaseBoxType.MoneyOwned:
		{
			int totalSoulsCollected = Souls_EV.GetTotalSoulsCollected(GameModeType.Regular, deductSpentSouls: true);
			m_text1.text = totalSoulsCollected.ToString();
			break;
		}
		case OmniUIPurchaseBoxType.MoneyCost:
		{
			int num = soulShopObj.CurrentCost(applyMaxLevelScalingCap: true);
			if (soulShopType == SoulShopType.AetherOreSwap || soulShopType == SoulShopType.OreAetherSwap || soulShopType == SoulShopType.SoulSwap || soulShopObj.CurrentOwnedLevel >= soulShopObj.MaxLevel)
			{
				m_text1.text = LocalizationManager.GetString("LOC_ID_GENERAL_UI_NA_1", isFemale: false);
			}
			else
			{
				m_text1.text = num.ToString();
			}
			break;
		}
		case OmniUIPurchaseBoxType.FlavourText:
			break;
		}
	}
}
