using System;
using System.Collections.Generic;

[Serializable]
public class ChallengeObj
{
	public ChallengeType ChallengeType;

	public int TotalHighScore;

	public Dictionary<ClassType, int> ClassHighScores = new Dictionary<ClassType, int>();

	public float BestTime = float.MaxValue;

	public float BestTimeWithoutHandicaps = float.MaxValue;

	public int FoundLevel = -3;

	public int EquippedLevel;

	public int UpgradeBlueprintsFound;

	public FoundState FoundState
	{
		get
		{
			if (ChallengeData.Disabled)
			{
				return FoundState.NotFound;
			}
			if (FoundLevel < -3)
			{
				return FoundState.NotFound;
			}
			return (FoundState)FoundLevel;
		}
	}

	public int MaxLevel
	{
		get
		{
			if (!ChallengeData)
			{
				return 0;
			}
			return ChallengeData.MaxHandicap;
		}
	}

	public int MaxEquippableLevel => UpgradeBlueprintsFound;

	public bool HasMaxBlueprints => UpgradeBlueprintsFound >= MaxLevel;

	public ChallengeData ChallengeData => ChallengeLibrary.GetChallengeData(ChallengeType);

	public ChallengeScoringType ScoringType => ChallengeData.ScoringType;

	public ChallengeObj(ChallengeType challengeType)
	{
		ChallengeType = challengeType;
	}
}
