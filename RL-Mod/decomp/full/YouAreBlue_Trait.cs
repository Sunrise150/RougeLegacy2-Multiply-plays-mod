using System.Collections;

public class YouAreBlue_Trait : BaseTrait
{
	public override TraitType TraitType => TraitType.YouAreBlue;

	private IEnumerator Start()
	{
		while (!PlayerManager.IsInstantiated)
		{
			yield return null;
		}
		PlayerManager.GetPlayerController().LookController.InitializeLook(SaveManager.PlayerSaveData.CurrentCharacter);
	}
}
