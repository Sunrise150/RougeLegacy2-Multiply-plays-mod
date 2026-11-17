public class BlacksmithOmniUIPurchaseBoxEntry : BaseOmniUIPurchaseBoxEntry<BlacksmithOmniUIDescriptionEventArgs>
{
	protected override void DisplayNullPurchaseBox()
	{
		base.DisplayNullPurchaseBox();
		switch (m_descriptionType)
		{
		case OmniUIPurchaseBoxType.FlavourText:
			m_text1.text = LocalizationManager.GetString("LOC_ID_BLACKSMITH_PURCHASE_WARNING_1", isFemale: false);
			break;
		case OmniUIPurchaseBoxType.MoneyOwned:
		{
			int equipmentOreCollected = SaveManager.PlayerSaveData.EquipmentOreCollected;
			m_text1.text = SaveManager.PlayerSaveData.GetActualAvailableGoldString();
			m_text2.text = equipmentOreCollected.ToString();
			break;
		}
		case OmniUIPurchaseBoxType.MoneyCost:
			m_text1.text = "???";
			m_text2.text = "???";
			break;
		}
	}

	protected override void DisplayPurchaseBox(BlacksmithOmniUIDescriptionEventArgs args)
	{
		EquipmentCategoryType categoryType = args.CategoryType;
		EquipmentType equipmentType = args.EquipmentType;
		EquipmentObj equipment = EquipmentManager.GetEquipment(categoryType, equipmentType);
		if (equipment == null)
		{
			return;
		}
		switch (m_descriptionType)
		{
		case OmniUIPurchaseBoxType.MoneyOwned:
		{
			int equipmentOreCollected = SaveManager.PlayerSaveData.EquipmentOreCollected;
			m_text1.text = SaveManager.PlayerSaveData.GetActualAvailableGoldString();
			m_text2.text = equipmentOreCollected.ToString();
			break;
		}
		case OmniUIPurchaseBoxType.MoneyCost:
		{
			int goldCostToUpgrade = equipment.GoldCostToUpgrade;
			int oreCostToUpgrade = equipment.OreCostToUpgrade;
			bool flag = args.ButtonType == OmniUIButtonType.Purchasing || args.ButtonType == OmniUIButtonType.Upgrading;
			if ((!equipment.IsMaxUpgradeLevel || equipment.FoundState < FoundState.Purchased) && flag)
			{
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
