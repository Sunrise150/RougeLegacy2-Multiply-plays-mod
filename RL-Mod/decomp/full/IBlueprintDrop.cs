public interface IBlueprintDrop : ISpecialItemDrop
{
	EquipmentCategoryType CategoryType { get; }

	EquipmentType EquipmentType { get; }
}
