using System;

public class CooldownEventArgs : EventArgs
{
	public ICooldown Cooldown { get; private set; }

	public CooldownEventArgs(ICooldown cooldown)
	{
		Initialize(cooldown);
	}

	public void Initialize(ICooldown cooldown)
	{
		Cooldown = cooldown;
	}
}
