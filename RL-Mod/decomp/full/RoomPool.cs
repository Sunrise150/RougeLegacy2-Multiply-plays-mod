using System.Collections.Generic;
using System.Linq;
using Rooms;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Level Editor/Room Pool")]
public class RoomPool : ScriptableObject
{
	[SerializeField]
	private CompiledScene_ScriptableObject[] m_trapRoomPool;

	[SerializeField]
	private CompiledScene_ScriptableObject[] m_fairyRoomPool;

	[SerializeField]
	private CompiledScene_ScriptableObject[] m_specialRoomPool;

	[SerializeField]
	private List<RoomPoolEntry> m_biomeRoomPools = new List<RoomPoolEntry>
	{
		new RoomPoolEntry(BiomeType.Castle),
		new RoomPoolEntry(BiomeType.Cave),
		new RoomPoolEntry(BiomeType.Dragon),
		new RoomPoolEntry(BiomeType.Editor),
		new RoomPoolEntry(BiomeType.Forest),
		new RoomPoolEntry(BiomeType.Garden),
		new RoomPoolEntry(BiomeType.Lake),
		new RoomPoolEntry(BiomeType.Stone),
		new RoomPoolEntry(BiomeType.Study),
		new RoomPoolEntry(BiomeType.Sunken),
		new RoomPoolEntry(BiomeType.Tower),
		new RoomPoolEntry(BiomeType.TowerExterior),
		new RoomPoolEntry(BiomeType.Town)
	};

	public static string RESOURCES_PATH = "Scriptable Objects/RoomPool";

	public static string ASSET_PATH = "Assets/Content/" + RESOURCES_PATH + ".asset";

	public List<RoomPoolEntry> BiomeRoomPools
	{
		get
		{
			return m_biomeRoomPools;
		}
		set
		{
			m_biomeRoomPools = value;
		}
	}

	public CompiledScene_ScriptableObject[] TrapRoomPool => m_trapRoomPool;

	public CompiledScene_ScriptableObject[] FairyRoomPool => m_fairyRoomPool;

	public CompiledScene_ScriptableObject[] SpecialRoomPool => m_specialRoomPool;

	public CompiledScene_ScriptableObject[] GetCompiledScenes(BiomeType biomeType, RoomType roomType)
	{
		RoomPoolEntry roomPoolEntry = BiomeRoomPools.SingleOrDefault((RoomPoolEntry entry) => entry.Biome == biomeType);
		if (roomPoolEntry != null)
		{
			switch (roomType)
			{
			case RoomType.Standard:
				return roomPoolEntry.StandardRoomPool;
			case RoomType.Fairy:
				if (roomPoolEntry.FairyRoomPoolOverride != null && roomPoolEntry.FairyRoomPoolOverride.Length != 0)
				{
					return roomPoolEntry.FairyRoomPoolOverride;
				}
				return m_fairyRoomPool;
			case RoomType.Trap:
				if (roomPoolEntry.TrapRoomPoolOverride != null && roomPoolEntry.TrapRoomPoolOverride.Length != 0)
				{
					return roomPoolEntry.TrapRoomPoolOverride;
				}
				return m_trapRoomPool;
			case RoomType.Bonus:
				if (roomPoolEntry.SpecialRoomProxies != null && roomPoolEntry.SpecialRoomProxies.Length != 0)
				{
					return roomPoolEntry.SpecialRoomProxies;
				}
				return m_specialRoomPool;
			default:
				return new CompiledScene_ScriptableObject[0];
			}
		}
		return new CompiledScene_ScriptableObject[0];
	}

	public MandatoryRoomEntry[] GetMandatoryRoomEntries(BiomeType biomeType)
	{
		RoomPoolEntry roomPoolEntry = BiomeRoomPools.SingleOrDefault((RoomPoolEntry entry) => entry.Biome == biomeType);
		if (roomPoolEntry != null)
		{
			return roomPoolEntry.MandatoryRoomPool;
		}
		return new MandatoryRoomEntry[0];
	}

	public RoomMetaData GetTransitionRoom(BiomeType biomeType)
	{
		return BiomeRoomPools.SingleOrDefault((RoomPoolEntry entry) => entry.Biome == biomeType)?.TransitionRoomMetaData;
	}
}
