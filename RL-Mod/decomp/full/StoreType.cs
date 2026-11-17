using System;

[Flags]
public enum StoreType
{
	None = 0,
	Steam = 1,
	Epic = 2,
	Xbox = 4,
	PSN = 8,
	All = 0xF
}
