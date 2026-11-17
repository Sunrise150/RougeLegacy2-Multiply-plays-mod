public struct ChallengeDrop : IChallengeDrop, ISpecialItemDrop
{
	public SpecialItemType SpecialItemType => SpecialItemType.Challenge;

	public ChallengeType ChallengeType { get; private set; }

	public ChallengeDrop(ChallengeType challengeType)
	{
		ChallengeType = challengeType;
	}
}
