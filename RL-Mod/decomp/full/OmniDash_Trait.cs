using System.Collections;

public class OmniDash_Trait : BaseTrait
{
	public override TraitType TraitType => TraitType.OmniDash;

	private IEnumerator Start()
	{
		while (!PlayerManager.IsInstantiated)
		{
			yield return null;
		}
		PlayerManager.GetPlayerController().CharacterDash.EnableOmnidash = true;
	}

	private void OnDestroy()
	{
		if (!PlayerManager.IsDisposed)
		{
			PlayerManager.GetPlayerController().CharacterDash.EnableOmnidash = false;
		}
	}
}
