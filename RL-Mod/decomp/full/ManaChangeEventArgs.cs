using System;

public class ManaChangeEventArgs : EventArgs
{
	public PlayerController Player { get; private set; }

	public float NewManaValue { get; private set; }

	public float PrevManaValue { get; private set; }

	public ManaChangeEventArgs(PlayerController player, float newMana, float prevMana)
	{
		Initialise(player, newMana, prevMana);
	}

	public void Initialise(PlayerController player, float newMana, float prevMana)
	{
		NewManaValue = newMana;
		PrevManaValue = prevMana;
		Player = player;
	}
}
