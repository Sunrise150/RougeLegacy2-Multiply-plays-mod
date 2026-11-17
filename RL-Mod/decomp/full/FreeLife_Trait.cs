public class FreeLife_Trait : BaseTrait
{
	public override TraitType TraitType => TraitType.FreeLife;

	private void Start()
	{
		GiveFreeLife();
	}

	private void GiveFreeLife()
	{
		bool flag = false;
		if (SaveManager.PlayerSaveData.GetRelic(RelicType.ExtraLife_Unity).IsFreeRelic || SaveManager.PlayerSaveData.GetRelic(RelicType.ExtraLife_UnityUsed).IsFreeRelic)
		{
			flag = true;
		}
		if (!flag)
		{
			RelicType relicType = RelicType.ExtraLife_Unity;
			RelicObj relic = SaveManager.PlayerSaveData.GetRelic(relicType);
			relic.SetLevel(1, additive: true);
			relic.IsFreeRelic = true;
		}
	}
}
