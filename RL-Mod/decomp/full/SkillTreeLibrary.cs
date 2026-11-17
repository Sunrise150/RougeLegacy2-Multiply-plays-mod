using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Skill Tree Library")]
public class SkillTreeLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/SkillTreeLibrary";

	[Space(10f)]
	[SerializeField]
	private SkillTreeTypeSkillTreeDataDictionary m_skillTreeLibrary;

	private static SkillTreeLibrary m_instance;

	private static SkillTreeLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<SkillTreeLibrary>("Scriptable Objects/Libraries/SkillTreeLibrary");
			}
			return m_instance;
		}
	}

	public static SkillTreeData GetSkillTreeData(SkillTreeType skillType)
	{
		SkillTreeData value = null;
		if (Instance.m_skillTreeLibrary != null)
		{
			Instance.m_skillTreeLibrary.TryGetValue(skillType, out value);
			return value;
		}
		throw new Exception("SkillTree Library is null.");
	}
}
