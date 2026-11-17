using System;

[Flags]
public enum ProjectileCollisionFlag
{
	None = 0,
	ResonantBounceable = 1,
	VoidDashable = 2,
	WeaponStrikeable = 4,
	ReflectWeak = 8,
	ReflectStrong = 0x10,
	Bounceable = 0x20,
	All = -1
}
