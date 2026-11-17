public class CandyDrop : HealthDrop
{
	public override ItemDropType ItemDropType => ItemDropType.CandyDrop;

	protected override void GainHealth(float hpGain = 0f)
	{
		if ((bool)PlayerManager.GetPlayerController())
		{
			if (TraitManager.IsTraitActive(TraitType.NoMeat))
			{
				hpGain = 1f;
			}
			base.GainHealth(hpGain);
		}
	}
}
