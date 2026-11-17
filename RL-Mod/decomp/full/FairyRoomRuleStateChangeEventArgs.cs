using System;

public class FairyRoomRuleStateChangeEventArgs : EventArgs
{
	public FairyRule Rule { get; private set; }

	public FairyRoomRuleStateChangeEventArgs(FairyRule rule)
	{
		Initialise(rule);
	}

	public void Initialise(FairyRule rule)
	{
		Rule = rule;
	}
}
