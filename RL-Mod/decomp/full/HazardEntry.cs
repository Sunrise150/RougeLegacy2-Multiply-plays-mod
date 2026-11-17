using System;

[Serializable]
public class HazardEntry
{
	public BiomeType Biome;

	public HazardType[] Hazards = new HazardType[0];
}
