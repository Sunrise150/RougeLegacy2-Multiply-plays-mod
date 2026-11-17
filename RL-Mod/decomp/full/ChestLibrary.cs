using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Chest Library")]
public class ChestLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/ChestLibrary";

	[SerializeField]
	private ChestTypeChestObjDictionary m_chestLibrary;

	private static ChestLibrary m_instance;

	public static ChestLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<ChestLibrary>("Scriptable Objects/Libraries/ChestLibrary");
			}
			return m_instance;
		}
	}

	public static ChestObj GetChestPrefab(ChestType chestType)
	{
		ChestObj value = null;
		if (Instance.m_chestLibrary.TryGetValue(chestType, out value))
		{
			return value;
		}
		Debug.Log("<color=red>Could not find Chest type: " + chestType.ToString() + " in Chest Library.</color>");
		return null;
	}
}
