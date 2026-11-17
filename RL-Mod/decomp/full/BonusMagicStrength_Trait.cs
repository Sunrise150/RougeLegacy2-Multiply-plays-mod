using System.Collections;
using UnityEngine;

public class BonusMagicStrength_Trait : BaseTrait
{
	public override TraitType TraitType => TraitType.BonusMagicStrength;

	public IEnumerator Start()
	{
		if (!PlayerManager.IsInstantiated)
		{
			yield return new WaitUntil(() => PlayerManager.IsInstantiated);
		}
		PlayerManager.GetPlayerController().InitializeMagicMods();
	}

	private void OnDestroy()
	{
		if (!PlayerManager.IsDisposed)
		{
			PlayerManager.GetPlayerController().InitializeMagicMods();
		}
	}
}
