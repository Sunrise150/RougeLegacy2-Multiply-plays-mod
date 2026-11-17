public interface IAbilityDrop : ISpecialItemDrop
{
	AbilityType AbilityType { get; }

	CastAbilityType CastAbilityType { get; }
}
