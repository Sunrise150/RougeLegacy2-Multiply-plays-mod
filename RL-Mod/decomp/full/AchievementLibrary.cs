using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Achievement Library")]
public class AchievementLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/AchievementLibrary";

	[SerializeField]
	private AchievementTypeAchievementDataDictionary m_achievementLibrary;

	private static AchievementLibrary m_instance;

	private static AchievementLibrary Instance
	{
		get
		{
			if (!m_instance)
			{
				m_instance = CDGResources.Load<AchievementLibrary>("Scriptable Objects/Libraries/AchievementLibrary");
			}
			return m_instance;
		}
	}

	public static AchievementData GetAchievementData(AchievementType achievementType)
	{
		AchievementData value = null;
		if (Instance.m_achievementLibrary != null)
		{
			Instance.m_achievementLibrary.TryGetValue(achievementType, out value);
			return value;
		}
		throw new Exception("Achievement Library is null.");
	}
}
