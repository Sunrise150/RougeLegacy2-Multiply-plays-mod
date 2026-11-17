using System;

public class PlayerBlockedEventArgs : EventArgs
{
	public IDamageObj DamageObj { get; private set; }

	public PlayerBlockedEventArgs(IDamageObj damageObj)
	{
		Initialize(damageObj);
	}

	public void Initialize(IDamageObj damageObj)
	{
		DamageObj = damageObj;
	}
}
