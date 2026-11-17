public struct RelicDrop : IRelicDrop, ISpecialItemDrop
{
	public RelicType RelicType { get; private set; }

	public RelicModType RelicModType { get; private set; }

	public SpecialItemType SpecialItemType => SpecialItemType.Relic;

	public RelicDrop(RelicType relicType, RelicModType relicModType)
	{
		RelicType = relicType;
		RelicModType = relicModType;
	}
}
