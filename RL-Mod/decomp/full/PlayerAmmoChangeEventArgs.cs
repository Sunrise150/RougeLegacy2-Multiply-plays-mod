using System;

public class PlayerAmmoChangeEventArgs : EventArgs
{
	public IAbility AbilityObj { get; private set; }

	public float NewAmmoValue { get; private set; }

	public float PrevAmmoValue { get; private set; }

	public PlayerAmmoChangeEventArgs(IAbility ability, float newAmmo, float prevAmmo)
	{
		Initialize(ability, newAmmo, prevAmmo);
	}

	public void Initialize(IAbility ability, float newAmmo, float prevAmmo)
	{
		AbilityObj = ability;
		NewAmmoValue = newAmmo;
		PrevAmmoValue = prevAmmo;
	}
}
