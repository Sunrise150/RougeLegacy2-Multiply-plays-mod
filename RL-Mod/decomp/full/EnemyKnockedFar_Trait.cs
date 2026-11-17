using System.Collections;
using UnityEngine;

public class EnemyKnockedFar_Trait : BaseTrait
{
	public override TraitType TraitType => TraitType.EnemyKnockedFar;

	private IEnumerator Start()
	{
		if (!PlayerManager.IsInstantiated)
		{
			yield return new WaitUntil(() => PlayerManager.IsInstantiated);
		}
		PlayerManager.GetPlayerController().Animator.SetFloat("LimbType", 2f);
	}

	private void OnDestroy()
	{
		if (!PlayerManager.IsDisposed)
		{
			PlayerManager.GetPlayerController().Animator.SetFloat("LimbType", 0f);
		}
	}
}
