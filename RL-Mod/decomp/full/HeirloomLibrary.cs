using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Heirloom Library")]
public class HeirloomLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/HeirloomLibrary";

	[SerializeField]
	private HeirloomTypeHeirloomDataDictionary m_heirloomLibrary;

	private static HeirloomLibrary m_instance;

	private static HeirloomLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<HeirloomLibrary>("Scriptable Objects/Libraries/HeirloomLibrary");
			}
			return m_instance;
		}
	}

	public static HeirloomData GetHeirloomData(HeirloomType heirloomType)
	{
		HeirloomData value = null;
		if (Instance.m_heirloomLibrary != null)
		{
			Instance.m_heirloomLibrary.TryGetValue(heirloomType, out value);
			return value;
		}
		throw new Exception("Heirloom Library is null.");
	}
}
