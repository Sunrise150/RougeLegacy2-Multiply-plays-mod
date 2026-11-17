using UnityEngine;

public class ChallengeData : ScriptableObject
{
	public ChallengeType ChallengeType;

	public ChallengeScoringType ScoringType;

	public ClassType ClassOverride;

	public float ParTime;

	public int GoldReq;

	public int SilverReq;

	public int BronzeReq;

	public int Reward;

	public int BaseHandicap;

	public float ScalingHandicap;

	public int MaxHandicap;

	public bool Disabled;

	[Space]
	[Header("Text Fields")]
	[Space]
	public string Title;

	public string Description;

	public string Controls;

	public string BossRequirementHint;

	public string Hint;
}
