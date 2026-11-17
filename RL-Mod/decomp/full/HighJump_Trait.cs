using System.Collections;

public class HighJump_Trait : BaseTrait
{
	private float m_jumpHeightMultiplier = 1.5f;

	public override TraitType TraitType => TraitType.HighJump;

	private IEnumerator Start()
	{
		while (!PlayerManager.IsInstantiated)
		{
			yield return null;
		}
		PlayerManager.GetPlayerController().CharacterJump.JumpHeightMultiplier = m_jumpHeightMultiplier;
	}

	private void OnDestroy()
	{
		if (!PlayerManager.IsDisposed)
		{
			PlayerManager.GetPlayerController().CharacterJump.JumpHeightMultiplier = 1f;
		}
	}
}
