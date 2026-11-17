using System;

public class ForceManaRegenEventArgs : EventArgs
{
	public float RegenAmount { get; private set; }

	public bool UsePlayerRegenMods { get; private set; }

	public ForceManaRegenEventArgs(float regenAmount, bool usePlayerRegenMods)
	{
		Initialise(regenAmount, usePlayerRegenMods);
	}

	public void Initialise(float regenAmount, bool usePlayerRegenMods)
	{
		RegenAmount = regenAmount;
		UsePlayerRegenMods = usePlayerRegenMods;
	}
}
