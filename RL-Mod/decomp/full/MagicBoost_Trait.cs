using System.Collections;
using UnityEngine;

public class MagicBoost_Trait : BaseTrait
{
	public override TraitType TraitType => TraitType.MagicBoost;

	public IEnumerator Start()
	{
		if (!PlayerManager.IsInstantiated)
		{
			yield return new WaitUntil(() => PlayerManager.IsInstantiated);
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.InitializeStrengthMods();
		playerController.InitializeManaMods();
		playerController.InitializeMagicMods();
		playerController.ResetMana();
	}

	private void OnDestroy()
	{
		if (!PlayerManager.IsDisposed)
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			playerController.InitializeStrengthMods();
			playerController.InitializeManaMods();
			playerController.InitializeMagicMods();
			playerController.ResetMana();
		}
	}
}
