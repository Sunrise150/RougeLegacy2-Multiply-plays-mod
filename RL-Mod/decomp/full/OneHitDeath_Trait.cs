using System.Collections;
using UnityEngine;

public class OneHitDeath_Trait : BaseTrait
{
	public override TraitType TraitType => TraitType.OneHitDeath;

	private IEnumerator Start()
	{
		if (!PlayerManager.IsInstantiated)
		{
			yield return new WaitUntil(() => PlayerManager.IsInstantiated);
		}
		PlayerManager.GetPlayerController().ResetHealth();
	}

	private void OnDestroy()
	{
		if (!PlayerManager.IsDisposed)
		{
			PlayerManager.GetPlayerController().ResetHealth();
		}
	}
}
