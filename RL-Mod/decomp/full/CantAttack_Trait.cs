public class CantAttack_Trait : BaseTrait
{
	public override TraitType TraitType => TraitType.CantAttack;

	private void Start()
	{
		if (SaveManager.PlayerSaveData.GetRelic(RelicType.WeaponSwap).Level <= 0)
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			SaveManager.PlayerSaveData.CurrentCharacter.Weapon = AbilityType.PacifistWeapon;
			playerController.CharacterClass.SetAbility(CastAbilityType.Weapon, AbilityType.PacifistWeapon);
		}
	}
}
