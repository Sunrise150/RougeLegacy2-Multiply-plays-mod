using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Challenge Library")]
public class ChallengeLibrary : ScriptableObject
{
	[Serializable]
	public class ChallengeIconEntry
	{
		public Sprite ChallengeIcon;

		public Sprite BronzeTrophyIcon;

		public Sprite SilverTrophyIcon;

		public Sprite GoldTrophyIcon;
	}

	public enum ChallengeIconEntryType
	{
		Challenge,
		Bronze,
		Silver,
		Gold
	}

	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/ChallengeLibrary";

	[Space(10f)]
	[SerializeField]
	private ChallengeTypeChallengeDataDictionary m_challengeLibrary;

	private static ChallengeLibrary m_instance;

	private static ChallengeLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<ChallengeLibrary>("Scriptable Objects/Libraries/ChallengeLibrary");
			}
			return m_instance;
		}
	}

	public static ChallengeData GetChallengeData(ChallengeType challengeType)
	{
		ChallengeData value = null;
		if (Instance.m_challengeLibrary != null)
		{
			Instance.m_challengeLibrary.TryGetValue(challengeType, out value);
			return value;
		}
		throw new Exception("Challenge Library is null.");
	}
}
