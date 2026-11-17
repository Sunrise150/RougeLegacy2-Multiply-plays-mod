public struct AbilityDrop : IAbilityDrop, ISpecialItemDrop
{
	public CastAbilityType CastAbilityType { get; private set; }

	public AbilityType AbilityType { get; private set; }

	public SpecialItemType SpecialItemType => SpecialItemType.Ability;

	public AbilityDrop(AbilityType abilityType, CastAbilityType castAbilityType)
	{
		AbilityType = abilityType;
		CastAbilityType = castAbilityType;
	}
}
