public class BossDrop
{
	public int BlueprintCount { get; private set; }

	public int GoldAmount { get; private set; }

	public int OreCount { get; private set; }

	public int RuneOreCount { get; private set; }

	public BossDrop(int goldAmount, int blueprintCount, int runeOreCount, int oreCount)
	{
		GoldAmount = goldAmount;
		BlueprintCount = blueprintCount;
		RuneOreCount = runeOreCount;
		OreCount = oreCount;
	}
}
