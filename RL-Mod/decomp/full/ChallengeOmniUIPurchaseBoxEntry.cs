public class ChallengeOmniUIPurchaseBoxEntry : BaseOmniUIPurchaseBoxEntry<ChallengeOmniUIDescriptionEventArgs>
{
	protected override void DisplayNullPurchaseBox()
	{
		base.DisplayNullPurchaseBox();
		_ = m_descriptionType;
		_ = 1;
	}

	protected override void DisplayPurchaseBox(ChallengeOmniUIDescriptionEventArgs args)
	{
		if (ChallengeManager.GetChallenge(args.ChallengeType) != null)
		{
			_ = m_descriptionType;
			_ = 1;
		}
	}
}
