using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Ability Library")]
public class AbilityLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/AbilityLibrary";

	[SerializeField]
	private BaseAbility_RL[] m_abilityList;

	private static AbilityLibrary m_instance;

	private Dictionary<AbilityType, BaseAbility_RL> m_abilityLibrary;

	private static AbilityLibrary Instance
	{
		get
		{
			if (!m_instance)
			{
				m_instance = CDGResources.Load<AbilityLibrary>("Scriptable Objects/Libraries/AbilityLibrary");
				m_instance.Initialize();
			}
			return m_instance;
		}
	}

	public void Initialize()
	{
		if (m_abilityLibrary == null)
		{
			m_abilityLibrary = new Dictionary<AbilityType, BaseAbility_RL>();
		}
		else
		{
			m_abilityLibrary.Clear();
		}
		BaseAbility_RL[] abilityList = m_abilityList;
		foreach (BaseAbility_RL baseAbility_RL in abilityList)
		{
			if (!(baseAbility_RL == null))
			{
				AbilityType abilityType = baseAbility_RL.AbilityType;
				if (m_abilityLibrary.ContainsKey(abilityType))
				{
					throw new Exception("Entry in Ability Library for " + abilityType.ToString() + " already found. Ability Library cannot have dupilcates.");
				}
				m_abilityLibrary.Add(abilityType, baseAbility_RL);
			}
		}
	}

	public static BaseAbility_RL GetAbility(AbilityType abilityType)
	{
		BaseAbility_RL value = null;
		if (Instance.m_abilityLibrary != null)
		{
			Instance.m_abilityLibrary.TryGetValue(abilityType, out value);
		}
		if (!value)
		{
			Debug.LogWarningFormat("<color=red>{0}: ({1}) Could not find Ability ({2}) in Ability Library.</color>", Time.frameCount, Instance, abilityType);
		}
		return value;
	}
}
