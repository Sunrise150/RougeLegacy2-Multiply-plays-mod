public class Antique_Trait : BaseTrait
{
	public static RelicType[] RelicExceptionArray = new RelicType[10]
	{
		RelicType.None,
		RelicType.GoldDeathCurse,
		RelicType.BonusDamageCurse,
		RelicType.AttackCooldown,
		RelicType.ResolveCombatChallenge,
		RelicType.NoGoldXPBonus,
		RelicType.GoldCombatChallenge,
		RelicType.FoodChallenge,
		RelicType.FlightBonusCurse,
		RelicType.RangeDamageBonusCurse
	};

	public override TraitType TraitType => TraitType.Antique;

	private void Start()
	{
		GiveAntique();
	}

	private void GiveAntique()
	{
		RelicType relicType = (base.IsFirstTrait ? SaveManager.PlayerSaveData.CurrentCharacter.AntiqueOneOwned : SaveManager.PlayerSaveData.CurrentCharacter.AntiqueTwoOwned);
		if (relicType == RelicType.None)
		{
			relicType = RelicLibrary.GetRandomRelic(RngID.None, limitMaxStacks: true, RelicExceptionArray);
			if (base.IsFirstTrait)
			{
				SaveManager.PlayerSaveData.CurrentCharacter.AntiqueOneOwned = relicType;
			}
			else
			{
				SaveManager.PlayerSaveData.CurrentCharacter.AntiqueTwoOwned = relicType;
			}
		}
		RelicObj relic = SaveManager.PlayerSaveData.GetRelic(relicType);
		if (relic != null && !relic.IsFreeRelic)
		{
			relic.SetLevel(1, additive: false);
			relic.IsFreeRelic = true;
		}
	}
}
