using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/SoulShopLibrary")]
public class SoulShopLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/SoulShopLibrary";

	[SerializeField]
	private SoulShopTypeSoulShopDataDictionary m_soulShopLibrary;

	private static SoulShopLibrary m_instance;

	private static SoulShopLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<SoulShopLibrary>("Scriptable Objects/Libraries/SoulShopLibrary");
			}
			return m_instance;
		}
	}

	public static SoulShopData GetSoulShopData(SoulShopType soulShopType)
	{
		if (Instance.m_soulShopLibrary.TryGetValue(soulShopType, out var value))
		{
			return value;
		}
		Debug.LogWarningFormat("<color=red>{0}: ({1}) Could not find SoulShopData ({2}) in SoulShop Library.</color>", Time.frameCount, Instance, value);
		return null;
	}
}
