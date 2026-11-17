public class ScarObjectiveCompleteHUDEventArgs : ObjectiveCompleteHUDEventArgs
{
	public ChallengeType ChallengeType { get; private set; }

	public ScarObjectiveCompleteHUDEventArgs(ChallengeType challengeType, float displayDuration = 5f, string titleTextOverride = null, string subtitleTextOverride = null, string descriptionTextOverride = null)
		: base(ObjectiveCompleteHUDType.Scar, displayDuration)
	{
		Initialize(challengeType, displayDuration, titleTextOverride, subtitleTextOverride, descriptionTextOverride);
	}

	public void Initialize(ChallengeType challengeType, float displayDuration = 5f, string titleTextOverride = null, string subtitleTextOverride = null, string descriptionTextOverride = null)
	{
		Initialize(ObjectiveCompleteHUDType.Scar, displayDuration, titleTextOverride, subtitleTextOverride, descriptionTextOverride);
		ChallengeType = challengeType;
	}
}
