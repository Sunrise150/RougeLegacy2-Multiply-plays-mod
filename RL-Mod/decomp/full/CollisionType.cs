using System;

[Flags]
public enum CollisionType
{
	None = 0,
	Player = 1,
	PlayerProjectile = 2,
	Enemy = 4,
	EnemyProjectile = 8,
	ItemDrop = 0x10,
	Breakable = 0x20,
	Hazard = 0x40,
	Platform = 0x80,
	Chest = 0x100,
	NPC = 0x200,
	TriggerHazard = 0x400,
	Player_Dodging = 0x800,
	FlimsyBreakable = 0x4000,
	Generic_Bounceable = 0x8000,
	NonResonant_Bounceable = 0x10000,
	All = -1
}
