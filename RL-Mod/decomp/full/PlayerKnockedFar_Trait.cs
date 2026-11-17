using System.Collections;
using UnityEngine;

public class PlayerKnockedFar_Trait : BaseTrait
{
	public override TraitType TraitType => TraitType.PlayerKnockedFar;

	private IEnumerator Start()
	{
		if (!PlayerManager.IsInstantiated)
		{
			yield return new WaitUntil(() => PlayerManager.IsInstantiated);
		}
		PlayerManager.GetPlayerController().Animator.SetFloat("BoneStructureType", 1f);
	}

	private void OnDestroy()
	{
		if (!PlayerManager.IsDisposed)
		{
			PlayerManager.GetPlayerController().Animator.SetFloat("BoneStructureType", 0f);
		}
	}
}
