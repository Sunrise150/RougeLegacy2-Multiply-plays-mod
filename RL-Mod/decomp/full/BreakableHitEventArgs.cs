using System;

public class BreakableHitEventArgs : EventArgs
{
	public Breakable Breakable { get; private set; }

	public IDamageObj DamageObject { get; private set; }

	public BreakableHitEventArgs(Breakable breakable, IDamageObj damageObject)
	{
		Initialize(breakable, damageObject);
	}

	public void Initialize(Breakable breakable, IDamageObj damageObject)
	{
		Breakable = breakable;
		DamageObject = damageObject;
	}
}
