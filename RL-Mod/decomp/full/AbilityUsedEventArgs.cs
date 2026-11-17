using System;

public class AbilityUsedEventArgs : EventArgs
{
	public IAbility Ability { get; private set; }

	public AbilityUsedEventArgs(IAbility ability)
	{
		Initialise(ability);
	}

	public void Initialise(IAbility ability)
	{
		Ability = ability;
	}
}
