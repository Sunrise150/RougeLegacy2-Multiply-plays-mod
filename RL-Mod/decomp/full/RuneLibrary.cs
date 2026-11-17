using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Rune Library")]
public class RuneLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/RuneLibrary";

	[Space(10f)]
	[SerializeField]
	private RuneTypeRuneDataDictionary m_runeLibrary;

	private static RuneLibrary m_instance;

	private static RuneLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<RuneLibrary>("Scriptable Objects/Libraries/RuneLibrary");
			}
			return m_instance;
		}
	}

	public static RuneData GetRuneData(RuneType runeType)
	{
		RuneData value = null;
		if (Instance.m_runeLibrary != null)
		{
			Instance.m_runeLibrary.TryGetValue(runeType, out value);
			return value;
		}
		throw new Exception("Rune Library is null.");
	}
}
