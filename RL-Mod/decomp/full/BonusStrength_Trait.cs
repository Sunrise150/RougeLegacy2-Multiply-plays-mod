using System.Collections;
using UnityEngine;

public class BonusStrength_Trait : BaseTrait
{
	public override TraitType TraitType => TraitType.BonusStrength;

	public IEnumerator Start()
	{
		if (!PlayerManager.IsInstantiated)
		{
			yield return new WaitUntil(() => PlayerManager.IsInstantiated);
		}
		PlayerManager.GetPlayerController().InitializeStrengthMods();
	}

	private void OnDestroy()
	{
		if (!PlayerManager.IsDisposed)
		{
			PlayerManager.GetPlayerController().InitializeStrengthMods();
		}
	}
}
