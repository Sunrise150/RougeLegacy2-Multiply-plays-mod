using System;

public class BlacksmithOmniUIDescriptionEventArgs : EventArgs
{
	public EquipmentCategoryType CategoryType { get; private set; }

	public EquipmentType EquipmentType { get; private set; }

	public OmniUIButtonType ButtonType { get; private set; }

	public BlacksmithOmniUIDescriptionEventArgs(EquipmentCategoryType categoryType, EquipmentType equipType, OmniUIButtonType buttonType)
	{
		Initialize(categoryType, equipType, buttonType);
	}

	public void Initialize(EquipmentCategoryType categoryType, EquipmentType equipType, OmniUIButtonType buttonType)
	{
		CategoryType = categoryType;
		EquipmentType = equipType;
		ButtonType = buttonType;
	}
}
