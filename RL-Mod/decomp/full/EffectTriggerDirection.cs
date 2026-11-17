using System;

[Flags]
public enum EffectTriggerDirection
{
	None = 0,
	MovingLeft = 2,
	MovingRight = 4,
	MovingDown = 8,
	MovingUp = 0x10,
	StandingOn = 0x20,
	Anywhere = -1
}
