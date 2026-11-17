using System;
using System.Collections.Generic;

[Serializable]
public class Hazards
{
	public HazardEntry[] HazardsInBiomes;

	public HazardType[] DefaultHazardsInBiomes;

	private Dictionary<BiomeType, HazardType[]> m_hazardsInBiomeTable;

	public Dictionary<BiomeType, HazardType[]> HazardsInBiomeTable
	{
		get
		{
			if (m_hazardsInBiomeTable == null)
			{
				InitialiseHazardTable();
			}
			return m_hazardsInBiomeTable;
		}
	}

	private void InitialiseHazardTable()
	{
		m_hazardsInBiomeTable = new Dictionary<BiomeType, HazardType[]>();
		HazardEntry[] hazardsInBiomes = HazardsInBiomes;
		foreach (HazardEntry hazardEntry in hazardsInBiomes)
		{
			m_hazardsInBiomeTable.Add(hazardEntry.Biome, hazardEntry.Hazards);
		}
	}
}
