using System;

public class BiomeEventArgs : EventArgs
{
	public BiomeType Biome { get; private set; }

	public BiomeEventArgs()
	{
		Biome = BiomeType.None;
	}

	public BiomeEventArgs(BiomeType biome)
	{
		SetBiome(biome);
	}

	public void SetBiome(BiomeType biome)
	{
		Biome = biome;
	}
}
