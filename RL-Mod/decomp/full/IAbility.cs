public interface IAbility
{
	AbilityData AbilityData { get; }

	AbilityType AbilityType { get; }

	CastAbilityType CastAbilityType { get; }

	int CurrentAmmo { get; }

	int MaxAmmo { get; }

	int BaseCost { get; }

	bool DealsNoDamage { get; }

	bool ForceTriggerCrit { get; set; }
}
