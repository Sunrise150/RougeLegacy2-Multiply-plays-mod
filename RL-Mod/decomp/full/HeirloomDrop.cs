public struct HeirloomDrop : IHeirloomDrop, ISpecialItemDrop
{
	public SpecialItemType SpecialItemType => SpecialItemType.Heirloom;

	public HeirloomType HeirloomType { get; private set; }

	public HeirloomDrop(HeirloomType heirloomType)
	{
		HeirloomType = heirloomType;
	}
}
