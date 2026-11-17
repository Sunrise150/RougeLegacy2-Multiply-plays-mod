using System;

public class AbilityCooldownOverEventArgs : EventArgs
{
	public IAbility Ability { get; private set; }

	public AbilityCooldownOverEventArgs(IAbility ability)
	{
		Initialise(ability);
	}

	public void Initialise(IAbility ability)
	{
		Ability = ability;
	}
}
