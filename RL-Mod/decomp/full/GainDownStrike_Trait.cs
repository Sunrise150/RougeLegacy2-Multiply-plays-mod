using System.Collections;
using UnityEngine;

public class GainDownStrike_Trait : BaseTrait
{
	public override TraitType TraitType => TraitType.GainDownStrike;

	private IEnumerator Start()
	{
		if (!PlayerManager.IsInstantiated)
		{
			yield return new WaitUntil(() => PlayerManager.IsInstantiated);
		}
		PlayerManager.GetPlayerController().CharacterDownStrike.SpinKickInstead = false;
	}

	private void OnDestroy()
	{
		if (!PlayerManager.IsDisposed)
		{
			PlayerManager.GetPlayerController().CharacterDownStrike.SpinKickInstead = true;
		}
	}
}
