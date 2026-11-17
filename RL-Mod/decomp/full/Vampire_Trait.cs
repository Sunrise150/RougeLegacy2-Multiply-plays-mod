using System.Collections;

public class Vampire_Trait : BaseTrait
{
	private float m_healthDropTick = 1f;

	private WaitRL_Yield m_waitYield;

	public override TraitType TraitType => TraitType.Vampire;

	private IEnumerator Start()
	{
		m_waitYield = new WaitRL_Yield(0f);
		while (!PlayerManager.IsInstantiated)
		{
			yield return null;
		}
		PlayerManager.GetPlayerController().LookController.InitializeLook(SaveManager.PlayerSaveData.CurrentCharacter);
	}

	private IEnumerator VampirismCoroutine(PlayerController playerController)
	{
		while (true)
		{
			m_waitYield.CreateNew(m_healthDropTick);
			yield return m_waitYield;
			if (playerController.CurrentHealth > 1f)
			{
				playerController.SetHealth(-1f, additive: true, runEvents: true);
				continue;
			}
			break;
		}
	}
}
