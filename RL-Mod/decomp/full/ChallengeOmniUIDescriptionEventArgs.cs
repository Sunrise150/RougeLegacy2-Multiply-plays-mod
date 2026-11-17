using System;

public class ChallengeOmniUIDescriptionEventArgs : EventArgs
{
	public ChallengeType ChallengeType { get; private set; }

	public OmniUIButtonType ButtonType { get; private set; }

	public ChallengeOmniUIDescriptionEventArgs(ChallengeType challengeType, OmniUIButtonType buttonType)
	{
		Initialize(challengeType, buttonType);
	}

	public void Initialize(ChallengeType challengeType, OmniUIButtonType buttonType)
	{
		ChallengeType = challengeType;
		ButtonType = buttonType;
	}
}
