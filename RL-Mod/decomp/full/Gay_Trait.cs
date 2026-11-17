public class Gay_Trait : BaseTrait
{
	public override TraitType TraitType => TraitType.Disposition;

	public static string GetDispositionLocID(CharacterData charData, bool getDescription2 = false)
	{
		if (!getDescription2)
		{
			return "LOC_ID_TRAIT_DESCRIPTION_Gay_" + (charData.Disposition_ID + 1);
		}
		return "LOC_ID_TRAIT_DESCRIPTION2_Gay_" + (charData.Disposition_ID + 1);
	}
}
