using System;
using Rooms;
using UnityEngine;

[Serializable]
public class RoomPoolEntry
{
	[SerializeField]
	[ReadOnly]
	private BiomeType m_biome;

	public CompiledScene_ScriptableObject[] StandardRoomPool;

	public MandatoryRoomEntry[] MandatoryRoomPool;

	public RoomMetaData TransitionRoomMetaData;

	public CompiledScene_ScriptableObject[] FairyRoomPoolOverride;

	public CompiledScene_ScriptableObject[] TrapRoomPoolOverride;

	public CompiledScene_ScriptableObject[] SpecialRoomProxies;

	public BiomeType Biome
	{
		get
		{
			return m_biome;
		}
		private set
		{
			m_biome = value;
		}
	}

	public RoomPoolEntry(BiomeType biome)
	{
		Biome = biome;
	}
}
