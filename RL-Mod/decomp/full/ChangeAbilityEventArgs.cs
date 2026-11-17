using System;

public class ChangeAbilityEventArgs : EventArgs
{
	public CastAbilityType CastAbilityType { get; private set; }

	public IAbility Ability { get; private set; }

	public ChangeAbilityEventArgs(CastAbilityType castAbilityType, IAbility ability)
	{
		Initialise(castAbilityType, ability);
	}

	public void Initialise(CastAbilityType castAbilityType, IAbility ability)
	{
		CastAbilityType = castAbilityType;
		Ability = ability;
	}
}
