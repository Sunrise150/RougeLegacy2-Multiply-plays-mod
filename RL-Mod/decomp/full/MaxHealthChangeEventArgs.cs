using System;

public class MaxHealthChangeEventArgs : EventArgs
{
	public float NewMaxHealthValue { get; private set; }

	public float PrevMaxHealthValue { get; private set; }

	public MaxHealthChangeEventArgs(float newMaxHealth, float prevMaxHealth)
	{
		Initialise(newMaxHealth, prevMaxHealth);
	}

	public void Initialise(float newMaxHealth, float prevMaxHealth)
	{
		NewMaxHealthValue = newMaxHealth;
		PrevMaxHealthValue = prevMaxHealth;
	}
}
