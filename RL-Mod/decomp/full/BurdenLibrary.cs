using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/BurdenLibrary")]
public class BurdenLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/BurdenLibrary";

	[SerializeField]
	private BurdenTypeBurdenDataDictionary m_burdenLibrary;

	private static BurdenLibrary m_instance;

	private static BurdenLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<BurdenLibrary>("Scriptable Objects/Libraries/BurdenLibrary");
			}
			return m_instance;
		}
	}

	public static BurdenData GetBurdenData(BurdenType burdenType)
	{
		if (Instance.m_burdenLibrary.TryGetValue(burdenType, out var value))
		{
			return value;
		}
		Debug.LogWarningFormat("<color=red>{0}: ({1}) Could not find BurdenData ({2}) in Burden Library.</color>", Time.frameCount, Instance, burdenType);
		return null;
	}
}
