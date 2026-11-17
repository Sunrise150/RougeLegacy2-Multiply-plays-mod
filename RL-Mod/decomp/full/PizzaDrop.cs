public class PizzaDrop : HealthDrop
{
	public override ItemDropType ItemDropType => ItemDropType.PizzaDrop;

	protected override void GainHealth(float hpGain = 0f)
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		if ((bool)playerController)
		{
			float num = (float)SaveManager.PlayerSaveData.GetRelic(RelicType.FoodHealsMore).Level * 0.08f;
			hpGain = (float)playerController.ActualMaxHealth * (0f + num + SkillTreeLogicHelper.GetPotionMods() + 0.2f);
			float num2 = playerController.ActualMagic * 2f;
			hpGain += num2;
			base.GainHealth(hpGain);
		}
	}
}
