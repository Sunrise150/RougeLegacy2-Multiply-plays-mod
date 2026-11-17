public struct BlueprintDrop : IBlueprintDrop, ISpecialItemDrop
{
	public EquipmentCategoryType CategoryType { get; private set; }

	public EquipmentType EquipmentType { get; private set; }

	public SpecialItemType SpecialItemType => SpecialItemType.Blueprint;

	public BlueprintDrop(EquipmentCategoryType categoryType, EquipmentType equipmentType)
	{
		CategoryType = categoryType;
		EquipmentType = equipmentType;
	}
}
