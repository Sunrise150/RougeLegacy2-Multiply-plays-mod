using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Trait Library")]
public class TraitLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/TraitLibrary";

	[SerializeField]
	private BaseTrait[] m_traitLibrary;

	private Dictionary<TraitType, BaseTrait> m_traitDict;

	private static TraitLibrary m_instance;

	public static BaseTrait[] TraitArray
	{
		get
		{
			return Instance.m_traitLibrary;
		}
		set
		{
			Instance.m_traitLibrary = value;
		}
	}

	public static TraitLibrary Instance
	{
		get
		{
			if (!m_instance)
			{
				m_instance = CDGResources.Load<TraitLibrary>("Scriptable Objects/Libraries/TraitLibrary");
				m_instance.Initialize();
			}
			return m_instance;
		}
	}

	private void Initialize()
	{
		m_traitDict = new Dictionary<TraitType, BaseTrait>(m_traitLibrary.Length);
		BaseTrait[] traitLibrary = m_traitLibrary;
		foreach (BaseTrait baseTrait in traitLibrary)
		{
			if (m_traitDict.ContainsKey(baseTrait.TraitType))
			{
				throw new Exception("Duplicate trait: " + baseTrait.TraitType.ToString() + " found in Trait Library.");
			}
			m_traitDict.Add(baseTrait.TraitType, baseTrait);
		}
	}

	public static BaseTrait GetTrait(TraitType traitType)
	{
		if (traitType == TraitType.None)
		{
			return null;
		}
		BaseTrait value = null;
		if (Instance.m_traitDict != null)
		{
			Instance.m_traitDict.TryGetValue(traitType, out value);
			return value;
		}
		throw new Exception("Trait Library is null.");
	}

	public static TraitData GetTraitData(TraitType traitType)
	{
		if (traitType == TraitType.None)
		{
			return null;
		}
		BaseTrait trait = GetTrait(traitType);
		if (trait != null)
		{
			return trait.TraitData;
		}
		return null;
	}
}
