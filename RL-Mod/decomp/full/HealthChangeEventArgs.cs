using System;

public class HealthChangeEventArgs : EventArgs
{
	public IHealth HealthObj { get; private set; }

	public float NewHealthValue { get; private set; }

	public float PrevHealthValue { get; private set; }

	public HealthChangeEventArgs(IHealth healthObj, float newHealth, float prevHealth)
	{
		Initialise(healthObj, newHealth, prevHealth);
	}

	public void Initialise(IHealth healthObj, float newHealth, float prevHealth)
	{
		HealthObj = healthObj;
		NewHealthValue = newHealth;
		PrevHealthValue = prevHealth;
	}
}
