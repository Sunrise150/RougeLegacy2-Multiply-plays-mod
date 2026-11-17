using System.Collections.Generic;
using UnityEngine;

namespace RLAudio;

[CreateAssetMenu(menuName = "Custom/Libraries/Item Drop Audio Library")]
public class ItemDropAudioLibrary : AudioLibrary<ItemDropAudioLibraryEntry>
{
	private static ItemDropAudioLibrary m_instance = null;

	private static Dictionary<ItemDropType, string> m_toStringTable = new Dictionary<ItemDropType, string>();

	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/ItemDropAudioLibrary";

	public const string ASSET_PATH = "Assets/Content/Scriptable Objects/Libraries/ItemDropAudioLibrary.asset";

	private static ItemDropAudioLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<ItemDropAudioLibrary>("Scriptable Objects/Libraries/ItemDropAudioLibrary");
			}
			return m_instance;
		}
	}

	public static ItemDropAudioLibraryEntry GetItemDropAudioLibraryEntry(ItemDropType itemDrop)
	{
		if (!m_toStringTable.ContainsKey(itemDrop))
		{
			m_toStringTable.Add(itemDrop, itemDrop.ToString());
		}
		return Instance.GetAudioLibraryEntry(m_toStringTable[itemDrop]);
	}
}
