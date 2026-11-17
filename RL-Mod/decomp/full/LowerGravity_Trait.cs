using System.Collections;

public class LowerGravity_Trait : BaseTrait
{
	public override TraitType TraitType => TraitType.LowerGravity;

	public bool LowerGravityApplied { get; set; }

	private IEnumerator Start()
	{
		while (!PlayerManager.IsInstantiated)
		{
			yield return null;
		}
		if (!LowerGravityApplied)
		{
			PlayerManager.GetPlayerController().AscentMultiplierOverride *= 1f;
			PlayerManager.GetPlayerController().FallMultiplierOverride *= 0.35f;
			LowerGravityApplied = true;
		}
	}

	private void OnDestroy()
	{
		if (!PlayerManager.IsDisposed && LowerGravityApplied)
		{
			PlayerManager.GetPlayerController().AscentMultiplierOverride /= 1f;
			PlayerManager.GetPlayerController().FallMultiplierOverride /= 0.35f;
			LowerGravityApplied = false;
		}
	}
}
