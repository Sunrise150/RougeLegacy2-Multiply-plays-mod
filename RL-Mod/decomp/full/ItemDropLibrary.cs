using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Item Drop Library")]
public class ItemDropLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/ItemDropLibrary";

	[SerializeField]
	private List<ItemDropEntry> m_itemDropEntryList = new List<ItemDropEntry>();

	private static ItemDropLibrary m_instance;

	public static List<ItemDropEntry> ItemDropEntryList => Instance.m_itemDropEntryList;

	public static ItemDropLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<ItemDropLibrary>("Scriptable Objects/Libraries/ItemDropLibrary");
			}
			return m_instance;
		}
	}
}
