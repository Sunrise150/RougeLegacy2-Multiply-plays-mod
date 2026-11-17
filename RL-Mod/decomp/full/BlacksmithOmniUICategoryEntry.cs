public class BlacksmithOmniUICategoryEntry : BaseOmniUICategoryEntry
{
	public EquipmentCategoryType CategoryType { get; protected set; }

	public void Initialize(EquipmentCategoryType categoryType, int entryIndex, BlacksmithOmniUIWindowController windowController)
	{
		CategoryType = categoryType;
		Initialize(entryIndex, windowController);
		m_iconSprite.sprite = IconLibrary.GetEquipmentCategoryIcon(categoryType);
	}

	public override void UpdateState()
	{
		bool flag = false;
		bool flag2 = false;
		EquipmentType[] typeArray = EquipmentType_RL.TypeArray;
		foreach (EquipmentType equipmentType in typeArray)
		{
			if (equipmentType == EquipmentType.None)
			{
				continue;
			}
			EquipmentData equipmentData = EquipmentLibrary.GetEquipmentData(CategoryType, equipmentType);
			if ((bool)equipmentData && !equipmentData.Disabled)
			{
				if (EquipmentManager.GetFoundState(CategoryType, equipmentType) == FoundState.FoundButNotViewed)
				{
					flag = true;
					break;
				}
				if (EquipmentManager.CanPurchaseEquipment(CategoryType, equipmentType))
				{
					flag2 = true;
					break;
				}
			}
		}
		if (flag)
		{
			if (!m_newSymbol.gameObject.activeSelf)
			{
				m_newSymbol.gameObject.SetActive(value: true);
			}
			if (m_upgradeSymbol.gameObject.activeSelf)
			{
				m_upgradeSymbol.gameObject.SetActive(value: false);
			}
		}
		else if (flag2)
		{
			if (m_newSymbol.gameObject.activeSelf)
			{
				m_newSymbol.gameObject.SetActive(value: false);
			}
			if (!m_upgradeSymbol.gameObject.activeSelf)
			{
				m_upgradeSymbol.gameObject.SetActive(value: true);
			}
		}
		else
		{
			if (m_newSymbol.gameObject.activeSelf)
			{
				m_newSymbol.gameObject.SetActive(value: false);
			}
			if (m_upgradeSymbol.gameObject.activeSelf)
			{
				m_upgradeSymbol.gameObject.SetActive(value: false);
			}
		}
	}
}
