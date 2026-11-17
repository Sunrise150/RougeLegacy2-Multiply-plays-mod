using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Equipment Set Library")]
public class EquipmentSetLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/EquipmentSetLibrary";

	[SerializeField]
	private EquipmentTypeEquipmentSetDataDictionary m_equipmentSetLibrary;

	[SerializeField]
	private EquipmentSetBonusTypeStringDictionary m_equipmentSetBonusLocIDLibrary;

	private static EquipmentSetLibrary m_instance;

	private static EquipmentSetLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<EquipmentSetLibrary>("Scriptable Objects/Libraries/EquipmentSetLibrary");
			}
			return m_instance;
		}
	}

	public static EquipmentSetData GetEquipmentSetData(EquipmentType equipType)
	{
		if (Instance.m_equipmentSetLibrary.TryGetValue(equipType, out var value))
		{
			return value;
		}
		Debug.LogWarningFormat("<color=red>{0}: ({1}) Could not find EquipmentSetData ({2}) in EquipmentSetData Library.</color>", Time.frameCount, Instance, equipType);
		return null;
	}

	public static string GetEquipmentSetBonusLocID(EquipmentSetBonusType bonusType)
	{
		if (Instance.m_equipmentSetBonusLocIDLibrary.TryGetValue(bonusType, out var value))
		{
			return value;
		}
		Debug.LogWarningFormat("<color=red>{0}: ({1}) Could not find EquipmentSetBonus LocID ({2}) in EquipmentSetData Library.</color>", Time.frameCount, Instance, bonusType);
		return null;
	}
}
