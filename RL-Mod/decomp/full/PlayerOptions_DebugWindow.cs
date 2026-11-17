using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PlayerOptions_DebugWindow : MonoBehaviour
{
	[SerializeField]
	private TMP_Dropdown m_weaponDropdown;

	[SerializeField]
	private TMP_Dropdown m_classDropdown;

	private Dictionary<int, ClassType> m_indexToClassTable;

	private ClassType[] m_classes;

	private Dictionary<int, AbilityType> m_indexToWeaponTable;

	private AbilityType[] m_weapons;

	public AbilityType[] Weapons => m_weapons;

	public ClassType[] Classes => m_classes;

	private void Awake()
	{
		InitializeIndexToClassTable();
		InitializeIndexToWeaponTable();
		m_weaponDropdown.ClearOptions();
		int length = "Weapon".Length;
		List<string> list = m_weapons.Select((AbilityType weapon) => weapon.ToString()).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].EndsWith("Weapon"))
			{
				list[i] = list[i].Substring(0, list[i].Length - length);
			}
		}
		m_weaponDropdown.AddOptions(list);
		m_classDropdown.ClearOptions();
		m_classDropdown.AddOptions(m_classes.Select((ClassType classType) => classType.ToString()).ToList());
	}

	private void InitializeIndexToClassTable()
	{
		m_indexToClassTable = new Dictionary<int, ClassType>();
		int num = 0;
		foreach (ClassType value in Enum.GetValues(typeof(ClassType)))
		{
			if (value != 0)
			{
				m_indexToClassTable.Add(num, value);
				num++;
			}
		}
		m_classes = new ClassType[m_indexToClassTable.Count];
		num = 0;
		foreach (KeyValuePair<int, ClassType> item in m_indexToClassTable)
		{
			m_classes[num] = item.Value;
			num++;
		}
	}

	private void InitializeIndexToWeaponTable()
	{
		m_indexToWeaponTable = new Dictionary<int, AbilityType>();
		int num = 0;
		foreach (AbilityType weaponAbility in AbilityType_RL.GetWeaponAbilityList())
		{
			if (weaponAbility != 0)
			{
				m_indexToWeaponTable.Add(num, weaponAbility);
				num++;
			}
		}
		m_weapons = new AbilityType[m_indexToWeaponTable.Count];
		num = 0;
		foreach (KeyValuePair<int, AbilityType> item in m_indexToWeaponTable)
		{
			m_weapons[num] = item.Value;
			num++;
		}
	}

	public void SetPlayerClass(int classIndex)
	{
		ClassType classType = m_classes[classIndex];
		Debug.LogFormat("Set Player Class to {0}. NOT IMPLEMENTED AT THIS TIME.", classType);
	}

	public void SetPlayerWeapon(int weaponIndex)
	{
		AbilityType abilityType = m_weapons[weaponIndex];
		Debug.LogFormat("Set Player Weapon to {0}. NOT IMPLEMENTED AT THIS TIME.", abilityType);
	}
}
