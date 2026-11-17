public class SuperFart_Trait : BaseTrait
{
	private WaitRL_Yield m_waitYield;

	private bool m_isFarting;

	public override TraitType TraitType => TraitType.SuperFart;

	private void Start()
	{
		if (SaveManager.PlayerSaveData.GetRelic(RelicType.TalentSwap).Level <= 0)
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			SaveManager.PlayerSaveData.CurrentCharacter.Talent = AbilityType.SuperFart;
			playerController.CharacterClass.SetAbility(CastAbilityType.Talent, AbilityType.SuperFart);
		}
	}
}
