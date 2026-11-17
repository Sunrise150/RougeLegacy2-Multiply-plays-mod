using System;
using System.Collections.Generic;
using UnityEngine;

public static class BiomeUtility
{
	private static Dictionary<BiomeType, BiomeLayer> m_biomeTypeToLayerTable = new Dictionary<BiomeType, BiomeLayer>
	{
		{
			BiomeType.None,
			BiomeLayer.None
		},
		{
			BiomeType.Any,
			BiomeLayer.Any
		},
		{
			BiomeType.Castle,
			BiomeLayer.Castle
		},
		{
			BiomeType.Cave,
			BiomeLayer.Cave
		},
		{
			BiomeType.Dragon,
			BiomeLayer.Dragon
		},
		{
			BiomeType.Forest,
			BiomeLayer.Forest
		},
		{
			BiomeType.Garden,
			BiomeLayer.Garden
		},
		{
			BiomeType.DriftHouse,
			BiomeLayer.DriftHouse
		},
		{
			BiomeType.HubTown,
			BiomeLayer.HubTown
		},
		{
			BiomeType.Lake,
			BiomeLayer.Lake
		},
		{
			BiomeType.Stone,
			BiomeLayer.Stone
		},
		{
			BiomeType.Study,
			BiomeLayer.Study
		},
		{
			BiomeType.Sunken,
			BiomeLayer.Sunken
		},
		{
			BiomeType.Tower,
			BiomeLayer.Tower
		},
		{
			BiomeType.TowerExterior,
			BiomeLayer.Tower
		},
		{
			BiomeType.Town,
			BiomeLayer.Town
		},
		{
			BiomeType.Tutorial,
			BiomeLayer.Tutorial
		}
	};

	private static Dictionary<BiomeLayer, BiomeType> m_biomeLayerToTypeTable = new Dictionary<BiomeLayer, BiomeType>
	{
		{
			BiomeLayer.None,
			BiomeType.None
		},
		{
			BiomeLayer.Any,
			BiomeType.Any
		},
		{
			BiomeLayer.Castle,
			BiomeType.Castle
		},
		{
			BiomeLayer.Cave,
			BiomeType.Cave
		},
		{
			BiomeLayer.Dragon,
			BiomeType.Dragon
		},
		{
			BiomeLayer.Forest,
			BiomeType.Forest
		},
		{
			BiomeLayer.Garden,
			BiomeType.Garden
		},
		{
			BiomeLayer.DriftHouse,
			BiomeType.DriftHouse
		},
		{
			BiomeLayer.HubTown,
			BiomeType.HubTown
		},
		{
			BiomeLayer.Lake,
			BiomeType.Lake
		},
		{
			BiomeLayer.Stone,
			BiomeType.Stone
		},
		{
			BiomeLayer.Study,
			BiomeType.Study
		},
		{
			BiomeLayer.Sunken,
			BiomeType.Sunken
		},
		{
			BiomeLayer.Tower,
			BiomeType.Tower
		},
		{
			BiomeLayer.Town,
			BiomeType.Town
		},
		{
			BiomeLayer.Tutorial,
			BiomeType.Tutorial
		}
	};

	private static Dictionary<BiomeType, List<BiomeType>> m_biomeToBiomesThatConnectTable = null;

	private static Dictionary<BiomeType, List<SpecialRoomType>> m_biomeToPotentialSpecialRoomTypesTable = new Dictionary<BiomeType, List<SpecialRoomType>>();

	public static bool IsBiomeInBiomeLayerMask(BiomeType biome, BiomeLayer biomeLayerMask)
	{
		if (m_biomeTypeToLayerTable.ContainsKey(biome))
		{
			if ((m_biomeTypeToLayerTable[biome] & biomeLayerMask) != 0)
			{
				return true;
			}
			return false;
		}
		Debug.LogWarningFormat("{0}: Biome ({1}) is not in Table", Time.frameCount, biome);
		return false;
	}

	public static BiomeType GetBiomeType(BiomeLayer biomeLayer)
	{
		if (m_biomeLayerToTypeTable.ContainsKey(biomeLayer))
		{
			return m_biomeLayerToTypeTable[biomeLayer];
		}
		throw new ArgumentException("biomeLayer", $"Table doesn't contain an entry with Key ({biomeLayer})");
	}

	public static BiomeLayer GetBiomeLayer(BiomeType biomeType)
	{
		if (m_biomeTypeToLayerTable.ContainsKey(biomeType))
		{
			return m_biomeTypeToLayerTable[biomeType];
		}
		throw new ArgumentException("biomeType", $"Table doesn't contain an entry with Key ({biomeType})");
	}

	public static List<BiomeType> GetAllBiomesThatConnectToBiome(BiomeType biome)
	{
		if (m_biomeToBiomesThatConnectTable == null)
		{
			m_biomeToBiomesThatConnectTable = new Dictionary<BiomeType, List<BiomeType>>();
		}
		if (!m_biomeToBiomesThatConnectTable.ContainsKey(biome))
		{
			m_biomeToBiomesThatConnectTable.Add(biome, new List<BiomeType>());
			foreach (BiomeType value in Enum.GetValues(typeof(BiomeType)))
			{
				if (IsConnectsToBiome(value, biome))
				{
					m_biomeToBiomesThatConnectTable[biome].Add(value);
				}
			}
		}
		return m_biomeToBiomesThatConnectTable[biome];
	}

	public static bool IsConnectsToBiome(BiomeType biome, BiomeType connectsTo)
	{
		if (biome == BiomeType.None || biome == BiomeType.Any)
		{
			return false;
		}
		BiomeData data = BiomeDataLibrary.GetData(biome);
		if ((bool)data)
		{
			return data.ConnectsTo == connectsTo;
		}
		if (BiomeType_RL.IsValidBiome(biome))
		{
			Debug.LogFormat("Failed to find BiomeData for Biome ({0})", biome);
		}
		return false;
	}

	public static BiomeType GetConnectsToBiome(BiomeType biome)
	{
		if (biome == BiomeType.None || biome == BiomeType.Any)
		{
			throw new ArgumentException("biome");
		}
		BiomeData data = BiomeDataLibrary.GetData(biome);
		if ((bool)data)
		{
			return data.ConnectsTo;
		}
		Debug.LogFormat("Failed to find BiomeData for Biome ({0})", biome);
		return BiomeType.None;
	}

	public static RoomSide GetConnectDirection(BiomeType biome)
	{
		if (biome == BiomeType.None || biome == BiomeType.Any)
		{
			throw new ArgumentException("biome");
		}
		BiomeData data = BiomeDataLibrary.GetData(biome);
		if ((bool)data)
		{
			return data.ConnectDirection;
		}
		throw new ArgumentException($"Failed to find BiomeData for Biome ({biome})");
	}

	public static List<SpecialRoomType> GetPotentialSpecialRoomTypesInBiome(BiomeType biome)
	{
		List<SpecialRoomType> list = new List<SpecialRoomType>();
		BiomeData data = BiomeDataLibrary.GetData(biome);
		if (data.BonusRoomWeights != null && data.BonusRoomWeights.Length != 0)
		{
			for (int i = 0; i < data.BonusRoomWeights.Length; i++)
			{
				if (data.BonusRoomWeights[i].Weight != 0 && !list.Contains(data.BonusRoomWeights[i].Type))
				{
					list.Add(data.BonusRoomWeights[i].Type);
				}
			}
		}
		SpecialRoomType[] specialRoomTypeArray = RoomType_RL.SpecialRoomTypeArray;
		foreach (SpecialRoomType specialRoomType in specialRoomTypeArray)
		{
			if (BiomeCreation_EV.GetDefaultSpecialRoomWeight(specialRoomType) != 0 && !list.Contains(specialRoomType))
			{
				list.Add(specialRoomType);
			}
		}
		if (!m_biomeToPotentialSpecialRoomTypesTable.ContainsKey(biome))
		{
			m_biomeToPotentialSpecialRoomTypesTable.Add(biome, list);
		}
		return m_biomeToPotentialSpecialRoomTypesTable[biome];
	}
}
