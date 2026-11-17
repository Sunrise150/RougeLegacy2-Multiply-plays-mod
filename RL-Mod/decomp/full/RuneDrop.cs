public struct RuneDrop : IRuneDrop, ISpecialItemDrop
{
	public SpecialItemType SpecialItemType => SpecialItemType.Rune;

	public RuneType RuneType { get; private set; }

	public RuneDrop(RuneType runeType)
	{
		RuneType = runeType;
	}
}
