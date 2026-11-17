public struct WeaponDrop : IWeaponDrop, ISpecialItemDrop
{
	public SpecialItemType SpecialItemType => SpecialItemType.Weapon;

	public AbilityType WeaponType { get; private set; }

	public WeaponDrop(AbilityType weaponType)
	{
		WeaponType = weaponType;
	}
}
