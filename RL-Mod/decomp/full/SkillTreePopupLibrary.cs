using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Skill Tree Popup Library")]
public class SkillTreePopupLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/SkillTreePopupLibrary";

	[Space(10f)]
	[SerializeField]
	private SkillTreeTypeSkillTreePopupDataDictionary m_popupLibrary;

	private static SkillTreePopupLibrary m_instance;

	private static SkillTreePopupLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<SkillTreePopupLibrary>("Scriptable Objects/Libraries/SkillTreePopupLibrary");
			}
			return m_instance;
		}
	}

	public static SkillTreePopupData GetPopupData(SkillTreeType skillType)
	{
		SkillTreePopupData value = null;
		if (Instance.m_popupLibrary != null)
		{
			Instance.m_popupLibrary.TryGetValue(skillType, out value);
			return value;
		}
		throw new Exception("SkillTree Popup Library is null.");
	}
}
