using System.Collections;

public class BounceTerrain_Trait : BaseTrait
{
	public override TraitType TraitType => TraitType.BounceTerrain;

	private IEnumerator Start()
	{
		while (!PlayerManager.IsInstantiated)
		{
			yield return null;
		}
		PlayerManager.GetPlayerController().LookController.InitializeLook(SaveManager.PlayerSaveData.CurrentCharacter);
	}
}
