using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Enemy Class Library")]
public class EnemyClassLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/EnemyClassLibrary";

	[SerializeField]
	private EnemyTypeEnemyClassDataDictionary m_enemyClassLibrary;

	private static EnemyClassLibrary m_instance;

	private static EnemyClassLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<EnemyClassLibrary>("Scriptable Objects/Libraries/EnemyClassLibrary");
			}
			return m_instance;
		}
	}

	public static EnemyClassData GetEnemyClassData(EnemyType enemyType)
	{
		EnemyClassData value = null;
		if (Instance.m_enemyClassLibrary != null)
		{
			Instance.m_enemyClassLibrary.TryGetValue(enemyType, out value);
			if (value == null)
			{
				Debug.LogFormat("<color=red>Class: " + enemyType.ToString() + " not found in Enemy Class Library. Please ensure the class exists in the Enemy Class Library scriptable object.</color>");
			}
			return value;
		}
		throw new Exception("Enemy Class Library is null.");
	}

	public static EnemyData GetEnemyData(EnemyType enemyType, EnemyRank enemyRank)
	{
		EnemyClassData value = null;
		if (Instance.m_enemyClassLibrary != null)
		{
			Instance.m_enemyClassLibrary.TryGetValue(enemyType, out value);
			if (value == null)
			{
				Debug.LogFormat("<color=red>Enemy Data: " + enemyType.ToString() + " not found in Enemy Class Library. Please ensure the class exists in the Enemy Class Library scriptable object.</color>");
				return null;
			}
			return value.GetEnemyData(enemyRank);
		}
		throw new Exception("Enemy Class Library is null.");
	}
}
